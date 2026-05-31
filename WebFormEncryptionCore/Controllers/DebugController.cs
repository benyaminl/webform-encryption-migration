using Microsoft.AspNetCore.Mvc;
using WebFormEncryptionCore.Services;

namespace WebFormEncryptionCore.Controllers;

public class DebugController : Controller
{
    private readonly RemoteSessionService _session;

    public DebugController(RemoteSessionService session) => _session = session;

    // GET /Debug/Session - shows what session data we can fetch
    public async Task<IActionResult> Session()
    {
        var cookieHeader = Request.Headers["Cookie"].ToString();
        var data = await _session.GetSessionAsync(cookieHeader);
        return Json(new
        {
            cookieHeaderSent = cookieHeader,
            session = data
        });
    }
}
