export type ID = string

export interface Transaction {
  id: ID
  title: string
  amount: number
  type: 'income' | 'expense'
  category: string
  date: string // ISO date format
  notes?: string
}
