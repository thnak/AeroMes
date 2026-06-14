import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  LinearProgress,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useMemo, useState } from 'react';
import {
  EmptyState,
  ExportButton,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
} from '../../components';
import { useGetApiV1ReportsQuality } from '../../api/reports/reports';
import type { QualityReportRowDto } from '../../api/model';

type Range = '7d' | '30d' | '90d';

const RANGE_DAYS: Record<Range, number> = { '7d': 7, '30d': 30, '90d': 90 };

function rangeParams(range: Range) {
  const to = new Date();
  const from = new Date(to.getTime() - RANGE_DAYS[range] * 24 * 60 * 60 * 1000);
  return {
    from: from.toISOString().split('T')[0],
    to: to.toISOString().split('T')[0],
  };
}

const CATEGORY_COLORS = ['#DC2626', '#D97706', '#7C3AED', '#1D4ED8', '#0D9488', '#15803D', '#475569'];

function numVal(v: number | string): number {
  return typeof v === 'number' ? v : parseFloat(v) || 0;
}

function KpiCard({ label, value, sub, color, icon }: { label: string; value: string; sub: string; color: string; icon: Parameters<typeof SolarIcon>[0]['name'] }) {
  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        <Stack direction="row" sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11, fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5 }}>
              {label}
            </Typography>
            <Typography variant="h5" sx={{ fontWeight: 700, mt: 0.5, color }}>
              {value}
            </Typography>
            <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11 }}>
              {sub}
            </Typography>
          </Box>
          <Box sx={{ p: 1, borderRadius: 1.5, bgcolor: alpha(color, 0.1) }}>
            <SolarIcon name={icon} size={20} color={color} />
          </Box>
        </Stack>
      </CardContent>
    </Card>
  );
}

