import { apiFetch } from '../lib/apiClient'

export type TransactionType = 0 | 1 // 0=Income, 1=Expense

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface CategoryDto {
  id: string
  userId: string
  name: string
  description?: string | null
  isDefault: boolean
}

export interface TransactionDto {
  id: string
  userId: string
  categoryId: string
  amount: number
  type: TransactionType
  transactionDateUtc: string
  note?: string | null
}

export interface CreateTransactionRequest {
  categoryId: string
  amount: number
  type: TransactionType
  transactionDateUtc: string
  note?: string
}

export interface TransactionQueryParams {
  page?: number
  pageSize?: number
  sortBy?: 'transactionDateUtc' | 'amount' | 'createdAtUtc'
  sortDirection?: 'asc' | 'desc'
  type?: TransactionType
  categoryId?: string
}

export async function getCategories() {
  return apiFetch<PagedResult<CategoryDto>>('/categories?page=1&pageSize=200&sortBy=name&sortDirection=asc')
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

export async function createTransaction(payload: CreateTransactionRequest) {
  return apiFetch<TransactionDto>('/transactions', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export async function deleteTransaction(id: string) {
  return apiFetch<void>(`/transactions/${id}`, { method: 'DELETE' })
}
