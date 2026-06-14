import {
  Alert,
  Box,
  Button,
  Chip,
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
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
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
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1Molds,
  getGetApiV1MoldsQueryKey,
  postApiV1Molds,
  putApiV1MoldsCode,
  deleteApiV1MoldsCode,
} from '../../api/molds/molds';
import type { MoldDto } from '../../api/model';
import { MoldType } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const MOLD_TYPE_LABELS: Record<string, string> = {
  Injection: 'Injection', DieCast: 'Die Cast', Stamping: 'Stamping',
  Blow: 'Blow', Forging: 'Forging', Other: 'Other',
};

const STATUS_COLOR: Record<string, string> = {
  Available: '#15803D', InUse: '#2563EB', InMaintenance: '#D97706',
  Scrapped: '#DC2626',
};

const MoldSchema = z.object({
  code:            z.string().min(1, 'Required').max(30),
  name:            z.string().min(1, 'Required').max(200),
  moldType:        z.string().min(1, 'Required'),
  material:        z.string().max(100).optional().nullable(),
  maxShots:        z.number().int().min(1, 'Required'),
  pmIntervalShots: z.number().int().min(1, 'Required'),
  cavities:        z.number().int().min(1).optional(),
  storageLocation: z.string().max(100).optional().nullable(),
});
type MoldFormValues = z.infer<typeof MoldSchema>;

