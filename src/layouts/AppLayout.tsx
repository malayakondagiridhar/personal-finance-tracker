import { Link, NavLink, Outlet } from 'react-router-dom'
import { useFirebaseAuth } from '../hooks/useFirebaseAuth'

export default function AppLayout() {
  const { user, logout } = useFirebaseAuth()

  return (
    <div className="min-h-screen bg-gray-100">
      <header className="border-b bg-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
          <Link to="/dashboard" className="text-lg font-bold text-blue-700">Personal Finance Tracker</Link>
          <div className="flex items-center gap-3">
            <span className="hidden text-sm text-gray-600 sm:inline">{user?.email}</span>
            <button onClick={logout} className="rounded bg-gray-200 px-3 py-1 text-sm hover:bg-gray-300">Sign out</button>
          </div>
        </div>
      </header>

      <div className="mx-auto grid max-w-6xl gap-4 px-4 py-4 md:grid-cols-[220px_1fr]">
        <aside className="rounded-lg bg-white p-2 shadow-sm">
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

        <main>
          <Outlet />
        </main>
      </div>
    </div>
  )
}
