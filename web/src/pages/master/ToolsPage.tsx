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
  useGetApiV1Tools,
  getGetApiV1ToolsQueryKey,
  postApiV1Tools,
  putApiV1ToolsCode,
  deleteApiV1ToolsCode,
} from '../../api/tools/tools';
import type { ToolDto } from '../../api/model';
import { ToolType } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const TOOL_TYPE_LABELS: Record<string, string> = {
  CuttingTool: 'Cutting Tool', Fixture: 'Fixture', Jig: 'Jig',
  Gauge: 'Gauge', Clamp: 'Clamp', Die: 'Die',
  Electrode: 'Electrode', ConsumableTool: 'Consumable',
};

const STATUS_COLOR: Record<string, string> = {
  Available: '#15803D', CheckedOut: '#2563EB', InService: '#D97706', Scrapped: '#DC2626',
};

const ToolSchema = z.object({
  code:              z.string().min(1, 'Required').max(30),
  name:              z.string().min(1, 'Required').max(200),
  toolType:          z.string().min(1, 'Required'),
  brand:             z.string().max(100).optional().nullable(),
  model:             z.string().max(100).optional().nullable(),
  specification:     z.string().max(500).optional().nullable(),
  maxUsageCount:     z.number().int().min(1).optional().nullable(),
  pmIntervalCount:   z.number().int().min(1).optional().nullable(),
  requiresCalibration: z.boolean().optional(),
  storageLocation:   z.string().max(100).optional().nullable(),
});
type ToolFormValues = z.infer<typeof ToolSchema>;

function ToolForm({ defaultValues, isEdit, onSubmit }: {
  defaultValues: Partial<ToolFormValues>;
  isEdit: boolean;
  onSubmit: (d: ToolFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } =
    useForm<ToolFormValues>({ resolver: zodResolver(ToolSchema), defaultValues });

  return (
    <Box component="form" id="tool-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')} label="Tool Code" fullWidth required disabled={isEdit}
            error={!!errors.code} helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller name="toolType" control={control} render={({ field }) => (
            <TextField {...field} select label="Tool Type" fullWidth required
              error={!!errors.toolType} helperText={errors.toolType?.message}>
              {Object.entries(ToolType).map(([k, v]) => (
                <MenuItem key={k} value={v}>{TOOL_TYPE_LABELS[k] ?? k}</MenuItem>
              ))}
            </TextField>
          )} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('name')} label="Tool Name" fullWidth required
            error={!!errors.name} helperText={errors.name?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('brand')} label="Brand" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('model')} label="Model" fullWidth />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('specification')} label="Specification" fullWidth multiline rows={2} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('maxUsageCount', { valueAsNumber: true })} label="Max Usage Count" fullWidth type="number"
            slotProps={{ htmlInput: { min: 1 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('pmIntervalCount', { valueAsNumber: true })} label="PM Interval" fullWidth type="number"
            slotProps={{ htmlInput: { min: 1 } }} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('storageLocation')} label="Storage Location" fullWidth />
        </Grid>
      </Grid>
    </Box>
  );
}

type DrawerMode = 'create' | 'edit';

