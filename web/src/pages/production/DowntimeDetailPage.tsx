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
} from '../../components';

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_DOWNTIME_DETAIL = {
  id: '1',
  eventNo: 'DT-0089',
  machine: 'CNC Lathe 1',
  machineId: 'MC-01',
  workCenter: 'CNC Turning Cell',
  category: 'Breakdown' as const,
  reason: 'Spindle bearing failure',
  rootCause: 'Progressive wear of spindle bearing due to insufficient lubrication during high-speed operation. Bearing surface showed pitting and heat discoloration on inspection.',
  startedAt: '2026-06-12 06:45',
  endedAt: '2026-06-12 08:20',
  duration: '1h 35m',
  reportedBy: 'Nguyen Van A',
  resolvedBy: 'Le Van C (Maintenance)',
  status: 'Resolved' as 'Active' | 'Resolved',
  lastPm: '2026-05-15',
  lostOutput: 24,
};

interface RepairAction {
  time: string;
  action: string;
  technician: string;
}

const MOCK_REPAIR_ACTIONS: RepairAction[] = [
  { time: '2026-06-12 07:00', action: 'Machine isolated and locked out. Initial diagnosis: spindle bearing. Ordered replacement bearing from stores.',           technician: 'Le Van C' },
  { time: '2026-06-12 07:45', action: 'Old bearing removed. Shaft inspected — no secondary damage. New bearing (SKF 6308-2RS) installed and torqued to spec.', technician: 'Le Van C' },
  { time: '2026-06-12 08:10', action: 'Trial run at 500 rpm — 2 min. Vibration and temperature nominal. Lock-out removed, machine returned to production.',      technician: 'Le Van C' },
];

const CATEGORY_COLORS: Record<string, { color: string; bg: string; border: string }> = {
  Breakdown:    { color: '#DC2626', bg: alpha('#DC2626', 0.1), border: alpha('#DC2626', 0.3) },
  Planned:      { color: '#1D4ED8', bg: alpha('#1D4ED8', 0.1), border: alpha('#1D4ED8', 0.3) },
  Setup:        { color: '#D97706', bg: alpha('#D97706', 0.1), border: alpha('#D97706', 0.3) },
  'Minor Stop': { color: '#EA580C', bg: alpha('#EA580C', 0.1), border: alpha('#EA580C', 0.3) },
};

