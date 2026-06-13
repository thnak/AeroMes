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
  useGetApiV1StorageLocations,
  getGetApiV1StorageLocationsQueryKey,
  postApiV1StorageLocations,
  putApiV1StorageLocationsId,
  deleteApiV1StorageLocationsId,
} from '../../api/storage-locations/storage-locations';
import { useGetApiV1WorkCenters } from '../../api/work-centers/work-centers';
import type { StorageLocationDto } from '../../api/model';
import { LocationType } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Constants ────────────────────────────────────────────────────────────────

const LOCATION_TYPE_LABELS: Record<string, string> = {
  RawMaterial:   'Raw Material',
  Wip:           'WIP',
  FinishedGoods: 'Finished Goods',
  Scrap:         'Scrap',
};

const LOCATION_TYPE_COLORS: Record<string, string> = {
  RawMaterial:   '#1D4ED8',
  Wip:           '#D97706',
  FinishedGoods: '#15803D',
  Scrap:         '#DC2626',
};

const LOCATION_TYPES = Object.values(LocationType) as string[];

// ─── Form schema ──────────────────────────────────────────────────────────────

const StorageSchema = z.object({
  code:          z.string().min(1, 'Code is required').max(20)
    .regex(/^[A-Za-z0-9\-_.]+$/, 'Letters, digits, hyphens, underscores, and dots only'),
  name:          z.string().min(1, 'Name is required').max(200),
  locationType:  z.string().min(1, 'Location type is required'),
  workCenterId:  z.coerce.number().int().positive().nullable().optional(),
});

