import React from 'react';
import { Outlet, useLocation, Navigate } from 'react-router-dom';
import { Layout } from './components/layout/Layout';

const PROTECTED_PATHS = ['/dashboard', '/product', '/stock', '/order', '/report'];

function App() {
  const { pathname } = useLocation();
  const isProtected = PROTECTED_PATHS.some((p) => pathname.startsWith(p));
  const hasToken = Boolean(localStorage.getItem('jwtToken'));

  if (isProtected && !hasToken) {
    return <Navigate to="/login" replace />;
  }

  if (!isProtected) {
    return <Outlet />;
  }

  return (
    <Layout>
      <Outlet />
    </Layout>
  );
}

export default App;
