import { useState } from 'react'
import type { Transaction } from '../../types'

interface Props {
  onAdd: (data: Omit<Transaction, 'id'>) => void
}

export default function AddTransactionForm({ onAdd }: Props) {
  const [form, setForm] = useState<Omit<Transaction, 'id'>>({
    title: '',
    amount: 0,
    type: 'expense',
    category: '',
    date: new Date().toISOString().split('T')[0],
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.title || !form.amount) return
    onAdd(form)
    setForm({ ...form, title: '', amount: 0 })
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-white shadow-md rounded-xl p-4 space-y-3 w-full max-w-md"
    >
      <h2 className="text-xl font-semibold text-gray-800 mb-2">
        Add Transaction
      </h2>

      <input
        type="text"
        placeholder="Title"
        value={form.title}
        onChange={e => setForm({ ...form, title: e.target.value })}
        className="w-full border rounded-lg px-3 py-2 focus:ring focus:ring-blue-200"
      />

      <input
        type="number"
        placeholder="Amount"
        value={form.amount || ''}
        onChange={e =>
          setForm({ ...form, amount: parseFloat(e.target.value) })
        }
        className="w-full border rounded-lg px-3 py-2 focus:ring focus:ring-blue-200"
      />

      <select
        value={form.type}
        onChange={e =>
          setForm({ ...form, type: e.target.value as 'income' | 'expense' })
        }
        className="w-full border rounded-lg px-3 py-2"
      >
        <option value="income">Income</option>
        <option value="expense">Expense</option>
      </select>

      <input
        type="text"
        placeholder="Category"
        value={form.category}
        onChange={e => setForm({ ...form, category: e.target.value })}
        className="w-full border rounded-lg px-3 py-2 focus:ring focus:ring-blue-200"
      />

      <input
        type="date"
        value={form.date}
        onChange={e => setForm({ ...form, date: e.target.value })}
        className="w-full border rounded-lg px-3 py-2"
      />

      <button
        type="submit"
        className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-all"
      >
        Add Transaction
      </button>
    </form>
  )
}
