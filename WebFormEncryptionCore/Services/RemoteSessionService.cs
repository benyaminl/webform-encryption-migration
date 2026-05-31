using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebFormEncryptionCore.Services;

public class RemoteSessionService
{
    private readonly HttpClient _client;
    private readonly ILogger<RemoteSessionService> _logger;
    private readonly string _baseUri;

    public RemoteSessionService(IConfiguration config, ILogger<RemoteSessionService> logger)
    {
        _baseUri = config["RemoteAppUri"]!;
        _client = new HttpClient { BaseAddress = new Uri(_baseUri) };
        _client.DefaultRequestHeaders.Add("X-Session-ApiKey", config["RemoteAppApiKey"]);
        _logger = logger;
    }

    public async Task<SessionData> GetSessionAsync(string? cookieHeader)
    {
        _logger.LogInformation("RemoteSession: calling {BaseUri}SessionApi.ashx with cookie: {Cookie}",
            _baseUri, cookieHeader ?? "(none)");

        if (string.IsNullOrEmpty(cookieHeader))
            return new SessionData();

        var request = new HttpRequestMessage(HttpMethod.Get, "SessionApi.ashx");
        request.Headers.Add("Cookie", cookieHeader);

        try
        {
            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("RemoteSession: status={Status}, body={Body}",
                response.StatusCode, body);

            if (!response.IsSuccessStatusCode)
                return new SessionData();

            return JsonSerializer.Deserialize<SessionData>(body) ?? new SessionData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RemoteSession: failed to reach session endpoint");
            return new SessionData();
        }
    }

    public async Task<bool> SetSessionAsync(string? cookieHeader, Dictionary<string, object?> values)
    {
        if (string.IsNullOrEmpty(cookieHeader))
            return false;

        var request = new HttpRequestMessage(HttpMethod.Put, "SessionApi.ashx");
        request.Headers.Add("Cookie", cookieHeader);
        request.Content = new StringContent(
            JsonSerializer.Serialize(values),
            System.Text.Encoding.UTF8,
            "application/json");

        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RemoteSession: failed to set session");
            return false;
        }
    }
}

public class SessionData
{
    [JsonPropertyName("UserId")]
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public bool? IsAdmin { get; set; }
}
