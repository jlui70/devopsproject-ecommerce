import React, { useCallback, useEffect, useMemo, useState } from 'react';
import AddRoundedIcon from '@mui/icons-material/AddRounded';
import {
  MaterialReactTable,
  type MaterialReactTableProps,
  type MRT_ColumnDef,
  type MRT_Row,
} from 'material-react-table';
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import DeleteRoundedIcon from '@mui/icons-material/DeleteRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import { OrderEntity } from '../../types/entities';
import { getOrders, useCreateOrder, useDeleteOrder, useOrder, useUpdateOrder } from '../../hooks/useOrder';
import { useProduct } from '../../hooks/useProduct';
import { useQueryClient } from '@tanstack/react-query';

const STATUS_COLOR: Record<string, 'default' | 'warning' | 'success' | 'error'> = {
  Pendente:    'warning',
  Confirmado:  'success',
  Pending:     'warning',
  Processing:  'warning',
  Completed:   'success',
  Cancelled:   'error',
};

const Order: React.FC = () => {
  const { data: orders = [] } = useOrder();
  const { data: products = [] } = useProduct();
  const [changed, setChanged] = useState(false);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [tableData, setTableData] = useState<OrderEntity[]>(() => orders);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});
  const queryClient = useQueryClient();

  const toggle = () => setChanged((c) => !c);
  const createOrder = useCreateOrder({
    onSuccess: (newOrder) => {
      // Mostra imediatamente como Pendente enquanto Lambda processa
      setTableData((prev) => [...prev, newOrder]);
      // Inicia polling para detectar confirmação (Pendente → Confirmado)
      toggle();
    },
  });
  const updateOrder = useUpdateOrder({ onSuccess: toggle });
  const deleteOrder = useDeleteOrder({ onSuccess: toggle });

  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: ['order'] });
    queryClient.invalidateQueries({ queryKey: ['stock'] });
    queryClient.invalidateQueries({ queryKey: ['report'] });
    getOrders().then((res) => setTableData(res ?? []));
  }, [changed]);

  // Polling automático enquanto houver pedidos Pendente (Lambda processando)
  useEffect(() => {
    const hasPending = tableData.some((o) => o.status === 'Pendente');
    if (!hasPending) return;

    const interval = setInterval(async () => {
      const updated = await getOrders();
      setTableData(updated ?? []);
      if (!updated?.some((o) => o.status === 'Pendente')) {
        clearInterval(interval);
        queryClient.invalidateQueries({ queryKey: ['stock'] });
        queryClient.invalidateQueries({ queryKey: ['report'] });
      }
    }, 2000);

    return () => clearInterval(interval);
  }, [tableData]);

  const handleCreateNewRow = (values: { productId: number; quantity: number }) => {
    createOrder.mutate({ productId: values.productId, quantity: values.quantity });
  };

  const handleSaveRowEdits: MaterialReactTableProps<OrderEntity>['onEditingRowSave'] =
    async ({ exitEditingMode, values }) => {
      if (!Object.keys(validationErrors).length) {
        const current = orders.find((o) => o.id === Number(values.id));
        if (current) {
          updateOrder.mutate({
            id: current.id,
            body: { productId: current.product.id, quantity: Number(values.quantity) },
          });
        }
        exitEditingMode();
      }
    };

  const handleDeleteRow = useCallback(
    (row: MRT_Row<OrderEntity>) => {
      if (!confirm(`Deseja excluir o pedido #${row.getValue('id')}?`)) return;
      deleteOrder.mutate({ id: row.getValue('id') });
    },
    [tableData]
  );

  const columns = useMemo<MRT_ColumnDef<OrderEntity>[]>(
    () => [
      { accessorKey: 'id', header: 'Pedido #', enableEditing: false, size: 80 },
      { accessorKey: 'product.name', header: 'Produto', enableEditing: false, size: 180 },
      { accessorKey: 'quantity', header: 'Quantidade', size: 100 },
      {
        accessorKey: 'status',
        header: 'Status',
        enableEditing: false,
        size: 120,
        Cell: ({ cell }) => (
          <Chip
            label={cell.getValue<string>()}
            size="small"
            color={STATUS_COLOR[cell.getValue<string>()] ?? 'default'}
          />
        ),
      },
      { accessorKey: 'boughtBy', header: 'Comprado por', enableEditing: false, size: 160 },
    ],
    [validationErrors]
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 700, color: '#1e293b' }}>
            Pedidos
          </Typography>
          <Typography sx={{ color: '#64748b', fontSize: 13 }}>
            Gerencie os pedidos realizados pelos clientes
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddRoundedIcon />}
          onClick={() => setCreateModalOpen(true)}
          sx={{ boxShadow: '0 2px 8px rgba(37,99,235,0.3)' }}
        >
          Novo Pedido
        </Button>
      </Box>

      <MaterialReactTable
        columns={columns}
        data={tableData}
        editingMode="modal"
        enableEditing
        onEditingRowSave={handleSaveRowEdits}
        onEditingRowCancel={() => setValidationErrors({})}
        renderRowActions={({ row, table }) => (
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            <Tooltip title="Editar">
              <IconButton onClick={() => table.setEditingRow(row)} size="small">
                <EditRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Excluir">
              <IconButton onClick={() => handleDeleteRow(row)} size="small" color="error">
                <DeleteRoundedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        )}
        muiTablePaperProps={{
          sx: { borderRadius: 3, border: '1px solid #e2e8f0', boxShadow: 'none' },
        }}
        muiTableHeadCellProps={{ sx: { backgroundColor: '#f8fafc' } }}
      />

      <CreateOrderModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onSubmit={handleCreateNewRow}
        products={products}
      />
    </Box>
  );
};

interface CreateOrderModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: { productId: number; quantity: number }) => void;
  products: { id: number; name: string }[];
}

const CreateOrderModal: React.FC<CreateOrderModalProps> = ({ open, onClose, onSubmit, products }) => {
  const [productId, setProductId] = useState<number | ''>('');
  const [quantity, setQuantity] = useState('');

  const handleSubmit = () => {
    if (productId === '' || !quantity) return;
    onSubmit({ productId: Number(productId), quantity: Number(quantity) });
    setProductId('');
    setQuantity('');
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle sx={{ fontWeight: 700 }}>Novo Pedido</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <Box>
            <InputLabel sx={{ mb: 0.5, fontSize: 14, fontWeight: 500 }}>Produto</InputLabel>
            <Select
              fullWidth
              value={productId}
              onChange={(e) => setProductId(Number(e.target.value))}
              displayEmpty
            >
              <MenuItem value="" disabled>Selecione um produto</MenuItem>
              {products.map((p) => (
                <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>
              ))}
            </Select>
          </Box>
          <TextField
            label="Quantidade"
            required
            fullWidth
            type="number"
            inputProps={{ min: 1 }}
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
          />
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose} variant="outlined" color="inherit">
          Cancelar
        </Button>
        <Button onClick={handleSubmit} variant="contained" disabled={!productId || !quantity}>
          Criar Pedido
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default Order;
