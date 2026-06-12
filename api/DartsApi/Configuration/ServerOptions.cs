namespace DartsApi.Configuration;

public class ServerOptions
{
    public const string SectionName = "Server";

    public int Port { get; set; } = 8080;

    /// <summary>
    /// Resolves listen port: <c>Server:Port</c> in config, then <c>PORT</c> env, then 8080.
    /// </summary>
    public static ServerOptions GetServerOptions(IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ServerOptions>() ?? new ServerOptions();

        if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var envPort) && envPort > 0)
            options.Port = envPort;

        if (options.Port <= 0)
            options.Port = 8080;

        return options;
    }
}
