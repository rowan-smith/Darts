using DartsApi.Data;
using DartsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Services;

public class ScoringService(DartsDbContext db)
{
    private static readonly HashSet<int> ValidScores = BuildValidScores();

    private static HashSet<int> BuildValidScores()
    {
        var scores = new HashSet<int> { 0 };
        for (var i = 1; i <= 20; i++)
        {
            scores.Add(i);
            scores.Add(i * 2);
            scores.Add(i * 3);
        }
        scores.Add(25);
        scores.Add(50);
        return scores;
    }

    public static bool IsValidDartScore(int score) => score >= 0 && score <= 60 && ValidScores.Contains(score);

    public static bool IsValidVisitScore(int score) => score >= 0 && score <= 180;

    public static bool CanCheckout(int remaining, int score)
    {
        if (score > remaining) return false;
        if (score == remaining) return remaining <= 170 && remaining >= 2 && CanFinishWithDouble(remaining);
        return true;
    }

    private static bool CanFinishWithDouble(int remaining)
    {
        if (remaining < 2 || remaining > 170) return false;
        if (remaining == 50) return true;
        for (var d = 1; d <= 20; d++)
        {
            var afterDouble = remaining - d * 2;
            if (afterDouble == 0) return true;
            if (afterDouble > 1 && afterDouble <= 180 && ValidScores.Contains(afterDouble)) return true;
        }
        if (remaining == 25) return true;
        return false;
    }

    public async Task<Visit?> RecordVisitAsync(Guid matchId, Guid playerId, int score)
    {
        if (!IsValidVisitScore(score)) return null;

        var match = await db.Matches
            .Include(m => m.Sets).ThenInclude(s => s.Legs).ThenInclude(l => l.Visits)
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match is null || match.Status != MatchStatus.InProgress) return null;
        if (playerId != match.Player1Id && playerId != match.Player2Id) return null;

        var currentLeg = await GetOrCreateCurrentLegAsync(match);
        if (currentLeg.IsComplete) return null;
        if (currentLeg.CurrentPlayerId != playerId) return null;

        var isPlayer1 = playerId == match.Player1Id;
        var remaining = isPlayer1 ? currentLeg.Player1Remaining : currentLeg.Player2Remaining;
        var visitNumber = currentLeg.Visits.Count(v => v.PlayerId == playerId) + 1;

        var isBust = false;
        var isCheckout = false;
        var remainingAfter = remaining;

        if (score > remaining)
        {
            isBust = true;
        }
        else if (score == remaining)
        {
            if (!CanCheckout(remaining, score))
            {
                isBust = true;
            }
            else
            {
                isCheckout = true;
                remainingAfter = 0;
            }
        }
        else
        {
            remainingAfter = remaining - score;
            if (remainingAfter == 1) isBust = true;
        }

        var visit = new Visit
        {
            Id = Guid.NewGuid(),
            LegId = currentLeg.Id,
            PlayerId = playerId,
            Score = score,
            RemainingAfter = isBust ? remaining : remainingAfter,
            IsBust = isBust,
            IsCheckout = isCheckout,
            Is180 = score == 180,
            VisitNumber = visitNumber,
            CreatedAt = DateTime.UtcNow
        };

        db.Visits.Add(visit);

        if (!isBust)
        {
            if (isPlayer1)
            {
                currentLeg.Player1Remaining = remainingAfter;
                currentLeg.Player1DartsThrown += 3;
            }
            else
            {
                currentLeg.Player2Remaining = remainingAfter;
                currentLeg.Player2DartsThrown += 3;
            }
        }
        else if (isPlayer1)
        {
            currentLeg.Player1DartsThrown += 3;
        }
        else
        {
            currentLeg.Player2DartsThrown += 3;
        }

        if (isCheckout)
        {
            await CompleteLegAsync(match, currentLeg, playerId);
        }
        else
        {
            currentLeg.CurrentPlayerId = isPlayer1 ? match.Player2Id : match.Player1Id;
        }

