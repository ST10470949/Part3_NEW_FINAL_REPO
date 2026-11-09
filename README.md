# RaceDay — Part 3: MVC Front End, Azure Blob Storage and Docker

**Module:** PROG6212 — Programming 2B  
**Student:** Abongile Lathitha Manqana  
**Student number:** ST10470949

## What this solution contains

This is the combined RaceDay solution. `RaceDay.API` is the Part 2 REST API, extended for Part 3 image uploads. `RaceDay.MVC` is the Part 3 web interface. `RaceDay.Tests` contains API unit tests. The MVC project calls the API over HTTP; it does **not** connect directly to SQL Server and does not duplicate API database/business logic.

Architecture: `Browser → RaceDay.MVC → RaceDay.API → SQL Server`, with image uploads sent by MVC to the API and the API storing the images in Azure Blob Storage.

## Main features

- Registration, login, logout, session-based sign-in, and Organiser/Participant navigation.
- Organiser dashboard, event create/edit/delete, event banner upload, category management, enrolment viewing, and result entry.
- Participant event browsing and details, enrolment, own enrolments/results, profile editing, and profile picture upload.
- API-side Azure Blob Storage integration; MVC sends the file to the API and displays the URL returned by the API.
- Entity Framework Core SQL Server data access and migrations.
- Dockerfile for the MVC web application and GitHub Actions workflow for restore/build/test.

## Requirements

- .NET 8 SDK and Visual Studio 2022 (or equivalent).
- SQL Server LocalDB for local development, or another SQL Server instance with a matching connection string.
- Docker Desktop for the container step.
- An Azure Storage account to test real image uploads.

## Configure the API database

Check `RaceDay.API/appsettings.json`. The default connection string uses Windows SQL Server LocalDB. Change it if your SQL Server instance differs. Never commit real passwords or Azure connection strings to GitHub. Use user secrets or environment variables for private settings.

The API applies the checked-in EF Core migrations automatically at startup, before demo seeding runs. This is important for a newly created, empty `RaceDay` database: the database must have its tables created before `SeedData` queries `Users`.

From the solution root, run:

```powershell
dotnet restore
dotnet build
dotnet test
```

You can also apply migrations manually from the solution root with `dotnet ef database update --project RaceDay.API/RaceDay.API.csproj`. If the database already contains Part 2 tables or data, back it up and reconcile the schema/migration history before applying the initial migration.

The API's initial migration includes the Part 2 schema plus the Part 3 `BannerImageUrl` and `ProfilePictureUrl` fields. The reference SQL script in `Documentation/Part2/RaceDay_Database.sql` recreates the `RaceDay` database and will delete an existing database with that name; do not run it against a database containing data you need. If your database already contains the Part 2 tables, back it up and reconcile the migration history/schema before applying an initial migration.

## Configure Azure Blob Storage

Set `AzureBlobStorage:ConnectionString` and `AzureBlobStorage:ContainerName` in API user secrets or environment variables. For example, from the solution root:

```powershell
dotnet user-secrets init --project RaceDay.API/RaceDay.API.csproj
dotnet user-secrets set "AzureBlobStorage:ConnectionString" "YOUR_AZURE_STORAGE_CONNECTION_STRING" --project RaceDay.API/RaceDay.API.csproj
dotnet user-secrets set "AzureBlobStorage:ContainerName" "raceday-images" --project RaceDay.API/RaceDay.API.csproj
```

Do not put the real connection string in source control. The Azure integration can only be fully verified with a valid Azure Storage account.

## Run locally

Open `RaceDay.sln` in Visual Studio. For the first run, start the API and MVC separately. This avoids a Visual Studio launch-profile error that can occur when the solution's saved multi-project startup profile is used.

Terminal 1 — start the API (HTTP profile):

```powershell
dotnet run --project RaceDay.API/RaceDay.API.csproj --launch-profile http
```

The API HTTP profile listens on port `5170` (Swagger: `http://localhost:5170/swagger`) and binds to all host interfaces so Docker Desktop can reach it. The MVC app's default API URL is `http://localhost:5170/`. For Docker, the Dockerfile overrides this with `http://host.docker.internal:5170/`.

Terminal 2 — start MVC:

```powershell
dotnet run --project RaceDay.MVC/RaceDay.MVC.csproj --launch-profile http
```

MVC is configured for `http://localhost:5220` (HTTP profile) or `https://localhost:7221` (HTTPS profile). Port `7220` was assigned to MVC's HTTPS profile, not the API; it has been moved to `7221` to avoid the reported conflict. Keep the API terminal running while starting MVC in a second terminal.


## Build and test

```powershell
dotnet restore
dotnet build
dotnet test
```

Record the actual output from your own machine for submission evidence. Do not claim that GitHub Actions passed unless the latest run in the repository shows a green check.

