namespace DartsApi.Models;

public enum MatchStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}

public enum ThemeMode
{
    Light,
    Dark,
    System
}

public enum ArticleCategory
{
    News,
    Tournament,
    Player,
    Tips,
    Featured
}

public enum SuggestionType
{
    Practice,
    Strategy,
    Equipment,
    Tournament
}
