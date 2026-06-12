import {
  Box,
  Card,
  CardContent,
  CardHeader,
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
import {
  ExportButton,
  KpiCard,
  PageHeader,
  PageRoot,
} from '../../components';

// ─── Mock data ────────────────────────────────────────────────────────────────

interface DefectPareto {
  code: string;
  name: string;
  count: number;
  pct: number;
}

const MOCK_DEFECT_PARETO: DefectPareto[] = [
  { code: 'DC-003', name: 'Dimensional Out-of-spec',   count: 198, pct: 31.5 },
  { code: 'DC-007', name: 'Surface Scratch',           count: 142, pct: 22.6 },
  { code: 'DC-001', name: 'Burr on Edge',              count: 98,  pct: 15.6 },
  { code: 'DC-012', name: 'Weld Porosity',             count: 76,  pct: 12.1 },
  { code: 'DC-005', name: 'Wrong Material Marking',    count: 62,  pct:  9.9 },
  { code: 'DC-009', name: 'Delamination',              count: 52,  pct:  8.3 },
];

const MAX_DEFECT_COUNT = Math.max(...MOCK_DEFECT_PARETO.map((d) => d.count));

interface ProductQuality {
  productCode: string;
  productName: string;
  inspected: number;
  defects: number;
  defectRate: number;
  topDefect: string;
}

const MOCK_PRODUCT_QUALITY: ProductQuality[] = [
  { productCode: 'FRM-A001', productName: 'Frame Assembly A',      inspected: 12_800, defects: 192,  defectRate: 1.50, topDefect: 'DC-003 Dimensional'     },
  { productCode: 'PNL-B002', productName: 'Panel Sub-assembly B',  inspected: 9_600,  defects: 58,   defectRate: 0.60, topDefect: 'DC-007 Surface Scratch'  },
  { productCode: 'SHT-C003', productName: 'Shaft Housing C',       inspected: 7_200,  defects: 252,  defectRate: 3.50, topDefect: 'DC-003 Dimensional'     },
  { productCode: 'BRK-D004', productName: 'Bracket Set D',         inspected: 5_800,  defects: 52,   defectRate: 0.90, topDefect: 'DC-001 Burr on Edge'    },
  { productCode: 'MTR-E005', productName: 'Motor Mount E',         inspected: 4_376,  defects: 70,   defectRate: 1.60, topDefect: 'DC-012 Weld Porosity'   },
  { productCode: 'COV-F006', productName: 'Cover Plate F',         inspected: 8_514,  defects: 4,    defectRate: 0.05, topDefect: 'DC-007 Surface Scratch'  },
];

function defectRateColor(rate: number): string {
  if (rate < 1.0) return '#15803D';
  if (rate < 3.0) return '#D97706';
  return '#B91C1C';
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function QualityReportPage() {
  return (
    <PageRoot>
      <PageHeader
        title="Quality Report"
        subtitle="Defect rates, top defects and inspection results"
        breadcrumbs={[{ label: 'Reports' }, { label: 'Quality' }]}
        actions={
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <TextField
              type="date"
              size="small"
              label="From"
              defaultValue="2026-05-01"
              sx={{ width: 150 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              type="date"
              size="small"
              label="To"
              defaultValue="2026-06-11"
              sx={{ width: 150 }}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <ExportButton />
          </Stack>
        }
      />

      {/* ── KPI row ──────────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Defect Rate"
            value="1.3"
            unit="%"
            icon="warning"
            accentColor="#D97706"
            trend={-0.2}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Total Inspected"
            value="48,290"
            unit="EA"
            icon="success"
            accentColor="#1D4ED8"
            trend={2.1}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Total Defects"
            value="628"
            unit="EA"
            icon="quality"
            accentColor="#B91C1C"
            trend={-4.3}
            trendLabel="vs prev period"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="First Pass Yield"
            value="98.7"
            unit="%"
            icon="oee"
            accentColor="#15803D"
            trend={0.2}
            trendLabel="vs prev period"
          />
        </Grid>
      </Grid>

      {/* ── Charts row ───────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardHeader
              title="Defect Rate Trend"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
            />
            <CardContent>
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
                  Chart: Defect Rate % by day · Last 30 days
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Top Defects"
              subheader="Pareto"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
              subheaderTypographyProps={{ variant: 'caption' }}
            />
            <CardContent>
              <Stack spacing={1.5}>
                {MOCK_DEFECT_PARETO.map((d) => (
                  <Box key={d.code}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                      <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600, color: 'text.secondary', fontSize: 11 }}>
                          {d.code}
                        </Typography>
                        <Typography variant="caption" sx={{ fontWeight: 500 }}>{d.name}</Typography>
                      </Stack>
                      <Typography variant="caption" color="text.secondary" sx={{ whiteSpace: 'nowrap', ml: 1 }}>
                        {d.count} · {d.pct}%
                      </Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={(d.count / MAX_DEFECT_COUNT) * 100}
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
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* ── Defects by product table ──────────────────────────────────── */}
      <Card>
        <CardHeader
          title="Defects by Product"
          titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
        />
        <CardContent sx={{ p: '0 !important' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                  <TableCell>Product Code</TableCell>
                  <TableCell>Product Name</TableCell>
                  <TableCell align="right">Inspected</TableCell>
                  <TableCell align="right">Defects</TableCell>
                  <TableCell align="right">Defect Rate %</TableCell>
                  <TableCell>Top Defect</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {MOCK_PRODUCT_QUALITY.map((row) => {
                  const rateColor = defectRateColor(row.defectRate);
                  return (
                    <TableRow key={row.productCode} hover>
                      <TableCell>
                        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                          {row.productCode}
                        </Typography>
                      </TableCell>
                      <TableCell>{row.productName}</TableCell>
                      <TableCell align="right">{row.inspected.toLocaleString()}</TableCell>
                      <TableCell align="right">{row.defects.toLocaleString()}</TableCell>
                      <TableCell align="right">
                        <Box
                          sx={{
                            display: 'inline-block',
                            px: 1,
                            py: 0.25,
                            borderRadius: 1,
                            bgcolor: alpha(rateColor, 0.1),
                          }}
                        >
                          <Typography variant="caption" sx={{ fontWeight: 700, color: rateColor }}>
                            {row.defectRate.toFixed(2)}%
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption" color="text.secondary">{row.topDefect}</Typography>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </Box>
        </CardContent>
      </Card>
    </PageRoot>
  );
}
