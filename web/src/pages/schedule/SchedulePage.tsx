import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  MenuItem,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import type { ApexOptions } from 'apexcharts';
import ReactApexChart from 'react-apexcharts';
import { useState, useMemo } from 'react';
import {
  PageHeader,
  PageRoot,
  SolarIcon,
  StatusChip,
} from '../../components';
import type { WorkOrderStatus } from '../../theme/tokens';

// ─── Mock schedule data ───────────────────────────────────────────────────────

interface ScheduleWO {
  id: string;
  woNo: string;
  product: string;
  machine: string;
  status: WorkOrderStatus;
  plannedStart: string;  // ISO date
  plannedEnd: string;
  qty: number;
  priority: 'LOW' | 'NORMAL' | 'HIGH';
}

const d = (s: string) => new Date(s).getTime();

const SCHEDULE_DATA: ScheduleWO[] = [
  { id: '1', woNo: 'WO-0089', product: 'Frame Assembly A',     machine: 'CNC Lathe 1',    status: 'RUNNING',   plannedStart: '2026-06-09', plannedEnd: '2026-06-11', qty: 200, priority: 'HIGH'   },
  { id: '2', woNo: 'WO-0090', product: 'Panel Sub-assy B',     machine: 'Press Line A',   status: 'RUNNING',   plannedStart: '2026-06-09', plannedEnd: '2026-06-13', qty: 150, priority: 'NORMAL' },
  { id: '3', woNo: 'WO-0088', product: 'Shaft Housing C',      machine: 'CNC Lathe 1',    status: 'PAUSED',    plannedStart: '2026-06-07', plannedEnd: '2026-06-10', qty: 80,  priority: 'NORMAL' },
  { id: '4', woNo: 'WO-0091', product: 'Bracket Set D',        machine: 'Assembly St. 2', status: 'RELEASED',  plannedStart: '2026-06-11', plannedEnd: '2026-06-14', qty: 300, priority: 'NORMAL' },
  { id: '5', woNo: 'WO-0092', product: 'Motor Mount E',        machine: 'CNC Mill 2',     status: 'RUNNING',   plannedStart: '2026-06-08', plannedEnd: '2026-06-12', qty: 60,  priority: 'HIGH'   },
  { id: '6', woNo: 'WO-0093', product: 'Cover Plate F',        machine: 'Grinder 1',      status: 'RUNNING',   plannedStart: '2026-06-09', plannedEnd: '2026-06-10', qty: 120, priority: 'LOW'    },
  { id: '7', woNo: 'WO-0094', product: 'Frame Assembly A',     machine: 'CNC Lathe 1',    status: 'DRAFT',     plannedStart: '2026-06-12', plannedEnd: '2026-06-16', qty: 200, priority: 'NORMAL' },
  { id: '8', woNo: 'WO-0095', product: 'Guard Assy G',         machine: 'Welding Bay 1',  status: 'RELEASED',  plannedStart: '2026-06-10', plannedEnd: '2026-06-15', qty: 40,  priority: 'LOW'    },
  { id: '9', woNo: 'WO-0086', product: 'Shaft Housing C',      machine: 'CNC Mill 2',     status: 'COMPLETED', plannedStart: '2026-06-03', plannedEnd: '2026-06-07', qty: 80,  priority: 'NORMAL' },
  { id:'10', woNo: 'WO-0087', product: 'Panel Sub-assy B',     machine: 'Press Line A',   status: 'COMPLETED', plannedStart: '2026-06-04', plannedEnd: '2026-06-08', qty: 150, priority: 'NORMAL' },
];

// ApexCharts Gantt color map by status
const GANTT_COLORS: Record<WorkOrderStatus, string> = {
  DRAFT:     '#94A3B8',
  RELEASED:  '#60A5FA',
  RUNNING:   '#22C55E',
  PAUSED:    '#F59E0B',
  COMPLETED: '#4ADE80',
  CLOSED:    '#94A3B8',
  CANCELLED: '#F87171',
  ON_HOLD:   '#FDE68A',
};

// ─── View range options ───────────────────────────────────────────────────────

const VIEW_RANGES = [
  { label: 'This Week',   start: '2026-06-09', end: '2026-06-14' },
  { label: 'Next 2 Weeks', start: '2026-06-09', end: '2026-06-21' },
  { label: 'This Month',  start: '2026-06-01', end: '2026-06-30' },
];

// ─── Gantt chart ──────────────────────────────────────────────────────────────

