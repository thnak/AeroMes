import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useParams } from 'react-router-dom';
import {
  PageHeader,
  PageRoot,
  SolarIcon,
  StatusDot,
} from '../../components';

// ─── Mock data ────────────────────────────────────────────────────────────────

interface ProductionLogEntry {
  time: string;
  qtyOk: number;
  qtyNg: number;
  note: string;
}

const MOCK_JOB = {
  id: '1',
  no: 'JOB-4421',
  woNo: 'WO-2026-0089',
  productCode: 'FRM-A001',
  productName: 'Frame Assembly A',
  machine: 'MC-01',
  machineName: 'CNC Lathe 1',
  operator: 'Nguyen Van A',
  status: 'RUNNING' as 'QUEUED' | 'RUNNING' | 'PAUSED' | 'COMPLETED' | 'CANCELLED',
  startedAt: '2026-06-12 07:30',
  completedAt: undefined as string | undefined,
  targetQty: 100,
  actualQty: 78,
  defects: 2,
  machineStatus: 'RUNNING' as 'RUNNING' | 'IDLE' | 'SETUP' | 'DOWN' | 'OFFLINE',
  machineOee: 74.2,
  machineUptime: '6h 12m',
  workCenter: 'CNC Turning Cell',
  lastPm: '2026-05-15',
};

const MOCK_LOG: ProductionLogEntry[] = [
  { time: '2026-06-12 07:45', qtyOk: 12, qtyNg: 0, note: 'First batch — nominal cycle time' },
  { time: '2026-06-12 08:30', qtyOk: 15, qtyNg: 1, note: 'NG: surface scratch on P/N 2' },
  { time: '2026-06-12 09:15', qtyOk: 14, qtyNg: 0, note: 'Coolant adjusted — better finish' },
  { time: '2026-06-12 10:00', qtyOk: 16, qtyNg: 1, note: 'NG: dimension out-of-spec on P/N 1' },
  { time: '2026-06-12 11:00', qtyOk: 15, qtyNg: 0, note: 'Tool change at cycle 78 — OK' },
  { time: '2026-06-12 11:42', qtyOk: 6,  qtyNg: 0, note: 'Partial — shift break' },
];

const DEFECT_BREAKDOWN = [
  { type: 'Scratch',   count: 1, color: '#DC2626' },
  { type: 'Dimension', count: 1, color: '#D97706' },
  { type: 'Cosmetic',  count: 0, color: '#94A3B8' },
];

const JOB_STATUS_COLORS: Record<string, { color: string; bg: string; border: string }> = {
  QUEUED:    { color: '#475569', bg: alpha('#475569', 0.1), border: alpha('#475569', 0.3) },
  RUNNING:   { color: '#15803D', bg: alpha('#15803D', 0.1), border: alpha('#15803D', 0.3) },
  PAUSED:    { color: '#B45309', bg: alpha('#B45309', 0.1), border: alpha('#B45309', 0.3) },
  COMPLETED: { color: '#166534', bg: alpha('#166534', 0.1), border: alpha('#166534', 0.3) },
  CANCELLED: { color: '#B91C1C', bg: alpha('#B91C1C', 0.1), border: alpha('#B91C1C', 0.3) },
};

