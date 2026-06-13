import {
  Alert,
  Box,
  Button,
  Chip,
  Divider,
  FormControlLabel,
  IconButton,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useForm, Controller, useFieldArray } from 'react-hook-form';
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
  useGetApiV1Rules,
  getGetApiV1RulesQueryKey,
  postApiV1Rules,
  putApiV1RulesId,
  deleteApiV1RulesId,
  patchApiV1RulesIdToggle,
} from '../../api/rules/rules';
import type { CreateRuleRequest, UpdateRuleRequest, RuleListItemDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const TRIGGER_TYPES = ['SIGNAL_THRESHOLD', 'EVENT', 'SCHEDULE'];
const CONDITION_TYPES = ['METRIC_VALUE', 'STATUS_MATCH'];
const ACTION_TYPES = ['RAISE_ALERT', 'CHANGE_MACHINE_STATE', 'TRIGGER_WEBHOOK'];
const LOGIC_OPS = ['AND', 'OR'];

const ConditionSchema = z.object({
  logicOperator: z.string().min(1),
  conditionType: z.string().min(1),
  conditionConfig: z.string().min(2),
});

const ActionSchema = z.object({
  actionType: z.string().min(1),
  actionConfig: z.string().min(2),
  continueOnFail: z.boolean(),
});

const RuleSchema = z.object({
  code: z.string().min(1, 'Required').max(50).regex(/^[A-Z0-9_-]+$/, 'Use UPPER_CASE, digits, hyphen, underscore only'),
  name: z.string().min(1, 'Required').max(200),
  description: z.string().max(500).nullable().optional(),
  priority: z.number().int().min(1).max(9999),
  triggerType: z.string().min(1, 'Required'),
  triggerConfig: z.string().min(2, 'Must be valid JSON'),
  isActive: z.boolean(),
  conditions: z.array(ConditionSchema),
  actions: z.array(ActionSchema),
});
type RuleForm = z.infer<typeof RuleSchema>;

const DEFAULT_FORM: RuleForm = {
  code: '', name: '', description: null, priority: 100,
  triggerType: 'SIGNAL_THRESHOLD',
  triggerConfig: '{"machineCode":"","signalKey":"","operator":">","value":0}',
  isActive: true,
  conditions: [],
  actions: [{ actionType: 'RAISE_ALERT', actionConfig: '{"alertLevel":"WARNING","message":"Rule fired on {{machineCode}}"}', continueOnFail: true }],
};

const TRIGGER_CONFIG_TEMPLATES: Record<string, string> = {
  SIGNAL_THRESHOLD: '{"machineCode":"","signalKey":"","operator":">","value":0}',
  EVENT: '{"eventType":""}',
  SCHEDULE: '{"cron":"0 * * * *"}',
};

function validateJson(v: string): boolean {
  try { JSON.parse(v); return true; } catch { return false; }
}

export default function RuleEnginePage() {
  const queryClient = useQueryClient();

  const [search, setSearch] = useState('');
  const [filterActive, setFilterActive] = useState('all');
  const [filterTrigger, setFilterTrigger] = useState('');
  const [drawer, setDrawer] = useState<'create' | 'edit' | null>(null);
  const [editTarget, setEditTarget] = useState<RuleListItemDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<RuleListItemDto | null>(null);
  const [toastError, setToastError] = useState<string | null>(null);

  const { data: rules = [], isLoading, error, refetch } = useGetApiV1Rules({
    ...(filterTrigger ? { triggerType: filterTrigger } : {}),
    ...(filterActive !== 'all' ? { isActive: filterActive === 'true' } : {}),
  });

  const filtered = useMemo(() => {
    if (!search) return rules;
    const s = search.toLowerCase();
    return rules.filter((r) =>
      r.code.toLowerCase().includes(s) ||
      r.name.toLowerCase().includes(s) ||
      r.triggerType.toLowerCase().includes(s) ||
      (r.description ?? '').toLowerCase().includes(s),
    );
  }, [rules, search]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1RulesQueryKey() });

  const { control, handleSubmit, reset, watch, setValue, formState: { errors } } =
    useForm<RuleForm>({ resolver: zodResolver(RuleSchema), defaultValues: DEFAULT_FORM });

  const { fields: condFields, append: appendCond, remove: removeCond } = useFieldArray({ control, name: 'conditions' });
  const { fields: actFields, append: appendAct, remove: removeAct } = useFieldArray({ control, name: 'actions' });

  const watchedTriggerType = watch('triggerType');

  const createMut = useMutation({
    mutationFn: (body: CreateRuleRequest) => postApiV1Rules(body),
    onSuccess: () => { invalidate(); setDrawer(null); },
    onError: (e) => setToastError(getErrorMessage(e)),
  });

  const updateMut = useMutation({
    mutationFn: ({ id, body }: { id: number; body: UpdateRuleRequest }) => putApiV1RulesId(id, body),
    onSuccess: () => { invalidate(); setDrawer(null); },
    onError: (e) => setToastError(getErrorMessage(e)),
  });

  const deleteMut = useMutation({
    mutationFn: (id: number) => deleteApiV1RulesId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
    onError: (e) => setToastError(getErrorMessage(e)),
  });

  const toggleMut = useMutation({
    mutationFn: ({ id, isActive }: { id: number; isActive: boolean }) =>
      patchApiV1RulesIdToggle(id, { isActive }),
    onSuccess: () => invalidate(),
    onError: (e) => setToastError(getErrorMessage(e)),
  });

  function openCreate() {
    reset(DEFAULT_FORM);
    setEditTarget(null);
    setDrawer('create');
  }

  function openEdit(r: RuleListItemDto) {
    setEditTarget(r);
    reset({
      code: r.code,
      name: r.name,
      description: r.description ?? null,
      priority: Number(r.priority),
      triggerType: r.triggerType,
      triggerConfig: TRIGGER_CONFIG_TEMPLATES[r.triggerType] ?? '{}',
      isActive: r.isActive,
      conditions: [],
      actions: [],
    });
    setDrawer('edit');
  }

  function onSubmit(data: RuleForm) {
    if (!validateJson(data.triggerConfig)) {
      setToastError('TriggerConfig is not valid JSON');
      return;
    }
    const body = {
      code: data.code,
      name: data.name,
      description: data.description ?? null,
      priority: data.priority,
      triggerType: data.triggerType,
      triggerConfig: data.triggerConfig,
      conditions: data.conditions,
      actions: data.actions,
    };
    if (drawer === 'create') {
      createMut.mutate(body);
    } else if (editTarget) {
      updateMut.mutate({ id: Number(editTarget.ruleId), body });
    }
  }

  const isSaving = createMut.isPending || updateMut.isPending;

  const tableFilters = [
    {
      label: 'Status',
      value: filterActive,
      options: [
        { value: 'all', label: 'All statuses' },
        { value: 'true', label: 'Active' },
        { value: 'false', label: 'Inactive' },
      ],
      onChange: setFilterActive,
      width: 140,
    },
    {
      label: 'Trigger',
      value: filterTrigger,
      options: [
        { value: '', label: 'All triggers' },
        ...TRIGGER_TYPES.map((t) => ({ value: t, label: t })),
      ],
      onChange: setFilterTrigger,
      width: 180,
    },
  ];

  const columns: GridColDef<RuleListItemDto>[] = [
    {
      field: 'code', headerName: 'Code', width: 180,
      renderCell: (p: GridRenderCellParams<RuleListItemDto>) => (
        <Typography variant="body2" sx={{ fontWeight: 600, fontFamily: 'monospace' }}>{p.row.code}</Typography>
      ),
    },
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 180 },
    {
      field: 'triggerType', headerName: 'Trigger', width: 160,
      renderCell: (p: GridRenderCellParams<RuleListItemDto>) => (
        <Chip label={p.row.triggerType} size="small" variant="outlined" />
      ),
    },
    {
      field: 'priority', headerName: 'Priority', width: 90, type: 'number',
      valueGetter: (_, row) => Number(row.priority),
    },
    {
      field: 'conditionCount', headerName: 'Conditions', width: 100,
      valueGetter: (_, row) => Number(row.conditionCount),
    },
    {
      field: 'actionCount', headerName: 'Actions', width: 90,
      valueGetter: (_, row) => Number(row.actionCount),
    },
    {
      field: 'isActive', headerName: 'Active', width: 90,
      renderCell: (p: GridRenderCellParams<RuleListItemDto>) => (
        <Switch
          size="small"
          checked={p.row.isActive}
          onChange={(_, checked) => toggleMut.mutate({ id: Number(p.row.ruleId), isActive: checked })}
          onClick={(e) => e.stopPropagation()}
        />
      ),
    },
    {
      field: 'rowActions', headerName: '', width: 90, sortable: false,
      renderCell: (p: GridRenderCellParams<RuleListItemDto>) => (
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => openEdit(p.row)}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton size="small" color="error" onClick={() => setDeleteTarget(p.row)}>
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
      <Alert severity="error">{getErrorMessage(error)}</Alert>
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Rule Engine"
        breadcrumbs={[{ label: 'IoT' }, { label: 'Rule Engine' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => refetch()} />
            <Button variant="contained" startIcon={<SolarIcon name="add" size={18} />} onClick={openCreate}>
              New Rule
            </Button>
          </Stack>
        }
      />

      {toastError && (
        <Alert severity="error" onClose={() => setToastError(null)} sx={{ mb: 2 }}>
          {toastError}
        </Alert>
      )}

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search code, name, trigger…"
        filters={tableFilters}
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        {filtered.length === 0 ? (
          <EmptyState
            title="No rules found"
            description="Create a rule to automate responses to machine signal events."
            action={<Button variant="contained" onClick={openCreate}>New Rule</Button>}
          />
        ) : (
          <DataGrid
            rows={filtered}
            columns={columns}
            getRowId={(r) => Number(r.ruleId)}
            disableRowSelectionOnClick
            density="compact"
            pageSizeOptions={[25, 50, 100]}
            initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          />
        )}
      </Box>

      {/* Create / Edit Drawer */}
      <FormDrawer
        open={drawer !== null}
        onClose={() => setDrawer(null)}
        title={drawer === 'create' ? 'New Rule' : `Edit Rule — ${editTarget?.code}`}
        onSubmit={handleSubmit(onSubmit)}
        loading={isSaving}
        submitLabel={drawer === 'create' ? 'Create' : 'Save'}
        width={560}
      >
        <Stack spacing={2}>
          <Stack direction="row" spacing={1.5}>
            <Controller
              name="code"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Code"
                  required
                  fullWidth
                  disabled={drawer === 'edit'}
                  error={!!errors.code}
                  helperText={errors.code?.message ?? 'e.g. TEMP_HIGH_ALARM'}
                  slotProps={{ htmlInput: { style: { fontFamily: 'monospace' } } }}
                />
              )}
            />
            <Controller
              name="priority"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  onChange={(e) => field.onChange(parseInt(e.target.value, 10) || 100)}
                  label="Priority"
                  type="number"
                  sx={{ width: 120 }}
                  error={!!errors.priority}
                  helperText={errors.priority?.message}
                />
              )}
            />
          </Stack>

          <Controller
            name="name"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Name"
                required
                fullWidth
                error={!!errors.name}
                helperText={errors.name?.message}
              />
            )}
          />

          <Controller
            name="description"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? ''}
                label="Description"
                fullWidth
                multiline
                rows={2}
              />
            )}
          />

          <Controller
            name="isActive"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={<Switch checked={field.value} onChange={(_, c) => field.onChange(c)} />}
                label="Active"
              />
            )}
          />

          <Divider><Typography variant="caption">Trigger</Typography></Divider>

          <Controller
            name="triggerType"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Trigger Type"
                required
                fullWidth
                onChange={(e) => {
                  field.onChange(e);
                  setValue('triggerConfig', TRIGGER_CONFIG_TEMPLATES[e.target.value] ?? '{}');
                }}
              >
                {TRIGGER_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
              </TextField>
            )}
          />

          <Controller
            name="triggerConfig"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Trigger Config (JSON)"
                fullWidth
                multiline
                rows={4}
                slotProps={{ htmlInput: { style: { fontFamily: 'monospace', fontSize: 12 } } }}
                error={!!errors.triggerConfig}
                helperText={
                  errors.triggerConfig?.message ??
                  (watchedTriggerType === 'SIGNAL_THRESHOLD'
                    ? 'Fields: machineCode, signalKey, operator (> >= < <= == !=), value'
                    : undefined)
                }
              />
            )}
          />

          <Divider>
            <Typography variant="caption">Conditions ({condFields.length})</Typography>
          </Divider>

          {condFields.map((f, i) => (
            <Box key={f.id} sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 1, p: 1.5 }}>
              <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                <Typography variant="caption" sx={{ fontWeight: 600 }}>Condition {i + 1}</Typography>
                <IconButton size="small" color="error" onClick={() => removeCond(i)}>
                  <SolarIcon name="delete" size={14} />
                </IconButton>
              </Stack>
              <Stack spacing={1}>
                <Stack direction="row" spacing={1}>
                  <Controller
                    name={`conditions.${i}.logicOperator`}
                    control={control}
                    render={({ field }) => (
                      <TextField {...field} select label="Logic" size="small" sx={{ width: 90 }}>
                        {LOGIC_OPS.map((op) => <MenuItem key={op} value={op}>{op}</MenuItem>)}
                      </TextField>
                    )}
                  />
                  <Controller
                    name={`conditions.${i}.conditionType`}
                    control={control}
                    render={({ field }) => (
                      <TextField {...field} select label="Type" size="small" fullWidth>
                        {CONDITION_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                      </TextField>
                    )}
                  />
                </Stack>
                <Controller
                  name={`conditions.${i}.conditionConfig`}
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Condition Config (JSON)"
                      size="small"
                      fullWidth
                      slotProps={{ htmlInput: { style: { fontFamily: 'monospace', fontSize: 12 } } }}
                    />
                  )}
                />
              </Stack>
            </Box>
          ))}

          <Button
            variant="outlined"
            size="small"
            startIcon={<SolarIcon name="add" size={14} />}
            onClick={() => appendCond({
              logicOperator: 'AND',
              conditionType: 'METRIC_VALUE',
              conditionConfig: '{"field":"value","operator":">","value":0}',
            })}
          >
            Add Condition
          </Button>

          <Divider>
            <Typography variant="caption">Actions ({actFields.length})</Typography>
          </Divider>

          {actFields.map((f, i) => (
            <Box key={f.id} sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 1, p: 1.5 }}>
              <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                <Typography variant="caption" sx={{ fontWeight: 600 }}>Action {i + 1}</Typography>
                <IconButton size="small" color="error" onClick={() => removeAct(i)}>
                  <SolarIcon name="delete" size={14} />
                </IconButton>
              </Stack>
              <Stack spacing={1}>
                <Controller
                  name={`actions.${i}.actionType`}
                  control={control}
                  render={({ field }) => (
                    <TextField {...field} select label="Action Type" size="small" fullWidth>
                      {ACTION_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                    </TextField>
                  )}
                />
                <Controller
                  name={`actions.${i}.actionConfig`}
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Action Config (JSON)"
                      size="small"
                      fullWidth
                      slotProps={{ htmlInput: { style: { fontFamily: 'monospace', fontSize: 12 } } }}
                    />
                  )}
                />
                <Controller
                  name={`actions.${i}.continueOnFail`}
                  control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Switch size="small" checked={field.value} onChange={(_, c) => field.onChange(c)} />}
                      label={<Typography variant="caption">Continue on failure</Typography>}
                    />
                  )}
                />
              </Stack>
            </Box>
          ))}

          <Button
            variant="outlined"
            size="small"
            startIcon={<SolarIcon name="add" size={14} />}
            onClick={() => appendAct({
              actionType: 'RAISE_ALERT',
              actionConfig: '{"alertLevel":"WARNING","message":"Alert fired"}',
              continueOnFail: true,
            })}
          >
            Add Action
          </Button>
        </Stack>
      </FormDrawer>

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Delete Rule"
        description={`Delete rule "${deleteTarget?.name}" (${deleteTarget?.code})? This cannot be undone.`}
        confirmLabel="Delete"
        confirmColor="error"
        onConfirm={() => deleteTarget && deleteMut.mutate(Number(deleteTarget.ruleId))}
        onClose={() => setDeleteTarget(null)}
        loading={deleteMut.isPending}
      />
    </PageRoot>
  );
}
