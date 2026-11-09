# Part 2 API ↔ Part 3 MVC integration checklist

1. Run `dotnet restore`, `dotnet build`, and `dotnet test` from the solution root.
2. Configure the API database and apply the EF Core migration to a clean database. If reusing an existing Part 2 database, back it up and reconcile its migration history before applying schema changes.
3. Start the API at `http://localhost:5170` and verify `/swagger`.
4. Start MVC at `http://localhost:5220`; confirm it reads `ApiSettings:BaseUrl` from configuration.
5. Register/login as both an Organiser and Participant. Confirm each gets the correct navigation and logout clears the MVC/API session.
6. Test organiser event/category management, enrolment viewing, and result entry.
7. Test participant event browsing, enrolment, own results, and profile update.
8. Configure a real Azure Storage connection string through user secrets or environment variables and test both event-banner and profile-picture uploads. Confirm the API saves each returned blob URL and MVC displays the image.
9. Build and run the MVC Docker image while the API is reachable at `http://host.docker.internal:5170/`.
10. Check the GitHub Actions result and include genuine screenshot/video evidence.
