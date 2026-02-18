# Sign-out and Token Invalidation Contract

This system uses Firebase-managed sessions with a stateless backend.

## Frontend expectations

On user sign-out:
1. Call Firebase `signOut(auth)`.
2. Clear any in-memory auth state (current user/token cache).
3. Stop sending `Authorization` header for subsequent API calls.
4. Redirect user to signed-out/login UX.

After sign-out, previously cached ID token must not be reused by client code.

## Backend expectations

- Backend does not maintain server-side session state.
- Backend validates token on each request.
- Backend does not issue or store refresh tokens.
- Backend does not implement token blacklist/invalidation list.

## Practical invalidation model

- Primary invalidation event is Firebase session end on client.
- Expired tokens are rejected by backend (`401`, with expiry hint header when applicable).
- If immediate revocation is needed (security incident), revoke in Firebase Admin and require client re-authentication.

## Failure contracts

### Signed-out client requests protected route without token
- Response: `401 Unauthorized`

### Client sends expired token after sign-out or stale cache
- Response: `401 Unauthorized`
- Header: `x-token-expired: true` when expiration detected

## Security guidance

- Never persist custom refresh tokens in app storage.
- Use Firebase SDK persistence/session handling.
- Keep frontend auth state transitions deterministic to avoid stale header reuse.
