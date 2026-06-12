using System.ComponentModel.DataAnnotations;

namespace DartsApi.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = "Player";
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    public ThemeMode Theme { get; set; } = ThemeMode.Dark;
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
    public int Total180s { get; set; }
    public int MatchesPlayed { get; set; }
    public int MatchesWon { get; set; }
    public double AverageScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Player
{
    public Guid Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(10)]
    public string? CountryCode { get; set; }
    public int? Ranking { get; set; }
    public double Average { get; set; }
    public int Total180s { get; set; }
    public bool IsLocal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Match> MatchesAsPlayer1 { get; set; } = [];
    public ICollection<Match> MatchesAsPlayer2 { get; set; } = [];
    public ICollection<Match> MatchesWon { get; set; } = [];
    public ICollection<Leg> LegsWon { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
}

public class Match
{
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public Guid Player1Id { get; set; }
    public Player Player1 { get; set; } = null!;
    public Guid Player2Id { get; set; }
    public Player Player2 { get; set; } = null!;
    public int SetsToWin { get; set; } = 3;
    public int LegsPerSet { get; set; } = 3;
    public int StartingScore { get; set; } = 501;
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public int Player1Sets { get; set; }
    public int Player2Sets { get; set; }
    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public Guid? CurrentLegId { get; set; }
    public Guid? StartingPlayerId { get; set; }
    [MaxLength(200)]
    public string? Venue { get; set; }
    [MaxLength(100)]
    public string? Tournament { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Set> Sets { get; set; } = [];
}

public class Set
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public int SetNumber { get; set; }
    public int Player1Legs { get; set; }
    public int Player2Legs { get; set; }
    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public bool IsComplete { get; set; }
    public ICollection<Leg> Legs { get; set; } = [];
}

public class Leg
{
    public Guid Id { get; set; }
    public Guid SetId { get; set; }
    public Set Set { get; set; } = null!;
    public int LegNumber { get; set; }
    public int Player1Remaining { get; set; } = 501;
    public int Player2Remaining { get; set; } = 501;
    public Guid? CurrentPlayerId { get; set; }
    public Guid? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public bool IsComplete { get; set; }
    public int Player1DartsThrown { get; set; }
    public int Player2DartsThrown { get; set; }
    public ICollection<Visit> Visits { get; set; } = [];
}

public class Visit
{
    public Guid Id { get; set; }
    public Guid LegId { get; set; }
    public Leg Leg { get; set; } = null!;
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int Score { get; set; }
    public int RemainingAfter { get; set; }
    public bool IsBust { get; set; }
    public bool IsCheckout { get; set; }
    public bool Is180 { get; set; }
    public int VisitNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Article
{
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    public ArticleCategory Category { get; set; }
    public bool IsFeatured { get; set; }
    [MaxLength(100)]
    public string? Author { get; set; }
    public int ReadTimeMinutes { get; set; } = 3;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Suggestion
{
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    public SuggestionType Type { get; set; }
    [MaxLength(50)]
    public string? Icon { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FeaturedHighlight
{
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Subtitle { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    [MaxLength(50)]
    public string? Badge { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? MatchId { get; set; }
    public Match? Match { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
