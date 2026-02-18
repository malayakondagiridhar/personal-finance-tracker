import { auth } from './firebase'

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? '/api/v1'

interface ApiErrorPayload {
  title?: string
  detail?: string
  status?: number
  traceId?: string
}

export class ApiError extends Error {
  status: number
  traceId?: string

  constructor(message: string, status: number, traceId?: string) {
    super(message)
    this.status = status
    this.traceId = traceId
  }
}

async function parseError(response: Response): Promise<ApiError> {
  const fallback = `Request failed with status ${response.status}`

  try {
    const payload = (await response.json()) as ApiErrorPayload
    return new ApiError(payload.detail ?? payload.title ?? fallback, response.status, payload.traceId)
  } catch {
    return new ApiError(fallback, response.status)
  }
}

async function fetchWithToken(path: string, init?: RequestInit, forceRefresh = false): Promise<Response> {
  const token = await auth.currentUser?.getIdToken(forceRefresh)

  return fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
  })
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  let response = await fetchWithToken(path, init, false)

  if (response.status === 401 && auth.currentUser) {
    const tokenExpired = response.headers.get('x-token-expired') === 'true'

    if (tokenExpired) {
      response = await fetchWithToken(path, init, true)
    }

    if (response.status === 401) {
      window.dispatchEvent(new CustomEvent('pft:unauthorized'))
    }
  }

  if (!response.ok) {
    throw await parseError(response)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
