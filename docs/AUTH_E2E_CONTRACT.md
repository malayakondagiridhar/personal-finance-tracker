# End-to-End Auth Contract (Firebase)

This document defines the production auth contract between frontend, backend, and Firebase Auth.

## 1) Identity model

- Identity provider: **Firebase Authentication**
- Sign-in provider: Google
- API auth model: stateless bearer JWT validation on every request
- Internal app user identity: mapped from external subject (`sub`) in backend profile sync middleware

## 2) Token lifecycle

### Client-side (Firebase SDK)
- User signs in with Firebase provider.
- Client acquires Firebase ID token (`getIdToken()`).
- Client sends token in `Authorization: Bearer <token>`.
- Firebase SDK handles refresh token lifecycle internally.
- Client must not persist custom refresh-token material.

### Server-side (API)
- API validates token signature, issuer, audience, and lifetime.
- Firebase mode uses:
  - issuer: `https://securetoken.google.com/{projectId}`
  - audience: `{projectId}`
- For valid token, backend resolves internal user context and enforces authorization policy.

## 3) Claims expectations

Required/expected claims in authenticated flows:
- `sub` (external subject id; stable per Firebase user)
- `iss` (Firebase issuer URL)
- `aud` (Firebase project id)

Policy compatibility:
- Existing policy accepts either:
  - legacy `scope=finance-api`, or
  - Firebase issuer-based authenticated path.

User scoping:
- Protected resources are always scoped by internal resolved user id.
- Client-supplied `userId` is ignored/rejected for protected business operations.

## 4) Failure modes and contract

### Missing token
- API returns `401 Unauthorized` JSON payload.

### Invalid token (issuer/audience/signature mismatch)
- API returns `401 Unauthorized` JSON payload.

### Expired token
- API returns `401 Unauthorized` JSON payload.
- API includes `x-token-expired: true` header.
- Client should force-refresh Firebase ID token (`getIdToken(true)`) and retry once.

### Authenticated but insufficient authorization
- API returns `403 Forbidden`.

## 5) Sign-out behavior

- Frontend sign-out ends local Firebase session on client.
- Backend remains stateless and does not keep server-side session state.
- See `docs/SIGNOUT_AND_TOKEN_INVALIDATION.md` for precise sign-out, stale-token handling, and invalidation model.

## 6) Operational requirements

- Configure Firebase authorized domains for all production/staging origins.
- Keep `Auth:FirebaseProjectId` aligned with deployed environment.
- Never commit service-account private keys.
