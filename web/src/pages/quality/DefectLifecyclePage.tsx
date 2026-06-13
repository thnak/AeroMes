import {
  Alert,
  Box,
  Button,
  Chip,
  Drawer,
  MenuItem,
  Stack,
  Tab,
  Tabs,
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
  TablePageSkeleton,
  PageHeader,
  PageRoot,
  RefreshButton,
  TableToolbar,
  SolarIcon,
} from '../../components';
import {
  useGetApiV1DefectLifecycleEntries,
  useGetApiV1DefectLifecycleRepairOrders,
  postApiV1DefectLifecycleEntries,
  patchApiV1DefectLifecycleEntriesIdStatus,
  postApiV1DefectLifecycleRepairOrders,
  patchApiV1DefectLifecycleRepairOrdersIdStatus,
  getGetApiV1DefectLifecycleEntriesQueryKey,
  getGetApiV1DefectLifecycleRepairOrdersQueryKey,
} from '../../api/defect-lifecycle/defect-lifecycle';
import { useGetApiV1QualityDefectCodes } from '../../api/defect-codes/defect-codes';
import { getErrorMessage } from '../../lib/apiClient';
import type {
  DefectEntryDto,
  RepairOrderDto,
} from '../../api/model';

// ── Constants ──────────────────────────────────────────────────────────────

const ENTRY_STATUS_COLORS: Record<string, 'default' | 'warning' | 'info' | 'success' | 'error'> = {
  PENDING: 'default',
  IN_REPAIR: 'warning',
  REPAIRED: 'success',
  SCRAPPED: 'error',
};

const REPAIR_STATUS_COLORS: Record<string, 'default' | 'info' | 'success' | 'error'> = {
  DRAFT: 'default',
  IN_PROGRESS: 'info',
  COMPLETED: 'success',
  CANCELLED: 'error',
};

// ── Schemas ────────────────────────────────────────────────────────────────

const entrySchema = z.object({
  workOrderId: z.number().int().positive(),
  jobId: z.number().int().positive().optional(),
  defectCodeId: z.number().int().positive(),
  quantity: z.number().positive(),
  notes: z.string().optional(),
});
type EntryForm = z.infer<typeof entrySchema>;

const repairOrderSchema = z.object({
  notes: z.string().optional(),
});
type RepairOrderForm = z.infer<typeof repairOrderSchema>;

// ── DefectEntries tab ──────────────────────────────────────────────────────

