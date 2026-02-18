import { useCallback, useEffect, useState } from 'react'
import { listCategories } from '../services/categoriesApi'
import {
  createTransaction,
  deleteTransaction,
  getTransactions,
  type TransactionQueryParams,
} from '../services/transactionsApi'
import type { CategoryDto, CreateTransactionRequest, TransactionDto } from '../types/api'

export function useTransactionsApi() {
  const [transactions, setTransactions] = useState<TransactionDto[]>([])
  const [categories, setCategories] = useState<CategoryDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async (query?: TransactionQueryParams) => {
    setLoading(true)
    setError(null)
    try {
      const [categoriesResponse, transactionsResponse] = await Promise.all([
        listCategories(),
        getTransactions(query),
      ])
      setCategories(categoriesResponse.items)
      setTransactions(transactionsResponse.items)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load transactions')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  const addTransaction = useCallback(async (payload: CreateTransactionRequest) => {
    await createTransaction(payload)
    await load()
  }, [load])

  const removeTransaction = useCallback(async (id: string) => {
    await deleteTransaction(id)
    await load()
  }, [load])

  return {
    transactions,
    categories,
    loading,
    error,
    reload: load,
    addTransaction,
    removeTransaction,
  }
}
