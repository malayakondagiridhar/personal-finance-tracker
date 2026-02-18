import { auth } from './firebase'

const API_VERSION_PREFIX = import.meta.env.VITE_API_VERSION_PREFIX || '/api/v1'

function resolveApiBaseUrl() {
  const configured = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim()

  if (configured) {
    return configured.replace(/\/$/, '')
  }

  // In dev, default to relative API path so Vite proxy handles backend routing.
  // This avoids accidental calls to the frontend origin that return index.html.
  return API_VERSION_PREFIX
}

const API_BASE_URL = resolveApiBaseUrl()

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

  const contentType = response.headers.get('content-type')?.toLowerCase() ?? ''

  if (!contentType.includes('application/json')) {
    return new ApiError(
      'Received non-JSON response from API. Verify frontend API base URL / Vite proxy target.',
      response.status,
    )
  }

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
  let retriedExpiredToken = false

  if (response.status === 401 && auth.currentUser) {
    const tokenExpired = response.headers.get('x-token-expired') === 'true'

    if (tokenExpired) {
      retriedExpiredToken = true
      response = await fetchWithToken(path, init, true)
    }

    // Only force sign-out when expired-token refresh retry also fails.
    // For generic 401s (for example backend auth configuration mismatch),
    // keep session and let UI surface actionable error instead of auth-loop.
    if (response.status === 401 && retriedExpiredToken) {
      window.dispatchEvent(new CustomEvent('pft:unauthorized', { detail: { reason: 'expired-token' } }))
    }
  }

  if (!response.ok) {
    const error = await parseError(response)

    if (error.status === 401 && !retriedExpiredToken) {
      throw new ApiError(
        `${error.message} (If this persists after Google sign-in, verify backend Auth__FirebaseProjectId matches your Firebase project and API auth mode.)`,
        error.status,
        error.traceId,
      )
    }

    throw error
  }

  if (response.status === 204) {
    return undefined as T
  }

  const contentType = response.headers.get('content-type')?.toLowerCase() ?? ''
  if (!contentType.includes('application/json')) {
    throw new ApiError(
      'Received non-JSON response from API. Verify frontend API base URL / Vite proxy target.',
      response.status,
    )
  }

  return response.json() as Promise<T>
}
