import { Navigate } from 'react-router-dom'
import { useFirebaseAuth } from '../hooks/useFirebaseAuth'

export default function AuthPage() {
  const { user, loading, error, loginWithGoogle } = useFirebaseAuth()

  if (loading) {
    return <div className="p-6 text-center text-gray-600">Checking authentication...</div>
  }

  if (user) {
    return <Navigate to="/dashboard" replace />
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-100 p-6">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-md">
        <h1 className="mb-2 text-2xl font-bold text-blue-700">Personal Finance Tracker</h1>
        <p className="mb-4 text-gray-600">Sign in to continue.</p>
        {error && <p className="mb-3 rounded border border-red-200 bg-red-50 p-2 text-sm text-red-600">{error}</p>}
        <button onClick={loginWithGoogle} className="w-full rounded-lg bg-blue-600 py-2 text-white hover:bg-blue-700">Continue with Google</button>
      </div>
    </div>
  )
}
