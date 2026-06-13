import {
  Alert,
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
  Typography,
} from '@mui/material';
import { alpha, useTheme } from '@mui/material/styles';
import { useEffect, useRef } from 'react';
import { useGetApiV1Machines } from '../../api/machines/machines';
import { useGetApiV1IotMachinesStates } from '../../api/iot/iot';
import { useGetApiV1WorkOrders } from '../../api/work-orders/work-orders';
import { useGetApiV1ReportsDowntime } from '../../api/reports/reports';
import type { MachineDto, MachineStateSnapshotDto, WorkOrderDto } from '../../api/model';
import { SolarIcon } from '../../components';
import {
  machineColors,
  oeeZoneColor,
  oeeZones,
} from '../../theme/tokens';

const OEE_ZONE_LIST = Object.values(oeeZones).sort((a, b) => b.min - a.min);

// ── Helpers ────────────────────────────────────────────────────────────────

function todayIso() { return new Date().toISOString().split('T')[0]; }
function pad2(n: number) { return String(n).padStart(2, '0'); }
function formatTime(d: Date) {
  return `${pad2(d.getHours())}:${pad2(d.getMinutes())}:${pad2(d.getSeconds())}`;
}

function stateColor(state: string): string {
  return machineColors[state as keyof typeof machineColors] ?? '#94A3B8';
}

// ── Clock ──────────────────────────────────────────────────────────────────

function DashboardClock() {
  const ref = useRef<HTMLSpanElement>(null);
  useEffect(() => {
    const tick = () => {
      if (ref.current) ref.current.textContent = formatTime(new Date());
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, []);
  return (
    <Typography
      variant="h4"
      component="span"
      ref={ref}
      sx={{ fontFamily: 'monospace', fontWeight: 700, color: 'primary.main', fontSize: { xs: 22, md: 32 } }}
    />
  );
}

// ── OEE gauge bar ──────────────────────────────────────────────────────────

function OeeBar({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <Box>
      <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
        <Typography variant="caption" sx={{ fontWeight: 600, color: 'text.secondary' }}>{label}</Typography>
        <Typography variant="caption" sx={{ fontWeight: 800, color }}>{value.toFixed(1)}%</Typography>
      </Stack>
      <LinearProgress
        variant="determinate"
        value={value}
        sx={{
          height: 8, borderRadius: 4,
          bgcolor: alpha(color, 0.14),
          '& .MuiLinearProgress-bar': { bgcolor: color, borderRadius: 4 },
        }}
      />
    </Box>
  );
}

// ── Machine status card ────────────────────────────────────────────────────

function MachineCard({
  machine,
  snapshot,
}: {
  machine: MachineDto;
  snapshot: MachineStateSnapshotDto | undefined;
}) {
  const state = snapshot?.currentState ?? machine.status ?? 'OFFLINE';
  const color = stateColor(state);
  const isDown = state === 'DOWN' || state === 'OFFLINE';

  return (
    <Box
      sx={{
        p: 1.5,
        borderRadius: 2,
        border: '1.5px solid',
        borderColor: isDown ? 'error.main' : color,
        bgcolor: (t) => alpha(color, t.palette.mode === 'dark' ? 0.12 : 0.06),
        animation: isDown ? 'pulse 2s ease-in-out infinite' : undefined,
        '@keyframes pulse': {
          '0%, 100%': { opacity: 1 },
          '50%': { opacity: 0.6 },
        },
      }}
    >
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <Box>
          <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 700, color, fontSize: 11 }}>
            {machine.machineCode}
          </Typography>
          <Typography variant="caption" sx={{ display: 'block', color: 'text.secondary', fontSize: 10, mt: 0.25 }} noWrap>
            {machine.machineName}
          </Typography>
        </Box>
        <Chip
          label={state}
          size="small"
          sx={{
            height: 18, fontSize: 9, fontWeight: 700,
            bgcolor: color, color: '#fff',
            '& .MuiChip-label': { px: 1 },
          }}
        />
      </Stack>
      {snapshot && (
        <Typography variant="caption" sx={{ color: 'text.disabled', fontSize: 9, mt: 0.5, display: 'block' }}>
          {new Date(snapshot.stateChangedAt).toLocaleTimeString()}
        </Typography>
      )}
    </Box>
  );
}

// ── Dashboard page ─────────────────────────────────────────────────────────

const todayFrom = `${todayIso()}T00:00:00Z`;
const todayTo = `${todayIso()}T23:59:59Z`;

