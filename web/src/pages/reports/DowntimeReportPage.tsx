import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Grid,
  LinearProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import {
  ExportButton,
  KpiCard,
  PageHeader,
  PageRoot,
} from '../../components';

// ─── Mock data ────────────────────────────────────────────────────────────────

interface DowntimeCategory {
  category: string;
  hours: number;
  pct: number;
  color: string;
}

const MOCK_CATEGORIES: DowntimeCategory[] = [
  { category: 'Breakdown',           hours: 38.0, pct: 45, color: '#B91C1C' },
  { category: 'Planned Maintenance', hours: 22.0, pct: 26, color: '#1D4ED8' },
  { category: 'Setup / Changeover',  hours: 14.2, pct: 17, color: '#D97706' },
  { category: 'Minor Stop',          hours: 10.0, pct: 12, color: '#94A3B8' },
];

interface MachineDowntime {
  machineCode: string;
  machineName: string;
  hours: number;
}

const MOCK_MACHINE_DOWNTIME: MachineDowntime[] = [
  { machineCode: 'MC-04', machineName: 'CNC Mill 2',       hours: 18.4 },
  { machineCode: 'MC-06', machineName: 'Welding Robot 1',  hours: 14.7 },
  { machineCode: 'MC-01', machineName: 'CNC Lathe 1',      hours: 12.1 },
  { machineCode: 'MC-02', machineName: 'CNC Mill 1',       hours: 10.8 },
  { machineCode: 'MC-05', machineName: 'Press Machine 1',  hours: 8.6  },
  { machineCode: 'MC-03', machineName: 'Assembly Line 1',  hours: 5.0  },
];

const MAX_MACHINE_HOURS = Math.max(...MOCK_MACHINE_DOWNTIME.map((m) => m.hours));

type DtCategory = 'Breakdown' | 'Planned Maintenance' | 'Setup / Changeover' | 'Minor Stop';

interface DowntimeEvent {
  machine: string;
  category: DtCategory;
  reason: string;
  start: string;
  durationMin: number;
  resolvedBy: string;
  rootCause: string;
}

const MOCK_DOWNTIME_EVENTS: DowntimeEvent[] = [
  { machine: 'MC-04 CNC Mill 2',      category: 'Breakdown',           reason: 'Spindle bearing failure',       start: '2026-06-11 02:14', durationMin: 148, resolvedBy: 'Maintenance Team A', rootCause: 'Wear — scheduled replacement overdue'   },
  { machine: 'MC-06 Welding Robot 1', category: 'Breakdown',           reason: 'Wire feed jam',                 start: '2026-06-10 14:32', durationMin: 82,  resolvedBy: 'Operator B',         rootCause: 'Poor quality consumable wire spool'     },
  { machine: 'MC-01 CNC Lathe 1',     category: 'Planned Maintenance', reason: 'Weekly PM — lubrication',       start: '2026-06-09 07:00', durationMin: 60,  resolvedBy: 'Maintenance Team B', rootCause: 'Scheduled'                               },
  { machine: 'MC-02 CNC Mill 1',      category: 'Setup / Changeover',  reason: 'Tool change — product switch',  start: '2026-06-09 13:10', durationMin: 45,  resolvedBy: 'Operator C',         rootCause: 'Product changeover'                     },
  { machine: 'MC-04 CNC Mill 2',      category: 'Minor Stop',          reason: 'Coolant level low',             start: '2026-06-08 09:55', durationMin: 18,  resolvedBy: 'Operator D',         rootCause: 'Top-up required — sensor alert worked'  },
  { machine: 'MC-05 Press Machine 1', category: 'Planned Maintenance', reason: 'Die change — preventive check', start: '2026-06-08 07:00', durationMin: 90,  resolvedBy: 'Maintenance Team A', rootCause: 'Scheduled'                               },
  { machine: 'MC-01 CNC Lathe 1',     category: 'Breakdown',           reason: 'Chuck hydraulic leak',          start: '2026-06-07 16:40', durationMin: 210, resolvedBy: 'Maintenance Team B', rootCause: 'O-ring seal degradation'                },
  { machine: 'MC-03 Assembly Line 1', category: 'Minor Stop',          reason: 'Parts feeder jam',              start: '2026-06-07 10:20', durationMin: 22,  resolvedBy: 'Operator E',         rootCause: 'Burr on component — supplier quality'   },
];

