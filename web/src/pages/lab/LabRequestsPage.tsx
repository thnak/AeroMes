import {
  Alert,
  Box,
  Button,
  Chip,
  Drawer,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  DataGrid,
  type GridColDef,
  type GridRenderCellParams,
} from '@mui/x-data-grid';
import {
  EmptyState,
  KpiCard,
  TablePageSkeleton,
  PageHeader,
  PageRoot,
  RefreshButton,
  ConfirmDialog,
  TableToolbar,
  SolarIcon,
} from '../../components';
import {
  useGetApiV1LabRequests,
  postApiV1LabRequests,
  patchApiV1LabRequestsIdCancel,
  getGetApiV1LabRequestsQueryKey,
} from '../../api/lab/lab';
import { getErrorMessage } from '../../lib/apiClient';
import type { CreateRequestDto, LabRequestListDto } from '../../api/model';

// ── Constants ──────────────────────────────────────────────────────────────

const REQUEST_TYPES = ['INCOMING', 'IN_PROCESS', 'FINAL_RELEASE', 'CUSTOMER_RETURN', 'PERIODIC'];
const PRIORITIES = ['ROUTINE', 'URGENT', 'CRITICAL'];

const STATUS_COLORS: Record<string, 'default' | 'info' | 'warning' | 'success' | 'error'> = {
  PENDING: 'default',
  SAMPLING: 'info',
  IN_PROGRESS: 'warning',
  COMPLETED: 'success',
  CANCELLED: 'error',
};

const PRIORITY_COLORS: Record<string, 'default' | 'warning' | 'error'> = {
  ROUTINE: 'default',
  URGENT: 'warning',
  CRITICAL: 'error',
};

// ── Form schema ────────────────────────────────────────────────────────────

