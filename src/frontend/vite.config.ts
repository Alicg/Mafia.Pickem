import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
// For development, all browser traffic should stay on Vite origin and use proxy.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const getEnv = (key: string, fallback: string) => process.env[key] || env[key] || fallback

  // Dev proxy target: where Vite should forward browser /api requests.
  const apiProxyTarget = getEnv('VITE_DEV_PROXY_API_TARGET', 'http://localhost:7071')
  // Dev proxy target: where Vite should forward browser /blob requests.
  const blobProxyTarget = getEnv('VITE_DEV_PROXY_BLOB_TARGET', 'http://127.0.0.1:10000')
  const blobProxyAccount = getEnv('VITE_DEV_PROXY_BLOB_ACCOUNT', 'devstoreaccount1')
  const blobProxyContainer = getEnv('VITE_DEV_PROXY_BLOB_CONTAINER', 'match-states')
  const blobPrefix = `/${blobProxyAccount}/${blobProxyContainer}`

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          target: apiProxyTarget,
          changeOrigin: true,
        },
        '/blob': {
          target: blobProxyTarget,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/blob/, blobPrefix),
        },
      },
    },
  }
})
