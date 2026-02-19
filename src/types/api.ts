export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type TransactionType = 1 | 2

export interface CategoryDto {
  id: string
  userId: string
  name: string
  description?: string | null
  isDefault: boolean
}

export interface CreateCategoryRequest {
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

export type UpdateTransactionRequest = CreateTransactionRequest

export interface BudgetStatusDto {
  budgetId: string
  categoryId: string
  categoryName: string
  limitAmount: number
  spentAmount: number
  remainingAmount: number
}

export interface MonthlySummaryDto {
  userId: string
  year: number
  month: number
  totalIncome: number
  totalExpense: number
  netSavings: number
  categoryBreakdown: {
    categoryId: string
    categoryName: string
    amount: number
  }[]
}
