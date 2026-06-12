using DartsApi.E2E.Tests.Infrastructure;
using FluentAssertions;

namespace DartsApi.E2E.Tests;

public class ArticlesE2ETests(PostgresFixture postgres) : E2ETestBase(postgres)
{
    [Fact]
    public async Task Articles_ListAndGetById_Works()
    {
        var articles = await GetAsync<List<ArticleResponse>>("/api/articles");
        articles.Should().NotBeEmpty();

        var featured = await GetAsync<List<ArticleResponse>>("/api/articles?featured=true");
        featured.Should().NotBeEmpty();
        featured.Should().OnlyContain(a => a.IsFeatured);

        var article = await GetAsync<ArticleResponse>($"/api/articles/{articles[0].Id}");
        article.Title.Should().Be(articles[0].Title);
        article.Content.Should().NotBeNullOrEmpty();
    }

    private record ArticleResponse(Guid Id, string Title, string Content, bool IsFeatured);
}
