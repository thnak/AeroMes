import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Grid,
  Skeleton,
  Stack,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { KpiCard, PageHeader, PageRoot, RefreshButton } from '../../components';
import {
  AlertBanner,
  DashboardParetoChart,
  ShiftComparisonBar,
  StatusDonut,
  TrendLineChart,
} from '../../components/dashboard';
import type { AlertItem } from '../../components/dashboard';
import { apiClient } from '../../lib/apiClient';
import { oeeZoneColor } from '../../theme/tokens';

// ── Types ─────────────────────────────────────────────────────────────────────

interface FactoryKpiDto {
  totalActualQtyOK: number;
  totalTargetQty: number;
  activeWorkOrders: number;
  openDowntimeCount: number;
  totalActualQtyNG: number;
  lowStockAlertCount: number;
  expiringLotCount: number;
}

interface OeeByMachineDto {
  machineCode: string;
  oee: number;
  availability: number;
  performance: number;
  quality: number;
}

interface ShiftOutputDto {
  shiftName: string;
  actualQtyOK: number;
  actualQtyNG: number;
}

interface DefectParetoItemDto {
  code: string;
  name: string;
  count: number;
  cumulativePct: number;
}

interface InventoryAlertSummaryDto {
  lowStockItems: { productCode: string; productName: string; availableQty: number; minStock: number }[];
  expiringItems: { lotNumber: string; productCode: string; expiryDate: string; availableQty: number }[];
}

interface SoFulfillmentDto {
  totalSOs: number;
  onTimeCount: number;
  overdueCount: number;
  pendingCount: number;
  fulfillmentRate: number;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function todayIso() {
  return new Date().toISOString().split('T')[0];
}

function lastNDays(n: number): string[] {
  return Array.from({ length: n }, (_, i) => {
    const d = new Date();
    d.setDate(d.getDate() - (n - 1 - i));
    return d.toISOString().split('T')[0];
  });
}

// ── API fetchers ──────────────────────────────────────────────────────────────

const fetchKpi = (date: string): Promise<FactoryKpiDto> =>
  apiClient.get(`/api/v1/dashboard/kpi?date=${date}`).then((r) => r.data);

const fetchOee = (date: string): Promise<OeeByMachineDto[]> =>
  apiClient.get(`/api/v1/dashboard/oee?date=${date}`).then((r) => r.data);

const fetchShiftOutput = (date: string): Promise<ShiftOutputDto[]> =>
  apiClient.get(`/api/v1/dashboard/shift-output?date=${date}`).then((r) => r.data);

const fetchDefectPareto = (from: string, to: string): Promise<DefectParetoItemDto[]> =>
  apiClient.get(`/api/v1/dashboard/defect-pareto?from=${from}&to=${to}`).then((r) => r.data);

const fetchInventoryAlerts = (): Promise<InventoryAlertSummaryDto> =>
  apiClient.get('/api/v1/dashboard/inventory-alerts').then((r) => r.data);

const fetchSoFulfillment = (year: number, month: number): Promise<SoFulfillmentDto> =>
  apiClient.get(`/api/v1/dashboard/so-fulfillment?year=${year}&month=${month}`).then((r) => r.data);

// ── Page ─────────────────────────────────────────────────────────────────────

export default function ManagerKpiDashboardPage() {
  const [date, setDate] = useState(todayIso());
  const navigate = useNavigate();

  const days = lastNDays(7);
  const now = new Date();
  const from30 = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 30).toISOString().split('T')[0];

  const { data: kpi, isLoading: kpiLoading, refetch: refetchKpi } = useQuery({
    queryKey: ['dashboard-kpi', date],
    queryFn: () => fetchKpi(date),
  });

  const { data: kpiTrend = [], isLoading: trendLoading } = useQuery({
    queryKey: ['dashboard-kpi-trend', ...days],
    queryFn: async () => {
      const results = await Promise.all(days.map((d) => fetchKpi(d)));
      return results.map((r, i) => ({
        label: days[i].slice(5),
        value: r.totalActualQtyOK,
        secondary: r.totalTargetQty,
      }));
    },
  });

  const { data: oeeData = [], isLoading: oeeLoading, refetch: refetchOee } = useQuery({
    queryKey: ['dashboard-oee', date],
    queryFn: () => fetchOee(date),
  });

  const { data: shiftOutput = [], isLoading: shiftLoading, refetch: refetchShift } = useQuery({
    queryKey: ['dashboard-shift', date],
    queryFn: () => fetchShiftOutput(date),
  });

  const { data: defectPareto = [], isLoading: paretoLoading } = useQuery({
    queryKey: ['dashboard-pareto', from30, date],
    queryFn: () => fetchDefectPareto(from30, date),
  });

  const { data: alerts, isLoading: alertsLoading, refetch: refetchAlerts } = useQuery({
    queryKey: ['dashboard-inventory-alerts'],
    queryFn: fetchInventoryAlerts,
  });

  const { data: soFulfillment, isLoading: soLoading } = useQuery({
    queryKey: ['dashboard-so', now.getFullYear(), now.getMonth() + 1],
    queryFn: () => fetchSoFulfillment(now.getFullYear(), now.getMonth() + 1),
  });

