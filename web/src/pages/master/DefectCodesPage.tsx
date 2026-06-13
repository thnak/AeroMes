import {
  Alert,
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
  useGetApiV1QualityDefectCodes,
  getGetApiV1QualityDefectCodesQueryKey,
  postApiV1QualityDefectCodes,
  putApiV1QualityDefectCodesId,
  deleteApiV1QualityDefectCodesId,
} from '../../api/defect-codes/defect-codes';
import type { DefectCodeDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Constants ────────────────────────────────────────────────────────────────

const CATEGORIES = ['Dimensional', 'Visual', 'Functional', 'Material', 'Process'] as const;

const CATEGORY_COLORS: Record<string, string> = {
  Dimensional: '#1D4ED8',
  Visual:      '#D97706',
  Functional:  '#DC2626',
  Material:    '#7C3AED',
  Process:     '#0D9488',
};

// ─── Form schema ──────────────────────────────────────────────────────────────

const DefectCodeSchema = z.object({
  code:     z.string().min(1, 'Code is required').max(30)
    .regex(/^[A-Za-z0-9\-_]+$/, 'Letters, digits, hyphens, and underscores only'),
  name:     z.string().min(1, 'Name is required').max(150),
  category: z.string().max(100).optional(),
  isActive: z.boolean(),
});

type DefectCodeFormValues = z.infer<typeof DefectCodeSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function DefectCodeForm({
  defaultValues,
  isEdit,
  onSubmit,
}: {
  defaultValues: Partial<DefectCodeFormValues>;
  isEdit: boolean;
  onSubmit: (data: DefectCodeFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<DefectCodeFormValues>({
    resolver: zodResolver(DefectCodeSchema),
    defaultValues: { isActive: true, ...defaultValues },
  });

  return (
    <Box component="form" id="defect-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Defect Code"
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
            name="category"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? ''}
                select
                label="Category"
                fullWidth
                error={!!errors.category}
                helperText={errors.category?.message}
              >
                <MenuItem value=""><em>None</em></MenuItem>
                {CATEGORIES.map((c) => (
                  <MenuItem key={c} value={c}>{c}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Defect Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        {isEdit && (
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
        )}
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function DefectCodesPage() {
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<DefectCodeDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<DefectCodeDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: defectCodes = [], isLoading, error, refetch } =
    useGetApiV1QualityDefectCodes({ activeOnly: false });

  const filtered = useMemo(() => {
    let r = defectCodes;
    if (search)         r = r.filter((d) => d.defectName.toLowerCase().includes(search.toLowerCase()) || d.code.toLowerCase().includes(search.toLowerCase()));
    if (categoryFilter) r = r.filter((d) => d.defectCategory === categoryFilter);
    if (statusFilter)   r = r.filter((d) => statusFilter === 'active' ? d.isActive : !d.isActive);
    return r;
  }, [defectCodes, search, categoryFilter, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1QualityDefectCodesQueryKey({ activeOnly: false }) });

  const createMutation = useMutation({
    mutationFn: (data: DefectCodeFormValues) =>
      postApiV1QualityDefectCodes({ code: data.code, defectName: data.name, defectCategory: data.category ?? null }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: DefectCodeFormValues) =>
      putApiV1QualityDefectCodesId(Number(editTarget!.defectCodeId), {
        defectName: data.name,
        defectCategory: data.category ?? null,
        isActive: data.isActive,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1QualityDefectCodesId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(d: DefectCodeDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(d); setDrawerOpen(true); }

  function handleSave(data: DefectCodeFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  const columns: GridColDef<DefectCodeDto>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<DefectCodeDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'defectName', headerName: 'Name', width: 200 },
    {
      field: 'defectCategory',
      headerName: 'Category',
      width: 130,
      renderCell: (params: GridRenderCellParams<DefectCodeDto>) => {
        if (!params.value) return <Typography variant="body2" color="text.disabled" sx={{ fontSize: 12, fontStyle: 'italic' }}>—</Typography>;
        const color = CATEGORY_COLORS[params.value as string] ?? '#64748B';
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
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<DefectCodeDto>) => (
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
      renderCell: (params: GridRenderCellParams<DefectCodeDto>) => (
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
      <PageHeader title="Defect Codes" breadcrumbs={[{ label: 'Master Data' }, { label: 'Defect Codes' }]} />
      <EmptyState icon="emptyTable" title="Failed to load defect codes" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Defect Codes"
        subtitle="Manage the defect code library used in quality inspections and non-conformance reports"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Defect Codes' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Defect Code
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
            label: 'Category',
            value: categoryFilter,
            options: CATEGORIES.map((c) => ({ label: c, value: c })),
            onChange: setCategoryFilter,
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
          getRowId={(row) => row.defectCodeId}
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
                icon={search || categoryFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || categoryFilter || statusFilter ? 'No defect codes match your filters' : 'No defect codes yet'}
                description={search || categoryFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first defect code to get started.'}
                action={!search && !categoryFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Defect Code</Button>
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
        title={drawerMode === 'create' ? 'New Defect Code' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter defect code details below' : editTarget?.defectName}
        onSubmit={() => document.getElementById('defect-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Defect Code' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <DefectCodeForm
          key={editTarget ? String(editTarget.defectCodeId) : 'new'}
          isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code:     editTarget.code,
            name:     editTarget.defectName,
            category: editTarget.defectCategory ?? undefined,
            isActive: editTarget.isActive,
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.defectCodeId))}
        title="Delete Defect Code"
        description={
          <>
            Delete <strong>{deleteTarget?.defectName}</strong> ({deleteTarget?.code})?
            This cannot be undone and may affect quality records that reference this code.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
