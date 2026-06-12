using DartsApi.Data;
using DartsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DartsApi.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(DartsDbContext db)
    {
        if (await db.Players.AnyAsync()) return;

        var profile = new UserProfile
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Alex Morgan",
            Email = "alex@dartspro.local",
            Theme = ThemeMode.Dark,
            Total180s = 12,
            MatchesPlayed = 28,
            MatchesWon = 16,
            AverageScore = 87.4
        };
        db.UserProfiles.Add(profile);

        var pros = new[]
        {
            ("Luke Littler", "ENG", 1, 99.2, 45),
            ("Michael van Gerwen", "NED", 2, 98.7, 120),
            ("Luke Humphries", "ENG", 3, 97.5, 85),
            ("Gerwyn Price", "WAL", 4, 96.8, 72),
            ("Rob Cross", "ENG", 5, 95.4, 58),
            ("Peter Wright", "SCO", 6, 94.9, 65),
            ("Dimitri Van den Bergh", "BEL", 7, 94.2, 42),
            ("Nathan Aspinall", "ENG", 8, 93.8, 38),
            ("Jonny Clayton", "WAL", 9, 93.1, 35),
            ("Danny Noppert", "NED", 10, 92.7, 30),
            ("Josh Rock", "NIR", 11, 92.3, 28),
            ("Gary Anderson", "SCO", 12, 91.9, 95),
            ("Dave Chisnall", "ENG", 13, 91.5, 48),
            ("Chris Dobey", "ENG", 14, 91.0, 32),
            ("Ryan Searle", "ENG", 15, 90.6, 25),
            ("Ross Smith", "ENG", 16, 90.2, 22),
            ("Stephen Bunting", "ENG", 17, 89.8, 40),
            ("Martin Schindler", "GER", 18, 89.4, 18),
            ("Andrew Gilding", "ENG", 19, 89.0, 20),
            ("Damon Heta", "AUS", 20, 88.6, 24)
        };

        var players = pros.Select((p, i) => new Player
        {
            Id = Guid.NewGuid(),
            Name = p.Item1,
            CountryCode = p.Item2,
            Ranking = p.Item3,
            Average = p.Item4,
            Total180s = p.Item5,
            IsLocal = false
        }).ToList();

        var localPlayer = new Player
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Alex Morgan",
            CountryCode = "ENG",
            Ranking = null,
            Average = 87.4,
            Total180s = 12,
            IsLocal = true
        };
        players.Add(localPlayer);
        db.Players.AddRange(players);

        var tournaments = new[] { "PDC World Championship", "Premier League", "UK Open", "World Matchplay", "Grand Slam", "European Championship", "Players Championship", "World Grand Prix" };
        var venues = new[] { "Alexandra Palace", "O2 Arena", "Butlin's Minehead", "Blackpool Winter Gardens", "AFAS Dome", "MEO Arena", "Motorpoint Arena" };
        var random = new Random(42);
        var matches = new List<Match>();

        for (var i = 0; i < 25; i++)
        {
            var p1 = players[random.Next(players.Count - 1)];
            var p2 = players.Where(p => p.Id != p1.Id).OrderBy(_ => random.Next()).First();
            var status = i switch
            {
                < 5 => MatchStatus.InProgress,
                < 18 => MatchStatus.Completed,
                < 22 => MatchStatus.Scheduled,
                _ => MatchStatus.Completed
            };

            var match = new Match
            {
                Id = Guid.NewGuid(),
                Title = $"{p1.Name} vs {p2.Name}",
                Player1Id = p1.Id,
                Player2Id = p2.Id,
                SetsToWin = 3,
                LegsPerSet = 3,
                StartingScore = 501,
                Status = status,
                Venue = venues[random.Next(venues.Length)],
                Tournament = tournaments[random.Next(tournaments.Length)],
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60))
            };

            if (status is MatchStatus.InProgress or MatchStatus.Completed)
            {
                match.StartedAt = match.CreatedAt.AddHours(1);
                match.StartingPlayerId = random.Next(2) == 0 ? p1.Id : p2.Id;
            }

            matches.Add(match);
        }

        db.Matches.AddRange(matches);
        await db.SaveChangesAsync();

        foreach (var match in matches.Where(m => m.Status == MatchStatus.Completed))
        {
            await SeedCompletedMatchAsync(db, match, random);
        }

        foreach (var match in matches.Where(m => m.Status == MatchStatus.InProgress))
        {
            await SeedInProgressMatchAsync(db, match, random);
        }

        SeedArticles(db);
        SeedSuggestions(db);
        await db.SaveChangesAsync();

        var completedMatches = matches.Where(m => m.Status == MatchStatus.Completed).Take(5).ToList();
        SeedFeatured(db, completedMatches);
        await db.SaveChangesAsync();
    }

    private static async Task SeedCompletedMatchAsync(DartsDbContext db, Match match, Random random)
    {
        var p1Sets = 0;
        var p2Sets = 0;
        var setsToWin = match.SetsToWin;

        while (p1Sets < setsToWin && p2Sets < setsToWin)
        {
            var set = new Set
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                SetNumber = p1Sets + p2Sets + 1
            };

            var p1Legs = 0;
            var p2Legs = 0;
            var legsPerSet = match.LegsPerSet;

            while (p1Legs < legsPerSet && p2Legs < legsPerSet)
            {
                var legWinner = random.Next(2) == 0 ? match.Player1Id : match.Player2Id;
                var leg = new Leg
                {
                    Id = Guid.NewGuid(),
                    SetId = set.Id,
                    LegNumber = p1Legs + p2Legs + 1,
                    Player1Remaining = 0,
                    Player2Remaining = 0,
                    WinnerId = legWinner,
                    IsComplete = true,
                    Player1DartsThrown = random.Next(12, 24),
                    Player2DartsThrown = random.Next(12, 24),
                    CurrentPlayerId = null
                };
                db.Legs.Add(leg);
                SeedVisitsForLeg(db, leg, match, random);

                if (legWinner == match.Player1Id) p1Legs++;
                else p2Legs++;
            }

            set.Player1Legs = p1Legs;
            set.Player2Legs = p2Legs;
            set.WinnerId = p1Legs >= legsPerSet ? match.Player1Id : match.Player2Id;
            set.IsComplete = true;
            db.Sets.Add(set);

            if (set.WinnerId == match.Player1Id) p1Sets++;
            else p2Sets++;
        }

        match.Player1Sets = p1Sets;
        match.Player2Sets = p2Sets;
        match.WinnerId = p1Sets >= setsToWin ? match.Player1Id : match.Player2Id;
        match.CompletedAt = match.StartedAt?.AddHours(random.Next(1, 4));
        match.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task SeedInProgressMatchAsync(DartsDbContext db, Match match, Random random)
    {
        var set = new Set
        {
            Id = Guid.NewGuid(),
            MatchId = match.Id,
            SetNumber = 1,
            Player1Legs = random.Next(0, 2),
            Player2Legs = random.Next(0, 2)
        };
        db.Sets.Add(set);

        var leg = new Leg
        {
            Id = Guid.NewGuid(),
            SetId = set.Id,
            LegNumber = set.Player1Legs + set.Player2Legs + 1,
            Player1Remaining = random.Next(40, 300),
            Player2Remaining = random.Next(40, 300),
            CurrentPlayerId = random.Next(2) == 0 ? match.Player1Id : match.Player2Id,
            Player1DartsThrown = random.Next(6, 18),
            Player2DartsThrown = random.Next(6, 18)
        };
        db.Legs.Add(leg);
        match.CurrentLegId = leg.Id;
        match.Player1Sets = 0;
        match.Player2Sets = 0;
        match.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static void SeedVisitsForLeg(DartsDbContext db, Leg leg, Match match, Random random)
    {
        var winnerId = leg.WinnerId ?? match.Player1Id;
        var loserId = winnerId == match.Player1Id ? match.Player2Id : match.Player1Id;
        var visits = new List<Visit>
        {
            new() { Id = Guid.NewGuid(), LegId = leg.Id, PlayerId = winnerId, Score = 180, RemainingAfter = 321, Is180 = true, VisitNumber = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new() { Id = Guid.NewGuid(), LegId = leg.Id, PlayerId = loserId, Score = 100, RemainingAfter = 401, VisitNumber = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-9) },
            new() { Id = Guid.NewGuid(), LegId = leg.Id, PlayerId = winnerId, Score = 140, RemainingAfter = 181, VisitNumber = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
            new() { Id = Guid.NewGuid(), LegId = leg.Id, PlayerId = loserId, Score = 85, RemainingAfter = 316, VisitNumber = 2, CreatedAt = DateTime.UtcNow.AddMinutes(-7) },
            new() { Id = Guid.NewGuid(), LegId = leg.Id, PlayerId = winnerId, Score = 81, RemainingAfter = 100, VisitNumber = 3, CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { Id = Guid.NewGuid(), LegId = leg.Id, PlayerId = loserId, Score = 60, RemainingAfter = 256, VisitNumber = 3, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new() { Id = Guid.NewGuid(), LegId = leg.Id, PlayerId = winnerId, Score = 100, RemainingAfter = 0, IsCheckout = true, VisitNumber = 4, CreatedAt = DateTime.UtcNow.AddMinutes(-4) }
        };
        db.Visits.AddRange(visits);
    }

    private static void SeedArticles(DartsDbContext db)
    {
        var articles = new List<Article>
        {
            new() { Id = Guid.NewGuid(), Title = "Littler Storms to Premier League Victory", Summary = "Teenage sensation Luke Littler claims another major title with a stunning 6-2 final win.", Content = "In a breathtaking display of darts, Luke Littler demonstrated why he is considered the future of the sport. The 17-year-old averaged 104.3 throughout the evening, hitting 12 maximums on his way to a commanding victory.\n\nThe final leg saw Littler check out 132 to seal the deal, sending the capacity crowd into raptures. His opponent, a seasoned campaigner, could only watch in admiration as the youngster showcased nerves of steel.", Category = ArticleCategory.Featured, IsFeatured = true, Author = "PDC News Desk", ReadTimeMinutes = 5, PublishedAt = DateTime.UtcNow.AddHours(-6), ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=800" },
            new() { Id = Guid.NewGuid(), Title = "World Championship Draw Announced", Summary = "The PDC World Championship draw has been made at Alexandra Palace with blockbuster first-round ties.", Content = "The draw for the 2025 PDC World Darts Championship has thrown up several tantalising first-round matches. Defending champion will face a qualifier in the opening round, while former champions have been drawn on opposite sides of the bracket.\n\nTournament director confirmed record prize money of £2.5 million, with the winner taking home £500,000.", Category = ArticleCategory.Tournament, IsFeatured = true, Author = "Tournament Office", ReadTimeMinutes = 4, PublishedAt = DateTime.UtcNow.AddDays(-1), ImageUrl = "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800" },
            new() { Id = Guid.NewGuid(), Title = "MVG Returns to Form with 9-Darter", Summary = "Michael van Gerwen throws a perfect nine-dart leg during practice ahead of the UK Open.", Content = "Michael van Gerwen reminded the darts world of his class by throwing a nine-dart finish during a practice session. The Dutch legend, seeking to reclaim the world number one spot, has been working tirelessly on his game.\n\nFans will hope this is a sign of things to come as the tournament season heats up.", Category = ArticleCategory.News, IsFeatured = false, Author = "Sports Reporter", ReadTimeMinutes = 3, PublishedAt = DateTime.UtcNow.AddDays(-2), ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Price Announces New Walk-On Song", Summary = "Gerwyn Price reveals a new entrance anthem for the upcoming Premier League season.", Content = "The Iceman is known for his electrifying walk-ons, and this season promises to be no different. Price has collaborated with a Welsh rock band to create a custom entrance track that he hopes will energise crowds across the UK and Europe.", Category = ArticleCategory.Player, IsFeatured = false, Author = "Player Media", ReadTimeMinutes = 2, PublishedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = Guid.NewGuid(), Title = "Mastering the Checkout: Top 10 Finishes", Summary = "Expert analysis of the most common and effective checkout combinations for 501 players.", Content = "Whether you are stuck on 40 or facing a tricky 170 finish, knowing your checkouts is essential. This guide covers the most efficient routes for every finish from 170 down to 2.\n\nPractice these combinations regularly and watch your game improve dramatically.", Category = ArticleCategory.Tips, IsFeatured = true, Author = "Coaching Team", ReadTimeMinutes = 8, PublishedAt = DateTime.UtcNow.AddDays(-4), ImageUrl = "https://images.unsplash.com/photo-1606107557195-0e29a4b5b4aa?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Humphries Extends Winning Streak", Summary = "World champion Luke Humphries wins his fifth consecutive ranking event.", Content = "Luke Humphries continues his dominant run with another tournament victory. The world number one has now won five straight ranking events, a feat not achieved since the days of Phil Taylor's dominance.", Category = ArticleCategory.News, IsFeatured = false, Author = "PDC News Desk", ReadTimeMinutes = 4, PublishedAt = DateTime.UtcNow.AddDays(-5) },
            new() { Id = Guid.NewGuid(), Title = "New Dartboard Technology Launched", Summary = "Smart dartboards with automatic scoring are changing how amateur players practice.", Content = "The latest generation of smart dartboards offers automatic scoring, online play, and detailed statistics tracking. We review the top models and explain how they can accelerate your improvement.", Category = ArticleCategory.Tips, IsFeatured = false, Author = "Equipment Review", ReadTimeMinutes = 6, PublishedAt = DateTime.UtcNow.AddDays(-6) },
            new() { Id = Guid.NewGuid(), Title = "Anderson Bids Farewell to Major Stage", Summary = "Two-time world champion Gary Anderson hints at retirement after emotional exit.", Content = "Gary Anderson fought back tears as he addressed the crowd following his exit from the World Matchplay. The Flying Scotsman, a two-time world champion, suggested this could be his final appearance at the Winter Gardens.", Category = ArticleCategory.Player, IsFeatured = true, Author = "Feature Writer", ReadTimeMinutes = 5, PublishedAt = DateTime.UtcNow.AddDays(-7), ImageUrl = "https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Premier League Night 5 Preview", Summary = "Everything you need to know ahead of another thrilling night of Premier League action.", Content = "Night 5 of the Premier League promises fireworks as the top four players battle for crucial table points. We preview each match, analyse head-to-head records, and predict the outcomes.", Category = ArticleCategory.Tournament, IsFeatured = false, Author = "Preview Team", ReadTimeMinutes = 7, PublishedAt = DateTime.UtcNow.AddHours(-12) },
            new() { Id = Guid.NewGuid(), Title = "Youth Academy Produces Next Star", Summary = "PDC Youth Academy graduate wins first ProTour event at just 19 years old.", Content = "The PDC's investment in youth development continues to pay dividends as another academy graduate makes his mark on the professional circuit. The 19-year-old averaged 95.7 in the final to claim his first title.", Category = ArticleCategory.News, IsFeatured = false, Author = "Development Desk", ReadTimeMinutes = 3, PublishedAt = DateTime.UtcNow.AddDays(-8) },
            new() { Id = Guid.NewGuid(), Title = "Bunting's Banana Celebration Goes Viral", Summary = "Stephen Bunting's unique celebration captures hearts on social media.", Content = "Stephen Bunting's post-victory banana celebration has become an internet sensation. The former BDO champion, known for his colourful personality, has embraced the meme with characteristic humour.", Category = ArticleCategory.Player, IsFeatured = false, Author = "Social Media Team", ReadTimeMinutes = 2, PublishedAt = DateTime.UtcNow.AddDays(-9) },
            new() { Id = Guid.NewGuid(), Title = "World Cup of Darts: Team England Favourites", Summary = "England enter the World Cup of Darts as overwhelming favourites with a star-studded pairing.", Content = "The pairing of Humphries and Littler makes England the team to beat at this year's World Cup of Darts. We examine the challengers from Netherlands, Wales, and Scotland who will be looking to upset the odds.", Category = ArticleCategory.Tournament, IsFeatured = true, Author = "International Desk", ReadTimeMinutes = 5, PublishedAt = DateTime.UtcNow.AddDays(-10), ImageUrl = "https://images.unsplash.com/photo-1461896836934-ff606baadc19?w=800" }
        };

        db.Articles.AddRange(articles);
    }

    private static void SeedSuggestions(DartsDbContext db)
    {
        var suggestions = new List<Suggestion>
        {
            new() { Id = Guid.NewGuid(), Title = "Practice Doubles Daily", Description = "Spend 15 minutes each day on doubles practice. Focus on D16, D20, and D18 — the most common checkout doubles.", Type = SuggestionType.Practice, Icon = "target", Priority = 1 },
            new() { Id = Guid.NewGuid(), Title = "Warm Up Before Matches", Description = "Arrive 30 minutes early and complete a structured warm-up: 20 scoring darts, 10 doubles, then 5 checkout attempts.", Type = SuggestionType.Strategy, Icon = "flame", Priority = 2 },
            new() { Id = Guid.NewGuid(), Title = "Upgrade Your Stems", Description = "Consider medium-length stems with a slightly heavier barrel for improved grouping and consistency.", Type = SuggestionType.Equipment, Icon = "construct", Priority = 3 },
            new() { Id = Guid.NewGuid(), Title = "Enter Local League", Description = "Join a local pub league to gain match experience under pressure. Check your regional darts association for fixtures.", Type = SuggestionType.Tournament, Icon = "trophy", Priority = 4 },
            new() { Id = Guid.NewGuid(), Title = "Track Your Averages", Description = "Use this app to monitor your 3-dart average over time. Aim for steady improvement rather than quick fixes.", Type = SuggestionType.Practice, Icon = "analytics", Priority = 5 },
            new() { Id = Guid.NewGuid(), Title = "Master the 170 Finish", Description = "T20, T20, Bull is the standard 170 checkout. Practice until it becomes muscle memory.", Type = SuggestionType.Strategy, Icon = "star", Priority = 6 },
            new() { Id = Guid.NewGuid(), Title = "Watch Pro Footage", Description = "Study how professionals set up their scoring and manage the board. PDC's YouTube channel has thousands of match legs.", Type = SuggestionType.Strategy, Icon = "play-circle", Priority = 7 },
            new() { Id = Guid.NewGuid(), Title = "Consistent Throw Routine", Description = "Develop a repeatable pre-throw routine: stance, grip, sight, throw. Consistency in setup leads to consistency in results.", Type = SuggestionType.Practice, Icon = "repeat", Priority = 8 }
        };

        db.Suggestions.AddRange(suggestions);
    }

    private static void SeedFeatured(DartsDbContext db, List<Match> matches)
    {
        var highlights = new List<FeaturedHighlight>
        {
            new() { Id = Guid.NewGuid(), Title = "Premier League Live", Subtitle = "Night 8 — All the action from the O2 Arena", Badge = "LIVE", SortOrder = 1, ImageUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=800", MatchId = matches.ElementAtOrDefault(0)?.Id },
            new() { Id = Guid.NewGuid(), Title = "World Championship Countdown", Subtitle = "32 days until the Ally Pally extravaganza", Badge = "FEATURED", SortOrder = 2, ImageUrl = "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Player of the Month", Subtitle = "Luke Littler — 3 titles, 2 nine-darters", Badge = "PLAYER", SortOrder = 3, ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800" },
            new() { Id = Guid.NewGuid(), Title = "Your Recent Win", Subtitle = "Replay your last completed match recap", Badge = "YOU", SortOrder = 4, MatchId = matches.ElementAtOrDefault(1)?.Id }
        };

        db.FeaturedHighlights.AddRange(highlights);
    }
}
