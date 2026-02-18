import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { useTransactionsApi } from '../hooks/useTransactionsApi'
import type { CreateTransactionRequest, TransactionType } from '../services/transactionsApi'

const currency = new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 2 })

export default function TransactionsPage() {
  const { transactions, categories, loading, error, addTransaction, removeTransaction } = useTransactionsApi()

  const [form, setForm] = useState<CreateTransactionRequest>({
    categoryId: '',
    amount: 0,
    type: 1,
    transactionDateUtc: new Date().toISOString(),
    note: '',
  })

  const canSubmit = useMemo(() => Boolean(form.categoryId && form.amount > 0 && form.transactionDateUtc), [form])

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!canSubmit) return
    await addTransaction(form)
    setForm(prev => ({ ...prev, amount: 0, note: '' }))
  }

  return (
    <div className="space-y-4">
      <section className="rounded-lg bg-white p-4 shadow-sm">
        <h2 className="mb-3 text-lg font-semibold text-gray-800">Add transaction</h2>
        <form onSubmit={onSubmit} className="grid gap-3 md:grid-cols-5">
          <select value={form.categoryId} onChange={e => setForm(prev => ({ ...prev, categoryId: e.target.value }))} className="rounded border px-3 py-2 md:col-span-2" required>
            <option value="">Select category</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>

          <input type="number" min="0.01" step="0.01" value={form.amount || ''} onChange={e => setForm(prev => ({ ...prev, amount: Number(e.target.value) }))} placeholder="Amount" className="rounded border px-3 py-2" required />

          <select value={form.type} onChange={e => setForm(prev => ({ ...prev, type: Number(e.target.value) as TransactionType }))} className="rounded border px-3 py-2">
            <option value={0}>Income</option>
            <option value={1}>Expense</option>
          </select>

          <input type="date" value={form.transactionDateUtc.slice(0, 10)} onChange={e => setForm(prev => ({ ...prev, transactionDateUtc: new Date(`${e.target.value}T00:00:00.000Z`).toISOString() }))} className="rounded border px-3 py-2" required />

          <input value={form.note ?? ''} onChange={e => setForm(prev => ({ ...prev, note: e.target.value }))} placeholder="Note" className="rounded border px-3 py-2 md:col-span-4" />

          <button type="submit" disabled={!canSubmit} className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50">Add</button>
        </form>
      </section>

      <section className="rounded-lg bg-white p-4 shadow-sm">
        <h2 className="mb-3 text-lg font-semibold text-gray-800">Recent transactions</h2>
        {loading && <p className="text-sm text-gray-500">Loading transactions...</p>}
        {error && <p className="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-600">{error}</p>}
        {!loading && !transactions.length && <p className="text-sm text-gray-500">No transactions found.</p>}

        <div className="space-y-2">
          {transactions.map(transaction => (
            <div key={transaction.id} className="flex flex-wrap items-center justify-between gap-2 rounded border p-3">
              <div>
                <p className="font-medium text-gray-800">{categories.find(c => c.id === transaction.categoryId)?.name ?? 'Category'}</p>
                <p className="text-xs text-gray-500">{new Date(transaction.transactionDateUtc).toLocaleDateString()} • {transaction.note || 'No note'}</p>
              </div>
              <div className="flex items-center gap-3">
                <span className={transaction.type === 0 ? 'font-semibold text-green-600' : 'font-semibold text-red-600'}>
                  {transaction.type === 0 ? '+' : '-'} {currency.format(transaction.amount)}
                </span>
                <button onClick={() => void removeTransaction(transaction.id)} className="text-sm text-red-600 hover:text-red-800">Delete</button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
