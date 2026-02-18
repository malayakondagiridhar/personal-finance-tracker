# API v1 Contract and Auth Guide

## Base URL
- Versioned base path: `/api/v1`

## Authentication
All finance endpoints require a bearer token.

Required:
- Header: `Authorization: Bearer <jwt>`
- Claim: `scope=finance-api`

Identity source of truth:
- User context is derived from JWT claims (`nameidentifier`/`sub`)
- Client-supplied `userId` in request body/query is not accepted for protected operations

## Endpoint Summary
- `POST /api/v1/categories`
- `GET /api/v1/categories`
- `POST /api/v1/transactions`
- `GET /api/v1/transactions?fromDateUtc=&toDateUtc=&categoryId=&type=`
- `PUT /api/v1/transactions/{transactionId}`
- `DELETE /api/v1/transactions/{transactionId}`
- `POST /api/v1/budgets`
- `GET /api/v1/budgets/status?year={year}&month={month}`
- `GET /api/v1/summary/monthly?year={year}&month={month}`

## Error Contract
Error responses follow standardized payload:

```json
{
  "title": "Business rule conflict",
  "status": 409,
  "detail": "Category with the same name already exists for this user.",
  "traceId": "00-..."
}
```

Mapped status behavior:
- 400: validation errors / invalid request constraints
- 401: missing/invalid/expired authentication token
- 403: authenticated but missing required scope/policy
- 404: resource or related entity not found
- 409: business conflict
- 500: unhandled server error

401 contract details:
- JSON payload includes `title`, `status`, `detail`, `traceId`
- For expired token flows, response includes `x-token-expired: true`
- Client should refresh Firebase ID token and retry once
