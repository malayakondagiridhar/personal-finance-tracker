import { ApiError } from './apiClient'

export function toUserMessage(error: unknown, fallback: string) {
  if (error instanceof ApiError) {
    if (error.status === 409) {
      return error.message || 'Conflict detected. Refresh and retry.'
    }

    if (error.status === 429) {
      return 'Too many requests right now. Please wait a few seconds and retry.'
    }

    if (error.status === 401) {
      return 'Session expired or unauthorized. Please sign in again.'
    }

    if (error.status === 403) {
      return 'You do not have permission for this action.'
    }

    return error.message || fallback
  }

  if (error instanceof Error) {
    return error.message
  }

  return fallback
}
