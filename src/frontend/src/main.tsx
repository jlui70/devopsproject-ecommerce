import React from 'react';
import ReactDOM from 'react-dom/client';
import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

import App from './App';
import Login from './pages/login/Index';
import Dashboard from './pages/dashboard/Index';
import Product from './pages/product/Index';
import Stock from './pages/stock/Index';
import Order from './pages/order/Index';
import Report from './pages/report/Index';

import { theme } from './theme';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 30_000,
    },
  },
});

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { index: true, element: <Navigate to="/login" replace /> },
      { path: 'login', element: <Login /> },
      { path: 'dashboard', element: <Dashboard /> },
      { path: 'product', element: <Product /> },
      { path: 'stock', element: <Stock /> },
      { path: 'order', element: <Order /> },
      { path: 'report', element: <Report /> },
      { path: '*', element: <Navigate to="/login" replace /> },
    ],
  },
]);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <RouterProvider router={router} />
      </ThemeProvider>
    </QueryClientProvider>
  </React.StrictMode>
);
// trigger: primeiro run da pipeline
// trigger: demo Teste 0
