import { apiFetch } from '../lib/apiClient'
import type {
  CreateTransactionRequest,
  PagedResult,
  TransactionDto,
  TransactionType,
  UpdateTransactionRequest,
} from '../types/api'

export interface TransactionQueryParams {
  page?: number
  pageSize?: number
  sortBy?: 'transactionDateUtc' | 'amount' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  type?: TransactionType
  categoryId?: string
}

export async function getTransactions(params: TransactionQueryParams = {}) {
  const search = new URLSearchParams()
  search.set('page', String(params.page ?? 1))
  search.set('pageSize', String(params.pageSize ?? 20))
  search.set('sortBy', params.sortBy ?? 'transactionDateUtc')
  search.set('sortDirection', params.sortDirection ?? 'desc')
  if (typeof params.type === 'number') search.set('type', String(params.type))
  if (params.categoryId) search.set('categoryId', params.categoryId)

  return apiFetch<PagedResult<TransactionDto>>(`/transactions?${search.toString()}`)
}

export function createTransaction(payload: CreateTransactionRequest) {
  return apiFetch<TransactionDto>('/transactions', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export function updateTransaction(id: string, payload: UpdateTransactionRequest) {
  return apiFetch<TransactionDto>(`/transactions/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
}

export function deleteTransaction(id: string) {
  return apiFetch<void>(`/transactions/${id}`, { method: 'DELETE' })
}
