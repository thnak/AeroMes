import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Divider,
  Grid,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
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

interface SoLineItem {
  line: number;
  productCode: string;
  productName: string;
  orderedQty: number;
  confirmedQty: number;
  uom: string;
  deliveryDate: string;
  status: 'Confirmed' | 'In Production' | 'Shipped' | 'Pending';
}

interface LinkedProductionOrder {
  poNo: string;
  product: string;
  qty: number;
  status: 'Released' | 'In Progress' | 'Completed';
  plannedEnd: string;
}

// ─── Mock detail data ─────────────────────────────────────────────────────────

const MOCK_LINE_ITEMS: SoLineItem[] = [
  { line: 1, productCode: 'FRM-A001', productName: 'Frame Assembly A',     orderedQty: 400, confirmedQty: 400, uom: 'EA', deliveryDate: '2026-06-30', status: 'In Production' },
  { line: 2, productCode: 'PNL-B002', productName: 'Panel Sub-assembly B', orderedQty: 300, confirmedQty: 300, uom: 'EA', deliveryDate: '2026-06-25', status: 'In Production' },
  { line: 3, productCode: 'SHT-C003', productName: 'Shaft Housing C',      orderedQty: 200, confirmedQty: 180, uom: 'EA', deliveryDate: '2026-06-20', status: 'Confirmed'     },
];

const MOCK_LINKED_POS: LinkedProductionOrder[] = [
  { poNo: 'PO-2026-0045', product: 'Frame Assembly A',     qty: 400, status: 'In Progress', plannedEnd: '2026-06-28' },
  { poNo: 'PO-2026-0046', product: 'Panel Sub-assembly B', qty: 300, status: 'Released',    plannedEnd: '2026-06-24' },
];

const LINE_STATUS_COLOR: Record<SoLineItem['status'], string> = {
  'Confirmed':     '#1D4ED8',
  'In Production': '#D97706',
  'Shipped':       '#15803D',
  'Pending':       '#94A3B8',
};

const PO_STATUS_COLOR: Record<LinkedProductionOrder['status'], string> = {
  'Released':   '#1D4ED8',
  'In Progress': '#D97706',
  'Completed':   '#15803D',
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

export default function SalesOrderDetailPage() {
  const { id } = useParams<{ id: string }>();

  // In a real app, look up by id. Here we use a fixed mock.
  const soNo = id === '1' ? 'SO-2026-0091' : `SO-2026-00${id ?? '91'}`;

  return (
    <PageRoot>
      <PageHeader
        title={soNo}
        subtitle="JAXA Research · Delivery 30 Jun 2026"
        breadcrumbs={[
          { label: 'Integration', href: '/integration' },
          { label: 'Sales Orders', href: '/integration/sales-orders' },
          { label: soNo },
        ]}
        actions={
          <Button
            variant="contained"
            startIcon={<SolarIcon name="add" size={16} />}
          >
            Create Production Orders
          </Button>
        }
      />

      <Grid container spacing={2.5}>
        {/* ── Order info card ─────────────────────────────────────── */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card sx={{ height: '100%' }}>
            <CardHeader
              title="Order Info"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
              sx={{ pb: 0 }}
            />
            <CardContent>
              <InfoRow label="SO #"          value={soNo}                   mono />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Customer Code" value="JAXA-001"               mono />
              <InfoRow label="Customer Name" value="JAXA Research" />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Order Date"    value="2026-05-01" />
              <InfoRow label="Delivery Date" value="2026-06-30" />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow
                label="Status"
                value={
                  <Chip
                    label="In Production"
                    size="small"
                    sx={{ bgcolor: alpha('#D97706', 0.12), color: '#D97706', fontWeight: 600, fontSize: 11 }}
                  />
                }
              />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Currency"      value="JPY" />
              <InfoRow label="Total Value"   value="¥ 4,500,000" />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Synced from ERP" value="2026-06-12 08:14" />
            </CardContent>
          </Card>
        </Grid>

        {/* ── Right column ─────────────────────────────────────────── */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Stack spacing={2.5}>
            {/* Line items */}
            <Card>
              <CardHeader
                title="Line Items"
                titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
                sx={{ pb: 0 }}
              />
              <CardContent sx={{ p: '0 !important' }}>
                <Box sx={{ overflowX: 'auto' }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                        <TableCell>Line</TableCell>
                        <TableCell>Product Code</TableCell>
                        <TableCell>Product Name</TableCell>
                        <TableCell align="right">Ordered</TableCell>
                        <TableCell align="right">Confirmed</TableCell>
                        <TableCell>UOM</TableCell>
                        <TableCell>Delivery</TableCell>
                        <TableCell>Status</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {MOCK_LINE_ITEMS.map((item) => (
                        <TableRow key={item.line} hover>
                          <TableCell>{item.line}</TableCell>
                          <TableCell>
                            <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                              {item.productCode}
                            </Typography>
                          </TableCell>
                          <TableCell>{item.productName}</TableCell>
                          <TableCell align="right">{item.orderedQty.toLocaleString()}</TableCell>
                          <TableCell align="right">{item.confirmedQty.toLocaleString()}</TableCell>
                          <TableCell>{item.uom}</TableCell>
                          <TableCell>{item.deliveryDate}</TableCell>
                          <TableCell>
                            <Chip
                              label={item.status}
                              size="small"
                              sx={{
                                bgcolor: alpha(LINE_STATUS_COLOR[item.status], 0.12),
                                color: LINE_STATUS_COLOR[item.status],
                                fontWeight: 600,
                                fontSize: 11,
                              }}
                            />
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </Box>
              </CardContent>
            </Card>

            {/* Linked production orders */}
            <Card>
              <CardHeader
                title="Linked Production Orders"
                titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
                sx={{ pb: 0 }}
              />
              <CardContent sx={{ p: '0 !important' }}>
                <Box sx={{ overflowX: 'auto' }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                        <TableCell>PO #</TableCell>
                        <TableCell>Product</TableCell>
                        <TableCell align="right">Qty</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>Planned End</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {MOCK_LINKED_POS.map((po) => (
                        <TableRow key={po.poNo} hover>
                          <TableCell>
                            <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                              {po.poNo}
                            </Typography>
                          </TableCell>
                          <TableCell>{po.product}</TableCell>
                          <TableCell align="right">{po.qty.toLocaleString()} EA</TableCell>
                          <TableCell>
                            <Chip
                              label={po.status}
                              size="small"
                              sx={{
                                bgcolor: alpha(PO_STATUS_COLOR[po.status], 0.12),
                                color: PO_STATUS_COLOR[po.status],
                                fontWeight: 600,
                                fontSize: 11,
                              }}
                            />
                          </TableCell>
                          <TableCell>{po.plannedEnd}</TableCell>
                        </TableRow>
                      ))}
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