function GanttChart({ data, rangeStart, rangeEnd }: { data: ScheduleWO[]; rangeStart: number; rangeEnd: number }) {
  // Group by status for series (each status = a color band)
  const statuses: WorkOrderStatus[] = ['RUNNING', 'RELEASED', 'PAUSED', 'DRAFT', 'COMPLETED'];

  const series = statuses
    .map((status) => {
      const items = data.filter((wo) => wo.status === status);
      return {
        name: status.charAt(0) + status.slice(1).toLowerCase(),
        data: items.map((wo) => ({
          x: wo.machine,
          y: [d(wo.plannedStart), d(wo.plannedEnd)] as [number, number],
          goals: [],
          fillColor: GANTT_COLORS[status],
          // Store extra data for tooltip
          extra: wo,
        })),
      };
    })
    .filter((s) => s.data.length > 0);

  const options: ApexOptions = {
    chart: {
      type: 'rangeBar',
      height: 380,
      toolbar: { show: false },
      fontFamily: '"Inter", "Segoe UI", sans-serif',
      background: 'transparent',
      animations: { enabled: true, speed: 300 },
    },
    plotOptions: {
      bar: {
        horizontal: true,
        barHeight: '55%',
        rangeBarGroupRows: true,
      },
    },
    xaxis: {
      type: 'datetime',
      min: rangeStart,
      max: rangeEnd,
      labels: {
        style: { fontSize: '11px', fontWeight: 500 },
        datetimeUTC: false,
        format: 'dd MMM',
      },
      axisBorder: { show: false },
    },
    yaxis: {
      labels: {
        style: { fontSize: '12px', fontWeight: 500 },
        maxWidth: 140,
      },
    },
    grid: {
      borderColor: 'rgba(4,74,66,0.08)',
      xaxis: { lines: { show: true } },
      yaxis: { lines: { show: false } },
    },
    colors: statuses.map((s) => GANTT_COLORS[s]),
    legend: {
      show: true,
      position: 'top',
      horizontalAlign: 'left',
      fontSize: '12px',
      markers: { size: 8 },
      itemMargin: { horizontal: 10 },
    },
    tooltip: {
      custom: ({ series: _s, seriesIndex: _si, dataPointIndex: _dpi, w }) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const seriesArr = w.config.series as any[];
        const data = seriesArr?.[_si]?.data?.[_dpi];
        if (!data?.extra) return '<div></div>';
        const wo = data.extra as ScheduleWO;
        const start = new Date(data.y[0]).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
        const end   = new Date(data.y[1]).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
        const color = GANTT_COLORS[wo.status];
        return `
          <div style="padding:10px 14px;font-family:Inter,sans-serif;font-size:12px;min-width:200px">
            <div style="font-weight:700;font-size:13px;margin-bottom:6px;color:#044A42">${wo.woNo}</div>
            <div style="color:#475569;margin-bottom:4px">${wo.product}</div>
            <div style="display:flex;align-items:center;gap:6px;margin-bottom:4px">
              <span style="background:${alpha(color, 0.15)};color:${color};padding:2px 6px;border-radius:4px;font-weight:600;font-size:11px">${wo.status}</span>
              <span style="color:#94A3B8;font-size:11px">${wo.priority} priority</span>
            </div>
            <div style="color:#64748B">${start} → ${end} · Qty: ${wo.qty}</div>
          </div>
        `;
      },
    },
  };

  return (
    <ReactApexChart
      options={options}
      series={series as ApexOptions['series']}
      type="rangeBar"
      height={380}
    />
  );
}

// ─── WO row card ──────────────────────────────────────────────────────────────

