using DartsApi.Data;
using DartsApi.DTOs;
using DartsApi.Models;
using DartsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController(DartsDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> GetPlayers([FromQuery] bool? localOnly = null)
    {
        var query = db.Players.AsQueryable();
        if (localOnly == true) query = query.Where(p => p.IsLocal);

        var players = await query.OrderBy(p => p.Ranking ?? 999).ThenBy(p => p.Name).ToListAsync();
        return Ok(players.Select(p => p.ToDto()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(Guid id)
    {
        var player = await db.Players.FindAsync(id);
        if (player is null) return NotFound();
        return Ok(player.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        var player = new Player
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CountryCode = request.CountryCode,
            Ranking = request.Ranking,
            IsLocal = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Players.Add(player);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlayerDto>> UpdatePlayer(Guid id, [FromBody] UpdatePlayerRequest request)
    {
        var player = await db.Players.FindAsync(id);
        if (player is null) return NotFound();

        player.Name = request.Name;
        player.CountryCode = request.CountryCode;
        player.Ranking = request.Ranking;
        await db.SaveChangesAsync();
        return Ok(player.ToDto());
    }
}
