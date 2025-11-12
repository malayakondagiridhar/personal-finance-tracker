import { useCallback } from 'react'
import { useLocalStorage } from './useLocalStorage'
import type { Transaction } from '../types'
import { v4 as uuidv4 } from 'uuid'

export function useTransactions() {
  const [transactions, setTransactions] = useLocalStorage<Transaction[]>('pft_transactions_v1', [])

  const addTransaction = useCallback((data: Omit<Transaction, 'id'>) => {
    const newTx: Transaction = { ...data, id: uuidv4() }
    setTransactions(prev => [newTx, ...prev])
  }, [setTransactions])

  const updateTransaction = useCallback((id: string, patch: Partial<Transaction>) => {
    setTransactions(prev =>
      prev.map(tx => (tx.id === id ? { ...tx, ...patch } : tx))
    )
  }, [setTransactions])

  const deleteTransaction = useCallback((id: string) => {
    setTransactions(prev => prev.filter(tx => tx.id !== id))
  }, [setTransactions])

  return { transactions, addTransaction, updateTransaction, deleteTransaction }
}
