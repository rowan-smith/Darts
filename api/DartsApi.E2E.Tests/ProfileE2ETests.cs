using DartsApi.E2E.Tests.Infrastructure;
using DartsApi.Models;
using FluentAssertions;

namespace DartsApi.E2E.Tests;

public class ProfileE2ETests(PostgresFixture postgres) : E2ETestBase(postgres)
{
    [Fact]
    public async Task Profile_GetAndUpdate_Works()
    {
        var profile = await GetAsync<ProfileResponse>("/api/profile");

        profile.Name.Should().Be("Alex Morgan");
        profile.Email.Should().NotBeNullOrEmpty();

        var updateResponse = await PutJsonAsync("/api/profile", new
        {
            name = "Test Player",
            theme = ThemeMode.Light,
        });
        updateResponse.EnsureSuccessStatusCode();

        var updated = await GetAsync<ProfileResponse>("/api/profile");
        updated.Name.Should().Be("Test Player");
        updated.Theme.Should().Be(ThemeMode.Light);
    }

    private record ProfileResponse(
        Guid Id,
        string Name,
        string Email,
        ThemeMode Theme,
        int Total180s,
        int MatchesPlayed,
        int MatchesWon,
        double AverageScore);
}
