using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DartsApi.E2E.Tests.Infrastructure;

[Collection(E2ECollection.Name)]
public abstract class E2ETestBase : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    protected readonly PostgresFixture Postgres;
    protected readonly DartsApiFactory Factory;
    protected HttpClient Client = null!;

    protected E2ETestBase(PostgresFixture postgres)
    {
        Postgres = postgres;
        Factory = new DartsApiFactory(postgres);
    }

    public Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }

    protected async Task<T> GetAsync<T>(string path)
    {
        var response = await Client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string path, T body) =>
        await Client.PostAsJsonAsync(path, body);

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string path, T body) =>
        await Client.PutAsJsonAsync(path, body);

    protected async Task<T> ReadJsonAsync<T>(HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }
}
