# AiChat — Frontend Documentation

The frontend is an **Angular 22** single-page application in `Frontend/ai-chat-web/`. It renders the chat UI and admin screens, and communicates with the backend over REST and **SignalR** (for streamed AI responses).

There is also a separate experimental **Avalonia** desktop client in `Frontend/ai-chat-Avolonia/` (not covered further here).

---

## 1. Stack

| Concern | Technology |
|---|---|
| Framework | Angular 22 (standalone components) |
| UI | Angular Material (mostly `MatSnackBar`), custom CSS |
| Forms | `FormsModule` + `ReactiveFormsModule` |
| State | Angular `signal()`-based stores |
| Realtime | `@microsoft/signalr` |
| Markdown | `marked` + `marked-highlight` + `highlight.js` |
| HTTP | Angular `HttpClient` with functional interceptors |
| Tests | Vitest + jsdom (a few `.spec.ts` files) |
| Styles | Plain CSS (RTL, dark-mode toggle) |

Package manager: `npm` (`packageManager: npm@11.13.0`). Scripts: `start`, `build`, `watch`, `test`.

---

## 2. Directory Structure (`src/`)

```
src/
├── index.html            # host page
├── main.ts               # bootstrapApplication(...)
├── styles.css            # global styles
├── environments/         # environment.ts / environment.prod.ts (apiUrl, hubUrl)
├── assets/               # fonts (Vazirmatn) and static assets
└── app/
    ├── app.ts            # root App component (shell)
    ├── app.html / app.css
    ├── app.config.ts     # providers (router, http interceptors, FormsModule)
    ├── app.routes.ts     # route table
    ├── chat/             # chat component
    ├── conversations/    # conversation-list component
    ├── pages/            # login, user-management, active-directory-settings
    ├── ollama-status/    # AI/Ollama status page
    ├── services/         # api, auth, signalr, active-directory-admin
    ├── store/            # conversation.store.ts
    ├── models/           # TS interfaces (conversation, message, AD, AiHealthStatus)
    ├── guard/            # auth.guard.ts
    └── interceptor/      # auth.interceptor.ts, error.interceptor.ts
```

---

## 3. Application Shell (`app.ts`)

`App` is the root component. It renders a sidebar + topbar layout (RTL, `direction: rtl`) when **not** on the login page.

Features:
- Collapsible sidebar (`toggleSidebar`).
- Dark/light theme toggle (`toggleTheme` — toggles a `dark` class on `<body>`).
- Admin-only buttons in the topbar (`isAdmin` getter checks the decoded JWT username for `admin`/`administrator`): **User Management**, **AI (Ollama) status**, **Active Directory settings**.
- Navigation helpers: `usersRoute()`, `aiHealthRoute()`, `activeDirectoryRoute()`.

---

## 4. Routing (`app.routes.ts`)

| Path | Component |
|---|---|
| `/login` | `LoginComponent` |
| `/chat/:id` | `ChatComponent` |
| `/users` | `UserManagement` |
| `/active-directory` | `ActiveDirectorySettings` |
| `/ollama` | `OllamaStatus` |
| `/` and `**` | redirect to `/login` |

---

## 5. Services

### `ApiService`
Central REST client (base URL from `environment.apiUrl`):
- Conversations: `createConversation`, `getConversations`, `getConversation`, `getMessages`, `sendMessage`, `deleteConversation`, `renameConversation`, `cancelMessage`.
- Users: `getUsers`, `createUser`, `updateUser`, `deleteUser`.
- Health: `getAiHealth` (falls back to an unhealthy status on 503/network error), `getOllamaDetails`, `getModels`.

### `AuthService`
- `getMode()` / `setMode()` — auth mode (`form` vs `windows`), persisted in `localStorage` (`auth_mode`).
- `login(userName, password)` — POST `/auth/login`, stores tokens.
- `windowsLogin()` — GET `/auth/me` with credentials (Windows Integrated path).
- `refresh()` — POST `/auth/refresh`, stores new tokens.
- Token helpers: `getToken`, `getRefreshToken`, `getUserName` (decodes the JWT payload to read the name claim), `isLoggedIn`, `logout`.
- Tokens stored in `localStorage` (`access_token`, `refresh_token`, `accessTokenExpiresAt`).

### `SignalRService`
Wraps a SignalR `HubConnection` to `environment.hubUrl`:
- `start()` — builds the connection with `accessTokenFactory` that refreshes the token if it's expiring (<30s), `withAutomaticReconnect`, and re-joins the conversation on reconnect.
- `joinConversation(id)` — invokes the `JoinConversation` hub method.
- `onReceiveToken(cb)` / `offReceiveToken()` — listens for streamed chunks.
- `onReceiveCompleted(cb)` / `offReceiveCompleted()` — stream-completion event.
- `stop()`.

