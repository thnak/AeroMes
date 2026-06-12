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

interface ProductionOrder {
  id: string;
  poNo: string;
  soNo?: string;
  productCode: string;
  productName: string;
  orderedQty: number;
  confirmedQty: number;
  status: 'Planned' | 'Released' | 'In Progress' | 'Completed' | 'Cancelled';
  plannedStart: string;
  plannedEnd: string;
  priority: 1 | 2 | 3;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_PRODUCTION_ORDERS: ProductionOrder[] = [
  { id: '1',  poNo: 'PO-2026-0045', soNo: 'SO-2026-0091', productCode: 'FRM-A001', productName: 'Frame Assembly A',        orderedQty: 400,  confirmedQty: 400,  status: 'In Progress', plannedStart: '2026-05-10', plannedEnd: '2026-06-28', priority: 1 },
  { id: '2',  poNo: 'PO-2026-0046', soNo: 'SO-2026-0091', productCode: 'PNL-B002', productName: 'Panel Sub-assembly B',    orderedQty: 300,  confirmedQty: 300,  status: 'Released',    plannedStart: '2026-05-20', plannedEnd: '2026-06-24', priority: 1 },
  { id: '3',  poNo: 'PO-2026-0044', soNo: 'SO-2026-0090', productCode: 'SHT-C003', productName: 'Shaft Housing C',         orderedQty: 500,  confirmedQty: 480,  status: 'In Progress', plannedStart: '2026-05-05', plannedEnd: '2026-06-14', priority: 2 },
  { id: '4',  poNo: 'PO-2026-0043', soNo: 'SO-2026-0089', productCode: 'BRK-D004', productName: 'Bracket Set D',           orderedQty: 200,  confirmedQty: 200,  status: 'Completed',   plannedStart: '2026-04-25', plannedEnd: '2026-05-30', priority: 2 },
  { id: '5',  poNo: 'PO-2026-0047', soNo: 'SO-2026-0092', productCode: 'MTR-E005', productName: 'Motor Mount E',           orderedQty: 1200, confirmedQty: 1100, status: 'Planned',     plannedStart: '2026-06-01', plannedEnd: '2026-07-10', priority: 1 },
  { id: '6',  poNo: 'PO-2026-0048', soNo: 'SO-2026-0092', productCode: 'COV-F006', productName: 'Cover Plate F',           orderedQty: 1200, confirmedQty: 0,    status: 'Planned',     plannedStart: '2026-06-15', plannedEnd: '2026-07-12', priority: 3 },
  { id: '7',  poNo: 'PO-2026-0042', soNo: 'SO-2026-0088', productCode: 'GRD-G007', productName: 'Guard Assembly G',        orderedQty: 350,  confirmedQty: 350,  status: 'Completed',   plannedStart: '2026-04-20', plannedEnd: '2026-05-27', priority: 2 },
  { id: '8',  poNo: 'PO-2026-0049', soNo: 'SO-2026-0093', productCode: 'HNG-J010', productName: 'Hinge Assembly J',        orderedQty: 2000, confirmedQty: 1800, status: 'Planned',     plannedStart: '2026-06-20', plannedEnd: '2026-07-30', priority: 1 },
  { id: '9',  poNo: 'PO-2026-0041', soNo: 'SO-2026-0087', productCode: 'FRM-A001', productName: 'Frame Assembly A',        orderedQty: 600,  confirmedQty: 600,  status: 'Completed',   plannedStart: '2026-04-12', plannedEnd: '2026-05-24', priority: 2 },
  { id: '10', poNo: 'PO-2026-0040', soNo: undefined,       productCode: 'CAP-K011', productName: 'Capacitor Bank K',       orderedQty: 100,  confirmedQty: 0,    status: 'Cancelled',   plannedStart: '2026-04-01', plannedEnd: '2026-04-30', priority: 3 },
];

const STATUS_COLORS: Record<ProductionOrder['status'], string> = {
  'Planned':    '#94A3B8',
  'Released':   '#1D4ED8',
  'In Progress':'#D97706',
  'Completed':  '#15803D',
  'Cancelled':  '#B91C1C',
};

const PRIORITY_COLORS: Record<1 | 2 | 3, string> = {
  1: '#DC2626',
  2: '#D97706',
  3: '#1D4ED8',
};

const STATUSES: ProductionOrder['status'][] = ['Planned', 'Released', 'In Progress', 'Completed', 'Cancelled'];
const PRODUCTS = [...new Set(MOCK_PRODUCTION_ORDERS.map((o) => o.productCode))].sort();

// ─── Component ────────────────────────────────────────────────────────────────

export default function ProductionOrdersPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [snackOpen, setSnackOpen] = useState(false);
  const [filterStatus, setFilterStatus] = useState('');
  const [filterProduct, setFilterProduct] = useState('');
  const [search, setSearch] = useState('');

