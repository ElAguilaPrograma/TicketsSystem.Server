# SupportFlow AI

SupportFlow AI is an open-source IT help desk and ticketing system built with ASP.NET Core. It provides a complete backend API for managing support tickets, users, notifications, and real-time communication, with a prepared infrastructure for AI integration via the Model Context Protocol (MCP).

## Features

- **Ticket Management** -- Create, update, assign, accept, close, and reopen support tickets with priority and status tracking.
- **Role-Based Access** -- Three roles: Admin, Agent, and End User, with granular permissions per action.
- **Real-Time Notifications** -- SignalR hub for live updates on new tickets, status changes, and comments.
- **Comment System** -- Internal (agent-only) and external (visible to the ticket owner) comments with real-time delivery.
- **Ticket History** -- Full audit trail of all changes made to each ticket, grouped by change event.
- **File Storage** -- Profile pictures and ticket attachments stored in Supabase Storage with validation (magic byte verification, 25 MB limit).
- **Excel Export** -- Export tickets and users to `.xlsx` format using ClosedXML.
- **JWT Authentication** -- Secure login with HttpOnly cookies and 7-day token expiry.
- **Pagination & Filtering** -- Filter tickets by status, priority, date range, assigned user, and more.
- **AI-Ready Infrastructure** -- Database schema prepared for MCP-based AI model integration (intent classification, auto-reply suggestions, etc.).

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 10.0 | Runtime framework |
| ASP.NET Core | Web API framework |
| Entity Framework Core 10.0 | ORM |
| PostgreSQL | Primary database |
| Npgsql | PostgreSQL provider |
| SignalR / Azure SignalR | Real-time messaging |
| Supabase Storage | File storage |
| FluentValidation | Request validation |
| FluentResults | Result pattern |
| ClosedXML | Excel generation |
| JWT Bearer | Authentication |
| Swashbuckle | API documentation |
| xUnit + Moq + FluentAssertions | Testing |

## Architecture

The solution follows a clean architecture pattern with four main layers:

```
TicketsSystem.slnx
├── TicketsSystem.Domain      -- Entities, enums, repository interfaces
├── TicketsSystem.Data        -- EF Core DbContext, migrations, repositories, Unit of Work
├── TicketsSystem.Core        -- Services, DTOs, mappers, validations, business logic
├── TicketsSystem.Api         -- Controllers, SignalR hubs, middleware, Program.cs
├── TicketsSystem.Tests       -- Unit tests (53)
├── TicketsSystem.Tests.Integration -- Integration tests (10)
└── TicketClientTest          -- Console app for SignalR testing
```

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/) (local or hosted)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for integration tests)
- [Supabase](https://supabase.com/) account (optional, for file storage)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/supportflow-ai.git
cd supportflow-ai/TicketsSystem.Server
```

### 2. Configure the Database

Create a PostgreSQL database and update the connection string in `TicketsSystem.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=supportflow;Username=postgres;Password=your_password"
  }
}
```

The application automatically applies migrations on startup via `dbContext.Database.Migrate()`.

### 3. Configure JWT and Supabase (Optional)

In the same file, configure JWT settings and Supabase credentials:

```json
{
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "SupportFlowAI",
    "Audience": "SupportFlowAI"
  },
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "Key": "your-supabase-key"
  }
}
```

> **Note:** The application will still run without Supabase configured, but file upload features will be unavailable.

### 4. Run the Application

```bash
dotnet run --project TicketsSystem.Api
```

The API will be available at:
- HTTP: `http://localhost:5056`
- HTTPS: `https://localhost:7121`

Swagger UI: `https://localhost:7121/swagger`

### 5. Seed Data

On first startup, the application automatically seeds:
- **Default admin user:** `admin@example.com` / `Admin123!`
- **Ticket statuses:** Open, InProgress, OnHold, Closed, Reopened
- **Ticket priorities:** Low, Medium, High, Critical

