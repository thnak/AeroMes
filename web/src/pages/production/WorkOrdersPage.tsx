import {
  Box,
  Button,
  Grid,
  IconButton,
  LinearProgress,
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
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  ConfirmDialog,
  EmptyState,
  ExportButton,
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  StatusChip,
  TableToolbar,
} from '../../components';
import type { WorkOrderStatus } from '../../theme/tokens';

// ─── Types & mock data ────────────────────────────────────────────────────────

interface WorkOrder {
  id: string;
  no: string;
  productCode: string;
  productName: string;
  targetQty: number;
  actualQty: number;
  status: WorkOrderStatus;
  plannedStart: string;
  plannedEnd: string;
  assignedMachine: string;
}

export const MOCK_WORK_ORDERS: WorkOrder[] = [
  { id: '1',  no: 'WO-2026-0089', productCode: 'FRM-A001', productName: 'Frame Assembly A',     targetQty: 500,  actualQty: 387,  status: 'RUNNING',   plannedStart: '2026-06-01', plannedEnd: '2026-06-15', assignedMachine: 'MC-01' },
  { id: '2',  no: 'WO-2026-0090', productCode: 'PNL-B002', productName: 'Panel Sub-assembly B', targetQty: 200,  actualQty: 0,    status: 'RELEASED',  plannedStart: '2026-06-10', plannedEnd: '2026-06-20', assignedMachine: 'MC-02' },
  { id: '3',  no: 'WO-2026-0088', productCode: 'SHT-C003', productName: 'Shaft Housing C',      targetQty: 800,  actualQty: 360,  status: 'PAUSED',    plannedStart: '2026-05-28', plannedEnd: '2026-06-18', assignedMachine: 'MC-03' },
  { id: '4',  no: 'WO-2026-0091', productCode: 'BRK-D004', productName: 'Bracket Set D',        targetQty: 1200, actualQty: 1200, status: 'COMPLETED', plannedStart: '2026-05-20', plannedEnd: '2026-06-05', assignedMachine: 'MC-04' },
  { id: '5',  no: 'WO-2026-0085', productCode: 'MTR-E005', productName: 'Motor Mount E',        targetQty: 300,  actualQty: 298,  status: 'CLOSED',    plannedStart: '2026-05-10', plannedEnd: '2026-05-30', assignedMachine: 'MC-01' },
  { id: '6',  no: 'WO-2026-0092', productCode: 'COV-F006', productName: 'Cover Plate F',        targetQty: 600,  actualQty: 0,    status: 'DRAFT',     plannedStart: '2026-06-18', plannedEnd: '2026-07-02', assignedMachine: 'MC-05' },
  { id: '7',  no: 'WO-2026-0087', productCode: 'GRD-G007', productName: 'Guard Assembly G',     targetQty: 400,  actualQty: 0,    status: 'CANCELLED', plannedStart: '2026-05-25', plannedEnd: '2026-06-10', assignedMachine: 'MC-02' },
  { id: '8',  no: 'WO-2026-0086', productCode: 'SPR-H008', productName: 'Sprocket Set H',       targetQty: 250,  actualQty: 180,  status: 'ON_HOLD',   plannedStart: '2026-06-02', plannedEnd: '2026-06-22', assignedMachine: 'MC-03' },
  { id: '9',  no: 'WO-2026-0093', productCode: 'HNG-J010', productName: 'Hinge Assembly J',     targetQty: 750,  actualQty: 0,    status: 'RELEASED',  plannedStart: '2026-06-12', plannedEnd: '2026-07-05', assignedMachine: 'MC-06' },
  { id: '10', no: 'WO-2026-0094', productCode: 'FRM-A001', productName: 'Frame Assembly A',     targetQty: 500,  actualQty: 120,  status: 'RUNNING',   plannedStart: '2026-06-08', plannedEnd: '2026-06-25', assignedMachine: 'MC-01' },
];

const MOCK_MACHINES = ['MC-01', 'MC-02', 'MC-03', 'MC-04', 'MC-05', 'MC-06'];

const MOCK_PRODUCTS = [
  { code: 'FRM-A001', name: 'Frame Assembly A' },
  { code: 'PNL-B002', name: 'Panel Sub-assembly B' },
  { code: 'SHT-C003', name: 'Shaft Housing C' },
  { code: 'BRK-D004', name: 'Bracket Set D' },
  { code: 'MTR-E005', name: 'Motor Mount E' },
  { code: 'COV-F006', name: 'Cover Plate F' },
  { code: 'GRD-G007', name: 'Guard Assembly G' },
  { code: 'SPR-H008', name: 'Sprocket Set H' },
  { code: 'HNG-J010', name: 'Hinge Assembly J' },
];

