import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: '../Assets/dist',
    rollupOptions: {
      output: {
        entryFileNames: 'assets/[name].js',
        assetFileNames: `assets/[name].[ext]`
      }
    }
  }
})
