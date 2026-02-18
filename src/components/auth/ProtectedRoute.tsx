import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useFirebaseAuth } from '../../hooks/useFirebaseAuth'

export default function ProtectedRoute() {
  const { user, loading } = useFirebaseAuth()
  const location = useLocation()

  if (loading) {
    return <div className="p-6 text-center text-gray-600">Checking session...</div>
  }

  if (!user) {
    return <Navigate to="/auth" replace state={{ from: location }} />
  }

  return <Outlet />
}