const MACHINE_STATUS_COLOR: Record<string, string> = {
  RUNNING: '#15803D',
  IDLE:    '#94A3B8',
  SETUP:   '#1D4ED8',
  DOWN:    '#B91C1C',
};

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function JobDetailPage() {
  const { id: _id } = useParams<{ id: string }>();
  const job = MOCK_JOB;

  const sc = JOB_STATUS_COLORS[job.status];
  const progress = job.targetQty > 0 ? Math.round((job.actualQty / job.targetQty) * 100) : 0;
  const isPaused = job.status === 'PAUSED';

  return (
    <PageRoot>
      <PageHeader
        title={job.no}
        subtitle={`${job.woNo} · ${job.machineName} · ${job.operator}`}
        breadcrumbs={[
          { label: 'Production', href: '/production' },
          { label: 'Jobs', href: '/production/jobs' },
          { label: job.no },
        ]}
        actions={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              size="small"
              startIcon={<SolarIcon name={isPaused ? 'start' : 'pause'} size={16} />}
              disabled={job.status === 'COMPLETED' || job.status === 'CANCELLED'}
            >
              {isPaused ? 'Resume' : 'Pause'}
            </Button>
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="complete" size={16} />}
              disabled={job.status === 'COMPLETED' || job.status === 'CANCELLED'}
            >
              Complete
            </Button>
          </Stack>
        }
      />

      <Grid container spacing={2.5}>
        {/* Left column */}
        <Grid size={{ xs: 12, md: 8 }}>
          {/* Job info card */}
          <Card sx={{ mb: 2.5 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Job Info</Typography>
              <Grid container spacing={2}>
                {[
                  { label: 'Status',       value: <Chip label={job.status} size="small" sx={{ height: 22, fontSize: '0.6875rem', fontWeight: 600, bgcolor: sc.bg, color: sc.color, border: `1px solid ${sc.border}`, '& .MuiChip-label': { px: 1 } }} /> },
                  { label: 'Work Order',   value: <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600, color: 'primary.main' }}>{job.woNo}</Typography> },
                  { label: 'Machine',      value: <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>{job.machine} — {job.machineName}</Typography> },
                  { label: 'Operator',     value: job.operator },
                  { label: 'Target Qty',   value: job.targetQty.toLocaleString() },
                  { label: 'Actual Qty',   value: `${job.actualQty.toLocaleString()} (${progress}%)` },
                  { label: 'Defects',      value: <Typography variant="body2" sx={{ fontWeight: 700, color: job.defects > 0 ? 'error.main' : 'text.primary' }}>{job.defects}</Typography> },
                  { label: 'Started At',   value: job.startedAt },
                  { label: 'Completed At', value: job.completedAt ?? '—' },
                ].map(({ label, value }) => (
                  <Grid key={label} size={{ xs: 6 }}>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', letterSpacing: '0.05em', fontWeight: 600 }}>
                      {label}
                    </Typography>
                    <Box sx={{ mt: 0.25 }}>
                      {typeof value === 'string' ? (
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>{value}</Typography>
                      ) : value}
                    </Box>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>

          {/* Production log */}
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Production Log</Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    {['Time', 'Qty OK', 'Qty NG', 'Operator Note'].map((h) => (
                      <TableCell key={h} sx={{ py: 0.75, fontWeight: 700, fontSize: '0.75rem', color: 'text.secondary', borderColor: 'divider' }}>{h}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {MOCK_LOG.map((entry, i) => (
                    <TableRow key={i} hover>
                      <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                        <Typography variant="body2" sx={{ fontSize: 12, fontFamily: 'ui-monospace, monospace' }}>{entry.time}</Typography>
                      </TableCell>
                      <TableCell sx={{ py: 0.75, borderColor: 'divider' }} align="right">
                        <Typography variant="body2" sx={{ fontWeight: 700, color: 'success.main' }}>{entry.qtyOk}</Typography>
                      </TableCell>
                      <TableCell sx={{ py: 0.75, borderColor: 'divider' }} align="right">
                        <Typography variant="body2" sx={{ fontWeight: 700, color: entry.qtyNg > 0 ? 'error.main' : 'text.disabled' }}>{entry.qtyNg > 0 ? entry.qtyNg : '—'}</Typography>
                      </TableCell>
                      <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                        <Typography variant="body2" sx={{ fontSize: 12 }}>{entry.note}</Typography>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>

        {/* Right column */}
        <Grid size={{ xs: 12, md: 4 }}>
          {/* Machine card */}
          <Card sx={{ mb: 2.5 }}>
            <CardContent sx={{ p: 3 }}>
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 2 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>Machine</Typography>
              </Stack>
              <Stack spacing={1.5}>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>ID</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>{job.machine}</Typography>
                </Stack>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Name</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>{job.machineName}</Typography>
                </Stack>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Status</Typography>
                  <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                    <StatusDot color={MACHINE_STATUS_COLOR[job.machineStatus]} size={8} pulse={job.machineStatus === 'RUNNING'} />
                    <Typography variant="body2" sx={{ fontWeight: 600, color: MACHINE_STATUS_COLOR[job.machineStatus] }}>{job.machineStatus}</Typography>
                  </Stack>
                </Stack>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>OEE</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#D97706' }}>{job.machineOee}%</Typography>
                </Stack>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Uptime (shift)</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>{job.machineUptime}</Typography>
                </Stack>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Work Center</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>{job.workCenter}</Typography>
                </Stack>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Last PM</Typography>
                  <Typography variant="body2" color="text.secondary">{job.lastPm}</Typography>
                </Stack>
              </Stack>
            </CardContent>
          </Card>

          {/* Defects card */}
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Defects ({job.defects} total)</Typography>
              <Stack spacing={1.5}>
                {DEFECT_BREAKDOWN.map((d) => (
                  <Stack key={d.type} direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
                    <Box
                      sx={{
                        width: 28,
                        height: 28,
                        borderRadius: 1.5,
                        bgcolor: alpha(d.color, 0.1),
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        flexShrink: 0,
                      }}
                    >
                      <Typography variant="caption" sx={{ fontWeight: 700, color: d.color, fontSize: 11 }}>
                        {d.count}
                      </Typography>
                    </Box>
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 500 }}>{d.type}</Typography>
                      <Box
                        sx={{
                          mt: 0.5,
                          height: 4,
                          borderRadius: 2,
                          bgcolor: (t) => alpha(t.palette.divider, 0.5),
                          overflow: 'hidden',
                        }}
                      >
                        <Box
                          sx={{
                            height: '100%',
                            width: `${job.defects > 0 ? (d.count / job.defects) * 100 : 0}%`,
                            bgcolor: d.color,
                            borderRadius: 2,
                          }}
                        />
                      </Box>
                    </Box>
                  </Stack>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
