import {
  Box,
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
  useGetApiV1IntegrationProductionOrdersId,
} from '../../api/integration/integration';
import type { WorkOrderSummaryDto } from '../../api/model/workOrderSummaryDto';

// ─── Constants ────────────────────────────────────────────────────────────────

const STATUS_COLORS: Record<string, string> = {
  Released:    '#1D4ED8',
  Running:     '#D97706',
  Paused:      '#9333EA',
  Completed:   '#15803D',
  Cancelled:   '#B91C1C',
};

const WO_STATUS_COLORS: Record<string, string> = {
  Draft:       '#94A3B8',
  Released:    '#1D4ED8',
  InProgress:  '#D97706',
  Completed:   '#15803D',
  Cancelled:   '#B91C1C',
  OnHold:      '#9333EA',
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
  const navigate = useNavigate();
  const numId = Number(id);

  const { data, isLoading, isError } = useGetApiV1IntegrationProductionOrdersId(numId);
  const po = data?.data;

  if (isLoading) return <DetailPageSkeleton />;
  if (isError || !po) {
    return (
      <PageRoot>
        <EmptyState title="Production order not found" description="This order may have been removed or the ERP sync hasn't run yet." />
      </PageRoot>
    );
  }

  const statusColor = STATUS_COLORS[po.status] ?? '#94A3B8';

  return (
    <PageRoot>
      <PageHeader
        title={po.poCode}
        subtitle={`${po.productCode} · ${Number(po.targetQuantity).toLocaleString()} EA`}
        breadcrumbs={[
          { label: 'Integration', href: '/integration' },
          { label: 'Production Orders', href: '/integration/production-orders' },
          { label: po.poCode },
        ]}
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
              <InfoRow label="PO #"         value={po.poCode}      mono />
              {po.soid && (
                <InfoRow
                  label="SO #"
                  value={
                    <Typography
                      variant="caption"
                      sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600, color: 'primary.main', cursor: 'pointer', '&:hover': { textDecoration: 'underline' } }}
                      onClick={() => navigate(`/integration/sales-orders/${po.soid}`)}
                    >
                      SO-{String(po.soid).padStart(6, '0')}
                    </Typography>
                  }
                />
              )}
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Product Code" value={po.productCode} mono />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Target Qty"   value={`${Number(po.targetQuantity).toLocaleString()} EA`} />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow
                label="Status"
                value={
                  <Chip
                    label={po.status}
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
              <InfoRow label="Planned Start"  value={po.plannedStartDate  ? new Date(po.plannedStartDate).toLocaleDateString()  : '—'} />
              <InfoRow label="Planned End"    value={po.plannedEndDate    ? new Date(po.plannedEndDate).toLocaleDateString()    : '—'} />
              <InfoRow label="Actual Start"   value={po.actualStartDate   ? new Date(po.actualStartDate).toLocaleDateString()   : '—'} />
              <InfoRow label="Actual End"     value={po.actualEndDate     ? new Date(po.actualEndDate).toLocaleDateString()     : '—'} />
              <Divider sx={{ my: 0.5 }} />
              <InfoRow label="Synced from ERP" value={new Date(po.syncedAt).toLocaleString()} />
            </CardContent>
          </Card>
        </Grid>

        {/* ── Linked work orders ───────────────────────────────────── */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardHeader
              title="Linked Work Orders"
              titleTypographyProps={{ variant: 'subtitle2', fontWeight: 700 }}
              sx={{ pb: 0 }}
            />
            <CardContent sx={{ p: '0 !important' }}>
              {po.workOrders.length === 0 ? (
                <Box sx={{ p: 3 }}>
                  <Typography variant="body2" color="text.secondary" align="center">
                    No work orders linked to this production order.
                  </Typography>
                </Box>
              ) : (
                <Box sx={{ overflowX: 'auto' }}>
                  <Table size="small">
                    <TableHead>
                      <TableRow sx={{ '& th': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), fontWeight: 600, fontSize: 12 } }}>
                        <TableCell>WO #</TableCell>
                        <TableCell>Work Center</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell sx={{ minWidth: 160 }}>Progress</TableCell>
                        <TableCell align="right">OK</TableCell>
                        <TableCell align="right">NG</TableCell>
                        <TableCell align="right">Target</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {po.workOrders.map((wo: WorkOrderSummaryDto) => {
                        const target = Number(wo.targetQuantity);
                        const ok = Number(wo.actualOK);
                        const ng = Number(wo.actualNG);
                        const progress = target > 0 ? Math.min(100, Math.round((ok / target) * 100)) : 0;
                        const color = WO_STATUS_COLORS[wo.status] ?? '#94A3B8';
                        return (
                          <TableRow key={String(wo.woid)} hover>
                            <TableCell>
                              <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                                {wo.woCode}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" sx={{ fontSize: 12 }}>
                                {wo.workCenterName ?? '—'}
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
                                  value={progress}
                                  sx={{ height: 6, borderRadius: 3, bgcolor: alpha(color, 0.15), '& .MuiLinearProgress-bar': { bgcolor: color } }}
                                />
                                <Typography variant="caption" color="text.secondary">{progress}%</Typography>
                              </Stack>
                            </TableCell>
                            <TableCell align="right">
                              <Typography variant="body2" sx={{ color: 'success.main', fontWeight: 500 }}>
                                {ok.toLocaleString()}
                              </Typography>
                            </TableCell>
                            <TableCell align="right">
                              <Typography variant="body2" sx={{ color: ng > 0 ? 'error.main' : 'text.secondary', fontWeight: ng > 0 ? 600 : 400 }}>
                                {ng.toLocaleString()}
                              </Typography>
                            </TableCell>
                            <TableCell align="right">{target.toLocaleString()} EA</TableCell>
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
