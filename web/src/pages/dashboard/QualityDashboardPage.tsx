import {
  Card,
  CardContent,
  CardHeader,
  Grid,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { KpiCard, PageHeader, PageRoot, RefreshButton } from '../../components';
import { DashboardParetoChart, StatusDonut, TrendLineChart } from '../../components/dashboard';
import { apiClient } from '../../lib/apiClient';

// ── Types ─────────────────────────────────────────────────────────────────────

interface FactoryKpiDto {
  totalActualQtyOK: number;
  totalActualQtyNG: number;
}

interface DefectParetoItemDto {
  defectCode: string;
  defectName: string;
  category: string;
  count: number;
  cumulativePct: number;
}

// ── API ───────────────────────────────────────────────────────────────────────

const fetchKpi = (date: string): Promise<FactoryKpiDto> =>
  apiClient.get(`/api/v1/dashboard/kpi?date=${date}`).then((r) => r.data);

const fetchPareto = (from: string, to: string): Promise<DefectParetoItemDto[]> =>
  apiClient.get(`/api/v1/dashboard/defect-pareto?from=${from}&to=${to}`).then((r) => r.data);

// ── Helpers ───────────────────────────────────────────────────────────────────

function todayIso() { return new Date().toISOString().split('T')[0]; }
function lastNDays(n: number): string[] {
  return Array.from({ length: n }, (_, i) => {
    const d = new Date();
    d.setDate(d.getDate() - (n - 1 - i));
    return d.toISOString().split('T')[0];
  });
}

const CATEGORY_COLORS: Record<string, string> = {
  Appearance: '#3B82F6',
  Dimension: '#F59E0B',
  Function: '#EF4444',
  Material: '#8B5CF6',
  Assembly: '#10B981',
  Other: '#64748B',
};

export default function QualityDashboardPage() {
  const today = todayIso();
  const days30 = lastNDays(30);
  const from30 = days30[0];

  const { data: kpiToday, isLoading: kpiLoading, refetch: refetchKpi } = useQuery({
    queryKey: ['q-kpi', today],
    queryFn: () => fetchKpi(today),
  });

  const { data: fpyTrend = [], isLoading: trendLoading } = useQuery({
    queryKey: ['q-fpy-trend', from30, today],
    queryFn: async () => {
      const results = await Promise.all(days30.map((d) => fetchKpi(d)));
      return results.map((r, i) => ({
        label: days30[i].slice(5),
        value: r.totalActualQtyOK + r.totalActualQtyNG > 0
          ? parseFloat(((r.totalActualQtyOK / (r.totalActualQtyOK + r.totalActualQtyNG)) * 100).toFixed(1))
          : 100,
      }));
    },
  });

  const { data: paretoRaw = [], isLoading: paretoLoading, refetch: refetchPareto } = useQuery({
    queryKey: ['q-pareto', from30, today],
    queryFn: () => fetchPareto(from30, today),
  });

  function refetchAll() { refetchKpi(); refetchPareto(); }

  const totalOK = kpiToday?.totalActualQtyOK ?? 0;
  const totalNG = kpiToday?.totalActualQtyNG ?? 0;
  const fpy = totalOK + totalNG > 0 ? ((totalOK / (totalOK + totalNG)) * 100).toFixed(1) : '100.0';

  // Build category donut segments from pareto
  const categoryMap = new Map<string, number>();
  paretoRaw.forEach((d) => {
    const cur = categoryMap.get(d.category) ?? 0;
    categoryMap.set(d.category, cur + d.count);
  });
  const categorySegments = [...categoryMap.entries()].map(([cat, cnt]) => ({
    label: cat,
    value: cnt,
    color: CATEGORY_COLORS[cat] ?? '#94A3B8',
  }));

  const paretoData = paretoRaw.slice(0, 10).map((d) => ({
    code: d.defectCode,
    name: d.defectName,
    count: d.count,
    cumulativePct: d.cumulativePct,
  }));

  return (
    <PageRoot>
      <PageHeader
        title="Quality Dashboard"
        actions={<RefreshButton onClick={refetchAll} />}
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard
            label="FPY Today"
            value={fpy}
            unit="%"
            icon="quality"
            accentColor={parseFloat(fpy) >= 98 ? '#10B981' : parseFloat(fpy) >= 95 ? '#F59E0B' : '#EF4444'}
            loading={kpiLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard
            label="Total NG Today"
            value={totalNG}
            unit="pcs"
            icon="quality"
            accentColor={totalNG > 0 ? '#EF4444' : '#10B981'}
            loading={kpiLoading}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <KpiCard
            label="Top Defect Types (30d)"
            value={paretoRaw.length}
            unit="codes"
            icon="quality"
            accentColor="#3B82F6"
            loading={paretoLoading}
          />
        </Grid>
      </Grid>

      {/* FPY trend + pareto */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardHeader title="FPY Trend (30 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <TrendLineChart
                data={fpyTrend}
                primaryLabel="FPY %"
                unit="%"
                loading={trendLoading}
              />
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardHeader title="Top Defects Pareto (30 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <DashboardParetoChart data={paretoData} loading={paretoLoading} />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Category donut */}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardHeader title="Defect by Category" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <StatusDonut segments={categorySegments} centerLabel="Defects" loading={paretoLoading} />
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
