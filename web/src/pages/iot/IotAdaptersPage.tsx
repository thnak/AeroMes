import {
  Alert,
  Box,
  Button,
  Chip,
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
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
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
  useGetApiV1IotAdapters,
  getGetApiV1IotAdaptersQueryKey,
  getApiV1IotAdaptersId,
  getGetApiV1IotAdaptersIdQueryKey,
  postApiV1IotAdapters,
  putApiV1IotAdaptersId,
  deleteApiV1IotAdaptersId,
  patchApiV1IotAdaptersIdEnable,
  patchApiV1IotAdaptersIdDisable,
} from '../../api/iot/iot';
import { AdapterType, AdapterStatus } from '../../api/model';
import type { AdapterDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const CONFIG_TEMPLATES: Record<string, string> = {
  [AdapterType.Mqtt]:    JSON.stringify({ brokerUrl: 'mqtt://broker:1883', clientId: '', username: '', password: '', topicPrefix: '' }, null, 2),
  [AdapterType.OpcUa]:  JSON.stringify({ endpointUrl: 'opc.tcp://server:4840', securityMode: 'None', username: '', password: '' }, null, 2),
  [AdapterType.Modbus]: JSON.stringify({ host: '', port: 502, unitId: 1, protocol: 'TCP' }, null, 2),
  [AdapterType.Webhook]: JSON.stringify({}, null, 2),
};

const ADAPTER_TYPE_LABELS: Record<string, string> = {
  [AdapterType.Mqtt]:    'MQTT',
  [AdapterType.OpcUa]:  'OPC-UA',
  [AdapterType.Modbus]: 'Modbus',
  [AdapterType.Webhook]: 'Webhook',
};

const STATUS_COLORS: Record<string, string> = {
  [AdapterStatus.Connected]:    '#15803D',
  [AdapterStatus.Disconnected]: '#DC2626',
  [AdapterStatus.Degraded]:     '#D97706',
  [AdapterStatus.Unknown]:      '#64748B',
};

const CreateSchema = z.object({
  adapterType: z.string().min(1),
  configJson:  z.string().min(1),
});
type CreateForm = z.infer<typeof CreateSchema>;

const EditSchema = z.object({ configJson: z.string().min(1) });
type EditForm = z.infer<typeof EditSchema>;

export default function IotAdaptersPage() {
  const { machineCode = '' } = useParams<{ machineCode: string }>();
  const navigate   = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch]         = useState('');
  const [drawer, setDrawer]         = useState<'create' | 'edit' | null>(null);
  const [editTarget, setEditTarget] = useState<AdapterDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<AdapterDto | null>(null);
  const [toastError, setToastError] = useState<string | null>(null);

  const { data: adapters = [], isLoading, error, refetch } =
    useGetApiV1IotAdapters({ machineCode }, { query: { enabled: !!machineCode } });

  // Fetch detail (with configJson) only when edit drawer is open
  const { data: editDetail } = useQuery({
    queryKey: getGetApiV1IotAdaptersIdQueryKey(Number(editTarget?.adapterId ?? 0)),
    queryFn:  () => getApiV1IotAdaptersId(Number(editTarget!.adapterId)),
    enabled:  drawer === 'edit' && !!editTarget,
  });

  const filtered = search
    ? adapters.filter((a) =>
        ADAPTER_TYPE_LABELS[a.adapterType]?.toLowerCase().includes(search.toLowerCase()) ||
        a.adapterType.toLowerCase().includes(search.toLowerCase()))
    : adapters;

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1IotAdaptersQueryKey({ machineCode }) });

  const createForm = useForm<CreateForm>({
    resolver: zodResolver(CreateSchema) as any,
    defaultValues: { adapterType: AdapterType.Mqtt, configJson: CONFIG_TEMPLATES[AdapterType.Mqtt] },
  });

  const editForm = useForm<EditForm>({
    resolver: zodResolver(EditSchema) as any,
    defaultValues: { configJson: '{}' },
    values: editDetail ? { configJson: editDetail.configJson } : undefined,
  });

  const createMutation = useMutation({
    mutationFn: (v: CreateForm) =>
      postApiV1IotAdapters({ machineCode, adapterType: v.adapterType, configJson: v.configJson }),
    onSuccess: () => {
      invalidate(); setDrawer(null);
      createForm.reset({ adapterType: AdapterType.Mqtt, configJson: CONFIG_TEMPLATES[AdapterType.Mqtt] });
    },
    onError: (e) => setToastError(getErrorMessage(e)),
  });

  const editMutation = useMutation({
    mutationFn: (v: EditForm) => putApiV1IotAdaptersId(Number(editTarget!.adapterId), { configJson: v.configJson }),
    onSuccess: () => { invalidate(); setDrawer(null); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1IotAdaptersId(id),
    onSuccess:  () => { invalidate(); setDeleteTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const enableMutation  = useMutation({ mutationFn: (id: number) => patchApiV1IotAdaptersIdEnable(id),  onSuccess: invalidate, onError: (e) => setToastError(getErrorMessage(e)) });
  const disableMutation = useMutation({ mutationFn: (id: number) => patchApiV1IotAdaptersIdDisable(id), onSuccess: invalidate, onError: (e) => setToastError(getErrorMessage(e)) });

  const columns: GridColDef<AdapterDto>[] = [
    {
      field: 'adapterType',
      headerName: 'Type',
      width: 110,
      renderCell: (p: GridRenderCellParams<AdapterDto>) => (
        <Typography variant="body2" sx={{ fontWeight: 600 }}>{ADAPTER_TYPE_LABELS[p.value] ?? p.value}</Typography>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (p: GridRenderCellParams<AdapterDto>) => (
        <Chip label={p.value} size="small" sx={{
          height: 20, fontSize: '0.6875rem', fontWeight: 600,
          bgcolor: alpha(STATUS_COLORS[p.value] ?? '#64748B', 0.1), color: STATUS_COLORS[p.value] ?? '#64748B',
          border: 'none', '& .MuiChip-label': { px: 0.75 },
        }} />
      ),
    },
    {
      field: 'isEnabled',
      headerName: 'Enabled',
      width: 90,
      renderCell: (p: GridRenderCellParams<AdapterDto>) => (
        <Chip label={p.value ? 'Yes' : 'No'} size="small" sx={{
          height: 20, fontSize: '0.6875rem', fontWeight: 600,
          bgcolor: p.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
          color: p.value ? '#15803D' : '#94A3B8',
          border: 'none', '& .MuiChip-label': { px: 0.75 },
        }} />
      ),
    },
    {
      field: 'signalCount',
      headerName: 'Signals',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<AdapterDto>) => (
        <Typography variant="body2">{Number(p.value)}</Typography>
      ),
    },
    {
      field: 'lastSignalAt',
      headerName: 'Last Signal',
      flex: 1,
      renderCell: (p: GridRenderCellParams<AdapterDto>) =>
        p.value
          ? <Typography variant="body2" color="text.secondary">{new Date(p.value).toLocaleString()}</Typography>
          : <Typography variant="body2" color="text.disabled">—</Typography>,
    },
    {
      field: 'webhookApiKey',
      headerName: 'API Key',
      width: 180,
      renderCell: (p: GridRenderCellParams<AdapterDto>) =>
        p.value
          ? <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.75rem', color: 'text.secondary' }}>{p.value}</Typography>
          : <Typography variant="body2" color="text.disabled">—</Typography>,
    },
    {
      field: 'actions',
      headerName: '',
      width: 160,
      sortable: false,
      align: 'center',
      renderCell: (p: GridRenderCellParams<AdapterDto>) => {
        const id = Number(p.row.adapterId);
        return (
          <Stack direction="row" spacing={0.25}>
            <Tooltip title="Signal Mappings">
              <IconButton size="small" onClick={() => navigate(`/iot/adapters/${id}/signals`)} sx={{ color: '#7C3AED' }}>
                <SolarIcon name="machineOn" size={16} />
              </IconButton>
            </Tooltip>
            <Tooltip title="Edit Config">
              <IconButton size="small" onClick={() => { setEditTarget(p.row); setDrawer('edit'); }} sx={{ color: 'text.secondary' }}>
                <SolarIcon name="edit" size={16} />
              </IconButton>
            </Tooltip>
            <Tooltip title={p.row.isEnabled ? 'Disable' : 'Enable'}>
              <IconButton
                size="small"
                onClick={() => p.row.isEnabled ? disableMutation.mutate(id) : enableMutation.mutate(id)}
                sx={{ color: p.row.isEnabled ? '#D97706' : '#15803D' }}
              >
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

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title={`IoT Adapters — ${machineCode}`} breadcrumbs={[{ label: 'Master Data' }, { label: 'Machines', href: '/master/machines' }, { label: machineCode }, { label: 'Adapters' }]} />
      <EmptyState icon="emptyTable" title="Failed to load adapters" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title={`IoT Adapters — ${machineCode}`}
        subtitle="Configure data acquisition adapters for this machine"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Machines', href: '/master/machines' }, { label: machineCode }, { label: 'Adapters' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => refetch()} />
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />}
              onClick={() => { createForm.reset({ adapterType: AdapterType.Mqtt, configJson: CONFIG_TEMPLATES[AdapterType.Mqtt] }); setDrawer('create'); }}>
              Add Adapter
            </Button>
          </Stack>
        }
      />

      {toastError && <Alert severity="error" onClose={() => setToastError(null)} sx={{ mb: 2 }}>{toastError}</Alert>}

      <TableToolbar search={search} onSearchChange={setSearch} searchPlaceholder="Filter by adapter type…" />

      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(row) => row.adapterId}
          disableRowSelectionOnClick
          slots={{ noRowsOverlay: () => <EmptyState icon="emptyTable" title="No adapters yet" description="Add an adapter to start receiving IoT data." /> }}
          sx={{ border: 'none', '& .MuiDataGrid-cell': { alignItems: 'center' } }}
        />
      </Box>

      {/* ── Create drawer ── */}
      <FormDrawer
        open={drawer === 'create'}
        onClose={() => setDrawer(null)}
        title="Add Adapter"
        onSubmit={() => void createForm.handleSubmit((v) => createMutation.mutate(v))()}
        loading={createMutation.isPending}
      >
        <Controller name="adapterType" control={createForm.control} render={({ field, fieldState }) => (
          <TextField {...field} select label="Adapter Type" fullWidth error={!!fieldState.error} helperText={fieldState.error?.message}
            onChange={(e) => { field.onChange(e); createForm.setValue('configJson', CONFIG_TEMPLATES[e.target.value] ?? '{}'); }}>
            {Object.values(AdapterType).map((t) => (
              <MenuItem key={t} value={t}>{ADAPTER_TYPE_LABELS[t] ?? t}</MenuItem>
            ))}
          </TextField>
        )} />
        <Controller name="configJson" control={createForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Config (JSON)" fullWidth multiline rows={8}
            error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Protocol-specific connection settings'}
            slotProps={{ htmlInput: { style: { fontFamily: 'monospace', fontSize: '0.8125rem' } } }} />
        )} />
      </FormDrawer>

      {/* ── Edit drawer ── */}
      <FormDrawer
        open={drawer === 'edit'}
        onClose={() => setDrawer(null)}
        title={`Edit Config — ${editTarget ? ADAPTER_TYPE_LABELS[editTarget.adapterType] ?? editTarget.adapterType : ''}`}
        onSubmit={() => void editForm.handleSubmit((v) => editMutation.mutate(v))()}
        loading={editMutation.isPending}
      >
        <Controller name="configJson" control={editForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Config (JSON)" fullWidth multiline rows={12}
            error={!!fieldState.error} helperText={fieldState.error?.message ?? 'Protocol-specific connection settings'}
            slotProps={{ htmlInput: { style: { fontFamily: 'monospace', fontSize: '0.8125rem' } } }} />
        )} />
      </FormDrawer>

      {/* ── Delete confirm ── */}
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Adapter"
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.adapterId))}
        loading={deleteMutation.isPending}
        description={<>Delete <strong>{ADAPTER_TYPE_LABELS[deleteTarget?.adapterType ?? ''] ?? deleteTarget?.adapterType}</strong> adapter? All {Number(deleteTarget?.signalCount) > 0 ? `${Number(deleteTarget?.signalCount)} signal mapping(s)` : 'associated signals'} will also be removed.</>}
      />
    </PageRoot>
  );
}
