using Microsoft.AspNetCore.Mvc;
using WebFormEncryptionCore.Services;

namespace WebFormEncryptionCore.Controllers;

public class UsersController : Controller
{
    private readonly AuthService _auth;
    private readonly RemoteSessionService _session;

    public UsersController(AuthService auth, RemoteSessionService session)
    {
        _auth = auth;
        _session = session;
    }

    private async Task<SessionData> GetSession() =>
        await _session.GetSessionAsync(Request.Headers["Cookie"]);

    public async Task<IActionResult> Index()
    {
        var s = await GetSession();
        if (s.UserId == null || s.IsAdmin != true)
            return Redirect("/Default.aspx");
        return View(_auth.GetAllUsers());
    }

    public async Task<IActionResult> Create()
    {
        var s = await GetSession();
        if (s.IsAdmin != true) return RedirectToAction("Index");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string username, string password, bool isAdmin)
    {
        var s = await GetSession();
        if (s.IsAdmin != true) return RedirectToAction("Index");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            TempData["Error"] = "Username and password required.";
            return View();
        }
        try
        {
            _auth.CreateUser(username, password, isAdmin);
            TempData["Success"] = $"User '{username}' created.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Error: " + ex.Message;
            return View();
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var s = await GetSession();
        if (s.IsAdmin != true) return RedirectToAction("Index");
        var user = _auth.GetAllUsers().FirstOrDefault(u => u.Id == id);
        if (user == null) return RedirectToAction("Index");
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, bool isAdmin, string? newPassword)
    {
        var s = await GetSession();
        if (s.IsAdmin != true) return RedirectToAction("Index");
        _auth.UpdateUser(id, isAdmin, newPassword);
        TempData["Success"] = "User updated.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await GetSession();
        if (s.IsAdmin != true) return RedirectToAction("Index");
        if (id == s.UserId)
        {
            TempData["Error"] = "Cannot delete yourself.";
            return RedirectToAction("Index");
        }
        _auth.DeleteUser(id);
        TempData["Success"] = "User deleted.";
        return RedirectToAction("Index");
    }
}
