using System.Net;
using DartsApi.E2E.Tests.Infrastructure;
using DartsApi.Models;
using FluentAssertions;

namespace DartsApi.E2E.Tests;

public class MatchFlowE2ETests(PostgresFixture postgres) : E2ETestBase(postgres)
{
    [Fact]
    public async Task FullMatchFlow_CreateStartScoreAndRecap()
    {
        var players = await GetAsync<List<PlayerResponse>>("/api/players");
        players.Should().NotBeEmpty();

        var local = players.First(p => p.IsLocal);
        var opponent = players.First(p => !p.IsLocal && p.Id != local.Id);

        var createResponse = await PostJsonAsync("/api/matches", new
        {
            title = "E2E Test Match",
            player1Id = local.Id,
            player2Id = opponent.Id,
            setsToWin = 1,
            legsPerSet = 1,
            startingScore = 501,
            tournament = "E2E Cup",
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await ReadJsonAsync<MatchDetailResponse>(createResponse.Content);
        created.Status.Should().Be(MatchStatus.Scheduled);
        created.Title.Should().Be("E2E Test Match");

        var startResponse = await Client.PostAsync($"/api/matches/{created.Id}/start", null);
        startResponse.EnsureSuccessStatusCode();

        var started = await GetAsync<MatchDetailResponse>($"/api/matches/{created.Id}");
        started.Status.Should().Be(MatchStatus.InProgress);
        started.CurrentLegId.Should().NotBeNull();

        var currentLeg = started.Sets.SelectMany(s => s.Legs).First(l => l.Id == started.CurrentLegId);
        var currentPlayerId = currentLeg.CurrentPlayerId ?? started.StartingPlayerId ?? local.Id;

        var visitResponse = await PostJsonAsync($"/api/matches/{created.Id}/visit", new RecordVisitBody(currentPlayerId, 180));
        visitResponse.EnsureSuccessStatusCode();

        var afterVisit = await ReadJsonAsync<MatchDetailResponse>(visitResponse.Content);
        afterVisit.Sets.Should().NotBeEmpty();
        afterVisit.Sets.SelectMany(s => s.Legs).SelectMany(l => l.Visits).Should().Contain(v => v.Score == 180);

        var recapBeforeComplete = await Client.GetAsync($"/api/matches/{created.Id}/recap");
        recapBeforeComplete.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompletedMatch_ReturnsRecap()
    {
        var matches = await GetAsync<List<MatchSummaryResponse>>("/api/matches?status=Completed");
        matches.Should().NotBeEmpty();

        var match = matches.First();
        var recap = await GetAsync<MatchRecapResponse>($"/api/matches/{match.Id}/recap");

        (recap.Player1Sets + recap.Player2Sets).Should().BeGreaterThan(0);
        recap.WinnerName.Should().NotBeNullOrEmpty();
        recap.Sets.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatePlayer_AndList_Works()
    {
        var response = await PostJsonAsync("/api/players", new { name = "E2E Newcomer", countryCode = "ENG" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await ReadJsonAsync<PlayerResponse>(response.Content);
        created.Name.Should().Be("E2E Newcomer");

        var players = await GetAsync<List<PlayerResponse>>("/api/players?localOnly=true");
        players.Should().Contain(p => p.Name == "E2E Newcomer");
    }

    private record PlayerResponse(Guid Id, string Name, string? CountryCode, bool IsLocal);

    private record MatchSummaryResponse(Guid Id, MatchStatus Status, string? WinnerName);

    private record MatchDetailResponse(
        Guid Id,
        string Title,
        MatchStatus Status,
        Guid? CurrentLegId,
        Guid? StartingPlayerId,
        List<SetResponse> Sets);

    private record SetResponse(List<LegResponse> Legs);

    private record LegResponse(Guid Id, Guid? CurrentPlayerId, List<VisitResponse> Visits);

    private record VisitResponse(int Score);

    private record RecordVisitBody(Guid PlayerId, int Score);

    private record MatchRecapResponse(
        int Player1Sets,
        int Player2Sets,
        string? WinnerName,
        List<object> Sets);
}
