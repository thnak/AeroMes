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
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState, useMemo } from 'react';
import {
  ExportButton,
  KpiCard,
  TablePageSkeleton,
  PageHeader,
  PageRoot,
} from '../../components';
import { useGetApiV1ReportsDowntime } from '../../api/reports/reports';
import { getErrorMessage } from '../../lib/apiClient';

function todayIso() { return new Date().toISOString().split('T')[0]; }
function monthAgoIso() {
  const d = new Date();
  d.setMonth(d.getMonth() - 1);
  return d.toISOString().split('T')[0];
}

export default function DowntimeReportPage() {
  const [from, setFrom] = useState(monthAgoIso());
  const [to, setTo] = useState(todayIso());

  const { data, isLoading, error } = useGetApiV1ReportsDowntime({
    from: `${from}T00:00:00Z`,
    to: `${to}T23:59:59Z`,
  });

  const dto = data?.data;
  const rows = useMemo(
    () => [...(dto?.rows ?? [])].sort((a, b) => Number(b.totalMinutes) - Number(a.totalMinutes)),
    [dto],
  );

  const totalEvents = Number(dto?.totalEvents ?? 0);
  const totalMinutes = Number(dto?.totalMinutes ?? 0);
  const totalHours = totalMinutes / 60;
  const maxMinutes = rows.length > 0 ? Number(rows[0].totalMinutes) : 1;

  const byMachine = useMemo(() => {
    const map = new Map<string, number>();
    rows.forEach((r) => {
      map.set(r.machineCode, (map.get(r.machineCode) ?? 0) + Number(r.totalMinutes));
    });
    return [...map.entries()]
      .sort(([, a], [, b]) => b - a)
      .slice(0, 8)
      .map(([code, mins]) => ({ code, hours: mins / 60 }));
  }, [rows]);

  const maxMachineHours = byMachine.length > 0 ? byMachine[0].hours : 1;

  if (isLoading) return <TablePageSkeleton />;

  return (
    <PageRoot>
      <PageHeader
        title="Downtime Report"
        subtitle="Downtime events by reason code and machine"
        breadcrumbs={[{ label: 'Reports' }, { label: 'Downtime' }]}
        actions={
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
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
            label="Total Downtime"
            value={totalHours.toFixed(1)}
            unit="h"
            icon="warning"
            accentColor="#B91C1C"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Total Events"
            value={totalEvents.toLocaleString()}
            unit="events"
            icon="warning"
            accentColor="#D97706"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Avg per Event"
            value={totalEvents > 0 ? (totalMinutes / totalEvents).toFixed(0) : '—'}
            unit="min"
            icon="time"
            accentColor="#64748B"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Machines Affected"
            value={new Set(rows.map((r) => r.machineCode)).size.toString()}
            unit="machines"
            icon="machineOn"
            accentColor="#1D4ED8"
          />
        </Grid>
      </Grid>

      {/* ── Pareto + By machine ───────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Downtime by Reason Code"
              subheader="Pareto — top 10 reasons"
              titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
            />
            <CardContent>
              {rows.length === 0 ? (
                <Typography variant="body2" color="text.disabled" sx={{ textAlign: 'center', py: 4 }}>
                  No downtime events in this period.
                </Typography>
              ) : (
                <Stack spacing={1.5}>
                  {rows.slice(0, 10).map((r) => {
                    const pct = totalMinutes > 0 ? (Number(r.totalMinutes) / totalMinutes) * 100 : 0;
                    return (
                      <Box key={`${r.machineCode}-${r.reasonCode}`}>
                        <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                          <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                            <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'text.secondary', fontSize: 11 }}>
                              {r.reasonCode}
                            </Typography>
                            <Typography variant="caption" sx={{ fontWeight: 500 }}>
                              {r.reasonName ?? r.reasonCode}
                            </Typography>
                          </Stack>
                          <Typography variant="caption" color="text.secondary" sx={{ whiteSpace: 'nowrap', ml: 1 }}>
                            {(Number(r.totalMinutes) / 60).toFixed(1)}h · {pct.toFixed(1)}%
                          </Typography>
                        </Stack>
                        <LinearProgress
                          variant="determinate"
                          value={(Number(r.totalMinutes) / maxMinutes) * 100}
                          sx={{
                            height: 6,
                            borderRadius: 3,
                            bgcolor: alpha('#B91C1C', 0.12),
                            '& .MuiLinearProgress-bar': { bgcolor: '#B91C1C' },
                          }}
                        />
                      </Box>
                    );
                  })}
                </Stack>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Downtime by Machine"
              subheader="Top 8 machines by total hours"
              titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
            />
            <CardContent>
              {byMachine.length === 0 ? (
                <Typography variant="body2" color="text.disabled" sx={{ textAlign: 'center', py: 4 }}>No data.</Typography>
              ) : (
                <Stack spacing={1.5}>
                  {byMachine.map(({ code, hours }) => (
                    <Box key={code}>
                      <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 500 }}>{code}</Typography>
                        <Typography variant="caption" color="text.secondary">{hours.toFixed(1)}h</Typography>
                      </Stack>
                      <LinearProgress
                        variant="determinate"
                        value={(hours / maxMachineHours) * 100}
                        sx={{
                          height: 6,
                          borderRadius: 3,
                          bgcolor: alpha('#D97706', 0.12),
                          '& .MuiLinearProgress-bar': { bgcolor: '#D97706' },
                        }}
                      />
                    </Box>
                  ))}
                </Stack>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* ── Full table ────────────────────────────────────────────────── */}
      <Card>
        <CardHeader
          title="Downtime Detail"
          titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
        />
        <CardContent sx={{ p: '0 !important' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                  <TableCell>Machine</TableCell>
                  <TableCell>Reason Code</TableCell>
                  <TableCell>Reason Name</TableCell>
                  <TableCell align="right">Events</TableCell>
                  <TableCell align="right">Total Minutes</TableCell>
                  <TableCell align="right">Hours</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {rows.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} sx={{ textAlign: 'center', py: 4, color: 'text.disabled' }}>
                      No downtime events recorded
                    </TableCell>
                  </TableRow>
                ) : (
                  rows.map((row, i) => (
                    <TableRow key={`${row.machineCode}-${row.reasonCode}-${i}`} hover>
                      <TableCell>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                          {row.machineCode}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', color: 'text.secondary' }}>
                          {row.reasonCode}
                        </Typography>
                      </TableCell>
                      <TableCell>{row.reasonName ?? '—'}</TableCell>
                      <TableCell align="right">{Number(row.eventCount).toLocaleString()}</TableCell>
                      <TableCell align="right">{Number(row.totalMinutes).toFixed(0)}</TableCell>
                      <TableCell align="right">{(Number(row.totalMinutes) / 60).toFixed(1)}</TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </Box>
        </CardContent>
      </Card>
    </PageRoot>
  );
}
