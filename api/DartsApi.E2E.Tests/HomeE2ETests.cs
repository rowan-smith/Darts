using DartsApi.E2E.Tests.Infrastructure;
using FluentAssertions;

namespace DartsApi.E2E.Tests;

public class HomeE2ETests(PostgresFixture postgres) : E2ETestBase(postgres)
{
    [Fact]
    public async Task Home_ReturnsSeededFeed()
    {
        var feed = await GetAsync<HomeFeedResponse>("/api/home");

        feed.RecentScores.Should().NotBeEmpty();
        feed.Articles.Should().NotBeEmpty();
        feed.Suggestions.Should().NotBeEmpty();
        feed.Featured.Should().NotBeEmpty();
        feed.FeaturedArticles.Should().NotBeEmpty();
    }

    private record HomeFeedResponse(
        List<object> RecentScores,
        List<object> Articles,
        List<object> FeaturedArticles,
        List<object> Suggestions,
        List<object> Featured);
}
