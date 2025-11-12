import type { Transaction } from '../../types'

interface Props {
  transactions: Transaction[]
}

export default function SummaryBox({ transactions }: Props) {
  const income = transactions
    .filter(t => t.type === 'income')
    .reduce((sum, t) => sum + t.amount, 0)
  const expense = transactions
    .filter(t => t.type === 'expense')
    .reduce((sum, t) => sum + t.amount, 0)
  const balance = income - expense

  return (
    <div className="bg-white shadow-md rounded-xl p-4 flex justify-between max-w-md w-full">
      <div>
        <p className="text-sm text-gray-500">Income</p>
        <p className="text-lg font-bold text-green-600">₹{income}</p>
      </div>
      <div>
        <p className="text-sm text-gray-500">Expense</p>
        <p className="text-lg font-bold text-red-600">₹{expense}</p>
      </div>
      <div>
        <p className="text-sm text-gray-500">Balance</p>
        <p className="text-lg font-bold text-blue-600">₹{balance}</p>
      </div>
    </div>
  )
}
