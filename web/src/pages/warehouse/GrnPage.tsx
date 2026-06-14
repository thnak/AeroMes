import {
  Alert,
  Box,
  Button,
  Chip,
  Divider,
  Grid,
  IconButton,
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
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
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
  useGetApiV1WarehouseGrn,
  getGetApiV1WarehouseGrnQueryKey,
  postApiV1WarehouseGrn,
  postApiV1WarehouseGrnGrnIdLines,
  postApiV1WarehouseGrnGrnIdConfirm,
} from '../../api/grn/grn';
import { useGetApiV1StorageLocations } from '../../api/storage-locations/storage-locations';
import { useAuth } from '../../contexts/AuthContext';
import type { GrnListDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const STATUS_COLOR: Record<string, string> = {
  DRAFT: '#94A3B8', CONFIRMED: '#15803D', CANCELLED: '#DC2626',
};

const GrnSchema = z.object({
  grnCode:           z.string().min(1, 'Required').max(30),
  storageLocationId: z.string().min(1, 'Location required'),
  receivedAt:        z.string().min(1, 'Required'),
  deliveryNoteRef:   z.string().max(100).optional().nullable(),
  notes:             z.string().max(500).optional().nullable(),
});
type GrnFormValues = z.infer<typeof GrnSchema>;

const LineSchema = z.object({
  productCode:       z.string().min(1, 'Required').max(50),
  lotNumber:         z.string().min(1, 'Required').max(50),
  receivedQty:       z.number().min(0.001, 'Must be > 0'),
  manufacturedDate:  z.string().optional().nullable(),
  expiryDate:        z.string().optional().nullable(),
  grossWeightKg:     z.number().optional().nullable(),
});
type LineFormValues = z.infer<typeof LineSchema>;

function GrnCreateForm({ locations, onSubmit }: {
  locations: { id: string; label: string }[];
  onSubmit: (d: GrnFormValues) => void;
}) {
  const todayStr = new Date().toISOString().slice(0, 16);
  const { register, handleSubmit, formState: { errors } } =
    useForm<GrnFormValues>({ resolver: zodResolver(GrnSchema), defaultValues: { receivedAt: todayStr } });

  return (
    <Box component="form" id="grn-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('grnCode')} label="GRN Code" fullWidth required
            error={!!errors.grnCode} helperText={errors.grnCode?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('receivedAt')} label="Received At" fullWidth required type="datetime-local"
            error={!!errors.receivedAt} helperText={errors.receivedAt?.message}
            slotProps={{ inputLabel: { shrink: true } }} />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('storageLocationId')} select label="Storage Location" fullWidth required
            error={!!errors.storageLocationId} helperText={errors.storageLocationId?.message}
            defaultValue="">
            {locations.map((l) => <MenuItem key={l.id} value={l.id}>{l.label}</MenuItem>)}
          </TextField>
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('deliveryNoteRef')} label="Delivery Note Ref" fullWidth placeholder="e.g. DN-2026-0123" />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('notes')} label="Notes" fullWidth multiline rows={2} />
        </Grid>
      </Grid>
    </Box>
  );
}