const CATEGORY_COLOR: Record<DtCategory, string> = {
  'Breakdown':           '#B91C1C',
  'Planned Maintenance': '#1D4ED8',
  'Setup / Changeover':  '#D97706',
  'Minor Stop':          '#94A3B8',
};

// ─── Component ────────────────────────────────────────────────────────────────

export default function DowntimeReportPage() {
  return (
    <PageRoot>
      <PageHeader
        title="Downtime Report"
        subtitle="Machine downtime analysis and Pareto"
        breadcrumbs={[{ label: 'Reports' }, { label: 'Downtime' }]}
        actions={
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <TextField
              type="date"
              size="small"
              label="From"
              defaultValue="2026-05-01"
              sx={{ width: 150 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              type="date"
              size="small"
              label="To"
              defaultValue="2026-06-11"
              sx={{ width: 150 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <ExportButton />
          </Stack>
        }
      />

      {/* ── KPI row ──────────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Total Downtime"
            value="84.2"
            unit="h"
            icon="machineDown"
            accentColor="#B91C1C"
            trend={-6.2}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="MTBF"
            value="48.3"
            unit="h"
            icon="success"
            accentColor="#15803D"
            trend={4.1}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="MTTR"
            value="2.1"
            unit="h"
            icon="warning"
            accentColor="#D97706"
            trend={-0.3}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Top Machine"
            value="MC-04"
            unit="18.4h"
            icon="oee"
            accentColor="#7C3AED"
          />
        </Grid>
      </Grid>

      {/* ── Charts row ───────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Downtime by Category"
              subheader="Pareto"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
              subheaderTypographyProps={{ variant: 'caption' }}
            />
            <CardContent>
              <Stack spacing={1.5}>
                {MOCK_CATEGORIES.map((c) => (
                  <Box key={c.category}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="caption" sx={{ fontWeight: 500 }}>{c.category}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {c.hours}h · {c.pct}%
                      </Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={c.pct}
                      sx={{
                        height: 8,
                        borderRadius: 3,
                        bgcolor: alpha(c.color, 0.15),
                        '& .MuiLinearProgress-bar': { bgcolor: c.color },
                      }}
                    />
                  </Box>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Downtime by Machine"
              subheader="Top 6"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
              subheaderTypographyProps={{ variant: 'caption' }}
            />
            <CardContent>
              <Stack spacing={1.5}>
                {MOCK_MACHINE_DOWNTIME.map((m) => (
                  <Box key={m.machineCode}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="caption" sx={{ fontWeight: 500 }}>{m.machineName}</Typography>
                      <Typography variant="caption" color="text.secondary">{m.hours}h</Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={(m.hours / MAX_MACHINE_HOURS) * 100}
                      sx={{
                        height: 8,
                        borderRadius: 3,
                        bgcolor: (t) => alpha(t.palette.error.main, 0.12),
                        '& .MuiLinearProgress-bar': { bgcolor: 'error.main' },
                      }}
                    />
                  </Box>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* ── Events table ─────────────────────────────────────────────── */}
      <Card>
        <CardHeader
          title="Downtime Events"
          titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
        />
        <CardContent sx={{ p: '0 !important' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                  <TableCell>Machine</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Reason</TableCell>
                  <TableCell>Start</TableCell>
                  <TableCell align="right">Duration (min)</TableCell>
                  <TableCell>Resolved By</TableCell>
                  <TableCell>Root Cause</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {MOCK_DOWNTIME_EVENTS.map((row, i) => {
                  const color = CATEGORY_COLOR[row.category] ?? '#94A3B8';
                  return (
                    <TableRow key={i} hover>
                      <TableCell>
                        <Typography variant="caption" sx={{ fontWeight: 500 }}>{row.machine}</Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={row.category}
                          size="small"
                          sx={{ bgcolor: alpha(color, 0.12), color, fontWeight: 600, fontSize: 11 }}
                        />
                      </TableCell>
                      <TableCell>{row.reason}</TableCell>
                      <TableCell>
                        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace' }}>
                          {row.start}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <Typography variant="body2" sx={{ fontWeight: 600, color: row.durationMin >= 120 ? 'error.main' : 'text.primary' }}>
                          {row.durationMin}
                        </Typography>
                      </TableCell>
                      <TableCell>{row.resolvedBy}</TableCell>
                      <TableCell>
                        <Typography variant="caption" color="text.secondary">{row.rootCause}</Typography>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </Box>
        </CardContent>
      </Card>
    </PageRoot>
  );
}
