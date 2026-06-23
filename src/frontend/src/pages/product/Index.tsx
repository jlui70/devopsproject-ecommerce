import React, { useCallback, useEffect, useMemo, useState } from 'react';
import AddRoundedIcon from '@mui/icons-material/AddRounded';
import {
  MaterialReactTable,
  type MaterialReactTableProps,
  type MRT_Cell,
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
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import DeleteRoundedIcon from '@mui/icons-material/DeleteRounded';
import EditRoundedIcon from '@mui/icons-material/EditRounded';
import { ProductEntity } from '../../types/entities';
import { getProducts, useProduct, useCreateProduct, useDeleteProduct, useUpdateProduct } from '../../hooks/useProduct';
import { useQueryClient } from '@tanstack/react-query';

const Product: React.FC = () => {
  const { data: products = [] } = useProduct();
  const [changed, setChanged] = useState(false);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [tableData, setTableData] = useState<ProductEntity[]>(() => products);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});
  const queryClient = useQueryClient();

  const toggle = () => setChanged((c) => !c);

  const createProduct = useCreateProduct({ onSuccess: toggle });
  const updateProduct = useUpdateProduct({ onSuccess: toggle });
  const deleteProduct = useDeleteProduct({ onSuccess: toggle });

  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: ['product'] });
    getProducts().then((res) => setTableData(res ?? []));
  }, [changed]);

  const handleCreateNewRow = (values: ProductEntity) => {
    createProduct.mutate({ name: values.name, price: Number(values.price) });
  };

  const handleSaveRowEdits: MaterialReactTableProps<ProductEntity>['onEditingRowSave'] =
    async ({ exitEditingMode, values }) => {
      if (!Object.keys(validationErrors).length) {
        const current = products.find((p) => p.id === Number(values.id));
        if (current) {
          updateProduct.mutate({ id: current.id, body: { name: values.name, price: Number(values.price) } });
        }
        exitEditingMode();
      }
    };

  const handleDeleteRow = useCallback(
    (row: MRT_Row<ProductEntity>) => {
      if (!confirm(`Deseja excluir o produto "${row.getValue('name')}"?`)) return;
      deleteProduct.mutate({ id: row.getValue('id') });
    },
    [tableData]
  );

  const columns = useMemo<MRT_ColumnDef<ProductEntity>[]>(
    () => [
      {
        accessorKey: 'id',
        header: 'ID',
        enableEditing: false,
        size: 60,
      },
      {
        accessorKey: 'name',
        header: 'Nome do Produto',
        size: 200,
        muiTableBodyCellEditTextFieldProps: ({ cell }: { cell: MRT_Cell<ProductEntity> }) => ({
          required: true,
          error: !!validationErrors[cell.id],
          helperText: validationErrors[cell.id],
          onBlur: (event: React.FocusEvent<HTMLInputElement>) => {
            const val = event.target.value;
            if (!val) setValidationErrors((e) => ({ ...e, [cell.id]: 'Campo obrigatório' }));
            else setValidationErrors((e) => { const n = { ...e }; delete n[cell.id]; return n; });
          },
        }),
      },
      {
        accessorKey: 'price',
        header: 'Preço (R$)',
        size: 120,
        Cell: ({ cell }) =>
          Number(cell.getValue()).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }),
      },
    ],
    [validationErrors]
  );

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 700, color: '#1e293b' }}>
            Produtos
          </Typography>
          <Typography sx={{ color: '#64748b', fontSize: 13 }}>
            Gerencie o catálogo de produtos da loja
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddRoundedIcon />}
          onClick={() => setCreateModalOpen(true)}
          sx={{ boxShadow: '0 2px 8px rgba(37,99,235,0.3)' }}
        >
          Novo Produto
        </Button>
      </Box>

      <MaterialReactTable
        columns={columns}
        data={tableData}
        editingMode="modal"
        enableColumnOrdering
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

      <CreateProductModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onSubmit={handleCreateNewRow}
      />
    </Box>
  );
};

interface CreateModalProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: ProductEntity) => void;
}

const CreateProductModal: React.FC<CreateModalProps> = ({ open, onClose, onSubmit }) => {
  const [values, setValues] = useState({ name: '', price: '' });

  const handleSubmit = () => {
    onSubmit({ id: 0, name: values.name, price: Number(values.price) });
    setValues({ name: '', price: '' });
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle sx={{ fontWeight: 700 }}>Novo Produto</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Nome do Produto"
            required
            fullWidth
            value={values.name}
            onChange={(e) => setValues({ ...values, name: e.target.value })}
          />
          <TextField
            label="Preço (R$)"
            required
            fullWidth
            type="number"
            inputProps={{ min: 0, step: '0.01' }}
            value={values.price}
            onChange={(e) => setValues({ ...values, price: e.target.value })}
          />
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose} variant="outlined" color="inherit">
          Cancelar
        </Button>
        <Button onClick={handleSubmit} variant="contained" disabled={!values.name || !values.price}>
          Criar Produto
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default Product;
