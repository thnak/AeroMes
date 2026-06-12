import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Grid,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState } from 'react';
import {
  ExportButton,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
} from '../../components';

// ─── Mock data ────────────────────────────────────────────────────────────────

const KPI_DATA = [
  { label: 'Total Defects',  value: '628',     sub: 'Last 30 days',    color: '#DC2626', icon: 'defect' as const },
  { label: 'Defect Rate',    value: '1.3%',    sub: 'vs 1.8% prev',    color: '#D97706', icon: 'quality' as const },
  { label: 'Top Defect',     value: 'Scratch', sub: '198 occurrences', color: '#7C3AED', icon: 'qualityOee' as const },
  { label: 'Top Product',    value: 'FRM-A001',sub: '312 defects',     color: '#1D4ED8', icon: 'production' as const },
];

const PARETO_DATA = [
  { name: 'Surface Scratch',  code: 'DC-001', count: 198, pct: 100 },
  { name: 'Dimension OOT',    code: 'DC-002', count: 142, pct: 71.7 },
  { name: 'Weld Porosity',    code: 'DC-003', count: 89,  pct: 44.9 },
  { name: 'Missing Treatment',code: 'DC-004', count: 76,  pct: 38.4 },
  { name: 'Delamination',     code: 'DC-008', count: 68,  pct: 34.3 },
  { name: 'Hole Misalignment',code: 'DC-005', count: 55,  pct: 27.8 },
];

const CATEGORY_DATA = [
  { label: 'Visual',      count: 284, pct: 45.2, color: '#DC2626' },
  { label: 'Dimensional', count: 198, pct: 31.5, color: '#D97706' },
  { label: 'Process',     count: 89,  pct: 14.2, color: '#7C3AED' },
  { label: 'Material',    count: 42,  pct: 6.7,  color: '#1D4ED8' },
  { label: 'Functional',  count: 15,  pct: 2.4,  color: '#0D9488' },
];

interface DefectLogRow {
  id: string;
  date: string;
  productCode: string;
  lotNo: string;
  defectCode: string;
  defectName: string;
  qty: number;
  operator: string;
  disposition: string;
}

const DEFECT_LOG: DefectLogRow[] = [
  { id: '1', date: '2026-06-10', productCode: 'FRM-A001', lotNo: 'LOT-A1042', defectCode: 'DC-001', defectName: 'Surface Scratch',   qty: 3,  operator: 'Nguyen Van A', disposition: 'Rework' },
  { id: '2', date: '2026-06-09', productCode: 'SHT-C003', lotNo: 'LOT-C0312', defectCode: 'DC-002', defectName: 'Dimension OOT',     qty: 5,  operator: 'Le Van C',    disposition: 'Scrap' },
  { id: '3', date: '2026-06-09', productCode: 'PNL-B002', lotNo: 'LOT-B0891', defectCode: 'DC-008', defectName: 'Delamination',       qty: 2,  operator: 'Tran Thi B',  disposition: 'Under Review' },
  { id: '4', date: '2026-06-08', productCode: 'FRM-A001', lotNo: 'LOT-A1038', defectCode: 'DC-003', defectName: 'Weld Porosity',      qty: 3,  operator: 'Le Van C',    disposition: 'Under Review' },
  { id: '5', date: '2026-06-07', productCode: 'WHL-L012', lotNo: 'LOT-L0045', defectCode: 'DC-002', defectName: 'Dimension OOT',     qty: 4,  operator: 'Tran Thi B',  disposition: 'Rework' },
  { id: '6', date: '2026-06-06', productCode: 'HNG-J010', lotNo: 'LOT-J0218', defectCode: 'DC-007', defectName: 'Torque NC',          qty: 15, operator: 'Nguyen Van A', disposition: 'Under Review' },
  { id: '7', date: '2026-06-05', productCode: 'BRK-D004', lotNo: 'LOT-D2201', defectCode: 'DC-004', defectName: 'Missing Treatment',  qty: 8,  operator: 'Pham Thi D',  disposition: 'Rework' },
  { id: '8', date: '2026-06-04', productCode: 'MTR-E005', lotNo: 'LOT-E0091', defectCode: 'DC-005', defectName: 'Hole Misalignment',  qty: 8,  operator: 'Pham Thi D',  disposition: 'Scrap' },
];