export default function DashboardPage() {
  const theme = useTheme();

  const { data: machinesData } = useGetApiV1Machines(
    undefined, { query: { refetchInterval: 30_000 } }
  );
  const { data: statesData } = useGetApiV1IotMachinesStates(
    { query: { refetchInterval: 10_000 } }
  );
  const { data: workOrdersData } = useGetApiV1WorkOrders(
    { status: 'IN_PROGRESS' }, { query: { refetchInterval: 30_000 } }
  );
  const { data: downtimeData } = useGetApiV1ReportsDowntime(
    { from: todayFrom, to: todayTo }, { query: { refetchInterval: 60_000 } }
  );

  const machines: MachineDto[] = machinesData ?? [];
  const stateMap = new Map<string, MachineStateSnapshotDto>(
    (statesData ?? []).map((s) => [s.machineCode, s]),
  );

  const activeWOs: WorkOrderDto[] = (workOrdersData?.data ?? []) as WorkOrderDto[];

  const running = machines.filter((m) => stateMap.get(m.machineCode)?.currentState === 'RUNNING').length;
  const down = machines.filter((m) => {
    const s = stateMap.get(m.machineCode)?.currentState ?? m.status;
    return s === 'DOWN' || s === 'OFFLINE';
  }).length;

  const totalTarget = activeWOs.reduce((s, w) => s + Number(w.targetQty), 0);
  const totalOk = activeWOs.reduce((s, w) => s + Number(w.actualOK), 0);
  const totalNg = activeWOs.reduce((s, w) => s + Number(w.actualNG), 0);
  const progressPct = totalTarget > 0 ? (totalOk / totalTarget) * 100 : 0;
  const qualityPct = totalOk + totalNg > 0 ? (totalOk / (totalOk + totalNg)) * 100 : 0;

  const downtimeMinutes = Number(downtimeData?.data?.totalMinutes ?? 0);
  const downtimeHours = downtimeMinutes / 60;

  const oeeEstimate = qualityPct > 0 ? Math.min(progressPct * 0.9, 100) : 0;
  const oeeColor = oeeZoneColor(oeeEstimate);

  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: theme.palette.mode === 'dark' ? '#0A0F1A' : '#F0F4FA',
        p: { xs: 1.5, md: 2.5 },
      }}
    >
      {/* ── Header ─────────────────────────────────────────────────────── */}
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 2.5 }}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
          <Box sx={{ width: 32, height: 32, bgcolor: 'primary.main', borderRadius: 1.5, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <SolarIcon name="machineOn" size={18} />
          </Box>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 800, lineHeight: 1.1 }}>
              Production Dashboard
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Live factory floor status
            </Typography>
          </Box>
        </Stack>
        <DashboardClock />
      </Stack>

      {/* ── KPI strip ──────────────────────────────────────────────────── */}
      <Grid container spacing={2} sx={{ mb: 2.5 }}>
        {[
          { label: 'Running', value: running, unit: 'machines', color: '#15803D', icon: 'machineOn' as const },
          { label: 'Down / Offline', value: down, unit: 'machines', color: '#DC2626', icon: 'machineOff' as const },
          { label: 'Active WOs', value: activeWOs.length, unit: 'orders', color: '#1D4ED8', icon: 'reports' as const },
          { label: 'Output Today', value: totalOk.toLocaleString(), unit: `/ ${totalTarget.toLocaleString()} target`, color: oeeColor, icon: 'success' as const },
          { label: 'NG Today', value: totalNg.toLocaleString(), unit: 'pcs', color: '#D97706', icon: 'warning' as const },
          { label: 'Downtime', value: downtimeHours.toFixed(1), unit: 'h', color: '#7C3AED', icon: 'shift' as const },
        ].map(({ label, value, unit, color, icon }) => (
          <Grid size={{ xs: 6, sm: 4, lg: 2 }} key={label}>
            <Card
              sx={{
                borderTop: `3px solid ${color}`,
                bgcolor: (t) => alpha(color, t.palette.mode === 'dark' ? 0.08 : 0.04),
              }}
            >
              <CardContent sx={{ py: 1.5, px: 2, '&:last-child': { pb: 1.5 } }}>
                <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, fontSize: 11 }}>
                      {label}
                    </Typography>
                    <Typography variant="h5" sx={{ fontWeight: 800, color, lineHeight: 1.1, mt: 0.25 }}>
                      {value}
                    </Typography>
                    <Typography variant="caption" color="text.disabled" sx={{ fontSize: 10 }}>
                      {unit}
                    </Typography>
                  </Box>
                  <Box sx={{ color, opacity: 0.6 }}>
                    <SolarIcon name={icon} size={22} />
                  </Box>
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={2.5}>
        {/* ── Machine Status Grid ───────────────────────────────────────── */}
        <Grid size={{ xs: 12, lg: 7 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Machine Status"
              titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
              subheader={`${machines.length} machines · last update ${new Date().toLocaleTimeString()}`}
              subheaderTypographyProps={{ variant: 'caption' }}
            />
            <CardContent>
              {machines.length === 0 ? (
                <Alert severity="info">No machines configured.</Alert>
              ) : (
                <Box
                  sx={{
                    display: 'grid',
                    gridTemplateColumns: 'repeat(auto-fill, minmax(140px, 1fr))',
                    gap: 1,
                  }}
                >
                  {machines.map((m) => (
                    <MachineCard key={m.machineCode} machine={m} snapshot={stateMap.get(m.machineCode)} />
                  ))}
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* ── Right column ──────────────────────────────────────────────── */}
        <Grid size={{ xs: 12, lg: 5 }}>
          <Stack spacing={2.5}>
            {/* OEE Gauges */}
            <Card>
              <CardHeader
                title="OEE Estimate (shift)"
                titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
                subheader="Derived from active WO progress"
                subheaderTypographyProps={{ variant: 'caption' }}
              />
              <CardContent>
                <Stack spacing={2}>
                  <OeeBar label="OEE (estimated)" value={oeeEstimate} color={oeeColor} />
                  <OeeBar label="Quality" value={qualityPct} color={oeeZoneColor(qualityPct)} />
                  <OeeBar label="Output Progress" value={progressPct} color={oeeZoneColor(progressPct)} />
                </Stack>
                <Stack direction="row" spacing={1} sx={{ mt: 2, flexWrap: 'wrap', gap: 0.75 }}>
                  {OEE_ZONE_LIST.map((z) => (
                    <Chip
                      key={z.label}
                      label={`${z.label} ≥${z.min}%`}
                      size="small"
                      sx={{ bgcolor: z.color, color: '#fff', fontSize: 10, height: 20 }}
                    />
                  ))}
                </Stack>
              </CardContent>
            </Card>

            {/* Active Work Orders */}
            <Card sx={{ flex: 1 }}>
              <CardHeader
                title="Active Work Orders"
                titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
                subheader={`${activeWOs.length} in progress`}
                subheaderTypographyProps={{ variant: 'caption' }}
              />
              <CardContent sx={{ p: '0 !important' }}>
                {activeWOs.length === 0 ? (
                  <Box sx={{ p: 2 }}>
                    <Typography variant="body2" color="text.disabled" sx={{ textAlign: 'center' }}>
                      No active work orders
                    </Typography>
                  </Box>
                ) : (
                  <Table size="small">
                    <TableHead>
                      <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 11 } }}>
                        <TableCell>WO Code</TableCell>
                        <TableCell align="right">Target</TableCell>
                        <TableCell align="right">OK</TableCell>
                        <TableCell align="right">Progress</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {activeWOs.slice(0, 10).map((wo) => {
                        const pct = Number(wo.targetQty) > 0
                          ? (Number(wo.actualOK) / Number(wo.targetQty)) * 100
                          : 0;
                        const woColor = oeeZoneColor(pct);
                        return (
                          <TableRow key={String(wo.woid)} hover>
                            <TableCell>
                              <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 700 }}>
                                {wo.woCode}
                              </Typography>
                            </TableCell>
                            <TableCell align="right">{Number(wo.targetQty).toLocaleString()}</TableCell>
                            <TableCell align="right">{Number(wo.actualOK).toLocaleString()}</TableCell>
                            <TableCell align="right">
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, justifyContent: 'flex-end' }}>
                                <LinearProgress
                                  variant="determinate"
                                  value={Math.min(pct, 100)}
                                  sx={{
                                    width: 50, height: 5, borderRadius: 3,
                                    bgcolor: alpha(woColor, 0.15),
                                    '& .MuiLinearProgress-bar': { bgcolor: woColor },
                                  }}
                                />
                                <Typography variant="caption" sx={{ color: woColor, fontWeight: 700, fontSize: 10, minWidth: 32 }}>
                                  {pct.toFixed(0)}%
                                </Typography>
                              </Box>
                            </TableCell>
                          </TableRow>
                        );
                      })}
                    </TableBody>
                  </Table>
                )}
              </CardContent>
            </Card>
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
}
