import { useEffect, useMemo, useState } from 'react'
import { CardSkeleton, Skeleton } from '../components/feedback/Skeleton'
import { getBudgetStatus } from '../services/budgetsApi'
import { getMonthlySummary } from '../services/summaryApi'
import { getTransactions } from '../services/transactionsApi'
import type { BudgetStatusDto, MonthlySummaryDto, TransactionDto } from '../types/api'

const currency = new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 })

interface TrendPoint {
  label: string
  income: number
  expense: number
}

export default function DashboardPage() {
  const [summary, setSummary] = useState<MonthlySummaryDto | null>(null)
  const [budgets, setBudgets] = useState<BudgetStatusDto[]>([])
  const [recent, setRecent] = useState<TransactionDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const run = async () => {
      setLoading(true)
      setError(null)
      try {
        const now = new Date()
        const [summaryData, budgetData, transactionData] = await Promise.all([
          getMonthlySummary(now.getUTCFullYear(), now.getUTCMonth() + 1),
          getBudgetStatus(now.getUTCFullYear(), now.getUTCMonth() + 1),
          getTransactions({ page: 1, pageSize: 100, sortDirection: 'desc' }),
        ])

        setSummary(summaryData)
        setBudgets(budgetData)
        setRecent(transactionData.items.slice(0, 8))
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load dashboard')
      } finally {
        setLoading(false)
      }
    }

    void run()
  }, [])

  const trendData = useMemo<TrendPoint[]>(() => {
    const map = new Map<string, TrendPoint>()

    recent.forEach(tx => {
      const d = new Date(tx.transactionDateUtc)
      const key = `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, '0')}`
      const label = d.toLocaleString('en-IN', { month: 'short' })
      const existing = map.get(key) ?? { label, income: 0, expense: 0 }
      if (tx.type === 0) existing.income += tx.amount
      else existing.expense += tx.amount
      map.set(key, existing)
    })

    return Array.from(map.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([, value]) => value)
  }, [recent])

  const maxTrend = Math.max(1, ...trendData.flatMap(t => [t.income, t.expense]))
  const maxCategory = Math.max(1, ...(summary?.categoryBreakdown.map(c => c.amount) ?? [1]))

  return (
    <div className="space-y-4">
      {loading && (
        <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          <CardSkeleton />
          <CardSkeleton />
          <CardSkeleton />
          <CardSkeleton />
        </section>
      )}
      {error && <p className="rounded border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <section className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard title="Total income" value={currency.format(summary?.totalIncome ?? 0)} color="text-green-600" />
        <StatCard title="Total expense" value={currency.format(summary?.totalExpense ?? 0)} color="text-red-600" />
        <StatCard title="Net savings" value={currency.format(summary?.netSavings ?? 0)} color="text-blue-600" />
        <StatCard title="Active budgets" value={String(budgets.length)} color="text-purple-600" />
      </section>

      <section className="grid gap-4 lg:grid-cols-2">
        <article className="rounded-lg bg-white p-4 shadow-sm">
          <h3 className="mb-3 text-base font-semibold text-gray-800">Monthly trends</h3>
          {loading ? <div className="space-y-2"><Skeleton className="h-3 w-full" /><Skeleton className="h-3 w-4/5" /><Skeleton className="h-3 w-3/5" /></div> : !trendData.length ? <p className="text-sm text-gray-500">Not enough data to render trends.</p> : (
            <div className="space-y-3">
              {trendData.map(point => (
                <div key={point.label}>
                  <p className="mb-1 text-xs text-gray-500">{point.label}</p>
                  <div className="space-y-1">
                    <Bar value={point.income} max={maxTrend} color="bg-green-500" label={`Income ${currency.format(point.income)}`} />
                    <Bar value={point.expense} max={maxTrend} color="bg-red-500" label={`Expense ${currency.format(point.expense)}`} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </article>

        <article className="rounded-lg bg-white p-4 shadow-sm">
          <h3 className="mb-3 text-base font-semibold text-gray-800">Category spend</h3>
          {!summary?.categoryBreakdown.length ? <p className="text-sm text-gray-500">No category spend data available.</p> : (
            <div className="space-y-2">
              {summary.categoryBreakdown.map(category => (
                <Bar
                  key={category.categoryId}
                  value={category.amount}
                  max={maxCategory}
                  color="bg-blue-500"
                  label={`${category.categoryName} • ${currency.format(category.amount)}`}
                />
              ))}
            </div>
          )}
        </article>
      </section>

      <section className="grid gap-4 lg:grid-cols-2">
        <article className="rounded-lg bg-white p-4 shadow-sm">
          <h3 className="mb-3 text-base font-semibold text-gray-800">Recent transactions</h3>
          <div className="space-y-2">
            {recent.length === 0 && <p className="text-sm text-gray-500">No recent transactions.</p>}
            {recent.map(tx => (
              <div key={tx.id} className="flex items-center justify-between rounded border p-2">
                <div>
                  <p className="text-sm font-medium text-gray-700">{new Date(tx.transactionDateUtc).toLocaleDateString()}</p>
                  <p className="text-xs text-gray-500">{tx.note || 'No note'}</p>
                </div>
                <span className={tx.type === 0 ? 'text-sm font-semibold text-green-600' : 'text-sm font-semibold text-red-600'}>
                  {tx.type === 0 ? '+' : '-'} {currency.format(tx.amount)}
                </span>
              </div>
            ))}
          </div>
        </article>

        <article className="rounded-lg bg-white p-4 shadow-sm">
          <h3 className="mb-3 text-base font-semibold text-gray-800">Budget highlights</h3>
          <div className="space-y-2">
            {budgets.length === 0 && <p className="text-sm text-gray-500">No budgets configured for this month.</p>}
            {budgets.map(budget => {
              const percent = budget.limitAmount > 0 ? Math.min(100, Math.round((budget.spentAmount / budget.limitAmount) * 100)) : 0
              const color = percent >= 90 ? 'bg-red-500' : percent >= 70 ? 'bg-amber-500' : 'bg-green-500'
              return (
                <div key={budget.budgetId} className="rounded border p-2">
                  <div className="mb-1 flex items-center justify-between">
                    <p className="text-sm font-medium text-gray-700">{budget.categoryName}</p>
                    <p className="text-xs text-gray-500">{percent}%</p>
                  </div>
                  <div className="h-2 rounded bg-gray-200">
                    <div className={`h-full rounded ${color}`} style={{ width: `${percent}%` }} />
                  </div>
                  <p className="mt-1 text-xs text-gray-500">{currency.format(budget.spentAmount)} / {currency.format(budget.limitAmount)}</p>
                </div>
              )
            })}
          </div>
        </article>
      </section>
    </div>
  )
}

function StatCard({ title, value, color }: { title: string; value: string; color: string }) {
  return (
    <article className="rounded-lg bg-white p-4 shadow-sm">
      <p className="text-xs uppercase tracking-wide text-gray-500">{title}</p>
      <p className={`mt-1 text-xl font-semibold ${color}`}>{value}</p>
    </article>
  )
}

function Bar({ value, max, color, label }: { value: number; max: number; color: string; label: string }) {
  const percent = Math.max(2, Math.round((value / max) * 100))
  return (
    <div>
      <div className="mb-1 flex justify-between text-xs text-gray-500"><span>{label}</span></div>
      <div className="h-2 rounded bg-gray-200">
        <div className={`h-full rounded ${color}`} style={{ width: `${percent}%` }} />
      </div>
    </div>
  )
}
