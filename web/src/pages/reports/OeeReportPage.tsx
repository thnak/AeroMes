import {
  Alert,
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
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState } from 'react';
import {
  ExportButton,
  KpiCard,
  TablePageSkeleton,
  PageHeader,
  PageRoot,
} from '../../components';
import { useGetApiV1ProductionOee } from '../../api/production/production';
import { useGetApiV1Machines } from '../../api/machines/machines';
import { getErrorMessage } from '../../lib/apiClient';
import { oeeZoneColor } from '../../theme/tokens';

function todayIso() { return new Date().toISOString().split('T')[0]; }
function monthAgoIso() {
  const d = new Date();
  d.setMonth(d.getMonth() - 1);
  return d.toISOString().split('T')[0];
}

export default function OeeReportPage() {
  const [from, setFrom] = useState(monthAgoIso());
  const [to, setTo] = useState(todayIso());
  const [machineCode, setMachineCode] = useState('');

  const { data: machinesData } = useGetApiV1Machines();
  const machines = machinesData ?? [];

  const { data: oeeData, isLoading, error } = useGetApiV1ProductionOee({
    machineCode: machineCode || undefined,
    shiftStart: `${from}T00:00:00Z`,
    shiftEnd: `${to}T23:59:59Z`,
  });

  const oee = oeeData?.data;

  const availability = oee ? Number(oee.availabilityPercent) : null;
  const performance  = oee ? Number(oee.performancePercent) : null;
  const quality      = oee ? Number(oee.qualityPercent) : null;
  const oeeVal       = oee ? Number(oee.oeePercent) : null;

  if (isLoading) return <TablePageSkeleton />;

  return (
    <PageRoot>
      <PageHeader
        title="OEE Report"
        subtitle="Overall Equipment Effectiveness by machine and period"
        breadcrumbs={[{ label: 'Reports' }, { label: 'OEE' }]}
        actions={
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <TextField
              select
              size="small"
              label="Machine"
              value={machineCode}
              onChange={(e) => setMachineCode(e.target.value)}
              sx={{ minWidth: 160 }}
              slotProps={{ select: { native: true } }}
            >
              <option value="">All machines</option>
              {machines.map((m) => (
                <option key={m.machineCode} value={m.machineCode ?? ''}>
                  {m.machineName} ({m.machineCode})
                </option>
              ))}
            </TextField>
            <TextField
              type="date"
              size="small"
              label="From"
              value={from}
              onChange={(e) => setFrom(e.target.value)}
              sx={{ width: 150 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              type="date"
              size="small"
              label="To"
              value={to}
              onChange={(e) => setTo(e.target.value)}
              sx={{ width: 150 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <ExportButton />
          </Stack>
        }
      />

      {error && <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(error)}</Alert>}

      {/* ── KPI row ──────────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="OEE"
            value={oeeVal !== null ? oeeVal.toFixed(1) : '—'}
            unit="%"
            icon="oee"
            accentColor={oeeVal !== null ? oeeZoneColor(oeeVal) : '#94A3B8'}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Availability"
            value={availability !== null ? availability.toFixed(1) : '—'}
            unit="%"
            icon="success"
            accentColor="#1D4ED8"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Performance"
            value={performance !== null ? performance.toFixed(1) : '—'}
            unit="%"
            icon="reports"
            accentColor="#D97706"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Quality"
            value={quality !== null ? quality.toFixed(1) : '—'}
            unit="%"
            icon="shift"
            accentColor="#15803D"
          />
        </Grid>
      </Grid>

      {/* ── OEE gauge bars + production summary ──────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="OEE Components"
              titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
            />
            <CardContent>
              {oee ? (
                <Stack spacing={2.5}>
                  {[
                    { label: 'Availability', value: availability!, color: '#1D4ED8' },
                    { label: 'Performance', value: performance!, color: '#D97706' },
                    { label: 'Quality', value: quality!, color: '#15803D' },
                    { label: 'OEE', value: oeeVal!, color: oeeZoneColor(oeeVal!) },
                  ].map(({ label, value, color }) => (
                    <Box key={label}>
                      <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                        <Typography variant="caption" sx={{ fontWeight: 500 }}>{label}</Typography>
                        <Typography variant="caption" sx={{ fontWeight: 700, color }}>
                          {value.toFixed(1)}%
                        </Typography>
                      </Stack>
                      <LinearProgress
                        variant="determinate"
                        value={value}
                        sx={{
                          height: 10,
                          borderRadius: 5,
                          bgcolor: alpha(color, 0.12),
                          '& .MuiLinearProgress-bar': { bgcolor: color, borderRadius: 5 },
                        }}
                      />
                    </Box>
                  ))}
                </Stack>
              ) : (
                <Typography variant="body2" color="text.disabled" sx={{ textAlign: 'center', py: 4 }}>
                  Select a machine and date range to view OEE.
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 7 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Production Summary"
              titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
            />
            <CardContent>
              {oee ? (
                <Table size="small">
                  <TableBody>
                    {[
                      { label: 'Machine Code', value: oee.machineCode },
                      { label: 'Planned Time', value: `${(Number(oee.totalPlannedMinutes) / 60).toFixed(1)} h` },
                      { label: 'Downtime', value: `${(Number(oee.downtimeMinutes) / 60).toFixed(1)} h` },
                      { label: 'Total Produced', value: Number(oee.totalProduced).toLocaleString() },
                      { label: 'OK Count', value: Number(oee.okCount).toLocaleString() },
                      { label: 'NG Count', value: Number(oee.ngCount).toLocaleString() },
                    ].map(({ label, value }) => (
                      <TableRow key={label}>
                        <TableCell sx={{ color: 'text.secondary', fontSize: 12, fontWeight: 500 }}>{label}</TableCell>
                        <TableCell align="right" sx={{ fontWeight: 600 }}>{value}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              ) : (
                <Typography variant="body2" color="text.disabled" sx={{ textAlign: 'center', py: 4 }}>
                  No data for the selected filters.
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