const STATUS_OPTIONS: { label: string; value: string }[] = [
  { label: 'Draft',     value: 'DRAFT' },
  { label: 'Released',  value: 'RELEASED' },
  { label: 'Running',   value: 'RUNNING' },
  { label: 'Paused',    value: 'PAUSED' },
  { label: 'Completed', value: 'COMPLETED' },
];

// ─── Form ─────────────────────────────────────────────────────────────────────

const WOSchema = z.object({
  no:              z.string().min(1, 'WO number is required'),
  productCode:     z.string().min(1, 'Product is required'),
  targetQty:       z.coerce.number().min(1, 'Target qty must be > 0'),
  plannedStart:    z.string().min(1, 'Planned start is required'),
  plannedEnd:      z.string().min(1, 'Planned end is required'),
  assignedMachine: z.string().min(1, 'Machine is required'),
});
type WOFormValues = z.infer<typeof WOSchema>;

function WorkOrderForm({
  defaultValues,
  onSubmit,
}: {
  defaultValues: Partial<WOFormValues>;
  onSubmit: (data: WOFormValues) => void;
}) {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const { register, control, handleSubmit, formState: { errors } } = useForm<WOFormValues>({
    resolver: zodResolver(WOSchema) as any,
    defaultValues,
  });

  return (
    <Box component="form" id="wo-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('no')}
            label="WO Number"
            fullWidth
            required
            error={!!errors.no}
            helperText={errors.no?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="productCode"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Product"
                fullWidth
                required
                error={!!errors.productCode}
                helperText={errors.productCode?.message}
              >
                {MOCK_PRODUCTS.map((p) => (
                  <MenuItem key={p.code} value={p.code}>{p.code} — {p.name}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('targetQty')}
            label="Target Qty"
            type="number"
            fullWidth
            required
            error={!!errors.targetQty}
            helperText={errors.targetQty?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="assignedMachine"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Machine"
                fullWidth
                required
                error={!!errors.assignedMachine}
                helperText={errors.assignedMachine?.message}
              >
                {MOCK_MACHINES.map((m) => (
                  <MenuItem key={m} value={m}>{m}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('plannedStart')}
            label="Planned Start"
            fullWidth
            required
            placeholder="YYYY-MM-DD"
            error={!!errors.plannedStart}
            helperText={errors.plannedStart?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('plannedEnd')}
            label="Planned End"
            fullWidth
            required
            placeholder="YYYY-MM-DD"
            error={!!errors.plannedEnd}
            helperText={errors.plannedEnd?.message}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function WorkOrdersPage() {
  const navigate = useNavigate();
  const [rows, setRows]                   = useState<WorkOrder[]>(MOCK_WORK_ORDERS);
  const [search, setSearch]               = useState('');
  const [statusFilter, setStatusFilter]   = useState('');
  const [productFilter, setProductFilter] = useState('');
  const [selection, setSelection]         = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]       = useState(false);
  const [drawerMode, setDrawerMode]       = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]       = useState<WorkOrder | null>(null);
  const [deleteTarget, setDeleteTarget]   = useState<WorkOrder | null>(null);
  const [saving, setSaving]               = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)        r = r.filter((w) => w.no.toLowerCase().includes(search.toLowerCase()) || w.productName.toLowerCase().includes(search.toLowerCase()));
    if (statusFilter)  r = r.filter((w) => w.status === statusFilter);
    if (productFilter) r = r.filter((w) => w.productCode === productFilter);
    return r;
  }, [rows, search, statusFilter, productFilter]);

  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(w: WorkOrder) { setDrawerMode('edit'); setEditTarget(w); setDrawerOpen(true); }

  function handleSave(data: WOFormValues) {
    setSaving(true);
    const product = MOCK_PRODUCTS.find((p) => p.code === data.productCode);
    setTimeout(() => {
      if (drawerMode === 'create') {
        setRows((prev) => [
          ...prev,
          { id: String(Date.now()), ...data, productName: product?.name ?? '', actualQty: 0, status: 'DRAFT' as WorkOrderStatus },
        ]);
      } else if (editTarget) {
        setRows((prev) => prev.map((w) => w.id === editTarget.id
          ? { ...w, ...data, productName: product?.name ?? w.productName }
          : w
        ));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((w) => w.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<WorkOrder>[] = [
    {
      field: 'no',
      headerName: 'WO #',
      width: 150,
      renderCell: (params: GridRenderCellParams<WorkOrder>) => (
        <Typography
          variant="body2"
          onClick={() => navigate(`/production/work-orders/${params.row.id}`)}
          sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main', cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }}
        >
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'productName',
      headerName: 'Product',
      flex: 1,
      minWidth: 160,
      renderCell: (params: GridRenderCellParams<WorkOrder>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600 }}>{params.value}</Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11 }}>{params.row.productCode}</Typography>
        </Stack>
      ),
    },
    {
      field: 'targetQty',
      headerName: 'Target Qty',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams<WorkOrder>) => (
        <Typography variant="body2" sx={{ fontWeight: 600 }}>{(params.value as number).toLocaleString()}</Typography>
      ),
    },
    {
      field: 'actualQty',
      headerName: 'Actual Qty',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams<WorkOrder>) => (
        <Typography variant="body2" sx={{ fontWeight: 600 }}>{(params.value as number).toLocaleString()}</Typography>
      ),
    },
    {
      field: 'progress',
      headerName: 'Progress',
      width: 130,
      sortable: false,
      renderCell: (params: GridRenderCellParams<WorkOrder>) => {
        const pct = params.row.targetQty > 0 ? Math.round((params.row.actualQty / params.row.targetQty) * 100) : 0;
        return (
          <Stack spacing={0.5} sx={{ width: '100%', py: 0.75 }}>
            <LinearProgress
              variant="determinate"
              value={pct}
              sx={{ height: 6, borderRadius: 3, bgcolor: (t) => alpha(t.palette.primary.main, 0.12) }}
            />
            <Typography variant="caption" color="text.secondary" sx={{ fontSize: 10 }}>{pct}%</Typography>
          </Stack>
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 110,
      renderCell: (params: GridRenderCellParams<WorkOrder>) => (
        <StatusChip status={params.value as WorkOrderStatus} />
      ),
    },
    {
      field: 'plannedEnd',
      headerName: 'Planned End',
      width: 110,
      renderCell: (params: GridRenderCellParams<WorkOrder>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>{params.value as string}</Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 105,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<WorkOrder>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View">
            <IconButton size="small" onClick={() => navigate(`/production/work-orders/${params.row.id}`)} sx={{ color: 'text.secondary' }}>
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
        title="Work Orders"
        subtitle="Plan, release and track production work orders"
        breadcrumbs={[{ label: 'Production' }, { label: 'Work Orders' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={openCreate}
          >
            New Work Order
          </Button>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search WO number or product…"
        filters={[
          {
            label: 'Status',
            value: statusFilter,
            options: STATUS_OPTIONS,
            onChange: setStatusFilter,
          },
          {
            label: 'Product',
            value: productFilter,
            options: MOCK_PRODUCTS.map((p) => ({ label: p.name, value: p.code })),
            onChange: setProductFilter,
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
                icon={search || statusFilter || productFilter ? 'emptySearch' : 'emptyTable'}
                title={search || statusFilter || productFilter ? 'No work orders match your filters' : 'No work orders yet'}
                description={
                  search || statusFilter || productFilter
                    ? 'Try adjusting your search or filters.'
                    : 'Create your first work order to start production.'
                }
                action={
                  !search && !statusFilter && !productFilter ? (
                    <Button variant="contained" size="small" onClick={openCreate}>New Work Order</Button>
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

      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'New Work Order' : `Edit ${editTarget?.no}`}
        subtitle={drawerMode === 'create' ? 'Enter work order details' : editTarget?.productName}
        onSubmit={() => document.getElementById('wo-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Work Order' : 'Save Changes'}
        loading={saving}
      >
        <WorkOrderForm
          key={editTarget?.id ?? 'new'}
          defaultValues={
            editTarget
              ? { no: editTarget.no, productCode: editTarget.productCode, targetQty: editTarget.targetQty, plannedStart: editTarget.plannedStart, plannedEnd: editTarget.plannedEnd, assignedMachine: editTarget.assignedMachine }
              : {}
          }
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Work Order"
        description={
          <>Delete work order <strong>{deleteTarget?.no}</strong>? This cannot be undone and will also remove all associated jobs and logs.</>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
