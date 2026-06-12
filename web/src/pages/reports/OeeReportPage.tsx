import {
  Box,
  Card,
  CardContent,
  CardHeader,
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
import { oeeZoneColor } from '../../theme/tokens';

// ─── Mock data ────────────────────────────────────────────────────────────────

interface MachineOee {
  machineCode: string;
  machineName: string;
  workCenter: string;
  availability: number;
  performance: number;
  quality: number;
  oee: number;
  trend: 'up' | 'down' | 'flat';
}

const MOCK_MACHINE_OEE: MachineOee[] = [
  { machineCode: 'MC-01', machineName: 'CNC Lathe 1',      workCenter: 'WC-TURNING',  availability: 93.2, performance: 87.4, quality: 99.1, oee: 80.8, trend: 'up'   },
  { machineCode: 'MC-02', machineName: 'CNC Mill 1',       workCenter: 'WC-MILLING',  availability: 91.5, performance: 90.2, quality: 99.8, oee: 82.4, trend: 'up'   },
  { machineCode: 'MC-03', machineName: 'Assembly Line 1',  workCenter: 'WC-ASSEMBLY', availability: 96.0, performance: 88.3, quality: 99.9, oee: 84.6, trend: 'flat' },
  { machineCode: 'MC-04', machineName: 'CNC Mill 2',       workCenter: 'WC-MILLING',  availability: 82.1, performance: 84.0, quality: 99.4, oee: 68.6, trend: 'down' },
  { machineCode: 'MC-05', machineName: 'Press Machine 1',  workCenter: 'WC-STAMPING', availability: 95.4, performance: 91.2, quality: 100.0,oee: 87.0, trend: 'up'   },
  { machineCode: 'MC-06', machineName: 'Welding Robot 1',  workCenter: 'WC-WELDING',  availability: 88.0, performance: 86.5, quality: 99.2, oee: 75.5, trend: 'down' },
];

const TREND_ICON: Record<string, string> = { up: '↑', down: '↓', flat: '→' };
const TREND_COLOR: Record<string, string> = { up: '#15803D', down: '#B91C1C', flat: '#94A3B8' };

const MAX_OEE = Math.max(...MOCK_MACHINE_OEE.map((m) => m.oee));

// ─── Component ────────────────────────────────────────────────────────────────

export default function OeeReportPage() {
  return (
    <PageRoot>
      <PageHeader
        title="OEE Report"
        subtitle="Overall Equipment Effectiveness by machine and period"
        breadcrumbs={[{ label: 'Reports' }, { label: 'OEE' }]}
        actions={
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <TextField
              type="date"
              size="small"
              label="From"
              defaultValue="2026-05-13"
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
            label="OEE"
            value="81.4"
            unit="%"
            icon="oee"
            accentColor={oeeZoneColor(81.4)}
            trend={0.9}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Availability"
            value="92.1"
            unit="%"
            icon="success"
            accentColor="#1D4ED8"
            trend={0.4}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Performance"
            value="88.7"
            unit="%"
            icon="reports"
            accentColor="#D97706"
            trend={-0.5}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Quality"
            value="99.6"
            unit="%"
            icon="shift"
            accentColor="#15803D"
            trend={0.1}
            trendLabel="vs prev period"
          />
        </Grid>
      </Grid>

      {/* ── Charts row ───────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardHeader
              title="OEE Trend"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
            />
            <CardContent>
              <Box
                sx={{
                  height: 260,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: (t) => alpha(t.palette.primary.main, 0.03),
                  borderRadius: 1,
                  border: '1px dashed',
                  borderColor: 'divider',
                }}
              >
                <Typography variant="body2" color="text.disabled">
                  Chart: OEE% trend by day · Last 30 days
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="OEE by Machine"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
            />
            <CardContent>
              <Stack spacing={1.5}>
                {MOCK_MACHINE_OEE.map((m) => {
                  const color = oeeZoneColor(m.oee);
                  return (
                    <Box key={m.machineCode}>
                      <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                        <Typography variant="caption" sx={{ fontWeight: 500 }}>{m.machineName}</Typography>
                        <Typography variant="caption" sx={{ fontWeight: 700, color }}>
                          {m.oee.toFixed(1)}%
                        </Typography>
                      </Stack>
                      <LinearProgress
                        variant="determinate"
                        value={(m.oee / MAX_OEE) * 100}
                        sx={{
                          height: 6,
                          borderRadius: 3,
                          bgcolor: alpha(color, 0.15),
                          '& .MuiLinearProgress-bar': { bgcolor: color },
                        }}
                      />
                    </Box>
                  );
                })}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* ── Machine OEE table ────────────────────────────────────────── */}
      <Card>
        <CardHeader
          title="Machine OEE Table"
          titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
        />
        <CardContent sx={{ p: '0 !important' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                  <TableCell>Machine Code</TableCell>
                  <TableCell>Machine Name</TableCell>
                  <TableCell>Work Center</TableCell>
                  <TableCell align="right">Availability %</TableCell>
                  <TableCell align="right">Performance %</TableCell>
                  <TableCell align="right">Quality %</TableCell>
                  <TableCell align="right">OEE %</TableCell>
                  <TableCell align="center">Trend</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {MOCK_MACHINE_OEE.map((row) => {
                  const oeeColor = oeeZoneColor(row.oee);
                  return (
                    <TableRow key={row.machineCode} hover>
                      <TableCell>
                        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                          {row.machineCode}
                        </Typography>
                      </TableCell>
                      <TableCell>{row.machineName}</TableCell>
                      <TableCell>
                        <Typography variant="caption" color="text.secondary">{row.workCenter}</Typography>
                      </TableCell>
                      <TableCell align="right">{row.availability.toFixed(1)}%</TableCell>
                      <TableCell align="right">{row.performance.toFixed(1)}%</TableCell>
                      <TableCell align="right">{row.quality.toFixed(1)}%</TableCell>
                      <TableCell align="right">
                        <Typography variant="body2" sx={{ fontWeight: 700, color: oeeColor }}>
                          {row.oee.toFixed(1)}%
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <Typography variant="body2" sx={{ fontWeight: 700, color: TREND_COLOR[row.trend] }}>
                          {TREND_ICON[row.trend]}
                        </Typography>
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
