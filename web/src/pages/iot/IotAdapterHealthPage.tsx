import {
  Alert,
  Box,
  Chip,
  Divider,
  Drawer,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  EmptyState,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
} from '../../components';
import { getApiV1IotAdaptersHealth, getApiV1IotAdaptersIdHealth } from '../../api/iot/iot';
import type { AdapterHealthDto, AdapterHealthLogDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';
import type { IconKey } from '../../lib/icons';

const STATUS_COLORS: Record<string, string> = {
  Connected:    '#15803D',
  Degraded:     '#D97706',
  Disconnected: '#DC2626',
  Unknown:      '#64748B',
};

const STATUS_ICONS: Record<string, IconKey> = {
  Connected:    'success',
  Degraded:     'warning',
  Disconnected: 'error',
  Unknown:      'info',
};

const EVENT_COLORS: Record<string, string> = {
  Connected:    '#15803D',
  Degraded:     '#D97706',
  Disconnected: '#DC2626',
  Unknown:      '#64748B',
  Disabled:     '#64748B',
  StaleDetected:'#DC2626',
  Recovered:    '#15803D',
};

function formatAgo(date: string | null | undefined): string {
  if (!date) return '—';
  const diff = (Date.now() - new Date(date).getTime()) / 1000;
  if (diff < 60) return `${Math.round(diff)}s ago`;
  if (diff < 3600) return `${Math.round(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.round(diff / 3600)}h ago`;
  return `${Math.round(diff / 86400)}d ago`;
}

function StatusChip({ status }: { status: string }) {
  const color = STATUS_COLORS[status] ?? '#64748B';
  return (
    <Chip
      icon={<SolarIcon name={STATUS_ICONS[status] ?? 'info'} size={12} color={color} />}
      label={status}
      size="small"
      sx={{
        height: 22, fontSize: '0.6875rem', fontWeight: 600,
        bgcolor: alpha(color, 0.1), color, border: 'none',
        '& .MuiChip-icon': { ml: 0.75 },
        '& .MuiChip-label': { pl: 0.5, pr: 0.75 },
      }}
    />
  );
}

function AdapterHealthDetail({ adapterId }: { adapterId: number }) {
  const { data, isLoading, error } = useQuery({
    queryKey: ['adapter-health-detail', adapterId],
    queryFn: () => getApiV1IotAdaptersIdHealth(adapterId, { logLimit: 50 }),
    refetchInterval: 30_000,
  });

  if (isLoading) return <Box sx={{ p: 2, color: 'text.secondary' }}>Loading…</Box>;
  if (error || !data) return <Box sx={{ p: 2, color: 'error.main' }}>{getErrorMessage(error)}</Box>;

  const logs: AdapterHealthLogDto[] = data.logs ?? [];

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="subtitle2" sx={{ mb: 2, fontWeight: 700 }}>Health Metrics</Typography>

      <Stack spacing={1} sx={{ mb: 3 }}>
        {[
          ['Status',          <StatusChip status={data.health.status} />],
          ['Last Connected',  <Typography variant="body2">{formatAgo(data.health.lastConnectedAt)}</Typography>],
          ['Last Signal',     <Typography variant="body2">{formatAgo(data.health.lastSignalAt)}</Typography>],
          ['Signal Rate',     <Typography variant="body2">{Number(data.health.signalRate1min ?? 0).toFixed(1)}/min</Typography>],
          ['Errors (1 hr)',   <Typography variant="body2" color={Number(data.health.errorCount1hr) > 0 ? 'error' : 'inherit'}>{data.health.errorCount1hr}</Typography>],
          ['Reconnects',      <Typography variant="body2">{data.health.reconnectAttempts}</Typography>],
          ['Last Error',      <Typography variant="body2" color="error.main" sx={{ wordBreak: 'break-word' }}>{data.health.lastError ?? '—'}</Typography>],
        ].map(([label, value], i) => (
          <Stack key={i} direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <Typography variant="body2" color="text.secondary" sx={{ minWidth: 110 }}>{label}</Typography>
            {value}
          </Stack>
        ))}
      </Stack>

      <Divider sx={{ mb: 2 }} />
      <Typography variant="subtitle2" sx={{ mb: 1.5, fontWeight: 700 }}>Recent Events</Typography>

      {logs.length === 0 ? (
        <Typography variant="body2" color="text.secondary">No events recorded yet.</Typography>
      ) : (
        <Stack spacing={0.75}>
          {logs.map((log) => {
            const color = EVENT_COLORS[log.eventType] ?? '#64748B';
            return (
              <Stack key={log.eventId} direction="row" spacing={1.5} sx={{ alignItems: 'flex-start' }}>
                <Box sx={{
                  width: 6, height: 6, borderRadius: '50%', bgcolor: color, mt: 0.75, flexShrink: 0,
                }} />
                <Box>
                  <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, color }}>{log.eventType}</Typography>
                    <Typography variant="caption" color="text.disabled">
                      {new Date(log.eventAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
                    </Typography>
                  </Stack>
                  {log.details && (
                    <Typography variant="caption" color="text.secondary">{log.details}</Typography>
                  )}
                </Box>
              </Stack>
            );
          })}
        </Stack>
      )}
    </Box>
  );
}

