# AiChat — Backend Documentation

The backend is an **ASP.NET Core** web API (C#) that serves the Angular chat client. It exposes REST controllers, a SignalR hub for streaming AI responses, and talks to **Ollama** (the AI backend) and a **SQLite** database (EF Core).

---

## 1. Solution Structure

Solution file: `Backend/AiChat.sln`

| Project | Responsibility |
|---|---|
| `AiChat.Domain` | Core entities + value objects. No external dependencies. |
| `AiChat.Application` | Use cases (CQRS commands/queries) + interfaces (`Abstractions`). |
| `AiChat.Infrastructure` | Implementations: EF Core persistence, Ollama AI, password hashing. |
| `AiChat.Api` | The web host: controllers, SignalR hub, DI wiring, services. |
| `Shared` | Generic `Result`/`Error` result-pattern types. |
| `AiChat.Contracts` | Shared DTOs/contracts (currently minimal/empty). |

This is a **clean architecture** layout: dependencies point inward (`Api → Application → Domain`, with `Infrastructure` implementing `Application`'s interfaces).

---

## 2. Domain Layer (`AiChat.Domain`)

### Entities

All entities use **private constructors + static factory methods** and expose setters only through domain methods (immutable-style aggregate design).

- **`User`** — `Id`, `UserName`, `PasswordHash`, `ExternalId`, `AuthProvider`, `DisplayName`, `IsActive`, `CreatedAt`, `Roles` (list of strings), `RefreshTokens`.
  - `CreateUser(userName, passwordHash, displayName, externalId, authProvider)`
  - `SetIsActive(bool)`, `ChangePassword(hash)`, `SetDisplayName(name)`
  - `ExternalId` + `AuthProvider` distinguish **Local** users (have a `PasswordHash`) from **Active Directory** users (empty hash, matched by `ExternalId`).
- **`Conversation`** — `Id`, `UserId`, `UserName`, `Title`, `CreatedAt`, `UpdatedAt`, `Messages` (read-only collection).
  - `CreateConversation(userId, userName, title)`
  - `AddMessage(content, aiModel, role)`, `Rename(title)`
- **`Message`** — `Id`, `Role`, `Content`, `CreatedAt`, `ConversationId`, `Model`.
  - `CreateMessage(conversationId, role, content, aiModel)`
- **`RefreshToken`** — `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `CreatedAt`, `RevokedAt`, `ReplacedByTokenHash`, with `IsExpired`/`IsActive` helpers.

### Value Objects

- **`MessageRole`** — enum: `User`, `Assistant`, `System`.
- **`ConversationTitle`** — currently an empty placeholder class.

---

## 3. Application Layer (`AiChat.Application`)

Organized **CQRS-style**: each feature has `Commands/` and `Queries/`, each containing a request record + a handler class. Handlers are plain classes registered in DI (no MediatR).

### Features

**Conversations**
- `CreateConversation` — creates a new "New Chat" conversation.
- `SendMessage` — the core chat operation (see §7 streaming flow).
- `DeleteConversation` — deletes if owned by the requesting user.
- `RenameConversation` — renames a conversation.
- `GetConversationList` — lists the user's conversations (with last message).
- `GetConversation` — returns a single conversation's history (last 20 messages).

**Users**
- `CreateUser` — creates a Local user (BCrypt-hashed password, uniqueness check).
- `UpdateUser` — updates display name, active flag, optional password change.
- `DeleteUser` — **soft delete** via `SetIsActive(false)`.
- `GetAllUsers` — lists all users **except** `admin`/`administrator`.

**Authentications**
- `Login` — the login orchestration (see §6).
- `GetCurrentUser` — resolves the current user from JWT claims.

### Abstractions (interfaces)

`IAiProvider`, `IAiStreamingProvider`, `IConversationTitleGenerator`, `IConversationRepository`, `IUserRepository`, `IRefreshTokenRepository`, `ITokenService`, `IPasswordHasher`, `ICurrentUserService`, `IUserResolver`, `IActiveDirectoryAuthService`, `IAiHealthService`, `IChatCancellationTracker`, `IChatStreamNotifier`.

### Result pattern

`Shared` provides `Result<T>` / `Result` / `Error` used by the **Users** handlers (success/failure with error codes like `NotFound`, `BadRequest`).

### DependencyInjection.cs

Registers all handlers as scoped services (`AddApplicationHandler()`), called from `Program.cs`.

---

## 4. Infrastructure Layer (`AiChat.Infrastructure`)

### Persistence (`Persistence/`)

- **`ChatDbContext`** — EF Core context. `DbSet`s: `Users`, `Conversations`, `Messages`, `RefreshTokens`. `RefreshToken` is configured inline (unique `TokenHash` index, cascade delete); other entities use `IEntityTypeConfiguration` classes under `Configurations/`.
- **`DbSeeder`** — seeds a default `admin` user (password `123456`, BCrypt-hashed) if the DB is empty. The provider matches the configured `Authentication:Mode`.
- **`Repositories/`**:
  - `UserRepository` — `GetByIdAsync`, `GetByUserNameAsync` (active only), `FindByExternalIdAsync`, `GetAllAsync`, `Add`, `SaveChangesAsync`.
  - `ConversationRepository` — CRUD + `GetConversationForUserAsync` (ownership check), `GetAllConversationsForUserAsync`, paged/list/search, `DeleteConversationByUserAsync`, `RenameAsync`.
  - `RefreshTokenRepository` — `AddAsync`, `GetByTokenHashAsync`, `SaveChangesAsync`.

### AI (`AI/`) — Ollama integration

- **`OllamaOptions`** — `BaseUrl`, `Model` (from config).
- **`OllamaProvider`** — non-streaming `AskAsync` (`POST /api/chat`).
- **`OllamaStreamingProvider`** — `StreamAsync` (reads NDJSON lines from Ollama's streamed response, calls `onChunk` per token). Filters out `<think>` / `</think>` tokens (DeepSeek-style reasoning blocks). Also has a non-streaming `AskAsync` used for title generation.
- **`OllamaConversationTitleGenerator`** — asks the model for a short (≤5 word) title on the first message; falls back to truncating the message to 40 chars.
- Supporting request/response models: `OllamaChatRequest`, `OllamaChatResponse`, `OllamaStreamChunk`, `OllamaStreamMessage`.

### Security (`Security/`)

- **`PasswordHasher`** — BCrypt (`BCrypt.Net.BCrypt.HashPassword` / `Verify`).

---

## 5. API Layer (`AiChat.Api`)

### `Program.cs` wiring

- Loads `appsettings.Runtime.json` (runtime-overridable settings, reload-on-change).
- Reads `Authentication:Mode` (`Local` vs `ActiveDirectory`).
- **Auth**: JWT bearer; `OnMessageReceived` reads `access_token` from the query string for `/hubs/chat` (SignalR). A `UserPolicy` authorization policy requires an authenticated user.
- **DI**: `CurrentUserService`, `ActiveDirectoryAuthService`, `ActiveDirectorySettingsService` (singleton), `ActiveDirectoryDiagnosticService`, Ollama HttpClient(s), EF Core + SQLite, SignalR, memory cache (used by AI health check).
- Registers repositories, `SignalRChatNotifier`, `JwtTokenService`, `PasswordHasher`, `UserResolver`, `ChatCancellationTracker` (singleton), `AiHealthService`, and all handlers.
- Global exception handler + ProblemDetails.
- CORS (`AngularClient` policy), Swagger, controllers, `ChatHub` mapped at `/hubs/chat` (authorized).
- Runs `DbSeeder.SeedAsync` at startup.

### Controllers

| Controller | Route | Endpoints |
|---|---|---|
| `AuthController` | `api/auth` | `GET me` (current user), `GET mode` (auth mode). |
| `LocalAuthController` | `api/auth` | `POST login` (username/password). |
| `ConversationsController` | `api/conversations` | `POST` (create), `GET` (list), `GET {id}` (history), `POST {id}/messages` (send), `POST {id}/cancel`, `DELETE {id}`, `PUT {id}` (rename). |
| `UsersController` | `api/users` | `GET` (list), `POST` (create), `PUT {id}` (update), `DELETE {id}` (soft delete). |
| `ActiveDirectoryAdminController` | `api/active-directory` | `GET settings`, `PUT settings`, `POST diagnostics`. |
| `AiHealthController` | `api/health` | `GET ai`, `GET ai/details`, `GET ai/models`. |

### SignalR Hub

- **`ChatHub`** — clients call `JoinConversation(conversationId)`; the hub validates ownership and adds the connection to a group named `conversation:{id}:user:{userId}`. Server pushes chunks to that group via `SignalRChatNotifier`.

### Services

- **`JwtTokenService`** — issues access tokens (claims: `NameIdentifier`, `Name`, `sub`, `jti`, `auth_provider`, `external_id`, roles) and refresh tokens (random 64 bytes, SHA-256 hashed before storage).
- **`CurrentUserService`** — parses `CurrentUser` from `HttpContext` claims (handles Local vs AD identity).
- **`UserResolver`** — resolves the full `User` entity for the current request (Local by `UserId`, AD by `ExternalId`).
- **`GlobalExceptionHandler`** — maps exceptions to `ProblemDetails` (404 for "model not found", 503 for Ollama unavailable, 500 otherwise). Messages are in Persian.
- **`ChatCancellationTracker`** — singleton `ConcurrentDictionary` of per-conversation `CancellationTokenSource`s used to cancel in-flight AI streams.
- **`AiHealthService`** — checks Ollama health (`api/tags`), caches for 10s, and returns server details (`api/version`, models) and available model names.
- **`ActiveDirectorySettingsService`** — reads/writes `Authentication:ActiveDirectory` from `appsettings.Runtime.json` (atomic temp-file write + `SemaphoreSlim`), with validation.

### Contracts / AD support

- **`SignalRChatNotifier`** — sends `ReceiveToken` (chunk) and `ReceiveCompleted` events to the conversation's SignalR group.
- **`ActiveDirectoryAuthService`** — validates credentials against AD using `System.DirectoryServices.AccountManagement` (`PrincipalContext`), with primary/fallback server + domain targets.
- **`ActiveDirectoryDiagnosticService`** — runs network diagnostics (LDAP 389, Kerberos 88, DNS 53, DC discovery via `nltest`, SRV lookup) by shelling out to `powershell.exe` / `nltest.exe`.
- **`AdInputValidator`** — validates hosts, domains, and LDAP distinguished names.

---

## 6. Authentication Flow

`Authentication:Mode` is either `Local` or `ActiveDirectory` (also referenced as "Windows Integrated").

**Local login** (`POST /api/auth/login` → `LoginCommandHandler`):
1. Normalize username.
2. Look up user in DB by username (active only).
3. If user has a `PasswordHash` (Local user), verify BCrypt. If wrong → reject (no AD fallback).
4. If AD is enabled, also try AD validation.
5. If still unresolved and AD is enabled, validate against AD; on success, find/create the user by `ExternalId` (auto-provisioning AD users with empty password hash).
6. Issue access + refresh tokens; store hashed refresh token.

**Windows Integrated** (frontend calls `GET /api/auth/me` with credentials): `AuthController.Me` returns the current user from the token/claims. (Note: the commented-out `AddNegotiate()` indicates this was explored via Windows Negotiate.)

---

## 7. Chat / Streaming Flow

1. Client sends `POST /api/conversations/{id}/messages` with `{ message, model }`.
2. `SendMessageHandler`:
   - Loads the conversation (ownership-checked).
   - Adds the user message; auto-generates a title on the **first** message.
   - Builds a `MessageDto` list from the **last 20 messages** as context.
   - Calls `OllamaStreamingProvider.StreamAsync`, streaming tokens back through `SignalRChatNotifier.SendChunkAsync` → SignalR `ReceiveToken`.
   - On completion → `CompleteAsync` → SignalR `ReceiveCompleted`.
   - Saves the assistant's full answer to the DB.
3. Cancellation: `POST /api/conversations/{id}/cancel` cancels the per-conversation `CancellationTokenSource`; the handler aborts streaming, notifies completion, and throws `OperationCanceledException` (HTTP 499).

---

## 8. Configuration (`appsettings*.json`)

- `Authentication:Mode` — `Local` | `ActiveDirectory`.
- `Authentication:Jwt` — `Issuer`, `Audience`, `Key`, `AccessTokenMinutes`, `RefreshTokenDays`.
- `Authentication:ActiveDirectory` — `Enabled`, `Domain`, `Container`, `PrimaryServer`, `FallbackServers`, `UseSsl`.
- `Ollama` — `BaseUrl`, `Model`.
- `ConnectionStrings:SqliteConnection`.
- `Cors:AllowedOrigins`.

`appsettings.Runtime.json` is the runtime-writable config (AD settings are persisted here from the admin UI).

---

## 9. Database

- **SQLite** via EF Core. Migration: `20260806060219_init`.
- Tables: `Users`, `Conversations`, `Messages`, `RefreshTokens`.
- `Chat.db` lives in `Backend/AiChat.Api/`.
