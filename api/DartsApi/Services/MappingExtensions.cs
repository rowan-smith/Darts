using DartsApi.DTOs;
using DartsApi.Models;

namespace DartsApi.Services;

public static class MappingExtensions
{
    public static PlayerDto ToDto(this Player p) =>
        new(p.Id, p.Name, p.CountryCode, p.Ranking, p.Average, p.Total180s, p.IsLocal);

    public static VisitDto ToDto(this Visit v) =>
        new(v.Id, v.PlayerId, v.Player?.Name ?? "", v.Score, v.RemainingAfter, v.IsBust, v.IsCheckout, v.Is180, v.VisitNumber, v.CreatedAt);

    public static LegDto ToDto(this Leg l) =>
        new(l.Id, l.LegNumber, l.Player1Remaining, l.Player2Remaining, l.CurrentPlayerId, l.WinnerId, l.IsComplete, l.Player1DartsThrown, l.Player2DartsThrown,
            l.Visits.OrderBy(v => v.CreatedAt).Select(v => v.ToDto()).ToList());

    public static SetDto ToDto(this Set s) =>
        new(s.Id, s.SetNumber, s.Player1Legs, s.Player2Legs, s.WinnerId, s.IsComplete,
            s.Legs.OrderBy(l => l.LegNumber).Select(l => l.ToDto()).ToList());

    public static MatchSummaryDto ToSummaryDto(this Match m) =>
        new(m.Id, m.Title, m.Player1?.Name ?? "", m.Player2?.Name ?? "", m.Player1Sets, m.Player2Sets, m.Status,
            m.Tournament, m.Venue, m.StartedAt, m.CompletedAt, m.Winner?.Name);

    public static MatchDetailDto ToDetailDto(this Match m) =>
        new(m.Id, m.Title, m.Player1.ToDto(), m.Player2.ToDto(), m.SetsToWin, m.LegsPerSet, m.StartingScore, m.Status,
            m.Player1Sets, m.Player2Sets, m.WinnerId, m.CurrentLegId, m.StartingPlayerId, m.Venue, m.Tournament,
            m.StartedAt, m.CompletedAt, m.Sets.OrderBy(s => s.SetNumber).Select(s => s.ToDto()).ToList());

    public static ArticleDto ToDto(this Article a) =>
        new(a.Id, a.Title, a.Summary, a.Content, a.ImageUrl, a.Category, a.IsFeatured, a.Author, a.ReadTimeMinutes, a.PublishedAt);

    public static SuggestionDto ToDto(this Suggestion s) =>
        new(s.Id, s.Title, s.Description, s.Type, s.Icon, s.Priority);

    public static FeaturedDto ToDto(this FeaturedHighlight f) =>
        new(f.Id, f.Title, f.Subtitle, f.ImageUrl, f.Badge, f.SortOrder, f.MatchId);

    public static UserProfileDto ToDto(this UserProfile p) =>
        new(p.Id, p.Name, p.Email, p.Theme, p.AvatarUrl, p.Total180s, p.MatchesPlayed, p.MatchesWon, p.AverageScore);
}