const DOWNTIME_BREAKDOWN = [
  { label: 'Breakdown',    minutes: 95,  color: '#DC2626' },
  { label: 'Planned',      minutes: 120, color: '#1D4ED8' },
  { label: 'Setup',        minutes: 45,  color: '#D97706' },
  { label: 'Minor Stop',   minutes: 30,  color: '#EA580C' },
];
const totalMinutes = DOWNTIME_BREAKDOWN.reduce((s, d) => s + d.minutes, 0);

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function DowntimeDetailPage() {
  const { id: _id } = useParams<{ id: string }>();
  const dt = MOCK_DOWNTIME_DETAIL;
  const cc = CATEGORY_COLORS[dt.category];
  const isActive = dt.status === 'Active';

  return (
    <PageRoot>
      <PageHeader
        title={`Downtime #${dt.eventNo}`}
        subtitle={`${dt.machineId} ${dt.machine} · ${dt.category}`}
        breadcrumbs={[
          { label: 'Production', href: '/production' },
          { label: 'Downtime', href: '/production/downtime' },
          { label: dt.eventNo },
        ]}
        actions={
          <Button
            variant="contained"
            size="small"
            color={isActive ? 'success' : 'inherit'}
            startIcon={<SolarIcon name={isActive ? 'complete' : 'success'} size={16} />}
            disabled={!isActive}
          >
            {isActive ? 'Resolve Incident' : 'Resolved'}
          </Button>
        }
      />

      <Grid container spacing={2.5}>
        {/* Left column */}
        <Grid size={{ xs: 12, md: 7 }}>
          {/* Incident details */}
          <Card sx={{ mb: 2.5 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Incident Details</Typography>
              <Grid container spacing={2}>
                {[
                  { label: 'Machine',      value: <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>{dt.machineId} — {dt.machine}</Typography> },
                  { label: 'Category',     value: <Chip label={dt.category} size="small" sx={{ height: 22, fontSize: '0.6875rem', fontWeight: 600, bgcolor: cc.bg, color: cc.color, border: `1px solid ${cc.border}`, '& .MuiChip-label': { px: 1 } }} /> },
                  { label: 'Reason',       value: dt.reason },
                  { label: 'Status',       value: (
                    <Chip
                      label={dt.status}
                      size="small"
                      sx={{
                        height: 22,
                        fontSize: '0.6875rem',
                        fontWeight: 600,
                        bgcolor: isActive ? alpha('#DC2626', 0.1) : alpha('#15803D', 0.1),
                        color: isActive ? '#DC2626' : '#15803D',
                        border: `1px solid ${isActive ? alpha('#DC2626', 0.3) : alpha('#15803D', 0.3)}`,
                        '& .MuiChip-label': { px: 1 },
                      }}
                    />
                  )},
                  { label: 'Started At',   value: dt.startedAt },
                  { label: 'Ended At',     value: dt.endedAt ?? '—' },
                  { label: 'Duration',     value: dt.duration ?? '—' },
                  { label: 'Reported By',  value: dt.reportedBy },
                  { label: 'Resolved By',  value: dt.resolvedBy ?? '—' },
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
                <Grid size={{ xs: 12 }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', letterSpacing: '0.05em', fontWeight: 600 }}>
                    Root Cause Analysis
                  </Typography>
                  <Typography variant="body2" sx={{ mt: 0.5, lineHeight: 1.6, color: 'text.secondary' }}>{dt.rootCause}</Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>

          {/* Repair actions */}
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Repair Actions</Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    {['Time', 'Action Taken', 'Technician'].map((h) => (
                      <TableCell key={h} sx={{ py: 0.75, fontWeight: 700, fontSize: '0.75rem', color: 'text.secondary', borderColor: 'divider' }}>{h}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {MOCK_REPAIR_ACTIONS.map((a, i) => (
                    <TableRow key={i} hover>
                      <TableCell sx={{ py: 0.75, borderColor: 'divider', verticalAlign: 'top', whiteSpace: 'nowrap' }}>
                        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace' }}>{a.time}</Typography>
                      </TableCell>
                      <TableCell sx={{ py: 0.75, borderColor: 'divider', verticalAlign: 'top' }}>
                        <Typography variant="body2" sx={{ fontSize: 12, lineHeight: 1.5 }}>{a.action}</Typography>
                      </TableCell>
                      <TableCell sx={{ py: 0.75, borderColor: 'divider', verticalAlign: 'top', whiteSpace: 'nowrap' }}>
                        <Typography variant="body2" sx={{ fontSize: 12 }}>{a.technician}</Typography>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>

        {/* Right column */}
        <Grid size={{ xs: 12, md: 5 }}>
          {/* Machine info */}
          <Card sx={{ mb: 2.5 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Machine Info</Typography>
              <Stack spacing={1.5}>
                {[
                  { label: 'Machine ID',    value: dt.machineId },
                  { label: 'Machine Name',  value: dt.machine },
                  { label: 'Work Center',   value: dt.workCenter },
                  { label: 'Last PM Date',  value: dt.lastPm },
                ].map(({ label, value }) => (
                  <Stack key={label} direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>{label}</Typography>
                    <Typography variant="body2" sx={{ fontWeight: 500, fontFamily: label === 'Machine ID' ? 'ui-monospace, monospace' : undefined }}>{value}</Typography>
                  </Stack>
                ))}
              </Stack>
            </CardContent>
          </Card>

          {/* Downtime impact */}
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Downtime Impact</Typography>
              <Stack spacing={1} sx={{ mb: 2.5 }}>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Lost Time</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: 'error.main' }}>{dt.duration}</Typography>
                </Stack>
                <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
                  <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Est. Lost Output</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 700 }}>~{dt.lostOutput} EA</Typography>
                </Stack>
              </Stack>

              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600, display: 'block', mb: 1.5 }}>
                Category Breakdown (this month)
              </Typography>
              <Stack spacing={1.25}>
                {DOWNTIME_BREAKDOWN.map((d) => (
                  <Stack key={d.label} spacing={0.5}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                      <Typography variant="caption" sx={{ fontSize: 11 }}>{d.label}</Typography>
                      <Typography variant="caption" sx={{ fontWeight: 600, color: d.color }}>{d.minutes}m</Typography>
                    </Stack>
                    <Box sx={{ height: 6, borderRadius: 3, bgcolor: (t) => alpha(t.palette.divider, 0.5), overflow: 'hidden' }}>
                      <Box sx={{ height: '100%', width: `${(d.minutes / totalMinutes) * 100}%`, bgcolor: d.color, borderRadius: 3 }} />
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
