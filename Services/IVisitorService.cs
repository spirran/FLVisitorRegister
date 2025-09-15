public interface IVisitorService
{
    Task RegisterAsync(string name, string? clientIp, string? userAgent, CancellationToken ct = default);
}