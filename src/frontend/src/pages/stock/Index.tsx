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
import { StockEntity } from '../../types/entities';
import { getStocks, useCreateStock, useDeleteStock, useStock, useUpdateStock } from '../../hooks/useStock';
import { useProduct } from '../../hooks/useProduct';
import { useQueryClient } from '@tanstack/react-query';

const Stock: React.FC = () => {
  const { data: stocks = [] } = useStock();
  const { data: products = [] } = useProduct();
  const [changed, setChanged] = useState(false);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [tableData, setTableData] = useState<StockEntity[]>(() => stocks);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});
  const queryClient = useQueryClient();

  const toggle = () => setChanged((c) => !c);
  const createStock = useCreateStock({ onSuccess: toggle });
  const updateStock = useUpdateStock({ onSuccess: toggle });
  const deleteStock = useDeleteStock({ onSuccess: toggle });

  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: ['stock'] });
    getStocks().then((res) => setTableData(res ?? []));
  }, [changed]);

  const handleCreateNewRow = (values: { productId: number; quantity: number }) => {
    createStock.mutate({ id: values.productId, body: { quantity: values.quantity } });
  };

  const handleSaveRowEdits: MaterialReactTableProps<StockEntity>['onEditingRowSave'] =
    async ({ exitEditingMode, values }) => {
      if (!Object.keys(validationErrors).length) {
        const current = stocks.find((s) => s.id === Number(values.id));
        if (current) {
          updateStock.mutate({ id: current.product.id, body: { quantity: Number(values.quantity) } });
        }
        exitEditingMode();
      }
    };

  const handleDeleteRow = useCallback(
    (row: MRT_Row<StockEntity>) => {
      const name = row.original?.product?.name ?? String(row.getValue('id'));
      if (!confirm(`Deseja excluir o estoque de "${name}"?`)) return;
      deleteStock.mutate({ id: row.getValue('id') });
    },
    [tableData]
  );

  const columns = useMemo<MRT_ColumnDef<StockEntity>[]>(
    () => [
      { accessorKey: 'id', header: 'ID', enableEditing: false, size: 60 },
      {
        accessorKey: 'product.name',
        header: 'Produto',
        enableEditing: false,
        size: 200,
      },
      {
        accessorKey: 'quantity',
        header: 'Qtde. em Estoque',
        size: 130,
      },
    ],
    [validationErrors]
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 700, color: '#1e293b' }}>
            Estoque
          </Typography>
          <Typography sx={{ color: '#64748b', fontSize: 13 }}>
            Controle o estoque disponível por produto
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddRoundedIcon />}
          onClick={() => setCreateModalOpen(true)}
          sx={{ boxShadow: '0 2px 8px rgba(37,99,235,0.3)' }}
        >
          Adicionar Estoque
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

      <CreateStockModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onSubmit={handleCreateNewRow}
        products={products}
      />
    </Box>
  );
};

interface CreateStockModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: { productId: number; quantity: number }) => void;
  products: { id: number; name: string }[];
}

const CreateStockModal: React.FC<CreateStockModalProps> = ({ open, onClose, onSubmit, products }) => {
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
      <DialogTitle sx={{ fontWeight: 700 }}>Adicionar Estoque</DialogTitle>
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
          Adicionar
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default Stock;
