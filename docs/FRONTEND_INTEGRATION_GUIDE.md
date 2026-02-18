# Frontend Integration Guide (Phase 4)

## Base URL
- All endpoints are versioned under `/api/v1`.

## Authentication
Required for all finance endpoints:
- Header: `Authorization: Bearer <jwt>`
- Claim: `scope=finance-api`

User context is derived from token claims and not accepted from body/query for protected operations.

Session refresh requirement:
- On `401` with header `x-token-expired: true`, refresh token using Firebase SDK (`getIdToken(true)`) and retry once.
- Do not keep custom refresh tokens in app storage.

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
