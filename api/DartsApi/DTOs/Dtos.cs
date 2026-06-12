using DartsApi.Models;

namespace DartsApi.DTOs;

public record PlayerDto(Guid Id, string Name, string? CountryCode, int? Ranking, double Average, int Total180s, bool IsLocal);
public record CreatePlayerRequest(string Name, string? CountryCode = null, int? Ranking = null);
public record UpdatePlayerRequest(string Name, string? CountryCode, int? Ranking);

public record VisitDto(Guid Id, Guid PlayerId, string PlayerName, int Score, int RemainingAfter, bool IsBust, bool IsCheckout, bool Is180, int VisitNumber, DateTime CreatedAt);
public record LegDto(Guid Id, int LegNumber, int Player1Remaining, int Player2Remaining, Guid? CurrentPlayerId, Guid? WinnerId, bool IsComplete, int Player1DartsThrown, int Player2DartsThrown, List<VisitDto> Visits);
public record SetDto(Guid Id, int SetNumber, int Player1Legs, int Player2Legs, Guid? WinnerId, bool IsComplete, List<LegDto> Legs);
public record MatchSummaryDto(Guid Id, string Title, string Player1Name, string Player2Name, int Player1Sets, int Player2Sets, MatchStatus Status, string? Tournament, string? Venue, DateTime? StartedAt, DateTime? CompletedAt, string? WinnerName);
public record MatchDetailDto(Guid Id, string Title, PlayerDto Player1, PlayerDto Player2, int SetsToWin, int LegsPerSet, int StartingScore, MatchStatus Status, int Player1Sets, int Player2Sets, Guid? WinnerId, Guid? CurrentLegId, Guid? StartingPlayerId, string? Venue, string? Tournament, DateTime? StartedAt, DateTime? CompletedAt, List<SetDto> Sets);
public record CreateMatchRequest(string Title, Guid Player1Id, Guid Player2Id, int SetsToWin = 3, int LegsPerSet = 3, int StartingScore = 501, string? Venue = null, string? Tournament = null, Guid? StartingPlayerId = null);
public record UpdateMatchRequest(string? Title, string? Venue, string? Tournament, MatchStatus? Status);
public record RecordVisitRequest(Guid PlayerId, int Score);

public record ArticleDto(Guid Id, string Title, string Summary, string Content, string? ImageUrl, ArticleCategory Category, bool IsFeatured, string? Author, int ReadTimeMinutes, DateTime PublishedAt);
public record SuggestionDto(Guid Id, string Title, string Description, SuggestionType Type, string? Icon, int Priority);
public record FeaturedDto(Guid Id, string Title, string Subtitle, string? ImageUrl, string? Badge, int SortOrder, Guid? MatchId);
public record HomeFeedDto(List<MatchSummaryDto> RecentScores, List<ArticleDto> Articles, List<ArticleDto> FeaturedArticles, List<SuggestionDto> Suggestions, List<FeaturedDto> Featured);

public record UserProfileDto(Guid Id, string Name, string Email, ThemeMode Theme, string? AvatarUrl, int Total180s, int MatchesPlayed, int MatchesWon, double AverageScore);
public record UpdateProfileRequest(string? Name, string? Email, ThemeMode? Theme, string? AvatarUrl);

public record MatchRecapDto(Guid Id, string Title, PlayerDto Player1, PlayerDto Player2, int Player1Sets, int Player2Sets, string? WinnerName, string? Tournament, string? Venue, DateTime? StartedAt, DateTime? CompletedAt, int Total180s, int TotalVisits, double Player1Average, double Player2Average, List<SetDto> Sets);
