import {
  Alert,
  Box,
  Button,
  Chip,
  FormControlLabel,
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
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ConfirmDialog,
  EmptyState,
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1IotStateRules,
  getGetApiV1IotStateRulesQueryKey,
  postApiV1IotStateRules,
  putApiV1IotStateRulesId,
  deleteApiV1IotStateRulesId,
  putApiV1IotStateRulesReorder,
} from '../../api/iot/iot';
import type { StateRuleDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const TARGET_STATES = ['Running', 'Down', 'Idle', 'Offline'];
const OPERATORS     = ['Gt', 'Lt', 'Gte', 'Lte', 'Eq', 'Neq', 'Change'];

const OPERATOR_LABELS: Record<string, string> = {
  Gt: '> (greater than)', Lt: '< (less than)', Gte: '≥ (at least)',
  Lte: '≤ (at most)', Eq: '= (equals)', Neq: '≠ (not equals)', Change: '~ (any change)',
};

const STATE_COLORS: Record<string, string> = {
  Running: '#15803D', Down: '#DC2626', Idle: '#D97706', Offline: '#64748B',
};

const RuleSchema = z.object({
  targetState:    z.string().min(1, 'Required'),
  signalTagKey:   z.string().min(1, 'Required').max(100),
  operator:       z.string().min(1, 'Required'),
  thresholdValue: z.coerce.number().nullable().optional(),
  hysteresis:     z.coerce.number().nullable().optional(),
  minDurationMs:  z.coerce.number().int().min(0),
  description:    z.string().max(500).nullable().optional(),
  isActive:       z.boolean(),
});
type RuleForm = z.infer<typeof RuleSchema>;

const DEFAULT_FORM: RuleForm = {
  targetState: 'Running', signalTagKey: '', operator: 'Gt',
  thresholdValue: null, hysteresis: null, minDurationMs: 0, description: null, isActive: true,
};

export default function IotStateRulesPage() {
  const { machineCode = '' } = useParams<{ machineCode: string }>();
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [drawer, setDrawer]             = useState<'create' | 'edit' | null>(null);
  const [editTarget, setEditTarget]     = useState<StateRuleDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<StateRuleDto | null>(null);
  const [toastError, setToastError]     = useState<string | null>(null);

  const { data: rules = [], isLoading, error, refetch } =
    useGetApiV1IotStateRules({ machineCode }, { query: { enabled: !!machineCode } });

  const sorted = useMemo(
    () => [...rules].sort((a, b) => Number(a.priority) - Number(b.priority)),
    [rules],
  );

  const filtered = search
    ? sorted.filter((r) =>
        r.signalTagKey.toLowerCase().includes(search.toLowerCase()) ||
        r.targetState.toLowerCase().includes(search.toLowerCase()) ||
        (r.description ?? '').toLowerCase().includes(search.toLowerCase()))
    : sorted;

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1IotStateRulesQueryKey({ machineCode }) });

  const { control, handleSubmit, reset, watch } = useForm<RuleForm>({
    resolver: zodResolver(RuleSchema) as any,
    defaultValues: DEFAULT_FORM,
  });
  const watchedOperator = watch('operator');

  const createMutation = useMutation({
    mutationFn: (v: RuleForm) =>
      postApiV1IotStateRules({
        machineCode, priority: sorted.length + 1, targetState: v.targetState,
        signalTagKey: v.signalTagKey, operator: v.operator,
        thresholdValue: v.thresholdValue ?? null, hysteresis: v.hysteresis ?? null,
        minDurationMs: v.minDurationMs, description: v.description ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawer(null); reset(DEFAULT_FORM); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const editMutation = useMutation({
    mutationFn: (v: RuleForm) =>
      putApiV1IotStateRulesId(Number(editTarget!.ruleId), {
        targetState: v.targetState, signalTagKey: v.signalTagKey, operator: v.operator,
        thresholdValue: v.thresholdValue ?? null, hysteresis: v.hysteresis ?? null,
        minDurationMs: v.minDurationMs, description: v.description ?? null, isActive: v.isActive,
      }),
    onSuccess: () => { invalidate(); setDrawer(null); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1IotStateRulesId(id),
    onSuccess:  () => { invalidate(); setDeleteTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const reorderMutation = useMutation({
    mutationFn: (orderedRuleIds: number[]) => putApiV1IotStateRulesReorder({ machineCode, orderedRuleIds }),
    onSuccess: invalidate,
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const moveRule = (ruleId: number, direction: 'up' | 'down') => {
    const ids = sorted.map((r) => Number(r.ruleId));
    const idx = ids.indexOf(ruleId);
    if (direction === 'up' && idx === 0) return;
    if (direction === 'down' && idx === ids.length - 1) return;
    const other = direction === 'up' ? idx - 1 : idx + 1;
    [ids[idx], ids[other]] = [ids[other], ids[idx]];
    reorderMutation.mutate(ids);
  };

  const openEdit = (row: StateRuleDto) => {
    setEditTarget(row);
    reset({
      targetState:    row.targetState,
      signalTagKey:   row.signalTagKey,
      operator:       row.operator,
      thresholdValue: row.thresholdValue != null ? Number(row.thresholdValue) : null,
      hysteresis:     row.hysteresis != null ? Number(row.hysteresis) : null,
      minDurationMs:  Number(row.minDurationMs),
      description:    row.description ?? null,
      isActive:       row.isActive,
    });
    setDrawer('edit');
  };

  const columns: GridColDef<StateRuleDto>[] = [
    {
      field: 'priority',
      headerName: '#',
      width: 60,
      align: 'center',
      headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<StateRuleDto>) => (
        <Typography variant="body2" sx={{ fontWeight: 700 }} color="text.secondary">{Number(p.value)}</Typography>
      ),
    },
    {
      field: 'targetState',
      headerName: 'Target State',
      width: 120,
      renderCell: (p: GridRenderCellParams<StateRuleDto>) => (
        <Chip label={p.value} size="small" sx={{
          height: 20, fontSize: '0.6875rem', fontWeight: 600,
          bgcolor: alpha(STATE_COLORS[p.value] ?? '#64748B', 0.1), color: STATE_COLORS[p.value] ?? '#64748B',
          border: 'none', '& .MuiChip-label': { px: 0.75 },
        }} />
      ),
    },
    {
      field: 'signalTagKey',
      headerName: 'Signal Tag',
      width: 160,
      renderCell: (p: GridRenderCellParams<StateRuleDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>{p.value}</Typography>
      ),
    },
    {
      field: 'operator',
      headerName: 'Condition',
      width: 160,
      renderCell: (p: GridRenderCellParams<StateRuleDto>) => (
        <Typography variant="body2" color="text.secondary">
          {p.row.operator}
          {p.row.operator !== 'Change' && p.row.thresholdValue != null ? ` ${Number(p.row.thresholdValue)}` : ''}
          {p.row.hysteresis != null ? ` ±${Number(p.row.hysteresis)}` : ''}
        </Typography>
      ),
    },
    {
      field: 'minDurationMs',
      headerName: 'Min Duration',
      width: 110,
      renderCell: (p: GridRenderCellParams<StateRuleDto>) => {
        const ms = Number(p.value);
        return (
          <Typography variant="body2" color="text.secondary">
            {ms === 0 ? '—' : ms >= 1000 ? `${(ms / 1000).toFixed(1)}s` : `${ms}ms`}
          </Typography>
        );
      },
    },
    {
      field: 'isActive',
      headerName: 'Active',
      width: 80,
      renderCell: (p: GridRenderCellParams<StateRuleDto>) => (
        <Chip label={p.value ? 'Yes' : 'No'} size="small" sx={{
          height: 20, fontSize: '0.6875rem', fontWeight: 600,
          bgcolor: p.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
          color: p.value ? '#15803D' : '#94A3B8',
          border: 'none', '& .MuiChip-label': { px: 0.75 },
        }} />
      ),
    },
    {
      field: 'description',
      headerName: 'Description',
      flex: 1,
      renderCell: (p: GridRenderCellParams<StateRuleDto>) =>
        p.value
          ? <Typography variant="body2" color="text.secondary">{p.value}</Typography>
          : <Typography variant="body2" color="text.disabled">—</Typography>,
    },
    {
      field: 'actions',
      headerName: '',
      width: 140,
      sortable: false,
      align: 'center',
      renderCell: (p: GridRenderCellParams<StateRuleDto>) => {
        const id  = Number(p.row.ruleId);
        const idx = sorted.findIndex((r) => Number(r.ruleId) === id);
        return (
          <Stack direction="row" spacing={0.25}>
            <Tooltip title="Move Up">
              <span>
                <IconButton size="small" disabled={idx === 0 || reorderMutation.isPending}
                  onClick={() => moveRule(id, 'up')} sx={{ color: 'text.secondary' }}>
                  <SolarIcon name="collapse" size={16} />
                </IconButton>
              </span>
            </Tooltip>
            <Tooltip title="Move Down">
              <span>
                <IconButton size="small" disabled={idx === sorted.length - 1 || reorderMutation.isPending}
                  onClick={() => moveRule(id, 'down')} sx={{ color: 'text.secondary' }}>
                  <SolarIcon name="expand" size={16} />
                </IconButton>
              </span>
            </Tooltip>
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
        );
      },
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title={`State Rules — ${machineCode}`} breadcrumbs={[{ label: 'Master Data' }, { label: 'Machines', href: '/master/machines' }, { label: machineCode }, { label: 'State Rules' }]} />
      <EmptyState icon="emptyTable" title="Failed to load state rules" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title={`State Rules — ${machineCode}`}
        subtitle="Priority-ordered rules that determine machine state from signal values. First match wins."
        breadcrumbs={[
          { label: 'Master Data' }, { label: 'Machines', href: '/master/machines' },
          { label: machineCode }, { label: 'State Rules' },
        ]}
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => refetch()} />
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />}
              onClick={() => { reset(DEFAULT_FORM); setDrawer('create'); }}>
              Add Rule
            </Button>
          </Stack>
        }
      />

      {toastError && <Alert severity="error" onClose={() => setToastError(null)} sx={{ mb: 2 }}>{toastError}</Alert>}

      <TableToolbar search={search} onSearchChange={setSearch} searchPlaceholder="Filter by tag key, state or description…" />

      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(row) => row.ruleId}
          disableRowSelectionOnClick
          slots={{ noRowsOverlay: () => <EmptyState icon="emptyTable" title="No state rules" description="Rules are evaluated top-to-bottom — first match wins." /> }}
          sx={{ border: 'none', '& .MuiDataGrid-cell': { alignItems: 'center' } }}
        />
      </Box>

      {/* ── Form drawer ── */}
      <FormDrawer
        open={drawer !== null}
        onClose={() => setDrawer(null)}
        title={drawer === 'create' ? 'Add State Rule' : `Edit Rule #${Number(editTarget?.priority ?? '')}`}
        onSubmit={() => void handleSubmit((v) => drawer === 'create' ? createMutation.mutate(v) : editMutation.mutate(v))()}
        loading={createMutation.isPending || editMutation.isPending}
      >
        <Controller name="targetState" control={control} render={({ field, fieldState }) => (
          <TextField {...field} select label="Target State" fullWidth error={!!fieldState.error} helperText={fieldState.error?.message}>
            {TARGET_STATES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
          </TextField>
        )} />
        <Controller name="signalTagKey" control={control} render={({ field, fieldState }) => (
          <TextField {...field} label="Signal Tag Key" fullWidth error={!!fieldState.error}
            helperText={fieldState.error?.message ?? 'Tag key from signal mappings, e.g. spindle_rpm'} />
        )} />
        <Controller name="operator" control={control} render={({ field, fieldState }) => (
          <TextField {...field} select label="Operator" fullWidth error={!!fieldState.error} helperText={fieldState.error?.message}>
            {OPERATORS.map((op) => <MenuItem key={op} value={op}>{OPERATOR_LABELS[op]}</MenuItem>)}
          </TextField>
        )} />
        {watchedOperator !== 'Change' && (
          <Stack direction="row" spacing={2}>
            <Controller name="thresholdValue" control={control} render={({ field, fieldState }) => (
              <TextField {...field} label="Threshold" type="number" fullWidth
                value={field.value ?? ''}
                onChange={(e) => field.onChange(e.target.value === '' ? null : e.target.value)}
                error={!!fieldState.error} helperText={fieldState.error?.message} />
            )} />
            <Controller name="hysteresis" control={control} render={({ field, fieldState }) => (
              <TextField {...field} label="Hysteresis ±" type="number" fullWidth
                value={field.value ?? ''}
                onChange={(e) => field.onChange(e.target.value === '' ? null : e.target.value)}
                error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Optional deadband'} />
            )} />
          </Stack>
        )}
        <Controller name="minDurationMs" control={control} render={({ field, fieldState }) => (
          <TextField {...field} label="Min Duration (ms)" type="number" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message ?? '0 = immediate; signal must hold this long before state changes'} />
        )} />
        <Controller name="description" control={control} render={({ field, fieldState }) => (
          <TextField {...field} label="Description" fullWidth
            value={field.value ?? ''}
            onChange={(e) => field.onChange(e.target.value || null)}
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        {drawer === 'edit' && (
          <Controller name="isActive" control={control} render={({ field }) => (
            <FormControlLabel
              control={<Switch checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />}
              label="Active"
            />
          )} />
        )}
      </FormDrawer>

      {/* ── Delete confirm ── */}
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete State Rule"
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.ruleId))}
        loading={deleteMutation.isPending}
        description={
          <>
            Delete rule targeting <strong>{deleteTarget?.targetState}</strong> when{' '}
            <strong>{deleteTarget?.signalTagKey}</strong> {deleteTarget?.operator}
            {deleteTarget?.thresholdValue != null ? ` ${Number(deleteTarget.thresholdValue)}` : ''}?
          </>
        }
      />
    </PageRoot>
  );
}
