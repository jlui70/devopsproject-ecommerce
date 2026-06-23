import React from 'react';
import { useLocation } from 'react-router-dom';
import { AppBar, Box, Toolbar, Typography, Chip } from '@mui/material';

const PAGE_TITLES: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/product': 'Produtos',
  '/stock': 'Estoque',
  '/order': 'Pedidos',
  '/report': 'Relatórios',
};

export const TopBar: React.FC = () => {
  const { pathname } = useLocation();
  const title = PAGE_TITLES[pathname] ?? 'DevOps Project Ecommerce';

  return (
    <AppBar
      position="fixed"
      elevation={0}
      sx={{
        left: 'var(--sidebar-width)',
        width: 'calc(100% - var(--sidebar-width))',
        backgroundColor: '#ffffff',
        borderBottom: '1px solid #e2e8f0',
        zIndex: 1100,
      }}
    >
      <Toolbar sx={{ minHeight: 'var(--topbar-height) !important', px: 3 }}>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h6" sx={{ color: '#1e293b', fontWeight: 700, fontSize: 18 }}>
            {title}
          </Typography>
        </Box>
        <Chip
          label="devopsproject.com.br"
          size="small"
          sx={{
            backgroundColor: '#eff6ff',
            color: '#2563eb',
            fontWeight: 600,
            fontSize: 11,
            border: '1px solid #bfdbfe',
          }}
        />
      </Toolbar>
    </AppBar>
  );
};
