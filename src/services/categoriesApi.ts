import { apiFetch } from '../lib/apiClient'
import type { CategoryDto, CreateCategoryRequest, PagedResult } from '../types/api'

export async function listCategories() {
  return apiFetch<PagedResult<CategoryDto>>('/categories?page=1&pageSize=200&sortBy=name&sortDirection=asc')
}

export async function createCategory(payload: CreateCategoryRequest) {
  return apiFetch<CategoryDto>('/categories', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}
