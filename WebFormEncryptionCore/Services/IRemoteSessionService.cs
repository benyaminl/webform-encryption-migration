namespace WebFormEncryptionCore.Services;

public interface IRemoteSessionService
{
    Task<SessionData> GetSessionAsync(string? cookieHeader);
    Task<bool> SetSessionAsync(string? cookieHeader, Dictionary<string, object?> values);
}
