import type { Transaction } from '../../types'

interface Props {
  transactions: Transaction[]
  onDelete: (id: string) => void
}

export default function TransactionList({ transactions, onDelete }: Props) {
  if (!transactions.length)
    return <p className="text-gray-500 text-center mt-6">No transactions yet</p>

  return (
    <div className="mt-6 w-full max-w-md space-y-3">
      {transactions.map(tx => (
        <div
          key={tx.id}
          className="flex justify-between items-center bg-white shadow rounded-lg p-3"
        >
          <div>
            <p className="font-medium">{tx.title}</p>
            <p className="text-sm text-gray-500">{tx.category}</p>
          </div>
          <div className="flex items-center gap-3">
            <span
              className={`font-semibold ${
                tx.type === 'income' ? 'text-green-600' : 'text-red-600'
              }`}
            >
              {tx.type === 'income' ? '+' : '-'}₹{tx.amount}
            </span>
            <button
              onClick={() => onDelete(tx.id)}
              className="text-sm text-red-500 hover:text-red-700"
            >
              ✕
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
