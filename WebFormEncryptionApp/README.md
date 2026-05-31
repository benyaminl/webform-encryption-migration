# WebFormEncryptionApp

A web-based file encryption and decryption application using AES-256-CBC. Built with ASP.NET WebForms on .NET Framework 4.8. Users upload files through the browser, and the server encrypts or decrypts them. All processed files are stored on the server with datetime-stamped filenames and are downloadable from the history table.

This project shares the same service logic as the companion WPF desktop application (WpfEncryptionApp), adapted for .NET Framework 4.8 and the web request/response model.

## Architecture

The application uses a service-extraction pattern. Business logic is behind interfaces, instantiated in `Global.asax.cs` at application startup, and exposed via static properties. The WebForms code-behind is a thin layer that delegates to services and updates the page.

```
WebFormEncryptionApp/
  App_Data/
    Files/                          Stored uploads and processed files
  Models/
    EncryptionHistoryEntry.cs       Data model for history records
    ValidationResult.cs             Validation return type (replaces C# 7 tuples)
  Services/
    IEncryptionService.cs           AES-256-CBC file encryption
    EncryptionService.cs
    IDecryptionService.cs           AES-256-CBC file decryption
    DecryptionService.cs
    IHistoryService.cs              SQLite CRUD for operation history
    HistoryService.cs
    IValidationService.cs           Input validation
    ValidationService.cs
  Default.aspx                      Main page (upload, encrypt/decrypt, history)
  Default.aspx.cs                   Code-behind
  Download.aspx                     File download handler
  Download.aspx.cs                  Serves files from App_Data/Files/
  Global.asax                       Application entry point
  Global.asax.cs                    Service composition (manual DI)
  Web.config                        Configuration
  packages.config                   NuGet package references
  WebFormEncryptionApp.csproj       .NET Framework 4.8 old-style project
  build.bat                         Build script (nuget restore + MSBuild)
  publish.bat                       Publish script (build + copy to publish/)
```

### Service Composition

Services are created in `Global.Application_Start` and stored as static properties on the `Global` class:

- `Global.HistoryService` -- receives database path from Web.config appSettings
- `Global.EncryptionService`
- `Global.DecryptionService`
- `Global.ValidationService`
- `Global.FilesPath` -- resolved server path for file storage

Pages access services via `Global.HistoryService`, `Global.EncryptionService`, etc.

This is not a full DI container. It is manual composition, chosen because .NET Framework 4.8 WebForms has no built-in DI support, and adding a container (Unity, Autofac) is unnecessary for this scope.

### Encryption

Identical to the WPF version:

- Algorithm: AES-256-CBC with PKCS7 padding
- Key derivation: PBKDF2 (Rfc2898DeriveBytes) with SHA-256, 100,000 iterations
- Random 16-byte salt and 16-byte IV per operation, prepended to output
- Files encrypted by either the WPF or web version can be decrypted by the other, provided the same password is used

### File Storage and Naming

Uploaded and processed files are stored in `App_Data/Files/` with unique datetime-stamped names to prevent collisions:

```
{prefix}{original_name}_{yyyy-MM-dd_HHmmss}_GMT{+/-offset}{extension}
```

Examples:
```
report_2026-04-25_215933_GMT+7.pdf              (uploaded source)
encrypted_report_2026-04-25_215934_GMT+7.pdf    (encrypted output)
decrypted_report_2026-04-25_215935_GMT+7.pdf    (decrypted output)
```

The timezone offset is derived from the server's local time.

### File Downloads

The `App_Data/` directory is protected by IIS and cannot be browsed directly. Files are served through `Download.aspx`, which:

