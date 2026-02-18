# Firebase Session & Token Refresh Strategy

## Session model

- Backend is stateless and validates bearer JWT on every request.
- Client must send Firebase ID token in `Authorization: Bearer <token>`.
- Backend does **not** issue app refresh tokens.
- Refresh is delegated to Firebase SDK on client side.

## Backend expectations

1. Access token must be valid and unexpired.
2. Token issuer/audience must match configured Firebase project.
3. Protected routes require authenticated user + `FinanceApi` policy.

## Unauthorized contract

On invalid/expired token, API returns `401`:

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Bearer token expired. Refresh Firebase ID token and retry.",
  "traceId": "..."
}
```

When expiration is detected, response also includes header:
- `x-token-expired: true`

## Client refresh behavior (required)

Before API calls, client should obtain a current Firebase ID token:
- normal path: `currentUser.getIdToken()`
- retry path after 401 + `x-token-expired=true`: `currentUser.getIdToken(true)` and retry once

Do not store refresh tokens manually in local storage/session storage.
Use Firebase Auth SDK managed persistence only.
