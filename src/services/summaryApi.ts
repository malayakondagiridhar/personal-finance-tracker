import { apiFetch } from '../lib/apiClient'
import type { MonthlySummaryDto } from '../types/api'

export function getMonthlySummary(year: number, month: number) {
  return apiFetch<MonthlySummaryDto>(`/summary/monthly?year=${year}&month=${month}`)
}
