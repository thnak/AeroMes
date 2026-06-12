import {
  Box,
  Button,
  Chip,
  FormControlLabel,
  Grid,
  IconButton,
  MenuItem,
  Stack,
  Switch,
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
  TableToolbar,
} from '../../components';

// ─── Types ────────────────────────────────────────────────────────────────────

interface Routing {
  id: string;
  code: string;
  productCode: string;
  productName: string;
  stepsCount: number;
  totalTimeMin: number;
  isActive: boolean;
  version: string;
  updatedAt: string;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_ROUTINGS: Routing[] = [
  { id: '1', code: 'RT-001', productCode: 'FRM-A001', productName: 'Frame Assembly A',      stepsCount: 5, totalTimeMin: 420, isActive: true,  version: 'Rev.C', updatedAt: '2026-03-15' },
  { id: '2', code: 'RT-002', productCode: 'PNL-B002', productName: 'Panel Sub-assembly B',  stepsCount: 4, totalTimeMin: 360, isActive: true,  version: 'Rev.B', updatedAt: '2026-04-10' },
  { id: '3', code: 'RT-003', productCode: 'SHT-C003', productName: 'Shaft Housing C',       stepsCount: 3, totalTimeMin: 280, isActive: true,  version: 'Rev.A', updatedAt: '2026-01-20' },
  { id: '4', code: 'RT-004', productCode: 'BRK-D004', productName: 'Bracket Set D',         stepsCount: 2, totalTimeMin: 90,  isActive: true,  version: 'Rev.A', updatedAt: '2026-05-05' },
  { id: '5', code: 'RT-005', productCode: 'MTR-E005', productName: 'Motor Mount E',         stepsCount: 4, totalTimeMin: 310, isActive: true,  version: 'Rev.B', updatedAt: '2026-02-28' },
  { id: '6', code: 'RT-006', productCode: 'COV-F006', productName: 'Cover Plate F',         stepsCount: 3, totalTimeMin: 150, isActive: true,  version: 'Rev.A', updatedAt: '2026-03-10' },
  { id: '7', code: 'RT-007', productCode: 'HNG-J010', productName: 'Hinge Assembly J',      stepsCount: 6, totalTimeMin: 480, isActive: true,  version: 'Rev.D', updatedAt: '2026-05-18' },
  { id: '8', code: 'RT-008', productCode: 'WHL-L012', productName: 'Wheel & Hub Assembly L',stepsCount: 7, totalTimeMin: 600, isActive: false, version: 'Rev.C', updatedAt: '2026-04-22' },
];

const PRODUCTS = MOCK_ROUTINGS.map((r) => ({ code: r.productCode, name: r.productName }));

// ─── Form schema ──────────────────────────────────────────────────────────────

const RoutingSchema = z.object({
  code:        z.string().min(1, 'Code is required').max(20),
  productCode: z.string().min(1, 'Product is required'),
  version:     z.string().min(1, 'Version is required'),
  isActive:    z.boolean(),
});

type RoutingFormValues = z.infer<typeof RoutingSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function RoutingForm({
  defaultValues,
  onSubmit,
  loading: _loading,
}: {
  defaultValues: Partial<RoutingFormValues>;
  onSubmit: (data: RoutingFormValues) => void;
  loading: boolean;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<RoutingFormValues>({
    resolver: zodResolver(RoutingSchema),
    defaultValues: { isActive: true, version: 'Rev.A', ...defaultValues },
  });

  return (
    <Box component="form" id="routing-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Routing Code"
            fullWidth
            required
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('version')}
            label="Version"
            fullWidth
            required
            placeholder="e.g. Rev.A"
            error={!!errors.version}
            helperText={errors.version?.message}
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
                {PRODUCTS.map((p) => (
                  <MenuItem key={p.code} value={p.code}>{p.code} — {p.name}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="isActive"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={<Switch checked={field.value} onChange={field.onChange} color="primary" />}
                label="Active"
                sx={{ mt: 0.5, ml: 0 }}
              />
            )}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function RoutingsPage() {
  const navigate = useNavigate();
  const [rows, setRows]                   = useState<Routing[]>(MOCK_ROUTINGS);
  const [search, setSearch]               = useState('');
  const [productFilter, setProductFilter] = useState('');
  const [statusFilter, setStatusFilter]   = useState('');
  const [selection, setSelection]         = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]       = useState(false);
  const [drawerMode, setDrawerMode]       = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]       = useState<Routing | null>(null);
  const [deleteTarget, setDeleteTarget]   = useState<Routing | null>(null);
  const [saving, setSaving]               = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)        r = r.filter((rt) => rt.code.toLowerCase().includes(search.toLowerCase()) || rt.productCode.toLowerCase().includes(search.toLowerCase()) || rt.productName.toLowerCase().includes(search.toLowerCase()));
    if (productFilter) r = r.filter((rt) => rt.productCode === productFilter);
    if (statusFilter)  r = r.filter((rt) => statusFilter === 'active' ? rt.isActive : !rt.isActive);
    return r;
  }, [rows, search, productFilter, statusFilter]);

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(rt: Routing) { setDrawerMode('edit'); setEditTarget(rt); setDrawerOpen(true); }

  function handleSave(data: RoutingFormValues) {
    setSaving(true);
    setTimeout(() => {
      const product = PRODUCTS.find((p) => p.code === data.productCode);
      if (drawerMode === 'create') {
        setRows((prev) => [...prev, {
          id: String(Date.now()),
          productName: product?.name ?? '',
          stepsCount: 0,
          totalTimeMin: 0,
          updatedAt: new Date().toISOString().slice(0, 10),
          ...data,
        }]);
      } else if (editTarget) {
        setRows((prev) => prev.map((rt) =>
          rt.id === editTarget.id
            ? { ...rt, ...data, productName: product?.name ?? rt.productName }
            : rt
        ));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((rt) => rt.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<Routing>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'productCode',
      headerName: 'Product',
      flex: 1,
      minWidth: 200,
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'primary.main' }}>
            {params.value}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>
            {params.row.productName}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'stepsCount',
      headerName: 'Steps',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Chip
          label={`${params.value} steps`}
          size="small"
          sx={(theme) => ({
            height: 20,
            fontSize: '0.6875rem',
            fontWeight: 600,
            bgcolor: alpha(theme.palette.primary.main, 0.08),
            color: 'primary.main',
            border: 'none',
            '& .MuiChip-label': { px: 0.75 },
          })}
        />
      ),
    },
    {
      field: 'totalTimeMin',
      headerName: 'Total Time',
      width: 110,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>{params.value} min</Typography>
      ),
    },
    {
      field: 'version',
      headerName: 'Version',
      width: 90,
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Chip
          label={params.value}
          size="small"
          sx={{
            height: 20,
            fontSize: '0.6875rem',
            fontWeight: 600,
            bgcolor: alpha('#7C3AED', 0.08),
            color: '#7C3AED',
            border: 'none',
            '& .MuiChip-label': { px: 0.75 },
          }}
        />
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Chip
          label={params.value ? 'Active' : 'Inactive'}
          size="small"
          sx={{
            height: 20,
            fontSize: '0.6875rem',
            fontWeight: 600,
            bgcolor: params.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
            color: params.value ? '#15803D' : '#94A3B8',
            border: 'none',
            '& .MuiChip-label': { px: 0.75 },
          }}
        />
      ),
    },
    {
      field: 'updatedAt',
      headerName: 'Updated',
      width: 110,
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Typography variant="body2" sx={{ fontSize: 12, color: 'text.secondary' }}>{params.value}</Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 110,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<Routing>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View Steps">
            <IconButton size="small" onClick={() => navigate(`/master/routings/${params.row.id}/steps`)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="eye" size={16} />
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
        title="Routings"
        subtitle="Manage production routing templates that define the sequence of operations for each product"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Routings' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Routing
            </Button>
          </>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search routing code or product…"
        filters={[
          {
            label: 'Product',
            value: productFilter,
            options: PRODUCTS.map((p) => ({ label: `${p.code} — ${p.name}`, value: p.code })),
            onChange: setProductFilter,
          },
          {
            label: 'Status',
            value: statusFilter,
            options: [{ label: 'Active', value: 'active' }, { label: 'Inactive', value: 'inactive' }],
            onChange: setStatusFilter,
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
                icon={search || productFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || productFilter || statusFilter ? 'No routings match your filters' : 'No routings yet'}
                description={search || productFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first routing to get started.'}
                action={!search && !productFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Routing</Button>
                ) : undefined}
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
        title={drawerMode === 'create' ? 'New Routing' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter routing details below' : editTarget?.productName}
        onSubmit={() => document.getElementById('routing-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Routing' : 'Save Changes'}
        loading={saving}
      >
        <RoutingForm
          key={editTarget?.id ?? 'new'}
          defaultValues={editTarget ? {
            code:        editTarget.code,
            productCode: editTarget.productCode,
            version:     editTarget.version,
            isActive:    editTarget.isActive,
          } : {}}
          onSubmit={handleSave}
          loading={saving}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Routing"
        description={
          <>
            Delete routing <strong>{deleteTarget?.code}</strong> for {deleteTarget?.productCode}?
            This cannot be undone and may affect work orders using this routing.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
