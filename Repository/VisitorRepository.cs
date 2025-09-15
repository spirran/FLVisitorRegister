using Npgsql;

public class VisitorRepository : IVisitorRepository
{
    private readonly NpgsqlDataSource _ds;
    public VisitorRepository(NpgsqlDataSource ds) => _ds = ds;

    public async Task<int> InsertAsync(Visitor v, CancellationToken ct = default)
    {
        await using var cmd = _ds.CreateCommand(
            "INSERT INTO visitors (name, client_ip, user_agent) VALUES (@n, @ip, @ua)");
        cmd.Parameters.AddWithValue("n", v.Name);
        cmd.Parameters.AddWithValue("ip", (object?)v.ClientIp ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ua", (object?)v.UserAgent ?? DBNull.Value);
        return await cmd.ExecuteNonQueryAsync(ct);
    }
}