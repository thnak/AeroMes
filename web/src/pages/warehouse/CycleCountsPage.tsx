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
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1CycleCounts,
  getGetApiV1CycleCountsQueryKey,
  postApiV1CycleCounts,
  postApiV1CycleCountsIdGenerateLines,
  postApiV1CycleCountsIdRecordLine,
  postApiV1CycleCountsIdSubmit,
  postApiV1CycleCountsIdApprove,
  deleteApiV1CycleCountsId,
  useGetApiV1CycleCountsId,
} from '../../api/cycle-counts/cycle-counts';
import type { CycleCountPlanSummaryDto } from '../../api/model';
import { CycleCountPlanStatus, CycleCountPlanType, CycleCountLineStatus } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const STATUS_COLORS: Record<string, string> = {
  Draft:            '#94A3B8',
  InProgress:       '#D97706',
  PendingApproval:  '#0891B2',
  Completed:        '#15803D',
  Cancelled:        '#DC2626',
};

function numVal(v: number | string | null | undefined): number {
  if (v == null) return 0;
  return typeof v === 'number' ? v : parseFloat(v as string) || 0;
}

// ─── Create plan form ─────────────────────────────────────────────────────────

const PlanSchema = z.object({
  planType:      z.string().min(1, 'Required'),
  scheduledDate: z.string().min(1, 'Required'),
  notes:         z.string().max(500).optional().nullable(),
});
type PlanForm = z.infer<typeof PlanSchema>;

function PlanCreateForm({ onSubmit }: { onSubmit: (d: PlanForm) => void }) {
  const today = new Date().toISOString().split('T')[0];
  const { register, control, handleSubmit, formState: { errors } } = useForm<PlanForm>({
    resolver: zodResolver(PlanSchema),
    defaultValues: { planType: CycleCountPlanType.Full, scheduledDate: today, notes: '' },
  });
  return (
    <Box component="form" id="cycle-count-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <Controller name="planType" control={control} render={({ field }) => (
            <TextField {...field} select label="Count Type" fullWidth required error={!!errors.planType} helperText={errors.planType?.message}>
              {Object.values(CycleCountPlanType).map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
            </TextField>
          )} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('scheduledDate')} label="Scheduled Date" type="date"
            fullWidth required error={!!errors.scheduledDate} helperText={errors.scheduledDate?.message}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('notes')} label="Notes" fullWidth multiline rows={2} />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Detail panel (sheet + variance) ─────────────────────────────────────────

