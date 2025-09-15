public class PostgresData
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Database { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }

    public static PostgresData FromEnv() => new PostgresData
    {
        Host = Env("PG_HOST"),
        Port = int.Parse(Env("PG_PORT", "5432")),
        Database = Env("PG_DB"),
        Username = Env("PG_USER"),
        Password = Env("PG_PASSWORD")
    };

    private static string Env(string key, string? def = null)
        => Environment.GetEnvironmentVariable(key) ?? def
            ?? throw new InvalidOperationException($"Missing env var: {key}");
}