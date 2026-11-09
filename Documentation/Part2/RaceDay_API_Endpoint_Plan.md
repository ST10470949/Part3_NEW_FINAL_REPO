# RaceDay — API Endpoint Plan

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Registers a new user as either an Organiser or a Participant. | None (public) | `{ fullName, email, password, role }` | 201 Created – user created<br>400 Bad Request – validation error<br>409 Conflict – email already registered |
| POST | /api/auth/login | Authenticates a user and returns a JWT access token. | None (public) | `{ email, password }` | 200 OK – token + user info<br>401 Unauthorized – invalid credentials |
| GET | /api/users/me | Returns the logged-in user's own profile. | Any | None | 200 OK – user profile<br>401 Unauthorized |
| PUT | /api/users/me | Updates the logged-in user's own profile details. | Any | `{ fullName, email }` | 200 OK – updated profile<br>400 Bad Request |
| GET | /api/events | Lists all upcoming events, with optional filters (date, location). | None (public) | None | 200 OK – array of events |
| GET | /api/events/{id} | Returns full details for a single event, including its categories. | None (public) | None | 200 OK – event details<br>404 Not Found |
| POST | /api/events | Creates a new event owned by the logged-in Organiser. | Organiser | `{ name, eventDate, location, description }` | 201 Created – new event<br>400 Bad Request |
| PUT | /api/events/{id} | Updates an event owned by the logged-in Organiser. | Organiser | `{ name, eventDate, location, description }` | 200 OK – updated event<br>403 Forbidden – not the owner<br>404 Not Found |
| DELETE | /api/events/{id} | Deletes an event owned by the logged-in Organiser. | Organiser | None | 204 No Content<br>403 Forbidden<br>404 Not Found |
| GET | /api/events/{id}/categories | Lists all categories for a specific event. | None (public) | None | 200 OK – array of categories<br>404 Not Found |
| POST | /api/events/{id}/categories | Adds a new category (e.g. 10km, 21km) to an event. | Organiser | `{ name, entryFee, maxParticipants }` | 201 Created – new category<br>404 Not Found – event doesn't exist |
| PUT | /api/categories/{id} | Updates an existing category. | Organiser | `{ name, entryFee, maxParticipants }` | 200 OK – updated category<br>403 Forbidden<br>404 Not Found |
| DELETE | /api/categories/{id} | Removes a category from an event. | Organiser | None | 204 No Content<br>403 Forbidden<br>404 Not Found |
| POST | /api/enrolments | Enrols the logged-in Participant into a category. | Participant | `{ categoryId }` | 201 Created – enrolment record<br>404 Not Found – category doesn't exist<br>409 Conflict – already enrolled / category full |
| GET | /api/enrolments/me | Lists all of the logged-in Participant's own enrolments. | Participant | None | 200 OK – array of enrolments |
| DELETE | /api/enrolments/{id} | Cancels the logged-in Participant's own enrolment. | Participant | None | 204 No Content<br>403 Forbidden – not the owner<br>404 Not Found |
| GET | /api/events/{id}/enrolments | Lists all participants enrolled in a specific event (all categories). | Organiser | None | 200 OK – array of enrolments<br>403 Forbidden – not the event owner |
| POST | /api/results | Captures a result for a completed enrolment. | Organiser | `{ enrolmentId, finishTime, position }` | 201 Created – result record<br>404 Not Found – enrolment doesn't exist<br>409 Conflict – result already captured |
| GET | /api/results/me | Lists the logged-in Participant's own historical results. | Participant | None | 200 OK – array of results |
| GET | /api/events/{id}/results | Lists all captured results for a specific event. | Organiser | None | 200 OK – array of results<br>403 Forbidden – not the event owner |
| GET | /api/events/{id}/weather | Returns the latest weather/route info for race-day prep. | Any | None | 200 OK – weather + route info<br>404 Not Found |
| POST | /api/events/{id}/weather | Adds/updates weather and route info for an event. | Organiser | `{ weatherForecast, routeMapUrl }` | 201 Created / 200 OK<br>403 Forbidden – not the event owner |

**Notes on role enforcement:** every Organiser-only route must check that the authenticated user's `Role` claim is `Organiser` **and**, where relevant (events, categories, results), that they own the parent Event before allowing writes. Participant-only routes must check the `Role` claim is `Participant`. This must be implemented at the API level in Part 2 (e.g. via `[Authorize(Roles = "Organiser")]` in ASP.NET Core, plus an ownership check in the handler).
