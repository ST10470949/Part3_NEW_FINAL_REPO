# RaceDay Part 3 Review Notes

This source package has been reviewed against the supplied Part 3 rubric and updated to improve alignment with the Part 2 API.

## Source changes made

- Added `GET /api/events/mine` so the API returns only the logged-in Organiser's own events. The MVC app now calls this API endpoint instead of downloading all public events and filtering them locally.
- Added MVC/API integration for editing and deleting event categories, alongside category creation.
- Fixed success/error messaging so an image-upload failure is not overwritten by a success message after event creation or update.
- Tightened banner/profile-picture upload validation to check both the content type and file extension.
- Added model validation to the API category update action.
- Changed the MVC default API URL to `http://localhost:5170/`, matching the API's HTTP launch profile.
- Bound the API HTTP profile to all interfaces on port `5170` so a Docker Desktop container can reach the host API.
- Changed the MVC HTTPS development port from `7220` to `7221`; `7220` belongs to the MVC app, not the API.
- Set the MVC Docker image's default API URL to `http://host.docker.internal:5170/`.
- Documented separate API/MVC terminal startup as a workaround for Visual Studio's `SelfHostWebServer` duplicate-key launch error.
- Added these review notes so the remaining external evidence is clear.

## Must still be verified in your environment

- The full solution build succeeded with .NET SDK 10.0.401 using `dotnet build RaceDay.sln --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false`. Tests were not run.
- Start the API with the HTTP profile on port `5170`, then rebuild and run the MVC Docker image.
- Configure real Azure Blob Storage credentials outside source control and test actual uploads. The ZIP intentionally contains an empty connection-string setting.
- Confirm the latest GitHub Actions run is green and add a genuine screenshot as `Documentation/ci-green-build.png`.
- Add the actual unlisted Part 3 video URL to `README.md`.
- Verify the GitHub commit history separately; it is not included in this ZIP.

## Docker commands (PowerShell)

Run the API using the `http` launch profile first. From the repository root, rebuild the MVC image:

```powershell
docker build -t raceday-mvc -f RaceDay.MVC/Dockerfile .
```

If a container with this name already exists, remove it:

```powershell
docker rm -f raceday-mvc
```

Run the MVC container:

```powershell
docker run -d -p 8080:8080 --name raceday-mvc raceday-mvc
```

Then open `http://localhost:8080`.

The API and database must be running and reachable. The MVC Docker image does not contain the API or SQL Server.
