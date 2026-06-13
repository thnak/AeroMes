import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  EmptyState,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
} from '../../components';
import { useGetApiV1Machines } from '../../api/machines/machines';
import { useGetApiV1WorkCenters } from '../../api/work-centers/work-centers';
import {
  getGetApiV1IotMachinesStatesQueryKey,
  getApiV1IotMachinesStates,
  getApiV1IotAdaptersHealth,
} from '../../api/iot/iot';
import type { MachineDto, MachineStateSnapshotDto, AdapterHealthDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const STATE_COLORS: Record<string, string> = {
  RUNNING: '#15803D',
  IDLE:    '#0891B2',
  SETUP:   '#7C3AED',
  DOWN:    '#D97706',
  FAULT:   '#DC2626',
  UNKNOWN: '#64748B',
};

function StateDot({ state }: { state: string }) {
  const color = STATE_COLORS[state] ?? '#64748B';
  const pulse = state === 'RUNNING';
  return (
    <Box sx={{
      width: 10, height: 10, borderRadius: '50%', bgcolor: color, flexShrink: 0,
      ...(pulse && {
        boxShadow: `0 0 0 0 ${alpha(color, 0.4)}`,
        animation: 'pulse 1.4s ease-in-out infinite',
        '@keyframes pulse': {
          '0%': { boxShadow: `0 0 0 0 ${alpha(color, 0.5)}` },
          '70%': { boxShadow: `0 0 0 8px ${alpha(color, 0)}` },
          '100%': { boxShadow: `0 0 0 0 ${alpha(color, 0)}` },
        },
      }),
    }} />
  );
}

interface MachineCardProps {
  machine: MachineDto;
  snapshot?: MachineStateSnapshotDto;
  health?: AdapterHealthDto;
}

function MachineCard({ machine, snapshot, health }: MachineCardProps) {
  const navigate = useNavigate();
  const state = snapshot?.currentState ?? 'UNKNOWN';
  const color = STATE_COLORS[state] ?? '#64748B';

  const signalAgeText = health?.lastSignalAt
    ? (() => {
        const secs = (Date.now() - new Date(health.lastSignalAt).getTime()) / 1000;
        if (secs < 5) return 'now';
        if (secs < 60) return `${Math.round(secs)}s`;
        return `${Math.round(secs / 60)}m`;
      })()
    : '—';

  return (
    <Card
      variant="outlined"
      sx={{
        borderColor: alpha(color, 0.3),
        transition: 'border-color 0.3s, box-shadow 0.3s',
        '&:hover': { borderColor: color, boxShadow: `0 0 0 2px ${alpha(color, 0.15)}` },
      }}
    >
      <CardActionArea onClick={() => navigate(`/iot/machines/${machine.machineCode}/signals`)}>
        <CardContent sx={{ p: 2, pb: '12px !important' }}>
          {/* Header row */}
          <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center', minWidth: 0 }}>
              <StateDot state={state} />
              <Typography variant="body2" sx={{ fontWeight: 700 }} noWrap>{machine.machineCode}</Typography>
            </Stack>
            <Chip
              label={state}
              size="small"
              sx={{
                height: 18, fontSize: '0.625rem', fontWeight: 700, flexShrink: 0,
                bgcolor: alpha(color, 0.1), color, border: 'none',
                '& .MuiChip-label': { px: 0.75 },
              }}
            />
          </Stack>

          <Typography variant="caption" color="text.secondary" noWrap sx={{ display: 'block', mb: 1.5 }}>
            {machine.machineName}
          </Typography>

          {/* Signal rate + last signal row */}
          <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="caption" color="text.secondary">
              Last signal: <strong>{signalAgeText}</strong>
            </Typography>
            {health && (
              <Typography variant="caption" color="text.secondary">
                {Number(health.signalRate1min ?? 0).toFixed(0)}/min
              </Typography>
            )}
          </Stack>

          {/* Adapter health dot */}
          {health && (
            <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', mt: 0.75 }}>
              <Box sx={{
                width: 6, height: 6, borderRadius: '50%',
                bgcolor: health.status === 'Connected' ? '#15803D'
                  : health.status === 'Degraded' ? '#D97706'
                  : health.status === 'Disconnected' ? '#DC2626'
                  : '#64748B',
              }} />
              <Typography variant="caption" color="text.disabled">{health.adapterType}</Typography>
            </Stack>
          )}
        </CardContent>
      </CardActionArea>
    </Card>
  );
}

export default function MachineFleetsPage() {
  const [search, setSearch] = useState('');
  const [wcFilter, setWcFilter] = useState('');
  const [stateFilter, setStateFilter] = useState('');

  const { data: machines = [], isLoading: loadingMachines, error: machinesError } = useGetApiV1Machines({ activeOnly: false });
  const { data: workCenters = [] } = useGetApiV1WorkCenters();
  const { data: snapshots = [], refetch: refetchStates } = useQuery({
    queryKey: getGetApiV1IotMachinesStatesQueryKey(),
    queryFn: () => getApiV1IotMachinesStates(),
    refetchInterval: 30_000,
  });
  const { data: healthList = [] } = useQuery({
    queryKey: ['adapters-health'],
    queryFn: () => getApiV1IotAdaptersHealth(),
    refetchInterval: 30_000,
  });

  // Build lookup maps
  const snapshotMap = new Map(snapshots.map((s) => [s.machineCode, s]));
  const healthMap = new Map(healthList.map((h) => [h.machineCode, h]));

  const filtered = machines.filter((m) => {
    if (search && !m.machineCode.toLowerCase().includes(search.toLowerCase()) &&
        !m.machineName.toLowerCase().includes(search.toLowerCase())) return false;
    if (wcFilter && m.workCenterName !== wcFilter) return false;
    if (stateFilter) {
      const snap = snapshotMap.get(m.machineCode);
      const state = snap?.currentState ?? 'UNKNOWN';
      if (state !== stateFilter) return false;
    }
    return true;
  });

  if (loadingMachines) return <TablePageSkeleton />;
  if (machinesError) return (
    <PageRoot>
      <PageHeader title="Machine Fleet" breadcrumbs={[{ label: 'IoT' }, { label: 'Fleet' }]} />
      <EmptyState icon="emptyTable" title="Failed to load machines" description={getErrorMessage(machinesError)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Machine Fleet"
        subtitle="Live state overview — click a card to open the signal monitor"
        breadcrumbs={[{ label: 'IoT' }, { label: 'Fleet' }]}
        actions={<RefreshButton onClick={() => { void refetchStates(); }} />}
      />

      <Stack direction="row" spacing={1.5} sx={{ mb: 2, alignItems: 'center', flexWrap: 'wrap', gap: 1 }}>
        <TextField
          size="small"
          placeholder="Search machine…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ minWidth: 200, flex: 1, maxWidth: 320 }}
          slotProps={{ input: { startAdornment: <SolarIcon name="search" size={16} color="#94A3B8" /> } }}
        />
        <Select
          value={wcFilter}
          onChange={(e) => setWcFilter(e.target.value)}
          size="small"
          displayEmpty
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">All Work Centers</MenuItem>
          {workCenters.map((wc) => <MenuItem key={wc.workCenterCode} value={wc.workCenterName}>{wc.workCenterName}</MenuItem>)}
        </Select>
        <Select
          value={stateFilter}
          onChange={(e) => setStateFilter(e.target.value)}
          size="small"
          displayEmpty
          sx={{ minWidth: 130 }}
        >
          <MenuItem value="">All States</MenuItem>
          {['RUNNING', 'IDLE', 'SETUP', 'DOWN', 'FAULT', 'UNKNOWN'].map((s) =>
            <MenuItem key={s} value={s}>{s}</MenuItem>)}
        </Select>
      </Stack>

      {filtered.length === 0 ? (
        <EmptyState
          icon="emptyTable"
          title="No machines found"
          description="No machines match the current filters."
        />
      ) : (
        <Box sx={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
          gap: 1.5,
          overflowY: 'auto',
          flex: 1,
        }}>
          {filtered.map((m) => (
            <MachineCard
              key={m.machineCode}
              machine={m}
              snapshot={snapshotMap.get(m.machineCode)}
              health={healthMap.get(m.machineCode)}
            />
          ))}
        </Box>
      )}
    </PageRoot>
  );
}