## Test Credentials

| Role | Email | Password |
|---|---|---|
| Admin | prueba3@gmail.com | Prueba3 |
| Agent | agente@example.com | Agente123 |
| User | user@example.com | User123 |

## Running Tests

### Unit Tests

```bash
dotnet test TicketsSystem.Tests
```

53 tests covering services (tickets, users, notifications, roles, current user) and validation logic. All dependencies mocked.

### Integration Tests

```bash
dotnet test TicketsSystem.Tests.Integration
```

10 tests running against a real SQL Server container (via Testcontainers) with an in-memory API host. Tests cover full authentication flows, ticket CRUD persistence, and SignalR real-time events.

> **Note:** Integration tests require Docker Desktop to be running.

## API Overview

### Authentication (`/api/authentication`)

- `POST /login` -- Authenticate and receive JWT in HttpOnly cookie
- `POST /logout` -- Clear authentication cookie
- `GET /getcurrentuser` -- Get current user claims
- `POST /createuser` -- Create a new user (Admin)
- `GET /getallusers` -- List users with pagination (Admin)
- `PUT /updateuser/{userId}` -- Update user information (Admin)

### Tickets (`/api/tickets`)

- `POST /createticket` -- Create a new ticket
- `GET /gettickets` -- List tickets with filters and pagination
- `GET /getticketbyid/{ticketId}` -- Get ticket details
- `PUT /updateticketinfo/{ticketId}` -- Update ticket (Admin/Agent)
- `PUT /updateticketuser/{ticketId}` -- Update own ticket (User)
- `POST /assingtickets/{ticketId}/{userId}` -- Assign ticket to agent (Admin)
- `POST /accepttickets/{ticketId}` -- Self-assign ticket (Agent)
- `POST /closetickets/{ticketId}` -- Close ticket (Admin/Agent)
- `POST /reopentickets/{ticketId}` -- Reopen ticket (Admin/Agent)
- `GET /exporttickets` -- Export tickets to Excel

### Comments (`/api/ticketcomments`)

- `POST /createticketcomment/{ticketId}` -- Add a comment (internal or external)
- `GET /getticketscomment/{ticketId}` -- Get all comments for a ticket

### History (`/api/tickethistory`)

- `GET /gettickethistory/{ticketId}` -- Get grouped change history for a ticket

### Notifications (`/api/notifications`)

- `GET /getusernotifications/{userId}` -- Get user notifications
- `PUT /tooglenotificationreadstatus/{notificationId}` -- Mark notification as read

### Real-Time (SignalR)

Connect to `/ticketHub` to receive live events:
- `ReceiveNewTicket` -- A new ticket was created
- `ReceiveNewTicketStatusChange` -- A ticket status changed
- `ReceiveNewTicketComment` -- A new comment was added

## Domain Model

| Entity | Description |
|---|---|
| User | System users with roles (Admin, Agent, User) |
| Ticket | Support tickets with status, priority, and assignment |
| TicketStatus | Lookup table (Open, InProgress, OnHold, Closed, Reopened) |
| TicketPriority | Lookup table (Low, Medium, High, Critical) |
| TicketComment | Comments on tickets (internal or external) |
| TicketAttachment | File attachments on tickets |
| TicketHistory | Audit trail of ticket changes |
| Notification | User notifications triggered by ticket events |
| Mcprequest / Mcpresponse | AI request/response records for future MCP integration |

## Project Configuration

### CORS

Configured to allow `http://localhost:4200` (Angular development) and the production Azure Static Web App URL. Supports credentials for cookie-based authentication.

### SignalR

- In development: In-memory SignalR
- In production: Azure SignalR Service

### File Storage

Files are uploaded to Supabase Storage with validation:
- Maximum file size: 25 MB
- Supported image formats: JPEG, PNG, GIF (validated by content type and magic bytes)

## License

This project is open source. See the LICENSE file for details.
