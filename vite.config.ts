import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const proxyTarget = env.VITE_API_PROXY_TARGET || 'http://localhost:5256'
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
