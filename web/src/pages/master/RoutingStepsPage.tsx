import Autocomplete from '@mui/material/Autocomplete';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  FormControlLabel,
  Grid,
  IconButton,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
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
  SolarIcon,
  TablePageSkeleton,
} from '../../components';
import {
  useGetApiV1RoutingsId,
  getGetApiV1RoutingsIdQueryKey,
  postApiV1RoutingsIdSteps,
  deleteApiV1RoutingsStepsStepId,
} from '../../api/routings/routings';
import { useGetApiV1Operations } from '../../api/operations/operations';
import { useGetApiV1WorkCenters } from '../../api/work-centers/work-centers';
import type { RoutingStepDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Form schema ──────────────────────────────────────────────────────────────

const StepSchema = z.object({
  stepNumber:          z.coerce.number().int('Must be a whole number').positive('Must be positive'),
  operationCode:       z.string().min(1, 'Operation is required'),
  defaultWorkCenterId: z.coerce.number().int().positive('Work center is required'),
  standardCycleTime:   z.coerce.number().min(0, 'Cannot be negative'),
  isQcRequired:        z.boolean(),
});

type StepFormValues = z.infer<typeof StepSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function StepForm({
  defaultValues,
  operationOptions,
  workCenterOptions,
  onSubmit,
}: {
  defaultValues: Partial<StepFormValues>;
  operationOptions: { code: string; name: string }[];
  workCenterOptions: { id: number; code: string; name: string }[];
  onSubmit: (data: StepFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<StepFormValues>({
    resolver: zodResolver(StepSchema) as any,
    defaultValues: { standardCycleTime: 0, isQcRequired: false, ...defaultValues },
  });

  return (
    <Box component="form" id="step-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 6 }}>
          <TextField
            {...register('stepNumber')}
            label="Step Number"
            type="number"
            fullWidth
            required
            error={!!errors.stepNumber}
            helperText={errors.stepNumber?.message}
            slotProps={{ htmlInput: { min: 1, step: 1 } }}
          />
        </Grid>
        <Grid size={{ xs: 6 }}>
          <Controller
            name="isQcRequired"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={<Switch checked={field.value} onChange={field.onChange} color="primary" />}
                label="QC Required"
                sx={{ mt: 1, ml: 0 }}
              />
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="operationCode"
            control={control}
            render={({ field }) => (
              <Autocomplete
                options={operationOptions}
                getOptionLabel={(opt) => typeof opt === 'string' ? opt : `${opt.code} — ${opt.name}`}
                isOptionEqualToValue={(opt, val) => opt.code === val.code}
                value={operationOptions.find((o) => o.code === field.value) ?? null}
                onChange={(_, val) => field.onChange(val?.code ?? '')}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Operation"
                    required
                    error={!!errors.operationCode}
                    helperText={errors.operationCode?.message}
                  />
                )}
              />
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="defaultWorkCenterId"
            control={control}
            render={({ field }) => (
              <Autocomplete
                options={workCenterOptions}
                getOptionLabel={(opt) => typeof opt === 'string' ? opt : `${opt.code} — ${opt.name}`}
                isOptionEqualToValue={(opt, val) => opt.id === val.id}
                value={workCenterOptions.find((w) => w.id === Number(field.value)) ?? null}
                onChange={(_, val) => field.onChange(val?.id ?? 0)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Default Work Center"
                    required
                    error={!!errors.defaultWorkCenterId}
                    helperText={errors.defaultWorkCenterId?.message}
                  />
                )}
              />
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('standardCycleTime')}
            label="Standard Cycle Time"
            type="number"
            fullWidth
            error={!!errors.standardCycleTime}
            helperText={errors.standardCycleTime?.message ?? '0 = not set'}
            slotProps={{ htmlInput: { min: 0, step: 0.01 } }}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function RoutingStepsPage() {
  const { id }       = useParams<{ id: string }>();
  const navigate     = useNavigate();
  const queryClient  = useQueryClient();
  const routingId    = Number(id);

  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<RoutingStepDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: routing, isLoading, error, refetch } = useGetApiV1RoutingsId(routingId);
  const { data: operations = [] } = useGetApiV1Operations();
  const { data: workCenters = [] } = useGetApiV1WorkCenters();

  const operationMap = useMemo(
    () => new Map(operations.map((o) => [o.operationCode, o.operationName])),
    [operations],
  );

  const workCenterMap = useMemo(
    () => new Map(workCenters.map((w) => [Number(w.workCenterID), { code: w.workCenterCode, name: w.workCenterName }])),
    [workCenters],
  );

  const operationOptions = useMemo(
    () => operations.filter((o) => o.isActive).map((o) => ({ code: o.operationCode, name: o.operationName })),
    [operations],
  );

  const workCenterOptions = useMemo(
    () => workCenters.filter((w) => w.isActive).map((w) => ({ id: Number(w.workCenterID), code: w.workCenterCode, name: w.workCenterName })),
    [workCenters],
  );

  const steps = useMemo(
    () => [...(routing?.steps ?? [])].sort((a, b) => Number(a.stepNumber) - Number(b.stepNumber)),
    [routing?.steps],
  );

  const nextStepNumber = useMemo(
    () => steps.length > 0 ? Math.max(...steps.map((s) => Number(s.stepNumber))) + 10 : 10,
    [steps],
  );

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1RoutingsIdQueryKey(routingId) });

  const addMutation = useMutation({
    mutationFn: (data: StepFormValues) =>
      postApiV1RoutingsIdSteps(routingId, {
        stepNumber:          data.stepNumber,
        operationCode:       data.operationCode,
        defaultWorkCenterId: data.defaultWorkCenterId,
        standardCycleTime:   data.standardCycleTime,
        isQcRequired:        data.isQcRequired,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (stepId: number) => deleteApiV1RoutingsStepsStepId(stepId),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  function openAdd() { setSaveError(''); setDrawerOpen(true); }
  function handleSave(data: StepFormValues) { setSaveError(''); addMutation.mutate(data); }

  if (isLoading) return <TablePageSkeleton />;
  if (error || !routing) return (
    <PageRoot>
      <PageHeader
        title="Routing Steps"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Routings', href: '/master/routings' }, { label: id ?? '' }, { label: 'Steps' }]}
      />
      <EmptyState icon="emptyTable" title="Failed to load routing" description={error ? getErrorMessage(error) : 'Routing not found'} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title={`Steps — ${routing.routingCode}`}
        subtitle={`${routing.routingName} · Product: ${routing.productCode}`}
        breadcrumbs={[
          { label: 'Master Data' },
          { label: 'Routings', href: '/master/routings' },
          { label: routing.routingCode },
          { label: 'Steps' },
        ]}
        actions={
          <>
            <Button variant="outlined" size="small" startIcon={<SolarIcon name="back" size={16} />} onClick={() => navigate(-1)}>
              Back
            </Button>
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openAdd}>
              Add Step
            </Button>
          </>
        }
      />

      <Card variant="outlined" sx={{ borderRadius: 2 }}>
        <CardContent sx={{ pb: '12px !important' }}>
          <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', fontSize: '0.6875rem', letterSpacing: 0.5 }}>
                Routing Steps
              </Typography>
              <Chip
                label={`${steps.length} step${steps.length !== 1 ? 's' : ''}`}
                size="small"
                sx={(theme) => ({ height: 18, fontSize: '0.625rem', fontWeight: 600, bgcolor: alpha(theme.palette.primary.main, 0.08), color: 'primary.main', border: 'none', '& .MuiChip-label': { px: 0.5 } })}
              />
              {routing.isDefault && (
                <Chip
                  label="Default Routing"
                  size="small"
                  sx={{ height: 18, fontSize: '0.625rem', fontWeight: 600, bgcolor: alpha('#7C3AED', 0.1), color: '#7C3AED', border: 'none', '& .MuiChip-label': { px: 0.5 } }}
                />
              )}
            </Stack>
            <Tooltip title="Refresh">
              <IconButton size="small" onClick={() => refetch()} sx={{ color: 'text.secondary' }}>
                <SolarIcon name="refresh" size={16} />
              </IconButton>
            </Tooltip>
          </Stack>

          {steps.length === 0 ? (
            <Box sx={{ py: 4 }}>
              <EmptyState
                icon="emptyTable"
                title="No steps yet"
                description="Click 'Add Step' to define the operations in sequence for this routing."
                action={<Button variant="contained" size="small" onClick={openAdd}>Add Step</Button>}
                compact
              />
            </Box>
          ) : (
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { py: 0.75, fontWeight: 600, fontSize: '0.75rem', borderColor: 'divider', color: 'text.secondary', bgcolor: (t) => alpha(t.palette.primary.main, 0.03) } }}>
                  <TableCell width={60}>Step #</TableCell>
                  <TableCell width={140}>Operation</TableCell>
                  <TableCell>Operation Name</TableCell>
                  <TableCell width={170}>Work Center</TableCell>
                  <TableCell width={120} align="right">Cycle Time</TableCell>
                  <TableCell width={80} align="center">QC</TableCell>
                  <TableCell width={60} align="center" />
                </TableRow>
              </TableHead>
              <TableBody>
                {steps.map((step) => {
                  const wc = workCenterMap.get(Number(step.defaultWorkCenterID));
                  return (
                    <TableRow
                      key={String(step.routingStepID)}
                      sx={{
                        '& td': { py: 0.5, fontSize: '0.8125rem', borderColor: 'divider' },
                        '&:last-child td': { border: 0 },
                        '&:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.02) },
                      }}
                    >
                      <TableCell>
                        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'text.secondary' }}>
                          {Number(step.stepNumber)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
                          {step.operationCode}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ fontSize: 12 }}>
                          {operationMap.get(step.operationCode) ?? (
                            <Box component="span" sx={{ color: 'text.disabled', fontStyle: 'italic' }}>Unknown</Box>
                          )}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {wc ? (
                          <Stack>
                            <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'text.secondary' }}>
                              {wc.code}
                            </Typography>
                            <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>
                              {wc.name}
                            </Typography>
                          </Stack>
                        ) : (
                          <Typography variant="body2" sx={{ fontSize: 12, color: 'text.disabled', fontStyle: 'italic' }}>
                            ID: {String(step.defaultWorkCenterID)}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell align="right">
                        <Typography variant="body2" sx={{ fontSize: 12, color: Number(step.standardCycleTime) > 0 ? 'text.primary' : 'text.disabled' }}>
                          {Number(step.standardCycleTime) > 0 ? Number(step.standardCycleTime) : '—'}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          label={step.isQcRequired ? 'Yes' : 'No'}
                          size="small"
                          sx={{
                            height: 18,
                            fontSize: '0.625rem',
                            fontWeight: 600,
                            bgcolor: step.isQcRequired ? alpha('#D97706', 0.1) : alpha('#94A3B8', 0.1),
                            color: step.isQcRequired ? '#D97706' : '#94A3B8',
                            border: 'none',
                            '& .MuiChip-label': { px: 0.5 },
                          }}
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Delete Step">
                          <IconButton size="small" onClick={() => setDeleteTarget(step)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
                            <SolarIcon name="delete" size={14} />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}

          <Box sx={{ mt: 2, p: 1.5, borderRadius: 1, bgcolor: (t) => alpha(t.palette.warning.main, 0.06), border: '1px solid', borderColor: (t) => alpha(t.palette.warning.main, 0.2) }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <SolarIcon name="warning" size={14} color="#D97706" />
              <Typography variant="caption" color="text.secondary">
                Routing changes take effect from the next released Work Order.
              </Typography>
            </Stack>
          </Box>
        </CardContent>
      </Card>

      {/* Add Step Drawer */}
      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title="Add Step"
        subtitle={`Step ${nextStepNumber} — ${routing.routingCode}`}
        onSubmit={() => document.getElementById('step-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Add Step"
        loading={addMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <StepForm
          key={String(drawerOpen)}
          defaultValues={{ stepNumber: nextStepNumber }}
          operationOptions={operationOptions}
          workCenterOptions={workCenterOptions}
          onSubmit={handleSave}
        />
      </FormDrawer>

      {/* Confirm delete */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.routingStepID))}
        title="Delete Step"
        description={
          <>
            Delete step <strong>#{Number(deleteTarget?.stepNumber)}</strong> ({deleteTarget?.operationCode}) from this routing?
            This cannot be undone.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
