using System.Net;
using DartsApi.E2E.Tests.Infrastructure;
using FluentAssertions;

namespace DartsApi.E2E.Tests;

public class HealthE2ETests(PostgresFixture postgres) : E2ETestBase(postgres)
{
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var result = await GetAsync<HealthResponse>("/api/health");

        result.Status.Should().Be("healthy");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    private record HealthResponse(string Status, DateTime Timestamp);
}
