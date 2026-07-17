# Password Reset Plan

## Goal

Allow users with local email/password accounts to securely reset their password through a one-time email link sent with Resend.

## Decisions

- Reset links use `App:PublicUrl`, configured in production as `https://fanfoot.swaenepoel.org`.
- Tokens are cryptographically random, persisted only as SHA-256 hashes, expire after one hour, and are single-use.
- The reset request endpoint returns the same response for known and unknown email addresses.
- A successful reset increments a user session version so all previously issued authentication cookies are rejected.

## Verification

1. Run `dotnet build` and `dotnet test`.
2. Set `RESEND_API_KEY`, `RESEND_FROM_EMAIL`, and `APP_PUBLIC_URL` in the deployment `.env` file.
3. Request a reset for a known account, follow the email link, and set a new password.
4. Confirm the old password and all pre-reset sessions no longer work.
