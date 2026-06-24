# ConnectHub API

A social media REST API built with ASP.NET Core 8. Users can follow each other, create posts, like and comment, get a personalized feed, and receive real-time notifications — all backed by a clean layered architecture.

I built this after finishing a .NET course to apply everything in a real project rather than just tutorials. It took longer than expected but I learned a lot from the mistakes along the way.

---

## Tech Stack

- **ASP.NET Core 8** — Web API
- **Entity Framework Core** + SQL Server
- **JWT** + Refresh Tokens
- **SignalR** — real-time WebSocket notifications
- **Redis** (StackExchange.Redis) — caching
- **Serilog** — structured logging
- **SkiaSharp** — image processing for avatars
- **Clean Architecture** — 4-layer project structure

---

## Architecture

The project follows Clean Architecture with a strict dependency rule — outer layers depend on inner layers, never the reverse.

```
ConnectHub.API           → controllers, middleware, SignalR hubs
ConnectHub.Application   → interfaces, DTOs, specifications, settings
ConnectHub.Domain        → entities, enums (no dependencies)
ConnectHub.Infrastructure → EF Core, repositories, services, Redis
```

### Patterns used

**Generic Repository + Unit of Work** — one repository handles all entities via `DbContext.Set<T>()`. All changes commit in a single `SaveChangesAsync()` call, which keeps transactions atomic.

**Specification Pattern** — query logic (WHERE, INCLUDE, ORDER BY, pagination) lives in dedicated specification classes instead of scattered across repositories. Makes queries reusable and testable.

```csharp
// Instead of raw LINQ in the repository
var spec = new PostsByUserSpecification(userId, pageNumber, pageSize);
var posts = await _unitOfWork.Posts.ListBySpecAsync(spec);
```

---

## Features

**Authentication**
- Register and login with BCrypt password hashing
- JWT access tokens (15 min expiry) + refresh tokens (7 days)
- Refresh token rotation on every use

**Users**
- Public profile with follower/following/post counts
- Update bio and username
- Upload and resize profile picture (256×256, SkiaSharp)

**Posts**
- Create, read, update, delete
- Paginated list with author info and like/comment counts
- Only the post owner can edit or delete

**Follow System**
- Follow and unfollow users
- List followers and following
- Follow stats with `isFollowing` flag for the current user

**Feed**
- Paginated posts from followed users only
- Ordered by most recent
- Uses `WHERE UserId IN (followingIds)` — fetched in two steps

**Likes and Comments**
- Like and unlike posts
- Add comments with notification to post owner
- No self-notification on your own posts

**Notifications**
- Auto-created on follow, like, and comment events
- Mark one or all as read
- Paginated list

**Real-time (SignalR)**
- Each user joins a private group `user_{id}` on connect
- Notifications pushed instantly via WebSocket
- JWT passed as query string (WebSockets can't use headers)
- Circular dependency resolved via `INotificationHubContext` marker interface in Application layer

**Caching (Redis)**
- User profiles cached for 10 minutes
- Posts cached for 5 minutes
- Feed cached for 2 minutes per page
- Cache invalidated on every write

**Logging + Error Handling**
- Serilog with daily rolling log files under `logs/`
- `GlobalExceptionMiddleware` catches all unhandled exceptions
- Standardized JSON error response: `{ statusCode, message, path }`
- EF Core SQL logging suppressed to reduce noise

---

## API Endpoints

### Auth
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
```

### Users
```
GET    /api/users/{id}
PUT    /api/users/me
POST   /api/users/me/avatar
```

### Posts
```
POST   /api/posts
GET    /api/posts
GET    /api/posts/{id}
GET    /api/posts/user/{userId}
PUT    /api/posts/{id}
DELETE /api/posts/{id}
POST   /api/posts/{id}/like
DELETE /api/posts/{id}/like
POST   /api/posts/{id}/comment
```

### Follow
```
POST   /api/follow/{userId}
DELETE /api/follow/{userId}
GET    /api/follow/{userId}/followers
GET    /api/follow/{userId}/following
GET    /api/follow/{userId}/stats
```

### Feed
```
GET /api/feed
```

### Notifications
```
GET /api/notifications
PUT /api/notifications/{id}/read
PUT /api/notifications/read-all
```

### SignalR
```
WS /hubs/notifications?access_token={jwt}
```
Client listens for: `ReceiveNotification`

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (or SQL Server Express)
- Redis running on `localhost:6379`

### Run locally

```bash
git clone https://github.com/your-username/ConnectHub.git
cd ConnectHub
```

Update `appsettings.json` with your connection strings:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ConnectHubDb;Trusted_Connection=True;TrustServerCertificate=True",
  "Redis": "localhost:6379"
},
"JwtSettings": {
  "SecretKey": "your-secret-key-min-32-chars",
  "Issuer": "ConnectHubAPI",
  "Audience": "ConnectHubClient",
  "ExpiryMinutes": 15,
  "RefreshTokenExpiryDays": 7
}
```

Apply migrations and run:

```bash
dotnet ef database update --project ConnectHub.Infrastructure --startup-project ConnectHub.API
dotnet run --project ConnectHub.API
```

Swagger UI: `https://localhost:{port}/swagger`

SignalR test page: `https://localhost:{port}/test.html`

---

## Project Structure

```
ConnectHub.sln
├── ConnectHub.API
│   ├── Controllers
│   ├── Hubs
│   ├── Middleware
│   └── wwwroot
├── ConnectHub.Application
│   ├── Constants
│   ├── DTOs
│   ├── Interfaces
│   ├── Settings
│   └── Specifications
├── ConnectHub.Domain
│   └── Entities
└── ConnectHub.Infrastructure
    ├── Persistence
    ├── Repositories
    └── Services
```

---

## What I learned building this

A few things I didn't fully understand from tutorials that became clear writing real code:

- The Specification Pattern feels like overkill until your queries get complex — then you wish you had it earlier
- SignalR JWT auth is different from REST (query string, not headers) and took longer to figure out than expected
- `IDistributedCache` doesn't support pattern-based key deletion — switched to `IConnectionMultiplexer` directly
- Global exception middleware only works if you remove try/catch from controllers — took a debugging session to realise
- Clean Architecture's strict dependency rules actually prevent a class of bugs rather than just being a style preference

---

## License

MIT
