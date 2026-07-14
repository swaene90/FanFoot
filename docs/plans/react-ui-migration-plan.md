# React UI Migration Plan

Target: a complete React SPA in `src/Fanfoot.Web/ClientApp`, built by Vite into ASP.NET Core `wwwroot`. The .NET host remains the API, authentication, data, and deployment service.

## 1. Replace Blazor Hosting

- Add React, TypeScript, Vite, React Router, and a minimal test setup.
- Configure Vite production output to `src/Fanfoot.Web/wwwroot`.
- Replace `AddRazorComponents`, MudBlazor registration, and `MapRazorComponents` in `Program.cs` with SPA static-file hosting and a history fallback.
- Remove the Blazor `Components` UI and the MudBlazor package after React parity is complete.
- Update Dockerfile and CI to install Node, build the React bundle, then build/test .NET.

## 2. Standardize Cookie Authentication

- Retain ASP.NET Core's HttpOnly same-origin authentication cookie.
- Extend `AuthController` with:
  - `POST /api/auth/register`
  - `POST /api/auth/login`, returning the authenticated user
  - `POST /api/auth/logout`
  - `GET /api/auth/me`
- Remove React's need for the current JavaScript-readable `fanfoot_user` cookie.
- Add antiforgery-token issuance and validation for cookie-authenticated mutations.
- Use a React API client that sends same-origin credentials and the antiforgery header.

## 3. Expose Authenticated Feature APIs

- Add explicit DTOs rather than serializing internal models or persistence entities.
- Add `/api/me` for profile, current leagues, and season-grouped teams.
- Add `/api/me/preferences` `GET`/`PUT` for dark mode.
- Add league endpoints for detail, prior-season import, roster detail, and draft detail.
- Add chat endpoints for session list/detail and sending a message.
- Keep Sleeper, FantasyCalc, ESPN, and LLM integrations exclusively server-side.
- Move draft-order, snake-draft, and traded-pick calculations from `DraftDetail.razor` into a server-side projection service.
- Make draft import an explicit `POST` operation rather than a mutating `GET`.

## 4. Add Authorization Boundaries

- Implement centralized checks based on the authenticated `ClaimTypes.NameIdentifier`.
- Require league membership before returning league, roster, draft, or chat-league data.
- Require session ownership before reading or writing chat sessions.
- Return `404` for inaccessible resources.
- Restrict or remove the globally expensive `/api/players/import` endpoint, which is currently accessible to every authenticated user.

## 5. Build the React Application

- Routes:
  - `/` sign-in / registration
  - `/user/:userId` user teams by season
  - `/league/:leagueId` standings and prior-season import
  - `/league/:leagueId/team/:teamId` roster
  - `/league/:leagueId/draft` draft and traded-pick view
  - `/chat` AI chat and history
- Use route guards backed by `/api/auth/me`; redirect unauthenticated users to `/`.
- Reject or redirect a `/user/:userId` URL that does not match the signed-in user.
- Preserve existing behavior, including loading/error states, roster sections, filters, chat auto-scroll, persisted chat history, and simple `Importing leagues...` registration loading state.

## 6. Recreate the Visual System With Custom CSS

- Replace MudBlazor components with accessible native controls and reusable React components for cards, buttons, inputs, tables, alerts, tabs, drawer, and loading indicators.
- Preserve the Lions design system in CSS variables:
  - Honolulu Blue `#0076B6`
  - light white/light-blue theme
  - black/silver dark theme
  - responsive app bar and collapsible drawer
- Implement theme via `data-theme` or a body class, initialize it from server preferences, and persist changes through `/api/me/preferences`.
- Move needed global styles, chat bubble styling, font, and favicon from current `wwwroot` assets.

## 7. Verify and Document

- Add controller tests for authentication, ownership isolation, DTO output, preferences, draft projection, and chat-session access.
- Add React tests for session guarding, login/registration errors, theme persistence, and key page rendering.
- Run `dotnet test`, the React test/build commands, and a production Docker build.
- Update README, architecture docs, UI guide, and local-development instructions to describe React/Vite instead of Blazor/MudBlazor.

## Primary Files

- Replace UI: `src/Fanfoot.Web/Components/**`
- Add React app: `src/Fanfoot.Web/ClientApp/**`
- Add APIs: `src/Fanfoot.Web/Controllers/{Me,Leagues,Chat}Controller.cs`
- Extend: `src/Fanfoot.Web/Controllers/AuthController.cs`
- Add DTOs: `src/Fanfoot.Web/Api/Dtos/**`
- Add authorization and draft projection services under `Domain/Services`
- Update: `Program.cs`, `Fanfoot.Web.csproj`, `Dockerfile`, `.github/workflows/dotnet.yml`, documentation
