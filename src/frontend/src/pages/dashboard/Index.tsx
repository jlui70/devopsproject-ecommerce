import React from 'react';
import { Grid, Box, Typography, Divider, CircularProgress, Alert } from '@mui/material';
import Inventory2RoundedIcon from '@mui/icons-material/Inventory2Rounded';
import WarehouseRoundedIcon from '@mui/icons-material/WarehouseRounded';
import ShoppingCartRoundedIcon from '@mui/icons-material/ShoppingCartRounded';
import AttachMoneyRoundedIcon from '@mui/icons-material/AttachMoneyRounded';
import { StatCard } from '../../components/ui/StatCard';
import { useProduct } from '../../hooks/useProduct';
import { useStock } from '../../hooks/useStock';
import { useOrder } from '../../hooks/useOrder';
import { useReport } from '../../hooks/useReport';

const Dashboard: React.FC = () => {
  const { data: products = [], isLoading: loadingProducts, isError: errorProducts } = useProduct();
  const { data: stocks = [], isLoading: loadingStocks } = useStock();
  const { data: orders = [], isLoading: loadingOrders } = useOrder();
  const { data: reports = [], isLoading: loadingReports } = useReport();

  const totalRevenue = reports.reduce((acc, r) => acc + (r.totalSold ?? 0), 0);
  const totalStockQty = stocks.reduce((acc, s) => acc + (s.quantity ?? 0), 0);

  // Reports usa MongoDB (pode não estar disponível localmente) — não bloqueia o dashboard
  const isLoading = loadingProducts || loadingStocks || loadingOrders;

  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h5" sx={{ color: '#1e293b', fontWeight: 700 }}>
          Visão Geral
        </Typography>
        <Typography sx={{ color: '#64748b', fontSize: 14, mt: 0.5 }}>
          Resumo dos dados da sua loja em tempo real.
        </Typography>
      </Box>

      {errorProducts && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Não foi possível carregar alguns dados. Verifique se os microserviços estão ativos.
        </Alert>
      )}

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6} lg={3}>
            <StatCard
              title="Produtos Cadastrados"
              value={products.length}
              icon={<Inventory2RoundedIcon />}
              color="#2563eb"
              bgColor="#eff6ff"
              subtitle="produtos ativos"
            />
          </Grid>
          <Grid item xs={12} sm={6} lg={3}>
            <StatCard
              title="Itens em Estoque"
              value={totalStockQty}
              icon={<WarehouseRoundedIcon />}
              color="#7c3aed"
              bgColor="#f5f3ff"
              subtitle={`${stocks.length} produtos com estoque`}
            />
          </Grid>
          <Grid item xs={12} sm={6} lg={3}>
            <StatCard
              title="Pedidos Realizados"
              value={orders.length}
              icon={<ShoppingCartRoundedIcon />}
              color="#059669"
              bgColor="#ecfdf5"
              subtitle="total de pedidos"
            />
          </Grid>
          <Grid item xs={12} sm={6} lg={3}>
            <StatCard
              title="Receita Total"
              value={`R$ ${totalRevenue.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`}
              icon={<AttachMoneyRoundedIcon />}
              color="#d97706"
              bgColor="#fffbeb"
              subtitle="soma de todos os pedidos"
            />
          </Grid>
        </Grid>
      )}

      <Divider sx={{ my: 4 }} />

      <Box sx={{ mb: 2 }}>
        <Typography variant="h6" sx={{ color: '#1e293b', fontWeight: 600 }}>
          Microserviços ativos
        </Typography>
        <Typography sx={{ color: '#64748b', fontSize: 13, mt: 0.5 }}>
          Todos os serviços abaixo estão integrados via nginx proxy.
        </Typography>
      </Box>

      <Grid container spacing={2}>
        {[
          { name: 'Main API', path: '/main/swagger', desc: 'Produtos, Estoque, Relatórios' },
          { name: 'Order API', path: '/order/swagger', desc: 'Pedidos e Processamento' },
          { name: 'Identity API', path: '/identity/swagger', desc: 'Autenticação JWT' },
          { name: 'Health Checks', path: '/healthchecks/ui', desc: 'Monitoramento de saúde' },
          { name: 'Invoice Worker', path: '/invoice/swagger', desc: 'Geração de faturas (SQS)' },
          { name: 'Notificator Worker', path: '/notificator/swagger', desc: 'Notificações (SES/SNS)' },
        ].map((svc) => (
          <Grid item xs={12} sm={6} lg={4} key={svc.name}>
            <Box
              component="a"
              href={svc.path}
              target="_blank"
              rel="noreferrer"
              sx={{
                display: 'block',
                p: 2,
                borderRadius: 2,
                border: '1px solid #e2e8f0',
                backgroundColor: '#fff',
                textDecoration: 'none',
                transition: 'border-color 0.15s, box-shadow 0.15s',
                '&:hover': {
                  borderColor: '#2563eb',
                  boxShadow: '0 0 0 3px rgba(37,99,235,0.08)',
                },
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                <Box
                  sx={{
                    width: 8,
                    height: 8,
                    borderRadius: '50%',
                    backgroundColor: '#10b981',
                    flexShrink: 0,
                  }}
                />
                <Typography sx={{ fontWeight: 600, fontSize: 14, color: '#1e293b' }}>
                  {svc.name}
                </Typography>
              </Box>
              <Typography sx={{ fontSize: 12, color: '#64748b', pl: 2.25 }}>
                {svc.desc}
              </Typography>
            </Box>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};

export default Dashboard;