// ─── KPI card ─────────────────────────────────────────────────────────────────

function KpiCard({ label, value, sub, color, icon }: { label: string; value: string; sub: string; color: string; icon: string }) {
  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        <Stack direction="row" alignItems="flex-start" justifyContent="space-between">
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
            <SolarIcon name={icon as Parameters<typeof SolarIcon>[0]['name']} size={20} color={color} />
          </Box>
        </Stack>
      </CardContent>
    </Card>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function DefectAnalysisPage() {
  const [_dateRange, setDateRange] = useState('30d');

  return (
    <PageRoot>
      <PageHeader
        title="Defect Analysis"
        subtitle="Defect trends, Pareto analysis and defect classification"
        breadcrumbs={[{ label: 'Quality' }, { label: 'Defect Analysis' }]}
        actions={
          <Stack direction="row" spacing={1} alignItems="center">
            <Stack direction="row" spacing={0.5}>
              {(['7d', '30d', '90d'] as const).map((r) => (
                <Button
                  key={r}
                  variant={_dateRange === r ? 'contained' : 'outlined'}
                  size="small"
                  onClick={() => setDateRange(r)}
                  sx={{ minWidth: 48 }}
                >
                  {r}
                </Button>
              ))}
            </Stack>
            <ExportButton />
            <RefreshButton />
          </Stack>
        }
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        {KPI_DATA.map((kpi) => (
          <Grid key={kpi.label} size={{ xs: 12, sm: 6, md: 3 }}>
            <KpiCard {...kpi} />
          </Grid>
        ))}
      </Grid>

      {/* Charts row */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        {/* Defect Trend chart */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
              <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1.5 }}>
                Defect Trend
              </Typography>
              <Box
                sx={{
                  height: 260,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: (t) => alpha(t.palette.primary.main, 0.03),
                  borderRadius: 1,
                  border: '1px dashed',
                  borderColor: 'divider',
                }}
              >
                <Typography variant="body2" color="text.disabled">
                  Chart: Defect Trend · Last 30 days
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Pareto */}
        <Grid size={{ xs: 12, md: 5 }}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
              <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1.5 }}>
                Top Defects (Pareto)
              </Typography>
              <Stack spacing={1.5}>
                {PARETO_DATA.map((item) => (
                  <Box key={item.code}>
                    <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 0.5 }}>
                      <Stack direction="row" spacing={1} alignItems="center">
                        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 10, color: 'text.secondary' }}>
                          {item.code}
                        </Typography>
                        <Typography variant="body2" sx={{ fontSize: 12 }}>
                          {item.name}
                        </Typography>
                      </Stack>
                      <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600, minWidth: 32, textAlign: 'right' }}>
                        {item.count}
                      </Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={item.pct}
                      sx={{
                        height: 6,
                        borderRadius: 3,
                        bgcolor: (t) => alpha(t.palette.primary.main, 0.08),
                        '& .MuiLinearProgress-bar': { borderRadius: 3, bgcolor: '#DC2626' },
                      }}
                    />
                  </Box>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Defects by Category */}
      <Card variant="outlined" sx={{ mb: 2 }}>
        <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
          <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 2 }}>
            Defects by Category
          </Typography>
          <Grid container spacing={2}>
            {CATEGORY_DATA.map((cat) => (
              <Grid key={cat.label} size={{ xs: 12, sm: 6, md: 'auto' }} sx={{ flex: '1 1 0' }}>
                <Box
                  sx={{
                    p: 1.5,
                    borderRadius: 1.5,
                    border: '1px solid',
                    borderColor: 'divider',
                    borderLeft: '4px solid',
                    borderLeftColor: cat.color,
                    bgcolor: alpha(cat.color, 0.03),
                  }}
                >
                  <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11, fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5 }}>
                    {cat.label}
                  </Typography>
                  <Stack direction="row" alignItems="baseline" spacing={0.75} sx={{ mt: 0.5 }}>
                    <Typography variant="h6" sx={{ fontWeight: 700, color: cat.color }}>
                      {cat.count}
                    </Typography>
                    <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11 }}>
                      {cat.pct}%
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={cat.pct}
                    sx={{
                      mt: 1, height: 4, borderRadius: 2,
                      bgcolor: alpha(cat.color, 0.1),
                      '& .MuiLinearProgress-bar': { borderRadius: 2, bgcolor: cat.color },
                    }}
                  />
                </Box>
              </Grid>
            ))}
          </Grid>
        </CardContent>
      </Card>

      {/* Recent Defect Log */}
      <Card variant="outlined">
        <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
          <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1.5 }}>
            Recent Defect Log
          </Typography>
          <Box sx={{ overflowX: 'auto' }}>
            <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
              <Box component="thead">
                <Box component="tr" sx={{ borderBottom: '2px solid', borderColor: 'divider' }}>
                  {['Date', 'Product', 'Lot', 'Defect Code', 'Defect Name', 'Qty', 'Operator', 'Disposition'].map((h) => (
                    <Box
                      key={h}
                      component="th"
                      sx={{
                        textAlign: 'left', py: 0.75, px: 1, fontSize: 11, fontWeight: 600,
                        color: 'text.secondary', textTransform: 'uppercase', letterSpacing: 0.4,
                        bgcolor: (t) => alpha(t.palette.primary.main, 0.04),
                        whiteSpace: 'nowrap',
                      }}
                    >
                      {h}
                    </Box>
                  ))}
                </Box>
              </Box>
              <Box component="tbody">
                {DEFECT_LOG.map((row, i) => (
                  <Box
                    key={row.id}
                    component="tr"
                    sx={{
                      borderBottom: '1px solid',
                      borderColor: 'divider',
                      bgcolor: i % 2 === 1 ? (t) => alpha(t.palette.primary.main, 0.01) : 'transparent',
                      '&:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.03) },
                    }}
                  >
                    <Box component="td" sx={{ py: 0.75, px: 1, color: 'text.secondary', whiteSpace: 'nowrap' }}>
                      {row.date}
                    </Box>
                    <Box component="td" sx={{ py: 0.75, px: 1, fontFamily: 'ui-monospace, monospace', fontWeight: 600, color: 'primary.main', whiteSpace: 'nowrap' }}>
                      {row.productCode}
                    </Box>
                    <Box component="td" sx={{ py: 0.75, px: 1, fontFamily: 'ui-monospace, monospace', fontSize: 11, color: 'text.secondary', whiteSpace: 'nowrap' }}>
                      {row.lotNo}
                    </Box>
                    <Box component="td" sx={{ py: 0.75, px: 1, fontFamily: 'ui-monospace, monospace', fontSize: 11, whiteSpace: 'nowrap' }}>
                      {row.defectCode}
                    </Box>
                    <Box component="td" sx={{ py: 0.75, px: 1, whiteSpace: 'nowrap' }}>
                      {row.defectName}
                    </Box>
                    <Box component="td" sx={{ py: 0.75, px: 1, fontWeight: 700, color: '#DC2626', textAlign: 'center' }}>
                      {row.qty}
                    </Box>
                    <Box component="td" sx={{ py: 0.75, px: 1, whiteSpace: 'nowrap' }}>
                      {row.operator}
                    </Box>
                    <Box component="td" sx={{ py: 0.75, px: 1, whiteSpace: 'nowrap' }}>
                      <Chip
                        label={row.disposition}
                        size="small"
                        sx={{
                          height: 18,
                          fontSize: '0.625rem',
                          fontWeight: 600,
                          bgcolor: row.disposition === 'Scrap'
                            ? alpha('#DC2626', 0.1)
                            : row.disposition === 'Rework'
                              ? alpha('#D97706', 0.1)
                              : alpha('#94A3B8', 0.1),
                          color: row.disposition === 'Scrap'
                            ? '#DC2626'
                            : row.disposition === 'Rework'
                              ? '#D97706'
                              : '#64748B',
                          border: 'none',
                          '& .MuiChip-label': { px: 0.75 },
                        }}
                      />
                    </Box>
                  </Box>
                ))}
              </Box>
            </Box>
          </Box>

          <Divider sx={{ mt: 1.5, mb: 1 }} />
          <Stack direction="row" justifyContent="flex-end">
            <Button size="small" endIcon={<SolarIcon name="view" size={14} />} sx={{ fontSize: 12 }}>
              View All Defects
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </PageRoot>
  );
}
