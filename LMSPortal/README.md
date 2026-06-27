# LMSPortal

This repository contains a Clean Architecture starter for an ASP.NET Core (.NET 8) LMS.

To scaffold the solution and projects, run the bootstrap script from PowerShell in the `LMSPortal` folder:

```powershell
Set-Location "${PWD}"
cd "LMSPortal"
.
\scripts\bootstrap.ps1
```

After the script finishes, open the solution `LMSPortal.sln` in Visual Studio or run `dotnet build`.

Next steps:
- Implement `Auth` feature in `src/LMS.Application/Features/Auth`
- Add JWT and refresh token services in `LMS.Infrastructure` and wire them in `LMS.API`.