function CountPlanDetail({
  planId,
  onClose,
  invalidatePlans,
}: {
  planId: number;
  onClose: () => void;
  invalidatePlans: () => void;
}) {
  const queryClient = useQueryClient();
  const [countInputs, setCountInputs] = useState<Record<number, string>>({});

  const { data: detail, isLoading, error, refetch } = useGetApiV1CycleCountsId(planId);
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['/api/v1/cycle-counts', planId] });
    invalidatePlans();
  };

  const recordMutation = useMutation({
    mutationFn: ({ lineId, qty }: { lineId: number; qty: number }) =>
      postApiV1CycleCountsIdRecordLine(planId, { lineId, countedQty: qty }),
    onSuccess: () => { invalidate(); },
  });

  const submitMutation = useMutation({
    mutationFn: () => postApiV1CycleCountsIdSubmit(planId),
    onSuccess: () => { invalidate(); },
  });

  const approveMutation = useMutation({
    mutationFn: () => postApiV1CycleCountsIdApprove(planId, { varianceThresholdPct: 5, notes: null }),
    onSuccess: () => { invalidate(); onClose(); },
  });

  const generateMutation = useMutation({
    mutationFn: () => postApiV1CycleCountsIdGenerateLines(planId, { binIds: null }),
    onSuccess: () => invalidate(),
  });

  if (isLoading) return <Box sx={{ p: 3 }}><LinearProgress /></Box>;
  if (error || !detail) return (
    <Box sx={{ p: 3 }}>
      <Alert severity="error">{getErrorMessage(error)}</Alert>
    </Box>
  );

  const lines = detail.lines ?? [];
  const counted = lines.filter((l) => l.status !== CycleCountLineStatus.Pending).length;
  const progressPct = lines.length > 0 ? (counted / lines.length) * 100 : 0;
  const isPending = detail.status === CycleCountPlanStatus.InProgress;
  const isApproval = detail.status === CycleCountPlanStatus.PendingApproval;

  function handleRecord(lineId: number) {
    const qty = parseFloat(countInputs[lineId] ?? '');
    if (isNaN(qty)) return;
    recordMutation.mutate({ lineId, qty });
    setCountInputs((prev) => { const n = { ...prev }; delete n[lineId]; return n; });
  }

  return (
    <Box sx={{ p: 2.5 }}>
      {/* Header */}
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 700 }}>{detail.planCode}</Typography>
          <Typography variant="caption" color="text.secondary">{detail.planType} · {new Date(detail.scheduledDate).toLocaleDateString()}</Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          {detail.status === CycleCountPlanStatus.Draft && lines.length === 0 && (
            <Button size="small" variant="outlined" onClick={() => generateMutation.mutate()} disabled={generateMutation.isPending}>
              Generate Lines
            </Button>
          )}
          {isPending && counted === lines.length && lines.length > 0 && (
            <Button size="small" variant="contained" color="primary" onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending}>
              Submit for Approval
            </Button>
          )}
          {isApproval && (
            <Button size="small" variant="contained" color="success" onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending}>
              Approve & Adjust
            </Button>
          )}
          <RefreshButton onClick={() => refetch()} />
          <IconButton size="small" onClick={onClose}><SolarIcon name="close" size={18} /></IconButton>
        </Stack>
      </Stack>

      {/* Progress */}
      {lines.length > 0 && (
        <Box sx={{ mb: 2 }}>
          <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
            <Typography variant="caption" color="text.secondary">Progress</Typography>
            <Typography variant="caption" sx={{ fontWeight: 600 }}>{counted} / {lines.length} lines</Typography>
          </Stack>
          <LinearProgress variant="determinate" value={progressPct}
            sx={{ height: 6, borderRadius: 3, bgcolor: (t) => alpha(t.palette.primary.main, 0.1), '& .MuiLinearProgress-bar': { borderRadius: 3 } }} />
        </Box>
      )}

      {/* Count sheet (blind) — InProgress */}
      {isPending && (
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>Count Sheet</Typography>
          {lines.length === 0 ? (
            <Alert severity="info">No lines generated. Click "Generate Lines" to create the count sheet.</Alert>
          ) : (
            <Stack spacing={1.5}>
              {lines.filter((l) => l.status === CycleCountLineStatus.Pending).map((line) => {
                const lid = numVal(line.lineId);
                return (
                  <Box key={lid} sx={{
                    p: 1.5, borderRadius: 2, border: '1.5px solid', borderColor: 'divider',
                    bgcolor: (t) => alpha(t.palette.primary.main, 0.02),
                  }}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                      <Box>
                        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 700, fontSize: 13, color: 'primary.main' }}>
                          {line.productCode}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Lot: <span style={{ fontFamily: 'ui-monospace, monospace' }}>{line.lotNumber}</span>
                          {' · '}Bin ID: {line.binId}
                        </Typography>
                      </Box>
                      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                        <TextField
                          size="small" type="number" label="Count Qty"
                          value={countInputs[lid] ?? ''}
                          onChange={(e) => setCountInputs((p) => ({ ...p, [lid]: e.target.value }))}
                          onKeyDown={(e) => e.key === 'Enter' && handleRecord(lid)}
                          sx={{ width: 110 }}
                          slotProps={{ htmlInput: { min: 0, step: 'any' } }}
                        />
                        <Button size="small" variant="contained"
                          onClick={() => handleRecord(lid)}
                          disabled={recordMutation.isPending || !countInputs[lid]}>
                          Submit
                        </Button>
                      </Stack>
                    </Stack>
                  </Box>
                );
              })}
              {/* Counted lines */}
              {lines.filter((l) => l.status !== CycleCountLineStatus.Pending).length > 0 && (
                <Box sx={{ mt: 1 }}>
                  <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                    Counted ({lines.filter((l) => l.status !== CycleCountLineStatus.Pending).length})
                  </Typography>
                  <Stack spacing={0.5} sx={{ mt: 0.5 }}>
                    {lines.filter((l) => l.status !== CycleCountLineStatus.Pending).map((line) => (
                      <Stack key={numVal(line.lineId)} direction="row" spacing={1} sx={{ px: 1.5, py: 0.75, borderRadius: 1, bgcolor: alpha('#15803D', 0.05), border: '1px solid', borderColor: alpha('#15803D', 0.2) }}>
                        <SolarIcon name="success" size={14} color="#15803D" />
                        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>{line.productCode}</Typography>
                        <Typography variant="caption" color="text.secondary">Qty: {numVal(line.countedQty)}</Typography>
                      </Stack>
                    ))}
                  </Stack>
                </Box>
              )}
            </Stack>
          )}
        </Box>
      )}

      {/* Variance review — PendingApproval / Completed */}
      {(isApproval || detail.status === CycleCountPlanStatus.Completed) && (
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>Variance Review</Typography>
          {lines.length === 0 ? (
            <Alert severity="info">No lines to review.</Alert>
          ) : (
            <Box sx={{ overflowX: 'auto' }}>
              <Table size="small">
                <TableHead>
                  <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 11 } }}>
                    <TableCell>Product</TableCell>
                    <TableCell>Lot</TableCell>
                    <TableCell align="right">Book Qty</TableCell>
                    <TableCell align="right">Counted</TableCell>
                    <TableCell align="right">Variance</TableCell>
                    <TableCell align="right">Var %</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {lines.map((line) => {
                    const book = numVal(line.bookQty);
                    const counted2 = numVal(line.countedQty);
                    const variance = numVal(line.varianceQty);
                    const pct = numVal(line.variancePct);
                    const absVariance = Math.abs(pct);
                    const rowColor = absVariance === 0 ? '#15803D' : absVariance <= 5 ? '#D97706' : '#DC2626';
                    return (
                      <TableRow key={numVal(line.lineId)} hover>
                        <TableCell sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
                          {line.productCode}
                        </TableCell>
                        <TableCell sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, color: 'text.secondary' }}>
                          {line.lotNumber}
                        </TableCell>
                        <TableCell align="right" sx={{ fontSize: 12 }}>{book.toLocaleString()}</TableCell>
                        <TableCell align="right" sx={{ fontSize: 12 }}>{counted2.toLocaleString()}</TableCell>
                        <TableCell align="right" sx={{ fontSize: 12, fontWeight: 600, color: variance !== 0 ? rowColor : 'text.primary' }}>
                          {variance > 0 ? '+' : ''}{variance.toLocaleString()}
                        </TableCell>
                        <TableCell align="right">
                          <Chip label={`${pct > 0 ? '+' : ''}${pct.toFixed(1)}%`} size="small"
                            sx={{ height: 18, fontSize: '0.625rem', fontWeight: 700, bgcolor: alpha(rowColor, 0.1), color: rowColor, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </Box>
          )}
        </Box>
      )}
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function CycleCountsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [createOpen, setCreateOpen]     = useState(false);
  const [selectedPlanId, setSelectedPlanId] = useState<number | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<CycleCountPlanSummaryDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: resp, isLoading, error, refetch } = useGetApiV1CycleCounts(
    statusFilter ? { status: statusFilter as CycleCountPlanSummaryDto['status'] } : undefined,
  );
  const plans: CycleCountPlanSummaryDto[] = (resp ?? []) as CycleCountPlanSummaryDto[];

  const filtered = useMemo(() => {
    if (!search) return plans;
    const q = search.toLowerCase();
    return plans.filter((p) => p.planCode.toLowerCase().includes(q) || p.planType.toLowerCase().includes(q));
  }, [plans, search]);

  const invalidatePlans = () => queryClient.invalidateQueries({ queryKey: getGetApiV1CycleCountsQueryKey() });

  const createMutation = useMutation({
    mutationFn: (d: PlanForm) => postApiV1CycleCounts({
      planType: d.planType as CycleCountPlanSummaryDto['planType'],
      scheduledDate: d.scheduledDate,
      notes: d.notes ?? null,
    }),
    onSuccess: () => { invalidatePlans(); setCreateOpen(false); setSaveError(''); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1CycleCountsId(id),
    onSuccess: () => { invalidatePlans(); setDeleteTarget(null); },
  });

  const columns: GridColDef<CycleCountPlanSummaryDto>[] = [
    {
      field: 'planCode', headerName: 'Plan Code', width: 150,
      renderCell: (p: GridRenderCellParams<CycleCountPlanSummaryDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main', cursor: 'pointer' }}
          onClick={() => setSelectedPlanId(numVal(p.row.planId))}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'planType', headerName: 'Type', width: 140,
      renderCell: (p: GridRenderCellParams<CycleCountPlanSummaryDto>) => (
        <Chip label={p.value} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.primary.main, 0.08), color: 'primary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'status', headerName: 'Status', width: 140,
      renderCell: (p: GridRenderCellParams<CycleCountPlanSummaryDto>) => {
        const color = STATUS_COLORS[p.value as string] ?? '#94A3B8';
        return (
          <Chip label={p.value} size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        );
      },
    },
    {
      field: 'scheduledDate', headerName: 'Scheduled', width: 120,
      renderCell: (p: GridRenderCellParams<CycleCountPlanSummaryDto>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>
          {p.value ? new Date(p.value as string).toLocaleDateString() : '—'}
        </Typography>
      ),
    },
    { field: 'createdBy', headerName: 'Created By', flex: 1, minWidth: 120 },
    {
      field: 'actions', headerName: '', width: 100, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<CycleCountPlanSummaryDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="Open">
            <IconButton size="small" onClick={() => setSelectedPlanId(numVal(p.row.planId))} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="view" size={16} />
            </IconButton>
          </Tooltip>
          {p.row.status === CycleCountPlanStatus.Draft && (
            <Tooltip title="Delete">
              <IconButton size="small" onClick={() => setDeleteTarget(p.row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
                <SolarIcon name="delete" size={16} />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      ),
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Cycle Counts" breadcrumbs={[{ label: 'Warehouse' }, { label: 'Cycle Counts' }]} />
      <EmptyState icon="emptyTable" title="Failed to load cycle count plans" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Cycle Counts"
        subtitle="Manage physical inventory count plans and variance approvals"
        breadcrumbs={[{ label: 'Warehouse' }, { label: 'Cycle Counts' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => { setSaveError(''); setCreateOpen(true); }}>
            New Plan
          </Button>
        }
      />

      {selectedPlanId ? (
        <Box sx={{ bgcolor: 'background.paper', borderRadius: 2, border: '1px solid', borderColor: 'divider', flex: 1 }}>
          <CountPlanDetail
            planId={selectedPlanId}
            onClose={() => setSelectedPlanId(null)}
            invalidatePlans={invalidatePlans}
          />
        </Box>
      ) : (
        <>
          <TableToolbar
            search={search} onSearchChange={setSearch} searchPlaceholder="Search plan code or type…"
            filters={[
              {
                label: 'Status', value: statusFilter,
                options: Object.values(CycleCountPlanStatus).map((s) => ({ label: s, value: s })),
                onChange: setStatusFilter,
              },
            ]}
            totalCount={filtered.length}
            actions={<RefreshButton onClick={() => refetch()} />}
          />

          <Box sx={{ flex: 1, minHeight: 400 }}>
            <DataGrid
              rows={filtered}
              getRowId={(r) => String(r.planId)}
              columns={columns}
              density="compact"
              disableRowSelectionOnClick
              pageSizeOptions={[10, 25, 50]}
              initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
              slots={{
                noRowsOverlay: () => (
                  <EmptyState
                    icon={search || statusFilter ? 'emptySearch' : 'emptyTable'}
                    title={search || statusFilter ? 'No plans match your filters' : 'No cycle count plans yet'}
                    description={search || statusFilter ? 'Try adjusting your search.' : 'Create your first count plan.'}
                    action={!search && !statusFilter ? <Button variant="contained" size="small" onClick={() => setCreateOpen(true)}>New Plan</Button> : undefined}
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
        </>
      )}

      <FormDrawer
        open={createOpen} onClose={() => setCreateOpen(false)}
        title="New Cycle Count Plan"
        subtitle="Select count type and schedule date"
        onSubmit={() => document.getElementById('cycle-count-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Create Plan"
        loading={createMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <PlanCreateForm onSubmit={(d) => createMutation.mutate(d)} />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget} onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(numVal(deleteTarget.planId))}
        title="Delete Cycle Count Plan"
        description={<>Delete plan <strong>{deleteTarget?.planCode}</strong>?</>}
        confirmLabel="Delete" confirmColor="error" loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
