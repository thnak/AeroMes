import {
  Box,
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
import { useParams, useNavigate } from 'react-router-dom';
import {
  DetailPageSkeleton,
  EmptyState,
  PageHeader,
  PageRoot,
} from '../../components';
import {
  useGetApiV1IntegrationSalesOrdersId,
} from '../../api/integration/integration';
import type { ProductionOrderSummaryDto } from '../../api/model/productionOrderSummaryDto';

// ─── Constants ────────────────────────────────────────────────────────────────

const PO_STATUS_COLOR: Record<string, string> = {
  Released:  '#1D4ED8',
  Running:   '#D97706',
  Paused:    '#9333EA',
  Completed: '#15803D',
  Cancelled: '#B91C1C',
};

const SO_STATUS_COLOR: Record<string, string> = {
  Open:      '#94A3B8',
  Closed:    '#475569',
  Cancelled: '#DC2626',
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
  const navigate = useNavigate();
  const numId = Number(id);

  const { data, isLoading, isError } = useGetApiV1IntegrationSalesOrdersId(numId);
  const so = data?.data;

  if (isLoading) return <DetailPageSkeleton />;
  if (isError || !so) {
    return (
      <PageRoot>
        <EmptyState title="Sales order not found" description="This order may have been removed or the ERP sync hasn't run yet." />
      </PageRoot>
    );
  }

  const statusColor = SO_STATUS_COLOR[so.status] ?? '#94A3B8';

  return (
    <PageRoot>
      <PageHeader
        title={so.soCode}
        subtitle={[so.customerName, so.deliveryDate ? `Delivery ${new Date(so.deliveryDate).toLocaleDateString()}` : null].filter(Boolean).join(' · ')}
        breadcrumbs={[
          { label: 'Integration', href: '/integration' },
          { label: 'Sales Orders', href: '/integration/sales-orders' },
          { label: so.soCode },
        ]}
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
              <InfoRow label="SO #"          value={so.soCode}  mono />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Customer"      value={so.customerName ?? '—'} />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Order Date"    value={new Date(so.orderDate).toLocaleDateString()} />
              <InfoRow
                label="Delivery Date"
                value={so.deliveryDate ? new Date(so.deliveryDate).toLocaleDateString() : '—'}
              />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow
                label="Status"
                value={
                  <Chip
                    label={so.status}
                    size="small"
                    sx={{
                      bgcolor: alpha(statusColor, 0.12),
                      color: statusColor,
                      fontWeight: 600,
                      fontSize: 11,
                      border: `1px solid ${alpha(statusColor, 0.25)}`,
                    }}
                  />
                }
              />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Synced from ERP" value={new Date(so.syncedAt).toLocaleString()} />
            </CardContent>
          </Card>
        </Grid>

        {/* ── Linked production orders ─────────────────────────────── */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardHeader
              title="Linked Production Orders"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
              sx={{ pb: 0 }}
            />
            <CardContent sx={{ p: '0 !important' }}>
              {so.productionOrders.length === 0 ? (
                <Box sx={{ p: 3 }}>
                  <Typography variant="body2" color="text.secondary" align="center">
                    No production orders linked to this sales order.
                  </Typography>
                </Box>
              ) : (
                <Box sx={{ overflowX: 'auto' }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                        <TableCell>PO #</TableCell>
                        <TableCell>Product</TableCell>
                        <TableCell align="right">Target Qty</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>Planned Start</TableCell>
                        <TableCell>Planned End</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {so.productionOrders.map((po: ProductionOrderSummaryDto) => {
                        const color = PO_STATUS_COLOR[po.status] ?? '#94A3B8';
                        return (
                          <TableRow
                            key={String(po.poid)}
                            hover
                            sx={{ cursor: 'pointer' }}
                            onClick={() => navigate(`/integration/production-orders/${po.poid}`)}
                          >
                            <TableCell>
                              <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                                {po.poCode}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600, color: 'primary.main' }}>
                                {po.productCode}
                              </Typography>
                            </TableCell>
                            <TableCell align="right">{Number(po.targetQuantity).toLocaleString()} EA</TableCell>
                            <TableCell>
                              <Chip
                                label={po.status}
                                size="small"
                                sx={{
                                  bgcolor: alpha(color, 0.12),
                                  color,
                                  fontWeight: 600,
                                  fontSize: 11,
                                  border: `1px solid ${alpha(color, 0.25)}`,
                                }}
                              />
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" sx={{ fontSize: 12 }}>
                                {po.plannedStartDate ? new Date(po.plannedStartDate).toLocaleDateString() : '—'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" sx={{ fontSize: 12 }}>
                                {po.plannedEndDate ? new Date(po.plannedEndDate).toLocaleDateString() : '—'}
                              </Typography>
                            </TableCell>
                          </TableRow>
                        );
                      })}
                    </TableBody>
                  </Table>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