function DefectEntriesTab() {
  const qc = useQueryClient();
  const [filterStatus, setFilterStatus] = useState('');
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isLoading, error, refetch } = useGetApiV1DefectLifecycleEntries(
    { status: filterStatus || undefined }
  );
  const { data: defectCodesData } = useGetApiV1QualityDefectCodes({ activeOnly: true });
  const defectCodes = defectCodesData ?? [];

  const entries: DefectEntryDto[] = data ?? [];
  const filtered = search
    ? entries.filter(
        (e) =>
          String(e.workOrderId).includes(search) ||
          (e.defectCode ?? '').toLowerCase().includes(search.toLowerCase()),
      )
    : entries;

  const createMut = useMutation({
    mutationFn: (body: Parameters<typeof postApiV1DefectLifecycleEntries>[0]) =>
      postApiV1DefectLifecycleEntries(body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: getGetApiV1DefectLifecycleEntriesQueryKey() });
      setCreateOpen(false);
      reset();
    },
  });

  const statusMut = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      patchApiV1DefectLifecycleEntriesIdStatus(id, { status }),
    onSuccess: () => qc.invalidateQueries({ queryKey: getGetApiV1DefectLifecycleEntriesQueryKey() }),
  });

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const { control, handleSubmit, reset, formState: { errors } } = useForm<EntryForm>({
    resolver: zodResolver(entrySchema) as any,
    defaultValues: { workOrderId: undefined, defectCodeId: undefined, quantity: 1 },
  });

  const onSubmit = (v: EntryForm) => {
    createMut.mutate({
      workOrderId: v.workOrderId,
      jobId: v.jobId ? Number(v.jobId) : null,
      defectCodeId: v.defectCodeId,
      quantity: v.quantity,
      notes: v.notes || null,
    });
  };

  const filters = [
    {
      label: 'Status',
      value: filterStatus,
      options: [
        { value: '', label: 'All' },
        ...Object.keys(ENTRY_STATUS_COLORS).map((s) => ({ value: s, label: s })),
      ],
      onChange: setFilterStatus,
      width: 150,
    },
  ];

  const columns: GridColDef<DefectEntryDto>[] = [
    { field: 'defectEntryId', headerName: 'ID', width: 70, type: 'number' },
    {
      field: 'workOrderId',
      headerName: 'Work Order',
      width: 120,
      renderCell: (p: GridRenderCellParams<DefectEntryDto, number>) => (
        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 700 }}>
          #{p.value}
        </Typography>
      ),
    },
    {
      field: 'defectCode',
      headerName: 'Defect Code',
      width: 140,
      renderCell: (p: GridRenderCellParams<DefectEntryDto, string>) => (
        <Box>
          <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 700 }}>
            {p.value}
          </Typography>
          <Typography variant="caption" sx={{ display: 'block', color: 'text.secondary', fontSize: 10 }}>
            {p.row.defectName}
          </Typography>
        </Box>
      ),
    },
    { field: 'quantity', headerName: 'Qty', width: 80, type: 'number' },
    { field: 'repairableQty', headerName: 'Repairable', width: 100, type: 'number' },
    { field: 'scrapQty', headerName: 'Scrap', width: 80, type: 'number' },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (p: GridRenderCellParams<DefectEntryDto, string>) => (
        <Chip label={p.value} color={ENTRY_STATUS_COLORS[p.value ?? ''] ?? 'default'} size="small" />
      ),
    },
    { field: 'createdBy', headerName: 'By', width: 100 },
    {
      field: 'createdAt',
      headerName: 'Date',
      width: 130,
      valueFormatter: (v: unknown) => v ? new Date(v as string).toLocaleDateString() : '',
    },
    {
      field: '__actions',
      headerName: '',
      width: 140,
      sortable: false,
      renderCell: (p: GridRenderCellParams<DefectEntryDto>) => {
        const s = p.row.status;
        if (s === 'REPAIRED' || s === 'SCRAPPED') return null;
        return (
          <Stack direction="row" spacing={0.5}>
            {s === 'PENDING' && (
              <Button size="small" variant="outlined" color="warning" sx={{ fontSize: 10 }}
                onClick={() => statusMut.mutate({ id: Number(p.row.defectEntryId), status: 'IN_REPAIR' })}>
                Repair
              </Button>
            )}
            {s !== 'SCRAPPED' && (
              <Button size="small" variant="outlined" color="error" sx={{ fontSize: 10 }}
                onClick={() => statusMut.mutate({ id: Number(p.row.defectEntryId), status: 'SCRAPPED' })}>
                Scrap
              </Button>
            )}
            {s === 'IN_REPAIR' && (
              <Button size="small" variant="outlined" color="success" sx={{ fontSize: 10 }}
                onClick={() => statusMut.mutate({ id: Number(p.row.defectEntryId), status: 'REPAIRED' })}>
                Done
              </Button>
            )}
          </Stack>
        );
      },
    },
  ];

  if (isLoading) return <TablePageSkeleton />;

  return (
    <Box>
      {!!error && <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(error as Error)}</Alert>}
      {!!createMut.error && (
        <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(createMut.error as Error)}</Alert>
      )}

      <Stack direction="row" sx={{ justifyContent: 'flex-end', mb: 1 }}>
        <Stack direction="row" spacing={1}>
          <RefreshButton onClick={() => refetch()} />
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => setCreateOpen(true)}
          >
            Log Defect
          </Button>
        </Stack>
      </Stack>

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search work order or defect code…"
        filters={filters}
      />

      {filtered.length === 0 ? (
        <EmptyState
          title="No defect entries"
          description="Log defective items detected during production."
          action={
            <Button variant="contained" onClick={() => setCreateOpen(true)}>
              Log Defect
            </Button>
          }
        />
      ) : (
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(r) => Number(r.defectEntryId)}
          pageSizeOptions={[25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          disableRowSelectionOnClick
          sx={{ bgcolor: 'background.paper', borderRadius: 2 }}
        />
      )}

      <Drawer
        anchor="right"
        open={createOpen}
        onClose={() => { setCreateOpen(false); reset(); }}
        slotProps={{ paper: { sx: { width: { xs: '100%', sm: 480 }, p: 3 } } }}
      >
        <Typography variant="h6" sx={{ fontWeight: 700, mb: 3 }}>Log Defect Entry</Typography>
        <form onSubmit={handleSubmit(onSubmit as any)}>
          <Stack spacing={2}>
            <Controller name="workOrderId" control={control}
              render={({ field }) => (
                <TextField fullWidth label="Work Order ID *" size="small" type="number"
                  value={field.value ?? ''} onChange={(e) => field.onChange(e.target.value === '' ? undefined : Number(e.target.value))}
                  error={!!errors.workOrderId} helperText={errors.workOrderId?.message} />
              )} />
            <Controller name="jobId" control={control}
              render={({ field }) => (
                <TextField fullWidth label="Job ID (optional)" size="small" type="number"
                  value={field.value ?? ''} onChange={(e) => field.onChange(e.target.value === '' ? undefined : Number(e.target.value))} />
              )} />
            <Controller name="defectCodeId" control={control}
              render={({ field }) => (
                <TextField select fullWidth label="Defect Code *" size="small"
                  value={field.value ?? ''} onChange={(e) => field.onChange(Number(e.target.value))}
                  error={!!errors.defectCodeId}>
                  {defectCodes.map((c) => (
                    <MenuItem key={Number(c.defectCodeId)} value={Number(c.defectCodeId)}>
                      {c.code} — {c.defectName}
                      {c.isRepairable && (
                        <Chip label="Repairable" size="small" color="success" sx={{ ml: 1, height: 16, fontSize: 9 }} />
                      )}
                    </MenuItem>
                  ))}
                </TextField>
              )} />
            <Controller name="quantity" control={control}
              render={({ field }) => (
                <TextField fullWidth label="Defective Quantity *" size="small" type="number"
                  value={field.value ?? ''} onChange={(e) => field.onChange(e.target.value === '' ? undefined : Number(e.target.value))}
                  error={!!errors.quantity} helperText={errors.quantity?.message} />
              )} />
            <Controller name="notes" control={control}
              render={({ field }) => (
                <TextField fullWidth label="Notes" size="small" multiline rows={2} {...field} />
              )} />
          </Stack>
          <Stack direction="row" spacing={1.5} sx={{ mt: 3, justifyContent: 'flex-end' }}>
            <Button variant="outlined" onClick={() => { setCreateOpen(false); reset(); }}>Cancel</Button>
            <Button type="submit" variant="contained" loading={createMut.isPending}>Log Defect</Button>
          </Stack>
        </form>
      </Drawer>
    </Box>
  );
}

