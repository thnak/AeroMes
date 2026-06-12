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
import {
  ExportButton,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TableToolbar,
} from '../../components';

// ─── Types ────────────────────────────────────────────────────────────────────

interface SalesOrder {
  id: string;
  soNo: string;
  customerCode: string;
  customerName: string;
  orderDate: string;
  deliveryDate: string;
  status: 'Open' | 'Confirmed' | 'In Production' | 'Shipped' | 'Closed';
  totalItems: number;
  totalQty: number;
  currency: string;
  totalValue: number;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_SALES_ORDERS: SalesOrder[] = [
  { id: '1',  soNo: 'SO-2026-0091', customerCode: 'JAXA-001', customerName: 'JAXA Research',           orderDate: '2026-05-01', deliveryDate: '2026-06-30', status: 'In Production', totalItems: 3, totalQty: 900,  currency: 'JPY', totalValue: 4_500_000 },
  { id: '2',  soNo: 'SO-2026-0090', customerCode: 'HNI-002',  customerName: 'Honda Aircraft',          orderDate: '2026-04-28', deliveryDate: '2026-06-15', status: 'Confirmed',     totalItems: 2, totalQty: 500,  currency: 'JPY', totalValue: 2_800_000 },
  { id: '3',  soNo: 'SO-2026-0089', customerCode: 'SBY-003',  customerName: 'Subaru Aerospace',        orderDate: '2026-04-20', deliveryDate: '2026-05-31', status: 'Shipped',       totalItems: 1, totalQty: 200,  currency: 'USD', totalValue: 128_000   },
  { id: '4',  soNo: 'SO-2026-0092', customerCode: 'MIT-004',  customerName: 'Mitsubishi Heavy Ind.',   orderDate: '2026-05-03', deliveryDate: '2026-07-15', status: 'Open',          totalItems: 4, totalQty: 1200, currency: 'JPY', totalValue: 7_200_000 },
  { id: '5',  soNo: 'SO-2026-0088', customerCode: 'KAW-005',  customerName: 'Kawasaki Aerospace',      orderDate: '2026-04-15', deliveryDate: '2026-05-28', status: 'Closed',        totalItems: 2, totalQty: 350,  currency: 'JPY', totalValue: 1_960_000 },
  { id: '6',  soNo: 'SO-2026-0087', customerCode: 'FJT-006',  customerName: 'Fujitsu Aero Systems',    orderDate: '2026-04-10', deliveryDate: '2026-05-25', status: 'Shipped',       totalItems: 3, totalQty: 600,  currency: 'USD', totalValue: 384_000   },
  { id: '7',  soNo: 'SO-2026-0093', customerCode: 'IHI-007',  customerName: 'IHI Corporation',         orderDate: '2026-05-05', deliveryDate: '2026-08-01', status: 'Open',          totalItems: 5, totalQty: 2000, currency: 'JPY', totalValue: 9_800_000 },
  { id: '8',  soNo: 'SO-2026-0086', customerCode: 'NEC-008',  customerName: 'NEC Space Technologies',  orderDate: '2026-04-05', deliveryDate: '2026-05-20', status: 'Closed',        totalItems: 1, totalQty: 150,  currency: 'JPY', totalValue: 750_000   },
  { id: '9',  soNo: 'SO-2026-0094', customerCode: 'SUM-009',  customerName: 'Sumitomo Precision',      orderDate: '2026-05-08', deliveryDate: '2026-07-30', status: 'Confirmed',     totalItems: 3, totalQty: 780,  currency: 'USD', totalValue: 421_200   },
  { id: '10', soNo: 'SO-2026-0085', customerCode: 'MHI-010',  customerName: 'Mitsui Engineering',      orderDate: '2026-04-02', deliveryDate: '2026-05-15', status: 'Closed',        totalItems: 2, totalQty: 420,  currency: 'JPY', totalValue: 2_100_000 },
];

const STATUS_COLORS: Record<SalesOrder['status'], string> = {
  'Open':          '#94A3B8',
  'Confirmed':     '#1D4ED8',
  'In Production': '#D97706',
  'Shipped':       '#15803D',
  'Closed':        '#475569',
};

const CUSTOMERS = [...new Set(MOCK_SALES_ORDERS.map((o) => o.customerCode))].sort();
const STATUSES: SalesOrder['status'][] = ['Open', 'Confirmed', 'In Production', 'Shipped', 'Closed'];

const TODAY = '2026-06-12';

// ─── Component ────────────────────────────────────────────────────────────────

export default function SalesOrdersPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [snackOpen, setSnackOpen] = useState(false);
  const [filterCustomer, setFilterCustomer] = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [search, setSearch] = useState('');

