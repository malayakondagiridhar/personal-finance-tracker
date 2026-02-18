import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Skeleton } from '../components/feedback/Skeleton'
import { useToast } from '../components/feedback/ToastProvider'
import { useTransactionsApi } from '../hooks/useTransactionsApi'
import { updateTransaction, type TransactionQueryParams } from '../services/transactionsApi'
import type { CreateTransactionRequest, TransactionDto, TransactionType } from '../types/api'

const currency = new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 2 })

interface FilterState {
  categoryId: string
  type: '' | `${TransactionType}`
  sortBy: NonNullable<TransactionQueryParams['sortBy']>
  sortDirection: NonNullable<TransactionQueryParams['sortDirection']>
}

export default function TransactionsPage() {
  const { transactions, categories, loading, error, addTransaction, removeTransaction, reload } = useTransactionsApi()
  const { notify } = useToast()

  const [form, setForm] = useState<CreateTransactionRequest>({
    categoryId: '',
    amount: 0,
    type: 1,
    transactionDateUtc: new Date().toISOString(),
    note: '',
  })
  const [filters, setFilters] = useState<FilterState>({
    categoryId: '',
    type: '',
    sortBy: 'transactionDateUtc',
    sortDirection: 'desc',
  })
  const [editing, setEditing] = useState<TransactionDto | null>(null)

  const canSubmit = useMemo(() => Boolean(form.categoryId && form.amount > 0 && form.transactionDateUtc), [form])

  const applyFilters = async () => {
    await reload({
      page: 1,
      pageSize: 50,
      categoryId: filters.categoryId || undefined,
      type: filters.type ? Number(filters.type) as TransactionType : undefined,
      sortBy: filters.sortBy,
      sortDirection: filters.sortDirection,
    })
  }

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!canSubmit) return
    try {
      await addTransaction(form)
      notify('Transaction added', 'success')
      setForm(prev => ({ ...prev, amount: 0, note: '' }))
      await applyFilters()
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Failed to add transaction', 'error')
    }
  }

  const onSaveEdit = async (event: FormEvent) => {
    event.preventDefault()
    if (!editing) return
    try {
      await updateTransaction(editing.id, {
        categoryId: editing.categoryId,
        amount: editing.amount,
        type: editing.type,
        transactionDateUtc: editing.transactionDateUtc,
        note: editing.note ?? '',
      })
      setEditing(null)
      notify('Transaction updated', 'success')
      await applyFilters()
    } catch (err) {
      notify(err instanceof Error ? err.message : 'Failed to update transaction', 'error')
    }
  }

  return (
    <div className="space-y-4">
      <section className="rounded-lg bg-white p-4 shadow-sm">
        <h2 className="mb-3 text-lg font-semibold text-gray-800">Add transaction</h2>
        <form onSubmit={onSubmit} className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <select value={form.categoryId} onChange={e => setForm(prev => ({ ...prev, categoryId: e.target.value }))} className="rounded border px-3 py-2 lg:col-span-2" required>
            <option value="">Select category</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
          <input type="number" min="0.01" step="0.01" value={form.amount || ''} onChange={e => setForm(prev => ({ ...prev, amount: Number(e.target.value) }))} placeholder="Amount" className="rounded border px-3 py-2" required />
          <select value={form.type} onChange={e => setForm(prev => ({ ...prev, type: Number(e.target.value) as TransactionType }))} className="rounded border px-3 py-2">
            <option value={0}>Income</option>
            <option value={1}>Expense</option>
          </select>
          <input type="date" value={form.transactionDateUtc.slice(0, 10)} onChange={e => setForm(prev => ({ ...prev, transactionDateUtc: new Date(`${e.target.value}T00:00:00.000Z`).toISOString() }))} className="rounded border px-3 py-2" required />
          <input value={form.note ?? ''} onChange={e => setForm(prev => ({ ...prev, note: e.target.value }))} placeholder="Note" className="rounded border px-3 py-2 lg:col-span-4" />
          <button type="submit" disabled={!canSubmit} className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50">Add</button>
        </form>
      </section>

      <section className="rounded-lg bg-white p-4 shadow-sm">
        <h2 className="mb-3 text-lg font-semibold text-gray-800">Transactions</h2>

        <div className="mb-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <select value={filters.categoryId} onChange={e => setFilters(prev => ({ ...prev, categoryId: e.target.value }))} className="rounded border px-3 py-2">
            <option value="">All categories</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
          <select value={filters.type} onChange={e => setFilters(prev => ({ ...prev, type: e.target.value as FilterState['type'] }))} className="rounded border px-3 py-2">
            <option value="">All types</option>
            <option value="0">Income</option>
            <option value="1">Expense</option>
          </select>
          <select value={filters.sortBy} onChange={e => setFilters(prev => ({ ...prev, sortBy: e.target.value as FilterState['sortBy'] }))} className="rounded border px-3 py-2">
            <option value="transactionDateUtc">Date</option>
            <option value="amount">Amount</option>
            <option value="createdAtUtc">Created</option>
          </select>
          <select value={filters.sortDirection} onChange={e => setFilters(prev => ({ ...prev, sortDirection: e.target.value as FilterState['sortDirection'] }))} className="rounded border px-3 py-2">
            <option value="desc">Desc</option>
            <option value="asc">Asc</option>
          </select>
          <button onClick={() => void applyFilters()} className="rounded bg-gray-800 px-4 py-2 text-white hover:bg-black">Apply</button>
        </div>

        {loading && <div className="space-y-2"><Skeleton className="h-8 w-full" /><Skeleton className="h-8 w-full" /><Skeleton className="h-8 w-full" /></div>}
        {error && <p className="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-600">{error}</p>}
        {!loading && !transactions.length && <p className="text-sm text-gray-500">No transactions found.</p>}

        <div className="overflow-x-auto">
          <table className="min-w-full border-collapse text-sm">
            <thead>
              <tr className="border-b text-left text-gray-500">
                <th className="py-2">Date</th>
                <th>Category</th>
                <th>Note</th>
                <th className="text-right">Amount</th>
                <th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map(transaction => (
                <tr key={transaction.id} className="border-b">
                  <td className="py-2">{new Date(transaction.transactionDateUtc).toLocaleDateString()}</td>
                  <td>{categories.find(c => c.id === transaction.categoryId)?.name ?? '-'}</td>
                  <td>{transaction.note || '-'}</td>
                  <td className={transaction.type === 0 ? 'text-right font-semibold text-green-600' : 'text-right font-semibold text-red-600'}>
                    {transaction.type === 0 ? '+' : '-'} {currency.format(transaction.amount)}
                  </td>
                  <td className="text-right">
                    <button onClick={() => setEditing(transaction)} className="mr-3 text-blue-600 hover:text-blue-800">Edit</button>
                    <button onClick={async () => {
                      await removeTransaction(transaction.id)
                      notify('Transaction deleted', 'success')
                    }} className="text-red-600 hover:text-red-800">Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {editing && (
        <div className="fixed inset-0 z-20 flex items-end justify-center bg-black/40 p-4 sm:items-center">
          <form onSubmit={onSaveEdit} className="w-full max-w-lg space-y-3 rounded-xl bg-white p-4 shadow-lg">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold">Edit transaction</h3>
              <button type="button" onClick={() => setEditing(null)} className="text-gray-500">✕</button>
            </div>
            <select value={editing.categoryId} onChange={e => setEditing(prev => prev ? { ...prev, categoryId: e.target.value } : prev)} className="w-full rounded border px-3 py-2">
              {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
            </select>
            <input type="number" min="0.01" step="0.01" value={editing.amount} onChange={e => setEditing(prev => prev ? { ...prev, amount: Number(e.target.value) } : prev)} className="w-full rounded border px-3 py-2" />
            <select value={editing.type} onChange={e => setEditing(prev => prev ? { ...prev, type: Number(e.target.value) as TransactionType } : prev)} className="w-full rounded border px-3 py-2">
              <option value={0}>Income</option>
              <option value={1}>Expense</option>
            </select>
            <input type="date" value={editing.transactionDateUtc.slice(0, 10)} onChange={e => setEditing(prev => prev ? { ...prev, transactionDateUtc: new Date(`${e.target.value}T00:00:00.000Z`).toISOString() } : prev)} className="w-full rounded border px-3 py-2" />
            <input value={editing.note ?? ''} onChange={e => setEditing(prev => prev ? { ...prev, note: e.target.value } : prev)} className="w-full rounded border px-3 py-2" />
            <div className="flex justify-end gap-2">
              <button type="button" onClick={() => setEditing(null)} className="rounded border px-3 py-2">Cancel</button>
              <button type="submit" className="rounded bg-blue-600 px-3 py-2 text-white">Save</button>
            </div>
          </form>
        </div>
      )}
    </div>
  )
}
