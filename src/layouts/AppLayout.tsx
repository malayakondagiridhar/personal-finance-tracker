import { useEffect } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useFirebaseAuth } from '../hooks/useFirebaseAuth'

export default function AppLayout() {
  const { user, logout } = useFirebaseAuth()
  const navigate = useNavigate()

  useEffect(() => {
    const onUnauthorized = async (event: Event) => {
      const customEvent = event as CustomEvent<{ reason?: string }>
      if (customEvent.detail?.reason !== 'expired-token') {
        return
      }

      await logout()
      navigate('/auth', { replace: true })
    }

    window.addEventListener('pft:unauthorized', onUnauthorized)
    return () => window.removeEventListener('pft:unauthorized', onUnauthorized)
  }, [logout, navigate])

  return (
    <div className="min-h-screen bg-gray-100">
      <header className="sticky top-0 z-10 border-b bg-white/95 backdrop-blur">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-2 px-3 py-3 sm:px-4">
          <Link to="/dashboard" className="text-base font-bold text-blue-700 sm:text-lg">Personal Finance Tracker</Link>
          <div className="flex items-center gap-2">
            <span className="max-w-[170px] truncate text-xs text-gray-600 sm:max-w-none sm:text-sm">{user?.email}</span>
            <button onClick={logout} className="rounded bg-gray-200 px-2.5 py-1 text-xs hover:bg-gray-300 sm:px-3 sm:text-sm">Sign out</button>
          </div>
        </div>

        <div className="mx-auto max-w-6xl px-2 pb-2 sm:hidden">
          <nav className="flex gap-1 overflow-x-auto rounded bg-gray-100 p-1">
            {[
              { to: '/dashboard', label: 'Dashboard' },
              { to: '/transactions', label: 'Transactions' },
              { to: '/budgets', label: 'Budgets' },
            ].map(item => (
              <NavLink key={item.to} to={item.to} className={({ isActive }) => `whitespace-nowrap rounded px-3 py-1.5 text-xs ${isActive ? 'bg-blue-600 text-white' : 'text-gray-700 hover:bg-white'}`}>
                {item.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>

      <div className="mx-auto grid max-w-6xl gap-4 px-3 py-4 sm:px-4 md:grid-cols-[220px_1fr]">
        <aside className="hidden rounded-lg bg-white p-2 shadow-sm md:block">
          <nav className="space-y-1">
            {[
              { to: '/dashboard', label: 'Dashboard' },
              { to: '/transactions', label: 'Transactions' },
              { to: '/budgets', label: 'Budgets' },
            ].map(item => (
              <NavLink key={item.to} to={item.to} className={({ isActive }) => `block rounded px-3 py-2 text-sm ${isActive ? 'bg-blue-600 text-white' : 'text-gray-700 hover:bg-gray-100'}`}>
                {item.label}
              </NavLink>
            ))}
          </nav>
        </aside>

        <main className="min-w-0">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
