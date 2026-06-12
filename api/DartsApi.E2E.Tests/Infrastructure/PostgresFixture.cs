using DartsApi.Data;
using DartsApi.Services;
using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace DartsApi.E2E.Tests.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=dartsdb_test;Username=darts;Password=darts123";

    private PostgreSqlContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        ConnectionString = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
            ?? await ResolveConnectionStringAsync();

        await PrepareDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }

    private async Task<string> ResolveConnectionStringAsync()
    {
        if (!await IsDockerAvailableAsync())
            return FallbackConnectionString;

        try
        {
            return await StartContainerAsync();
        }
        catch
        {
            return FallbackConnectionString;
        }
    }

    private static async Task<bool> IsDockerAvailableAsync()
    {
        try
        {
            using var client = new DockerClientConfiguration().CreateClient();
            await client.System.PingAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> StartContainerAsync()
    {
        _container = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("dartsdb_test")
            .WithUsername("darts")
            .WithPassword("darts123")
            .Build();

        await _container.StartAsync();
        return _container.GetConnectionString();
    }

    private async Task PrepareDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<DartsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var db = new DartsDbContext(options);

        if (_container is not null)
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.MigrateAsync();
            await db.Database.ExecuteSqlRawAsync("""
                TRUNCATE TABLE
                    "Visits",
                    "Legs",
                    "Sets",
                    "Matches",
                    "Players",
                    "UserProfiles",
                    "Articles",
                    "Suggestions",
                    "FeaturedHighlights"
                RESTART IDENTITY CASCADE;
                """);
        }

        await DataSeeder.SeedAsync(db);
    }
}

[CollectionDefinition(Name)]
public class E2ECollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "E2E";
}