1. Reads the `file` query parameter.
2. Validates the filename (rejects path traversal characters: `..`, `/`, `\`).
3. Checks the file exists in `App_Data/Files/`.
4. Sends the file with `Content-Disposition: attachment`.

The history GridView renders download links as `Download.aspx?file={url-encoded-filename}`.

### History

Operation history is stored in a SQLite database at the path specified in Web.config. Each record contains the original filename, machine name, timestamp, stored source filename, stored output filename, and action type. The history GridView displays all records with direct download links for both the source (before) and output (after) files.

### Differences from WPF Version

| Aspect | WPF | WebForms |
|--------|-----|----------|
| Framework | .NET 8 | .NET Framework 4.8 |
| SQLite package | Microsoft.Data.Sqlite | System.Data.SQLite |
| DI | Microsoft.Extensions.DependencyInjection | Manual composition in Global.asax |
| Validation return | C# 7 value tuple (bool, string) | ValidationResult class |
| File selection | Native file dialog | FileUpload control (browser upload) |
| File output | Written to local disk path | Stored on server, downloaded via browser |
| C# version | C# 12 (SDK-style) | C# 6 (MSBuild 14.0) |
| Configuration | appsettings.json | Web.config appSettings |

## Incremental Migration (.NET Core 10)

This application is being gradually migrated to ASP.NET Core 10 using the **Strangler Fig pattern**. A companion project `WebFormEncryptionCore` acts as the new frontend:

- All traffic enters through the .NET Core app (port 5000)
- YARP reverse proxy forwards unmigrated pages to this WebForm app (IIS)
- Session data is shared via a custom `SessionApi.ashx` endpoint
- Migrated features: **User Management** (`/Users` route in .NET Core MVC)

See `../WebFormEncryptionCore/README.md` for full architecture details.

### Changes for Migration

- `SessionApi.ashx` — exposes session data (UserId, Username, IsAdmin) as JSON, protected by API key
- `Web.config` — added `RemoteAppApiKey` and anonymous access for `SessionApi.ashx`
- `Users.aspx` — now redirects to `/Users` (handled by .NET Core MVC)

## Prerequisites

- .NET Framework 4.8 (runtime and targeting pack)
- MSBuild 14.0 (`C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe`)
- NuGet CLI (`C:\Apps\nuget.exe`)
- IIS or IIS Express for hosting

The MSBuild 14.0 toolset ships with Visual Studio 2015 Build Tools. It supports C# 6. The `Microsoft.Net.Compilers` NuGet package is not compatible with MSBuild 14.0, so C# 7+ features (tuples, pattern matching) cannot be used.

## Configuration

Edit `Web.config` to change paths:

```xml
<appSettings>
  <add key="DatabasePath" value="~/App_Data/encryption_history.db" />
  <add key="FilesPath" value="~/App_Data/Files/" />
</appSettings>
```

Both paths use ASP.NET virtual path syntax (`~/`), resolved to physical paths at startup via `Server.MapPath`.

The `maxRequestLength` in `httpRuntime` controls the maximum upload size. Default is set to 50 MB (52428800 KB).

## Build

```
build.bat
```

This runs `nuget restore` followed by MSBuild. Output is in `bin\`.

## Publish

```
publish.bat
```

This builds in Release mode, then copies the deployable files to `publish\`:

```
publish/
  App_Data/Files/               Writable directory for uploads
  bin/
    WebFormEncryptionApp.dll    Compiled application
    System.Data.SQLite.dll      SQLite managed library
    x64/SQLite.Interop.dll      Native SQLite (64-bit)
    x86/SQLite.Interop.dll      Native SQLite (32-bit)
  Default.aspx                  Main page
  Download.aspx                 File download handler
  Global.asax                   Application entry
  Web.config                    Configuration
```

## Deployment to IIS

1. Run `publish.bat`.
2. Copy the contents of `publish\` to the IIS site directory (e.g., `C:\inetpub\wwwroot\WebformEncryptionApp\`).
3. Ensure the IIS application pool is set to .NET Framework 4.0 (which covers 4.8), Integrated pipeline mode.
4. Grant the application pool identity (typically `IIS AppPool\DefaultAppPool`) write permissions on the `App_Data\` directory. This is required for SQLite database creation and file storage.
5. Browse to `http://localhost/WebformEncryptionApp/Default.aspx`.

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Stub.System.Data.SQLite.Core.NetFramework | 1.0.118.0 | SQLite managed + native libraries |
| System.Data.SQLite.Core | 1.0.118.0 | SQLite core dependency |

## Usage

1. Open the application in a browser.
2. Select a file using the file upload control.
3. Optionally enter a custom output filename.
4. Enter a password.
5. Click "Encrypt" or "Decrypt".
6. The file is processed on the server. A status message confirms success or failure.
7. The history table below shows all operations. Each row has "Download" links for both the original uploaded file and the processed output file.
8. Click "Refresh History" to reload the table.

The password must be the same for encryption and decryption of a given file. There is no password recovery mechanism.
