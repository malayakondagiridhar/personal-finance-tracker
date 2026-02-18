import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Skeleton } from '../components/feedback/Skeleton'
import { createBudget, getBudgetStatus } from '../services/budgetsApi'
import { listCategories } from '../services/categoriesApi'
import type { BudgetStatusDto, CategoryDto } from '../types/api'

const currency = new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 })

export default function BudgetsPage() {
  const now = new Date()
  const [year] = useState(now.getUTCFullYear())
  const [month] = useState(now.getUTCMonth() + 1)

  const [categories, setCategories] = useState<CategoryDto[]>([])
  const [budgets, setBudgets] = useState<BudgetStatusDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [form, setForm] = useState({ categoryId: '', limitAmount: 0 })
  const [editingBudget, setEditingBudget] = useState<BudgetStatusDto | null>(null)

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      const [categoriesResponse, budgetStatus] = await Promise.all([
        listCategories(),
        getBudgetStatus(year, month),
      ])
      setCategories(categoriesResponse.items)
      setBudgets(budgetStatus)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load budgets')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault()
    try {
      setError(null)
      await createBudget({
        categoryId: form.categoryId,
        limitAmount: form.limitAmount,
        month,
        year,
      })
      setForm({ categoryId: '', limitAmount: 0 })
      setEditingBudget(null)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save budget')
    }
  }

  const activeCategoryId = editingBudget?.categoryId ?? form.categoryId
  const activeLabel = useMemo(() => categories.find(c => c.id === activeCategoryId)?.name ?? '', [activeCategoryId, categories])

  return (
    <div className="space-y-4">
      <section className="rounded-lg bg-white p-4 shadow-sm">
        <h2 className="mb-3 text-lg font-semibold text-gray-800">{editingBudget ? `Edit budget • ${activeLabel}` : 'Add budget'}</h2>
        <form onSubmit={onSubmit} className="grid gap-3 md:grid-cols-4">
          <select value={editingBudget?.categoryId ?? form.categoryId} onChange={e => {
            setEditingBudget(null)
            setForm(prev => ({ ...prev, categoryId: e.target.value }))
          }} className="rounded border px-3 py-2 md:col-span-2" required>
            <option value="">Select category</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
          <input type="number" min="1" step="1" value={(editingBudget?.limitAmount ?? form.limitAmount) || ''} onChange={e => {
            const value = Number(e.target.value)
            if (editingBudget) setEditingBudget({ ...editingBudget, limitAmount: value })
            else setForm(prev => ({ ...prev, limitAmount: value }))
          }} placeholder="Monthly limit" className="rounded border px-3 py-2" required />
          <button className="rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700">{editingBudget ? 'Save changes' : 'Add budget'}</button>
        </form>
        {editingBudget && (
          <p className="mt-2 text-xs text-gray-500">
            API currently supports create semantics. Editing a same category/month budget may return conflict unless backend upsert/update is enabled.
          </p>
        )}
      </section>

      {loading && <div className="grid gap-2 sm:grid-cols-2"><Skeleton className="h-20 w-full" /><Skeleton className="h-20 w-full" /></div>}
      {error && <p className="rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
        {budgets.map(budget => {
          const percent = budget.limitAmount > 0 ? Math.min(100, Math.round((budget.spentAmount / budget.limitAmount) * 100)) : 0
          const color = percent >= 90 ? 'bg-red-500' : percent >= 70 ? 'bg-amber-500' : 'bg-green-500'
          return (
            <article key={budget.budgetId} className="rounded-xl bg-white p-4 shadow-sm">
              <div className="mb-2 flex items-start justify-between">
                <h3 className="font-semibold text-gray-800">{budget.categoryName}</h3>
                <button onClick={() => setEditingBudget(budget)} className="text-sm text-blue-600 hover:text-blue-800">Edit</button>
              </div>
              <p className="text-xs text-gray-500">{currency.format(budget.spentAmount)} spent of {currency.format(budget.limitAmount)}</p>
              <div className="mt-2 h-2 rounded bg-gray-200">
                <div className={`h-full rounded ${color}`} style={{ width: `${percent}%` }} />
              </div>
              <div className="mt-2 flex items-center justify-between text-xs">
                <span className="text-gray-500">Remaining</span>
                <span className={budget.remainingAmount < 0 ? 'font-medium text-red-600' : 'font-medium text-gray-700'}>{currency.format(budget.remainingAmount)}</span>
              </div>
            </article>
          )
        })}

        {!loading && budgets.length === 0 && (
          <div className="col-span-full rounded-lg border border-dashed bg-white p-6 text-center text-sm text-gray-500">No budgets yet for this month.</div>
        )}
      </section>
    </div>
  )
}
