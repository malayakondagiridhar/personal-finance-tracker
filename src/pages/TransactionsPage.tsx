import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Skeleton } from '../components/feedback/Skeleton'
import { useToast } from '../components/feedback/ToastProvider'
import { useTransactionsApi } from '../hooks/useTransactionsApi'
import { toUserMessage } from '../lib/apiErrorHandling'
import { createCategory } from '../services/categoriesApi'
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
    type: 2,
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
  const [newCategoryName, setNewCategoryName] = useState('')

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
      notify(toUserMessage(err, 'Failed to add transaction'), 'error')
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
      notify(toUserMessage(err, 'Failed to update transaction'), 'error')
    }
  }

  const onCreateCategory = async () => {
    const name = newCategoryName.trim()
    if (!name) return

    try {
      const created = await createCategory({ name, description: null, isDefault: false })
      notify('Category created', 'success')
      setNewCategoryName('')
      await reload()
      setForm(prev => ({ ...prev, categoryId: created.id }))
    } catch (err) {
      notify(toUserMessage(err, 'Failed to create category'), 'error')
    }
  }

  const colorForCategory = (name: string) => {
    const palette = ['#3B6EF8', '#16A34A', '#DC2626', '#D97706', '#7C3AED', '#0891B2']
    let hash = 0
    for (let i = 0; i < name.length; i += 1) hash = name.charCodeAt(i) + ((hash << 5) - hash)
    return palette[Math.abs(hash) % palette.length]
  }

  return (
    <div className="space-y-5">
      <h1 className="text-2xl font-bold text-slate-900">Transactions</h1>

      <section className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
        <h2 className="mb-4 text-[18px] font-semibold text-slate-900">Add transaction</h2>

        <div className="mb-4 grid gap-2 sm:grid-cols-[1fr_auto]">
          <input value={newCategoryName} onChange={e => setNewCategoryName(e.target.value)} placeholder="Quick add category (e.g. Food)" className="rounded-xl border border-slate-200 px-3 py-2 text-sm" />
          <button type="button" onClick={() => void onCreateCategory()} className="rounded-xl border border-[#3B6EF8] px-4 py-2 text-sm font-medium text-[#3B6EF8] transition-all duration-200 ease-in-out hover:bg-[#EAF1FF]">Add category</button>
        </div>

        <form onSubmit={onSubmit} className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <select value={form.categoryId} onChange={e => setForm(prev => ({ ...prev, categoryId: e.target.value }))} className="rounded-xl border border-slate-200 px-3 py-2 text-sm lg:col-span-2" required>
            <option value="">Select category</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
          <input type="number" min="0.01" step="0.01" value={form.amount || ''} onChange={e => setForm(prev => ({ ...prev, amount: Number(e.target.value) }))} placeholder="Amount" className="rounded-xl border border-slate-200 px-3 py-2 text-sm" required />
          <select value={form.type} onChange={e => setForm(prev => ({ ...prev, type: Number(e.target.value) as TransactionType }))} className="rounded-xl border border-slate-200 px-3 py-2 text-sm">
            <option value={1}>Income</option>
            <option value={2}>Expense</option>
          </select>
          <input type="date" value={form.transactionDateUtc.slice(0, 10)} onChange={e => setForm(prev => ({ ...prev, transactionDateUtc: new Date(`${e.target.value}T00:00:00.000Z`).toISOString() }))} className="rounded-xl border border-slate-200 px-3 py-2 text-sm" required />
          <input value={form.note ?? ''} onChange={e => setForm(prev => ({ ...prev, note: e.target.value }))} placeholder="Note" className="rounded-xl border border-slate-200 px-3 py-2 text-sm lg:col-span-4" />
          <button type="submit" disabled={!canSubmit} className="rounded-xl bg-[#3B6EF8] px-4 py-2 text-sm font-semibold text-white transition-all duration-200 ease-in-out hover:bg-[#305cce] disabled:opacity-50">Add</button>
        </form>
      </section>

      <section className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
        <h2 className="mb-4 text-[18px] font-semibold text-slate-900">Transactions</h2>

        <div className="mb-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <select value={filters.categoryId} onChange={e => setFilters(prev => ({ ...prev, categoryId: e.target.value }))} className="rounded-xl border border-slate-200 px-3 py-2 text-sm">
            <option value="">All categories</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
          <select value={filters.type} onChange={e => setFilters(prev => ({ ...prev, type: e.target.value as FilterState['type'] }))} className="rounded-xl border border-slate-200 px-3 py-2 text-sm">
            <option value="">All types</option>
            <option value="1">Income</option>
            <option value="2">Expense</option>
          </select>
          <select value={filters.sortBy} onChange={e => setFilters(prev => ({ ...prev, sortBy: e.target.value as FilterState['sortBy'] }))} className="rounded-xl border border-slate-200 px-3 py-2 text-sm">
            <option value="transactionDateUtc">Date</option>
            <option value="amount">Amount</option>
            <option value="createdAtUtc">Created</option>
          </select>
          <select value={filters.sortDirection} onChange={e => setFilters(prev => ({ ...prev, sortDirection: e.target.value as FilterState['sortDirection'] }))} className="rounded-xl border border-slate-200 px-3 py-2 text-sm">
            <option value="desc">Desc</option>
            <option value="asc">Asc</option>
          </select>
          <button onClick={() => void applyFilters()} className="rounded-xl bg-slate-900 px-4 py-2 text-sm font-medium text-white transition-all duration-200 ease-in-out hover:bg-black">Apply</button>
        </div>

        {loading && <div className="space-y-2"><Skeleton className="h-10 w-full" /><Skeleton className="h-10 w-full" /><Skeleton className="h-10 w-full" /></div>}
        {error && <p className="mb-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-[#DC2626]">{error}</p>}
        {!loading && !transactions.length && <p className="text-sm text-slate-500">No transactions found.</p>}

        <div className="overflow-x-auto rounded-xl border border-slate-100">
          <table className="min-w-full border-collapse text-sm">
            <thead className="bg-slate-50">
              <tr className="text-left text-slate-500">
                <th className="px-3 py-2">Date</th>
                <th className="px-3 py-2">Category</th>
                <th className="px-3 py-2">Note</th>
                <th className="px-3 py-2 text-right">Amount</th>
                <th className="px-3 py-2 text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map(transaction => {
                const categoryName = categories.find(c => c.id === transaction.categoryId)?.name ?? '-'
                const categoryColor = colorForCategory(categoryName)

                return (
                  <tr key={transaction.id} className="border-t border-slate-100 transition-all duration-200 ease-in-out hover:bg-[#F6F9FF]">
                    <td className="px-3 py-2">{new Date(transaction.transactionDateUtc).toLocaleDateString()}</td>
                    <td className="px-3 py-2">
                      <span className="inline-flex items-center gap-2 rounded-full bg-slate-50 px-2.5 py-1 text-xs font-medium text-slate-600">
                        <span className="h-2 w-2 rounded-full" style={{ backgroundColor: categoryColor }} />
                        {categoryName}
                      </span>
                    </td>
                    <td className="px-3 py-2 text-slate-500">{transaction.note || '-'}</td>
                    <td className={transaction.type === 1 ? 'px-3 py-2 text-right font-semibold text-[#16A34A]' : 'px-3 py-2 text-right font-semibold text-[#DC2626]'}>
                      {transaction.type === 1 ? '+' : '-'} {currency.format(transaction.amount)}
                    </td>
                    <td className="px-3 py-2 text-right">
                      <button onClick={() => setEditing(transaction)} className="mr-2 inline-flex items-center gap-1 rounded-lg border border-slate-300 px-2.5 py-1.5 text-xs font-medium text-slate-600 transition-all duration-200 ease-in-out hover:bg-slate-100">
                        ✎ Edit
                      </button>
                      <button onClick={async () => {
                        try {
                          await removeTransaction(transaction.id)
                          notify('Transaction deleted', 'success')
                        } catch (err) {
                          notify(toUserMessage(err, 'Failed to delete transaction'), 'error')
                        }
                      }} className="inline-flex items-center rounded-lg px-2.5 py-1.5 text-xs font-semibold text-[#DC2626] transition-all duration-200 ease-in-out hover:bg-red-50">
                        Delete
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      </section>

      {editing && (
        <div className="fixed inset-0 z-20 flex items-end justify-center bg-black/40 p-4 sm:items-center">
          <form onSubmit={onSaveEdit} className="w-full max-w-lg space-y-3 rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
            <div className="flex items-center justify-between">
              <h3 className="text-[18px] font-semibold text-slate-900">Edit transaction</h3>
              <button type="button" onClick={() => setEditing(null)} className="text-slate-500">✕</button>
            </div>
            <select value={editing.categoryId} onChange={e => setEditing(prev => prev ? { ...prev, categoryId: e.target.value } : prev)} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm">
              {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
            </select>
            <input type="number" min="0.01" step="0.01" value={editing.amount} onChange={e => setEditing(prev => prev ? { ...prev, amount: Number(e.target.value) } : prev)} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm" />
            <select value={editing.type} onChange={e => setEditing(prev => prev ? { ...prev, type: Number(e.target.value) as TransactionType } : prev)} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm">
              <option value={1}>Income</option>
              <option value={2}>Expense</option>
            </select>
            <input type="date" value={editing.transactionDateUtc.slice(0, 10)} onChange={e => setEditing(prev => prev ? { ...prev, transactionDateUtc: new Date(`${e.target.value}T00:00:00.000Z`).toISOString() } : prev)} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm" />
            <input value={editing.note ?? ''} onChange={e => setEditing(prev => prev ? { ...prev, note: e.target.value } : prev)} className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm" />
            <div className="flex justify-end gap-2">
              <button type="button" onClick={() => setEditing(null)} className="rounded-xl border border-slate-200 px-3 py-2 text-sm">Cancel</button>
              <button type="submit" className="rounded-xl bg-[#3B6EF8] px-3 py-2 text-sm font-medium text-white">Save</button>
            </div>
          </form>
        </div>
      )}
    </div>
  )
}