        match.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return visit;
    }

    private async Task<Leg> GetOrCreateCurrentLegAsync(Match match)
    {
        if (match.CurrentLegId.HasValue)
        {
            var existing = match.Sets.SelectMany(s => s.Legs).FirstOrDefault(l => l.Id == match.CurrentLegId);
            if (existing is not null) return existing;
        }

        var currentSet = match.Sets.OrderByDescending(s => s.SetNumber).FirstOrDefault();
        if (currentSet is null || currentSet.IsComplete)
        {
            currentSet = new Set
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                SetNumber = (match.Sets.MaxBy(s => s.SetNumber)?.SetNumber ?? 0) + 1
            };
            db.Sets.Add(currentSet);
            match.Sets.Add(currentSet);
        }

        var legNumber = (currentSet.Legs.MaxBy(l => l.LegNumber)?.LegNumber ?? 0) + 1;
        var startingPlayerId = match.StartingPlayerId ?? match.Player1Id;
        if (legNumber % 2 == 0)
        {
            startingPlayerId = startingPlayerId == match.Player1Id ? match.Player2Id : match.Player1Id;
        }

        var leg = new Leg
        {
            Id = Guid.NewGuid(),
            SetId = currentSet.Id,
            LegNumber = legNumber,
            Player1Remaining = match.StartingScore,
            Player2Remaining = match.StartingScore,
            CurrentPlayerId = startingPlayerId
        };

        db.Legs.Add(leg);
        currentSet.Legs.Add(leg);
        match.CurrentLegId = leg.Id;
        await db.SaveChangesAsync();
        return leg;
    }

    private async Task CompleteLegAsync(Match match, Leg leg, Guid winnerId)
    {
        leg.WinnerId = winnerId;
        leg.IsComplete = true;
        leg.CurrentPlayerId = null;

        var currentSet = match.Sets.First(s => s.Id == leg.SetId);
        if (winnerId == match.Player1Id) currentSet.Player1Legs++;
        else currentSet.Player2Legs++;

        if (currentSet.Player1Legs >= match.LegsPerSet)
        {
            await CompleteSetAsync(match, currentSet, match.Player1Id);
        }
        else if (currentSet.Player2Legs >= match.LegsPerSet)
        {
            await CompleteSetAsync(match, currentSet, match.Player2Id);
        }
        else
        {
            match.CurrentLegId = null;
        }

        await db.SaveChangesAsync();
    }

    private async Task CompleteSetAsync(Match match, Set set, Guid winnerId)
    {
        set.WinnerId = winnerId;
        set.IsComplete = true;

        if (winnerId == match.Player1Id) match.Player1Sets++;
        else match.Player2Sets++;

        if (match.Player1Sets >= match.SetsToWin)
        {
            await CompleteMatchAsync(match, match.Player1Id);
        }
        else if (match.Player2Sets >= match.SetsToWin)
        {
            await CompleteMatchAsync(match, match.Player2Id);
        }
        else
        {
            match.CurrentLegId = null;
        }

        await db.SaveChangesAsync();
    }

    private async Task CompleteMatchAsync(Match match, Guid winnerId)
    {
        match.WinnerId = winnerId;
        match.Status = MatchStatus.Completed;
        match.CompletedAt = DateTime.UtcNow;
        match.CurrentLegId = null;
        match.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<bool> StartMatchAsync(Guid matchId)
    {
        var match = await db.Matches.FindAsync(matchId);
        if (match is null || match.Status != MatchStatus.Scheduled) return false;

        match.Status = MatchStatus.InProgress;
        match.StartedAt = DateTime.UtcNow;
        match.StartingPlayerId ??= match.Player1Id;
        match.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await GetOrCreateCurrentLegAsync(await db.Matches.Include(m => m.Sets).ThenInclude(s => s.Legs).FirstAsync(m => m.Id == matchId));
        return true;
    }
}
