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
import { useGetApiV1ReportsQuality } from '../../api/reports/reports';
import { getErrorMessage } from '../../lib/apiClient';

function defectRateChipColor(pct: number): 'error' | 'warning' | 'success' {
  if (pct >= 5) return 'error';
  if (pct >= 2) return 'warning';
  return 'success';
}

function todayIso() { return new Date().toISOString().split('T')[0]; }
function monthAgoIso() {
  const d = new Date();
  d.setMonth(d.getMonth() - 1);
  return d.toISOString().split('T')[0];
}

export default function QualityReportPage() {
  const [from, setFrom] = useState(monthAgoIso());
  const [to, setTo] = useState(todayIso());

  const { data, isLoading, error } = useGetApiV1ReportsQuality({
    from: `${from}T00:00:00Z`,
    to: `${to}T23:59:59Z`,
  });

  const dto = data?.data;
  const rows = useMemo(
    () => [...(dto?.rows ?? [])].sort((a, b) => Number(b.totalQuantity) - Number(a.totalQuantity)),
    [dto],
  );

  const totalDefects = Number(dto?.totalDefects ?? 0);
  const maxCount = rows.length > 0 ? Number(rows[0].totalQuantity) : 1;

  if (isLoading) return <TablePageSkeleton />;

  return (
    <PageRoot>
      <PageHeader
        title="Quality Report"
        subtitle="Defect analysis by code, category, and trend"
        breadcrumbs={[{ label: 'Reports' }, { label: 'Quality' }]}
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
            label="Total Defects"
            value={totalDefects.toLocaleString()}
            unit="EA"
            icon="warning"
            accentColor="#B91C1C"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Defect Categories"
            value={new Set(rows.map((r) => r.category ?? 'Unknown')).size.toString()}
            unit="types"
            icon="quality"
            accentColor="#D97706"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Top Defect"
            value={rows[0]?.defectCode ?? '—'}
            icon="warning"
            accentColor="#7C3AED"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Top Defect Share"
            value={rows[0] ? Number(rows[0].percentage).toFixed(1) : '—'}
            unit="%"
            icon="oee"
            accentColor="#DC2626"
          />
        </Grid>
      </Grid>

      {/* ── Pareto + category summary ─────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Defect Pareto"
              subheader="Sorted by quantity descending"
              titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
              subheaderTypographyProps={{ variant: 'caption' }}
            />
            <CardContent>
              {rows.length === 0 ? (
                <Typography variant="body2" color="text.disabled" sx={{ textAlign: 'center', py: 4 }}>
                  No defects recorded in this period.
                </Typography>
              ) : (
                <Stack spacing={1.5}>
                  {rows.slice(0, 10).map((d) => (
                    <Box key={d.defectCode}>
                      <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                        <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                          <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'text.secondary', fontSize: 11 }}>
                            {d.defectCode}
                          </Typography>
                          <Typography variant="caption" sx={{ fontWeight: 500 }}>{d.defectName}</Typography>
                        </Stack>
                        <Typography variant="caption" color="text.secondary" sx={{ whiteSpace: 'nowrap', ml: 1 }}>
                          {Number(d.totalQuantity).toLocaleString()} · {Number(d.percentage).toFixed(1)}%
                        </Typography>
                      </Stack>
                      <LinearProgress
                        variant="determinate"
                        value={(Number(d.totalQuantity) / maxCount) * 100}
                        sx={{
                          height: 6,
                          borderRadius: 3,
                          bgcolor: alpha('#B91C1C', 0.12),
                          '& .MuiLinearProgress-bar': { bgcolor: '#B91C1C' },
                        }}
                      />
                    </Box>
                  ))}
                </Stack>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="By Category"
              titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
            />
            <CardContent>
              {rows.length === 0 ? (
                <Typography variant="body2" color="text.disabled" sx={{ textAlign: 'center', py: 4 }}>No data.</Typography>
              ) : (
                <Stack spacing={1}>
                  {Object.entries(
                    rows.reduce<Record<string, number>>((acc, r) => {
                      const cat = r.category ?? 'Unknown';
                      acc[cat] = (acc[cat] ?? 0) + Number(r.totalQuantity);
                      return acc;
                    }, {}),
                  )
                    .sort(([, a], [, b]) => b - a)
                    .map(([cat, qty]) => (
                      <Stack key={cat} direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                        <Typography variant="caption" sx={{ fontWeight: 500 }}>{cat}</Typography>
                        <Chip
                          label={`${qty.toLocaleString()} (${((qty / totalDefects) * 100).toFixed(1)}%)`}
                          size="small"
                          color={defectRateChipColor((qty / totalDefects) * 100)}
                          variant="outlined"
                        />
                      </Stack>
                    ))}
                </Stack>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* ── Full defect table ─────────────────────────────────────────── */}
      <Card>
        <CardHeader
          title="Defect Code Table"
          titleTypographyProps={{ variant: 'subtitle2', sx: { fontWeight: 700 } }}
        />
        <CardContent sx={{ p: '0 !important' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                  <TableCell>Defect Code</TableCell>
                  <TableCell>Defect Name</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell align="right">Quantity</TableCell>
                  <TableCell align="right">Share %</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {rows.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} sx={{ textAlign: 'center', py: 4, color: 'text.disabled' }}>
                      No defects recorded
                    </TableCell>
                  </TableRow>
                ) : (
                  rows.map((row) => (
                    <TableRow key={row.defectCode} hover>
                      <TableCell>
                        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                          {row.defectCode}
                        </Typography>
                      </TableCell>
                      <TableCell>{row.defectName}</TableCell>
                      <TableCell>
                        <Typography variant="caption" color="text.secondary">{row.category ?? '—'}</Typography>
                      </TableCell>
                      <TableCell align="right">{Number(row.totalQuantity).toLocaleString()}</TableCell>
                      <TableCell align="right">
                        <Box sx={{ display: 'inline-block', px: 1, py: 0.25, borderRadius: 1, bgcolor: alpha('#B91C1C', 0.1) }}>
                          <Typography variant="caption" sx={{ fontWeight: 700, color: '#B91C1C' }}>
                            {Number(row.percentage).toFixed(1)}%
                          </Typography>
                        </Box>
                      </TableCell>
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
