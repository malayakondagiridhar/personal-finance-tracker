import { Navigate } from 'react-router-dom'
import { useFirebaseAuth } from '../hooks/useFirebaseAuth'

export default function AuthPage() {
  const { user, loading, error, loginWithGoogle } = useFirebaseAuth()

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-100 p-6">
        <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-md">
          <p className="text-center text-gray-600">Restoring authentication session...</p>
        </div>
      </div>
    )
  }

  if (user) {
    return <Navigate to="/dashboard" replace />
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 p-4">
      <div className="w-full max-w-2xl rounded-2xl bg-white p-6 shadow-md md:p-8">
        <h1 className="text-2xl font-bold text-blue-700">Personal Finance Tracker</h1>
        <p className="mt-1 text-gray-600">Secure sign-in with Google. No separate password setup required.</p>

        <div className="mt-6 grid gap-4 md:grid-cols-2">
          <section className="rounded-xl border p-4">
            <h2 className="text-lg font-semibold text-gray-800">New user</h2>
            <p className="mt-1 text-sm text-gray-600">Use Google sign-in once. Your finance profile is auto-created on first authenticated API access.</p>
          </section>

          <section className="rounded-xl border p-4">
            <h2 className="text-lg font-semibold text-gray-800">Returning user</h2>
            <p className="mt-1 text-sm text-gray-600">Continue with the same Google account to access your existing finance workspace.</p>
          </section>
        </div>

        {error && (
          <div className="mt-4 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
            <p className="font-medium">Sign-in failed</p>
            <p className="mt-1">{error}</p>
            <p className="mt-2 text-xs">If this persists in production, verify Firebase authorized domains include this host.</p>
          </div>
        )}

        <button onClick={loginWithGoogle} className="mt-6 w-full rounded-lg bg-blue-600 py-2.5 font-medium text-white hover:bg-blue-700">
          Continue with Google
        </button>
      </div>
    </div>
  )
}
