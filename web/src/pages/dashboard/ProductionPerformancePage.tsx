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
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { PageHeader, PageRoot, RefreshButton } from '../../components';
import { OeeGauge, ShiftComparisonBar, TrendLineChart } from '../../components/dashboard';
import { apiClient } from '../../lib/apiClient';

// ── Types ─────────────────────────────────────────────────────────────────────

interface OeeByMachineDto {
  machineCode: string;
  availabilityPct: number;
  performancePct: number;
  qualityPct: number;
  oeePct: number;
}

interface ShiftOutputDto {
  shiftCode: string;
  qtyOK: number;
  qtyNG: number;
}

interface FactoryKpiDto {
  totalActualQtyOK: number;
  totalTargetQty: number;
}

interface WorkOrderListItem {
  id: number;
  woCode: string;
  productCode: string;
  targetQuantity: number;
  actualQtyOK: number;
  actualQtyNG: number;
  status: string;
  actualStartDate?: string;
  actualEndDate?: string;
}

// ── API ───────────────────────────────────────────────────────────────────────

const fetchOee = (from: string, to: string): Promise<OeeByMachineDto[]> =>
  apiClient.get(`/api/v1/dashboard/oee?from=${from}&to=${to}`).then((r) => r.data);

const fetchShift = (date: string): Promise<ShiftOutputDto[]> =>
  apiClient.get(`/api/v1/dashboard/shift-output?date=${date}`).then((r) => r.data);

const fetchKpi = (date: string): Promise<FactoryKpiDto> =>
  apiClient.get(`/api/v1/dashboard/kpi?date=${date}`).then((r) => r.data);

const fetchWorkOrders = (): Promise<{ items: WorkOrderListItem[] }> =>
  apiClient.get('/api/v1/work-orders?pageSize=50&status=Released,InProgress').then((r) => r.data);

// ── Helpers ───────────────────────────────────────────────────────────────────

function todayIso() { return new Date().toISOString().split('T')[0]; }
function lastNDays(n: number): string[] {
  return Array.from({ length: n }, (_, i) => {
    const d = new Date();
    d.setDate(d.getDate() - (n - 1 - i));
    return d.toISOString().split('T')[0];
  });
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function ProductionPerformancePage() {
  const today = todayIso();
  const days7 = lastNDays(7);
  const fromDate = days7[0];

  const { data: oeeList = [], isLoading: oeeLoading, refetch: refetchOee } = useQuery({
    queryKey: ['dash-oee', fromDate, today],
    queryFn: () => fetchOee(fromDate, today),
  });

  const { data: shiftData = [], isLoading: shiftLoading, refetch: refetchShift } = useQuery({
    queryKey: ['dash-shift', today],
    queryFn: () => fetchShift(today),
  });

  const { data: kpiTrend = [], isLoading: trendLoading } = useQuery({
    queryKey: ['dash-prod-trend', ...days7],
    queryFn: async () => {
      const results = await Promise.all(days7.map((d) => fetchKpi(d)));
      return results.map((r, i) => ({
        label: days7[i].slice(5),
        value: r.totalActualQtyOK,
        secondary: r.totalTargetQty,
      }));
    },
  });

  const { data: woData, isLoading: woLoading, refetch: refetchWo } = useQuery({
    queryKey: ['dash-wo-list'],
    queryFn: fetchWorkOrders,
  });

  function refetchAll() { refetchOee(); refetchShift(); refetchWo(); }

  const workOrders = woData?.items ?? [];

  return (
    <PageRoot>
      <PageHeader
        title="Production Performance"
        actions={<RefreshButton onClick={refetchAll} />}
      />

      {/* OEE gauge strip */}
      <Typography variant="subtitle2" sx={{ mb: 1.5, fontWeight: 600 }}>OEE per Machine</Typography>
      {oeeLoading ? (
        <LinearProgress sx={{ mb: 3 }} />
      ) : oeeList.length === 0 ? (
        <Typography color="text.disabled" sx={{ mb: 3 }}>No OEE data for this period.</Typography>
      ) : (
        <Box sx={{ display: 'flex', gap: 2, overflowX: 'auto', pb: 1, mb: 3 }}>
          {oeeList.map((m) => (
            <Card key={m.machineCode} sx={{ minWidth: 200, flexShrink: 0 }}>
              <CardContent>
                <OeeGauge
                  oee={m.oeePct}
                  availability={m.availabilityPct}
                  performance={m.performancePct}
                  quality={m.qualityPct}
                  machineCode={m.machineCode}
                />
              </CardContent>
            </Card>
          ))}
        </Box>
      )}

      {/* Charts */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardHeader title="Daily Output vs Plan (7 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
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
            <CardHeader title="Shift Comparison (today)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <ShiftComparisonBar
                data={shiftData.map((s) => ({ shift: s.shiftCode, ok: s.qtyOK, ng: s.qtyNG }))}
                loading={shiftLoading}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* WO table */}
      <Card>
        <CardHeader title="Work Order Progress" titleTypographyProps={{ variant: 'subtitle1' }} />
        <CardContent sx={{ pt: 0 }}>
          {woLoading ? (
            <LinearProgress />
          ) : workOrders.length === 0 ? (
            <Typography color="text.disabled" align="center" sx={{ py: 2 }}>No active work orders</Typography>
          ) : (
            <Box sx={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid rgba(0,0,0,0.12)' }}>
                    {['WO Code', 'Product', 'Target', 'Actual OK', 'NG', '% Done', 'Status'].map((h) => (
                      <th key={h} style={{ textAlign: 'left', padding: '8px 12px', fontWeight: 600, color: '#64748B' }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {workOrders.map((wo) => {
                    const pct = wo.targetQuantity > 0 ? (wo.actualQtyOK / wo.targetQuantity) * 100 : 0;
                    return (
                      <tr key={wo.id} style={{ borderBottom: '1px solid rgba(0,0,0,0.06)' }}>
                        <td style={{ padding: '8px 12px' }}>
                          <Link to={`/production/work-orders/${wo.id}`} style={{ color: 'inherit', textDecoration: 'none', fontWeight: 600 }}>
                            {wo.woCode}
                          </Link>
                        </td>
                        <td style={{ padding: '8px 12px' }}>{wo.productCode}</td>
                        <td style={{ padding: '8px 12px' }}>{wo.targetQuantity.toLocaleString()}</td>
                        <td style={{ padding: '8px 12px', color: '#10B981', fontWeight: 600 }}>{wo.actualQtyOK.toLocaleString()}</td>
                        <td style={{ padding: '8px 12px', color: wo.actualQtyNG > 0 ? '#EF4444' : undefined }}>{wo.actualQtyNG}</td>
                        <td style={{ padding: '8px 12px' }}>
                          <Stack direction="row" sx={{ alignItems: 'center', gap: 1 }}>
                            <LinearProgress
                              variant="determinate"
                              value={Math.min(100, pct)}
                              sx={{ flex: 1, height: 6, borderRadius: 3 }}
                            />
                            <Typography variant="caption" sx={{ minWidth: 38 }}>{pct.toFixed(0)}%</Typography>
                          </Stack>
                        </td>
                        <td style={{ padding: '8px 12px' }}>
                          <Chip label={wo.status} size="small" />
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </Box>
          )}
        </CardContent>
      </Card>
    </PageRoot>
  );
}