  const avgOee = oeeData.length > 0
    ? oeeData.reduce((s, m) => s + m.oee, 0) / oeeData.length
    : 0;

  function refetchAll() {
    refetchKpi();
    refetchOee();
    refetchShift();
    refetchAlerts();
  }

  const defectRate =
    kpi && kpi.totalActualQtyOK + kpi.totalActualQtyNG > 0
      ? ((kpi.totalActualQtyNG / (kpi.totalActualQtyOK + kpi.totalActualQtyNG)) * 100).toFixed(1)
      : '0.0';

  const alertBannerItems: AlertItem[] = [
    ...(alerts?.lowStockItems.map((i) => ({
      label: `Low stock: ${i.productCode} — ${i.productName}`,
      value: `${i.availableQty} / ${i.minStock} min`,
      severity: 'warning' as const,
    })) ?? []),
    ...(alerts?.expiringItems.map((i) => ({
      label: `Expiring lot: ${i.lotNumber} (${i.productCode})`,
      value: i.expiryDate,
      severity: 'error' as const,
    })) ?? []),
  ];

  const soSegments = soFulfillment
    ? [
        { label: 'On time', value: soFulfillment.onTimeCount, color: '#10B981' },
        { label: 'Overdue', value: soFulfillment.overdueCount, color: '#EF4444' },
        { label: 'Pending', value: soFulfillment.pendingCount, color: '#F59E0B' },
      ]
    : [];

  return (
    <PageRoot>
      <PageHeader
        title="Factory Overview"
        subtitle={date}
        actions={
          <Stack direction="row" sx={{ gap: 1, alignItems: 'center' }}>
            <input
              type="date"
              value={date}
              max={todayIso()}
              onChange={(e) => setDate(e.target.value)}
              style={{ padding: '6px 10px', borderRadius: 6, border: '1px solid #ccc', fontSize: 14 }}
            />
            <RefreshButton onClick={refetchAll} />
          </Stack>
        }
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <KpiCard
            label="Output Today"
            value={kpi?.totalActualQtyOK ?? 0}
            unit={`/ ${kpi?.totalTargetQty ?? 0} target`}
            icon="production"
            accentColor="#10B981"
            loading={kpiLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Box onClick={() => navigate('/production/work-orders')} sx={{ cursor: 'pointer', height: '100%' }}>
            <KpiCard
              label="Active Work Orders"
              value={kpi?.activeWorkOrders ?? 0}
              icon="workOrders"
              accentColor="#3B82F6"
              loading={kpiLoading}
            />
          </Box>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <KpiCard
            label="Avg OEE"
            value={avgOee.toFixed(1)}
            unit="%"
            icon="reports"
            accentColor={oeeZoneColor(avgOee)}
            loading={oeeLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <Box onClick={() => navigate('/production/downtime')} sx={{ cursor: 'pointer', height: '100%' }}>
            <KpiCard
              label="Open Downtime"
              value={kpi?.openDowntimeCount ?? 0}
              icon="maintenance"
              accentColor={kpi?.openDowntimeCount ? '#EF4444' : '#10B981'}
              loading={kpiLoading}
            />
          </Box>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <KpiCard
            label="Defect Rate"
            value={defectRate}
            unit="%"
            icon="quality"
            accentColor={parseFloat(defectRate) > 2 ? '#EF4444' : '#10B981'}
            loading={kpiLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 2 }}>
          <KpiCard
            label="Inventory Alerts"
            value={(kpi?.lowStockAlertCount ?? 0) + (kpi?.expiringLotCount ?? 0)}
            icon="bom"
            accentColor={(kpi?.lowStockAlertCount ?? 0) + (kpi?.expiringLotCount ?? 0) > 0 ? '#F59E0B' : '#10B981'}
            loading={kpiLoading}
          />
        </Grid>
      </Grid>

      {/* Charts row 1 */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardHeader title="Output Trend (7 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <TrendLineChart
                data={kpiTrend}
                primaryLabel="Actual OK"
                secondaryLabel="Target"
                unit="pcs"
                loading={trendLoading}
              />
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardHeader title="Shift Output (today)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <ShiftComparisonBar
                data={shiftOutput.map((s) => ({
                  shift: s.shiftName,
                  ok: s.actualQtyOK,
                  ng: s.actualQtyNG,
                }))}
                loading={shiftLoading}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts row 2 */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardHeader title="Top Defects (last 30 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <DashboardParetoChart data={defectPareto} loading={paretoLoading} />
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardHeader
              title="SO Fulfillment (this month)"
              subheader={soFulfillment ? `${soFulfillment.fulfillmentRate.toFixed(1)}% on time` : ''}
              titleTypographyProps={{ variant: 'subtitle1' }}
            />
            <CardContent sx={{ pt: 0 }}>
              {soLoading ? (
                <Skeleton variant="circular" width={200} height={200} sx={{ mx: 'auto' }} />
              ) : (
                <StatusDonut segments={soSegments} centerLabel="SOs" />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Alert banner */}
      {!alertsLoading && alertBannerItems.length > 0 && (
        <AlertBanner items={alertBannerItems} title="Inventory Alerts" />
      )}
    </PageRoot>
  );
}
