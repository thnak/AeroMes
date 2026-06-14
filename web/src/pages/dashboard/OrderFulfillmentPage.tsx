import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Grid,
  Skeleton,
  Typography,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { KpiCard, PageHeader, PageRoot, RefreshButton } from '../../components';
import { StatusDonut, TrendLineChart } from '../../components/dashboard';
import { apiClient } from '../../lib/apiClient';

// ── Types ─────────────────────────────────────────────────────────────────────

interface OverdueSoDto {
  soCode: string;
  customerName?: string;
  deliveryDate: string;
  overdueDays: number;
}

interface SoFulfillmentDto {
  totalOrders: number;
  onTimeCount: number;
  overdueCount: number;
  inProductionCount: number;
  onTimeRatePct: number;
  topOverdue: OverdueSoDto[];
}

// ── API ───────────────────────────────────────────────────────────────────────

const fetchFulfillment = (year: number, month: number): Promise<SoFulfillmentDto> =>
  apiClient.get(`/api/v1/dashboard/so-fulfillment?year=${year}&month=${month}`).then((r) => r.data);

// ── Helpers ───────────────────────────────────────────────────────────────────

function lastNMonths(n: number): { year: number; month: number; label: string }[] {
  return Array.from({ length: n }, (_, i) => {
    const d = new Date();
    d.setMonth(d.getMonth() - (n - 1 - i));
    return { year: d.getFullYear(), month: d.getMonth() + 1, label: `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}` };
  });
}

export default function OrderFulfillmentPage() {
  const now = new Date();
  const currentYear = now.getFullYear();
  const currentMonth = now.getMonth() + 1;
  const months6 = lastNMonths(6);

  const { data: current, isLoading, refetch } = useQuery({
    queryKey: ['so-fulfillment', currentYear, currentMonth],
    queryFn: () => fetchFulfillment(currentYear, currentMonth),
  });

  const { data: trendData = [], isLoading: trendLoading } = useQuery({
    queryKey: ['so-fulfillment-trend', ...months6.map((m) => m.label)],
    queryFn: async () => {
      const results = await Promise.all(months6.map((m) => fetchFulfillment(m.year, m.month)));
      return results.map((r, i) => ({
        label: months6[i].label.slice(5),
        value: r.onTimeRatePct,
      }));
    },
  });

  const soSegments = current
    ? [
        { label: 'On Time', value: current.onTimeCount, color: '#10B981' },
        { label: 'In Production', value: current.inProductionCount, color: '#3B82F6' },
        { label: 'Overdue', value: current.overdueCount, color: '#EF4444' },
      ]
    : [];

  const pendingCount = (current?.totalOrders ?? 0) - (current?.onTimeCount ?? 0) - (current?.overdueCount ?? 0) - (current?.inProductionCount ?? 0);

  return (
    <PageRoot>
      <PageHeader
        title="Order Fulfillment"
        actions={<RefreshButton onClick={refetch} />}
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="On-Time Rate"
            value={current?.onTimeRatePct.toFixed(1) ?? '—'}
            unit="%"
            icon="integration"
            accentColor={(current?.onTimeRatePct ?? 100) >= 90 ? '#10B981' : '#EF4444'}
            loading={isLoading}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="Overdue Orders"
            value={current?.overdueCount ?? 0}
            icon="integration"
            accentColor={(current?.overdueCount ?? 0) > 0 ? '#EF4444' : '#10B981'}
            loading={isLoading}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="In Production"
            value={current?.inProductionCount ?? 0}
            icon="production"
            accentColor="#3B82F6"
            loading={isLoading}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="Total Orders"
            value={current?.totalOrders ?? 0}
            icon="integration"
            loading={isLoading}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="On Time"
            value={current?.onTimeCount ?? 0}
            icon="integration"
            accentColor="#10B981"
            loading={isLoading}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="Pending"
            value={Math.max(0, pendingCount)}
            icon="integration"
            accentColor="#F59E0B"
            loading={isLoading}
          />
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardHeader
              title="SO Status (this month)"
              subheader={current ? `${current.onTimeRatePct.toFixed(1)}% on time` : ''}
              titleTypographyProps={{ variant: 'subtitle1' }}
            />
            <CardContent sx={{ pt: 0 }}>
              {isLoading ? (
                <Skeleton variant="circular" width={200} height={200} sx={{ mx: 'auto' }} />
              ) : (
                <StatusDonut segments={soSegments} centerLabel="SOs" />
              )}
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardHeader title="Monthly Fulfillment Rate (6 months)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <TrendLineChart
                data={trendData}
                primaryLabel="On-Time Rate"
                unit="%"
                loading={trendLoading}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Overdue orders table */}
      {(current?.topOverdue.length ?? 0) > 0 && (
        <Card>
          <CardHeader title="Overdue Orders" titleTypographyProps={{ variant: 'subtitle1' }} />
          <CardContent sx={{ pt: 0 }}>
            <Box sx={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid rgba(0,0,0,0.12)' }}>
                    {['SO Code', 'Customer', 'Delivery Date', 'Days Overdue'].map((h) => (
                      <th key={h} style={{ textAlign: 'left', padding: '8px 12px', fontWeight: 600, color: '#64748B' }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {current!.topOverdue.map((so) => {
                    const rowBg =
                      so.overdueDays > 14 ? '#FEE2E2'
                        : so.overdueDays > 7 ? '#FEF3C7'
                        : '#FEFCE8';
                    const daysColor =
                      so.overdueDays > 14 ? '#DC2626'
                        : so.overdueDays > 7 ? '#D97706'
                        : '#A16207';
                    return (
                      <tr key={so.soCode} style={{ borderBottom: '1px solid rgba(0,0,0,0.06)', backgroundColor: rowBg }}>
                        <td style={{ padding: '8px 12px', fontWeight: 600 }}>{so.soCode}</td>
                        <td style={{ padding: '8px 12px' }}>{so.customerName ?? '—'}</td>
                        <td style={{ padding: '8px 12px' }}>{so.deliveryDate.slice(0, 10)}</td>
                        <td style={{ padding: '8px 12px' }}>
                          <Typography variant="caption" sx={{ fontWeight: 700, color: daysColor }}>
                            {so.overdueDays}d
                          </Typography>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </Box>
          </CardContent>
        </Card>
      )}
    </PageRoot>
  );
}
