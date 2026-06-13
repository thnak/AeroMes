import {
  Alert,
  Box,
  Button,
  Chip,
  Grid,
  IconButton,
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
import { useForm } from 'react-hook-form';
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
  useGetApiV1WorkCenters,
  getGetApiV1WorkCentersQueryKey,
  postApiV1WorkCenters,
  putApiV1WorkCentersId,
  deleteApiV1WorkCentersId,
} from '../../api/work-centers/work-centers';
import type { WorkCenterDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Form schema ──────────────────────────────────────────────────────────────

const WorkCenterSchema = z.object({
  code:        z.string().min(1, 'Code is required').max(20)
    .regex(/^[A-Za-z0-9\-_]+$/, 'Letters, digits, hyphens, and underscores only'),
  name:        z.string().min(1, 'Name is required').max(200),
  description: z.string().max(500).optional(),
});

type WorkCenterFormValues = z.infer<typeof WorkCenterSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function WorkCenterForm({
  defaultValues,
  isEdit,
  onSubmit,
}: {
  defaultValues: Partial<WorkCenterFormValues>;
  isEdit: boolean;
  onSubmit: (data: WorkCenterFormValues) => void;
}) {
  const { register, handleSubmit, formState: { errors } } = useForm<WorkCenterFormValues>({
    resolver: zodResolver(WorkCenterSchema),
    defaultValues,
  });

  return (
    <Box component="form" id="wc-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('code')}
            label="Work Center Code"
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
            label="Work Center Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
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
  const navigate    = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<WorkCenterDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<WorkCenterDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: workCenters = [], isLoading, error, refetch } =
    useGetApiV1WorkCenters({ activeOnly: false });

  const filtered = useMemo(() => {
    let r = workCenters;
    if (search)       r = r.filter((w) => w.workCenterName.toLowerCase().includes(search.toLowerCase()) || w.workCenterCode.toLowerCase().includes(search.toLowerCase()));
    if (statusFilter) r = r.filter((w) => statusFilter === 'active' ? w.isActive : !w.isActive);
    return r;
  }, [workCenters, search, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1WorkCentersQueryKey({ activeOnly: false }) });

  const createMutation = useMutation({
    mutationFn: (data: WorkCenterFormValues) =>
      postApiV1WorkCenters({ code: data.code, name: data.name, description: data.description ?? null }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: WorkCenterFormValues) =>
      putApiV1WorkCentersId(Number(editTarget!.workCenterID), { name: data.name, description: data.description ?? null }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1WorkCentersId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(w: WorkCenterDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(w); setDrawerOpen(true); }

  function handleSave(data: WorkCenterFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  const columns: GridColDef<WorkCenterDto>[] = [
    {
      field: 'workCenterCode',
      headerName: 'Code',
      width: 120,
      renderCell: (params: GridRenderCellParams<WorkCenterDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'workCenterName', headerName: 'Name', flex: 1, minWidth: 180 },
    {
      field: 'description',
      headerName: 'Description',
      flex: 1,
      minWidth: 200,
      renderCell: (params: GridRenderCellParams<WorkCenterDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12, color: params.value ? 'text.primary' : 'text.disabled', fontStyle: params.value ? 'normal' : 'italic' }}>
          {params.value ?? 'No description'}
        </Typography>
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<WorkCenterDto>) => (
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
      renderCell: (params: GridRenderCellParams<WorkCenterDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View Detail">
            <IconButton size="small" onClick={() => navigate(`/master/work-centers/${params.row.workCenterID}`)} sx={{ color: 'text.secondary' }}>
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

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Work Centers" breadcrumbs={[{ label: 'Master Data' }, { label: 'Work Centers' }]} />
      <EmptyState icon="emptyTable" title="Failed to load work centers" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Work Centers"
        subtitle="Define and manage production work centers and their descriptions"
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
          getRowId={(row) => row.workCenterID}
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
                icon={search || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || statusFilter ? 'No work centers match your filters' : 'No work centers yet'}
                description={search || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first work center to get started.'}
                action={!search && !statusFilter ? (
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
        title={drawerMode === 'create' ? 'New Work Center' : `Edit ${editTarget?.workCenterCode}`}
        subtitle={drawerMode === 'create' ? 'Enter work center details below' : editTarget?.workCenterName}
        onSubmit={() => document.getElementById('wc-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Work Center' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <WorkCenterForm
          key={editTarget ? String(editTarget.workCenterID) : 'new'}
          isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code:        editTarget.workCenterCode,
            name:        editTarget.workCenterName,
            description: editTarget.description ?? undefined,
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.workCenterID))}
        title="Delete Work Center"
        description={
          <>
            Delete <strong>{deleteTarget?.workCenterName}</strong> ({deleteTarget?.workCenterCode})?
            This cannot be undone and may affect machines and routings assigned to this work center.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
