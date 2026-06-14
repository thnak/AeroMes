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
  MenuItem,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useFieldArray, useForm, Controller } from 'react-hook-form';
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
  useGetApiV1WarehousePurchaseOrders,
  getGetApiV1WarehousePurchaseOrdersQueryKey,
  postApiV1WarehousePurchaseOrders,
  postApiV1WarehousePurchaseOrdersIdConfirm,
} from '../../api/purchase-orders/purchase-orders';
import { useGetApiV1Suppliers } from '../../api/suppliers/suppliers';
import type { PurchaseOrderDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const STATUS_COLORS: Record<string, string> = {
  DRAFT:     '#94A3B8',
  CONFIRMED: '#60A5FA',
  OPEN:      '#D97706',
  RECEIVING: '#7C3AED',
  RECEIVED:  '#15803D',
  CANCELLED: '#DC2626',
};

function numVal(v: number | string): number {
  return typeof v === 'number' ? v : parseInt(v as string, 10) || 0;
}

// ─── Form schemas ─────────────────────────────────────────────────────────────

const LineSchema = z.object({
  productCode:       z.string().min(1, 'Required'),
  orderedQty:        z.number().min(0.001, 'Required'),
  unitPrice:         z.number().optional().nullable(),
  expectedLotNumber: z.string().optional().nullable(),
});

const PoSchema = z.object({
  poCode:               z.string().min(1, 'Required').max(50),
  supplierCode:         z.string().min(1, 'Required'),
  expectedDeliveryDate: z.string().min(1, 'Required'),
  notes:                z.string().max(1000).optional().nullable(),
  lines:                z.array(LineSchema).min(1, 'At least one line required'),
});
type PoFormValues = z.infer<typeof PoSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function PoForm({ onSubmit, suppliers }: { onSubmit: (d: PoFormValues) => void; suppliers: string[] }) {
  const today = new Date().toISOString().split('T')[0];
  const { register, control, handleSubmit, formState: { errors } } = useForm<PoFormValues>({
    resolver: zodResolver(PoSchema),
    defaultValues: {
      poCode: '', supplierCode: '', expectedDeliveryDate: today, notes: '',
      lines: [{ productCode: '', orderedQty: 1, unitPrice: null, expectedLotNumber: null }],
    },
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'lines' });

  return (
    <Box component="form" id="po-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 5 }}>
          <TextField
            {...register('poCode')} label="PO Code" fullWidth required
            error={!!errors.poCode} helperText={errors.poCode?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 7 }}>
          <Controller name="supplierCode" control={control} render={({ field }) => (
            <TextField {...field} select label="Supplier" fullWidth required
              error={!!errors.supplierCode} helperText={errors.supplierCode?.message}>
              <MenuItem value="">Select supplier…</MenuItem>
              {suppliers.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
            </TextField>
          )} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('expectedDeliveryDate')} label="Expected Delivery" type="date"
            fullWidth required error={!!errors.expectedDeliveryDate}
            helperText={errors.expectedDeliveryDate?.message}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('notes')} label="Notes" fullWidth multiline rows={1} />
        </Grid>

        {/* Lines */}
        <Grid size={{ xs: 12 }}>
          <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>Order Lines</Typography>
            <Button size="small" startIcon={<SolarIcon name="add" size={14} />}
              onClick={() => append({ productCode: '', orderedQty: 1, unitPrice: null, expectedLotNumber: null })}>
              Add Line
            </Button>
          </Stack>
          {errors.lines?.root && <Alert severity="error" sx={{ mb: 1 }}>{errors.lines.root.message}</Alert>}
          <Stack spacing={1.5}>
            {fields.map((field, idx) => (
              <Box key={field.id} sx={{ p: 1.5, borderRadius: 1.5, border: '1px solid', borderColor: 'divider', bgcolor: 'action.hover' }}>
                <Stack direction="row" spacing={1} sx={{ alignItems: 'flex-start' }}>
                  <TextField
                    {...register(`lines.${idx}.productCode`)} label="Product Code" size="small" required
                    error={!!errors.lines?.[idx]?.productCode}
                    helperText={errors.lines?.[idx]?.productCode?.message}
                    autoFocus={idx === 0}
                    slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 }, placeholder: 'Scan or type…' } }}
                    sx={{ flex: 1.5 }}
                  />
                  <TextField
                    {...register(`lines.${idx}.orderedQty`, { valueAsNumber: true })}
                    label="Qty" size="small" type="number" required
                    error={!!errors.lines?.[idx]?.orderedQty}
                    helperText={errors.lines?.[idx]?.orderedQty?.message}
                    sx={{ width: 80 }}
                  />
                  <TextField
                    {...register(`lines.${idx}.unitPrice`, { valueAsNumber: true, setValueAs: (v) => v === '' ? null : parseFloat(v) })}
                    label="Unit Price" size="small" type="number"
                    sx={{ width: 100 }}
                  />
                  <TextField
                    {...register(`lines.${idx}.expectedLotNumber`)}
                    label="Lot # (opt)" size="small"
                    slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
                    sx={{ flex: 1 }}
                  />
                  <Tooltip title="Remove line">
                    <IconButton size="small" onClick={() => remove(idx)} sx={{ mt: 0.5 }}>
                      <SolarIcon name="delete" size={16} />
                    </IconButton>
                  </Tooltip>
                </Stack>
              </Box>
            ))}
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function PurchaseOrdersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch]               = useState('');
  const [statusFilter, setStatusFilter]   = useState('');
  const [drawerOpen, setDrawerOpen]       = useState(false);
  const [confirmTarget, setConfirmTarget] = useState<PurchaseOrderDto | null>(null);
  const [saveError, setSaveError]         = useState('');

  const { data: poResp, isLoading, error, refetch } = useGetApiV1WarehousePurchaseOrders(
    statusFilter ? { status: statusFilter } : undefined,
  );
  const orders: PurchaseOrderDto[] = (poResp?.data ?? []) as PurchaseOrderDto[];

  const { data: suppliers = [] } = useGetApiV1Suppliers();
  const supplierCodes = useMemo(() => suppliers.map((s) => s.supplierCode), [suppliers]);

  const statuses = useMemo(() => [...new Set(orders.map((o) => o.status))], [orders]);

  const filtered = useMemo(() => {
    if (!search) return orders;
    const q = search.toLowerCase();
    return orders.filter((o) => o.poCode.toLowerCase().includes(q) || o.supplierCode.toLowerCase().includes(q));
  }, [orders, search]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1WarehousePurchaseOrdersQueryKey() });

  const createMutation = useMutation({
    mutationFn: (d: PoFormValues) => postApiV1WarehousePurchaseOrders({
      poCode: d.poCode,
      supplierCode: d.supplierCode,
      expectedDeliveryDate: d.expectedDeliveryDate,
      notes: d.notes ?? null,
      lines: d.lines.map((l) => ({
        productCode: l.productCode,
        orderedQty: l.orderedQty,
        unitPrice: l.unitPrice ?? null,
        expectedLotNumber: l.expectedLotNumber ?? null,
      })),
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); setSaveError(''); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const confirmMutation = useMutation({
    mutationFn: (id: number) => postApiV1WarehousePurchaseOrdersIdConfirm(id),
    onSuccess: () => { invalidate(); setConfirmTarget(null); },
  });

  const columns: GridColDef<PurchaseOrderDto>[] = [
    {
      field: 'poCode', headerName: 'PO Code', width: 140,
      renderCell: (p: GridRenderCellParams<PurchaseOrderDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    { field: 'supplierCode', headerName: 'Supplier', flex: 1, minWidth: 140 },
    {
      field: 'status', headerName: 'Status', width: 120,
      renderCell: (p: GridRenderCellParams<PurchaseOrderDto>) => {
        const color = STATUS_COLORS[p.value as string] ?? '#94A3B8';
        return (
          <Chip label={p.value} size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        );
      },
    },
    {
      field: 'expectedDeliveryDate', headerName: 'Expected Date', width: 130,
      renderCell: (p: GridRenderCellParams<PurchaseOrderDto>) => {
        const d = p.value ? new Date(p.value as string) : null;
        const today = new Date();
        const overdue = d && d < today && p.row.status !== 'RECEIVED' && p.row.status !== 'CANCELLED';
        return (
          <Typography variant="body2" sx={{ fontSize: 12, color: overdue ? 'error.main' : 'text.primary', fontWeight: overdue ? 600 : 400 }}>
            {d ? d.toLocaleDateString() : '—'}
            {overdue && ' ⚠'}
          </Typography>
        );
      },
    },
    {
      field: 'lineCount', headerName: 'Lines', width: 80, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<PurchaseOrderDto>) => (
        <Chip label={numVal(p.value as number)} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.primary.main, 0.08), color: 'primary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'createdAt', headerName: 'Created', width: 100,
      renderCell: (p: GridRenderCellParams<PurchaseOrderDto>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>
          {p.value ? new Date(p.value as string).toLocaleDateString() : '—'}
        </Typography>
      ),
    },
    {
      field: 'actions', headerName: '', width: 100, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<PurchaseOrderDto>) => (
        <Stack direction="row" spacing={0.25}>
          {p.row.status === 'DRAFT' && (
            <Tooltip title="Confirm PO">
              <IconButton size="small" onClick={() => setConfirmTarget(p.row)} sx={{ color: 'success.main' }}>
                <SolarIcon name="success" size={16} />
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
      <PageHeader title="Purchase Orders" breadcrumbs={[{ label: 'Warehouse' }, { label: 'Purchase Orders' }]} />
      <EmptyState icon="emptyTable" title="Failed to load purchase orders" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Purchase Orders"
        subtitle="Manage incoming purchase orders and receiving"
        breadcrumbs={[{ label: 'Warehouse' }, { label: 'Purchase Orders' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => { setSaveError(''); setDrawerOpen(true); }}>
            New PO
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search PO code or supplier…"
        filters={[
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
          getRowId={(r) => String(r.poId)}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || statusFilter ? 'No purchase orders match your filters' : 'No purchase orders yet'}
                description={search || statusFilter ? 'Try adjusting your search or filters.' : 'Create your first purchase order.'}
                action={!search && !statusFilter ? <Button variant="contained" size="small" onClick={() => setDrawerOpen(true)}>New PO</Button> : undefined}
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
        title="New Purchase Order"
        subtitle="Fill in PO details and add order lines"
        onSubmit={() => document.getElementById('po-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Create PO"
        loading={createMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <PoForm suppliers={supplierCodes} onSubmit={(d) => createMutation.mutate(d)} />
      </FormDrawer>

      <ConfirmDialog
        open={!!confirmTarget} onClose={() => setConfirmTarget(null)}
        onConfirm={() => confirmTarget && confirmMutation.mutate(numVal(confirmTarget.poId))}
        title="Confirm Purchase Order"
        description={<>Confirm PO <strong>{confirmTarget?.poCode}</strong> from <strong>{confirmTarget?.supplierCode}</strong>? This will lock the order for receiving.</>}
        confirmLabel="Confirm" loading={confirmMutation.isPending}
      />
    </PageRoot>
  );
}
