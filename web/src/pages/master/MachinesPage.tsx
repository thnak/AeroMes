import {
  Alert,
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
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ConfirmDialog,
  EmptyState,
  ExportButton,
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  StatusDot,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import { machineColors } from '../../theme/tokens';
import {
  useGetApiV1Machines,
  getGetApiV1MachinesQueryKey,
  postApiV1Machines,
  putApiV1MachinesCode,
  deleteApiV1MachinesCode,
} from '../../api/machines/machines';
import { useGetApiV1WorkCenters } from '../../api/work-centers/work-centers';
import type { MachineDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Constants ────────────────────────────────────────────────────────────────

const STATUS_LABELS: Record<string, string> = {
  RUNNING: 'Running',
  IDLE:    'Idle',
  SETUP:   'Setup',
  DOWN:    'Down',
  OFFLINE: 'Offline',
};

// ─── Form schema ──────────────────────────────────────────────────────────────

const MachineSchema = z.object({
  code:         z.string().min(1, 'Code is required').max(30)
    .regex(/^[A-Za-z0-9\-_]+$/, 'Letters, digits, hyphens, and underscores only'),
  name:         z.string().min(1, 'Name is required').max(200),
  workCenterId: z.string().min(1, 'Work center is required'),
  brand:        z.string().max(100).optional(),
  model:        z.string().max(100).optional(),
});

type MachineFormValues = z.infer<typeof MachineSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function MachineForm({
  defaultValues,
  isEdit,
  onSubmit,
  workCenterOptions,
}: {
  defaultValues: Partial<MachineFormValues>;
  isEdit: boolean;
  onSubmit: (data: MachineFormValues) => void;
  workCenterOptions: { id: string; label: string }[];
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<MachineFormValues>({
    resolver: zodResolver(MachineSchema),
    defaultValues,
  });

  return (
    <Box component="form" id="machine-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Machine Code"
            fullWidth
            required
            disabled={isEdit}
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="workCenterId"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? ''}
                select
                label="Work Center"
                fullWidth
                required
                error={!!errors.workCenterId}
                helperText={errors.workCenterId?.message}
              >
                {workCenterOptions.map((wc) => (
                  <MenuItem key={wc.id} value={wc.id}>{wc.label}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Machine Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('brand')}
            label="Brand"
            fullWidth
            placeholder="e.g. Fanuc"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('model')}
            label="Model"
            fullWidth
            placeholder="e.g. Robodrill T21iE"
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function MachinesPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [wcFilter, setWcFilter]         = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<MachineDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<MachineDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: machines = [], isLoading, error, refetch } =
    useGetApiV1Machines({ activeOnly: false });

  const { data: workCenters = [] } = useGetApiV1WorkCenters();

  const workCenterOptions = useMemo(
    () => workCenters.map((wc) => ({ id: String(wc.workCenterID), label: `${wc.workCenterCode} — ${wc.workCenterName}` })),
    [workCenters],
  );

  const filtered = useMemo(() => {
    let r = machines;
    if (search)       r = r.filter((m) => m.machineName.toLowerCase().includes(search.toLowerCase()) || m.machineCode.toLowerCase().includes(search.toLowerCase()));
    if (wcFilter)     r = r.filter((m) => String(m.workCenterID) === wcFilter);
    if (statusFilter) r = r.filter((m) => m.status === statusFilter);
    return r;
  }, [machines, search, wcFilter, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1MachinesQueryKey({ activeOnly: false }) });

  const createMutation = useMutation({
    mutationFn: (data: MachineFormValues) =>
      postApiV1Machines({
        code: data.code,
        name: data.name,
        workCenterId: data.workCenterId,
        brand: data.brand ?? null,
        model: data.model ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: MachineFormValues) =>
      putApiV1MachinesCode(editTarget!.machineCode, {
        name: data.name,
        workCenterId: data.workCenterId,
        brand: data.brand ?? null,
        model: data.model ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => deleteApiV1MachinesCode(code),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(m: MachineDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(m); setDrawerOpen(true); }

  function handleSave(data: MachineFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  const columns: GridColDef<MachineDto>[] = [
    {
      field: 'machineCode',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<MachineDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'machineName', headerName: 'Name', flex: 1, minWidth: 160 },
    {
      field: 'workCenterName',
      headerName: 'Work Center',
      width: 180,
      renderCell: (params: GridRenderCellParams<MachineDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12, color: params.value ? 'text.primary' : 'text.disabled', fontStyle: params.value ? 'normal' : 'italic' }}>
          {params.value ?? '—'}
        </Typography>
      ),
    },
    {
      field: 'brand',
      headerName: 'Brand / Model',
      width: 160,
      renderCell: (params: GridRenderCellParams<MachineDto>) => {
        const brand = params.row.brand;
        const model = params.row.model;
        if (!brand && !model) return <Typography variant="body2" color="text.disabled" sx={{ fontSize: 12, fontStyle: 'italic' }}>—</Typography>;
        return (
          <Stack>
            <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 500 }}>{brand ?? '—'}</Typography>
            {model && <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>{model}</Typography>}
          </Stack>
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 110,
      renderCell: (params: GridRenderCellParams<MachineDto>) => {
        const status = params.value as string;
        const color = (machineColors as Record<string, string>)[status] ?? '#94A3B8';
        return (
          <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
            <StatusDot color={color} size={7} pulse={status === 'RUNNING'} />
            <Typography variant="body2" sx={{ fontSize: 12, color, fontWeight: 500 }}>
              {STATUS_LABELS[status] ?? status}
            </Typography>
          </Stack>
        );
      },
    },
    {
      field: 'isActive',
      headerName: 'Active',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<MachineDto>) => (
        <Chip
          label={params.value ? 'Yes' : 'No'}
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
      width: 130,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<MachineDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="IoT Adapters">
            <IconButton size="small" onClick={() => navigate(`/iot/machines/${params.row.machineCode}/adapters`)} sx={{ color: '#7C3AED' }}>
              <SolarIcon name="machineOn" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="State Rules">
            <IconButton size="small" onClick={() => navigate(`/iot/machines/${params.row.machineCode}/state-rules`)} sx={{ color: '#0EA5E9' }}>
              <SolarIcon name="settings" size={16} />
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

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Machines" breadcrumbs={[{ label: 'Master Data' }, { label: 'Machines' }]} />
      <EmptyState icon="emptyTable" title="Failed to load machines" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Machines"
        subtitle="Monitor and manage all production machines across work centers"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Machines' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              Add Machine
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
            label: 'Work Center',
            value: wcFilter,
            options: workCenterOptions.map((wc) => ({ label: wc.label, value: wc.id })),
            onChange: setWcFilter,
          },
          {
            label: 'Status',
            value: statusFilter,
            options: ['RUNNING', 'IDLE', 'SETUP', 'DOWN', 'OFFLINE'].map((s) => ({ label: STATUS_LABELS[s] ?? s, value: s })),
            onChange: setStatusFilter,
          },
        ]}
        totalCount={filtered.length}
        actions={
          <Stack direction="row" spacing={0.5}>
            <ExportButton />
            <RefreshButton onClick={() => refetch()} />
          </Stack>
        }
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        <DataGrid
          rows={filtered}
          getRowId={(row) => row.machineCode}
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
                icon={search || wcFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || wcFilter || statusFilter ? 'No machines match your filters' : 'No machines yet'}
                description={search || wcFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first machine to get started.'}
                action={!search && !wcFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Machine</Button>
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
        title={drawerMode === 'create' ? 'Add Machine' : `Edit ${editTarget?.machineCode}`}
        subtitle={drawerMode === 'create' ? 'Enter machine details below' : editTarget?.machineName}
        onSubmit={() => document.getElementById('machine-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Add Machine' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <MachineForm
          key={editTarget?.machineCode ?? 'new'}
          isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code:         editTarget.machineCode,
            name:         editTarget.machineName,
            workCenterId: String(editTarget.workCenterID),
            brand:        editTarget.brand ?? undefined,
            model:        editTarget.model ?? undefined,
          } : {}}
          onSubmit={handleSave}
          workCenterOptions={workCenterOptions}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.machineCode)}
        title="Delete Machine"
        description={
          <>
            Delete <strong>{deleteTarget?.machineName}</strong> ({deleteTarget?.machineCode})?
            This cannot be undone and may affect production schedules.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
