using Microsoft.Extensions.Logging;

public class VisitorService : IVisitorService
{
    private readonly IVisitorRepository _repo;
    private readonly ILogger<VisitorService> _log;

    public VisitorService(IVisitorRepository repo, ILogger<VisitorService> log)
    {
        _repo = repo;
        _log = log;
    }

    public async Task RegisterAsync(string name, string? clientIp, string? userAgent, CancellationToken ct = default)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 100)
            throw new ArgumentException("Field 'name' requires between 1-100 chars", nameof(name));

        _log.LogInformation("visitor_registered Name={Name} ClientIp={ClientIp} UA={UA}", name, clientIp, userAgent);

        var visitor = new Visitor { Name = name, ClientIp = clientIp, UserAgent = userAgent };
        await _repo.InsertAsync(visitor, ct);
    }
}