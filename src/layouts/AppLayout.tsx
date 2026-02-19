import { useEffect } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useFirebaseAuth } from '../hooks/useFirebaseAuth'

const navItems = [
  {
    to: '/dashboard',
    label: 'Dashboard',
    icon: (
      <svg className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M3 13h8V3H3zM13 21h8v-6h-8zM13 3v8h8V3zM3 21h8v-6H3z" />
      </svg>
    ),
  },
  {
    to: '/transactions',
    label: 'Transactions',
    icon: (
      <svg className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M7 7h10M7 12h10M7 17h6" />
        <rect x="3" y="4" width="18" height="16" rx="2" />
      </svg>
    ),
  },
  {
    to: '/budgets',
    label: 'Budgets',
    icon: (
      <svg className="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M4 6h16M4 12h16M4 18h10" />
        <path d="M18 16l2 2 4-4" transform="translate(-2 -2)" />
      </svg>
    ),
  },
]

export default function AppLayout() {
  const { user, logout } = useFirebaseAuth()
  const navigate = useNavigate()

  useEffect(() => {
    const onUnauthorized = async (event: Event) => {
      const customEvent = event as CustomEvent<{ reason?: string }>
      if (customEvent.detail?.reason !== 'expired-token') return
      await logout()
      navigate('/auth', { replace: true })
    }

    window.addEventListener('pft:unauthorized', onUnauthorized)
    return () => window.removeEventListener('pft:unauthorized', onUnauthorized)
  }, [logout, navigate])

  return (
    <div className="min-h-screen bg-[#F0F4F8]">
      <header className="sticky top-0 z-20 border-b border-slate-200 bg-white/95 backdrop-blur">
        <div className="mx-auto flex max-w-7xl flex-wrap items-center justify-between gap-3 px-4 py-3 sm:px-6">
          <Link to="/dashboard" className="text-lg font-bold text-[#3B6EF8] transition-all duration-200 ease-in-out hover:opacity-90">
            Personal Finance Tracker
          </Link>

          <div className="flex items-center gap-2">
            <span className="max-w-[190px] truncate text-xs text-slate-500 sm:max-w-none sm:text-sm">{user?.email}</span>
            <button
              onClick={logout}
              className="rounded-xl border border-slate-200 bg-white px-3 py-1.5 text-xs font-medium text-slate-700 shadow-[0_1px_3px_rgba(0,0,0,0.08)] transition-all duration-200 ease-in-out hover:bg-slate-50 sm:text-sm"
            >
              Sign out
            </button>
          </div>
        </div>
      </header>

      <div className="mx-auto grid max-w-7xl gap-4 px-4 py-5 sm:px-6 md:grid-cols-[240px_1fr]">
        <aside className="rounded-xl bg-white p-3 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
          <nav className="space-y-1.5">
            {navItems.map(item => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `group flex items-center gap-2 rounded-lg border-l-[3px] px-3 py-2.5 text-sm font-medium transition-all duration-200 ease-in-out ${
                    isActive
                      ? 'border-l-[#3B6EF8] bg-[#EAF1FF] text-[#1E3A8A]'
                      : 'border-l-transparent text-slate-600 hover:bg-slate-50 hover:text-slate-900'
                  }`
                }
              >
                <span className="opacity-80">{item.icon}</span>
                <span>{item.label}</span>
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
