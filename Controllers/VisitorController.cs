using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class VisitorController
{
    private readonly IVisitorService _service;
    private readonly ILogger<VisitorController> _log;
    public VisitorController(IVisitorService service, ILogger<VisitorController> log)
    {
        _service = service;
        _log = log;
    }

    [Function("RegisterVisitor")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options")] HttpRequestData requestData)
    {
        if (requestData.Method == "OPTIONS")
        {
            var pre = requestData.CreateResponse(HttpStatusCode.OK);
            SetCors(pre);
            return pre;
        }

        RegisterVisitorRequest? input;
        try
        {
            var body = await new StreamReader(requestData.Body).ReadToEndAsync();
            input = JsonSerializer.Deserialize<RegisterVisitorRequest>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            var bad = requestData.CreateResponse(HttpStatusCode.BadRequest);
            SetCors(bad);
            await bad.WriteAsJsonAsync(new { error = "Body has to be JSON" });
            return bad;
        }

        var name = input?.name;
        var userAgent = requestData.Headers.TryGetValues("User-Agent", out var ua) ? ua.FirstOrDefault() : null;
        var clientIp = requestData.Headers.TryGetValues("X-Forwarded-For", out var fwd) ? fwd.FirstOrDefault()?.Split(',')[0] : null;
        if (string.IsNullOrWhiteSpace(clientIp))
        {
            clientIp = "unknown";
        }

        try
        {
            await _service.RegisterAsync(name ?? "", clientIp, userAgent);
        }
        catch (ArgumentException ex)
        {
            var bad = requestData.CreateResponse(HttpStatusCode.BadRequest);
            SetCors(bad);
            await bad.WriteAsJsonAsync(new { error = ex.Message });
            return bad;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DB insert failed");
            var err = requestData.CreateResponse(HttpStatusCode.InternalServerError);
            SetCors(err);
            await err.WriteAsJsonAsync(new { error = "Cant save user" });
            return err;
        }

        var ok = requestData.CreateResponse(HttpStatusCode.Created);
        SetCors(ok);
        await ok.WriteAsJsonAsync(new { ok = true, message = $"Registered {name?.Trim()}." });
        return ok;

    }

    private static void SetCors(HttpResponseData responseData)
    {
        var allow = Environment.GetEnvironmentVariable("CORS_ORIGIN") ?? "*";
        responseData.Headers.Add("Access-Control-Allow-Origin", allow);
        responseData.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
        responseData.Headers.Add("Access-Control-Allow-Headers", "content-type");
    }

}