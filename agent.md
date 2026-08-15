# AGENTS.md

Instructions for AI coding agents working in this repository. Read this before making changes.

## What this is

An AI chat application:
- **Backend** (`Backend/`): ASP.NET Core, clean architecture, CQRS-style handlers, EF Core + SQLite, Ollama (AI), SignalR streaming.
- **Frontend** (`Frontend/ai-chat-web/`): Angular 22 standalone components, signals, SignalR client.

See `docs/backend.md`, `docs/frontend.md`, and `docs/Rules.md` for full details.

---

## Build / Test / Verify

Run from the repo root (commands are bash).

```bash
# Backend build (typecheck/compile)
dotnet build Backend/AiChat.sln

# Backend publish (release)
dotnet publish Backend/AiChat.Api -o publish -c Release
```

```bash
# Frontend (cd Frontend/ai-chat-web)
npm run build        # compile (use for verification)
npm test             # Vitest unit tests
npm run start        # dev server (ng serve)
```

- There are **no backend unit-test projects** (the `.sln` has 5 source projects only). Verify backend changes with `dotnet build`.
- Frontend tests use **Vitest** (`ng test`); only a few `.spec.ts` files exist (`ollama-status`, `user-management`, `active-directory-settings`).
- After non-trivial changes, run the relevant build above and report results.

---

## Key locations (start here)

| Task | Look in |
|---|---|
| Add a backend endpoint | `AiChat.Api/Controllers/*`, handlers in `AiChat.Application/*/Commands|Queries/*` |
| Register a handler | `AiChat.Application/DependencyInjection.cs` (or `Program.cs` for API-level ones) |
| DB entities / schema | `AiChat.Domain/Entities/*`, configs in `AiChat.Infrastructure/Persistence/Configurations/*` |
| DB context / migrations | `AiChat.Infrastructure/Persistence/ChatDbContext.cs`, `.../Migrations/*` |
| AI / Ollama calls | `AiChat.Infrastructure/AI/*` |
| SignalR streaming | `AiChat.Api/Hubs/ChatHub.cs`, `AiChat.Api/Contracts/SignalRChatNotifier.cs` |
| Auth / JWT / AD | `AiChat.Api/Services/*`, `AiChat.Api/Contracts/Admin/*`, `AiChat.Application/Authentications/*` |
| Frontend API calls | `ai-chat-web/src/app/services/*` |
| Frontend state | `ai-chat-web/src/app/store/conversation.store.ts` |
| Frontend routes | `ai-chat-web/src/app/app.routes.ts` |

---

## Conventions (must follow)

### Backend
- **Layering**: `Api → Application → Domain`; `Infrastructure` implements `Application/Abstractions/*`. Never add framework deps to `Domain`.
- **CQRS without MediatR**: each feature is a request record + `XxxHandler` class. Register handlers in DI. Controllers use `[FromServices]` handler params.
- **Domain**: private constructors + static factories (`CreateConversation`, `CreateUser`, `CreateMessage`); mutate only via domain methods.
- **Results**: user-management handlers return `Shared.Result<T>` / `Error` (codes like `NotFound`, `BadRequest`).
- **Auth**: Local users have a `PasswordHash`; AD users have empty hash + `ExternalId`. BCrypt via `IPasswordHasher`. Refresh tokens are SHA-256 hashed before storage.
- **Ownership**: every conversation query/command filters by `UserId`.
- **Ollama streaming**: stream via `IAiStreamingProvider.StreamAsync`, push chunks via `IChatStreamNotifier` to SignalR group `conversation:{id}:user:{userId}`, keep `<think>` filtering.
- **User-facing strings are Persian**; keep new ones Persian.

### Frontend
- Standalone components (`imports: [...]`, no NgModules except app config).
- `signal()` for reactive state; shared state in `store/`.
- All HTTP through `ApiService` (or a domain service); base URL in `environment.ts`.
- Tokens in `localStorage`; `auth.interceptor.ts` handles Bearer + 401→refresh→retry.
- Errors via `error.interceptor.ts` (Persian, RTL MatSnackBar).
- SignalR through `SignalRService`; clean up handlers in `ngOnDestroy`.
- RTL layout, plain per-component CSS, no Tailwind.

---

## Gotchas

- Controllers take handlers as `[FromServices]` parameters — unusual but intentional; follow it for new endpoints.
- `ConversationTitle` value object exists but is empty/unused.
- `GetConversationHandler` has a bug: it sets `Id = conversation.Id` (not `x.Id`) for every message in the DTO mapping — be aware when touching that file.
- The AD diagnostics service shells out to `powershell.exe` / `nltest.exe` (Windows-only).
- AD settings are persisted to `appsettings.Runtime.json` at runtime (atomic temp-file write + `SemaphoreSlim`).
- `Chat.db` (SQLite) lives in `Backend/AiChat.Api/`; it's a dev artifact — don't commit schema drift by hand, use migrations.
- Frontend `environment.prod.ts` points at a hardcoded LAN IP (`132.168.250.205:8080`).
- Admin detection on the frontend is name-based (`admin`/`administrator`), not role-based.

## Do not

- Run `git push`, resets, or destructive commands without explicit permission.
- Modify files outside the project, install global packages, or touch the DB/production without asking.
- Introduce unrelated formatting-only churn (e.g., the whitespace tweaks in `UserRepository.cs`).
