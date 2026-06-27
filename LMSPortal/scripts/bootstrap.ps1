Set-StrictMode -Version Latest
Set-Location $PSScriptRoot

# Top-level solution
dotnet new sln -n LMSPortal

# Create projects
Set-Location (Join-Path $PSScriptRoot "src")
dotnet new webapi -n LMS.API
dotnet new classlib -n LMS.Application
dotnet new classlib -n LMS.Domain
dotnet new classlib -n LMS.Infrastructure
dotnet new classlib -n LMS.Contracts
dotnet new classlib -n LMS.Shared

# Create test projects
Set-Location (Join-Path $PSScriptRoot "..\tests")
dotnet new xunit -n LMS.UnitTests
dotnet new xunit -n LMS.IntegrationTests
dotnet new xunit -n LMS.PerformanceTests

# Add projects to solution (assumes script run from LMSPortal root)
Set-Location $PSScriptRoot
dotnet sln add src/LMS.API/LMS.API.csproj
dotnet sln add src/LMS.Application/LMS.Application.csproj
dotnet sln add src/LMS.Domain/LMS.Domain.csproj
dotnet sln add src/LMS.Infrastructure/LMS.Infrastructure.csproj
dotnet sln add src/LMS.Contracts/LMS.Contracts.csproj
dotnet sln add src/LMS.Shared/LMS.Shared.csproj
dotnet sln add tests/LMS.UnitTests/LMS.UnitTests.csproj
dotnet sln add tests/LMS.IntegrationTests/LMS.IntegrationTests.csproj
dotnet sln add tests/LMS.PerformanceTests/LMS.PerformanceTests.csproj

# Add project references
dotnet add src/LMS.API/LMS.API.csproj reference src/LMS.Application/LMS.Application.csproj
dotnet add src/LMS.API/LMS.API.csproj reference src/LMS.Infrastructure/LMS.Infrastructure.csproj
dotnet add src/LMS.API/LMS.API.csproj reference src/LMS.Contracts/LMS.Contracts.csproj
dotnet add src/LMS.API/LMS.API.csproj reference src/LMS.Shared/LMS.Shared.csproj
dotnet add src/LMS.Application/LMS.Application.csproj reference src/LMS.Domain/LMS.Domain.csproj
dotnet add src/LMS.Application/LMS.Application.csproj reference src/LMS.Contracts/LMS.Contracts.csproj
dotnet add src/LMS.Application/LMS.Application.csproj reference src/LMS.Shared/LMS.Shared.csproj
dotnet add src/LMS.Infrastructure/LMS.Infrastructure.csproj reference src/LMS.Application/LMS.Application.csproj
dotnet add src/LMS.Infrastructure/LMS.Infrastructure.csproj reference src/LMS.Domain/LMS.Domain.csproj
dotnet add src/LMS.Infrastructure/LMS.Infrastructure.csproj reference src/LMS.Shared/LMS.Shared.csproj

Write-Host "Bootstrap completed. Run 'dotnet restore' if needed." -ForegroundColor Green
