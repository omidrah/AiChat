# AiChat — Product Overview

An AI chat application: **ASP.NET Core** backend + **Angular 22** web frontend, with chat responses streamed live from **Ollama** over **SignalR**, persisted in **SQLite** (EF Core), and authentication via **JWT** (Local or Active Directory).

## Documents

- [Backend documentation](docs/backend.md) — solution structure, domain/application/infrastructure/API layers, auth flow, streaming flow, config, database.
- [Frontend documentation](docs/frontend.md) — Angular app structure, services, store, components/pages, guards/interceptors, environments.
- [Rules](Rules.md) — project conventions for contributors and AI agents.

## At a Glance

```
Angular 22 (ai-chat-web)  ──HTTP (JWT)──►  ASP.NET Core (AiChat.Api)
        │                                        │  CQRS handlers
        │                                        ▼
        └──────────SignalR (stream)────────►  ChatHub  ──►  Ollama (AI)
                                                 │
                                                 ▼
                                         SQLite (EF Core)
```

- **Backend** (`Backend/`): clean-architecture solution — `AiChat.Domain`, `AiChat.Application`, `AiChat.Infrastructure`, `AiChat.Api`, `Shared`, `AiChat.Contracts`.
- **Frontend** (`Frontend/`): `ai-chat-web` (Angular) + an experimental `ai-chat-Avolonia` desktop client.
- **Key flows**: Local/AD login → JWT → REST for conversations/users → SignalR streaming for AI replies (with cancellation).
