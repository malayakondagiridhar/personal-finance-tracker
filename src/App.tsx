import AddTransactionForm from './features/transactions/AddTransactionForm'
import TransactionList from './features/transactions/TransactionList'
import SummaryBox from './features/dashboard/SummaryBox'
import { useTransactions } from './hooks/useTransactions'
import { useFirebaseAuth } from './hooks/useFirebaseAuth'

function App() {
  const { user, loading, error, loginWithGoogle, logout } = useFirebaseAuth()
  const { transactions, addTransaction, deleteTransaction } = useTransactions()

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-100">
        <p className="text-gray-700">Checking authentication session...</p>
      </div>
    )
  }

  if (!user) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-100 p-6">
        <div className="bg-white shadow-md rounded-xl p-6 w-full max-w-md space-y-4">
          <h1 className="text-2xl font-bold text-blue-700">Personal Finance Tracker</h1>
          <p className="text-gray-600">
            Sign in with Google to access your finance workspace.
          </p>
          {error && (
            <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded p-2">
              {error}
            </p>
          )}
          <button
            type="button"
            onClick={loginWithGoogle}
            className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-all"
          >
            Continue with Google
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex flex-col items-center bg-gray-100 py-10">
      <div className="w-full max-w-md flex items-center justify-between mb-4">
        <h1 className="text-3xl font-bold text-blue-700">Personal Finance Tracker</h1>
        <button
          type="button"
          onClick={logout}
          className="text-sm bg-gray-200 hover:bg-gray-300 px-3 py-1 rounded"
        >
          Sign out
        </button>
      </div>

      <p className="text-sm text-gray-600 mb-4">Signed in as {user.email}</p>

      {error && (
        <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded p-2 mb-4 w-full max-w-md">
          {error}
        </p>
      )}

      <SummaryBox transactions={transactions} />

      <div className="mt-6">
        <AddTransactionForm onAdd={addTransaction} />
        <TransactionList
          transactions={transactions}
          onDelete={deleteTransaction}
        />
      </div>
    </div>
  )
}

export default App
