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

interface WorkCenter {
  id: string;
  code: string;
  name: string;
  type: string;
  capacity: number;
  capacityUnit: string;
  isActive: boolean;
  location: string;
  description?: string;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_WORK_CENTERS: WorkCenter[] = [
  { id: '1',  code: 'WC-001', name: 'CNC Machining Bay',    type: 'Machining',   capacity: 8, capacityUnit: 'machines/shift',  isActive: true,  location: 'Floor A', description: 'Primary CNC machining bay for turning and milling operations.' },
  { id: '2',  code: 'WC-002', name: 'Assembly Station 1',   type: 'Assembly',    capacity: 4, capacityUnit: 'operators/shift', isActive: true,  location: 'Floor B', description: 'Main assembly station for final product assembly.' },
  { id: '3',  code: 'WC-003', name: 'MIG Welding Bay',      type: 'Welding',     capacity: 6, capacityUnit: 'machines/shift',  isActive: true,  location: 'Floor A', description: 'MIG/MAG welding bay with robotic and manual cells.' },
  { id: '4',  code: 'WC-004', name: 'Final Inspection',     type: 'Inspection',  capacity: 3, capacityUnit: 'operators/shift', isActive: true,  location: 'Floor C', description: 'Final quality inspection before shipment.' },
  { id: '5',  code: 'WC-005', name: 'Paint Booth Line',     type: 'Painting',    capacity: 2, capacityUnit: 'lines/shift',     isActive: true,  location: 'Floor C', description: 'Automated and manual paint booth for surface finishing.' },
  { id: '6',  code: 'WC-006', name: 'Sub-assembly Line',    type: 'Assembly',    capacity: 8, capacityUnit: 'operators/shift', isActive: true,  location: 'Floor B', description: 'Sub-assembly line for component-level assembly work.' },
  { id: '7',  code: 'WC-007', name: 'CNC Milling Center',   type: 'Machining',   capacity: 6, capacityUnit: 'machines/shift',  isActive: true,  location: 'Floor A', description: 'Dedicated CNC milling center for precision parts.' },
  { id: '8',  code: 'WC-008', name: 'QC Incoming',          type: 'Inspection',  capacity: 4, capacityUnit: 'operators/shift', isActive: false, location: 'Floor C', description: 'Incoming quality control inspection station.' },
  { id: '9',  code: 'WC-009', name: 'Turning Center',       type: 'Machining',   capacity: 4, capacityUnit: 'machines/shift',  isActive: true,  location: 'Floor A', description: 'Turning center for shaft and cylindrical parts.' },
  { id: '10', code: 'WC-010', name: 'Press Shop',           type: 'Machining',   capacity: 3, capacityUnit: 'presses/shift',   isActive: true,  location: 'Floor A', description: 'Press shop for stamping and forming operations.' },
];

const WC_TYPES = ['Machining', 'Assembly', 'Welding', 'Inspection', 'Painting'];

const TYPE_COLORS: Record<string, string> = {
  Machining:  '#1D4ED8',
  Assembly:   '#0D9488',
  Welding:    '#D97706',
  Inspection: '#7C3AED',
  Painting:   '#DC2626',
};

// ─── Form schema ──────────────────────────────────────────────────────────────

const WorkCenterSchema = z.object({
  code:         z.string().min(1, 'Code is required').max(20),
  name:         z.string().min(1, 'Name is required').max(200),
  type:         z.string().min(1, 'Type is required'),
  capacity:     z.number().min(1, 'Capacity must be at least 1'),
  capacityUnit: z.string().min(1, 'Capacity unit is required'),
  location:     z.string().min(1, 'Location is required'),
  description:  z.string().optional(),
  isActive:     z.boolean(),
});

type WorkCenterFormValues = z.infer<typeof WorkCenterSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function WorkCenterForm({
  defaultValues,
  onSubmit,
  loading: _loading,
}: {
  defaultValues: Partial<WorkCenterFormValues>;
  onSubmit: (data: WorkCenterFormValues) => void;
  loading: boolean;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<WorkCenterFormValues>({
    resolver: zodResolver(WorkCenterSchema),
    defaultValues: { isActive: true, ...defaultValues },
  });

  return (
    <Box component="form" id="wc-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Work Center Code"
            fullWidth
            required
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="type"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Type"
                fullWidth
                required
                error={!!errors.type}
                helperText={errors.type?.message}
              >
                {WC_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>{t}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Work Center Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('capacity', { valueAsNumber: true })}
            label="Capacity"
            fullWidth
            required
            type="number"
            error={!!errors.capacity}
            helperText={errors.capacity?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('capacityUnit')}
            label="Capacity Unit"
            fullWidth
            required
            placeholder="e.g. machines/shift"
            error={!!errors.capacityUnit}
            helperText={errors.capacityUnit?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('location')}
            label="Location"
            fullWidth
            required
            placeholder="e.g. Floor A"
            error={!!errors.location}
            helperText={errors.location?.message}
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
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('description')}
            label="Description"
            fullWidth
            multiline
            rows={3}
            placeholder="Optional description…"
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function WorkCentersPage() {
  const navigate = useNavigate();
  const [rows, setRows]                 = useState<WorkCenter[]>(MOCK_WORK_CENTERS);
  const [search, setSearch]             = useState('');
  const [typeFilter, setTypeFilter]     = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<WorkCenter | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<WorkCenter | null>(null);
  const [saving, setSaving]             = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)       r = r.filter((w) => w.name.toLowerCase().includes(search.toLowerCase()) || w.code.toLowerCase().includes(search.toLowerCase()));
    if (typeFilter)   r = r.filter((w) => w.type === typeFilter);
    if (statusFilter) r = r.filter((w) => statusFilter === 'active' ? w.isActive : !w.isActive);
    return r;
  }, [rows, search, typeFilter, statusFilter]);

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(w: WorkCenter) { setDrawerMode('edit'); setEditTarget(w); setDrawerOpen(true); }

