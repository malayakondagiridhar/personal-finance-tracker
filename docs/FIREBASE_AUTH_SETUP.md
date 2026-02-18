# Firebase Auth Setup (Free Tier)

## Goal
Configure Firebase Authentication (Google sign-in) for `personal-finance-tracker`.

## Completed setup (console)
- Firebase project created
- Google sign-in provider enabled
- Web app registered

## Required local environment values
Set in `.env` (or equivalent secure local env file):

- `VITE_FIREBASE_API_KEY`
- `VITE_FIREBASE_AUTH_DOMAIN`
- `VITE_FIREBASE_PROJECT_ID`
- `VITE_FIREBASE_APP_ID`
- `Auth__FirebaseProjectId` (backend)

## Security notes
- Firebase web config is public client configuration (not a server secret).
- Never commit service-account private keys.
- Restrict authorized domains in Firebase console for production.

## Next implementation steps
1. Add Firebase SDK initialization in frontend.
2. Implement Google sign-in/sign-out UI flow.
3. Attach Firebase ID token to API calls.
4. Configure backend to validate Firebase-issued JWT tokens.
5. Implement token refresh retry flow (`getIdToken(true)` on 401 expired response).

## Session/refresh reference
See `docs/AUTH_SESSION_STRATEGY.md` for backend expectations and 401 retry contract.