  const filtered = useMemo(() => {
    let rows = MOCK_PRODUCTION_ORDERS;
    if (filterStatus)  rows = rows.filter((r) => r.status === filterStatus);
    if (filterProduct) rows = rows.filter((r) => r.productCode === filterProduct);
    if (search)        rows = rows.filter((r) =>
      r.poNo.toLowerCase().includes(search.toLowerCase()) ||
      r.productName.toLowerCase().includes(search.toLowerCase()) ||
      (r.soNo ?? '').toLowerCase().includes(search.toLowerCase())
    );
    return rows;
  }, [filterStatus, filterProduct, search]);

  function handleSync() {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      setSnackOpen(true);
    }, 800);
  }

  const columns: GridColDef<ProductionOrder>[] = [
    {
      field: 'poNo',
      headerName: 'PO #',
      width: 148,
      renderCell: (p: GridRenderCellParams<ProductionOrder>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600 }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'soNo',
      headerName: 'SO #',
      width: 140,
      renderCell: (p: GridRenderCellParams<ProductionOrder>) => p.value ? (
        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', color: 'text.secondary' }}>
          {p.value}
        </Typography>
      ) : (
        <Typography variant="caption" color="text.disabled">—</Typography>
      ),
    },
    {
      field: 'productName',
      headerName: 'Product',
      flex: 1,
      minWidth: 180,
      renderCell: (p: GridRenderCellParams<ProductionOrder>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontWeight: 500 }}>{p.value}</Typography>
          <Typography variant="caption" color="text.secondary">{p.row.productCode}</Typography>
        </Stack>
      ),
    },
    {
      field: 'orderedQty',
      headerName: 'Qty (Ord/Conf)',
      width: 130,
      align: 'right',
      headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<ProductionOrder>) => (
        <Typography variant="body2">
          <Box component="span" sx={{ fontWeight: 600 }}>{p.row.orderedQty.toLocaleString()}</Box>
          <Box component="span" color="text.secondary"> / {p.row.confirmedQty.toLocaleString()}</Box>
        </Typography>
      ),
    },
    {
      field: 'priority',
      headerName: 'P',
      width: 60,
      renderCell: (p: GridRenderCellParams<ProductionOrder>) => {
        const pr = p.value as 1 | 2 | 3;
        const color = PRIORITY_COLORS[pr];
        return (
          <Chip
            label={`P${pr}`}
            size="small"
            sx={{ bgcolor: alpha(color, 0.12), color, fontWeight: 700, fontSize: 11, minWidth: 36 }}
          />
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (p: GridRenderCellParams<ProductionOrder>) => {
        const color = STATUS_COLORS[p.value as ProductionOrder['status']];
        return (
          <Chip
            label={p.value}
            size="small"
            sx={{ bgcolor: alpha(color, 0.12), color, fontWeight: 600, fontSize: 11 }}
          />
        );
      },
    },
    {
      field: 'plannedStart',
      headerName: 'Planned Start',
      width: 115,
    },
    {
      field: 'plannedEnd',
      headerName: 'Planned End',
      width: 110,
    },
    {
      field: 'actions',
      headerName: '',
      width: 52,
      sortable: false,
      filterable: false,
      renderCell: (p: GridRenderCellParams<ProductionOrder>) => (
        <Tooltip title="View detail">
          <IconButton size="small" onClick={() => navigate(`/integration/production-orders/${p.row.id}`)}>
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
        subtitle="ERP-sourced production orders — read-only"
        breadcrumbs={[{ label: 'Integration' }, { label: 'Production Orders' }]}
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
        searchPlaceholder="Search PO#, SO# or product…"
        filters={[
          {
            label: 'Status',
            value: filterStatus,
            options: STATUSES.map((s) => ({ label: s, value: s })),
            onChange: setFilterStatus,
            width: 150,
          },
          {
            label: 'Product',
            value: filterProduct,
            options: PRODUCTS.map((c) => ({ label: c, value: c })),
            onChange: setFilterProduct,
            width: 150,
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
        message="Sync initiated — 0 new production orders found"
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      />
    </PageRoot>
  );
}
