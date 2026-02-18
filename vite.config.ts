import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // Default to HTTPS backend endpoint to avoid 307 redirects that can drop Authorization headers.
  const proxyTarget = env.VITE_API_PROXY_TARGET || 'https://localhost:7121'
  const apiPrefix = env.VITE_API_VERSION_PREFIX || '/api/v1'

  return {
    plugins: [react()],
    server: {
      proxy: {
        [apiPrefix]: {
          target: proxyTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})
