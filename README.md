# ChatApp Backend

Backend for a real-time messaging application built on ASP.NET Core 8. It supports JWT-based authentication, real-time communication via SignalR, media sharing, and a "status" (story) feature.

## Features

- **Authentication & Authorization** — ASP.NET Core Identity + JWT Bearer tokens, registration/login with email confirmation
- **Real-time messaging** — instant message delivery, "typing..." notifications, and call ring notifications via SignalR (`/hubs/chat`)
- **Message management** — sending, editing, (soft) deleting, replying, reactions, and read receipts
- **Media support** — uploading and sending audio, video, and files
- **Online status tracking** — real-time tracking of users' online/offline status
- **Status (Story) sharing** — uploading and viewing temporary image/video statuses
- **User profile** — updating profile information and avatar
- **Swagger / OpenAPI** — interactive API documentation in the development environment

## Tech Stack

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8.0 (Web API) |
| Database | SQL Server + Entity Framework Core 8 |
| Authentication | ASP.NET Core Identity, JWT Bearer |
| Real-time | SignalR |
| API Documentation | Swashbuckle (Swagger) |

## Project Structure

```
chatApp/
├── Controllers/       # API endpoints (Auth, Messages, Status, User)
├── Hubs/               # SignalR hubs (MessageHub)
├── Models/             # Entities and DTOs
├── Data/               # ApplicationDbContext (EF Core)
├── Services/           # Business logic (Auth, Message, Status, Image, Email)
├── Interfaces/         # Service interfaces
├── Repositories/       # Data-access layer
├── Migrations/         # EF Core migrations
└── wwwroot/            # Static files (audio, video, images, uploads)
```

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express, or full version)

## Setup

1. Clone the repository:
   ```bash
   git clone <repo-url>
   cd chatAppBack
   ```

2. Configure the following values in `chatApp/appsettings.json` for your environment:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=<server>;Initial Catalog=chatapp;Integrated Security=True;..."
     },
     "Jwt": {
       "SecretKey": "<a strong key of at least 32 characters>",
       "Issuer": "chatApp",
       "Audience": "chatApp"
     },
     "Domain": "http://localhost:5051"
   }
   ```
   > ⚠️ **Note:** Keep sensitive values such as `SecretKey` and the database connection string private — in production, use [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables instead of committing them to `appsettings.json`.

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Apply database migrations:
   ```bash
   dotnet ef database update --project chatApp
   ```

5. Run the application:
   ```bash
   dotnet run --project chatApp
   ```

6. Open Swagger UI (in the development environment): `https://localhost:<port>/swagger`

## Configuration

CORS is open to the `http://localhost:5173` origin for frontend integration (see the `_myAllowSpecificOrigins` policy in `Program.cs`). Update this value if your frontend runs on a different address.

For SignalR, the JWT token is passed via the `access_token` query parameter (for the `/hubs/chat` endpoint).

## API Endpoints

### Auth (`/api/auth`)
| Method | Endpoint | Description |
|---|---|---|
| POST | `/register` | Register a new user |
| GET | `/confirm-email` | Confirm email address |
| POST | `/login` | Log in and obtain a JWT token |
| POST | `/logout` | Log out |

### Messages (`/api/messages`) — requires `Authorize`
| Method | Endpoint | Description |
|---|---|---|
| POST | `/send` | Send a message (with reply support) |
| GET | `/with/{userId}` | Get messages exchanged with a user |
| DELETE | `/{id}` | (Soft) delete a message |
| GET | `/is-online/{userId}` | Check a user's online status |
| POST | `/upload-audio` | Upload a voice message |
| POST | `/upload-video` | Upload a video |
| POST | `/upload-file` | Upload a file |
| POST | `/react` | Add a reaction to a message |
| PUT | `/{id}` | Edit a message |
| POST | `/mark-as-read/{id}` | Mark a message as read |

### Status (`/api/status`) — requires `Authorize`
| Method | Endpoint | Description |
|---|---|---|
| POST | `/add` | Upload a new status (image/video) |
| GET | `/` | Get your own statuses |
| GET | `/by-email/{email}` | Get a user's statuses by email |
| DELETE | `/{id}` | Delete a status |

### User (`/api/user`)
| Method | Endpoint | Description |
|---|---|---|
| PUT | `/update-by-email` | Update user profile (including avatar) |
| GET | `/all` | Get the list of all users |

### SignalR Hub (`/hubs/chat`)
| Method | Description |
|---|---|
| `SendMessageToUser` | Send a message in real time |
| `SendTypingNotification` | "Typing..." notification |
| `SendRing` | Call ring notification |
| `NotifyEditedMessage` | Notify about an edited message |
| `MarkMessageAsRead` | Mark a message as read in real time |
| `UserConnected` / `UserDisconnected` | Online status events |

## Database Migrations

To create a new migration:
```bash
dotnet ef migrations add <MigrationName> --project chatApp
dotnet ef database update --project chatApp
```

## License

This project is intended for internal use.
