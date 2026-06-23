import React, { useEffect, useMemo, useState } from 'react';
import { MaterialReactTable, type MRT_ColumnDef } from 'material-react-table';
import { Box, Typography } from '@mui/material';
import { ReportEntity } from '../../types/entities';
import { getReports, useReport } from '../../hooks/useReport';
import { useQueryClient } from '@tanstack/react-query';

const Report: React.FC = () => {
  const { data: reports = [] } = useReport();
  const [tableData, setTableData] = useState<ReportEntity[]>(() => reports);
  const queryClient = useQueryClient();

  useEffect(() => {
    queryClient.invalidateQueries({ queryKey: ['report'] });
    getReports().then((res) => setTableData(res ?? []));
  }, []);

  const columns = useMemo<MRT_ColumnDef<ReportEntity>[]>(
    () => [
      { accessorKey: 'productName', header: 'Produto', size: 200 },
      {
        accessorKey: 'totalOrders',
        header: 'Qtde. Pedidos',
        size: 130,
        Cell: ({ cell }) => cell.getValue<number>().toLocaleString('pt-BR'),
      },
      {
        accessorKey: 'totalOrdered',
        header: 'Unidades Pedidas',
        size: 150,
        Cell: ({ cell }) => cell.getValue<number>().toLocaleString('pt-BR'),
      },
      {
        accessorKey: 'averagePrice',
        header: 'Preço Médio',
        size: 130,
        Cell: ({ cell }) =>
          Number(cell.getValue()).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }),
      },
      {
        accessorKey: 'totalSold',
        header: 'Receita Total',
        size: 150,
        Cell: ({ cell }) =>
          Number(cell.getValue()).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }),
      },
    ],
    []
  );

  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h6" sx={{ fontWeight: 700, color: '#1e293b' }}>
          Relatórios de Vendas
        </Typography>
        <Typography sx={{ color: '#64748b', fontSize: 13 }}>
          Análise de desempenho por produto
        </Typography>
      </Box>

      <MaterialReactTable
        columns={columns}
        data={tableData}
        enableEditing={false}
        muiTablePaperProps={{
          sx: { borderRadius: 3, border: '1px solid #e2e8f0', boxShadow: 'none' },
        }}
        muiTableHeadCellProps={{ sx: { backgroundColor: '#f8fafc' } }}
      />
    </Box>
  );
};

export default Report;
