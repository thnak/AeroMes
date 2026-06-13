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
  useGetApiV1IntegrationSalesOrders,
  postApiV1IntegrationSyncSalesOrders,
  getGetApiV1IntegrationSalesOrdersQueryKey,
} from '../../api/integration/integration';
import type { SalesOrderDto } from '../../api/model/salesOrderDto';

// ─── Constants ────────────────────────────────────────────────────────────────

const STATUS_COLORS: Record<string, string> = {
  Open:      '#94A3B8',
  Closed:    '#475569',
  Cancelled: '#DC2626',
};

const TODAY = new Date().toISOString().slice(0, 10);

// ─── Component ────────────────────────────────────────────────────────────────

export default function SalesOrdersPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [snackMsg, setSnackMsg] = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [search, setSearch] = useState('');

  const { data, isLoading } = useGetApiV1IntegrationSalesOrders();
  const rows: SalesOrderDto[] = data?.data ?? [];

  const syncMutation = useMutation({
    mutationFn: () => postApiV1IntegrationSyncSalesOrders(),
    onSuccess: (res) => {
      const r = res.data;
      setSnackMsg(`Sync complete — ${r?.created ?? 0} created, ${r?.updated ?? 0} updated`);
      queryClient.invalidateQueries({ queryKey: getGetApiV1IntegrationSalesOrdersQueryKey() });
    },
    onError: () => setSnackMsg('Sync failed. Check ERP connection settings.'),
  });

  const filtered = useMemo(() => {
    let r = rows;
    if (filterStatus) r = r.filter((x) => x.status === filterStatus);
    if (search) r = r.filter((x) =>
      x.soCode.toLowerCase().includes(search.toLowerCase()) ||
      (x.customerName ?? '').toLowerCase().includes(search.toLowerCase()),
    );
    return r;
  }, [rows, filterStatus, search]);

  const statuses = [...new Set(rows.map((r) => r.status))].sort();

  const columns: GridColDef<SalesOrderDto>[] = [
    {
      field: 'soCode',
      headerName: 'SO #',
      width: 148,
      renderCell: (p: GridRenderCellParams<SalesOrderDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600 }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'customerName',
      headerName: 'Customer',
      flex: 1,
      minWidth: 200,
      renderCell: (p: GridRenderCellParams<SalesOrderDto>) => (
        <Typography variant="body2" sx={{ fontWeight: 500 }}>{p.value ?? '—'}</Typography>
      ),
    },
    {
      field: 'orderDate',
      headerName: 'Order Date',
      width: 120,
      renderCell: (p: GridRenderCellParams<SalesOrderDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          {new Date(p.value as string).toLocaleDateString()}
        </Typography>
      ),
    },
    {
      field: 'deliveryDate',
      headerName: 'Delivery Date',
      width: 130,
      renderCell: (p: GridRenderCellParams<SalesOrderDto>) => {
        if (!p.value) return <Typography variant="body2" color="text.secondary">—</Typography>;
        const dateStr = new Date(p.value as string).toISOString().slice(0, 10);
        const pastDue = dateStr < TODAY && p.row.status !== 'Closed' && p.row.status !== 'Cancelled';
        return (
          <Typography
            variant="body2"
            sx={{ color: pastDue ? 'error.main' : 'text.primary', fontWeight: pastDue ? 600 : 400 }}
          >
            {new Date(p.value as string).toLocaleDateString()}
          </Typography>
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (p: GridRenderCellParams<SalesOrderDto>) => {
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
      renderCell: (p: GridRenderCellParams<SalesOrderDto>) => (
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
      renderCell: (p: GridRenderCellParams<SalesOrderDto>) => (
        <Tooltip title="View detail">
          <IconButton size="small" onClick={() => navigate(`/integration/sales-orders/${p.row.soid}`)}>
            <SolarIcon name="view" size={16} />
          </IconButton>
        </Tooltip>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Sales Orders"
        subtitle="ERP-sourced customer orders — read-only"
        breadcrumbs={[{ label: 'Integration' }, { label: 'Sales Orders' }]}
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
        searchPlaceholder="Search SO# or customer…"
        filters={[
          {
            label: 'Status',
            value: filterStatus,
            options: statuses.map((s) => ({ label: s, value: s })),
            onChange: setFilterStatus,
            width: 160,
          },
        ]}
        totalCount={filtered.length}
        actions={<RefreshButton loading={isLoading} onClick={() => queryClient.invalidateQueries({ queryKey: getGetApiV1IntegrationSalesOrdersQueryKey() })} />}
      />

      <Box sx={{ mt: 2 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          loading={isLoading}
          getRowId={(r) => String(r.soid)}
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
