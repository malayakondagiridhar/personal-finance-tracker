import { apiFetch } from '../lib/apiClient'
import type { BudgetStatusDto } from '../types/api'

export interface CreateBudgetRequest {
  categoryId: string
  year: number
  month: number
  limitAmount: number
}

export function getBudgetStatus(year: number, month: number) {
  return apiFetch<BudgetStatusDto[]>(`/budgets/status?year=${year}&month=${month}`)
}

export function createBudget(payload: CreateBudgetRequest) {
  return apiFetch('/budgets', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}
