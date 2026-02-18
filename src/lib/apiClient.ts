import { auth } from './firebase'

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? '/api/v1'

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const token = await auth.currentUser?.getIdToken()

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
  })

  if (!response.ok) {
    const fallback = `Request failed with status ${response.status}`
    try {
      const data = (await response.json()) as { detail?: string; title?: string }
      throw new ApiError(data.detail ?? data.title ?? fallback, response.status)
    } catch {
      throw new ApiError(fallback, response.status)
    }
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
