# Rules — AiChat Project Conventions

Guidelines for working on this codebase (for humans and AI agents). Follow these to keep changes consistent with the existing style.

---

## General

- **Layers must stay decoupled.** Dependencies point inward: `Api → Application → Domain`, and `Infrastructure` implements `Application`'s interfaces (`Abstractions/`). Don't let `Domain` reference EF Core, ASP.NET, etc.
- **Match existing patterns before adding new ones.** This is a CQRS-style app without MediatR — handlers are plain classes registered in DI.
- **UI text and user-facing error messages are in Persian (Farsi).** Keep new user-facing strings in Persian. Code comments are frequently Persian too.
- **Ownership is enforced on every resource access.** Conversation queries/commands always filter by `UserId` (`GetConversationForUserAsync`, etc.).
- **No destructive commands** (git push, resets, DB deletes) unless explicitly requested.

---

## Backend (C#)

### New feature = Command/Query + Handler
- Put a request record and a handler in `AiChat.Application/<Feature>/Commands/<Name>/` or `.../Queries/<Name>/`.
- Handlers take dependencies via constructor injection; names end in `Handler`.
- Register the handler in `AiChat.Application/DependencyInjection.cs` (`AddApplicationHandler()`) — or in `Program.cs` for API-level handlers (`UsersController` handlers are registered there).
- Controllers resolve handlers via `[FromServices]` parameters (a quirk of this codebase — follow it for new endpoints).

### Domain
- Entities use **private constructors + static factory methods** (`CreateConversation`, `CreateUser`, `CreateMessage`); mutate only through domain methods (`AddMessage`, `Rename`, `SetIsActive`...).
- Keep EF Core's parameterless constructor requirement satisfied (private parameterless ctor pattern is already used in `User`).

### Persistence
- Entity configuration belongs in `AiChat.Infrastructure/Persistence/Configurations/` (or inline in `ChatDbContext` for simple entities like `RefreshToken`).
- Repositories implement interfaces from `AiChat.Application/Abstractions/`; register them in `Program.cs`.
- New schema changes require an EF Core migration.

### Result / errors
- The `Shared` project provides `Result<T>` / `Result` / `Error`. User-management handlers return `Result<T>` with error codes (`NotFound`, `BadRequest`). Use it for new validation-heavy handlers.

### Auth
- Local users have a `PasswordHash`; AD users have an empty hash and are matched by `ExternalId`. Preserve this distinction.
- Refresh tokens are hashed (SHA-256) before storage.
- Passwords are hashed with **BCrypt** (`IPasswordHasher`).

### AI / Ollama
- Stream responses via `IAiStreamingProvider.StreamAsync` and notify clients via `IChatStreamNotifier` (SignalR groups named `conversation:{id}:user:{userId}`).
- Keep the `<think>`/`</think>` chunk filtering when streaming reasoning models.

---

## Frontend (Angular)

- **Standalone components** (no `NgModule` except app config). Use `imports: [...]` in each `@Component`.
- **State**: use `signal()` for reactive state; shared state lives in `src/app/store/` (see `ConversationStore`). Services are `providedIn: 'root'`.
- **HTTP**: route all API calls through `ApiService` (or a domain service like `ActiveDirectoryAdminApi`). Keep base URLs in `environment.ts`.
- **Auth**: tokens in `localStorage`; `auth.interceptor.ts` attaches the Bearer token and handles 401 → refresh → retry.
- **Errors**: user-facing toasts go through `error.interceptor.ts` (MatSnackBar, Persian, RTL).
- **Realtime**: use `SignalRService`; register token/completed handlers per conversation and clean them up in `ngOnDestroy`.
- **UI**: RTL layout (`direction: rtl`), plain CSS per component (`.css` next to `.ts`/`.html`), Persian labels. No Tailwind.
- **Models**: TypeScript interfaces live in `src/app/models/`.

---

## Configuration

- Runtime-writable settings (e.g. Active Directory) go in `appsettings.Runtime.json` and are persisted via `ActiveDirectorySettingsService`.
- New options classes follow the existing `XxxOptions` + `Configure<T>` pattern (`JwtOptions`, `OllamaOptions`, `ActiveDirectoryOptions`).

---

## Naming / Style

- C#: file-scoped namespaces, `PascalCase` types/methods, `camelCase` parameters.
- TypeScript: `PascalCase` classes/interfaces/components, `camelCase` members.
- Keep the existing `.editorconfig` / `.prettierrc` conventions. Don't introduce unrelated formatting-only churn.
