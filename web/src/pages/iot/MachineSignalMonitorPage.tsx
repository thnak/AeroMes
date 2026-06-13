import {
  Box,
  Chip,
  Divider,
  Paper,
  Stack,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState, useCallback, useRef, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import ReactApexChart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { PageHeader, PageRoot, TablePageSkeleton } from '../../components';
import {
  useGetApiV1IotMachinesCodeState,
  useGetApiV1IotSignalsLive,
  useGetApiV1IotSignalsHistory,
  useGetApiV1IotMachinesCodeStateHistory,
  getGetApiV1IotMachinesCodeStateQueryKey,
} from '../../api/iot/iot';
import type { LiveSignalDto, SignalHistoryPoint, MachineStateHistoryDto } from '../../api/model';
import { useIotHub, type MachineSignalUpdate, type MachineStateUpdate } from '../../lib/useIotHub';
import { useQueryClient } from '@tanstack/react-query';

const STATE_COLORS: Record<string, string> = {
  RUNNING: '#15803D',
  IDLE:    '#0891B2',
  SETUP:   '#7C3AED',
  DOWN:    '#D97706',
  FAULT:   '#DC2626',
  UNKNOWN: '#64748B',
};

const HISTORY_RANGES = [
  { label: '15m', minutes: 15 },
  { label: '1h',  minutes: 60 },
  { label: '6h',  minutes: 360 },
  { label: '24h', minutes: 1440 },
];

function StateChip({ state }: { state: string }) {
  const color = STATE_COLORS[state] ?? '#64748B';
  return (
    <Chip
      label={state}
      size="small"
      sx={{
        bgcolor: alpha(color, 0.12), color,
        fontWeight: 700, fontSize: '0.7rem',
        border: `1px solid ${alpha(color, 0.3)}`,
      }}
    />
  );
}

interface LiveSignalRowProps {
  signal: LiveSignalDto;
}

function LiveSignalRow({ signal }: LiveSignalRowProps) {
  const valNum = Number(signal.value);
  return (
    <Stack
      direction="row"
      sx={{
        alignItems: 'center', justifyContent: 'space-between',
        py: 0.75, px: 1.5, borderRadius: 1,
        '&:hover': { bgcolor: 'action.hover' },
      }}
    >
      <Typography variant="body2" color="text.secondary" sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>
        {signal.tagKey}
      </Typography>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'baseline' }}>
        <Typography variant="body2" sx={{ fontWeight: 700, fontFamily: 'monospace' }}>
          {isNaN(valNum) ? String(signal.value) : valNum.toFixed(3)}
        </Typography>
        {signal.unit && (
          <Typography variant="caption" color="text.secondary">{signal.unit}</Typography>
        )}
        <Typography variant="caption" color="text.disabled" sx={{ minWidth: 56, textAlign: 'right' }}>
          {new Date(signal.timestamp).toLocaleTimeString()}
        </Typography>
      </Stack>
    </Stack>
  );
}

interface HistoryChartProps {
  machineCode: string;
  tagKey: string;
  rangeMinutes: number;
}

function HistoryChart({ machineCode, tagKey, rangeMinutes }: HistoryChartProps) {
  const now = new Date();
  const from = new Date(now.getTime() - rangeMinutes * 60 * 1000).toISOString();
  const { data: points = [], isLoading } = useGetApiV1IotSignalsHistory({
    machineCode, tagKey, from, limit: 500,
  });

  const series = [{
    name: tagKey,
    data: points.map((p: SignalHistoryPoint) => [new Date(p.timestamp).getTime(), Number(p.value)]),
  }];

  const options: ApexOptions = {
    chart: { type: 'area', toolbar: { show: false }, animations: { enabled: false }, background: 'transparent' },
    xaxis: { type: 'datetime', labels: { style: { fontSize: '10px' } } },
    yaxis: { labels: { style: { fontSize: '10px' } } },
    stroke: { width: 2, curve: 'smooth' },
    fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.3, opacityTo: 0.0 } },
    colors: ['#044A42'],
    grid: { borderColor: '#e2e8f0', strokeDashArray: 3 },
    dataLabels: { enabled: false },
    tooltip: { x: { format: 'dd MMM HH:mm:ss' } },
    theme: { mode: 'light' },
  };

  if (isLoading) return <Box sx={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}><Typography variant="caption" color="text.secondary">Loading…</Typography></Box>;
  if (points.length === 0) return <Box sx={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}><Typography variant="caption" color="text.secondary">No data in range</Typography></Box>;

  return <ReactApexChart options={options} series={series} type="area" height={200} />;
}

