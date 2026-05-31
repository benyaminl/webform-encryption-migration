# WebFormEncryptionCore

ASP.NET Core 10 MVC application that serves as the **new frontend** for the WebFormEncryptionApp, implementing the **Strangler Fig pattern** for incremental migration from .NET Framework 4.8 WebForms.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│  Browser (all traffic on port 5000)                             │
└────────────────────────────┬────────────────────────────────────┘
                             │
┌────────────────────────────▼────────────────────────────────────┐
│  WebFormEncryptionCore (.NET Core 10 MVC)  :5000                │
│                                                                 │
│  ┌──────────────┐   ┌──────────────────────────────────────┐   │
│  │ UsersController│   │ YARP Reverse Proxy (catch-all)       │   │
│  │ /Users/*      │   │ Adds /WebFormEncryptionApp prefix     │   │
│  │ [Migrated]    │   │ Strips prefix from redirect headers  │   │
│  └───────┬───────┘   └──────────────────┬───────────────────┘   │
│          │                               │                      │
│          │  RemoteSessionService         │                      │
│          │  (reads session via HTTP)     │                      │
│          │         │                     │                      │
└──────────┼─────────┼─────────────────────┼──────────────────────┘
           │         │                     │
           │         ▼                     ▼
           │  ┌─────────────────────────────────────────────┐
           │  │  WebFormEncryptionApp (IIS)  :80             │
           │  │  /WebFormEncryptionApp/                      │
           │  │                                             │
           │  │  SessionApi.ashx ← session read endpoint    │
           │  │  Default.aspx, Login.aspx, Settings.aspx    │
           │  │  Download.aspx                              │
           │  └─────────────────────────────────────────────┘
           │
           ▼
    ┌──────────────┐
    │  SQLite DB   │
    │  (shared)    │
    └──────────────┘
```

## Strangler Fig Pattern

The migration follows these principles:

1. **New app is the entry point** — All traffic goes through port 5000 (Core app)
2. **YARP proxies unmigrated pages** — Any route not handled by MVC controllers is forwarded to the legacy WebForm app on IIS
3. **Session sharing** — The Core app reads session state from the WebForm app via `SessionApi.ashx`
4. **Migrate one feature at a time** — Currently only User Management is migrated; everything else still runs on WebForm
5. **Clean URLs** — The `/WebFormEncryptionApp` IIS virtual directory is invisible to the user; YARP handles path rewriting

## What's Migrated

| Feature | Old Route | New Route | Status |
|---------|-----------|-----------|--------|
| User Management | Users.aspx | /Users, /Users/Create, /Users/Edit/{id} | ✅ Migrated |
| Login | Login.aspx | /Login.aspx (proxied) | Proxied |
| File Encryption | Default.aspx | /Default.aspx (proxied) | Proxied |
| Settings | Settings.aspx | /Settings.aspx (proxied) | Proxied |
| File Download | Download.aspx | /Download.aspx (proxied) | Proxied |

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "RemoteAppUri": "http://localhost:8080/",
  "RemoteAppApiKey": "00000000-0000-0000-0000-000000000001",
  "DatabasePath": "encryption_history.db",
  "ReverseProxy": {
    "Routes": {
      "fallback": {
        "ClusterId": "webform",
        "Order": 9999,
        "Match": {
          "Path": "{**catch-all}"
        },
        "Transforms": [
          { "PathPrefix": "/WebFormEncryptionApp" }
        ]
      }
    },
    "Clusters": {
      "webform": {
        "Destinations": {
          "legacy": {
            "Address": "http://localhost"
          }
        }
      }
    }
  }
}

```

| Setting | Purpose |
|---------|---------|
| `RemoteAppUri` | Base URL of the WebForm app (with trailing slash) for session API calls |
| `RemoteAppApiKey` | Shared secret between Core and WebForm for session endpoint auth |
| `DatabasePath` | Path to the shared SQLite database |
| `ReverseProxy.Routes.fallback` | Catch-all route (Order 9999) that forwards unmatched requests to IIS |
| `PathPrefix` transform | Prepends `/WebFormEncryptionApp` to the path before forwarding |

### YARP Response Transform

The `Program.cs` registers a response transform that strips `/WebFormEncryptionApp` from `Location` headers. This ensures that when IIS sends a 302 redirect like `Location: /WebFormEncryptionApp/Default.aspx`, the browser sees `/Default.aspx` instead.

## Session Sharing

The Core app cannot directly access the WebForm app's in-process session. Instead:

1. **WebForm exposes `SessionApi.ashx`** — An HTTP handler that reads the ASP.NET session and returns `UserId`, `Username`, `IsAdmin` as JSON
2. **Core's `RemoteSessionService`** forwards the browser's cookies to this endpoint
3. The `UsersController` calls `RemoteSessionService` on each request to verify authentication and admin status

### Flow

```
Browser request to /Users
  → UsersController receives request (has ASP.NET_SessionId cookie)
  → RemoteSessionService sends GET to http://localhost/WebFormEncryptionApp/SessionApi.ashx
    with Cookie header forwarded + X-Session-ApiKey header
  → SessionApi.ashx reads Session["UserId"], Session["IsAdmin"], etc.
  → Returns JSON: {"UserId":1,"Username":"admin","IsAdmin":true}
  → UsersController uses this data for auth checks
```

### Security

- `SessionApi.ashx` requires the `X-Session-ApiKey` header matching the configured key
- Anonymous access is allowed for `SessionApi.ashx` in Web.config (it does its own API key auth)
- The ASP.NET Forms Authentication cookie (`.ASPXAUTH`) + session cookie are forwarded by the browser naturally since both apps share the same domain

## Project Structure

```
WebFormEncryptionCore/
├── Controllers/
│   ├── UsersController.cs      Migrated user management (CRUD)
│   └── DebugController.cs      Debug endpoint for session inspection
├── Models/
│   └── User.cs                 User entity (shared with WebForm)
├── Services/
│   ├── AuthService.cs          SQLite user CRUD (ported from WebForm)
│   └── RemoteSessionService.cs Fetches session from WebForm via HTTP
├── Views/
│   └── Users/
│       ├── Index.cshtml        User list with edit/delete
│       ├── Create.cshtml       Create user form
│       └── Edit.cshtml         Edit user form
├── Program.cs                  App startup, YARP config, response transforms
├── appsettings.json            Configuration
└── WebFormEncryptionCore.csproj
```

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Yarp.ReverseProxy | 2.3.0 | Reverse proxy to forward traffic to WebForm |
| Microsoft.Data.Sqlite | 10.0.8 | SQLite access for user management |

## Running

### Prerequisites

- .NET 10 SDK
- WebFormEncryptionApp deployed to IIS at `http://localhost/WebFormEncryptionApp/`

### Shared Database Setup

Both apps share the same SQLite database. Create a symlink so the Core app points to the Framework app's database:

**Windows (run as Administrator):**
```cmd
mklink C:\Users\Ben\CodeWin\WebFormEncryptionMigration\WebFormEncryptionCore\encryption_history.db C:\Users\Ben\CodeWin\WebFormEncryptionMigration\WebFormEncryptionApp\App_Data\encryption_history.db
```

**WSL / Linux:**
```bash
ln -s ../WebFormEncryptionApp/App_Data/encryption_history.db WebFormEncryptionCore/encryption_history.db
```

### Start

```bash
cd WebFormEncryptionCore
dotnet run
```

The app starts on `http://localhost:5000`. All WebForm pages are accessible through this port.

### Debugging Session

Visit `http://localhost:5000/Debug/Session` to inspect:
- What cookies the browser is sending
- What session data the RemoteSessionService retrieves from WebForm

## How to Migrate the Next Feature

1. Create a new Controller in the Core app (e.g., `SettingsController`)
2. Add views under `Views/{ControllerName}/`
3. The controller will automatically take priority over YARP (YARP's route has Order 9999)
4. Update the WebForm `.aspx` page to redirect to the new route (optional, for direct access)
5. Read session via `RemoteSessionService` for auth checks

Once all features are migrated, the WebForm app and YARP proxy can be removed entirely.