  const filtered = useMemo(() => {
    let rows = MOCK_SALES_ORDERS;
    if (filterCustomer) rows = rows.filter((r) => r.customerCode === filterCustomer);
    if (filterStatus)   rows = rows.filter((r) => r.status === filterStatus);
    if (search)         rows = rows.filter((r) =>
      r.soNo.toLowerCase().includes(search.toLowerCase()) ||
      r.customerName.toLowerCase().includes(search.toLowerCase())
    );
    return rows;
  }, [filterCustomer, filterStatus, search]);

  function handleSync() {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      setSnackOpen(true);
    }, 800);
  }

  const columns: GridColDef<SalesOrder>[] = [
    {
      field: 'soNo',
      headerName: 'SO #',
      width: 148,
      renderCell: (p: GridRenderCellParams<SalesOrder>) => (
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
      renderCell: (p: GridRenderCellParams<SalesOrder>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontWeight: 500 }}>{p.value}</Typography>
          <Typography variant="caption" color="text.secondary">{p.row.customerCode}</Typography>
        </Stack>
      ),
    },
    {
      field: 'orderDate',
      headerName: 'Order Date',
      width: 110,
    },
    {
      field: 'deliveryDate',
      headerName: 'Delivery Date',
      width: 120,
      renderCell: (p: GridRenderCellParams<SalesOrder>) => {
        const pastDue = p.value < TODAY && p.row.status !== 'Shipped' && p.row.status !== 'Closed';
        return (
          <Typography
            variant="body2"
            sx={{ color: pastDue ? 'error.main' : 'text.primary', fontWeight: pastDue ? 600 : 400 }}
          >
            {p.value}
          </Typography>
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 145,
      renderCell: (p: GridRenderCellParams<SalesOrder>) => {
        const color = STATUS_COLORS[p.value as SalesOrder['status']];
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
      field: 'totalItems',
      headerName: 'Items',
      width: 70,
      align: 'right',
      headerAlign: 'right',
    },
    {
      field: 'totalQty',
      headerName: 'Qty',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<SalesOrder>) => (
        <Typography variant="body2" sx={{ fontWeight: 500 }}>
          {(p.value as number).toLocaleString()} EA
        </Typography>
      ),
    },
    {
      field: 'totalValue',
      headerName: 'Value',
      width: 140,
      align: 'right',
      headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<SalesOrder>) => (
        <Typography variant="body2">
          {p.row.currency} {(p.value as number).toLocaleString()}
        </Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 52,
      sortable: false,
      filterable: false,
      renderCell: (p: GridRenderCellParams<SalesOrder>) => (
        <Tooltip title="View detail">
          <IconButton size="small" onClick={() => navigate(`/integration/sales-orders/${p.row.id}`)}>
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
              onClick={handleSync}
              disabled={loading}
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
            label: 'Customer',
            value: filterCustomer,
            options: CUSTOMERS.map((c) => ({ label: c, value: c })),
            onChange: setFilterCustomer,
            width: 180,
          },
          {
            label: 'Status',
            value: filterStatus,
            options: STATUSES.map((s) => ({ label: s, value: s })),
            onChange: setFilterStatus,
            width: 160,
          },
        ]}
        totalCount={filtered.length}
        actions={<RefreshButton loading={loading} onClick={handleSync} />}
      />

      <Box sx={{ mt: 2 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          loading={loading}
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
        open={snackOpen}
        autoHideDuration={4000}
        onClose={() => setSnackOpen(false)}
        message="Sync initiated — 0 new orders found"
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      />
    </PageRoot>
  );
}
