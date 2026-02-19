import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Skeleton } from '../components/feedback/Skeleton'
import { useToast } from '../components/feedback/ToastProvider'
import { toUserMessage } from '../lib/apiErrorHandling'
import { createBudget, getBudgetStatus } from '../services/budgetsApi'
import { createCategory, listCategories } from '../services/categoriesApi'
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
  const [newCategoryName, setNewCategoryName] = useState('')
  const { notify } = useToast()

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      const [categoriesResponse, budgetStatus] = await Promise.all([listCategories(), getBudgetStatus(year, month)])
      setCategories(categoriesResponse.items)
      setBudgets(budgetStatus)
    } catch (err) {
      setError(toUserMessage(err, 'Failed to load budgets'))
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
      notify('Budget saved', 'success')
      await load()
    } catch (err) {
      const message = toUserMessage(err, 'Failed to save budget')
      setError(message)
      notify(message, 'error')
    }
  }

  const onCreateCategory = async () => {
    const name = newCategoryName.trim()
    if (!name) return

    try {
      const created = await createCategory({ name, description: null, isDefault: false })
      notify('Category created', 'success')
      setNewCategoryName('')
      await load()
      setForm(prev => ({ ...prev, categoryId: created.id }))
    } catch (err) {
      const message = toUserMessage(err, 'Failed to create category')
      setError(message)
      notify(message, 'error')
    }
  }

  const activeCategoryId = editingBudget?.categoryId ?? form.categoryId
  const activeLabel = useMemo(() => categories.find(c => c.id === activeCategoryId)?.name ?? '', [activeCategoryId, categories])

  return (
    <div className="space-y-5">
      <h1 className="text-2xl font-bold text-slate-900">Budgets</h1>

      <section className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
        <h2 className="mb-4 text-[18px] font-semibold text-slate-900">{editingBudget ? `Edit budget • ${activeLabel}` : 'Add budget'}</h2>

        <div className="mb-4 grid gap-2 sm:grid-cols-[1fr_auto]">
          <input value={newCategoryName} onChange={e => setNewCategoryName(e.target.value)} placeholder="Quick add category (e.g. Rent)" className="rounded-xl border border-slate-200 px-3 py-2 text-sm" />
          <button type="button" onClick={() => void onCreateCategory()} className="rounded-xl border border-[#3B6EF8] px-4 py-2 text-sm font-medium text-[#3B6EF8] transition-all duration-200 ease-in-out hover:bg-[#EAF1FF]">Add category</button>
        </div>

        <form onSubmit={onSubmit} className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <select
            value={editingBudget?.categoryId ?? form.categoryId}
            onChange={e => {
              setEditingBudget(null)
              setForm(prev => ({ ...prev, categoryId: e.target.value }))
            }}
            className="rounded-xl border border-slate-200 px-3 py-2 text-sm lg:col-span-2"
            required
          >
            <option value="">Select category</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
          <input
            type="number"
            min="1"
            step="1"
            value={(editingBudget?.limitAmount ?? form.limitAmount) || ''}
            onChange={e => {
              const value = Number(e.target.value)
              if (editingBudget) setEditingBudget({ ...editingBudget, limitAmount: value })
              else setForm(prev => ({ ...prev, limitAmount: value }))
            }}
            placeholder="Monthly limit"
            className="rounded-xl border border-slate-200 px-3 py-2 text-sm"
            required
          />
          <button className="rounded-xl bg-[#3B6EF8] px-4 py-2 text-sm font-semibold text-white transition-all duration-200 ease-in-out hover:bg-[#305cce]">
            {editingBudget ? 'Save changes' : 'Add budget'}
          </button>
        </form>
      </section>

      {loading && <div className="grid gap-2 sm:grid-cols-2"><Skeleton className="h-24 w-full" /><Skeleton className="h-24 w-full" /></div>}
      {error && <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-[#DC2626]">{error}</p>}

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {budgets.map(budget => {
          const percent = budget.limitAmount > 0 ? Math.min(100, Math.round((budget.spentAmount / budget.limitAmount) * 100)) : 0
          const color = percent < 50 ? 'bg-[#16A34A]' : percent <= 80 ? 'bg-[#D97706]' : 'bg-[#DC2626]'

          return (
            <article key={budget.budgetId} className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)] transition-all duration-200 ease-in-out hover:-translate-y-0.5">
              <div className="mb-3 flex items-start justify-between gap-2">
                <h3 className="text-sm font-semibold text-slate-800">{budget.categoryName}</h3>
                <div className="flex items-center gap-2">
                  {percent >= 80 && <span className="text-[#D97706]">⚠</span>}
                  <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-semibold text-slate-600">{percent}%</span>
                </div>
              </div>

              <p className="text-sm text-slate-500">{currency.format(budget.spentAmount)} spent of {currency.format(budget.limitAmount)}</p>
              <div className="mt-3 h-2 rounded-full bg-slate-200">
                <div className={`h-full rounded-full ${color}`} style={{ width: `${percent}%` }} />
              </div>

              <div className="mt-3 flex items-center justify-between">
                <p className="text-sm text-slate-500">Remaining</p>
                <p className={budget.remainingAmount < 0 ? 'text-sm font-semibold text-[#DC2626]' : 'text-sm font-semibold text-slate-700'}>
                  {currency.format(budget.remainingAmount)}
                </p>
              </div>

              <button onClick={() => setEditingBudget(budget)} className="mt-4 rounded-lg border border-slate-300 px-2.5 py-1.5 text-xs font-medium text-slate-600 transition-all duration-200 ease-in-out hover:bg-slate-100">Edit</button>
            </article>
          )
        })}

        {!loading && budgets.length === 0 && (
          <div className="col-span-full rounded-xl border border-dashed border-slate-300 bg-white p-6 text-center text-sm text-slate-500 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
            No budgets yet for this month.
          </div>
        )}
      </section>
    </div>
  )
}
