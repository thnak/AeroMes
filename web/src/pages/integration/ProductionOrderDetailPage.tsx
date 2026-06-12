import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Divider,
  Grid,
  LinearProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useParams } from 'react-router-dom';
import {
  PageHeader,
  PageRoot,
  SolarIcon,
} from '../../components';

// ─── Types ────────────────────────────────────────────────────────────────────

interface BomComponent {
  componentCode: string;
  componentName: string;
  requiredQty: number;
  availableQty: number;
  uom: string;
}

interface LinkedWorkOrder {
  woNo: string;
  status: 'Released' | 'In Progress' | 'Completed';
  progress: number;
  actualQty: number;
  plannedQty: number;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_BOM: BomComponent[] = [
  { componentCode: 'ALU-2024-H',  componentName: 'Aluminium Sheet 2024-H',   requiredQty: 12,   availableQty: 15,  uom: 'SHT' },
  { componentCode: 'RIVET-4MM',   componentName: 'Rivet 4mm (Aerospace)',    requiredQty: 2400, availableQty: 1800,uom: 'EA'  },
  { componentCode: 'SEAL-BK-01',  componentName: 'Sealant Bead Kit 01',      requiredQty: 8,    availableQty: 10,  uom: 'KIT' },
  { componentCode: 'BOLT-M6-TI',  componentName: 'Titanium Bolt M6x20',      requiredQty: 960,  availableQty: 400, uom: 'EA'  },
  { componentCode: 'FILM-ADHSV',  componentName: 'Structural Adhesive Film', requiredQty: 4,    availableQty: 6,   uom: 'ROLL'},
];

const MOCK_WORK_ORDERS: LinkedWorkOrder[] = [
  { woNo: 'WO-2026-1042', status: 'In Progress', progress: 62, actualQty: 248, plannedQty: 400 },
];

const STATUS_COLORS: Record<string, string> = {
  'Planned':     '#94A3B8',
  'Released':    '#1D4ED8',
  'In Progress': '#D97706',
  'Completed':   '#15803D',
  'Cancelled':   '#B91C1C',
};

const PRIORITY_COLORS: Record<number, string> = {
  1: '#DC2626',
  2: '#D97706',
  3: '#1D4ED8',
};

// ─── Sub-components ───────────────────────────────────────────────────────────

function InfoRow({ label, value, mono = false }: { label: string; value: React.ReactNode; mono?: boolean }) {
  return (
    <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', py: 0.75 }}>
      <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 500, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
        {label}
      </Typography>
      {typeof value === 'string' || typeof value === 'number' ? (
        <Typography
          variant="body2"
          sx={{ fontWeight: 500, fontFamily: mono ? 'ui-monospace, monospace' : undefined, fontSize: mono ? 12 : undefined }}
        >
          {value}
        </Typography>
      ) : (
        value
      )}
    </Stack>
  );
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function ProductionOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const poNo = id === '1' ? 'PO-2026-0045' : `PO-2026-00${id ?? '45'}`;

  return (
    <PageRoot>
      <PageHeader
        title={poNo}
        subtitle="Frame Assembly A · 400 EA"
        breadcrumbs={[
          { label: 'Integration', href: '/integration' },
          { label: 'Production Orders', href: '/integration/production-orders' },
          { label: poNo },
        ]}
        actions={
          <Tooltip title="Work order already exists for this production order">
            <span>
              <Button
                variant="contained"
                startIcon={<SolarIcon name="add" size={16} />}
                disabled
              >
                Create Work Order
              </Button>
            </span>
          </Tooltip>
        }
      />

      <Grid container spacing={2.5}>
        {/* ── Order info card ──────────────────────────────────────── */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Order Info"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
              sx={{ pb: 0 }}
            />
            <CardContent>
              <InfoRow label="PO #" value={poNo} mono />
              <InfoRow
                label="SO #"
                value={
                  <Typography
                    variant="caption"
                    sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600, color: 'primary.main', cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }}
                  >
                    SO-2026-0091
                  </Typography>
                }
              />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Product Code" value="FRM-A001" mono />
              <InfoRow label="Product Name" value="Frame Assembly A" />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Ordered Qty"   value="400 EA" />
              <InfoRow label="Confirmed Qty" value="400 EA" />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow
                label="Priority"
                value={
                  <Chip
                    label="P1"
                    size="small"
                    sx={{ bgcolor: alpha(PRIORITY_COLORS[1], 0.12), color: PRIORITY_COLORS[1], fontWeight: 700, fontSize: 11, minWidth: 36 }}
                  />
                }
              />
              <InfoRow
                label="Status"
                value={
                  <Chip
                    label="In Progress"
                    size="small"
                    sx={{ bgcolor: alpha(STATUS_COLORS['In Progress'], 0.12), color: STATUS_COLORS['In Progress'], fontWeight: 600, fontSize: 11 }}
                  />
                }
              />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Planned Start" value="2026-05-10" />
              <InfoRow label="Planned End"   value="2026-06-28" />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Synced from ERP" value="2026-06-12 08:14" />
            </CardContent>
          </Card>
        </Grid>

        {/* ── Right column ─────────────────────────────────────────── */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Stack spacing={2.5}>
            {/* Bill of Materials */}
            <Card>
              <CardHeader
                title="Bill of Materials"
                titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
                sx={{ pb: 0 }}
              />
              <CardContent sx={{ p: '0 !important' }}>
                <Box sx={{ overflowX: 'auto' }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                        <TableCell>Component</TableCell>
                        <TableCell>Name</TableCell>
                        <TableCell align="right">Required</TableCell>
                        <TableCell align="right">Available</TableCell>
                        <TableCell>UOM</TableCell>
                        <TableCell>Status</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {MOCK_BOM.map((row) => {
                        const shortage = row.availableQty < row.requiredQty;
                        return (
                          <TableRow key={row.componentCode} hover>
                            <TableCell>
                              <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                                {row.componentCode}
                              </Typography>
                            </TableCell>
                            <TableCell>{row.componentName}</TableCell>
                            <TableCell align="right">{row.requiredQty.toLocaleString()}</TableCell>
                            <TableCell align="right">
                              <Typography
                                variant="body2"
                                sx={{ color: shortage ? 'error.main' : 'text.primary', fontWeight: shortage ? 600 : 400 }}
                              >
                                {row.availableQty.toLocaleString()}
                              </Typography>
                            </TableCell>
                            <TableCell>{row.uom}</TableCell>
                            <TableCell>
                              <Chip
                                label={shortage ? 'Shortage' : 'Available'}
                                size="small"
                                sx={{
                                  bgcolor: shortage ? alpha('#B91C1C', 0.1) : alpha('#15803D', 0.1),
                                  color: shortage ? '#B91C1C' : '#15803D',
                                  fontWeight: 600,
                                  fontSize: 11,
                                }}
                              />
                            </TableCell>
                          </TableRow>
                        );
                      })}
                    </TableBody>
                  </Table>
                </Box>
              </CardContent>
            </Card>

            {/* Linked work orders */}
            <Card>
              <CardHeader
                title="Linked Work Orders"
                titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
                sx={{ pb: 0 }}
              />
              <CardContent sx={{ p: '0 !important' }}>
                <Box sx={{ overflowX: 'auto' }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                        <TableCell>WO #</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell sx={{ minWidth: 160 }}>Progress</TableCell>
                        <TableCell align="right">Actual Qty</TableCell>
                        <TableCell align="right">Planned Qty</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {MOCK_WORK_ORDERS.map((wo) => {
                        const color = STATUS_COLORS[wo.status] ?? '#94A3B8';
                        return (
                          <TableRow key={wo.woNo} hover>
                            <TableCell>
                              <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                                {wo.woNo}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Chip
                                label={wo.status}
                                size="small"
                                sx={{ bgcolor: alpha(color, 0.12), color, fontWeight: 600, fontSize: 11 }}
                              />
                            </TableCell>
                            <TableCell>
                              <Stack spacing={0.5}>
                                <LinearProgress
                                  variant="determinate"
                                  value={wo.progress}
                                  sx={{ height: 6, borderRadius: 3, bgcolor: alpha(color, 0.15), '& .MuiLinearProgress-bar': { bgcolor: color } }}
                                />
                                <Typography variant="caption" color="text.secondary">{wo.progress}%</Typography>
                              </Stack>
                            </TableCell>
                            <TableCell align="right">{wo.actualQty.toLocaleString()} EA</TableCell>
                            <TableCell align="right">{wo.plannedQty.toLocaleString()} EA</TableCell>
                          </TableRow>
                        );
                      })}
                    </TableBody>
                  </Table>
                </Box>
              </CardContent>
            </Card>
          </Stack>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