function AddLineForm({ onSubmit }: { onSubmit: (d: LineFormValues) => void }) {
  const { register, handleSubmit, formState: { errors } } =
    useForm<LineFormValues>({ resolver: zodResolver(LineSchema) });

  return (
    <Box component="form" id="line-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('productCode')} label="Product Code" fullWidth required autoFocus
            error={!!errors.productCode} helperText={errors.productCode?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('lotNumber')} label="Lot Number" fullWidth required
            error={!!errors.lotNumber} helperText={errors.lotNumber?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('receivedQty', { valueAsNumber: true })} label="Received Qty" fullWidth required type="number"
            error={!!errors.receivedQty} helperText={errors.receivedQty?.message}
            slotProps={{ htmlInput: { min: 0.001, step: 0.001 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('grossWeightKg', { valueAsNumber: true })} label="Gross Weight (kg)" fullWidth type="number"
            slotProps={{ htmlInput: { min: 0, step: 0.001 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('manufacturedDate')} label="Manufactured Date" fullWidth type="date"
            slotProps={{ inputLabel: { shrink: true } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('expiryDate')} label="Expiry Date" fullWidth type="date"
            slotProps={{ inputLabel: { shrink: true } }} />
        </Grid>
      </Grid>
    </Box>
  );
}

export default function GrnPage() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [createOpen, setCreateOpen]     = useState(false);
  const [addLineTarget, setAddLineTarget] = useState<GrnListDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: grnResponse, isLoading, error, refetch } = useGetApiV1WarehouseGrn();
  const { data: locations = [] } = useGetApiV1StorageLocations();

  const grns: GrnListDto[] = (grnResponse?.data ?? []) as GrnListDto[];

  const locationOptions = useMemo(
    () => locations.filter((l) => l.isActive).map((l) => ({ id: String(l.locationID), label: `${l.locationCode} — ${l.locationName}` })),
    [locations],
  );

  const filtered = useMemo(() => {
    let r = grns;
    if (search) { const q = search.toLowerCase(); r = r.filter((g) => g.grnCode.toLowerCase().includes(q)); }
    if (statusFilter) r = r.filter((g) => g.status === statusFilter);
    return r;
  }, [grns, search, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1WarehouseGrnQueryKey() });
  const numVal = (v: number | string) => typeof v === 'number' ? v : parseInt(v, 10);

  const createMutation = useMutation({
    mutationFn: (d: GrnFormValues) => postApiV1WarehouseGrn({
      grnCode: d.grnCode, poId: null, storageLocationId: d.storageLocationId,
      receivedBy: user?.name ?? 'system', receivedAt: d.receivedAt,
      deliveryNoteRef: d.deliveryNoteRef ?? null, notes: d.notes ?? null,
    }),
    onSuccess: () => { invalidate(); setCreateOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const addLineMutation = useMutation({
    mutationFn: (d: LineFormValues) => postApiV1WarehouseGrnGrnIdLines(
      numVal(addLineTarget!.grnId),
      {
        poLineId: null, productCode: d.productCode, lotNumber: d.lotNumber,
        receivedQty: d.receivedQty, manufacturedDate: d.manufacturedDate ?? null,
        expiryDate: d.expiryDate ?? null,
        grossWeightKg: d.grossWeightKg ?? null, destinationBinId: null,
      },
    ),
    onSuccess: () => { invalidate(); setAddLineTarget(null); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const confirmMutation = useMutation({
    mutationFn: (grnId: number) => postApiV1WarehouseGrnGrnIdConfirm(grnId),
    onSuccess: () => invalidate(),
  });

  const statuses = [...new Set(grns.map((g) => g.status))];

  const columns: GridColDef<GrnListDto>[] = [
    {
      field: 'grnCode', headerName: 'GRN Code', width: 160,
      renderCell: (p: GridRenderCellParams<GrnListDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'status', headerName: 'Status', width: 110,
      renderCell: (p: GridRenderCellParams<GrnListDto>) => {
        const s = p.value as string;
        const color = STATUS_COLOR[s] ?? '#94A3B8';
        return (
          <Chip label={s} size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.12), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        );
      },
    },
    {
      field: 'receivedBy', headerName: 'Received By', width: 150,
      renderCell: (p: GridRenderCellParams<GrnListDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>{p.value}</Typography>
      ),
    },
    {
      field: 'receivedAt', headerName: 'Received At', width: 140,
      renderCell: (p: GridRenderCellParams<GrnListDto>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>
          {new Date(p.value as string).toLocaleString()}
        </Typography>
      ),
    },
    {
      field: 'deliveryNoteRef', headerName: 'Delivery Note', width: 150,
      renderCell: (p: GridRenderCellParams<GrnListDto>) => (
        p.value
          ? <Typography variant="body2" sx={{ fontSize: 12, fontFamily: 'ui-monospace, monospace' }}>{p.value}</Typography>
          : <Typography variant="body2" color="text.disabled" sx={{ fontSize: 12, fontStyle: 'italic' }}>—</Typography>
      ),
    },
    {
      field: 'lineCount', headerName: 'Lines', width: 70, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<GrnListDto>) => (
        <Chip label={numVal(p.value as number)} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.primary.main, 0.08), color: 'primary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'actions', headerName: '', width: 110, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<GrnListDto>) => (
        <Stack direction="row" spacing={0.25}>
          {p.row.status === 'DRAFT' && (
            <>
              <Tooltip title="Add Line">
                <IconButton size="small" onClick={() => { setSaveError(''); setAddLineTarget(p.row); }} sx={{ color: 'text.secondary' }}>
                  <SolarIcon name="add" size={16} />
                </IconButton>
              </Tooltip>
              <Tooltip title="Confirm GRN">
                <IconButton size="small" onClick={() => confirmMutation.mutate(numVal(p.row.grnId))} sx={{ color: '#15803D' }}>
                  <SolarIcon name="success" size={16} />
                </IconButton>
              </Tooltip>
            </>
          )}
        </Stack>
      ),
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Goods Received Notes" breadcrumbs={[{ label: 'Warehouse' }, { label: 'GRN' }]} />
      <EmptyState icon="emptyTable" title="Failed to load GRNs" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Goods Received Notes"
        subtitle="Record and confirm incoming material receipts"
        breadcrumbs={[{ label: 'Warehouse' }, { label: 'GRN' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => { setSaveError(''); setCreateOpen(true); }}>
            New GRN
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search GRN code…"
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
          getRowId={(r) => r.grnId}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || statusFilter ? 'No GRNs match your filters' : 'No receipts yet'}
                description="Create a new GRN to record an incoming shipment."
                action={!search && !statusFilter ? <Button variant="contained" size="small" onClick={() => setCreateOpen(true)}>New GRN</Button> : undefined}
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

      {/* Create GRN drawer */}
      <FormDrawer
        open={createOpen} onClose={() => setCreateOpen(false)}
        title="New GRN" subtitle="Record a new goods receipt"
        onSubmit={() => document.getElementById('grn-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Create GRN" loading={createMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <GrnCreateForm locations={locationOptions} onSubmit={(d) => createMutation.mutate(d)} />
      </FormDrawer>

      {/* Add GRN line drawer */}
      <FormDrawer
        open={!!addLineTarget} onClose={() => setAddLineTarget(null)}
        title={`Add Line — ${addLineTarget?.grnCode ?? ''}`}
        subtitle="Scan or enter product and lot details"
        onSubmit={() => document.getElementById('line-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Add Line" loading={addLineMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        {addLineTarget && (
          <>
            <Stack direction="row" spacing={1} sx={{ mb: 2, p: 1.5, borderRadius: 1, bgcolor: (t) => alpha(t.palette.primary.main, 0.04) }}>
              <Box>
                <Typography variant="caption" color="text.secondary" sx={{ fontSize: 10, fontWeight: 600, textTransform: 'uppercase' }}>GRN</Typography>
                <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>{addLineTarget.grnCode}</Typography>
              </Box>
              <Divider orientation="vertical" flexItem />
              <Box>
                <Typography variant="caption" color="text.secondary" sx={{ fontSize: 10, fontWeight: 600, textTransform: 'uppercase' }}>Lines</Typography>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>{numVal(addLineTarget.lineCount)}</Typography>
              </Box>
            </Stack>
            <AddLineForm onSubmit={(d) => addLineMutation.mutate(d)} />
          </>
        )}
      </FormDrawer>
    </PageRoot>
  );
}
