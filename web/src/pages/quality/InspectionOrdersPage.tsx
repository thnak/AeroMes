import {
  Alert,
  Box,
  Chip,
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
import { useState } from 'react';
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
  useGetApiV1QualityInspectionOrders,
  getGetApiV1QualityInspectionOrdersQueryKey,
  patchApiV1QualityInspectionOrdersIdAssign,
  patchApiV1QualityInspectionOrdersIdStart,
  patchApiV1QualityInspectionOrdersIdPass,
  patchApiV1QualityInspectionOrdersIdFail,
  patchApiV1QualityInspectionOrdersIdWaive,
} from '../../api/quality/quality';
import type { InspectionOrderListDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const STATUS_CONFIG: Record<string, { label: string; color: string }> = {
  PENDING:     { label: 'Pending',     color: '#D97706' },
  ASSIGNED:    { label: 'Assigned',    color: '#4F46E5' },
  IN_PROGRESS: { label: 'In Progress', color: '#0891B2' },
  PASSED:      { label: 'Passed',      color: '#15803D' },
  FAILED:      { label: 'Failed',      color: '#DC2626' },
  WAIVED:      { label: 'Waived',      color: '#64748B' },
};

const AssignSchema = z.object({ inspectorCode: z.string().min(1, 'Required').max(100) });
type AssignForm = z.infer<typeof AssignSchema>;

const WaiveSchema = z.object({
  waivedBy: z.string().min(1, 'Required').max(100),
  reason:   z.string().min(1, 'Required').max(500),
});
type WaiveForm = z.infer<typeof WaiveSchema>;

export default function InspectionOrdersPage() {
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [assignTarget, setAssignTarget] = useState<InspectionOrderListDto | null>(null);
  const [waiveTarget, setWaiveTarget]   = useState<InspectionOrderListDto | null>(null);
  const [passTarget, setPassTarget]     = useState<InspectionOrderListDto | null>(null);
  const [failTarget, setFailTarget]     = useState<InspectionOrderListDto | null>(null);
  const [toastError, setToastError]     = useState<string | null>(null);

  const queryParams = statusFilter ? { status: statusFilter } : undefined;
  const { data: orders = [], isLoading, error, refetch } =
    useGetApiV1QualityInspectionOrders(queryParams);

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1QualityInspectionOrdersQueryKey(queryParams) });

  const filtered = search
    ? orders.filter((o) =>
        o.orderNo.toLowerCase().includes(search.toLowerCase()) ||
        o.productCode.toLowerCase().includes(search.toLowerCase()) ||
        (o.inspectorCode ?? '').toLowerCase().includes(search.toLowerCase()))
    : orders;

  const assignForm = useForm<AssignForm>({
    resolver: zodResolver(AssignSchema) as any,
    defaultValues: { inspectorCode: '' },
  });
  const waiveForm = useForm<WaiveForm>({
    resolver: zodResolver(WaiveSchema) as any,
    defaultValues: { waivedBy: '', reason: '' },
  });

  const assignMutation = useMutation({
    mutationFn: ({ id, inspectorCode }: { id: number; inspectorCode: string }) =>
      patchApiV1QualityInspectionOrdersIdAssign(id, { inspectorCode }),
    onSuccess: () => { invalidate(); setAssignTarget(null); assignForm.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const startMutation = useMutation({
    mutationFn: (id: number) => patchApiV1QualityInspectionOrdersIdStart(id),
    onSuccess:  () => invalidate(),
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const passMutation = useMutation({
    mutationFn: (id: number) => patchApiV1QualityInspectionOrdersIdPass(id),
    onSuccess:  () => { invalidate(); setPassTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const failMutation = useMutation({
    mutationFn: (id: number) => patchApiV1QualityInspectionOrdersIdFail(id),
    onSuccess:  () => { invalidate(); setFailTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const waiveMutation = useMutation({
    mutationFn: ({ id, body }: { id: number; body: WaiveForm }) =>
      patchApiV1QualityInspectionOrdersIdWaive(id, body),
    onSuccess: () => { invalidate(); setWaiveTarget(null); waiveForm.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const columns: GridColDef<InspectionOrderListDto>[] = [
    {
      field: 'orderNo',
      headerName: 'Order No.',
      width: 150,
      renderCell: (p: GridRenderCellParams<InspectionOrderListDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>{p.value}</Typography>
      ),
    },
    { field: 'productCode', headerName: 'Product', width: 120 },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (p: GridRenderCellParams<InspectionOrderListDto>) => {
        const cfg = STATUS_CONFIG[p.value as string] ?? { label: p.value as string, color: '#64748B' };
        return (
          <Chip
            label={cfg.label}
            size="small"
            sx={{
              height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha(cfg.color, 0.1), color: cfg.color, border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'triggeredBy',
      headerName: 'Triggered',
      width: 120,
      renderCell: (p: GridRenderCellParams<InspectionOrderListDto>) => {
        const isAuto = p.value === 'AUTO_ON_STEP_COMPLETE';
        return (
          <Chip
            label={isAuto ? 'Auto' : 'Manual'}
            size="small"
            sx={{
              height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha(isAuto ? '#7C3AED' : '#0891B2', 0.1),
              color: isAuto ? '#7C3AED' : '#0891B2',
              border: 'none', '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'inspectorCode',
      headerName: 'Inspector',
      flex: 1,
      renderCell: (p: GridRenderCellParams<InspectionOrderListDto>) =>
        p.value ? (
          <Typography variant="body2">{p.value as string}</Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">—</Typography>
        ),
    },
    {
      field: 'sampleSize',
      headerName: 'Sample',
      width: 80,
      renderCell: (p: GridRenderCellParams<InspectionOrderListDto>) => (
        <Typography variant="body2">{Number(p.value)}</Typography>
      ),
    },
    {
      field: 'createdAt',
      headerName: 'Created',
      width: 155,
      renderCell: (p: GridRenderCellParams<InspectionOrderListDto>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.8125rem' }}>
          {new Date(p.value as string).toLocaleString()}
        </Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 200,
      sortable: false,
      align: 'center',
      renderCell: (p: GridRenderCellParams<InspectionOrderListDto>) => {
        const id = Number(p.row.inspectionOrderId);
        const st = p.row.status;
        return (
          <Stack direction="row" spacing={0.25}>
            {st === 'PENDING' && (
              <Tooltip title="Assign Inspector">
                <IconButton size="small"
                  onClick={() => { assignForm.reset({ inspectorCode: p.row.inspectorCode ?? '' }); setAssignTarget(p.row); }}
                  sx={{ color: '#4F46E5' }}>
                  <SolarIcon name="operator" size={15} />
                </IconButton>
              </Tooltip>
            )}
            {(st === 'PENDING' || st === 'ASSIGNED') && (
              <Tooltip title="Start Inspection">
                <IconButton size="small" onClick={() => startMutation.mutate(id)} sx={{ color: '#0891B2' }}>
                  <SolarIcon name="resume" size={15} />
                </IconButton>
              </Tooltip>
            )}
            {st === 'IN_PROGRESS' && (
              <>
                <Tooltip title="Mark Passed">
                  <IconButton size="small" onClick={() => setPassTarget(p.row)} sx={{ color: '#15803D' }}>
                    <SolarIcon name="complete" size={15} />
                  </IconButton>
                </Tooltip>
                <Tooltip title="Mark Failed">
                  <IconButton size="small" onClick={() => setFailTarget(p.row)} sx={{ color: '#DC2626' }}>
                    <SolarIcon name="close" size={15} />
                  </IconButton>
                </Tooltip>
              </>
            )}
            {(st === 'PENDING' || st === 'ASSIGNED' || st === 'IN_PROGRESS') && (
              <Tooltip title="Waive">
                <IconButton size="small"
                  onClick={() => { waiveForm.reset(); setWaiveTarget(p.row); }}
                  sx={{ color: '#64748B' }}>
                  <SolarIcon name="hold" size={15} />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        );
      },
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Inspection Orders" breadcrumbs={[{ label: 'Quality' }, { label: 'Inspection Orders' }]} />
      <EmptyState icon="emptyTable" title="Failed to load orders" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Inspection Orders"
        subtitle="Track and manage runtime inspection order lifecycle"
        breadcrumbs={[{ label: 'Quality' }, { label: 'Inspection Orders' }]}
        actions={<RefreshButton onClick={() => refetch()} />}
      />

      {toastError && (
        <Alert severity="error" onClose={() => setToastError(null)} sx={{ mb: 2 }}>
          {toastError}
        </Alert>
      )}

      <Stack direction="row" spacing={1.5} sx={{ mb: 1.5, alignItems: 'center' }}>
        <Box sx={{ flex: 1 }}>
          <TableToolbar
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Filter by order no, product or inspector…"
          />
        </Box>
        <Select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          size="small"
          displayEmpty
          sx={{ minWidth: 140, flexShrink: 0 }}
        >
          <MenuItem value="">All Status</MenuItem>
          {Object.entries(STATUS_CONFIG).map(([k, v]) => (
            <MenuItem key={k} value={k}>{v.label}</MenuItem>
          ))}
        </Select>
      </Stack>

      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(row) => row.inspectionOrderId}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon="emptyTable"
                title="No inspection orders"
                description="Orders are created automatically when a QC-required job step completes."
              />
            ),
          }}
          sx={{ border: 'none', '& .MuiDataGrid-cell': { alignItems: 'center' } }}
        />
      </Box>

      {/* ── Assign Inspector drawer ── */}
      <FormDrawer
        open={!!assignTarget}
        onClose={() => setAssignTarget(null)}
        title={`Assign Inspector — ${assignTarget?.orderNo ?? ''}`}
        onSubmit={() => void assignForm.handleSubmit((v) =>
          assignMutation.mutate({ id: Number(assignTarget!.inspectionOrderId), inspectorCode: v.inspectorCode }))()}
        loading={assignMutation.isPending}
      >
        <Controller name="inspectorCode" control={assignForm.control} render={({ field, fieldState }) => (
          <TextField
            {...field}
            label="Inspector Code / ID"
            fullWidth
            error={!!fieldState.error}
            helperText={fieldState.error?.message ?? 'Employee code of the assigned inspector'}
          />
        )} />
      </FormDrawer>

      {/* ── Waive drawer ── */}
      <FormDrawer
        open={!!waiveTarget}
        onClose={() => setWaiveTarget(null)}
        title={`Waive Inspection — ${waiveTarget?.orderNo ?? ''}`}
        onSubmit={() => void waiveForm.handleSubmit((v) =>
          waiveMutation.mutate({ id: Number(waiveTarget!.inspectionOrderId), body: v }))()}
        loading={waiveMutation.isPending}
      >
        <Controller name="waivedBy" control={waiveForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Supervisor Code" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Controller name="reason" control={waiveForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Waive Reason" fullWidth multiline rows={3}
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
      </FormDrawer>

      {/* ── Pass confirm ── */}
      <ConfirmDialog
        open={!!passTarget}
        title="Mark Inspection Passed"
        description={
          <>Mark <strong>{passTarget?.orderNo}</strong> as <strong>PASSED</strong>?
          This will clear the QC gate for the next job.</>
        }
        onClose={() => setPassTarget(null)}
        onConfirm={() => passTarget && passMutation.mutate(Number(passTarget.inspectionOrderId))}
        loading={passMutation.isPending}
      />

      {/* ── Fail confirm ── */}
      <ConfirmDialog
        open={!!failTarget}
        title="Mark Inspection Failed"
        description={
          <>Mark <strong>{failTarget?.orderNo}</strong> as <strong>FAILED</strong>?
          This will block the next job until resolved.</>
        }
        onClose={() => setFailTarget(null)}
        onConfirm={() => failTarget && failMutation.mutate(Number(failTarget.inspectionOrderId))}
        loading={failMutation.isPending}
      />
    </PageRoot>
  );
}