### `ActiveDirectoryAdminApi`
- `getSettings()`, `updateSettings(model)`, `runDiagnostic(model)` against `/active-directory/*`.

---

## 6. State Store (`store/conversation.store.ts`)

`ConversationStore` is a signal-based store (provided in root):
- `conversations` — a `signal<Conversation[]>` exposed read-only.
- `load()` — fetches and sets the list.
- `create()` — creates a conversation and reloads; returns the new id.
- `rename(id, title)` — renames via API and updates local state.
- `delete(id)` / `deleteAndNavigate(id, currentId)` — delete + reload, returning the next conversation to navigate to.

---

## 7. Components / Pages

### `ChatComponent` (`chat/`)
The main chat screen.
- Loads available models (`getModels`), defaults to the first.
- Periodically polls AI health every **15 seconds** (`checkAiHealthCheckEvery15Seconds`), showing `ONLINE`/`OFFLINE`/`RECONNECTING` state and warning on disconnect.
- On route change, opens the conversation (starts SignalR, joins the conversation group, loads history).
- `send()` — optimistically adds the user message, then POSTs to `/messages`; the assistant reply is built token-by-token from `ReceiveToken` events and appended/finalized on `ReceiveCompleted`.
- `cancel()` — POSTs `/cancel` to stop generation.
- Markdown rendering via `marked` + `highlight.js`; auto-scroll with user-override detection.
- Copy-to-clipboard (Clipboard API + fallback), auto-resizing textarea, Enter-to-send / Shift+Enter for newline.

### `ConversationList` (`conversations/conversation-list/`)
Sidebar list:
- Lists conversations from the store, tracks the selected one via router URL.
- Create, delete (with confirm), rename (inline edit), open, and logout.

### `LoginComponent` (`pages/login/`)
- On init fetches the auth `mode`. Two modes:
  - **form** — username/password form.
  - **windows** — auto `windowsLogin()`.
- After login, loads the conversation list and navigates to the first conversation (or creates one).

### `UserManagement` (`pages/user-management/`)
- Lists users (filters out `admin`/`administrator` system accounts client-side too).
- Search by username/displayName/authProvider.
- Create / edit / soft-delete users (form fields: userName, displayName, role, password, isActive, authProvider).

### `ActiveDirectorySettings` (`pages/active-directory-settings/`)
- Reactive form for AD settings (`enabled`, `domain`, `container`, `server`, `useSsl`, dynamic `servers` list).
- Load / save settings, and run diagnostic tests (LDAP 389, Kerberos 88, DNS 53, DC discovery, SRV lookup) showing command output.

### `OllamaStatus` (`ollama-status/`)
- Shows Ollama server details (version, connection status, installed models with size/format/family/parameter size).

---

## 8. Guards & Interceptors

- **`auth.guard.ts`** — `authGuard` redirects to `/login` unless `isLoggedIn()`.
- **`auth.interceptor.ts`** — attaches `Authorization: Bearer <token>`; for `windows` mode adds `withCredentials`. On **401**, attempts a token refresh and retries the request; on refresh failure logs out and redirects to login.
- **`error.interceptor.ts`** — surfaces backend errors (or network/404/503) as a Persian `MatSnackBar` toast.

---

## 9. Models (`models/`)

- `conversation.ts` — `Conversation { id, title }`.
- `message.ts` — `Message { id?, role: 'user'|'assistant', content, createdAt, model?, copied? }` and `ConversationDetailsDto`.
- `ActiveDirectory.ts` — `ActiveDirectorySettings`, `ActiveDirectoryDiagnosticType` enum, request/result types.
- `AiHealthStatus.ts` — `AiHealthStatus`, `OllamaModelInfo`, `OllamaServerDetails`.

---

## 10. Environments

- `environment.ts` (dev): `apiUrl: https://localhost:7117/api`, `hubUrl: https://localhost:7117/hubs/chat`.
- `environment.prod.ts`: `http://132.168.250.205:8080/api` / `.../hubs/chat`.

---

## 11. UI Notes

- **RTL** layout (Persian UI text throughout: buttons, errors, snackbars).
- Custom CSS (no Tailwind): `app.css`, `chat.css`, `conversation-list.css`, page-specific CSS.
- Fonts: Vazirmatn (woff2) under `assets/fonts/vazirmatn/`.
