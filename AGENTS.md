# Repository Guidelines

## Project Structure & Module Organization

`src/WinNotiMirror/` contains the .NET 8 WPF application. `App.xaml.cs` handles startup; `NotificationListenerService.cs` reads toast notifications; `OverlayManager.cs` and `OverlayWindow.xaml` display banners on each monitor. `TrayIconManager.cs` manages tray controls, and `Models/NotificationItem.cs` represents notification data.

`packaging/` contains the sparse-package manifest, signing and build scripts, and logos under `Assets/`. Treat `bin/`, `obj/`, and generated packages as build outputs. No test project currently exists.

## Build, Test, and Development Commands

Run from the repository root on Windows with the .NET 8 SDK:

- `dotnet build src/WinNotiMirror/WinNotiMirror.csproj` restores dependencies and compiles the application.
- `dotnet run --project src/WinNotiMirror/WinNotiMirror.csproj` launches the tray application.
- `packaging\New-DevCert.ps1` creates and trusts the development certificate; run once in elevated PowerShell.
- `packaging\Build-Package.ps1` builds and signs `packaging/sparse.msix`. It expects Windows SDK tools under version `10.0.22621.0`.

Follow README setup instructions before first launch. Rebuild the sparse package after manifest changes.

## Coding Style & Naming Conventions

Match existing C#: four-space indentation, braces on separate lines, file-scoped namespaces, PascalCase types and members, camelCase locals, and `_camelCase` private fields. Name asynchronous methods with an `Async` suffix. Preserve nullable annotations and keep XAML paired with its code-behind file. No repository formatter or lint configuration is present.

## Testing Guidelines

No automated framework or coverage threshold is configured. Build before submitting changes, then manually verify notification permission handling, banners on every connected display, stacking and dismissal, tray pause/resume, startup toggling, and clean exit. Include mixed-DPI monitors when changing positioning. Record the Windows version and checks performed.

## Commit & Pull Request Guidelines

This checkout has no Git metadata, so historical commit conventions cannot be verified. Use short, imperative subjects describing the change. Keep changes focused. Pull requests should explain behavior changes, link relevant issues, list validation performed, and include screenshots for banner or tray UI changes.

## Security & Configuration

Development `.pfx` and `.cer` files are local signing artifacts; do not distribute them as production credentials. Keep the manifest publisher consistent with the signing certificate, and avoid logging notification contents.
