import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState } from 'react';
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
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1IotAdaptersId,
  useGetApiV1IotAdaptersIdSignals,
  getGetApiV1IotAdaptersIdSignalsQueryKey,
  postApiV1IotAdaptersIdSignals,
  putApiV1IotSignalsId,
  deleteApiV1IotSignalsId,
  patchApiV1IotSignalsIdEnable,
  patchApiV1IotSignalsIdDisable,
} from '../../api/iot/iot';
import { AdapterType } from '../../api/model';
import type { SignalDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const ADAPTER_TYPE_LABELS: Record<string, string> = {
  [AdapterType.Mqtt]:    'MQTT',
  [AdapterType.OpcUa]:  'OPC-UA',
  [AdapterType.Modbus]: 'Modbus',
  [AdapterType.Webhook]: 'Webhook',
};

const SignalSchema = z.object({
  tagKey:        z.string().min(1, 'Required').max(100).regex(/^[A-Za-z0-9_\-.]+$/, 'Letters, digits, _, -, . only'),
  displayName:   z.string().min(1, 'Required').max(200),
  sourceAddress: z.string().min(1, 'Required').max(500),
  scale:         z.coerce.number().refine((v) => v !== 0, 'Scale cannot be 0'),
  offset:        z.coerce.number(),
  qualityMin:    z.coerce.number().nullable().optional(),
  qualityMax:    z.coerce.number().nullable().optional(),
});
type SignalForm = z.infer<typeof SignalSchema>;

const DEFAULT_FORM: SignalForm = {
  tagKey: '', displayName: '', sourceAddress: '', scale: 1, offset: 0, qualityMin: null, qualityMax: null,
};

export default function IotSignalsPage() {
  const { adapterId: adapterIdStr = '' } = useParams<{ adapterId: string }>();
  const adapterId = Number(adapterIdStr);
  const navigate  = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [drawer, setDrawer]             = useState<'create' | 'edit' | null>(null);
  const [editTarget, setEditTarget]     = useState<SignalDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<SignalDto | null>(null);
  const [toastError, setToastError]     = useState<string | null>(null);

  const { data: adapter } = useGetApiV1IotAdaptersId(adapterId, { query: { enabled: !!adapterId } });
  const { data: signals = [], isLoading, error, refetch } =
    useGetApiV1IotAdaptersIdSignals(adapterId, { query: { enabled: !!adapterId } });

  const filtered = search
    ? signals.filter((s) =>
        s.tagKey.toLowerCase().includes(search.toLowerCase()) ||
        s.displayName.toLowerCase().includes(search.toLowerCase()) ||
        s.sourceAddress.toLowerCase().includes(search.toLowerCase()))
    : signals;

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1IotAdaptersIdSignalsQueryKey(adapterId) });

  const { control, handleSubmit, reset } = useForm<SignalForm>({
    resolver: zodResolver(SignalSchema) as any,
    defaultValues: DEFAULT_FORM,
  });

  const createMutation = useMutation({
    mutationFn: (v: SignalForm) =>
      postApiV1IotAdaptersIdSignals(adapterId, {
        tagKey: v.tagKey, displayName: v.displayName, sourceAddress: v.sourceAddress,
        scale: v.scale, offset: v.offset, qualityMin: v.qualityMin ?? null, qualityMax: v.qualityMax ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawer(null); reset(DEFAULT_FORM); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const editMutation = useMutation({
    mutationFn: (v: SignalForm) =>
      putApiV1IotSignalsId(Number(editTarget!.signalId), {
        displayName: v.displayName, sourceAddress: v.sourceAddress,
        scale: v.scale, offset: v.offset, qualityMin: v.qualityMin ?? null, qualityMax: v.qualityMax ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawer(null); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1IotSignalsId(id),
    onSuccess:  () => { invalidate(); setDeleteTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const enableMutation  = useMutation({ mutationFn: (id: number) => patchApiV1IotSignalsIdEnable(id),  onSuccess: invalidate, onError: (e) => setToastError(getErrorMessage(e)) });
  const disableMutation = useMutation({ mutationFn: (id: number) => patchApiV1IotSignalsIdDisable(id), onSuccess: invalidate, onError: (e) => setToastError(getErrorMessage(e)) });

  const openEdit = (row: SignalDto) => {
    setEditTarget(row);
    reset({
      tagKey:        row.tagKey,
      displayName:   row.displayName,
      sourceAddress: row.sourceAddress,
      scale:         Number(row.scale),
      offset:        Number(row.offset),
      qualityMin:    row.qualityMin != null ? Number(row.qualityMin) : null,
      qualityMax:    row.qualityMax != null ? Number(row.qualityMax) : null,
    });
    setDrawer('edit');
  };

  const columns: GridColDef<SignalDto>[] = [
    {
      field: 'tagKey',
      headerName: 'Tag Key',
      width: 160,
      renderCell: (p: GridRenderCellParams<SignalDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>{p.value}</Typography>
      ),
    },
    { field: 'displayName', headerName: 'Display Name', flex: 1 },
    {
      field: 'sourceAddress',
      headerName: 'Source Address',
      flex: 1,
      renderCell: (p: GridRenderCellParams<SignalDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.75rem', color: 'text.secondary' }}>{p.value}</Typography>
      ),
    },
    {
      field: 'scale',
      headerName: 'Scale / Offset',
      width: 130,
      renderCell: (p: GridRenderCellParams<SignalDto>) => (
        <Typography variant="body2" color="text.secondary">×{Number(p.row.scale)} + {Number(p.row.offset)}</Typography>
      ),
    },
    {
      field: 'qualityMin',
      headerName: 'Quality Range',
      width: 130,
      renderCell: (p: GridRenderCellParams<SignalDto>) =>
        p.row.qualityMin != null || p.row.qualityMax != null ? (
          <Typography variant="body2" color="text.secondary">
            {p.row.qualityMin != null ? Number(p.row.qualityMin) : '—'} … {p.row.qualityMax != null ? Number(p.row.qualityMax) : '—'}
          </Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">—</Typography>
        ),
    },
    {
      field: 'isEnabled',
      headerName: 'Enabled',
      width: 90,
      renderCell: (p: GridRenderCellParams<SignalDto>) => (
        <Chip label={p.value ? 'Yes' : 'No'} size="small" sx={{
          height: 20, fontSize: '0.6875rem', fontWeight: 600,
          bgcolor: p.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
          color: p.value ? '#15803D' : '#94A3B8',
          border: 'none', '& .MuiChip-label': { px: 0.75 },
        }} />
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 120,
      sortable: false,
      align: 'center',
      renderCell: (p: GridRenderCellParams<SignalDto>) => {
        const id = Number(p.row.signalId);
        return (
          <Stack direction="row" spacing={0.25}>
            <Tooltip title="Edit">
              <IconButton size="small" onClick={() => openEdit(p.row)} sx={{ color: 'text.secondary' }}>
                <SolarIcon name="edit" size={16} />
              </IconButton>
            </Tooltip>
            <Tooltip title={p.row.isEnabled ? 'Disable' : 'Enable'}>
              <IconButton size="small" onClick={() => p.row.isEnabled ? disableMutation.mutate(id) : enableMutation.mutate(id)}
                sx={{ color: p.row.isEnabled ? '#D97706' : '#15803D' }}>
                <SolarIcon name={p.row.isEnabled ? 'pause' : 'resume'} size={16} />
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

  const adapterLabel = adapter
    ? `${ADAPTER_TYPE_LABELS[adapter.adapterType] ?? adapter.adapterType} on ${adapter.machineCode}`
    : `Adapter #${adapterId}`;

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title={`Signal Mappings — ${adapterLabel}`} breadcrumbs={[{ label: 'IoT' }, { label: adapterLabel }]} />
      <EmptyState icon="emptyTable" title="Failed to load signals" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title={`Signal Mappings — ${adapterLabel}`}
        subtitle="Map protocol-specific addresses to named signal tags"
        breadcrumbs={[
          { label: 'Master Data' },
          { label: 'Machines', href: '/master/machines' },
          { label: adapter?.machineCode ?? '' },
          { label: 'Adapters', href: `/iot/machines/${adapter?.machineCode}/adapters` },
          { label: 'Signals' },
        ]}
        actions={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" size="small" startIcon={<SolarIcon name="back" size={16} />}
              onClick={() => navigate(`/iot/machines/${adapter?.machineCode}/adapters`)}>
              Back
            </Button>
            <RefreshButton onClick={() => refetch()} />
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />}
              onClick={() => { reset(DEFAULT_FORM); setDrawer('create'); }}>
              Add Signal
            </Button>
          </Stack>
        }
      />

      {toastError && <Alert severity="error" onClose={() => setToastError(null)} sx={{ mb: 2 }}>{toastError}</Alert>}

      <TableToolbar search={search} onSearchChange={setSearch} searchPlaceholder="Filter by tag key, name or address…" />

      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(row) => row.signalId}
          disableRowSelectionOnClick
          slots={{ noRowsOverlay: () => <EmptyState icon="emptyTable" title="No signal mappings" description="Add a signal mapping to start ingesting data from this adapter." /> }}
          sx={{ border: 'none', '& .MuiDataGrid-cell': { alignItems: 'center' } }}
        />
      </Box>

      {/* ── Form drawer ── */}
      <FormDrawer
        open={drawer !== null}
        onClose={() => setDrawer(null)}
        title={drawer === 'create' ? 'Add Signal Mapping' : `Edit ${editTarget?.tagKey ?? ''}`}
        onSubmit={() => void handleSubmit((v) => drawer === 'create' ? createMutation.mutate(v) : editMutation.mutate(v))()}
        loading={createMutation.isPending || editMutation.isPending}
      >
        <Controller name="tagKey" control={control} render={({ field, fieldState }) => (
          <TextField {...field} label="Tag Key" fullWidth disabled={drawer === 'edit'}
            error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Unique identifier, e.g. spindle_rpm'} />
        )} />
        <Controller name="displayName" control={control} render={({ field, fieldState }) => (
          <TextField {...field} label="Display Name" fullWidth error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Controller name="sourceAddress" control={control} render={({ field, fieldState }) => (
          <TextField {...field} label="Source Address" fullWidth error={!!fieldState.error}
            helperText={fieldState.error?.message ?? 'e.g. MQTT topic, OPC-UA node ID, Modbus register'} />
        )} />
        <Stack direction="row" spacing={2}>
          <Controller name="scale" control={control} render={({ field, fieldState }) => (
            <TextField {...field} label="Scale" type="number" fullWidth
              error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Multiplier (not 0)'} />
          )} />
          <Controller name="offset" control={control} render={({ field, fieldState }) => (
            <TextField {...field} label="Offset" type="number" fullWidth
              error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Added after scale'} />
          )} />
        </Stack>
        <Stack direction="row" spacing={2}>
          <Controller name="qualityMin" control={control} render={({ field, fieldState }) => (
            <TextField {...field} label="Quality Min" type="number" fullWidth
              value={field.value ?? ''} onChange={(e) => field.onChange(e.target.value === '' ? null : e.target.value)}
              error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Optional lower bound'} />
          )} />
          <Controller name="qualityMax" control={control} render={({ field, fieldState }) => (
            <TextField {...field} label="Quality Max" type="number" fullWidth
              value={field.value ?? ''} onChange={(e) => field.onChange(e.target.value === '' ? null : e.target.value)}
              error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Optional upper bound'} />
          )} />
        </Stack>
      </FormDrawer>

      {/* ── Delete confirm ── */}
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Signal Mapping"
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.signalId))}
        loading={deleteMutation.isPending}
        description={<>Delete signal mapping <strong>{deleteTarget?.tagKey}</strong>?</>}
      />
    </PageRoot>
  );
}
