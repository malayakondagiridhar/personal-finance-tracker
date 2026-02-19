import { Navigate } from 'react-router-dom'
import { useFirebaseAuth } from '../hooks/useFirebaseAuth'

export default function AuthPage() {
  const { user, loading, error, loginWithGoogle } = useFirebaseAuth()

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[#F0F4F8] p-6">
        <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
          <p className="text-center text-sm text-slate-500">Restoring authentication session...</p>
        </div>
      </div>
    )
  }

  if (user) return <Navigate to="/dashboard" replace />

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#F0F4F8] p-4">
      <div className="w-full max-w-2xl rounded-xl bg-white p-6 shadow-[0_1px_3px_rgba(0,0,0,0.08)] md:p-8">
        <div className="mb-4 flex justify-center">
          <div className="rounded-2xl bg-[#EAF1FF] p-4 text-[#3B6EF8]">
            <svg className="h-10 w-10" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
              <rect x="3" y="6" width="18" height="12" rx="2" />
              <path d="M8 12h8" />
              <path d="M8 9h4" />
            </svg>
          </div>
        </div>

        <h1 className="text-center text-2xl font-bold text-slate-900">Personal Finance Tracker</h1>
        <p className="mt-1 text-center text-sm text-slate-500">Plan smarter. Track every rupee. Build better money habits.</p>

        <div className="mt-6 grid gap-4 md:grid-cols-2">
          <section className="rounded-xl border border-slate-200 p-4">
            <h2 className="text-[18px] font-semibold text-slate-900">New user</h2>
            <p className="mt-1 text-sm text-slate-500">Use Google sign-in once. Your finance profile is auto-created on first authenticated API access.</p>
          </section>

          <section className="rounded-xl border border-slate-200 p-4">
            <h2 className="text-[18px] font-semibold text-slate-900">Returning user</h2>
            <p className="mt-1 text-sm text-slate-500">Continue with the same Google account to access your existing finance workspace.</p>
          </section>
        </div>

        {error && (
          <div className="mt-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-[#DC2626]">
            <p className="font-semibold">Sign-in failed</p>
            <p className="mt-1">{error}</p>
          </div>
        )}

        <button
          onClick={loginWithGoogle}
          className="mt-6 flex w-full items-center justify-center gap-2 rounded-xl bg-[#3B6EF8] py-2.5 text-sm font-semibold text-white transition-all duration-200 ease-in-out hover:bg-[#305cce]"
        >
          <svg className="h-4 w-4" viewBox="0 0 24 24" fill="currentColor">
            <path d="M21.35 11.1H12v2.98h5.36c-.23 1.5-1.7 4.4-5.36 4.4-3.23 0-5.86-2.67-5.86-5.96S8.77 6.56 12 6.56c1.84 0 3.07.78 3.78 1.46l2.58-2.48C16.73 4 14.56 3 12 3 6.97 3 2.89 7.03 2.89 12s4.08 9 9.11 9c5.26 0 8.75-3.7 8.75-8.92 0-.6-.07-1.05-.15-1.48Z" />
          </svg>
          Continue with Google
        </button>
      </div>
    </div>
  )
}
