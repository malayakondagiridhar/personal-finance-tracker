# Frontend Integration Guide (Phase 4)

## Base URL
- All endpoints are versioned under `/api/v1`.
- Frontend environment wiring:
  - `VITE_API_BASE_URL` (recommended in staging/prod)
  - `VITE_API_VERSION_PREFIX` (default `/api/v1`)
  - `VITE_API_PROXY_TARGET` (dev-only Vite proxy target, default `http://localhost:5256`)
- Dev default behavior when `VITE_API_BASE_URL` is not set:
  - frontend calls relative `/api/v1/...`
  - Vite proxies `/api/v1` to `VITE_API_PROXY_TARGET`

## Authentication
Required for all finance endpoints:
- Header: `Authorization: Bearer <jwt>`
- Claim: `scope=finance-api`

User context is derived from token claims and not accepted from body/query for protected operations.

Session refresh requirement:
- On `401` with header `x-token-expired: true`, refresh token using Firebase SDK (`getIdToken(true)`) and retry once.
- Do not keep custom refresh tokens in app storage.

Sign-out contract:
- Call Firebase `signOut(auth)` and clear client auth state.
- Do not send cached bearer tokens after sign-out.
- Reference: `docs/SIGNOUT_AND_TOKEN_INVALIDATION.md`

## CORS
Configured via:
- `Cors:AllowedOrigins` in appsettings / environment variables.
- Development default includes `http://localhost:5173`.

## Pagination and Sorting
Category list:
- `GET /api/v1/categories?page=1&pageSize=20&sortBy=name&sortDirection=asc`
- Supported `sortBy`: `name`, `createdAtUtc`
- `page >= 1`, `pageSize` between 1 and 100

Transaction list:
- `GET /api/v1/transactions?fromDateUtc=&toDateUtc=&categoryId=&type=&page=1&pageSize=20&sortBy=transactionDateUtc&sortDirection=desc`
- Supported `sortBy`: `transactionDateUtc`, `amount`, `createdAtUtc`
- `page >= 1`, `pageSize` between 1 and 100

Validation failures return HTTP 400 with standard error payload.

Client-side contract handling expectations:
- `409` (conflict): show actionable message (for example duplicate category or existing monthly budget).
- `429` (rate limited): show retry guidance and avoid immediate repeated retries.
- `401` (expired token): rely on centralized refresh-retry path; if still unauthorized, redirect to auth.

## Rate Limiting
Global fixed-window limiter:
- 60 requests per minute per authenticated subject (fallback to client IP for anonymous traffic)
- Exceeding limit returns HTTP 429 with JSON payload:

```json
{
  "title": "Rate limit exceeded",
  "status": 429,
  "detail": "Too many requests. Please retry after a short delay.",
  "traceId": "..."
}
```

## Example Requests
Create category:

```http
POST /api/v1/categories
Authorization: Bearer <jwt>
Content-Type: application/json

{
  "name": "Food",
  "description": "Groceries and dining",
  "isDefault": false
}
```

Fetch paged transactions sorted by amount descending:

```http
GET /api/v1/transactions?page=1&pageSize=10&sortBy=amount&sortDirection=desc
Authorization: Bearer <jwt>
```
