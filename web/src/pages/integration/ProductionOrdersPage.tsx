import {
  Box,
  Button,
  Chip,
  IconButton,
  Snackbar,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ExportButton,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1IntegrationProductionOrders,
  postApiV1IntegrationSyncProductionOrders,
  getGetApiV1IntegrationProductionOrdersQueryKey,
} from '../../api/integration/integration';
import type { ProductionOrderDto } from '../../api/model/productionOrderDto';

// ─── Constants ────────────────────────────────────────────────────────────────

const STATUS_COLORS: Record<string, string> = {
  Released:  '#1D4ED8',
  Running:   '#D97706',
  Paused:    '#9333EA',
  Completed: '#15803D',
  Cancelled: '#B91C1C',
};

// ─── Component ────────────────────────────────────────────────────────────────

export default function ProductionOrdersPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [snackMsg, setSnackMsg] = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [filterProduct, setFilterProduct] = useState('');
  const [search, setSearch] = useState('');

  const { data, isLoading } = useGetApiV1IntegrationProductionOrders();
  const rows: ProductionOrderDto[] = data?.data ?? [];

  const syncMutation = useMutation({
    mutationFn: () => postApiV1IntegrationSyncProductionOrders(),
    onSuccess: (res) => {
      const r = res.data;
      setSnackMsg(`Sync complete — ${r?.created ?? 0} created, ${r?.updated ?? 0} updated`);
      queryClient.invalidateQueries({ queryKey: getGetApiV1IntegrationProductionOrdersQueryKey() });
    },
    onError: () => setSnackMsg('Sync failed. Check ERP connection settings.'),
  });

  const products = useMemo(() => [...new Set(rows.map((r) => r.productCode))].sort(), [rows]);
  const statuses = useMemo(() => [...new Set(rows.map((r) => r.status))].sort(), [rows]);

  const filtered = useMemo(() => {
    let r = rows;
    if (filterStatus)  r = r.filter((x) => x.status === filterStatus);
    if (filterProduct) r = r.filter((x) => x.productCode === filterProduct);
    if (search) r = r.filter((x) =>
      x.poCode.toLowerCase().includes(search.toLowerCase()) ||
      x.productCode.toLowerCase().includes(search.toLowerCase()),
    );
    return r;
  }, [rows, filterStatus, filterProduct, search]);

  const columns: GridColDef<ProductionOrderDto>[] = [
    {
      field: 'poCode',
      headerName: 'PO #',
      width: 148,
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600 }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'productCode',
      headerName: 'Product',
      width: 140,
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'targetQuantity',
      headerName: 'Target Qty',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => (
        <Typography variant="body2" sx={{ fontWeight: 500 }}>
          {Number(p.value).toLocaleString()} EA
        </Typography>
      ),
    },
    {
      field: 'plannedStartDate',
      headerName: 'Planned Start',
      width: 120,
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          {p.value ? new Date(p.value as string).toLocaleDateString() : '—'}
        </Typography>
      ),
    },
    {
      field: 'plannedEndDate',
      headerName: 'Planned End',
      width: 120,
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          {p.value ? new Date(p.value as string).toLocaleDateString() : '—'}
        </Typography>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => {
        const color = STATUS_COLORS[p.value as string] ?? '#94A3B8';
        return (
          <Chip
            label={p.value}
            size="small"
            sx={{
              bgcolor: alpha(color, 0.12),
              color,
              fontWeight: 600,
              fontSize: 11,
              border: `1px solid ${alpha(color, 0.25)}`,
            }}
          />
        );
      },
    },
    {
      field: 'syncedAt',
      headerName: 'Synced',
      width: 130,
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => (
        <Typography variant="caption" color="text.secondary">
          {new Date(p.value as string).toLocaleString()}
        </Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 52,
      sortable: false,
      filterable: false,
      renderCell: (p: GridRenderCellParams<ProductionOrderDto>) => (
        <Tooltip title="View detail">
          <IconButton size="small" onClick={() => navigate(`/integration/production-orders/${p.row.poid}`)}>
            <SolarIcon name="view" size={16} />
          </IconButton>
        </Tooltip>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Production Orders"
        subtitle="ERP-sourced manufacturing orders — read-only"
        breadcrumbs={[{ label: 'Integration' }, { label: 'Production Orders' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <ExportButton />
            <Button
              variant="contained"
              startIcon={<SolarIcon name="refresh" size={16} />}
              onClick={() => syncMutation.mutate()}
              disabled={syncMutation.isPending}
            >
              Sync from ERP
            </Button>
          </Stack>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search PO# or product…"
        filters={[
          {
            label: 'Product',
            value: filterProduct,
            options: products.map((c) => ({ label: c, value: c })),
            onChange: setFilterProduct,
            width: 180,
          },
          {
            label: 'Status',
            value: filterStatus,
            options: statuses.map((s) => ({ label: s, value: s })),
            onChange: setFilterStatus,
            width: 160,
          },
        ]}
        totalCount={filtered.length}
        actions={<RefreshButton loading={isLoading} onClick={() => queryClient.invalidateQueries({ queryKey: getGetApiV1IntegrationProductionOrdersQueryKey() })} />}
      />

      <Box sx={{ mt: 2 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          loading={isLoading}
          getRowId={(r) => String(r.poid)}
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
          disableRowSelectionOnClick
          rowHeight={52}
          sx={{
            border: '1px solid', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.paper',
            '& .MuiDataGrid-columnHeaders': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), borderBottom: '1px solid', borderColor: 'divider' },
            '& .MuiDataGrid-row:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.03) },
            '& .MuiDataGrid-cell:focus, & .MuiDataGrid-cell:focus-within': { outline: 'none' },
            '& .MuiDataGrid-columnHeader:focus, & .MuiDataGrid-columnHeader:focus-within': { outline: 'none' },
          }}
        />
      </Box>

      <Snackbar
        open={!!snackMsg}
        autoHideDuration={4000}
        onClose={() => setSnackMsg('')}
        message={snackMsg}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      />
    </PageRoot>
  );
}
