# Firebase Authorized Domains (Production/Staging)

Firebase Authentication must explicitly restrict OAuth redirect/callback domains.

## Why this is required

If authorized domains are left broad or incomplete, sign-in may fail in production or allow unintended origins.

## Required domains checklist

Add every deployed frontend host in Firebase Console:
- Production app domain(s)
- Staging/pre-production domain(s)
- Local development domain(s), if needed (`localhost`)

Example placeholders (replace with real values):
- `app.example.com`
- `staging.example.com`
- `localhost`

## Configuration steps

1. Open Firebase Console.
2. Go to **Authentication → Settings → Authorized domains**.
3. Add each approved domain explicitly.
4. Remove obsolete/non-owned domains.
5. Save and re-test sign-in on each environment.

## Verification gate (release checklist)

Before release:
- [ ] Production hostname present in authorized domains.
- [ ] Staging hostname present (if used).
- [ ] Old/unused domains removed.
- [ ] Google sign-in tested on each environment.

## Ownership note

This is an infra-console control and cannot be enforced from repository code.
Track completion in deployment checklist/PR notes for every environment rollout.
