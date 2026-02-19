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
      if (tx.type === 1) existing.income += tx.amount
      else existing.expense += tx.amount
      map.set(key, existing)
    })

    return Array.from(map.entries()).sort(([a], [b]) => a.localeCompare(b)).map(([, value]) => value)
  }, [recent])

  const maxTrend = Math.max(1, ...trendData.flatMap(t => [t.income, t.expense]))
  const maxCategory = Math.max(1, ...(summary?.categoryBreakdown.map(c => c.amount) ?? [1]))

  return (
    <div className="space-y-5">
      <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>

      {loading && (
        <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <CardSkeleton />
          <CardSkeleton />
          <CardSkeleton />
          <CardSkeleton />
        </section>
      )}

      {error && <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-[#DC2626] shadow-[0_1px_3px_rgba(0,0,0,0.08)]">{error}</p>}

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard title="Total Income" value={currency.format(summary?.totalIncome ?? 0)} accent="#16A34A" trend="+12% vs last month" icon={<WalletIcon />} />
        <StatCard title="Total Expense" value={currency.format(summary?.totalExpense ?? 0)} accent="#DC2626" trend="+4% vs last month" icon={<DowntrendIcon />} />
        <StatCard title="Net Savings" value={currency.format(summary?.netSavings ?? 0)} accent="#16A34A" trend="+8% vs last month" icon={<SavingsIcon />} />
        <StatCard title="Active Budgets" value={String(budgets.length)} accent="#D97706" trend="Target tracking" icon={<TargetIcon />} />
      </section>

      <section className="grid gap-4 lg:grid-cols-2">
        <article className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
          <h2 className="mb-4 text-[18px] font-semibold text-slate-900">Monthly trends</h2>
          {loading ? (
            <div className="space-y-2"><Skeleton className="h-3 w-full" /><Skeleton className="h-3 w-4/5" /><Skeleton className="h-3 w-3/5" /></div>
          ) : !trendData.length ? (
            <p className="text-sm text-slate-500">Not enough data to render trends.</p>
          ) : (
            <div className="space-y-3">
              {trendData.map(point => (
                <div key={point.label}>
                  <p className="mb-1 text-xs uppercase tracking-wide text-slate-400">{point.label}</p>
                  <div className="space-y-2">
                    <Bar value={point.income} max={maxTrend} color="bg-[#16A34A]" label={`Income ${currency.format(point.income)}`} />
                    <Bar value={point.expense} max={maxTrend} color="bg-[#DC2626]" label={`Expense ${currency.format(point.expense)}`} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </article>

        <article className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
          <h2 className="mb-4 text-[18px] font-semibold text-slate-900">Category spend</h2>
          {!summary?.categoryBreakdown.length ? (
            <p className="text-sm text-slate-500">No category spend data available.</p>
          ) : (
            <div className="space-y-2">
              {summary.categoryBreakdown.map(category => (
                <Bar
                  key={category.categoryId}
                  value={category.amount}
                  max={maxCategory}
                  color="bg-[#3B6EF8]"
                  label={`${category.categoryName} • ${currency.format(category.amount)}`}
                />
              ))}
            </div>
          )}
        </article>
      </section>

      <section className="grid gap-4 lg:grid-cols-2">
        <article className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
          <h2 className="mb-4 text-[18px] font-semibold text-slate-900">Recent transactions</h2>
          <div className="space-y-2">
            {recent.length === 0 && <p className="text-sm text-slate-500">No recent transactions.</p>}
            {recent.map(tx => (
              <div key={tx.id} className="flex items-center justify-between rounded-xl border border-slate-100 p-3 transition-all duration-200 ease-in-out hover:bg-slate-50">
                <div>
                  <p className="text-sm font-medium text-slate-700">{new Date(tx.transactionDateUtc).toLocaleDateString()}</p>
                  <p className="text-sm text-slate-500">{tx.note || 'No note'}</p>
                </div>
                <span className={tx.type === 1 ? 'text-sm font-semibold text-[#16A34A]' : 'text-sm font-semibold text-[#DC2626]'}>
                  {tx.type === 1 ? '+' : '-'} {currency.format(tx.amount)}
                </span>
              </div>
            ))}
          </div>
        </article>

        <article className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)]">
          <h2 className="mb-4 text-[18px] font-semibold text-slate-900">Budget highlights</h2>
          <div className="space-y-2">
            {budgets.length === 0 && <p className="text-sm text-slate-500">No budgets configured for this month.</p>}
            {budgets.map(budget => {
              const percent = budget.limitAmount > 0 ? Math.min(100, Math.round((budget.spentAmount / budget.limitAmount) * 100)) : 0
              const color = percent < 50 ? 'bg-[#16A34A]' : percent <= 80 ? 'bg-[#D97706]' : 'bg-[#DC2626]'

              return (
                <div key={budget.budgetId} className="rounded-xl border border-slate-100 p-3">
                  <div className="mb-2 flex items-center justify-between gap-2">
                    <p className="text-sm font-medium text-slate-700">{budget.categoryName}</p>
                    <div className="flex items-center gap-2">
                      {percent >= 80 && <span className="text-[#D97706]">⚠</span>}
                      <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-semibold text-slate-600">{percent}%</span>
                    </div>
                  </div>
                  <div className="h-2 rounded-full bg-slate-200">
                    <div className={`h-full rounded-full ${color}`} style={{ width: `${percent}%` }} />
                  </div>
                  <p className="mt-2 text-sm text-slate-500">{currency.format(budget.spentAmount)} / {currency.format(budget.limitAmount)}</p>
                </div>
              )
            })}
          </div>
        </article>
      </section>
    </div>
  )
}

function StatCard({ title, value, accent, trend, icon }: { title: string; value: string; accent: string; trend: string; icon: React.ReactNode }) {
  return (
    <article className="rounded-xl bg-white p-5 shadow-[0_1px_3px_rgba(0,0,0,0.08)] transition-all duration-200 ease-in-out hover:-translate-y-0.5" style={{ borderTop: `3px solid ${accent}` }}>
      <div className="mb-3 flex items-center justify-between">
        <p className="text-xs uppercase tracking-wide text-slate-400">{title}</p>
        <span className="text-slate-500">{icon}</span>
      </div>
      <p className="text-[30px] font-bold leading-none" style={{ color: accent }}>{value}</p>
      <p className="mt-2 text-sm text-slate-500">{trend}</p>
    </article>
  )
}

function Bar({ value, max, color, label }: { value: number; max: number; color: string; label: string }) {
  const percent = Math.max(2, Math.round((value / max) * 100))
  return (
    <div>
      <div className="mb-1 flex justify-between text-sm text-slate-500"><span>{label}</span></div>
      <div className="h-2 rounded-full bg-slate-200">
        <div className={`h-full rounded-full ${color}`} style={{ width: `${percent}%` }} />
      </div>
    </div>
  )
}

function WalletIcon() {
  return <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"><rect x="3" y="6" width="18" height="12" rx="2" /><path d="M16 12h5" /><circle cx="16" cy="12" r="1" /></svg>
}

function DowntrendIcon() {
  return <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M4 7h16" /><path d="M7 17l4-4 3 3 6-6" /></svg>
}

function SavingsIcon() {
  return <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"><path d="M12 3v4" /><path d="M6 9h12" /><path d="M4 13h16v7H4z" /></svg>
}

function TargetIcon() {
  return <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8"><circle cx="12" cy="12" r="8" /><circle cx="12" cy="12" r="4" /><circle cx="12" cy="12" r="1" /></svg>
}
