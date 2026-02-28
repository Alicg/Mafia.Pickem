import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
// For development, we proxy API requests to the backend server running on localhost:7071
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:7071',
        changeOrigin: true,
      },
      '/blob': {
        target: 'http://127.0.0.1:10000',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/blob/, '/devstoreaccount1/match-states'),
      },
    },
  },
})
