import { Component } from 'react'
import type { ErrorInfo, ReactNode } from 'react'

interface Props {
  children: ReactNode
}

interface State {
  hasError: boolean
  message: string
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = {
    hasError: false,
    message: '',
  }

  static getDerivedStateFromError(error: Error): State {
    return {
      hasError: true,
      message: error.message || 'Unexpected UI error',
    }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    console.error('Unhandled UI error', error, errorInfo)
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-screen items-center justify-center bg-gray-100 p-4">
          <div className="w-full max-w-lg rounded-lg border border-red-200 bg-white p-5 shadow-sm">
            <h1 className="text-lg font-semibold text-red-700">Something went wrong</h1>
            <p className="mt-2 text-sm text-gray-700">{this.state.message}</p>
            <button onClick={() => window.location.reload()} className="mt-4 rounded bg-red-600 px-4 py-2 text-white hover:bg-red-700">Reload app</button>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}
