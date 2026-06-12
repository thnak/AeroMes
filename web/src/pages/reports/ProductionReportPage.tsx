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

interface ProductOutput {
  productCode: string;
  productName: string;
  output: number;
}

const MOCK_PRODUCT_OUTPUT: ProductOutput[] = [
  { productCode: 'FRM-A001', productName: 'Frame Assembly A',      output: 12_840 },
  { productCode: 'PNL-B002', productName: 'Panel Sub-assembly B',  output: 9_600  },
  { productCode: 'SHT-C003', productName: 'Shaft Housing C',       output: 7_200  },
  { productCode: 'BRK-D004', productName: 'Bracket Set D',         output: 5_800  },
  { productCode: 'MTR-E005', productName: 'Motor Mount E',         output: 4_376  },
  { productCode: 'COV-F006', productName: 'Cover Plate F',         output: 3_000  },
];

const MAX_OUTPUT = Math.max(...MOCK_PRODUCT_OUTPUT.map((p) => p.output));

interface ShiftRow {
  date: string;
  shift: 'Morning' | 'Afternoon' | 'Night';
  machine: string;
  output: number;
  scrap: number;
  oee: number;
  downtime: number;
  operator: string;
}

const MOCK_SHIFT_ROWS: ShiftRow[] = [
  { date: '2026-06-11', shift: 'Morning',   machine: 'MC-01 CNC Lathe 1',    output: 320, scrap: 4,  oee: 85.2, downtime: 12, operator: 'Nguyen Van A'   },
  { date: '2026-06-11', shift: 'Afternoon', machine: 'MC-01 CNC Lathe 1',    output: 305, scrap: 6,  oee: 81.4, downtime: 18, operator: 'Tran Thi B'     },
  { date: '2026-06-11', shift: 'Night',     machine: 'MC-01 CNC Lathe 1',    output: 280, scrap: 3,  oee: 79.6, downtime: 24, operator: 'Le Van C'       },
  { date: '2026-06-11', shift: 'Morning',   machine: 'MC-02 CNC Mill 1',     output: 410, scrap: 5,  oee: 88.1, downtime: 8,  operator: 'Pham Thi D'     },
  { date: '2026-06-11', shift: 'Afternoon', machine: 'MC-02 CNC Mill 1',     output: 395, scrap: 7,  oee: 84.3, downtime: 15, operator: 'Hoang Van E'    },
  { date: '2026-06-10', shift: 'Morning',   machine: 'MC-03 Assembly Line 1', output: 500, scrap: 8, oee: 92.0, downtime: 5,  operator: 'Vu Thi F'       },
];

// ─── Component ────────────────────────────────────────────────────────────────

export default function ProductionReportPage() {
  return (
    <PageRoot>
      <PageHeader
        title="Production Report"
        subtitle="Output, efficiency and shift performance"
        breadcrumbs={[{ label: 'Reports' }, { label: 'Production' }]}
        actions={
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <TextField
              type="date"
              size="small"
              label="From"
              defaultValue="2026-05-29"
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
            label="Total Output"
            value="42,816"
            unit="EA"
            icon="success"
            accentColor="#15803D"
            trend={3.2}
            trendLabel="vs prev week"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Planned vs Actual"
            value="94.2"
            unit="%"
            icon="oee"
            accentColor="#1D4ED8"
            trend={-0.8}
            trendLabel="vs prev week"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Scrap Rate"
            value="1.8"
            unit="%"
            icon="warning"
            accentColor="#DC2626"
            trend={-0.3}
            trendLabel="vs prev week"
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            label="Shifts Run"
            value="21"
            icon="shift"
            accentColor="#7C3AED"
          />
        </Grid>
      </Grid>

      {/* ── Charts row ───────────────────────────────────────────────── */}
      <Grid container spacing={2.5} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardHeader
              title="Output by Day"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
            />
            <CardContent>
              <Box
                sx={{
                  height: 280,
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
                  Chart: Daily Output (EA) · Last 14 days
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Output by Product"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
            />
            <CardContent>
              <Stack spacing={1.5}>
                {MOCK_PRODUCT_OUTPUT.map((p) => (
                  <Box key={p.productCode}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="caption" sx={{ fontWeight: 500, color: 'text.primary' }} noWrap>
                        {p.productCode}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {p.output.toLocaleString()}
                      </Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={(p.output / MAX_OUTPUT) * 100}
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                  </Box>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* ── Shift summary table ───────────────────────────────────────── */}
      <Card>
        <CardHeader
          title="Shift Summary"
          titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
        />
        <CardContent sx={{ p: '0 !important' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                  <TableCell>Date</TableCell>
                  <TableCell>Shift</TableCell>
                  <TableCell>Machine</TableCell>
                  <TableCell align="right">Output</TableCell>
                  <TableCell align="right">Scrap</TableCell>
                  <TableCell align="right">OEE %</TableCell>
                  <TableCell align="right">Downtime (min)</TableCell>
                  <TableCell>Operator</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {MOCK_SHIFT_ROWS.map((row, i) => (
                  <TableRow key={i} hover>
                    <TableCell>{row.date}</TableCell>
                    <TableCell>{row.shift}</TableCell>
                    <TableCell>{row.machine}</TableCell>
                    <TableCell align="right">{row.output.toLocaleString()}</TableCell>
                    <TableCell align="right">{row.scrap}</TableCell>
                    <TableCell align="right">
                      <Typography
                        variant="body2"
                        sx={{
                          fontWeight: 600,
                          color: row.oee >= 85 ? '#15803D' : row.oee >= 65 ? '#D97706' : '#B91C1C',
                        }}
                      >
                        {row.oee.toFixed(1)}%
                      </Typography>
                    </TableCell>
                    <TableCell align="right">{row.downtime}</TableCell>
                    <TableCell>{row.operator}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Box>
        </CardContent>
      </Card>
    </PageRoot>
  );
}
