import {
  Alert,
  Box,
  Button,
  Chip,
  Grid,
  IconButton,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
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
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1QualityInspectionPlans,
  getGetApiV1QualityInspectionPlansQueryKey,
  postApiV1QualityInspectionPlans,
  putApiV1QualityInspectionPlansId,
  deleteApiV1QualityInspectionPlansId,
  patchApiV1QualityInspectionPlansIdActivate,
  patchApiV1QualityInspectionPlansIdDeactivate,
} from '../../api/quality/quality';
import type { InspectionPlanListDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Zod schema ───────────────────────────────────────────────────────────────

const PlanSchema = z.object({
  code: z
    .string()
    .min(1, 'Code is required')
    .max(50)
    .regex(/^[A-Z0-9\-]+$/, 'Uppercase letters, digits, hyphens only'),
  name: z.string().min(1, 'Name is required').max(200),
  routingStepId: z.number().int().positive('Must be a positive integer'),
  productCode: z.string().max(50).nullable().optional(),
  samplingMethod: z.enum(['FULL', 'AQL', 'FIXED_N']),
  sampleSize: z.number().int().positive().nullable().optional(),
  acceptNumber: z.number().int().min(0, 'Must be 0 or more'),
  rejectNumber: z.number().int().min(1, 'Must be at least 1'),
  inspectionType: z.enum(['DIMENSIONAL', 'VISUAL', 'FUNCTIONAL', 'COMBINED']),
  notes: z.string().max(500).nullable().optional(),
});

type PlanFormValues = z.infer<typeof PlanSchema>;

// ─── Chip helpers ─────────────────────────────────────────────────────────────

const INSPECTION_TYPE_COLORS: Record<string, string> = {
  DIMENSIONAL: '#1D4ED8',
  VISUAL:      '#15803D',
  FUNCTIONAL:  '#C2410C',
  COMBINED:    '#7C3AED',
};

const SAMPLING_METHOD_COLORS: Record<string, string> = {
  FULL:    '#0F766E',
  AQL:     '#1D4ED8',
  FIXED_N: '#64748B',
};

// ─── Form ─────────────────────────────────────────────────────────────────────

function PlanForm({
  defaultValues,
  isEdit,
  onSubmit,
  saveError,
}: {
  defaultValues: Partial<PlanFormValues>;
  isEdit: boolean;
  onSubmit: (data: PlanFormValues) => void;
  saveError: string;
}) {
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<PlanFormValues>({
    resolver: zodResolver(PlanSchema),
    defaultValues: {
      samplingMethod: 'FULL',
      inspectionType: 'DIMENSIONAL',
      acceptNumber: 0,
      rejectNumber: 1,
      ...defaultValues,
    },
  });

  return (
    <Box component="form" id="plan-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        {saveError && (
          <Grid size={{ xs: 12 }}>
            <Alert severity="error" sx={{ fontSize: 12 }}>
              {saveError}
            </Alert>
          </Grid>
        )}

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Plan Code"
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
            name="inspectionType"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Inspection Type"
                fullWidth
                required
                error={!!errors.inspectionType}
                helperText={errors.inspectionType?.message}
              >
                <MenuItem value="DIMENSIONAL">Dimensional</MenuItem>
                <MenuItem value="VISUAL">Visual</MenuItem>
                <MenuItem value="FUNCTIONAL">Functional</MenuItem>
                <MenuItem value="COMBINED">Combined</MenuItem>
              </TextField>
            )}
          />
        </Grid>

        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Plan Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('routingStepId', { valueAsNumber: true })}
            label="Routing Step ID"
            type="number"
            fullWidth
            required
            error={!!errors.routingStepId}
            helperText={errors.routingStepId?.message}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('productCode')}
            label="Product Code"
            fullWidth
            error={!!errors.productCode}
            helperText={errors.productCode?.message ?? 'Optional — leave blank to apply to all products'}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="samplingMethod"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Sampling Method"
                fullWidth
                required
                error={!!errors.samplingMethod}
                helperText={errors.samplingMethod?.message}
              >
                <MenuItem value="FULL">Full</MenuItem>
                <MenuItem value="AQL">AQL</MenuItem>
                <MenuItem value="FIXED_N">Fixed N</MenuItem>
              </TextField>
            )}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('sampleSize', { valueAsNumber: true, setValueAs: (v: string) => v === '' ? null : Number(v) })}
            label="Sample Size"
            type="number"
            fullWidth
            error={!!errors.sampleSize}
            helperText={errors.sampleSize?.message ?? 'Required for AQL / Fixed N methods'}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('acceptNumber', { valueAsNumber: true })}
            label="Accept Number (Ac)"
            type="number"
            fullWidth
            required
            error={!!errors.acceptNumber}
            helperText={errors.acceptNumber?.message}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('rejectNumber', { valueAsNumber: true })}
            label="Reject Number (Re)"
            type="number"
            fullWidth
            required
            error={!!errors.rejectNumber}
            helperText={errors.rejectNumber?.message}
          />
        </Grid>

        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('notes')}
            label="Notes"
            fullWidth
            multiline
            rows={3}
            error={!!errors.notes}
            helperText={errors.notes?.message}
            placeholder="Optional notes…"
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function InspectionPlanPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch]           = useState('');
  const [typeFilter, setTypeFilter]   = useState('');
  const [activeFilter, setActiveFilter] = useState<'' | 'true' | 'false'>('');
  const [drawerOpen, setDrawerOpen]   = useState(false);
  const [drawerMode, setDrawerMode]   = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]   = useState<InspectionPlanListDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<InspectionPlanListDto | null>(null);
  const [saveError, setSaveError]     = useState('');
  const [formKey, setFormKey]         = useState(0);

  // ─── Query ──────────────────────────────────────────────────────────────────

  const queryParams = useMemo(() => ({
    ...(activeFilter !== '' ? { isActive: activeFilter === 'true' } : {}),
  }), [activeFilter]);

  const { data: plans = [], isLoading, error, refetch } = useGetApiV1QualityInspectionPlans(queryParams);

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1QualityInspectionPlansQueryKey() });

  // ─── Mutations ──────────────────────────────────────────────────────────────

  const createMutation = useMutation({
    mutationFn: (data: PlanFormValues) =>
      postApiV1QualityInspectionPlans({
        code: data.code,
        name: data.name,
        routingStepId: data.routingStepId,
        productCode: data.productCode ?? null,
        samplingMethod: data.samplingMethod,
        sampleSize: data.sampleSize ?? null,
        acceptNumber: data.acceptNumber,
        rejectNumber: data.rejectNumber,
        inspectionType: data.inspectionType,
        notes: data.notes ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: PlanFormValues) =>
      putApiV1QualityInspectionPlansId(Number(editTarget!.planId), {
        name: data.name,
        routingStepId: data.routingStepId,
        productCode: data.productCode ?? null,
        samplingMethod: data.samplingMethod,
        sampleSize: data.sampleSize ?? null,
        acceptNumber: data.acceptNumber,
        rejectNumber: data.rejectNumber,
        inspectionType: data.inspectionType,
        notes: data.notes ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1QualityInspectionPlansId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const activateMutation = useMutation({
    mutationFn: (id: number) => patchApiV1QualityInspectionPlansIdActivate(id),
    onSuccess: () => invalidate(),
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: number) => patchApiV1QualityInspectionPlansIdDeactivate(id),
    onSuccess: () => invalidate(),
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  // ─── Filter (client-side search + type filter) ────────────────────────────

  const filtered = useMemo(() => {
    let r = plans;
    if (search) {
      const q = search.toLowerCase();
      r = r.filter(
        (p) =>
          p.code.toLowerCase().includes(q) ||
          p.name.toLowerCase().includes(q) ||
          (p.productCode ?? '').toLowerCase().includes(q),
      );
    }
    if (typeFilter) r = r.filter((p) => p.inspectionType === typeFilter);
    return r;
  }, [plans, search, typeFilter]);

  // ─── Drawer helpers ────────────────────────────────────────────────────────

  function openCreate() {
    setSaveError('');
    setDrawerMode('create');
    setEditTarget(null);
    setFormKey((k) => k + 1);
    setDrawerOpen(true);
  }

  function openEdit(p: InspectionPlanListDto) {
    setSaveError('');
    setDrawerMode('edit');
    setEditTarget(p);
    setFormKey((k) => k + 1);
    setDrawerOpen(true);
  }

  function handleSave(data: PlanFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  // ─── Columns ───────────────────────────────────────────────────────────────

  const columns: GridColDef<InspectionPlanListDto>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 140,
      renderCell: (params: GridRenderCellParams<InspectionPlanListDto>) => (
        <Typography
          variant="body2"
          sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}
        >
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'name',
      headerName: 'Name',
      flex: 1,
      minWidth: 180,
    },
    {
      field: 'inspectionType',
      headerName: 'Inspection Type',
      width: 150,
      renderCell: (params: GridRenderCellParams<InspectionPlanListDto>) => {
        const color = INSPECTION_TYPE_COLORS[params.value as string] ?? '#64748B';
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{
              height: 20,
              fontSize: '0.6875rem',
              fontWeight: 600,
              bgcolor: alpha(color, 0.08),
              color,
              border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'samplingMethod',
      headerName: 'Sampling',
      width: 105,
      renderCell: (params: GridRenderCellParams<InspectionPlanListDto>) => {
        const color = SAMPLING_METHOD_COLORS[params.value as string] ?? '#64748B';
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{
              height: 20,
              fontSize: '0.6875rem',
              fontWeight: 600,
              bgcolor: alpha(color, 0.08),
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
      renderCell: (params: GridRenderCellParams<InspectionPlanListDto>) => (
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
      field: 'characteristicCount',
      headerName: 'Chars',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionPlanListDto>) => (
        <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', justifyContent: 'center' }}>
          <SolarIcon name="complete" size={14} />
          <Typography variant="body2" sx={{ fontSize: 12 }}>
            {params.value}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 110,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<InspectionPlanListDto>) => {
        const id = Number(params.row.planId);
        const isToggling =
          activateMutation.isPending || deactivateMutation.isPending;
        return (
          <Stack direction="row" spacing={0.25} onClick={(e) => e.stopPropagation()}>
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={() => openEdit(params.row)}
                sx={{ color: 'text.secondary' }}
              >
                <SolarIcon name="edit" size={16} />
              </IconButton>
            </Tooltip>
            <Tooltip title={params.row.isActive ? 'Deactivate' : 'Activate'}>
              <IconButton
                size="small"
                disabled={isToggling}
                onClick={() =>
                  params.row.isActive
                    ? deactivateMutation.mutate(id)
                    : activateMutation.mutate(id)
                }
                sx={{ color: 'text.secondary' }}
              >
                <SolarIcon name={params.row.isActive ? 'pause' : 'resume'} size={16} />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete">
              <IconButton
                size="small"
                onClick={() => setDeleteTarget(params.row)}
                sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}
              >
                <SolarIcon name="delete" size={16} />
              </IconButton>
            </Tooltip>
          </Stack>
        );
      },
    },
  ];

  // ─── Loading / error states ───────────────────────────────────────────────

  if (isLoading) return <TablePageSkeleton />;
  if (error) {
    return (
      <PageRoot>
        <PageHeader
          title="Inspection Plans"
          breadcrumbs={[{ label: 'Quality' }, { label: 'Inspection Plans' }]}
        />
        <EmptyState
          icon="emptyTable"
          title="Failed to load inspection plans"
          description={getErrorMessage(error)}
        />
      </PageRoot>
    );
  }

  const hasFilters = !!(search || typeFilter || activeFilter);

  return (
    <PageRoot>
      <PageHeader
        title="Inspection Plans"
        subtitle="Define QC sampling plans and inspection triggers per routing step"
        breadcrumbs={[{ label: 'Quality' }, { label: 'Inspection Plans' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={openCreate}
          >
            New Plan
          </Button>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search code, name or product…"
        totalCount={filtered.length}
        actions={
          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
            {/* Inspection type filter */}
            <Select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              size="small"
              displayEmpty
              sx={{ minWidth: 140 }}
            >
              <MenuItem value="">All Types</MenuItem>
              <MenuItem value="DIMENSIONAL">Dimensional</MenuItem>
              <MenuItem value="VISUAL">Visual</MenuItem>
              <MenuItem value="FUNCTIONAL">Functional</MenuItem>
              <MenuItem value="COMBINED">Combined</MenuItem>
            </Select>

            {/* Active / inactive filter */}
            <Select
              value={activeFilter}
              onChange={(e) => setActiveFilter(e.target.value as '' | 'true' | 'false')}
              size="small"
              displayEmpty
              sx={{ minWidth: 120 }}
            >
              <MenuItem value="">All Status</MenuItem>
              <MenuItem value="true">Active</MenuItem>
              <MenuItem value="false">Inactive</MenuItem>
            </Select>

            <ExportButton />
            <RefreshButton onClick={() => void refetch()} />
          </Stack>
        }
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        <DataGrid
          rows={filtered}
          getRowId={(row) => Number(row.planId)}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          onRowClick={(params) =>
            navigate(`/quality/inspection-plans/${Number(params.row.planId)}/characteristics`)
          }
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={hasFilters ? 'emptySearch' : 'emptyTable'}
                title={hasFilters ? 'No plans match your filters' : 'No inspection plans yet'}
                description={
                  hasFilters
                    ? 'Try adjusting your search or filters.'
                    : 'Create your first inspection plan to get started.'
                }
                action={
                  !hasFilters ? (
                    <Button variant="contained" size="small" onClick={openCreate}>
                      New Plan
                    </Button>
                  ) : undefined
                }
                compact
              />
            ),
          }}
          sx={{
            border: '1px solid',
            borderColor: 'divider',
            borderRadius: 2,
            bgcolor: 'background.paper',
            cursor: 'pointer',
            '& .MuiDataGrid-columnHeaders': {
              bgcolor: (t) => alpha(t.palette.primary.main, 0.04),
              borderBottom: '1px solid',
              borderColor: 'divider',
            },
            '& .MuiDataGrid-row:hover': {
              bgcolor: (t) => alpha(t.palette.primary.main, 0.03),
            },
            '& .MuiDataGrid-cell:focus, & .MuiDataGrid-cell:focus-within': { outline: 'none' },
            '& .MuiDataGrid-columnHeader:focus, & .MuiDataGrid-columnHeader:focus-within': {
              outline: 'none',
            },
          }}
        />
      </Box>

      {/* Create / Edit drawer */}
      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'New Inspection Plan' : `Edit ${editTarget?.code ?? ''}`}
        subtitle={
          drawerMode === 'create'
            ? 'Define sampling method and inspection parameters'
            : editTarget?.name
        }
        onSubmit={() => void (document.getElementById('plan-form') as HTMLFormElement | null)?.requestSubmit()}
        submitLabel={drawerMode === 'create' ? 'Create Plan' : 'Save Changes'}
        loading={saving}
      >
        <PlanForm
          key={formKey}
          isEdit={drawerMode === 'edit'}
          saveError={saveError}
          defaultValues={
            editTarget
              ? {
                  code: editTarget.code,
                  name: editTarget.name,
                  routingStepId: Number(editTarget.routingStepId),
                  productCode: editTarget.productCode ?? undefined,
                  samplingMethod: editTarget.samplingMethod as PlanFormValues['samplingMethod'],
                  sampleSize:
                    editTarget.sampleSize != null ? Number(editTarget.sampleSize) : undefined,
                  acceptNumber: Number(editTarget.acceptNumber),
                  rejectNumber: Number(editTarget.rejectNumber),
                  inspectionType: editTarget.inspectionType as PlanFormValues['inspectionType'],
                }
              : {}
          }
          onSubmit={handleSave}
        />
      </FormDrawer>

      {/* Delete confirmation */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteMutation.mutate(Number(deleteTarget!.planId))}
        title="Delete Inspection Plan"
        description={
          <>
            Delete plan <strong>{deleteTarget?.code}</strong>
            {deleteTarget?.productCode ? (
              <>
                {' '}for product <strong>{deleteTarget.productCode}</strong>
              </>
            ) : null}
            ? This cannot be undone.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
