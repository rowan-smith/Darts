using System.Net;
using DartsApi.E2E.Tests.Infrastructure;
using FluentAssertions;

namespace DartsApi.E2E.Tests;

public class HealthE2ETests(PostgresFixture postgres) : E2ETestBase(postgres)
{
    [Theory]
    [InlineData("/api/health")]
    [InlineData("/api/healthz")]
    public async Task Health_ReturnsHealthy(string path)
    {
        var result = await GetAsync<HealthResponse>(path);

        result.Status.Should().Be("healthy");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    private record HealthResponse(string Status, DateTime Timestamp);
}