  function handleSave(data: WorkCenterFormValues) {
    setSaving(true);
    setTimeout(() => {
      if (drawerMode === 'create') {
        setRows((prev) => [...prev, { id: String(Date.now()), ...data }]);
      } else if (editTarget) {
        setRows((prev) => prev.map((w) => w.id === editTarget.id ? { ...w, ...data } : w));
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

  const columns: GridColDef<WorkCenter>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<WorkCenter>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 180 },
    {
      field: 'type',
      headerName: 'Type',
      width: 120,
      renderCell: (params: GridRenderCellParams<WorkCenter>) => {
        const color = TYPE_COLORS[params.value as string] ?? '#64748B';
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{
              height: 20,
              fontSize: '0.6875rem',
              fontWeight: 600,
              bgcolor: alpha(color, 0.1),
              color,
              border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'capacity',
      headerName: 'Capacity',
      width: 175,
      renderCell: (params: GridRenderCellParams<WorkCenter>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          {params.row.capacity} {params.row.capacityUnit}
        </Typography>
      ),
    },
    { field: 'location', headerName: 'Location', width: 100 },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<WorkCenter>) => (
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
      field: 'actions',
      headerName: '',
      width: 110,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<WorkCenter>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View Detail">
            <IconButton size="small" onClick={() => navigate(`/master/work-centers/${params.row.id}`)} sx={{ color: 'text.secondary' }}>
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
        title="Work Centers"
        subtitle="Define and manage production work centers, capacities, and locations"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Work Centers' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Work Center
            </Button>
          </>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search code or name…"
        filters={[
          {
            label: 'Type',
            value: typeFilter,
            options: WC_TYPES.map((t) => ({ label: t, value: t })),
            onChange: setTypeFilter,
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
                icon={search || typeFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || typeFilter || statusFilter ? 'No work centers match your filters' : 'No work centers yet'}
                description={search || typeFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first work center to get started.'}
                action={!search && !typeFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Work Center</Button>
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
        title={drawerMode === 'create' ? 'New Work Center' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter work center details below' : editTarget?.name}
        onSubmit={() => document.getElementById('wc-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Work Center' : 'Save Changes'}
        loading={saving}
      >
        <WorkCenterForm
          key={editTarget?.id ?? 'new'}
          defaultValues={editTarget ?? {}}
          onSubmit={handleSave}
          loading={saving}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Work Center"
        description={
          <>
            Delete <strong>{deleteTarget?.name}</strong> ({deleteTarget?.code})?
            This cannot be undone and may affect machines and routings assigned to this work center.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
