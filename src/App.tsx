import AddTransactionForm from './features/transactions/AddTransactionForm'
import TransactionList from './features/transactions/TransactionList'
import SummaryBox from './features/dashboard/SummaryBox'
import { useTransactions } from './hooks/useTransactions'

function App() {
  const { transactions, addTransaction, deleteTransaction } = useTransactions()

  return (
    <div className="min-h-screen flex flex-col items-center bg-gray-100 py-10">
      <h1 className="text-3xl font-bold mb-6 text-blue-700">
        Personal Finance Tracker
      </h1>

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
