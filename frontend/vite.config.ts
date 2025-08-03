import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import basicSsl from '@vitejs/plugin-basic-ssl'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    basicSsl() // Use Vite's basic SSL plugin for better HTTPS support
  ],
  server: {
    port: 3000,
    open: true,
    https: true
  },
  build: {
    outDir: 'build'
  }
})