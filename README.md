# WebFormEncryptionMigration

Incremental migration of a .NET Framework 4.8 WebForms application to ASP.NET Core 10, using the **Strangler Fig pattern** with YARP reverse proxy.

## Projects

| Project | Framework | Role |
|---------|-----------|------|
| [WebFormEncryptionApp](WebFormEncryptionApp/) | .NET Framework 4.8 | Legacy WebForms app (IIS) — file encryption, login, settings |
| [WebFormEncryptionCore](WebFormEncryptionCore/) | .NET 10 | New frontend (Kestrel) — migrated features + YARP proxy to legacy |

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│  Browser → http://localhost:5000                         │
└─────────────────────────┬────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────┐
│  WebFormEncryptionCore (.NET 10)                          │
│                                                          │
│  • Migrated routes (/Users/*)     → MVC Controllers      │
│  • Unmigrated routes (*.aspx)     → YARP → IIS           │
│  • Session reads                  → SessionApi.ashx      │
└─────────────────────────┬────────────────────────────────┘
                          │
┌─────────────────────────▼────────────────────────────────┐
│  WebFormEncryptionApp (.NET Framework 4.8 / IIS)         │
│                                                          │
│  • Login, Encryption, Settings, Download                 │
│  • SessionApi.ashx (GET/PUT session as JSON)             │
│  • SQLite database                                       │
└──────────────────────────────────────────────────────────┘
```

## How It Works

1. **All traffic enters through the Core app** (port 5000)
2. **Migrated features** are handled by ASP.NET Core MVC controllers
3. **Unmigrated pages** are proxied to the legacy WebForms app via YARP
4. **Session sharing** — Core reads/writes Framework session via a custom `SessionApi.ashx` endpoint, authenticated by API key
5. **Shared database** — Both apps access the same SQLite file

## Migration Status

| Feature | Status | Served By |
|---------|--------|-----------|
| User Management | ✅ Migrated | Core MVC |
| Login | ⏳ Proxied | WebForms |
| File Encryption/Decryption | ⏳ Proxied | WebForms |
| Settings | ⏳ Proxied | WebForms |
| File Download | ⏳ Proxied | WebForms |

## Prerequisites

- **Windows** with IIS/IIS Express (for the Framework app)
- **.NET Framework 4.8** targeting pack + MSBuild 14.0
- **.NET 10 SDK** (for the Core app)
- **NuGet CLI** (for Framework package restore)

## Quick Start

```bash
# 1. Build and deploy the Framework app to IIS
cd WebFormEncryptionApp
build.bat
# Deploy to IIS at http://localhost/WebFormEncryptionApp/

# 2. Run the Core app
cd ../WebFormEncryptionCore
dotnet run
# Browse to http://localhost:5000
```

## Session API

The Framework app exposes `SessionApi.ashx` for cross-app session access:

- **GET** — Returns all session key-value pairs as JSON
- **PUT/POST** — Sets session values from a JSON body, returns updated session

Both require the `X-Session-ApiKey` header. The endpoint is anonymous (no Forms Auth required) but validates the API key internally.

## Migrating the Next Feature

1. Create a new Controller in `WebFormEncryptionCore`
2. Add Razor views under `Views/{Controller}/`
3. The new route automatically takes priority over YARP (fallback route has Order 9999)
4. Use `RemoteSessionService` for auth checks
5. Once all features are migrated, remove YARP and decommission the Framework app
