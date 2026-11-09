# Part 2 Implementation Notes

This file records the deliberate reconciliation between Part 1, the Part 2 guide and the implementation.

## 1. Authentication

Part 1's endpoint plan describes JWT in the login response. The official Part 2 guide requires server-side session management. Part 2 therefore uses ASP.NET Core Session and stores `UserId`, `Role` and `FullName` in the session.

## 2. Profile routes

Part 1 defines:

- `GET /api/users/me`
- `PUT /api/users/me`

The Part 2 guide shows `/api/profile/me`. The implementation keeps the Part 1 routes and adds `/api/profile/me` as a compatibility alias. Both routes use the same own-profile logic.

## 3. Event ownership

Organiser-only Event, Category, enrolment-management, Result and weather operations verify that the logged-in Organiser owns the parent Event.

## 4. Database consistency

The Part 1 ERD/SQL did not originally show `DistanceKm` and `EventType`, although the Part 2 guide requires them for Events and the Part 2 model already contained them. The updated ERD and SQL now include both fields.

`RouteWeatherInfo.EventID` is now explicitly unique so an Event has zero or one current weather/route record, matching the API's update behaviour.

## 5. EF Core migration

An initial EF Core migration is included under `RaceDay.API/Migrations` so the database can be created with:

```powershell
dotnet ef database update --project RaceDay.API
```

## 6. Automated tests

The test suite was expanded to cover both successful and failure scenarios across authentication, profiles, events, categories, enrolments, results and weather.
