# Room Booking API

REST API for booking conference rooms. .NET 10, ASP.NET Core Web API, EF Core, SQLite.

## How to run

```
cd RoomsAPI
dotnet run
```

The API runs on `http://localhost:5243`. The SQLite database (`roombooking.db`) is created on first start with three sample rooms. Delete the file to reset the data.

`RoomsAPI/RoomsAPI.http` has ready-made requests, including the overlap examples from the task. Open it in Visual Studio and click "Send request".

## How to test

```
dotnet test
```

Run from the repository root.

- `RoomsAPI.Tests/Controllers` - integration tests. The whole API runs in memory with its own temporary SQLite file per test, and the tests send real HTTP requests. This is on purpose: the overlap rule is a SQL query, so it has to be tested against a real database.
- `RoomsAPI.Tests/Dtos`, `Binding` - unit tests for validation and date parsing.

## API

| Method | URL | |
|---|---|---|
| GET | `/api/rooms` | All rooms |
| GET | `/api/rooms/{roomId}/reservations?from=&to=&page=&pageSize=` | Reservations of a room, ordered by start. `from`/`to` are optional |
| POST | `/api/rooms/{roomId}/reservations` | Create a reservation |

```json
{ "start": "2030-10-01T10:00:00Z", "end": "2030-10-01T11:00:00Z", "title": "Team sync" }
```

Responses: `201` created, `400` validation error, `404` unknown room, `409` overlaps with an existing reservation. Errors are returned as ProblemDetails.

## Details

**Overlaps.** Two reservations overlap when `existing.Start < new.End && new.Start < existing.End`. `End` is exclusive, so 10:00-11:00 and 11:00-12:00 don't overlap. A conflict returns `409` with the reservation that is in the way.

**Parallel requests.** The overlap check and the insert run in one transaction. On SQLite that takes the write lock before the check, so two requests for the same slot can't both succeed. There is a test that sends 200 requests at once.

**Performance.** Reservations have an index on `(RoomId, Start, End)`, which is used by both the overlap check and the list query. The list is paged (default 50, max 100, configurable in `appsettings.json`) so a room with a long history doesn't return everything at once. Rooms aren't paged - there are only a few of them and the number doesn't grow with usage.

**Dates.** All dates must include an offset (`Z` or `+02:00`); a date without one is rejected, otherwise it would be read as the server's local time. Everything is stored in UTC. In a query string, `+` has to be sent as `%2B`.

**Extra validation.** Besides the task requirements: `start` can't be in the past and a reservation can't be longer than 24 hours, so a single request can't block a room forever.

**Logging.** Every request is logged with its status code and duration. For rejected requests the reason is logged too (validation errors, conflicts). Titles and request bodies aren't logged.

## What's missing

- **Authentication** - the API is open. I'd add JWT bearer authentication and store who made each reservation.
- **Cancel / update reservations** - not required by the task, but needed in practice. Update would use the same overlap check, excluding the reservation itself.
- **Migrations** - they are applied on startup to keep running the app simple. In production I'd run them from the deployment pipeline.
- **Response DTOs** - entities are returned directly because they are flat and simple. In a bigger API I'd add response models so the database schema isn't the API contract.
- **HTTPS, rate limiting, health checks** - usually handled by the hosting environment or added when deploying.