type StorageFormValues = z.infer<typeof StorageSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function StorageForm({
  defaultValues,
  isEdit,
  workCenterOptions,
  onSubmit,
}: {
  defaultValues: Partial<StorageFormValues>;
  isEdit: boolean;
  workCenterOptions: { id: number; code: string; name: string }[];
  onSubmit: (data: StorageFormValues) => void;
}) {
  const { register, control, handleSubmit, watch, formState: { errors } } = useForm<StorageFormValues>({
    resolver: zodResolver(StorageSchema) as any,
    defaultValues: { workCenterId: null, ...defaultValues },
  });

  const selectedType = watch('locationType');
  const typeColor = LOCATION_TYPE_COLORS[selectedType] ?? '#64748B';

  return (
    <Box component="form" id="storage-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('code')}
            label="Location Code"
            fullWidth
            required
            disabled={isEdit}
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Location Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="locationType"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Location Type"
                fullWidth
                required
                error={!!errors.locationType}
                helperText={errors.locationType?.message}
                slotProps={{
                  select: {
                    renderValue: (val) => val ? (
                      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                        <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: LOCATION_TYPE_COLORS[val as string] ?? '#64748B', flexShrink: 0 }} />
                        <span>{LOCATION_TYPE_LABELS[val as string] ?? val}</span>
                      </Stack>
                    ) : '',
                  },
                }}
              >
                {LOCATION_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>
                    <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                      <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: LOCATION_TYPE_COLORS[t] ?? '#64748B', flexShrink: 0 }} />
                      <span>{LOCATION_TYPE_LABELS[t] ?? t}</span>
                    </Stack>
                  </MenuItem>
                ))}
              </TextField>
            )}
          />
          {selectedType && (
            <Box sx={{ mt: 0.75, display: 'flex', alignItems: 'center', gap: 1 }}>
              <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: typeColor }} />
              <Typography variant="caption" sx={{ color: typeColor, fontWeight: 600 }}>
                {LOCATION_TYPE_LABELS[selectedType] ?? selectedType}
              </Typography>
            </Box>
          )}
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="workCenterId"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Linked Work Center (optional)"
                fullWidth
                value={field.value ?? ''}
                onChange={(e) => field.onChange(e.target.value === '' ? null : Number(e.target.value))}
              >
                <MenuItem value=""><em>None</em></MenuItem>
                {workCenterOptions.map((w) => (
                  <MenuItem key={w.id} value={w.id}>{w.code} — {w.name}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function StorageLocationsPage() {
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [typeFilter, setTypeFilter]     = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<StorageLocationDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<StorageLocationDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: locations = [], isLoading, error, refetch } = useGetApiV1StorageLocations({ activeOnly: false });
  const { data: workCenters = [] } = useGetApiV1WorkCenters({ activeOnly: true });

  const workCenterOptions = useMemo(
    () => workCenters.map((w) => ({ id: Number(w.workCenterID), code: w.workCenterCode, name: w.workCenterName })),
    [workCenters],
  );

  const filtered = useMemo(() => {
    let r = locations;
    if (search)       r = r.filter((l) => l.locationCode.toLowerCase().includes(search.toLowerCase()) || l.locationName.toLowerCase().includes(search.toLowerCase()));
    if (typeFilter)   r = r.filter((l) => l.locationType === typeFilter);
    if (statusFilter) r = r.filter((l) => statusFilter === 'active' ? l.isActive : !l.isActive);
    return r;
  }, [locations, search, typeFilter, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1StorageLocationsQueryKey({ activeOnly: false }) });

  const createMutation = useMutation({
    mutationFn: (data: StorageFormValues) =>
      postApiV1StorageLocations({
        code: data.code,
        name: data.name,
        locationType: data.locationType as typeof LocationType[keyof typeof LocationType],
        workCenterId: data.workCenterId ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: StorageFormValues) =>
      putApiV1StorageLocationsId(Number(editTarget!.locationID), {
        name: data.name,
        locationType: data.locationType as typeof LocationType[keyof typeof LocationType],
        workCenterId: data.workCenterId ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1StorageLocationsId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(l: StorageLocationDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(l); setDrawerOpen(true); }
  function handleSave(data: StorageFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  const columns: GridColDef<StorageLocationDto>[] = [
    {
      field: 'locationCode',
      headerName: 'Code',
      width: 130,
      renderCell: (params: GridRenderCellParams<StorageLocationDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'locationName', headerName: 'Name', flex: 1, minWidth: 180 },
    {
      field: 'locationType',
      headerName: 'Type',
      width: 130,
      renderCell: (params: GridRenderCellParams<StorageLocationDto>) => {
        const color = LOCATION_TYPE_COLORS[params.value as string] ?? '#64748B';
        return (
          <Chip
            label={LOCATION_TYPE_LABELS[params.value as string] ?? params.value}
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
      field: 'workCenterName',
      headerName: 'Work Center',
      flex: 1,
      minWidth: 160,
      renderCell: (params: GridRenderCellParams<StorageLocationDto>) => (
        params.value ? (
          <Typography variant="body2" sx={{ fontSize: 12 }}>{params.value}</Typography>
        ) : (
          <Typography variant="body2" sx={{ fontSize: 12, color: 'text.disabled', fontStyle: 'italic' }}>—</Typography>
        )
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<StorageLocationDto>) => (
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
      renderCell: (params: GridRenderCellParams<StorageLocationDto>) => (
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

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Storage Locations" breadcrumbs={[{ label: 'Master Data' }, { label: 'Storage' }]} />
      <EmptyState icon="emptyTable" title="Failed to load storage locations" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Storage Locations"
        subtitle="Manage warehouse and production storage locations and their work center assignments"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Storage' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Location
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
            options: LOCATION_TYPES.map((t) => ({ label: LOCATION_TYPE_LABELS[t] ?? t, value: t })),
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
            <RefreshButton onClick={() => refetch()} />
          </Stack>
        }
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        <DataGrid
          rows={filtered}
          getRowId={(row) => row.locationID}
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
                title={search || typeFilter || statusFilter ? 'No locations match your filters' : 'No storage locations yet'}
                description={search || typeFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first storage location to get started.'}
                action={!search && !typeFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Location</Button>
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
        title={drawerMode === 'create' ? 'New Location' : `Edit ${editTarget?.locationCode}`}
        subtitle={drawerMode === 'create' ? 'Enter location details below' : editTarget?.locationName}
        onSubmit={() => document.getElementById('storage-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Location' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <StorageForm
          key={editTarget ? String(editTarget.locationID) : 'new'}
          isEdit={drawerMode === 'edit'}
          workCenterOptions={workCenterOptions}
          defaultValues={editTarget ? {
            code:         editTarget.locationCode,
            name:         editTarget.locationName,
            locationType: editTarget.locationType,
            workCenterId: editTarget.workCenterID ? Number(editTarget.workCenterID) : null,
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.locationID))}
        title="Delete Location"
        description={
          <>
            Delete <strong>{deleteTarget?.locationName}</strong> ({deleteTarget?.locationCode})?
            This cannot be undone and will affect inventory assignments for this location.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
