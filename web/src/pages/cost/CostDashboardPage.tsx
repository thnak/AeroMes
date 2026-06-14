import {
  Box,
  Card,
  CardContent,
  Chip,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { KpiCard, PageHeader, PageRoot } from '../../components';
import { ParetoChart } from '../../components/charts';
import { apiClient } from '../../lib/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface CopqTrendPoint {
  month: string;
  scrapCost: number;
  reworkCost: number;
  warrantyCost: number;
  totalCopq: number;
}

interface ScrapParetoDto {
  productCode: string;
  totalScrapQty: number;
  totalScrapCost: number;
}

interface ReworkOrderDto {
  reworkID: number;
  reworkCode: string;
  productCode: string;
  reworkQty: number;
  status: string;
  actMaterialCost?: number;
  actLaborCost?: number;
  actMachineCost?: number;
}

// ─── API calls ────────────────────────────────────────────────────────────────

const fetchCopqTrend = (months: number): Promise<CopqTrendPoint[]> =>
  apiClient.get(`/api/v1/cost/copq-trend?months=${months}`).then((r) => r.data);

const fetchScrapPareto = (from: string, to: string): Promise<ScrapParetoDto[]> =>
  apiClient.get(`/api/v1/cost/scrap/pareto?from=${from}&to=${to}`).then((r) => r.data);

const fetchReworkOrders = (): Promise<{ items: ReworkOrderDto[]; total: number }> =>
  apiClient.get('/api/v1/cost/rework-orders?pageSize=20').then((r) => r.data);

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function CostDashboardPage() {
  const [months, setMonths] = useState(6);

  const now = new Date();
  const from = new Date(now.getFullYear(), now.getMonth() - months, 1).toISOString();
  const to = now.toISOString();

  const { data: copqTrend = [] } = useQuery({
    queryKey: ['copq-trend', months],
    queryFn: () => fetchCopqTrend(months),
  });

  const { data: scrapPareto = [] } = useQuery({
    queryKey: ['scrap-pareto', from, to],
    queryFn: () => fetchScrapPareto(from, to),
  });

  const { data: reworkData } = useQuery({
    queryKey: ['rework-orders'],
    queryFn: fetchReworkOrders,
  });

  // ── Derived KPIs ────────────────────────────────────────────────────────────
  const latestCopq = copqTrend[copqTrend.length - 1];
  const totalScrapCost = scrapPareto.reduce((s, d) => s + d.totalScrapCost, 0);
  const totalReworkCost = (reworkData?.items ?? []).reduce(
    (s, r) => s + (r.actMaterialCost ?? 0) + (r.actLaborCost ?? 0) + (r.actMachineCost ?? 0),
    0
  );
  const openReworks = (reworkData?.items ?? []).filter((r) => r.status !== 'Closed').length;

  const paretoItems = scrapPareto.map((d) => ({
    label: d.productCode,
    value: d.totalScrapQty,
  }));

  return (
    <PageRoot>
      <PageHeader
        title="Cost Management"
        subtitle="Quality cost, scrap Pareto & rework tracking"
        actions={
          <TextField
            select
            size="small"
            value={months}
            onChange={(e) => setMonths(Number(e.target.value))}
            sx={{ minWidth: 140 }}
          >
            {[3, 6, 12].map((m) => (
              <MenuItem key={m} value={m}>
                Last {m} months
              </MenuItem>
            ))}
          </TextField>
        }
      />

      {/* ── KPI Row ──────────────────────────────────────────────────────── */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            icon="quality"
            label="Total COPQ"
            value={latestCopq ? `₫ ${(latestCopq.totalCopq / 1_000_000).toFixed(1)}M` : '—'}
            unit="latest month"
            accentColor="#DC2626"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            icon="production"
            label="Scrap Cost"
            value={`₫ ${(totalScrapCost / 1_000_000).toFixed(1)}M`}
            unit="from Pareto"
            accentColor="#D97706"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            icon="maintenance"
            label="Rework Cost"
            value={`₫ ${(totalReworkCost / 1_000_000).toFixed(1)}M`}
            unit="closed orders"
            accentColor="#7C3AED"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            icon="machineOff"
            label="Open Reworks"
            value={String(openReworks)}
            unit="pending closure"
            accentColor="#1D4ED8"
          />
        </Grid>
      </Grid>

      {/* ── Scrap Pareto ─────────────────────────────────────────────────── */}
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card variant="outlined" sx={{ borderRadius: 3, height: '100%' }}>
            <CardContent>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
                Scrap Pareto — by Product Code
              </Typography>
              {paretoItems.length > 0 ? (
                <ParetoChart data={paretoItems} unit="pcs" height={300} />
              ) : (
                <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                  <Typography color="text.disabled">No scrap data for selected period</Typography>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* ── COPQ Trend Summary ──────────────────────────────────────────── */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card variant="outlined" sx={{ borderRadius: 3 }}>
            <CardContent>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>
                COPQ Trend
              </Typography>
              <Stack spacing={1.5}>
                {copqTrend.slice(-6).map((pt) => (
                  <Box key={pt.month}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                      <Typography variant="caption" color="text.secondary">
                        {pt.month}
                      </Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }} color="error.main">
                        ₫ {(pt.totalCopq / 1_000_000).toFixed(1)}M
                      </Typography>
                    </Stack>
                    <Box
                      sx={{
                        height: 4,
                        borderRadius: 1,
                        bgcolor: (t) => alpha(t.palette.error.main, 0.12),
                        mt: 0.5,
                      }}
                    >
                      <Box
                        sx={{
                          height: '100%',
                          borderRadius: 1,
                          bgcolor: 'error.main',
                          width: `${Math.min(100, (pt.totalCopq / Math.max(...copqTrend.map((x) => x.totalCopq), 1)) * 100)}%`,
                        }}
                      />
                    </Box>
                  </Box>
                ))}
                {copqTrend.length === 0 && (
                  <Typography color="text.disabled" variant="body2">
                    No COPQ data available
                  </Typography>
                )}
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* ── Open Rework Orders ───────────────────────────────────────────── */}
        <Grid size={{ xs: 12 }}>
          <Card variant="outlined" sx={{ borderRadius: 3 }}>
            <CardContent>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>
                Rework Orders
              </Typography>
              <Stack spacing={1}>
                {(reworkData?.items ?? []).length === 0 && (
                  <Typography color="text.disabled" variant="body2">
                    No rework orders found
                  </Typography>
                )}
                {(reworkData?.items ?? []).map((r) => (
                  <Box
                    key={r.reworkID}
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 2,
                      p: 1.5,
                      borderRadius: 2,
                      border: '1px solid',
                      borderColor: 'divider',
                    }}
                  >
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <Typography variant="body2" sx={{ fontWeight: 600 }} noWrap>
                        {r.reworkCode}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {r.productCode} · Qty {r.reworkQty}
                      </Typography>
                    </Box>
                    <Chip
                      label={r.status}
                      size="small"
                      sx={{
                        bgcolor: (t) =>
                          alpha(r.status === 'Closed' ? t.palette.success.main : t.palette.warning.main, 0.1),
                        color: r.status === 'Closed' ? 'success.main' : 'warning.main',
                        fontWeight: 600,
                      }}
                    />
                    {r.actMaterialCost != null && (
                      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 80, textAlign: 'right' }}>
                        ₫{' '}
                        {(
                          ((r.actMaterialCost ?? 0) + (r.actLaborCost ?? 0) + (r.actMachineCost ?? 0)) /
                          1000
                        ).toFixed(0)}
                        K
                      </Typography>
                    )}
                  </Box>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
