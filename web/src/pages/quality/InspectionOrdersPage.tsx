import {
  Box,
  Button,
  Chip,
  Grid,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams, GridRowSelectionModel } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import {
  ConfirmDialog,
  EmptyState,
  ExportButton,
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TableToolbar,
} from '../../components';

// ─── Types ────────────────────────────────────────────────────────────────────

interface InspectionOrder {
  id: string;
  orderNo: string;
  type: 'Incoming' | 'In-Process' | 'Final' | 'Customer Return';
  productCode: string;
  productName: string;
  lotNo: string;
  sampleSize: number;
  inspectedQty: number;
  defects: number;
  result?: 'PASS' | 'FAIL' | 'CONDITIONAL';
  status: 'Pending' | 'In Progress' | 'Completed';
  inspector: string;
  createdAt: string;
  completedAt?: string;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_ORDERS: InspectionOrder[] = [
  { id: '1',  orderNo: 'IQ-2026-0120', type: 'Incoming',        productCode: 'FRM-A001', productName: 'Frame Assembly A',     lotNo: 'LOT-A1042', sampleSize: 50,  inspectedQty: 50, defects: 0, result: 'PASS',        status: 'Completed',   inspector: 'Nguyen Van A', createdAt: '2026-06-01', completedAt: '2026-06-01' },
  { id: '2',  orderNo: 'IQ-2026-0121', type: 'In-Process',      productCode: 'PNL-B002', productName: 'Panel Sub-assembly B', lotNo: 'LOT-B0891', sampleSize: 30,  inspectedQty: 30, defects: 2, result: 'CONDITIONAL', status: 'Completed',   inspector: 'Tran Thi B',  createdAt: '2026-06-02', completedAt: '2026-06-02' },
  { id: '3',  orderNo: 'IQ-2026-0122', type: 'Final',           productCode: 'SHT-C003', productName: 'Shaft Housing C',      lotNo: 'LOT-C0312', sampleSize: 100, inspectedQty: 78, defects: 5, result: undefined,     status: 'In Progress', inspector: 'Le Van C',    createdAt: '2026-06-03' },
  { id: '4',  orderNo: 'IQ-2026-0123', type: 'Incoming',        productCode: 'BRK-D004', productName: 'Bracket Set D',        lotNo: 'LOT-D2210', sampleSize: 80,  inspectedQty: 80, defects: 0, result: 'PASS',        status: 'Completed',   inspector: 'Nguyen Van A', createdAt: '2026-06-03', completedAt: '2026-06-03' },
  { id: '5',  orderNo: 'IQ-2026-0124', type: 'Final',           productCode: 'MTR-E005', productName: 'Motor Mount E',        lotNo: 'LOT-E0091', sampleSize: 50,  inspectedQty: 50, defects: 8, result: 'FAIL',        status: 'Completed',   inspector: 'Pham Thi D',  createdAt: '2026-06-04', completedAt: '2026-06-04' },
  { id: '6',  orderNo: 'IQ-2026-0125', type: 'In-Process',      productCode: 'COV-F006', productName: 'Cover Plate F',        lotNo: 'LOT-F0441', sampleSize: 25,  inspectedQty: 0,  defects: 0, result: undefined,     status: 'Pending',     inspector: 'Tran Thi B',  createdAt: '2026-06-05' },
  { id: '7',  orderNo: 'IQ-2026-0126', type: 'Customer Return', productCode: 'FRM-A001', productName: 'Frame Assembly A',     lotNo: 'LOT-A1038', sampleSize: 10,  inspectedQty: 10, defects: 3, result: 'FAIL',        status: 'Completed',   inspector: 'Le Van C',    createdAt: '2026-06-05', completedAt: '2026-06-05' },
  { id: '8',  orderNo: 'IQ-2026-0127', type: 'Incoming',        productCode: 'HNG-J010', productName: 'Hinge Assembly J',     lotNo: 'LOT-J0221', sampleSize: 60,  inspectedQty: 60, defects: 1, result: 'PASS',        status: 'Completed',   inspector: 'Nguyen Van A', createdAt: '2026-06-06', completedAt: '2026-06-06' },
  { id: '9',  orderNo: 'IQ-2026-0128', type: 'Final',           productCode: 'WHL-L012', productName: 'Wheel & Hub L',        lotNo: 'LOT-L0051', sampleSize: 40,  inspectedQty: 20, defects: 0, result: undefined,     status: 'In Progress', inspector: 'Pham Thi D',  createdAt: '2026-06-07' },
  { id: '10', orderNo: 'IQ-2026-0129', type: 'Incoming',        productCode: 'SPR-H008', productName: 'Sprocket Set H',       lotNo: 'LOT-H0881', sampleSize: 100, inspectedQty: 100,defects: 0, result: 'PASS',        status: 'Completed',   inspector: 'Tran Thi B',  createdAt: '2026-06-08', completedAt: '2026-06-08' },
  { id: '11', orderNo: 'IQ-2026-0130', type: 'In-Process',      productCode: 'CAP-K011', productName: 'Capacitor Bank K',     lotNo: 'LOT-K0131', sampleSize: 30,  inspectedQty: 30, defects: 1, result: 'CONDITIONAL', status: 'Completed',   inspector: 'Le Van C',    createdAt: '2026-06-09', completedAt: '2026-06-09' },
  { id: '12', orderNo: 'IQ-2026-0131', type: 'Final',           productCode: 'GRD-G007', productName: 'Guard Assembly G',     lotNo: 'LOT-G0022', sampleSize: 20,  inspectedQty: 0,  defects: 0, result: undefined,     status: 'Pending',     inspector: 'Nguyen Van A', createdAt: '2026-06-10' },
];

// ─── Color helpers ────────────────────────────────────────────────────────────

const TYPE_COLORS: Record<InspectionOrder['type'], string> = {
  'Incoming':        '#1D4ED8',
  'In-Process':      '#D97706',
  'Final':           '#15803D',
  'Customer Return': '#DC2626',
};

const RESULT_COLORS: Record<'PASS' | 'FAIL' | 'CONDITIONAL', string> = {
  PASS:        '#15803D',
  FAIL:        '#DC2626',
  CONDITIONAL: '#D97706',
};

const STATUS_COLORS: Record<InspectionOrder['status'], string> = {
  'Pending':     '#94A3B8',
  'In Progress': '#1D4ED8',
  'Completed':   '#15803D',
};

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function InspectionOrdersPage() {
  const [rows, setRows]               = useState<InspectionOrder[]>(MOCK_ORDERS);
  const [search, setSearch]           = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter]   = useState('');
  const [selection, setSelection]     = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]   = useState(false);
  const [drawerMode, setDrawerMode]   = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]   = useState<InspectionOrder | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<InspectionOrder | null>(null);
  const [saving, setSaving]           = useState(false);

  // Form state
  const [formType, setFormType]           = useState<InspectionOrder['type']>('Incoming');
  const [formProductCode, setFormProductCode] = useState('');
  const [formLotNo, setFormLotNo]         = useState('');
  const [formSampleSize, setFormSampleSize] = useState('');
  const [formInspector, setFormInspector] = useState('');

  const filtered = useMemo(() => {
    let r = rows;
    if (search)       r = r.filter((o) => o.orderNo.toLowerCase().includes(search.toLowerCase()) || o.productCode.toLowerCase().includes(search.toLowerCase()) || o.productName.toLowerCase().includes(search.toLowerCase()));
    if (statusFilter) r = r.filter((o) => o.status === statusFilter);
    if (typeFilter)   r = r.filter((o) => o.type === typeFilter);
    return r;
  }, [rows, search, statusFilter, typeFilter]);

  function openCreate() {
    setDrawerMode('create');
    setEditTarget(null);
    setFormType('Incoming');
    setFormProductCode('');
    setFormLotNo('');
    setFormSampleSize('');
    setFormInspector('');
    setDrawerOpen(true);
  }

  function openEdit(o: InspectionOrder) {
    setDrawerMode('edit');
    setEditTarget(o);
    setFormType(o.type);
    setFormProductCode(o.productCode);
    setFormLotNo(o.lotNo);
    setFormSampleSize(String(o.sampleSize));
    setFormInspector(o.inspector);
    setDrawerOpen(true);
  }

  function handleSave() {
    setSaving(true);
    setTimeout(() => {
      const newRow: Partial<InspectionOrder> = {
        type: formType,
        productCode: formProductCode,
        lotNo: formLotNo,
        sampleSize: Number(formSampleSize) || 0,
        inspector: formInspector,
      };
      if (drawerMode === 'create') {
        setRows((prev) => [
          ...prev,
          {
            id: String(Date.now()),
            orderNo: `IQ-2026-${String(Date.now()).slice(-4)}`,
            productName: formProductCode,
            inspectedQty: 0,
            defects: 0,
            status: 'Pending',
            createdAt: new Date().toISOString().slice(0, 10),
            ...newRow,
          } as InspectionOrder,
        ]);
      } else if (editTarget) {
        setRows((prev) => prev.map((o) => o.id === editTarget.id ? { ...o, ...newRow } : o));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((o) => o.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<InspectionOrder>[] = [
    {
      field: 'orderNo',
      headerName: 'Order #',
      width: 148,
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'type',
      headerName: 'Type',
      width: 140,
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => {
        const color = TYPE_COLORS[params.value as InspectionOrder['type']];
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }}
          />
        );
      },
    },
    {
      field: 'productCode',
      headerName: 'Product',
      width: 180,
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => (
        <Stack spacing={0}>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'primary.main', lineHeight: 1.3 }}>
            {params.row.productCode}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11, lineHeight: 1.3 }}>
            {params.row.productName}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'lotNo',
      headerName: 'Lot #',
      width: 110,
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, color: 'text.secondary' }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'inspectedQty',
      headerName: 'Sample',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          {params.row.inspectedQty}/{params.row.sampleSize}
        </Typography>
      ),
    },
    {
      field: 'defects',
      headerName: 'Defects',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => (
        <Typography
          variant="body2"
          sx={{ fontSize: 12, fontWeight: params.value > 0 ? 700 : 400, color: params.value > 0 ? '#DC2626' : 'text.secondary' }}
        >
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'result',
      headerName: 'Result',
      width: 110,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => {
        if (!params.value) return <Typography variant="body2" color="text.disabled" sx={{ fontSize: 11 }}>—</Typography>;
        const color = RESULT_COLORS[params.value as 'PASS' | 'FAIL' | 'CONDITIONAL'];
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 700, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }}
          />
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 110,
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => {
        const color = STATUS_COLORS[params.value as InspectionOrder['status']];
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }}
          />
        );
      },
    },
    { field: 'inspector', headerName: 'Inspector', width: 130 },
    {
      field: 'actions',
      headerName: '',
      width: 100,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<InspectionOrder>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View">
            <IconButton size="small" sx={{ color: 'text.secondary' }}>
              <SolarIcon name="view" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => openEdit(params.row)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton size="small" onClick={() => setDeleteTarget(params.row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
              <SolarIcon name="delete" size={16} />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Inspection Orders"
        subtitle="Manage incoming, in-process, final and customer return inspection orders"
        breadcrumbs={[{ label: 'Quality' }, { label: 'Inspection Orders' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={openCreate}
          >
            New Inspection Order
          </Button>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search order # or product…"
        filters={[
          {
            label: 'Status',
            value: statusFilter,
            options: [
              { label: 'Pending',     value: 'Pending' },
              { label: 'In Progress', value: 'In Progress' },
              { label: 'Completed',   value: 'Completed' },
            ],
            onChange: setStatusFilter,
          },
          {
            label: 'Type',
            value: typeFilter,
            options: [
              { label: 'Incoming',        value: 'Incoming' },
              { label: 'In-Process',      value: 'In-Process' },
              { label: 'Final',           value: 'Final' },
              { label: 'Customer Return', value: 'Customer Return' },
            ],
            onChange: setTypeFilter,
          },
        ]}
        totalCount={filtered.length}
        actions={
          <Stack direction="row" spacing={0.5}>
            <ExportButton />
            <RefreshButton />
          </Stack>
        }
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          density="compact"
          checkboxSelection
          disableRowSelectionOnClick
          rowSelectionModel={selection}
          onRowSelectionModelChange={setSelection}
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || statusFilter || typeFilter ? 'emptySearch' : 'emptyTable'}
                title={search || statusFilter || typeFilter ? 'No orders match your filters' : 'No inspection orders yet'}
                description={
                  search || statusFilter || typeFilter
                    ? 'Try adjusting your search or filters.'
                    : 'Create your first inspection order to get started.'
                }
                action={
                  !search && !statusFilter && !typeFilter ? (
                    <Button variant="contained" size="small" onClick={openCreate}>
                      New Inspection Order
                    </Button>
                  ) : undefined
                }
                compact
              />
            ),
          }}
          sx={{
            border: '1px solid', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.paper',
            '& .MuiDataGrid-columnHeaders': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), borderBottom: '1px solid', borderColor: 'divider' },
            '& .MuiDataGrid-row:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.03) },
            '& .MuiDataGrid-cell:focus, & .MuiDataGrid-cell:focus-within': { outline: 'none' },
            '& .MuiDataGrid-columnHeader:focus, & .MuiDataGrid-columnHeader:focus-within': { outline: 'none' },
          }}
        />
      </Box>

      {/* Create / Edit drawer */}
      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'New Inspection Order' : `Edit ${editTarget?.orderNo}`}
        subtitle={drawerMode === 'create' ? 'Enter inspection order details below' : editTarget?.productName}
        onSubmit={handleSave}
        submitLabel={drawerMode === 'create' ? 'Create Order' : 'Save Changes'}
        loading={saving}
      >
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <TextField
              select
              label="Inspection Type"
              value={formType}
              onChange={(e) => setFormType(e.target.value as InspectionOrder['type'])}
              fullWidth
              required
            >
              {(['Incoming', 'In-Process', 'Final', 'Customer Return'] as const).map((t) => (
                <MenuItem key={t} value={t}>{t}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Product Code"
              value={formProductCode}
              onChange={(e) => setFormProductCode(e.target.value)}
              fullWidth
              required
              slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Lot #"
              value={formLotNo}
              onChange={(e) => setFormLotNo(e.target.value)}
              fullWidth
              required
              slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Sample Size"
              type="number"
              value={formSampleSize}
              onChange={(e) => setFormSampleSize(e.target.value)}
              fullWidth
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Inspector"
              value={formInspector}
              onChange={(e) => setFormInspector(e.target.value)}
              fullWidth
              required
            />
          </Grid>
        </Grid>
      </FormDrawer>

      {/* Delete confirmation */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Inspection Order"
        description={
          <>
            Delete inspection order <strong>{deleteTarget?.orderNo}</strong> for lot <strong>{deleteTarget?.lotNo}</strong>?
            This cannot be undone.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