function MoldForm({ defaultValues, isEdit, onSubmit }: {
  defaultValues: Partial<MoldFormValues>;
  isEdit: boolean;
  onSubmit: (d: MoldFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } =
    useForm<MoldFormValues>({ resolver: zodResolver(MoldSchema), defaultValues });

  return (
    <Box component="form" id="mold-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')} label="Mold Code" fullWidth required disabled={isEdit}
            error={!!errors.code} helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller name="moldType" control={control} render={({ field }) => (
            <TextField {...field} select label="Mold Type" fullWidth required
              error={!!errors.moldType} helperText={errors.moldType?.message}>
              {Object.entries(MoldType).map(([k, v]) => (
                <MenuItem key={k} value={v}>{MOLD_TYPE_LABELS[k] ?? k}</MenuItem>
              ))}
            </TextField>
          )} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('name')} label="Mold Name" fullWidth required
            error={!!errors.name} helperText={errors.name?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('material')} label="Material" fullWidth placeholder="e.g. P20 Steel" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('cavities', { valueAsNumber: true })} label="Cavities" fullWidth type="number"
            slotProps={{ htmlInput: { min: 1 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('maxShots', { valueAsNumber: true })} label="Max Shots" fullWidth required type="number"
            error={!!errors.maxShots} helperText={errors.maxShots?.message}
            slotProps={{ htmlInput: { min: 1 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('pmIntervalShots', { valueAsNumber: true })} label="PM Interval (shots)" fullWidth required type="number"
            error={!!errors.pmIntervalShots} helperText={errors.pmIntervalShots?.message}
            slotProps={{ htmlInput: { min: 1 } }} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('storageLocation')} label="Storage Location" fullWidth
            placeholder="e.g. Rack A-12" />
        </Grid>
      </Grid>
    </Box>
  );
}

type DrawerMode = 'create' | 'edit';

export default function MoldsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch]           = useState('');
  const [typeFilter, setTypeFilter]   = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [drawerOpen, setDrawerOpen]   = useState(false);
  const [drawerMode, setDrawerMode]   = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]   = useState<MoldDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<MoldDto | null>(null);
  const [saveError, setSaveError]     = useState('');

  const { data: molds = [], isLoading, error, refetch } = useGetApiV1Molds();

  const filtered = useMemo(() => {
    let r = molds;
    if (search)       r = r.filter((m) => m.moldCode.toLowerCase().includes(search.toLowerCase()) || m.moldName.toLowerCase().includes(search.toLowerCase()));
    if (typeFilter)   r = r.filter((m) => m.moldType === typeFilter);
    if (statusFilter) r = r.filter((m) => m.status === statusFilter);
    return r;
  }, [molds, search, typeFilter, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1MoldsQueryKey() });

  const createMutation = useMutation({
    mutationFn: (d: MoldFormValues) => postApiV1Molds({
      code: d.code, name: d.name, moldType: d.moldType as never,
      material: d.material ?? null, maxShots: d.maxShots, pmIntervalShots: d.pmIntervalShots,
      cavities: d.cavities, storageLocation: d.storageLocation ?? null,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (d: MoldFormValues) => putApiV1MoldsCode(editTarget!.moldCode, {
      name: d.name, moldType: d.moldType as never, material: d.material ?? null,
      maxShots: d.maxShots, pmIntervalShots: d.pmIntervalShots,
      cavities: d.cavities ?? 1, isActive: editTarget!.isActive,
      storageLocation: d.storageLocation ?? null,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => deleteApiV1MoldsCode(code),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(m: MoldDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(m); setDrawerOpen(true); }
  function handleSave(d: MoldFormValues) { setSaveError(''); drawerMode === 'create' ? createMutation.mutate(d) : updateMutation.mutate(d); }

  const statusColor = (s: string) => STATUS_COLOR[s] ?? '#94A3B8';
  const numVal = (v: number | string) => typeof v === 'number' ? v : parseInt(v, 10);

  const columns: GridColDef<MoldDto>[] = [
    {
      field: 'moldCode', headerName: 'Code', width: 120,
      renderCell: (p: GridRenderCellParams<MoldDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    { field: 'moldName', headerName: 'Name', flex: 1, minWidth: 160 },
    {
      field: 'moldType', headerName: 'Type', width: 110,
      renderCell: (p: GridRenderCellParams<MoldDto>) => (
        <Chip label={MOLD_TYPE_LABELS[p.value as string] ?? p.value} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.secondary.main, 0.1), color: 'secondary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'shotUtilizationPercent', headerName: 'Utilization', width: 140,
      renderCell: (p: GridRenderCellParams<MoldDto>) => {
        const pct = numVal(p.value as number);
        const color = pct >= 90 ? '#DC2626' : pct >= 70 ? '#D97706' : '#15803D';
        return (
          <Box sx={{ width: '100%' }}>
            <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.25 }}>
              <Typography variant="caption" sx={{ fontSize: 11, color }}>{pct.toFixed(1)}%</Typography>
              <Typography variant="caption" sx={{ fontSize: 10, color: 'text.disabled' }}>
                {numVal(p.row.currentShots as number).toLocaleString()} / {numVal(p.row.maxShots as number).toLocaleString()}
              </Typography>
            </Stack>
            <LinearProgress variant="determinate" value={Math.min(pct, 100)}
              sx={{ height: 4, borderRadius: 2, bgcolor: alpha(color, 0.15), '& .MuiLinearProgress-bar': { bgcolor: color } }} />
          </Box>
        );
      },
    },
    {
      field: 'pmDue', headerName: 'PM', width: 70, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<MoldDto>) => p.value
        ? <Chip label="Due" size="small" sx={{ height: 20, fontSize: '0.65rem', fontWeight: 700, bgcolor: alpha('#D97706', 0.12), color: '#D97706', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        : null,
    },
    {
      field: 'status', headerName: 'Status', width: 120,
      renderCell: (p: GridRenderCellParams<MoldDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 500, color: statusColor(p.value as string) }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'cavities', headerName: 'Cav.', width: 60, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<MoldDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>{numVal(p.value as number)}</Typography>
      ),
    },
    {
      field: 'actions', headerName: '', width: 80, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<MoldDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => openEdit(p.row)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton size="small" onClick={() => setDeleteTarget(p.row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
              <SolarIcon name="delete" size={16} />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ];

  const moldTypes = [...new Set(molds.map((m) => m.moldType))];
  const statuses  = [...new Set(molds.map((m) => m.status))];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Molds" breadcrumbs={[{ label: 'Master Data' }, { label: 'Molds' }]} />
      <EmptyState icon="emptyTable" title="Failed to load molds" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Molds"
        subtitle="Manage injection molds, track shot utilization and PM schedules"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Molds' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
            Register Mold
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search code or name…"
        filters={[
          { label: 'Type', value: typeFilter, options: moldTypes.map((t) => ({ label: MOLD_TYPE_LABELS[t] ?? t, value: t })), onChange: setTypeFilter },
          { label: 'Status', value: statusFilter, options: statuses.map((s) => ({ label: s, value: s })), onChange: setStatusFilter },
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
          getRowId={(r) => r.moldCode}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || typeFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || typeFilter || statusFilter ? 'No molds match your filters' : 'No molds registered'}
                description={search || typeFilter || statusFilter ? 'Try adjusting your filters.' : 'Register your first mold to get started.'}
                action={!search && !typeFilter && !statusFilter ? <Button variant="contained" size="small" onClick={openCreate}>Register Mold</Button> : undefined}
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
        open={drawerOpen} onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'Register Mold' : `Edit ${editTarget?.moldCode}`}
        subtitle={drawerMode === 'create' ? 'Enter mold details below' : editTarget?.moldName}
        onSubmit={() => document.getElementById('mold-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Register' : 'Save Changes'}
        loading={createMutation.isPending || updateMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <MoldForm
          key={editTarget?.moldCode ?? 'new'} isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code: editTarget.moldCode, name: editTarget.moldName,
            moldType: editTarget.moldType, material: editTarget.material ?? '',
            maxShots: numVal(editTarget.maxShots as number),
            pmIntervalShots: 10000,
            cavities: numVal(editTarget.cavities as number),
            storageLocation: editTarget.storageLocation ?? '',
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget} onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.moldCode)}
        title="Delete Mold"
        description={<>Delete <strong>{deleteTarget?.moldName}</strong> ({deleteTarget?.moldCode})?</>}
        confirmLabel="Delete" confirmColor="error" loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
