import React from 'react';
import {
  Box,
  Button,
  TextField,
  Typography,
  Paper,
  InputAdornment,
  Alert,
  CircularProgress,
} from '@mui/material';
import LockRoundedIcon from '@mui/icons-material/LockRounded';
import EmailRoundedIcon from '@mui/icons-material/EmailRounded';
import { http } from '../../utils/HttpClient';
import { useNavigate } from 'react-router-dom';

const Login: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState('');

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setLoading(true);
    const data = new FormData(event.currentTarget);
    try {
      const response = await http.post('/identity/api/auth', {
        email: data.get('email'),
        password: data.get('password'),
      });
      localStorage.setItem('jwtToken', response.data.token);
      navigate('/dashboard');
    } catch {
      setError('Email ou senha inválidos. Tente novamente.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #0f172a 0%, #1e3a5f 50%, #0f172a 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        p: 2,
      }}
    >
      <Box sx={{ width: '100%', maxWidth: 420 }}>
        {/* Logo/Brand */}
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Box
            sx={{
              width: 64,
              height: 64,
              borderRadius: 3,
              background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              margin: '0 auto 16px',
              boxShadow: '0 8px 32px rgba(37,99,235,0.4)',
            }}
          >
            <Typography sx={{ color: '#fff', fontWeight: 900, fontSize: 28, lineHeight: 1 }}>
              D
            </Typography>
          </Box>
          <Typography
            sx={{ color: '#f1f5f9', fontWeight: 800, fontSize: 24, letterSpacing: '-0.02em' }}
          >
            DevOps Project
          </Typography>
          <Typography sx={{ color: '#94a3b8', fontSize: 14, mt: 0.5 }}>
            Ecommerce · devopsproject.com.br
          </Typography>
        </Box>

        {/* Card */}
        <Paper
          sx={{
            p: 4,
            borderRadius: 3,
            backgroundColor: '#ffffff',
            boxShadow: '0 24px 64px rgba(0,0,0,0.4)',
          }}
        >
          <Typography
            sx={{ fontWeight: 700, fontSize: 20, color: '#1e293b', mb: 0.5 }}
          >
            Entrar
          </Typography>
          <Typography sx={{ color: '#64748b', fontSize: 14, mb: 3 }}>
            Acesse o painel de gerenciamento
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email"
              name="email"
              autoComplete="email"
              autoFocus
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <EmailRoundedIcon sx={{ color: '#94a3b8', fontSize: 20 }} />
                  </InputAdornment>
                ),
              }}
              sx={{ mb: 1 }}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Senha"
              type="password"
              id="password"
              autoComplete="current-password"
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <LockRoundedIcon sx={{ color: '#94a3b8', fontSize: 20 }} />
                  </InputAdornment>
                ),
              }}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              disabled={loading}
              sx={{
                mt: 3,
                py: 1.5,
                fontSize: 15,
                fontWeight: 700,
                background: 'linear-gradient(135deg, #2563eb 0%, #7c3aed 100%)',
                boxShadow: '0 4px 14px rgba(37,99,235,0.4)',
                '&:hover': {
                  background: 'linear-gradient(135deg, #1d4ed8 0%, #6d28d9 100%)',
                },
              }}
            >
              {loading ? <CircularProgress size={20} color="inherit" /> : 'Entrar'}
            </Button>
          </Box>
        </Paper>

        <Typography sx={{ color: '#475569', textAlign: 'center', fontSize: 12, mt: 3 }}>
          © {new Date().getFullYear()} DevOps Project Ecommerce · devopsproject.com.br
        </Typography>
      </Box>
    </Box>
  );
};

export default Login;