interface StateTimelineProps {
  history: MachineStateHistoryDto[];
}

function StateTimeline({ history }: StateTimelineProps) {
  if (history.length === 0) return (
    <Box sx={{ py: 2, textAlign: 'center' }}>
      <Typography variant="caption" color="text.secondary">No state transitions yet</Typography>
    </Box>
  );

  return (
    <Stack spacing={0}>
      {history.map((h) => {
        const color = STATE_COLORS[h.toState] ?? '#64748B';
        const dur = Number(h.durationMs);
        const durText = dur <= 0 ? '—'
          : dur < 60_000 ? `${Math.round(dur / 1000)}s`
          : dur < 3_600_000 ? `${Math.round(dur / 60_000)}m`
          : `${(dur / 3_600_000).toFixed(1)}h`;
        return (
          <Stack key={Number(h.historyId)} direction="row" sx={{ alignItems: 'center', py: 0.5, px: 1 }}>
            <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: color, flexShrink: 0, mr: 1 }} />
            <Typography variant="caption" sx={{ flex: 1 }}>
              <strong>{h.fromState}</strong> → <strong>{h.toState}</strong>
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ mr: 2 }}>{durText}</Typography>
            <Typography variant="caption" color="text.disabled">
              {new Date(h.transitionAt).toLocaleString()}
            </Typography>
          </Stack>
        );
      })}
    </Stack>
  );
}

