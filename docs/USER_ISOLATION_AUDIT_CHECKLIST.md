# UserId Isolation Audit Checklist

Use this checklist for every new endpoint and during release hardening.

## Mandatory data-scope rules

- Every read/write query must be constrained by authenticated `userId`.
- Never trust `userId` from request body/query/path for protected endpoints.
- Update/Delete operations must include owner-scoped lookup (`Id + UserId`).
- Cross-user access must return `404` or `403` consistently by contract.

## Controller layer checks

- Controller resolves internal user id from auth context (`HttpContext.GetRequiredUserId()`).
- No protected controller action accepts raw `userId` from client.
- `[Authorize(Policy = "FinanceApi")]` is present on protected endpoints.

## Service layer checks

- Service methods for user data include `Guid userId` parameter.
- Query filters include `Where(x => x.UserId == userId)` before projection/paging.
- Aggregate/summarization queries are user-scoped.

## Persistence checks

- Entity relationships preserve user ownership semantics.
- Soft delete filters must still respect user scope.
- New indexes should include `UserId` when query patterns are user-scoped.

## Testing gates (required)

For each protected resource type, include/keep tests for:
1. user A can create/read own data.
2. user B cannot read/update/delete user A data.
3. list endpoints only return current user records.
4. summary/aggregate endpoints exclude other users data.

## Future endpoint template

Before merging any new endpoint:
1. Add endpoint to this checklist review in PR description.
2. Add at least one cross-user integration test.
3. Verify no client-supplied `userId` is used.
4. Verify response/error contract for unauthorized scope.