export default function DefectAnalysisPage() {
  const [range, setRange] = useState<Range>('30d');
  const params = useMemo(() => rangeParams(range), [range]);

  const { data: resp, isLoading, refetch } = useGetApiV1ReportsQuality(params);
  const report = resp?.data;
  const rows: QualityReportRowDto[] = report?.rows ?? [];
  const totalDefects = numVal(report?.totalDefects ?? 0);

  const sorted = useMemo(() => [...rows].sort((a, b) => numVal(b.totalQuantity) - numVal(a.totalQuantity)), [rows]);
  const maxCount = sorted.length > 0 ? numVal(sorted[0].totalQuantity) : 1;
  const topDefect = sorted[0];

  const categoryMap = useMemo(() => {
    const map = new Map<string, number>();
    rows.forEach((r) => {
      const cat = r.category ?? 'Uncategorized';
      map.set(cat, (map.get(cat) ?? 0) + numVal(r.totalQuantity));
    });
    return [...map.entries()].sort((a, b) => b[1] - a[1]);
  }, [rows]);

  return (
    <PageRoot>
      <PageHeader
        title="Defect Analysis"
        subtitle="Defect trends, Pareto analysis and defect classification"
        breadcrumbs={[{ label: 'Quality' }, { label: 'Defect Analysis' }]}
        actions={
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <Stack direction="row" spacing={0.5}>
              {(['7d', '30d', '90d'] as Range[]).map((r) => (
                <Button key={r} variant={range === r ? 'contained' : 'outlined'} size="small"
                  onClick={() => setRange(r)} sx={{ minWidth: 48 }}>
                  {r}
                </Button>
              ))}
            </Stack>
            <ExportButton />
            <RefreshButton onClick={() => refetch()} />
          </Stack>
        }
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          {isLoading
            ? <Skeleton variant="rectangular" height={96} sx={{ borderRadius: 1 }} />
            : <KpiCard label="Total Defects" value={totalDefects.toLocaleString()} sub={`Last ${RANGE_DAYS[range]} days`} color="#DC2626" icon="warning" />}
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          {isLoading
            ? <Skeleton variant="rectangular" height={96} sx={{ borderRadius: 1 }} />
            : <KpiCard label="Defect Types" value={rows.length.toString()} sub="Unique defect codes" color="#D97706" icon="quality" />}
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          {isLoading
            ? <Skeleton variant="rectangular" height={96} sx={{ borderRadius: 1 }} />
            : <KpiCard label="Top Defect" value={topDefect?.defectCode ?? '—'} sub={topDefect ? `${numVal(topDefect.totalQuantity).toLocaleString()} pcs` : 'No data'} color="#7C3AED" icon="qualityOee" />}
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          {isLoading
            ? <Skeleton variant="rectangular" height={96} sx={{ borderRadius: 1 }} />
            : <KpiCard label="Categories" value={categoryMap.length.toString()} sub="Defect categories" color="#1D4ED8" icon="production" />}
        </Grid>
      </Grid>

      <Grid container spacing={2} sx={{ mb: 2 }}>
        {/* Pareto */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Top Defects (Pareto)
              </Typography>
              {isLoading ? (
                <Stack spacing={1.5}>
                  {[1, 2, 3, 4, 5].map((i) => <Skeleton key={i} height={32} sx={{ borderRadius: 1 }} />)}
                </Stack>
              ) : sorted.length === 0 ? (
                <EmptyState icon="emptyTable" title="No defects in this period" compact />
              ) : (
                <Stack spacing={1.5}>
                  {sorted.slice(0, 10).map((item) => {
                    const count = numVal(item.totalQuantity);
                    const barPct = (count / maxCount) * 100;
                    return (
                      <Box key={item.defectCode}>
                        <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 0.5 }}>
                          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', minWidth: 0 }}>
                            <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 10, color: 'text.secondary', flexShrink: 0 }}>
                              {item.defectCode}
                            </Typography>
                            <Typography variant="body2" sx={{ fontSize: 12 }} noWrap>
                              {item.defectName}
                            </Typography>
                            {item.category && (
                              <Chip label={item.category} size="small"
                                sx={{ height: 16, fontSize: '0.625rem', flexShrink: 0, '& .MuiChip-label': { px: 0.75 } }} />
                            )}
                          </Stack>
                          <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600, minWidth: 40, textAlign: 'right', flexShrink: 0 }}>
                            {count.toLocaleString()}
                          </Typography>
                        </Stack>
                        <LinearProgress variant="determinate" value={barPct}
                          sx={{
                            height: 6, borderRadius: 3,
                            bgcolor: (t) => alpha(t.palette.primary.main, 0.08),
                            '& .MuiLinearProgress-bar': { borderRadius: 3, bgcolor: '#DC2626' },
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

        {/* Category breakdown */}
        <Grid size={{ xs: 12, md: 5 }}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
                Defects by Category
              </Typography>
              {isLoading ? (
                <Stack spacing={1}>
                  {[1, 2, 3, 4].map((i) => <Skeleton key={i} height={52} sx={{ borderRadius: 1 }} />)}
                </Stack>
              ) : categoryMap.length === 0 ? (
                <EmptyState icon="emptyTable" title="No data" compact />
              ) : (
                <Stack spacing={1}>
                  {categoryMap.map(([cat, count], idx) => {
                    const color = CATEGORY_COLORS[idx % CATEGORY_COLORS.length];
                    const pct = totalDefects > 0 ? (count / totalDefects) * 100 : 0;
                    return (
                      <Box key={cat}
                        sx={{
                          p: 1.5, borderRadius: 1.5, border: '1px solid', borderColor: 'divider',
                          borderLeft: '4px solid', borderLeftColor: color, bgcolor: alpha(color, 0.03),
                        }}
                      >
                        <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'baseline', mb: 0.5 }}>
                          <Typography variant="caption" color="text.secondary"
                            sx={{ fontSize: 11, fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5 }}>
                            {cat}
                          </Typography>
                          <Stack direction="row" spacing={0.75} sx={{ alignItems: 'baseline' }}>
                            <Typography variant="body2" sx={{ fontWeight: 700, color }}>{count.toLocaleString()}</Typography>
                            <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11 }}>{pct.toFixed(1)}%</Typography>
                          </Stack>
                        </Stack>
                        <LinearProgress variant="determinate" value={pct}
                          sx={{
                            height: 4, borderRadius: 2,
                            bgcolor: alpha(color, 0.1),
                            '& .MuiLinearProgress-bar': { borderRadius: 2, bgcolor: color },
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
      </Grid>

      {/* Defect detail table */}
      <Card variant="outlined">
        <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
          <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1.5 }}>
            Defect Summary
          </Typography>
          {isLoading ? (
            <Stack spacing={1}>{[1, 2, 3, 4, 5].map((i) => <Skeleton key={i} height={32} />)}</Stack>
          ) : sorted.length === 0 ? (
            <EmptyState icon="emptyTable" title="No defects recorded in this period" compact />
          ) : (
            <Box sx={{ overflowX: 'auto' }}>
              <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
                <Box component="thead">
                  <Box component="tr" sx={{ borderBottom: '2px solid', borderColor: 'divider' }}>
                    {['Code', 'Defect Name', 'Category', 'Qty', '% of Total'].map((h) => (
                      <Box key={h} component="th"
                        sx={{
                          textAlign: 'left', py: 0.75, px: 1, fontSize: 11, fontWeight: 600,
                          color: 'text.secondary', textTransform: 'uppercase', letterSpacing: 0.4,
                          bgcolor: (t) => alpha(t.palette.primary.main, 0.04),
                          whiteSpace: 'nowrap',
                        }}>
                        {h}
                      </Box>
                    ))}
                  </Box>
                </Box>
                <Box component="tbody">
                  {sorted.map((row, i) => {
                    const count = numVal(row.totalQuantity);
                    const pct = numVal(row.percentage);
                    return (
                      <Box key={row.defectCode} component="tr"
                        sx={{
                          borderBottom: '1px solid', borderColor: 'divider',
                          bgcolor: i % 2 === 1 ? (t) => alpha(t.palette.primary.main, 0.01) : 'transparent',
                          '&:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.03) },
                        }}>
                        <Box component="td" sx={{ py: 0.75, px: 1, fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'primary.main', whiteSpace: 'nowrap' }}>
                          {row.defectCode}
                        </Box>
                        <Box component="td" sx={{ py: 0.75, px: 1 }}>{row.defectName}</Box>
                        <Box component="td" sx={{ py: 0.75, px: 1 }}>
                          {row.category
                            ? <Chip label={row.category} size="small" sx={{ height: 18, fontSize: '0.625rem', '& .MuiChip-label': { px: 0.75 } }} />
                            : <Typography variant="caption" color="text.disabled">—</Typography>}
                        </Box>
                        <Box component="td" sx={{ py: 0.75, px: 1, fontWeight: 700, color: '#DC2626', textAlign: 'right' }}>
                          {count.toLocaleString()}
                        </Box>
                        <Box component="td" sx={{ py: 0.75, px: 1, textAlign: 'right', color: 'text.secondary' }}>
                          {pct.toFixed(1)}%
                        </Box>
                      </Box>
                    );
                  })}
                </Box>
              </Box>
            </Box>
          )}
        </CardContent>
      </Card>
    </PageRoot>
  );
}