export default function MachineSignalMonitorPage() {
  const { machineCode } = useParams<{ machineCode: string }>();
  const code = machineCode ?? '';
  const queryClient = useQueryClient();

  const [tab, setTab] = useState(0);
  const [selectedTag, setSelectedTag] = useState<string | null>(null);
  const [rangeIdx, setRangeIdx] = useState(1);

  // Server-polled live signals (fallback + initial load)
  const { data: liveSignals = [], isLoading: loadingLive } = useGetApiV1IotSignalsLive(
    { machineCode: code },
    { query: { refetchInterval: 5_000, enabled: !!code } }
  );

  // Current state
  const { data: stateSnap } = useGetApiV1IotMachinesCodeState(code, {
    query: { refetchInterval: 15_000, enabled: !!code },
  });

  // State history for the timeline
  const { data: stateHistory = [] } = useGetApiV1IotMachinesCodeStateHistory(code, { limit: 50 });

  // Local live signal map for real-time overlay
  const liveMapRef = useRef<Map<string, LiveSignalDto>>(new Map());
  const [liveMap, setLiveMap] = useState<Map<string, LiveSignalDto>>(new Map());

  // Seed liveMap from polled data initially
  useEffect(() => {
    if (liveSignals.length > 0) {
      const m = new Map(liveSignals.map((s) => [s.tagKey, s]));
      liveMapRef.current = m;
      setLiveMap(new Map(m));
      if (!selectedTag && liveSignals[0]) setSelectedTag(liveSignals[0].tagKey);
    }
  }, [liveSignals, selectedTag]);

  // SignalR callbacks
  const onSignalUpdated = useCallback((update: MachineSignalUpdate) => {
    liveMapRef.current.set(update.tagKey, {
      tagKey: update.tagKey,
      value: update.value,
      unit: update.unit,
      timestamp: update.timestamp,
    });
    setLiveMap(new Map(liveMapRef.current));
  }, []);

  const onStateChanged = useCallback((_update: MachineStateUpdate) => {
    queryClient.invalidateQueries({ queryKey: getGetApiV1IotMachinesCodeStateQueryKey(code) });
  }, [queryClient, code]);

  useIotHub(code || null, onSignalUpdated, onStateChanged);

  const displaySignals = liveMap.size > 0
    ? [...liveMap.values()].sort((a, b) => a.tagKey.localeCompare(b.tagKey))
    : liveSignals;

  const currentState = stateSnap?.currentState ?? 'UNKNOWN';

  if (loadingLive && liveSignals.length === 0) return <TablePageSkeleton />;

  const rangeMinutes = HISTORY_RANGES[rangeIdx]?.minutes ?? 60;

  return (
    <PageRoot>
      <PageHeader
        title={`Signal Monitor — ${code}`}
        subtitle={code}
        breadcrumbs={[{ label: 'IoT' }, { label: 'Fleet', href: '/iot/machines' }, { label: code }]}
        actions={<StateChip state={currentState} />}
      />

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ flex: 1, overflow: 'hidden' }}>
        {/* Left: live signals list */}
        <Paper variant="outlined" sx={{ width: { xs: '100%', md: 280 }, display: 'flex', flexDirection: 'column', overflow: 'hidden', flexShrink: 0 }}>
          <Box sx={{ px: 1.5, py: 1, borderBottom: 1, borderColor: 'divider' }}>
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5 }}>
              Live Tags ({displaySignals.length})
            </Typography>
          </Box>
          <Box sx={{ overflowY: 'auto', flex: 1, py: 0.5 }}>
            {displaySignals.length === 0 ? (
              <Box sx={{ p: 2 }}>
                <Typography variant="caption" color="text.secondary">No signals received in the past 5 min</Typography>
              </Box>
            ) : (
              displaySignals.map((s) => (
                <Box
                  key={s.tagKey}
                  onClick={() => setSelectedTag(s.tagKey)}
                  sx={{
                    cursor: 'pointer',
                    borderLeft: '3px solid',
                    borderColor: selectedTag === s.tagKey ? 'primary.main' : 'transparent',
                    bgcolor: selectedTag === s.tagKey ? alpha('#044A42', 0.06) : 'transparent',
                  }}
                >
                  <LiveSignalRow signal={s} />
                </Box>
              ))
            )}
          </Box>
        </Paper>

        {/* Right: chart + state timeline */}
        <Stack spacing={2} sx={{ flex: 1, overflow: 'hidden', minWidth: 0 }}>
          <Paper variant="outlined" sx={{ p: 1.5 }}>
            <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" sx={{ fontWeight: 600 }}>
                {selectedTag ?? '—'}
              </Typography>
              <Stack direction="row" spacing={0.5}>
                {HISTORY_RANGES.map((r, i) => (
                  <Chip
                    key={r.label}
                    label={r.label}
                    size="small"
                    onClick={() => setRangeIdx(i)}
                    sx={{
                      height: 20, fontSize: '0.65rem', cursor: 'pointer',
                      bgcolor: rangeIdx === i ? 'primary.main' : 'transparent',
                      color: rangeIdx === i ? 'primary.contrastText' : 'text.secondary',
                      border: '1px solid',
                      borderColor: rangeIdx === i ? 'primary.main' : 'divider',
                      '& .MuiChip-label': { px: 0.75 },
                    }}
                  />
                ))}
              </Stack>
            </Stack>
            {selectedTag ? (
              <HistoryChart machineCode={code} tagKey={selectedTag} rangeMinutes={rangeMinutes} />
            ) : (
              <Box sx={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <Typography variant="caption" color="text.secondary">Select a tag from the list</Typography>
              </Box>
            )}
          </Paper>

          <Paper variant="outlined" sx={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
            <Tabs value={tab} onChange={(_, v) => setTab(v as number)} sx={{ px: 1, minHeight: 36, '& .MuiTab-root': { minHeight: 36, py: 0.5, fontSize: '0.75rem' } }}>
              <Tab label="State Transitions" />
            </Tabs>
            <Divider />
            <Box sx={{ overflowY: 'auto', flex: 1, py: 0.5 }}>
              {tab === 0 && <StateTimeline history={stateHistory as MachineStateHistoryDto[]} />}
            </Box>
          </Paper>
        </Stack>
      </Stack>
    </PageRoot>
  );
}