// ── RepairOrders tab ───────────────────────────────────────────────────────

function RepairOrdersTab() {
  const qc = useQueryClient();
  const [filterStatus, setFilterStatus] = useState('');
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isLoading, error, refetch } = useGetApiV1DefectLifecycleRepairOrders(
    { status: filterStatus || undefined }
  );
  const orders: RepairOrderDto[] = data ?? [];
  const filtered = search
    ? orders.filter((o) => o.repairOrderNo.toLowerCase().includes(search.toLowerCase()))
    : orders;

  const createMut = useMutation({
    mutationFn: (body: Parameters<typeof postApiV1DefectLifecycleRepairOrders>[0]) =>
      postApiV1DefectLifecycleRepairOrders(body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: getGetApiV1DefectLifecycleRepairOrdersQueryKey() });
      setCreateOpen(false);
      resetForm();
    },
  });

  const statusMut = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      patchApiV1DefectLifecycleRepairOrdersIdStatus(id, { status }),
    onSuccess: () => qc.invalidateQueries({ queryKey: getGetApiV1DefectLifecycleRepairOrdersQueryKey() }),
  });

  const { control, handleSubmit, reset: resetForm } = useForm<RepairOrderForm>({
    resolver: zodResolver(repairOrderSchema),
    defaultValues: { notes: '' },
  });

  const filters = [
    {
      label: 'Status',
      value: filterStatus,
      options: [
        { value: '', label: 'All' },
        ...Object.keys(REPAIR_STATUS_COLORS).map((s) => ({ value: s, label: s })),
      ],
      onChange: setFilterStatus,
      width: 150,
    },
  ];

  const columns: GridColDef<RepairOrderDto>[] = [
    {
      field: 'repairOrderNo',
      headerName: 'Repair Order',
      width: 160,
      renderCell: (p: GridRenderCellParams<RepairOrderDto, string>) => (
        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 700 }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (p: GridRenderCellParams<RepairOrderDto, string>) => (
        <Chip label={p.value} color={REPAIR_STATUS_COLORS[p.value ?? ''] ?? 'default'} size="small" />
      ),
    },
    {
      field: 'defectEntryIds',
      headerName: 'Defect Entries',
      width: 130,
      valueFormatter: (v: unknown) => `${(v as number[]).length} entries`,
    },
    {
      field: 'materialLines',
      headerName: 'Materials',
      width: 100,
      valueFormatter: (v: unknown) => `${(v as unknown[]).length} lines`,
    },
    { field: 'createdBy', headerName: 'Created By', width: 120 },
    {
      field: 'createdAt',
      headerName: 'Date',
      width: 130,
      valueFormatter: (v: unknown) => v ? new Date(v as string).toLocaleDateString() : '',
    },
    {
      field: 'completedAt',
      headerName: 'Completed',
      width: 130,
      valueFormatter: (v: unknown) => v ? new Date(v as string).toLocaleDateString() : '—',
    },
    {
      field: '__actions',
      headerName: '',
      width: 180,
      sortable: false,
      renderCell: (p: GridRenderCellParams<RepairOrderDto>) => {
        const s = p.row.status;
        if (s === 'COMPLETED' || s === 'CANCELLED') return null;
        return (
          <Stack direction="row" spacing={0.5}>
            {s === 'DRAFT' && (
              <Button size="small" variant="outlined" color="info" sx={{ fontSize: 10 }}
                onClick={() => statusMut.mutate({ id: Number(p.row.repairOrderId), status: 'IN_PROGRESS' })}>
                Start
              </Button>
            )}
            {s === 'IN_PROGRESS' && (
              <Button size="small" variant="outlined" color="success" sx={{ fontSize: 10 }}
                onClick={() => statusMut.mutate({ id: Number(p.row.repairOrderId), status: 'COMPLETED' })}>
                Complete
              </Button>
            )}
            <Button size="small" variant="outlined" color="error" sx={{ fontSize: 10 }}
              onClick={() => statusMut.mutate({ id: Number(p.row.repairOrderId), status: 'CANCELLED' })}>
              Cancel
            </Button>
          </Stack>
        );
      },
    },
  ];

  if (isLoading) return <TablePageSkeleton />;

  return (
    <Box>
      {!!error && <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(error as Error)}</Alert>}

      <Stack direction="row" sx={{ justifyContent: 'flex-end', mb: 1 }}>
        <Stack direction="row" spacing={1}>
          <RefreshButton onClick={() => refetch()} />
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => setCreateOpen(true)}
          >
            New Repair Order
          </Button>
        </Stack>
      </Stack>

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search repair order no…"
        filters={filters}
      />

      {filtered.length === 0 ? (
        <EmptyState
          title="No repair orders"
          description="Create a repair order to track and manage defective item repair."
          action={
            <Button variant="contained" onClick={() => setCreateOpen(true)}>
              New Repair Order
            </Button>
          }
        />
      ) : (
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(r) => Number(r.repairOrderId)}
          pageSizeOptions={[25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          disableRowSelectionOnClick
          sx={{ bgcolor: 'background.paper', borderRadius: 2 }}
        />
      )}

      <Drawer
        anchor="right"
        open={createOpen}
        onClose={() => { setCreateOpen(false); resetForm(); }}
        slotProps={{ paper: { sx: { width: { xs: '100%', sm: 480 }, p: 3 } } }}
      >
        <Typography variant="h6" sx={{ fontWeight: 700, mb: 3 }}>New Repair Order</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Creates an empty repair order. After creation, defect entries will be linked to this order.
        </Typography>
        <form onSubmit={handleSubmit((v) => createMut.mutate({ defectEntryIds: [], materialLines: [], notes: v.notes || null }))}>
          <Controller name="notes" control={control}
            render={({ field }) => (
              <TextField fullWidth label="Notes" size="small" multiline rows={3} {...field} />
            )} />
          <Stack direction="row" spacing={1.5} sx={{ mt: 3, justifyContent: 'flex-end' }}>
            <Button variant="outlined" onClick={() => { setCreateOpen(false); resetForm(); }}>Cancel</Button>
            <Button type="submit" variant="contained" loading={createMut.isPending}>Create</Button>
          </Stack>
        </form>
      </Drawer>
    </Box>
  );
}

// ── Main page ──────────────────────────────────────────────────────────────

export default function DefectLifecyclePage() {
  const [tab, setTab] = useState(0);

  return (
    <PageRoot>
      <PageHeader
        title="Defect Lifecycle"
        subtitle="Track defective items from detection through repair or scrap disposal"
        breadcrumbs={[{ label: 'Quality' }, { label: 'Defect Lifecycle' }]}
      />

      <Tabs
        value={tab}
        onChange={(_, v) => setTab(v)}
        sx={{ mb: 2, borderBottom: '1px solid', borderColor: 'divider' }}
      >
        <Tab label="Defect Entries" />
        <Tab label="Repair Orders" />
      </Tabs>

      {tab === 0 && <DefectEntriesTab />}
      {tab === 1 && <RepairOrdersTab />}
    </PageRoot>
  );
}
