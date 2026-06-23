import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  Typography,
  Tooltip,
} from '@mui/material';
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded';
import Inventory2RoundedIcon from '@mui/icons-material/Inventory2Rounded';
import WarehouseRoundedIcon from '@mui/icons-material/WarehouseRounded';
import ShoppingCartRoundedIcon from '@mui/icons-material/ShoppingCartRounded';
import BarChartRoundedIcon from '@mui/icons-material/BarChartRounded';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';

const NAV_ITEMS = [
  { label: 'Dashboard', path: '/dashboard', icon: <DashboardRoundedIcon /> },
  { label: 'Produtos', path: '/product', icon: <Inventory2RoundedIcon /> },
  { label: 'Estoque', path: '/stock', icon: <WarehouseRoundedIcon /> },
  { label: 'Pedidos', path: '/order', icon: <ShoppingCartRoundedIcon /> },
  { label: 'Relatórios', path: '/report', icon: <BarChartRoundedIcon /> },
];

export const Sidebar: React.FC = () => {
  const navigate = useNavigate();
  const { pathname } = useLocation();

  const handleLogout = () => {
    localStorage.removeItem('jwtToken');
    navigate('/login');
  };

  return (
    <Box
      sx={{
        width: 'var(--sidebar-width)',
        minHeight: '100vh',
        backgroundColor: '#0f172a',
        display: 'flex',
        flexDirection: 'column',
        position: 'fixed',
        top: 0,
        left: 0,
        zIndex: 1200,
        borderRight: 'none',
      }}
    >
      {/* Logo */}
      <Box
        sx={{
          px: 3,
          py: 3,
          display: 'flex',
          alignItems: 'center',
          gap: 1.5,
          borderBottom: '1px solid rgba(255,255,255,0.06)',
          minHeight: 'var(--topbar-height)',
        }}
      >
        <Box
          sx={{
            width: 36,
            height: 36,
            borderRadius: 2,
            background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0,
          }}
        >
          <Typography sx={{ color: '#fff', fontWeight: 800, fontSize: 16 }}>D</Typography>
        </Box>
        <Box>
          <Typography
            sx={{
              color: '#fff',
              fontWeight: 700,
              fontSize: 13,
              lineHeight: 1.2,
              letterSpacing: '-0.01em',
            }}
          >
            DevOps Project
          </Typography>
          <Typography sx={{ color: '#94a3b8', fontSize: 11 }}>Ecommerce</Typography>
        </Box>
      </Box>

      {/* Navigation */}
      <Box sx={{ px: 2, py: 2, flex: 1 }}>
        <Typography
          sx={{
            color: '#475569',
            fontSize: 11,
            fontWeight: 600,
            letterSpacing: '0.08em',
            textTransform: 'uppercase',
            px: 1,
            mb: 1,
          }}
        >
          Menu
        </Typography>
        <List disablePadding>
          {NAV_ITEMS.map((item) => {
            const isActive = pathname === item.path;
            return (
              <ListItemButton
                key={item.path}
                onClick={() => navigate(item.path)}
                sx={{
                  borderRadius: 2,
                  mb: 0.5,
                  px: 1.5,
                  py: 1,
                  backgroundColor: isActive ? 'rgba(37,99,235,0.18)' : 'transparent',
                  '&:hover': {
                    backgroundColor: isActive
                      ? 'rgba(37,99,235,0.22)'
                      : 'rgba(255,255,255,0.05)',
                  },
                  transition: 'background-color 0.15s',
                }}
              >
                <ListItemIcon
                  sx={{
                    minWidth: 36,
                    color: isActive ? '#3b82f6' : '#64748b',
                    '& svg': { fontSize: 20 },
                  }}
                >
                  {item.icon}
                </ListItemIcon>
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{
                    fontSize: 14,
                    fontWeight: isActive ? 600 : 400,
                    color: isActive ? '#e2e8f0' : '#94a3b8',
                  }}
                />
                {isActive && (
                  <Box
                    sx={{
                      width: 4,
                      height: 20,
                      borderRadius: 4,
                      backgroundColor: '#3b82f6',
                      ml: 1,
                    }}
                  />
                )}
              </ListItemButton>
            );
          })}
        </List>
      </Box>

      {/* Logout */}
      <Box sx={{ px: 2, pb: 2, borderTop: '1px solid rgba(255,255,255,0.06)', pt: 2 }}>
        <Tooltip title="Sair da conta" placement="right">
          <ListItemButton
            onClick={handleLogout}
            sx={{
              borderRadius: 2,
              px: 1.5,
              py: 1,
              '&:hover': { backgroundColor: 'rgba(239,68,68,0.1)' },
            }}
          >
            <ListItemIcon sx={{ minWidth: 36, color: '#64748b', '& svg': { fontSize: 20 } }}>
              <LogoutRoundedIcon />
            </ListItemIcon>
            <ListItemText
              primary="Sair"
              primaryTypographyProps={{ fontSize: 14, color: '#94a3b8' }}
            />
          </ListItemButton>
        </Tooltip>
      </Box>
    </Box>
  );
};
