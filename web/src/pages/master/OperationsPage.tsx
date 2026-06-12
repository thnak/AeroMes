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

interface Operation {
  id: string;
  code: string;
  name: string;
  machineType: string;
  setupTimeMin: number;
  cycleTimeSec: number;
  isActive: boolean;
  description?: string;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_OPERATIONS: Operation[] = [
  { id: '1',  code: 'OP-001', name: 'CNC Turning',       machineType: 'Lathe',      setupTimeMin: 30,  cycleTimeSec: 45,  isActive: true,  description: 'CNC turning for cylindrical parts.' },
  { id: '2',  code: 'OP-002', name: 'CNC Milling',       machineType: 'Mill',       setupTimeMin: 45,  cycleTimeSec: 120, isActive: true,  description: 'CNC milling for prismatic parts.' },
  { id: '3',  code: 'OP-003', name: 'MIG Welding',       machineType: 'Welding',    setupTimeMin: 20,  cycleTimeSec: 180, isActive: true,  description: 'MIG/MAG welding operation.' },
  { id: '4',  code: 'OP-004', name: 'Visual Inspection', machineType: 'Inspection', setupTimeMin: 5,   cycleTimeSec: 60,  isActive: true,  description: 'Visual quality inspection.' },
  { id: '5',  code: 'OP-005', name: 'Painting',          machineType: 'Painting',   setupTimeMin: 30,  cycleTimeSec: 300, isActive: true,  description: 'Surface painting and coating.' },
  { id: '6',  code: 'OP-006', name: 'Assembly A',        machineType: 'Assembly',   setupTimeMin: 15,  cycleTimeSec: 240, isActive: true,  description: 'Standard assembly operation A.' },
  { id: '7',  code: 'OP-007', name: 'Pressing',          machineType: 'Press',      setupTimeMin: 25,  cycleTimeSec: 30,  isActive: true,  description: 'Stamping and pressing operation.' },
  { id: '8',  code: 'OP-008', name: 'Deburring',         machineType: 'Manual',     setupTimeMin: 10,  cycleTimeSec: 90,  isActive: true,  description: 'Manual deburring and edge cleanup.' },
  { id: '9',  code: 'OP-009', name: 'Heat Treatment',    machineType: 'Furnace',    setupTimeMin: 120, cycleTimeSec: 0,   isActive: true,  description: 'Batch heat treatment (cycle time is batch-based).' },
  { id: '10', code: 'OP-010', name: 'CMM Inspection',    machineType: 'Inspection', setupTimeMin: 10,  cycleTimeSec: 180, isActive: false, description: 'CMM dimensional inspection.' },
];

const MACHINE_TYPES = [...new Set(MOCK_OPERATIONS.map((o) => o.machineType))].sort();

const TYPE_COLORS: Record<string, string> = {
  Lathe:      '#1D4ED8',
  Mill:       '#0D9488',
  Welding:    '#D97706',
  Inspection: '#7C3AED',
  Painting:   '#DC2626',
  Assembly:   '#15803D',
  Press:      '#374151',
  Manual:     '#64748B',
  Furnace:    '#B45309',
};

// ─── Form schema ──────────────────────────────────────────────────────────────

const OperationSchema = z.object({
  code:         z.string().min(1, 'Code is required').max(20),
  name:         z.string().min(1, 'Name is required').max(200),
  machineType:  z.string().min(1, 'Machine type is required'),
  setupTimeMin: z.number().min(0),
  cycleTimeSec: z.number().min(0),
  description:  z.string().optional(),
  isActive:     z.boolean(),
});

type OperationFormValues = z.infer<typeof OperationSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function OperationForm({
  defaultValues,
  onSubmit,
  loading: _loading,
}: {
  defaultValues: Partial<OperationFormValues>;
  onSubmit: (data: OperationFormValues) => void;
  loading: boolean;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<OperationFormValues>({
    resolver: zodResolver(OperationSchema),
    defaultValues: { isActive: true, setupTimeMin: 0, cycleTimeSec: 0, ...defaultValues },
  });

  return (
    <Box component="form" id="op-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Operation Code"
            fullWidth
            required
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="machineType"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Machine Type"
                fullWidth
                required
                error={!!errors.machineType}
                helperText={errors.machineType?.message}
              >
                {MACHINE_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>{t}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Operation Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('setupTimeMin', { valueAsNumber: true })}
            label="Setup Time (min)"
            fullWidth
            required
            type="number"
            error={!!errors.setupTimeMin}
            helperText={errors.setupTimeMin?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('cycleTimeSec', { valueAsNumber: true })}
            label="Cycle Time (sec)"
            fullWidth
            required
            type="number"
            error={!!errors.cycleTimeSec}
            helperText={errors.cycleTimeSec?.message}
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

export default function OperationsPage() {
  const [rows, setRows]                   = useState<Operation[]>(MOCK_OPERATIONS);
  const [search, setSearch]               = useState('');
  const [typeFilter, setTypeFilter]       = useState('');
  const [statusFilter, setStatusFilter]   = useState('');
  const [selection, setSelection]         = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]       = useState(false);
  const [drawerMode, setDrawerMode]       = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]       = useState<Operation | null>(null);
  const [deleteTarget, setDeleteTarget]   = useState<Operation | null>(null);
  const [saving, setSaving]               = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)       r = r.filter((o) => o.name.toLowerCase().includes(search.toLowerCase()) || o.code.toLowerCase().includes(search.toLowerCase()));
    if (typeFilter)   r = r.filter((o) => o.machineType === typeFilter);
    if (statusFilter) r = r.filter((o) => statusFilter === 'active' ? o.isActive : !o.isActive);
    return r;
  }, [rows, search, typeFilter, statusFilter]);

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(o: Operation) { setDrawerMode('edit'); setEditTarget(o); setDrawerOpen(true); }

  function handleSave(data: OperationFormValues) {
    setSaving(true);
    setTimeout(() => {
      if (drawerMode === 'create') {
        setRows((prev) => [...prev, { id: String(Date.now()), ...data }]);
      } else if (editTarget) {
        setRows((prev) => prev.map((o) => o.id === editTarget.id ? { ...o, ...data } : o));
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

  const columns: GridColDef<Operation>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<Operation>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 160 },
    {
      field: 'machineType',
      headerName: 'Machine Type',
      width: 130,
      renderCell: (params: GridRenderCellParams<Operation>) => {
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
      field: 'setupTimeMin',
      headerName: 'Setup Time',
      width: 110,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams<Operation>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>{params.value} min</Typography>
      ),
    },
    {
      field: 'cycleTimeSec',
      headerName: 'Cycle Time',
      width: 110,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams<Operation>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          {(params.value as number) > 0 ? `${params.value} sec` : 'Batch'}
        </Typography>
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Operation>) => (
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
      width: 80,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<Operation>) => (
        <Stack direction="row" spacing={0.25}>
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
        title="Operations"
        subtitle="Define production operations with setup times, cycle times, and machine types"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Operations' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Operation
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
            label: 'Machine Type',
            value: typeFilter,
            options: MACHINE_TYPES.map((t) => ({ label: t, value: t })),
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
                title={search || typeFilter || statusFilter ? 'No operations match your filters' : 'No operations yet'}
                description={search || typeFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first operation to get started.'}
                action={!search && !typeFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Operation</Button>
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
        title={drawerMode === 'create' ? 'New Operation' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter operation details below' : editTarget?.name}
        onSubmit={() => document.getElementById('op-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Operation' : 'Save Changes'}
        loading={saving}
      >
        <OperationForm
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
        title="Delete Operation"
        description={
          <>
            Delete <strong>{deleteTarget?.name}</strong> ({deleteTarget?.code})?
            This cannot be undone and may affect routings that reference this operation.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
