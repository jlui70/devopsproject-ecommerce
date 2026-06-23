import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Proxy direto para cada serviço (sem depender do nginx).
// Os serviços expõem as mesmas rotas prefixadas: /main, /order, /identity, etc.
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 3000,
    proxy: {
      '/main':        { target: 'https://localhost:5000', changeOrigin: true, secure: false },
      '/order':       { target: 'https://localhost:5001', changeOrigin: true, secure: false },
      '/identity':    { target: 'https://localhost:5002', changeOrigin: true, secure: false },
      '/healthchecks':{ target: 'https://localhost:5003', changeOrigin: true, secure: false },
      '/invoice':     { target: 'https://localhost:5004', changeOrigin: true, secure: false },
      '/notificator': { target: 'https://localhost:5005', changeOrigin: true, secure: false },
    },
  },
});
