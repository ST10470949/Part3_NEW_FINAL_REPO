# RaceDay Part 3 — Manual Submission Checklist

Use this with `PROG6212_Part_3_Detailed_Student_Guide.pdf` and the official Part 3 brief. Tick each item only after personally verifying it.

## Correct submission (5)
- [ ] Solution, API, MVC, tests, documentation, Dockerfile and README are present.
- [ ] README contains the Docker build/run commands and actual video URL.
- [ ] Genuine screenshot of a successful CI run is saved under `Documentation/ci-green-build.png`.

## GitHub and CI/CD (10)
- [ ] Push Part 3 to a clearly named branch.
- [ ] At least 20 meaningful commits exist.
- [ ] Latest GitHub Actions run builds API, MVC and tests successfully.

## Authentication and role navigation (10)
- [ ] Register and login as Organiser and Participant.
- [ ] Verify session stores user ID, role and display name.
- [ ] Verify each role sees its correct navigation.
- [ ] Verify protected pages redirect unauthenticated users and wrong roles cannot use protected API actions.
- [ ] Verify logout clears MVC and API session state.

## Organiser interface (15)
- [ ] Dashboard counts own events and enrolments and shows upcoming dates.
- [ ] Create, edit and delete events.
- [ ] Add/manage event categories.
- [ ] View participants, selected category and enrolment status.
- [ ] Enter and publish finish times and positions for enrolled participants.

## Participant interface (15)
- [ ] Browse and filter upcoming events.
- [ ] View event detail and categories.
- [ ] Enrol and verify the selected category is recorded.
- [ ] View own enrolments and statuses.
- [ ] View personal results including event, date, category, time and position.
- [ ] Update profile and upload profile picture.

## Azure Blob Storage (10)
- [ ] Configure Azure Storage secrets on the API only.
- [ ] Upload an event banner through MVC → API → Azure.
- [ ] Upload a profile picture through MVC → API → Azure.
- [ ] Verify URLs are saved and images display in MVC.

## Docker (10)
- [ ] Docker Desktop is running.
- [ ] Build the image from the solution root.
- [ ] Run MVC in a container and access `http://localhost:8080`.
- [ ] Verify the container reaches the host API using `host.docker.internal`.
- [ ] Test at least one real API-backed page while containerised.

## UI design (10)
- [ ] Test desktop and mobile widths.
- [ ] Confirm consistent RaceDay colours, spacing, typography and navigation.
- [ ] Confirm Pending/Confirmed/Cancelled statuses are visually distinct.
- [ ] Invalid form data and API downtime show friendly messages rather than raw exceptions.

## Video presentation (15)
- [ ] Explain the MVC → API → SQL/Azure architecture.
- [ ] Demonstrate both roles and key workflows.
- [ ] Explain the API-side Blob Storage integration.
- [ ] Show Docker build/run and the containerised site.
- [ ] Add the actual video URL to README and submit it on ARC.

**Important:** This checklist is not proof that features passed testing. Complete each check using your own running application and preserve genuine screenshots/output as evidence.
