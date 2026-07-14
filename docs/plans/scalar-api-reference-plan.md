# Scalar API Reference Plan

## Goal

Provide a Scalar API reference at `/scalar`, backed by the existing Microsoft OpenAPI document at `/openapi/v1.json`.

## Implementation

1. Add the `Scalar.AspNetCore` package to `src/Fanfoot.Web/Fanfoot.Web.csproj`.
2. Import `Scalar.AspNetCore` in `src/Fanfoot.Web/Program.cs`.
3. Map Scalar at `/scalar` after `MapOpenApi()` so it uses the existing OpenAPI document.
4. In Development, leave both `/scalar` and `/openapi/v1.json` anonymous for local API exploration.
5. Outside Development, require the existing cookie-authenticated user for both `/scalar` and `/openapi/v1.json` so the reference can load its specification only for signed-in users.

## Verification

1. Run `dotnet build` from the repository root.
2. In Development, confirm `/scalar` and `/openapi/v1.json` load without signing in.
3. In a non-Development environment, confirm anonymous requests receive `401` and a signed-in user can load `/scalar` and its OpenAPI document.

## Files

- `src/Fanfoot.Web/Fanfoot.Web.csproj`
- `src/Fanfoot.Web/Program.cs`
