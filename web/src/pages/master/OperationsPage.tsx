import {
  Alert,
  Box,
  Button,
  Chip,
  FormControlLabel,
  Grid,
  IconButton,
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
  useGetApiV1Operations,
  getGetApiV1OperationsQueryKey,
  postApiV1Operations,
  putApiV1OperationsCode,
  deleteApiV1OperationsCode,
} from '../../api/operations/operations';
import type { OperationDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Form schema ──────────────────────────────────────────────────────────────

const OperationSchema = z.object({
  code:        z.string().min(1, 'Code is required').max(20),
  name:        z.string().min(1, 'Name is required').max(200),
  description: z.string().optional(),
  isActive:    z.boolean(),
});

type OperationFormValues = z.infer<typeof OperationSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function OperationForm({
  defaultValues,
  isEdit,
  onSubmit,
}: {
  defaultValues: Partial<OperationFormValues>;
  isEdit: boolean;
  onSubmit: (data: OperationFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<OperationFormValues>({
    resolver: zodResolver(OperationSchema),
    defaultValues: { isActive: true, ...defaultValues },
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
            disabled={isEdit}
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
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
                sx={{ mt: 1, ml: 0 }}
              />
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
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<OperationDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<OperationDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: operations = [], isLoading, error, refetch } = useGetApiV1Operations();

  const filtered = useMemo(() => {
    let r = operations;
    if (search)       r = r.filter((o) => o.operationName.toLowerCase().includes(search.toLowerCase()) || o.operationCode.toLowerCase().includes(search.toLowerCase()));
    if (statusFilter) r = r.filter((o) => statusFilter === 'active' ? o.isActive : !o.isActive);
    return r;
  }, [operations, search, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1OperationsQueryKey() });

  const createMutation = useMutation({
    mutationFn: (data: OperationFormValues) =>
      postApiV1Operations({ code: data.code, name: data.name, description: data.description ?? null }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: OperationFormValues) =>
      putApiV1OperationsCode(editTarget!.operationCode, { name: data.name, description: data.description ?? null }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => deleteApiV1OperationsCode(code),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(o: OperationDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(o); setDrawerOpen(true); }

  function handleSave(data: OperationFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  const columns: GridColDef<OperationDto>[] = [
    {
      field: 'operationCode',
      headerName: 'Code',
      width: 130,
      renderCell: (params: GridRenderCellParams<OperationDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'operationName', headerName: 'Name', flex: 1, minWidth: 160 },
    {
      field: 'description',
      headerName: 'Description',
      flex: 1,
      minWidth: 200,
      renderCell: (params: GridRenderCellParams<OperationDto>) => (
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
      renderCell: (params: GridRenderCellParams<OperationDto>) => (
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
      renderCell: (params: GridRenderCellParams<OperationDto>) => (
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
      <PageHeader title="Operations" breadcrumbs={[{ label: 'Master Data' }, { label: 'Operations' }]} />
      <EmptyState icon="emptyTable" title="Failed to load operations" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Operations"
        subtitle="Define production operations referenced in routings"
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
          getRowId={(row) => row.operationCode}
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
                title={search || statusFilter ? 'No operations match your filters' : 'No operations yet'}
                description={search || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first operation to get started.'}
                action={!search && !statusFilter ? (
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
        title={drawerMode === 'create' ? 'New Operation' : `Edit ${editTarget?.operationCode}`}
        subtitle={drawerMode === 'create' ? 'Enter operation details below' : editTarget?.operationName}
        onSubmit={() => document.getElementById('op-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Operation' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <OperationForm
          key={editTarget?.operationCode ?? 'new'}
          isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code:        editTarget.operationCode,
            name:        editTarget.operationName,
            description: editTarget.description ?? undefined,
            isActive:    editTarget.isActive,
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.operationCode)}
        title="Delete Operation"
        description={
          <>
            Delete <strong>{deleteTarget?.operationName}</strong> ({deleteTarget?.operationCode})?
            This cannot be undone and may affect routings that reference this operation.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