export default function IotAdapterHealthPage() {
  const [selectedId, setSelectedId] = useState<number | null>(null);

  const { data: healths = [], isLoading, error, refetch } = useQuery({
    queryKey: ['adapters-health'],
    queryFn: () => getApiV1IotAdaptersHealth(),
    refetchInterval: 30_000,
  });

  const columns: GridColDef<AdapterHealthDto>[] = [
    {
      field: 'adapterId',
      headerName: 'ID',
      width: 60,
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Typography variant="body2" color="text.secondary">#{p.value as number}</Typography>
      ),
    },
    { field: 'machineCode', headerName: 'Machine', width: 140 },
    {
      field: 'adapterType',
      headerName: 'Type',
      width: 90,
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Chip
          label={p.value as string}
          size="small"
          sx={{
            height: 20, fontSize: '0.6875rem', fontWeight: 600,
            bgcolor: alpha('#0891B2', 0.1), color: '#0891B2', border: 'none',
            '& .MuiChip-label': { px: 0.75 },
          }}
        />
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => <StatusChip status={p.value as string} />,
    },
    {
      field: 'lastSignalAt',
      headerName: 'Last Signal',
      width: 120,
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Typography variant="body2">{formatAgo(p.value as string)}</Typography>
      ),
    },
    {
      field: 'lastConnectedAt',
      headerName: 'Last Connected',
      width: 130,
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Typography variant="body2">{formatAgo(p.value as string)}</Typography>
      ),
    },
    {
      field: 'signalRate1min',
      headerName: 'Rate/min',
      width: 90,
      align: 'right',
      headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
          {Number(p.value ?? 0).toFixed(1)}
        </Typography>
      ),
    },
    {
      field: 'errorCount1hr',
      headerName: 'Errors',
      width: 80,
      align: 'right',
      headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Typography
          variant="body2"
          sx={{ fontFamily: 'monospace', color: Number(p.value) > 0 ? 'error.main' : 'inherit' }}
        >
          {Number(p.value ?? 0)}
        </Typography>
      ),
    },
    {
      field: 'reconnectAttempts',
      headerName: 'Reconnects',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>{p.value as number}</Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 60,
      sortable: false,
      align: 'center',
      renderCell: (p: GridRenderCellParams<AdapterHealthDto>) => (
        <Tooltip title="View detail">
          <IconButton size="small" onClick={() => setSelectedId(Number(p.row.adapterId) || null)} sx={{ color: 'primary.main' }}>
            <SolarIcon name="info" size={15} />
          </IconButton>
        </Tooltip>
      ),
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Adapter Health" breadcrumbs={[{ label: 'IoT' }, { label: 'Health' }]} />
      <Alert severity="error">{getErrorMessage(error)}</Alert>
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Adapter Health"
        subtitle="Liveness monitoring — updated every 30 s"
        breadcrumbs={[{ label: 'IoT' }, { label: 'Adapter Health' }]}
        actions={<RefreshButton onClick={() => void refetch()} />}
      />

      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={healths}
          columns={columns}
          getRowId={(row) => row.adapterId ?? 0}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon="emptyTable"
                title="No health data yet"
                description="The watchdog runs every 30 s. Come back shortly after adapters are enabled."
              />
            ),
          }}
          sx={{ border: 'none', '& .MuiDataGrid-cell': { alignItems: 'center' } }}
        />
      </Box>

      <Drawer
        anchor="right"
        open={selectedId !== null}
        onClose={() => setSelectedId(null)}
        slotProps={{ paper: { sx: { width: 380 } } }}
      >
        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', p: 2, borderBottom: 1, borderColor: 'divider' }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
            Adapter #{selectedId} — Health
          </Typography>
          <IconButton size="small" onClick={() => setSelectedId(null)}>
            <SolarIcon name="close" size={16} />
          </IconButton>
        </Stack>
        {selectedId !== null && <AdapterHealthDetail adapterId={selectedId} />}
      </Drawer>
    </PageRoot>
  );
}