## Docker: build and run MVC against the host API

The included Dockerfile builds the MVC project. For this setup, run the API on the host first and then start the MVC container. This is useful with LocalDB because LocalDB is a Windows-hosted database service and is not part of the MVC Linux container.

From the solution root:

```powershell
docker build -t raceday-mvc -f RaceDay.MVC/Dockerfile .
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5170/ raceday-mvc
```

Then browse to `http://localhost:8080`. Ensure the API is running on host port `5170` and can reach its database. If you have an existing container named `raceday-mvc`, remove it first with `docker rm -f raceday-mvc`.

## GitHub Actions and submission evidence

Workflow: `.github/workflows/dotnet-ci.yml`. It restores, builds, and tests the solution. Check the **Actions** tab for the actual run status and save a screenshot of the successful run under `Documentation/ci-green-build.png`. Add your actual video URL below before submitting.
<img width="1917" height="1016" alt="image" src="https://github.com/user-attachments/assets/dca65426-5eee-4dd0-9c4b-032ffe1b2448" />

**Video presentation:** _Add your submitted Part 3 video URL here._

## Part 2 reference documentation

The Part 2 database SQL, ERD, endpoint plan, and implementation notes are included under `Documentation/Part2/` to show how the Part 3 UI maps to the API and database design.


## Manual actions still required before submission

These tasks require your own Windows/Visual Studio account, Azure subscription, GitHub repository, and video. They cannot be honestly marked complete from a source ZIP alone.

1. **Build and test locally:** start `MSSQLLocalDB`, run `dotnet restore`, `dotnet build`, and `dotnet test`. Fix any errors shown by your local SDK before submission.
2. **Start the API first:** open a terminal at the solution root and run `dotnet run --project RaceDay.API/RaceDay.API.csproj --launch-profile http`. Verify `http://localhost:5170/swagger` opens, then start MVC in a second terminal with `dotnet run --project RaceDay.MVC/RaceDay.MVC.csproj --launch-profile http`. If Visual Studio shows the `SelfHostWebServer` duplicate-key dialog, close it and use these separate terminal commands rather than the saved solution multi-start profile.
3. **Test all features with real data:** register and log in as both Organiser and Participant; create/edit/delete an event; add a category; enrol; view enrolments; capture results; verify profile updates and logout/protected-page redirects.
4. **Azure setup:** create/choose an Azure Storage account and configure `AzureBlobStorage:ConnectionString` as a user secret/environment variable. Test event banner and profile picture upload and verify that returned blob URLs are stored and displayed. Never commit the connection string.
5. **Docker:** keep the API running on the host's HTTP profile (`http://localhost:5170`) for the included Docker command. Build with `docker build -t raceday-mvc -f RaceDay.MVC/Dockerfile .`, then run `docker run --rm -p 8080:8080 raceday-mvc`. The Dockerfile supplies `ApiSettings__BaseUrl=http://host.docker.internal:5170/`. Test the website at `http://localhost:8080`. LocalDB remains on the host; the API must be reachable from the container.
6. **GitHub:** push the final work to your Part 3 branch; make at least 20 meaningful commits over your actual development work (do not create fake commits just to satisfy the count); verify the latest GitHub Actions workflow is green. Save a genuine screenshot of the green build.
7. **Video and ARC:** record a clear demo of the app, role-specific pages, API-backed operations, Azure upload, and Docker. Add the actual video URL to this README and submit the GitHub link/video/evidence on ARC as your lecturer requires.
8. **Security review:** use a strong local secret for Azure, do not publish real credentials, and confirm the role checks are enforced by the API—not only hidden navigation links.

## Local troubleshooting

- If MVC reports that the API connection was refused, run `Test-NetConnection localhost -Port 5170` while the API is running. `TcpTestSucceeded` must be `True`.
- If the port is false, the API is stopped or listening on a different profile/port. Check the API Output window for its `Now listening on:` line.
- If the API starts but reports a database error, check the connection string and migrations before starting MVC. The MVC application must not connect directly to SQL Server.
- If Azure upload fails while browsing events works, configure the Azure Storage connection string in the API and retry the upload.

## Docker Evidence
<img width="1917" height="1013" alt="image" src="https://github.com/user-attachments/assets/bc048ffb-6efa-4bc0-b2b5-8fa5cf6f617e" /> 
raceday-mvc

## Azure Blob Evidence
### Link: https://portal.azure.com/#home
<img width="1917" height="915" alt="image" src="https://github.com/user-attachments/assets/31bfe7bf-c6b4-4718-9232-e1fe585e945c" />
<img width="1912" height="923" alt="image" src="https://github.com/user-attachments/assets/dc765ddf-32fa-43e1-957a-7771a9784442" />
