using DartsApi.Data;
using DartsApi.DTOs;
using DartsApi.Models;
using DartsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController(DartsDbContext db, ScoringService scoring) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MatchSummaryDto>>> GetMatches([FromQuery] MatchStatus? status = null)
    {
        var query = db.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        var matches = await query.OrderByDescending(m => m.UpdatedAt).ToListAsync();
        return Ok(matches.Select(m => m.ToSummaryDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchDetailDto>> GetMatch(Guid id)
    {
        var match = await LoadMatchDetail(id);
        if (match is null) return NotFound();
        return Ok(match.ToDetailDto());
    }

    [HttpGet("{id:guid}/recap")]
    public async Task<ActionResult<MatchRecapDto>> GetRecap(Guid id)
    {
        var match = await LoadMatchDetail(id);
        if (match is null) return NotFound();
        if (match.Status != MatchStatus.Completed)
            return BadRequest(new { message = "Recap is only available for completed matches." });

        var allVisits = match.Sets.SelectMany(s => s.Legs).SelectMany(l => l.Visits).ToList();
        var p1Visits = allVisits.Where(v => v.PlayerId == match.Player1Id && !v.IsBust).ToList();
        var p2Visits = allVisits.Where(v => v.PlayerId == match.Player2Id && !v.IsBust).ToList();

        return Ok(new MatchRecapDto(
            match.Id, match.Title, match.Player1.ToDto(), match.Player2.ToDto(),
            match.Player1Sets, match.Player2Sets, match.Winner?.Name,
            match.Tournament, match.Venue, match.StartedAt, match.CompletedAt,
            allVisits.Count(v => v.Is180), allVisits.Count,
            p1Visits.Count > 0 ? p1Visits.Average(v => (double)v.Score) : 0,
            p2Visits.Count > 0 ? p2Visits.Average(v => (double)v.Score) : 0,
            match.Sets.OrderBy(s => s.SetNumber).Select(s => s.ToDto()).ToList()
        ));
    }

    [HttpPost]
    public async Task<ActionResult<MatchDetailDto>> CreateMatch([FromBody] CreateMatchRequest request)
    {
        var p1 = await db.Players.FindAsync(request.Player1Id);
        var p2 = await db.Players.FindAsync(request.Player2Id);
        if (p1 is null || p2 is null) return BadRequest(new { message = "Invalid player IDs." });

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Title = string.IsNullOrWhiteSpace(request.Title) ? $"{p1.Name} vs {p2.Name}" : request.Title,
            Player1Id = request.Player1Id,
            Player2Id = request.Player2Id,
            SetsToWin = request.SetsToWin,
            LegsPerSet = request.LegsPerSet,
            StartingScore = request.StartingScore,
            Venue = request.Venue,
            Tournament = request.Tournament,
            StartingPlayerId = request.StartingPlayerId ?? request.Player1Id,
            Status = MatchStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Matches.Add(match);
        await db.SaveChangesAsync();

        var created = await LoadMatchDetail(match.Id);
        return CreatedAtAction(nameof(GetMatch), new { id = match.Id }, created!.ToDetailDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MatchDetailDto>> UpdateMatch(Guid id, [FromBody] UpdateMatchRequest request)
    {
        var match = await db.Matches.FindAsync(id);
        if (match is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Title)) match.Title = request.Title;
        if (request.Venue is not null) match.Venue = request.Venue;
        if (request.Tournament is not null) match.Tournament = request.Tournament;
        if (request.Status.HasValue) match.Status = request.Status.Value;
        match.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        var updated = await LoadMatchDetail(id);
        return Ok(updated!.ToDetailDto());
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<MatchDetailDto>> StartMatch(Guid id)
    {
        var success = await scoring.StartMatchAsync(id);
        if (!success) return BadRequest(new { message = "Match cannot be started." });

        var match = await LoadMatchDetail(id);
        return Ok(match!.ToDetailDto());
    }

    [HttpPost("{id:guid}/visit")]
    public async Task<ActionResult<MatchDetailDto>> RecordVisit(Guid id, [FromBody] RecordVisitRequest request)
    {
        if (!ScoringService.IsValidVisitScore(request.Score))
            return BadRequest(new { message = "Invalid visit score." });

        var visit = await scoring.RecordVisitAsync(id, request.PlayerId, request.Score);
        if (visit is null)
            return BadRequest(new { message = "Could not record visit. Check match status and turn." });

        var match = await LoadMatchDetail(id);
        return Ok(match!.ToDetailDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMatch(Guid id)
    {
        var match = await db.Matches.FindAsync(id);
        if (match is null) return NotFound();

        db.Matches.Remove(match);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Match?> LoadMatchDetail(Guid id) =>
        await db.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .Include(m => m.Sets).ThenInclude(s => s.Legs).ThenInclude(l => l.Visits).ThenInclude(v => v.Player)
            .FirstOrDefaultAsync(m => m.Id == id);
}