function WoRow({ wo }: { wo: ScheduleWO }) {
  const priorityColor = wo.priority === 'HIGH' ? '#DC2626' : wo.priority === 'LOW' ? '#94A3B8' : '#B45309';
  return (
    <Stack
      direction="row"
      spacing={2}
      sx={(theme) => ({
        alignItems: 'center',
        px: 2,
        py: 1.25,
        borderBottom: '1px solid',
        borderColor: 'divider',
        '&:last-child': { borderBottom: 'none' },
        '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.03) },
        cursor: 'default',
      })}
    >
      <Typography
        variant="caption"
        sx={{ fontWeight: 700, fontFamily: 'ui-monospace, monospace', color: 'primary.main', width: 80, flexShrink: 0 }}
      >
        {wo.woNo}
      </Typography>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography variant="body2" noWrap sx={{ fontWeight: 500 }}>{wo.product}</Typography>
        <Typography variant="caption" color="text.disabled">{wo.machine}</Typography>
      </Box>
      <Tooltip title={`${wo.priority} priority`}>
        <Box sx={{ width: 6, height: 6, borderRadius: '50%', bgcolor: priorityColor, flexShrink: 0 }} />
      </Tooltip>
      <StatusChip status={wo.status} />
      <Typography variant="caption" color="text.secondary" sx={{ width: 90, textAlign: 'right', flexShrink: 0 }}>
        {new Date(wo.plannedStart).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })} →{' '}
        {new Date(wo.plannedEnd).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
      </Typography>
      <Typography variant="caption" color="text.secondary" sx={{ width: 50, textAlign: 'right', flexShrink: 0 }}>
        {wo.qty} ea
      </Typography>
    </Stack>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function SchedulePage() {
  const [rangeIndex, setRangeIndex]       = useState(0);
  const [machineFilter, setMachineFilter] = useState('');
  const [statusFilter, setStatusFilter]   = useState('');

  const range = VIEW_RANGES[rangeIndex];
  const rangeStart = d(range.start);
  const rangeEnd   = d(range.end) + 86400000; // inclusive end

  const machines = [...new Set(SCHEDULE_DATA.map((w) => w.machine))].sort();

  const filtered = useMemo(() => {
    let r = SCHEDULE_DATA;
    if (machineFilter) r = r.filter((w) => w.machine === machineFilter);
    if (statusFilter)  r = r.filter((w) => w.status === statusFilter);
    return r;
  }, [machineFilter, statusFilter]);

  // Summary counts
  const running   = filtered.filter((w) => w.status === 'RUNNING').length;
  const released  = filtered.filter((w) => w.status === 'RELEASED').length;
  const completed = filtered.filter((w) => w.status === 'COMPLETED').length;
  const paused    = filtered.filter((w) => w.status === 'PAUSED').length;

  return (
    <PageRoot>
      <PageHeader
        title="Production Schedule"
        subtitle="Gantt view of work orders by machine across planned dates"
        breadcrumbs={[{ label: 'Production' }, { label: 'Schedule' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
          >
            Schedule WO
          </Button>
        }
      />

      {/* Controls */}
      <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center', flexWrap: 'wrap', mb: 2.5, gap: 1 }}>
        {/* Range buttons */}
        <Stack direction="row" spacing={0.5}>
          {VIEW_RANGES.map((r, i) => (
            <Button
              key={r.label}
              size="small"
              variant={rangeIndex === i ? 'contained' : 'outlined'}
              onClick={() => setRangeIndex(i)}
              sx={{ minWidth: 0, px: 1.5 }}
            >
              {r.label}
            </Button>
          ))}
        </Stack>

        <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />

        <TextField
          select
          size="small"
          label="Machine"
          value={machineFilter}
          onChange={(e) => setMachineFilter(e.target.value)}
          sx={{ width: 160 }}
        >
          <MenuItem value="">All machines</MenuItem>
          {machines.map((m) => <MenuItem key={m} value={m}>{m}</MenuItem>)}
        </TextField>

        <TextField
          select
          size="small"
          label="Status"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          sx={{ width: 140 }}
        >
          <MenuItem value="">All statuses</MenuItem>
          {(['RUNNING', 'RELEASED', 'PAUSED', 'DRAFT', 'COMPLETED'] as WorkOrderStatus[]).map((s) => (
            <MenuItem key={s} value={s}>{s}</MenuItem>
          ))}
        </TextField>

        <Box sx={{ flex: 1 }} />

        {/* Status summary chips */}
        <Stack direction="row" spacing={0.75} sx={{ flexWrap: 'wrap' }}>
          {[
            { label: `${running} Running`,   color: '#22C55E' },
            { label: `${released} Released`, color: '#60A5FA' },
            { label: `${paused} Paused`,     color: '#F59E0B' },
            { label: `${completed} Done`,    color: '#94A3B8' },
          ].map(({ label, color }) => (
            <Chip
              key={label}
              label={label}
              size="small"
              sx={{
                height: 22,
                fontSize: '0.6875rem',
                fontWeight: 600,
                bgcolor: alpha(color, 0.1),
                color,
                border: 'none',
              }}
            />
          ))}
        </Stack>
      </Stack>

      {/* Gantt chart */}
      <Card sx={{ mb: 2.5 }}>
        <CardContent sx={{ p: '20px !important', pb: '8px !important' }}>
          <GanttChart
            data={filtered}
            rangeStart={rangeStart}
            rangeEnd={rangeEnd}
          />
        </CardContent>
      </Card>

      {/* WO list table */}
      <Card>
        <CardContent sx={{ p: '0 !important' }}>
          {/* Header */}
          <Stack
            direction="row"
            spacing={2}
            sx={(theme) => ({
              alignItems: 'center',
              px: 2,
              py: 1,
              bgcolor: alpha(theme.palette.primary.main, 0.04),
              borderBottom: '1px solid',
              borderColor: 'divider',
            })}
          >
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 80 }}>WO No.</Typography>
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', flex: 1 }}>Product · Machine</Typography>
            <Box sx={{ width: 6 }} />
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 80 }}>Status</Typography>
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 90, textAlign: 'right' }}>Period</Typography>
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 50, textAlign: 'right' }}>Qty</Typography>
          </Stack>

          {filtered.length === 0 ? (
            <Box sx={{ py: 6, textAlign: 'center' }}>
              <Typography variant="body2" color="text.secondary">No work orders match the selected filters.</Typography>
            </Box>
          ) : (
            filtered
              .slice()
              .sort((a, b) => d(a.plannedStart) - d(b.plannedStart))
              .map((wo) => <WoRow key={wo.id} wo={wo} />)
          )}
        </CardContent>
      </Card>
    </PageRoot>
  );
}
