import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Grid,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material';
import { alpha, useTheme } from '@mui/material/styles';
import { useQuery } from '@tanstack/react-query';
import { PageHeader, PageRoot, RefreshButton } from '../../components';
import { TrendLineChart } from '../../components/dashboard';
import { apiClient } from '../../lib/apiClient';
import { useAuth } from '../../contexts/AuthContext';

// ── Types ─────────────────────────────────────────────────────────────────────

interface JobSummary {
  jobCode: string;
  workOrderCode: string;
  productCode: string;
  productName: string;
  machineCode: string;
  shiftName: string;
  startTime: string;
  endTime?: string;
  actualQtyOK: number;
  actualQtyNG: number;
  targetQty: number;
  status: string;
}

interface DowntimeSummary {
  machineCode: string;
  startTime: string;
  endTime?: string;
  durationMinutes?: number;
  reason: string;
}

interface MyDailyOutputDto {
  date: string;
  totalActualQtyOK: number;
  totalTargetQty: number;
  totalActualQtyNG: number;
  activeJobCount: number;
  finishedJobCount: number;
  jobs: JobSummary[];
  downtimes: DowntimeSummary[];
}

interface MyHistoryPoint {
  date: string;
  totalActualQtyOK: number;
  totalTargetQty: number;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function todayIso() {
  return new Date().toISOString().split('T')[0];
}

function formatTime(iso: string) {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

// ── API ───────────────────────────────────────────────────────────────────────

const fetchMyDaily = (date: string): Promise<MyDailyOutputDto> =>
  apiClient.get(`/api/v1/dashboard/my/daily?date=${date}`).then((r) => r.data);

const fetchMyHistory = (days: number): Promise<MyHistoryPoint[]> =>
  apiClient.get(`/api/v1/dashboard/my/history?days=${days}`).then((r) => r.data);

// ── Active Job Card ───────────────────────────────────────────────────────────

function ActiveJobCard({ job }: { job: JobSummary }) {
  const theme = useTheme();
  const pct = job.targetQty > 0 ? (job.actualQtyOK / job.targetQty) * 100 : 0;
  const progressColor =
    pct < 50 ? theme.palette.warning.main : pct < 100 ? theme.palette.primary.main : theme.palette.success.main;

  return (
    <Card variant="outlined" sx={{ mb: 1.5, border: `2px solid ${alpha(progressColor, 0.4)}` }}>
      <CardContent>
        <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
          <Stack spacing={0.25}>
            <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
              {job.jobCode}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {job.workOrderCode} · {job.productCode} {job.productName}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Machine: {job.machineCode} · Shift: {job.shiftName} · Started: {formatTime(job.startTime)}
            </Typography>
          </Stack>
          <Chip label={job.status} size="small" color="primary" />
        </Stack>

        <Stack direction="row" sx={{ alignItems: 'center', gap: 1, mb: 0.5 }}>
          <LinearProgress
            variant="determinate"
            value={Math.min(100, pct)}
            sx={{
              flex: 1,
              height: 8,
              borderRadius: 4,
              bgcolor: alpha(progressColor, 0.15),
              '& .MuiLinearProgress-bar': { bgcolor: progressColor, borderRadius: 4 },
            }}
          />
          <Typography variant="body2" sx={{ fontWeight: 600, minWidth: 80, textAlign: 'right' }}>
            {job.actualQtyOK} / {job.targetQty}
          </Typography>
        </Stack>

        {job.actualQtyNG > 0 && (
          <Typography variant="caption" color="error.main">
            QtyNG: {job.actualQtyNG}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function EmployeeDashboardPage() {
  const { user } = useAuth();
  const today = todayIso();

  const { data: daily, isLoading, refetch } = useQuery({
    queryKey: ['my-daily', today],
    queryFn: () => fetchMyDaily(today),
  });

  const { data: historyRaw = [] } = useQuery({
    queryKey: ['my-history', 14],
    queryFn: () => fetchMyHistory(14),
  });

  const defectRate =
    daily && daily.totalActualQtyOK + daily.totalActualQtyNG > 0
      ? ((daily.totalActualQtyNG / (daily.totalActualQtyOK + daily.totalActualQtyNG)) * 100).toFixed(1)
      : '0.0';

  const activeJobs = daily?.jobs.filter((j) => j.status === 'STARTED') ?? [];
  const allJobs = daily?.jobs ?? [];

  const historyTrend = historyRaw.map((p) => ({
    label: p.date.slice(5),
    value: p.totalActualQtyOK,
    secondary: p.totalActualQtyOK > 0 && p.totalTargetQty > 0
      ? parseFloat(((p.totalActualQtyOK / p.totalTargetQty) * 100).toFixed(1))
      : 0,
  }));

  return (
    <PageRoot>
      <PageHeader
        title="My Today"
        subtitle={today}
        actions={
          <Stack direction="row" sx={{ gap: 1, alignItems: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              Hi, {user?.name ?? 'Operator'}
            </Typography>
            <RefreshButton onClick={refetch} />
          </Stack>
        }
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontWeight: 600 }}>
                My Output Today
              </Typography>
              {isLoading ? (
                <Box sx={{ mt: 1 }}><LinearProgress /></Box>
              ) : (
                <>
                  <Typography variant="h3" sx={{ fontWeight: 700, color: 'success.main', mt: 0.5 }}>
                    {daily?.totalActualQtyOK ?? 0}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    / {daily?.totalTargetQty ?? 0} target pcs
                  </Typography>
                  {(daily?.totalTargetQty ?? 0) > 0 && (
                    <LinearProgress
                      variant="determinate"
                      value={Math.min(100, ((daily?.totalActualQtyOK ?? 0) / (daily?.totalTargetQty ?? 1)) * 100)}
                      sx={{ mt: 1, height: 6, borderRadius: 3 }}
                      color="success"
                    />
                  )}
                </>
              )}
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontWeight: 600 }}>
                My Defect Rate
              </Typography>
              <Typography
                variant="h3"
                sx={{ fontWeight: 700, color: parseFloat(defectRate) > 2 ? 'error.main' : 'success.main', mt: 0.5 }}
              >
                {defectRate}%
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {daily?.totalActualQtyNG ?? 0} NG out of {(daily?.totalActualQtyOK ?? 0) + (daily?.totalActualQtyNG ?? 0)} pcs
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontWeight: 600 }}>
                My Jobs Today
              </Typography>
              <Stack direction="row" spacing={2} sx={{ mt: 0.5, alignItems: 'center' }}>
                <Box>
                  <Typography variant="h3" sx={{ fontWeight: 700, color: 'primary.main' }}>
                    {daily?.activeJobCount ?? 0}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">Active</Typography>
                </Box>
                <Typography variant="h4" color="text.disabled">/</Typography>
                <Box>
                  <Typography variant="h3" sx={{ fontWeight: 700, color: 'text.primary' }}>
                    {daily?.finishedJobCount ?? 0}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">Finished</Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Active Jobs */}
      <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>My Active Job</Typography>
      {activeJobs.length === 0 ? (
        <Card variant="outlined" sx={{ mb: 3 }}>
          <CardContent>
            <Typography color="text.disabled" align="center">No active job right now</Typography>
          </CardContent>
        </Card>
      ) : (
        <Box sx={{ mb: 3 }}>
          {activeJobs.map((job) => (
            <ActiveJobCard key={job.jobCode} job={job} />
          ))}
        </Box>
      )}

      {/* Jobs Table */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardHeader title="My Jobs Today" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              {allJobs.length === 0 ? (
                <Typography color="text.disabled" align="center" sx={{ py: 2 }}>No jobs today</Typography>
              ) : (
                <Box sx={{ overflowX: 'auto' }}>
                  <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                    <thead>
                      <tr style={{ borderBottom: '1px solid rgba(0,0,0,0.12)' }}>
                        {['Job #', 'WO', 'Machine', 'Start', 'End', 'OK', 'NG', 'Status'].map((h) => (
                          <th key={h} style={{ textAlign: 'left', padding: '8px 12px', fontWeight: 600, color: '#64748B', whiteSpace: 'nowrap' }}>{h}</th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {allJobs.map((job) => (
                        <tr key={job.jobCode} style={{ borderBottom: '1px solid rgba(0,0,0,0.06)' }}>
                          <td style={{ padding: '8px 12px', fontWeight: 600 }}>{job.jobCode}</td>
                          <td style={{ padding: '8px 12px' }}>{job.workOrderCode}</td>
                          <td style={{ padding: '8px 12px' }}>{job.machineCode}</td>
                          <td style={{ padding: '8px 12px', whiteSpace: 'nowrap' }}>{formatTime(job.startTime)}</td>
                          <td style={{ padding: '8px 12px', whiteSpace: 'nowrap' }}>{job.endTime ? formatTime(job.endTime) : '—'}</td>
                          <td style={{ padding: '8px 12px', color: '#10B981', fontWeight: 600 }}>{job.actualQtyOK}</td>
                          <td style={{ padding: '8px 12px', color: job.actualQtyNG > 0 ? '#EF4444' : undefined }}>{job.actualQtyNG}</td>
                          <td style={{ padding: '8px 12px' }}>
                            <Chip label={job.status} size="small" />
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader title="My Output (14 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <TrendLineChart
                data={historyTrend}
                primaryLabel="OK qty"
                unit="pcs"
                height={220}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Downtime */}
      {(daily?.downtimes ?? []).length > 0 && (
        <Card>
          <CardHeader title="My Downtime Today" titleTypographyProps={{ variant: 'subtitle1' }} />
          <CardContent sx={{ pt: 0 }}>
            <Box sx={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid rgba(0,0,0,0.12)' }}>
                    {['Machine', 'Start', 'End', 'Duration (min)', 'Reason'].map((h) => (
                      <th key={h} style={{ textAlign: 'left', padding: '8px 12px', fontWeight: 600, color: '#64748B' }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {daily!.downtimes.map((d, i) => (
                    <tr key={i} style={{ borderBottom: '1px solid rgba(0,0,0,0.06)' }}>
                      <td style={{ padding: '8px 12px' }}>{d.machineCode}</td>
                      <td style={{ padding: '8px 12px' }}>{formatTime(d.startTime)}</td>
                      <td style={{ padding: '8px 12px' }}>{d.endTime ? formatTime(d.endTime) : '—'}</td>
                      <td style={{ padding: '8px 12px' }}>{d.durationMinutes ?? '—'}</td>
                      <td style={{ padding: '8px 12px' }}>{d.reason}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Box>
          </CardContent>
        </Card>
      )}
    </PageRoot>
  );
}
