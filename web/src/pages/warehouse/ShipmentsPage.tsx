import {
  Alert,
  Box,
  Button,
  Chip,
  Grid,
  IconButton,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useForm } from 'react-hook-form';
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
  useGetApiV1Shipments,
  getGetApiV1ShipmentsQueryKey,
  postApiV1Shipments,
  postApiV1ShipmentsIdPickList,
  postApiV1ShipmentsIdDispatch,
  deleteApiV1ShipmentsId,
} from '../../api/shipment/shipment';
import type { ShipmentSummaryDto } from '../../api/model';
import { ShipmentStatus } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const STATUS_COLORS: Record<string, string> = {
  Draft:      '#94A3B8',
  Picking:    '#D97706',
  Packed:     '#0891B2',
  Dispatched: '#15803D',
  Cancelled:  '#DC2626',
};

function numVal(v: number | string): number {
  return typeof v === 'number' ? v : parseInt(v as string, 10) || 0;
}

// ─── Create shipment form ──────────────────────────────────────────────────────

const CreateSchema = z.object({
  customerName:     z.string().min(1, 'Required').max(200),
  requestedShipDate: z.string().min(1, 'Required'),
});
type CreateForm = z.infer<typeof CreateSchema>;

function ShipmentCreateForm({ onSubmit }: { onSubmit: (d: CreateForm) => void }) {
  const today = new Date().toISOString().split('T')[0];
  const { register, handleSubmit, formState: { errors } } = useForm<CreateForm>({
    resolver: zodResolver(CreateSchema),
    defaultValues: { customerName: '', requestedShipDate: today },
  });
  return (
    <Box component="form" id="shipment-create-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('customerName')} label="Customer Name" fullWidth required
            error={!!errors.customerName} helperText={errors.customerName?.message}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('requestedShipDate')} label="Requested Ship Date" type="date"
            fullWidth required error={!!errors.requestedShipDate}
            helperText={errors.requestedShipDate?.message}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Dispatch form ─────────────────────────────────────────────────────────────

const DispatchSchema = z.object({
  carrierName:    z.string().max(200).optional().nullable(),
  trackingNumber: z.string().max(100).optional().nullable(),
});
type DispatchFormValues = z.infer<typeof DispatchSchema>;

function DispatchForm({ onSubmit, shipment }: { onSubmit: (d: DispatchFormValues) => void; shipment: ShipmentSummaryDto }) {
  const { register, handleSubmit } = useForm<DispatchFormValues>({
    resolver: zodResolver(DispatchSchema),
    defaultValues: {
      carrierName:    shipment.carrierName ?? '',
      trackingNumber: shipment.trackingNumber ?? '',
    },
  });
  return (
    <Box component="form" id="dispatch-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
            Dispatching <strong>{shipment.shipmentCode}</strong> to <strong>{shipment.customerName}</strong>
          </Typography>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('carrierName')} label="Carrier Name" fullWidth placeholder="e.g. DHL, FedEx, GHTK…" />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('trackingNumber')} label="Tracking Number" fullWidth
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ShipmentsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch]               = useState('');
  const [statusFilter, setStatusFilter]   = useState<string>('');
  const [createOpen, setCreateOpen]       = useState(false);
  const [dispatchTarget, setDispatchTarget] = useState<ShipmentSummaryDto | null>(null);
  const [deleteTarget, setDeleteTarget]   = useState<ShipmentSummaryDto | null>(null);
  const [saveError, setSaveError]         = useState('');

  const { data: resp, isLoading, error, refetch } = useGetApiV1Shipments(
    statusFilter ? { status: statusFilter as ShipmentSummaryDto['status'] } : undefined,
  );
  const shipments: ShipmentSummaryDto[] = (resp ?? []) as ShipmentSummaryDto[];

  const filtered = useMemo(() => {
    if (!search) return shipments;
    const q = search.toLowerCase();
    return shipments.filter((s) =>
      s.shipmentCode.toLowerCase().includes(q) || s.customerName.toLowerCase().includes(q),
    );
  }, [shipments, search]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1ShipmentsQueryKey() });

  const createMutation = useMutation({
    mutationFn: (d: CreateForm) => postApiV1Shipments({ soId: null, customerName: d.customerName, requestedShipDate: d.requestedShipDate }),
    onSuccess: () => { invalidate(); setCreateOpen(false); setSaveError(''); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const pickListMutation = useMutation({
    mutationFn: (id: number) => postApiV1ShipmentsIdPickList(id, { locationId: 0, assignedTo: null }),
    onSuccess: () => invalidate(),
  });

  const dispatchMutation = useMutation({
    mutationFn: ({ id, d }: { id: number; d: DispatchFormValues }) =>
      postApiV1ShipmentsIdDispatch(id, { carrierName: d.carrierName ?? null, trackingNumber: d.trackingNumber ?? null }),
    onSuccess: () => { invalidate(); setDispatchTarget(null); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1ShipmentsId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const columns: GridColDef<ShipmentSummaryDto>[] = [
    {
      field: 'shipmentCode', headerName: 'Shipment #', width: 150,
      renderCell: (p: GridRenderCellParams<ShipmentSummaryDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    { field: 'customerName', headerName: 'Customer', flex: 1, minWidth: 150 },
    {
      field: 'status', headerName: 'Status', width: 120,
      renderCell: (p: GridRenderCellParams<ShipmentSummaryDto>) => {
        const color = STATUS_COLORS[p.value as string] ?? '#94A3B8';
        return (
          <Chip label={p.value} size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        );
      },
    },
    {
      field: 'requestedShipDate', headerName: 'Ship Date', width: 110,
      renderCell: (p: GridRenderCellParams<ShipmentSummaryDto>) => {
        const d = p.value ? new Date(p.value as string) : null;
        const today = new Date();
        const overdue = d && d < today && p.row.status !== ShipmentStatus.Dispatched && p.row.status !== ShipmentStatus.Cancelled;
        return (
          <Typography variant="body2" sx={{ fontSize: 12, color: overdue ? 'error.main' : 'text.primary', fontWeight: overdue ? 600 : 400 }}>
            {d ? d.toLocaleDateString() : '—'}
          </Typography>
        );
      },
    },
    {
      field: 'carrierName', headerName: 'Carrier', width: 130,
      renderCell: (p: GridRenderCellParams<ShipmentSummaryDto>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>
          {p.value ?? '—'}
        </Typography>
      ),
    },
    {
      field: 'lineCount', headerName: 'Lines', width: 70, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<ShipmentSummaryDto>) => (
        <Chip label={numVal(p.value as number)} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.primary.main, 0.08), color: 'primary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'actions', headerName: '', width: 120, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<ShipmentSummaryDto>) => {
        const id = numVal(p.row.shipmentId);
        const st = p.row.status;
        return (
          <Stack direction="row" spacing={0.25}>
            {st === ShipmentStatus.Draft && (
              <Tooltip title="Generate Pick List">
                <IconButton size="small" onClick={() => pickListMutation.mutate(id)} sx={{ color: 'text.secondary' }}>
                  <SolarIcon name="reports" size={16} />
                </IconButton>
              </Tooltip>
            )}
            {(st === ShipmentStatus.Picking || st === ShipmentStatus.Packed) && (
              <Tooltip title="Dispatch">
                <IconButton size="small" onClick={() => setDispatchTarget(p.row)} sx={{ color: 'success.main' }}>
                  <SolarIcon name="success" size={16} />
                </IconButton>
              </Tooltip>
            )}
            {st === ShipmentStatus.Draft && (
              <Tooltip title="Delete">
                <IconButton size="small" onClick={() => setDeleteTarget(p.row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
                  <SolarIcon name="delete" size={16} />
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
      <PageHeader title="Shipments" breadcrumbs={[{ label: 'Warehouse' }, { label: 'Shipments' }]} />
      <EmptyState icon="emptyTable" title="Failed to load shipments" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Outbound Shipments"
        subtitle="Manage shipment orders, picking and dispatch"
        breadcrumbs={[{ label: 'Warehouse' }, { label: 'Shipments' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => { setSaveError(''); setCreateOpen(true); }}>
            New Shipment
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search shipment or customer…"
        filters={[
          {
            label: 'Status', value: statusFilter,
            options: Object.values(ShipmentStatus).map((s) => ({ label: s, value: s })),
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
          getRowId={(r) => String(r.shipmentId)}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || statusFilter ? 'No shipments match your filters' : 'No shipments yet'}
                description={search || statusFilter ? 'Try adjusting your search.' : 'Create your first shipment order.'}
                action={!search && !statusFilter ? <Button variant="contained" size="small" onClick={() => setCreateOpen(true)}>New Shipment</Button> : undefined}
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

      {/* Create drawer */}
      <FormDrawer
        open={createOpen} onClose={() => setCreateOpen(false)}
        title="New Shipment"
        subtitle="Create a new outbound shipment order"
        onSubmit={() => document.getElementById('shipment-create-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Create Shipment"
        loading={createMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <ShipmentCreateForm onSubmit={(d) => createMutation.mutate(d)} />
      </FormDrawer>

      {/* Dispatch drawer */}
      {dispatchTarget && (
        <FormDrawer
          open={!!dispatchTarget} onClose={() => setDispatchTarget(null)}
          title="Dispatch Shipment"
          subtitle={dispatchTarget.shipmentCode}
          onSubmit={() => document.getElementById('dispatch-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
          submitLabel="Confirm Dispatch"
          loading={dispatchMutation.isPending}
        >
          {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
          <DispatchForm
            key={String(dispatchTarget.shipmentId)}
            shipment={dispatchTarget}
            onSubmit={(d) => dispatchMutation.mutate({ id: numVal(dispatchTarget.shipmentId), d })}
          />
        </FormDrawer>
      )}

      {/* Delete dialog */}
      <ConfirmDialog
        open={!!deleteTarget} onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(numVal(deleteTarget.shipmentId))}
        title="Delete Shipment"
        description={<>Delete shipment <strong>{deleteTarget?.shipmentCode}</strong>? This cannot be undone.</>}
        confirmLabel="Delete" confirmColor="error" loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