const schema = z.object({
  requestType: z.string().min(1),
  priority: z.string().min(1),
  productCode: z.string().min(1),
  lotNumber: z.string().optional(),
  sampleQty: z.number().positive(),
  sampleUnit: z.string().min(1),
  sampleLocation: z.string().optional(),
  notes: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

// ── Page component ─────────────────────────────────────────────────────────

export default function LabRequestsPage() {
  const qc = useQueryClient();

  const [filterStatus, setFilterStatus] = useState('');
  const [filterPriority, setFilterPriority] = useState('');
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<LabRequestListDto | null>(null);

  const { data, isLoading, error, refetch } = useGetApiV1LabRequests({
    status: filterStatus || undefined,
    priority: filterPriority || undefined,
  });

  const rows: LabRequestListDto[] = data ?? [];
  const filtered = search
    ? rows.filter((r) =>
        r.requestNo.toLowerCase().includes(search.toLowerCase()) ||
        r.productCode.toLowerCase().includes(search.toLowerCase()),
      )
    : rows;

  const pending = rows.filter((r) => r.status === 'PENDING').length;
  const inProgress = rows.filter((r) => r.status === 'IN_PROGRESS' || r.status === 'SAMPLING').length;
  const completed = rows.filter((r) => r.status === 'COMPLETED').length;

  const createMut = useMutation({
    mutationFn: (body: CreateRequestDto) => postApiV1LabRequests(body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: getGetApiV1LabRequestsQueryKey() });
      setCreateOpen(false);
      reset();
    },
  });

  const cancelMut = useMutation({
    mutationFn: (id: number) => patchApiV1LabRequestsIdCancel(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: getGetApiV1LabRequestsQueryKey() });
      setCancelTarget(null);
    },
  });

  const { control, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      requestType: 'INCOMING',
      priority: 'ROUTINE',
      productCode: '',
      sampleQty: 1,
      sampleUnit: 'pcs',
    },
  });

  const onSubmit = (values: FormValues) => {
    createMut.mutate({
      requestType: values.requestType,
      priority: values.priority,
      productCode: values.productCode,
      lotNumber: values.lotNumber || null,
      workOrderId: null,
      inspectionOrderId: null,
      customerCode: null,
      panelId: null,
      sampleQty: values.sampleQty,
      sampleUnit: values.sampleUnit,
      sampleLocation: values.sampleLocation || null,
      requiredBy: null,
      notes: values.notes || null,
    });
  };

  const tableFilters = [
    {
      label: 'Status',
      value: filterStatus,
      options: [
        { value: '', label: 'All statuses' },
        ...Object.keys(STATUS_COLORS).map((s) => ({ value: s, label: s })),
      ],
      onChange: setFilterStatus,
      width: 150,
    },
    {
      label: 'Priority',
      value: filterPriority,
      options: [
        { value: '', label: 'All priorities' },
        ...PRIORITIES.map((p) => ({ value: p, label: p })),
      ],
      onChange: setFilterPriority,
      width: 140,
    },
  ];

  const columns: GridColDef<LabRequestListDto>[] = [
    {
      field: 'requestNo',
      headerName: 'Request No.',
      width: 160,
      renderCell: (params: GridRenderCellParams<LabRequestListDto, string>) => (
        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 700 }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (params: GridRenderCellParams<LabRequestListDto, string>) => (
        <Chip
          label={params.value}
          color={STATUS_COLORS[params.value ?? ''] ?? 'default'}
          size="small"
          variant="outlined"
        />
      ),
    },
    {
      field: 'priority',
      headerName: 'Priority',
      width: 110,
      renderCell: (params: GridRenderCellParams<LabRequestListDto, string>) => (
        <Chip
          label={params.value}
          color={PRIORITY_COLORS[params.value ?? ''] ?? 'default'}
          size="small"
        />
      ),
    },
    { field: 'requestType', headerName: 'Type', width: 140 },
    { field: 'productCode', headerName: 'Product', width: 130 },
    {
      field: 'lotNumber', headerName: 'Lot', width: 120,
      valueFormatter: (v: unknown) => (v as string | null) ?? '—',
    },
    {
      field: 'requestedAt',
      headerName: 'Requested At',
      width: 170,
      valueFormatter: (v: unknown) => v ? new Date(v as string).toLocaleString() : '—',
    },
    {
      field: 'requiredBy',
      headerName: 'Required By',
      width: 170,
      valueFormatter: (v: unknown) => v ? new Date(v as string).toLocaleString() : '—',
    },
    {
      field: 'assignedTo', headerName: 'Assigned To', width: 150,
      valueFormatter: (v: unknown) => (v as string | null) ?? '—',
    },
    { field: 'requestedBy', headerName: 'Requested By', width: 150 },
    {
      field: '__actions',
      headerName: '',
      width: 60,
      sortable: false,
      renderCell: (params: GridRenderCellParams<LabRequestListDto>) => {
        const row = params.row;
        if (row.status === 'CANCELLED' || row.status === 'COMPLETED') return null;
        return (
          <Box
            onClick={() => setCancelTarget(row)}
            sx={{ cursor: 'pointer', color: 'error.main', display: 'flex', alignItems: 'center' }}
          >
            <SolarIcon name="delete" size={18} />
          </Box>
        );
      },
    },
  ];

  if (isLoading) return <TablePageSkeleton />;

  return (
    <PageRoot>
      <PageHeader
        title="Lab Requests"
        subtitle="Test requests, sample management and Certificates of Analysis"
        breadcrumbs={[{ label: 'Lab' }, { label: 'Requests' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => refetch()} />
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="add" size={16} />}
              onClick={() => setCreateOpen(true)}
            >
              New Request
            </Button>
          </Stack>
        }
      />

      {!!error && <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(error as Error)}</Alert>}
      {!!createMut.error && (
        <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(createMut.error as Error)}</Alert>
      )}

      {/* ── KPI row ──────────────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard label="Pending" value={pending.toString()} unit="requests" icon="warning" accentColor="#D97706" />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard label="In Progress" value={inProgress.toString()} unit="requests" icon="reports" accentColor="#1D4ED8" />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard label="Completed" value={completed.toString()} unit="requests" icon="success" accentColor="#15803D" />
        </Grid>
      </Grid>

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search request no. or product…"
        filters={tableFilters}
      />

      {filtered.length === 0 ? (
        <EmptyState
          title="No lab requests found"
          description="Create a new lab request to start sample management."
          action={
            <Button variant="contained" onClick={() => setCreateOpen(true)}>
              New Request
            </Button>
          }
        />
      ) : (
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(r) => Number(r.requestId)}
          pageSizeOptions={[25, 50, 100]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          disableRowSelectionOnClick
          sx={{ bgcolor: 'background.paper', borderRadius: 2 }}
        />
      )}

      {/* ── Create Drawer ──────────────────────────────────────────────── */}
      <Drawer
        anchor="right"
        open={createOpen}
        onClose={() => { setCreateOpen(false); reset(); }}
        slotProps={{ paper: { sx: { width: { xs: '100%', sm: 480 }, p: 3 } } }}
      >
        <Typography variant="h6" sx={{ fontWeight: 700, mb: 3 }}>New Lab Request</Typography>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Stack spacing={2.5}>
            <Controller
              name="requestType"
              control={control}
              render={({ field }) => (
                <TextField select fullWidth label="Request Type" size="small" {...field}
                  error={!!errors.requestType}>
                  {REQUEST_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                </TextField>
              )}
            />
            <Controller
              name="priority"
              control={control}
              render={({ field }) => (
                <TextField select fullWidth label="Priority" size="small" {...field}
                  error={!!errors.priority}>
                  {PRIORITIES.map((p) => <MenuItem key={p} value={p}>{p}</MenuItem>)}
                </TextField>
              )}
            />
            <Controller
              name="productCode"
              control={control}
              render={({ field }) => (
                <TextField fullWidth label="Product Code *" size="small" {...field}
                  error={!!errors.productCode} helperText={errors.productCode?.message} />
              )}
            />
            <Controller
              name="lotNumber"
              control={control}
              render={({ field }) => (
                <TextField fullWidth label="Lot Number" size="small" {...field} />
              )}
            />
            <Stack direction="row" spacing={1.5}>
              <Controller
                name="sampleQty"
                control={control}
                render={({ field }) => (
                  <TextField fullWidth label="Sample Qty *" size="small" type="number"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                    error={!!errors.sampleQty} />
                )}
              />
              <Controller
                name="sampleUnit"
                control={control}
                render={({ field }) => (
                  <TextField fullWidth label="Unit *" size="small" {...field}
                    error={!!errors.sampleUnit} />
                )}
              />
            </Stack>
            <Controller
              name="sampleLocation"
              control={control}
              render={({ field }) => (
                <TextField fullWidth label="Sample Location" size="small" {...field} />
              )}
            />
            <Controller
              name="notes"
              control={control}
              render={({ field }) => (
                <TextField fullWidth label="Notes" size="small" multiline rows={3} {...field} />
              )}
            />
          </Stack>

          <Stack direction="row" spacing={1.5} sx={{ mt: 3, justifyContent: 'flex-end' }}>
            <Button variant="outlined" onClick={() => { setCreateOpen(false); reset(); }}>
              Cancel
            </Button>
            <Button
              type="submit"
              variant="contained"
              loading={createMut.isPending}
            >
              Create Request
            </Button>
          </Stack>
        </form>
      </Drawer>

      {/* ── Cancel Confirm ─────────────────────────────────────────────── */}
      <ConfirmDialog
        open={cancelTarget !== null}
        title="Cancel Lab Request"
        description={`Cancel request ${cancelTarget?.requestNo}? This cannot be undone.`}
        onClose={() => setCancelTarget(null)}
        onConfirm={() => cancelMut.mutate(Number(cancelTarget!.requestId))}
        loading={cancelMut.isPending}
      />
    </PageRoot>
  );
}