export default function ToolsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch]             = useState('');
  const [typeFilter, setTypeFilter]     = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<ToolDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ToolDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: tools = [], isLoading, error, refetch } = useGetApiV1Tools();

  const filtered = useMemo(() => {
    let r = tools;
    if (search)       r = r.filter((t) => t.toolCode.toLowerCase().includes(search.toLowerCase()) || t.toolName.toLowerCase().includes(search.toLowerCase()));
    if (typeFilter)   r = r.filter((t) => t.toolType === typeFilter);
    if (statusFilter) r = r.filter((t) => t.status === statusFilter);
    return r;
  }, [tools, search, typeFilter, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1ToolsQueryKey() });

  const createMutation = useMutation({
    mutationFn: (d: ToolFormValues) => postApiV1Tools({
      code: d.code, name: d.name, toolType: d.toolType as never,
      brand: d.brand ?? null, model: d.model ?? null,
      specification: d.specification ?? null,
      maxUsageCount: d.maxUsageCount ?? null,
      pmIntervalCount: d.pmIntervalCount ?? null,
      storageLocation: d.storageLocation ?? null,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (d: ToolFormValues) => putApiV1ToolsCode(editTarget!.toolCode, {
      name: d.name, toolType: d.toolType as never, isActive: editTarget!.isActive,
      brand: d.brand ?? null, model: d.model ?? null,
      specification: d.specification ?? null,
      maxUsageCount: d.maxUsageCount ?? null,
      pmIntervalCount: d.pmIntervalCount ?? null,
      storageLocation: d.storageLocation ?? null,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => deleteApiV1ToolsCode(code),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(t: ToolDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(t); setDrawerOpen(true); }
  function handleSave(d: ToolFormValues) { setSaveError(''); drawerMode === 'create' ? createMutation.mutate(d) : updateMutation.mutate(d); }

  const numVal = (v: number | string | null | undefined) => (v == null ? null : typeof v === 'number' ? v : parseInt(v, 10));

  const columns: GridColDef<ToolDto>[] = [
    {
      field: 'toolCode', headerName: 'Code', width: 120,
      renderCell: (p: GridRenderCellParams<ToolDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    { field: 'toolName', headerName: 'Name', flex: 1, minWidth: 160 },
    {
      field: 'toolType', headerName: 'Type', width: 120,
      renderCell: (p: GridRenderCellParams<ToolDto>) => (
        <Chip label={TOOL_TYPE_LABELS[p.value as string] ?? p.value} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.secondary.main, 0.1), color: 'secondary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'usagePercent', headerName: 'Usage', width: 130,
      renderCell: (p: GridRenderCellParams<ToolDto>) => {
        const pct = numVal(p.value as number | string | null);
        if (pct == null) return <Typography variant="caption" color="text.disabled" sx={{ fontSize: 11 }}>—</Typography>;
        const color = pct >= 90 ? '#DC2626' : pct >= 70 ? '#D97706' : '#15803D';
        return (
          <Box sx={{ width: '100%' }}>
            <Typography variant="caption" sx={{ fontSize: 11, color }}>{pct.toFixed(1)}%</Typography>
            <LinearProgress variant="determinate" value={Math.min(pct, 100)}
              sx={{ height: 4, borderRadius: 2, mt: 0.25, bgcolor: alpha(color, 0.15), '& .MuiLinearProgress-bar': { bgcolor: color } }} />
          </Box>
        );
      },
    },
    {
      field: 'reconditioningDue', headerName: 'PM', width: 65, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<ToolDto>) => p.value
        ? <Chip label="Due" size="small" sx={{ height: 20, fontSize: '0.65rem', fontWeight: 700, bgcolor: alpha('#D97706', 0.12), color: '#D97706', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        : null,
    },
    {
      field: 'status', headerName: 'Status', width: 120,
      renderCell: (p: GridRenderCellParams<ToolDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 500, color: STATUS_COLOR[p.value as string] ?? '#94A3B8' }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'requiresCalibration', headerName: 'Cal.', width: 65, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<ToolDto>) => p.value
        ? <SolarIcon name="quality" size={14} sx={{ color: '#2563EB' }} />
        : null,
    },
    {
      field: 'actions', headerName: '', width: 80, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<ToolDto>) => (
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

  const toolTypes = [...new Set(tools.map((t) => t.toolType))];
  const statuses  = [...new Set(tools.map((t) => t.status))];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Tools & Fixtures" breadcrumbs={[{ label: 'Master Data' }, { label: 'Tools' }]} />
      <EmptyState icon="emptyTable" title="Failed to load tools" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Tools & Fixtures"
        subtitle="Manage cutting tools, jigs, gauges and fixtures with calibration tracking"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Tools' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
            Register Tool
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search code or name…"
        filters={[
          { label: 'Type', value: typeFilter, options: toolTypes.map((t) => ({ label: TOOL_TYPE_LABELS[t] ?? t, value: t })), onChange: setTypeFilter },
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
          getRowId={(r) => r.toolCode}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || typeFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || typeFilter || statusFilter ? 'No tools match your filters' : 'No tools registered'}
                description={search || typeFilter || statusFilter ? 'Try adjusting your filters.' : 'Register your first tool to get started.'}
                action={!search && !typeFilter && !statusFilter ? <Button variant="contained" size="small" onClick={openCreate}>Register Tool</Button> : undefined}
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
        title={drawerMode === 'create' ? 'Register Tool' : `Edit ${editTarget?.toolCode}`}
        subtitle={drawerMode === 'create' ? 'Enter tool details below' : editTarget?.toolName}
        onSubmit={() => document.getElementById('tool-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Register' : 'Save Changes'}
        loading={createMutation.isPending || updateMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <ToolForm
          key={editTarget?.toolCode ?? 'new'} isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code: editTarget.toolCode, name: editTarget.toolName,
            toolType: editTarget.toolType, brand: editTarget.brand ?? '',
            model: editTarget.model ?? '', specification: editTarget.specification ?? '',
            maxUsageCount: numVal(editTarget.maxUsageCount), pmIntervalCount: null,
            requiresCalibration: editTarget.requiresCalibration,
            storageLocation: editTarget.storageLocation ?? '',
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget} onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.toolCode)}
        title="Delete Tool"
        description={<>Delete <strong>{deleteTarget?.toolName}</strong> ({deleteTarget?.toolCode})?</>}
        confirmLabel="Delete" confirmColor="error" loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
