import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
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
  KpiCard,
  PageHeader,
  PageRoot,
  SolarIcon,
  StatusChip,
} from '../../components';
import { MOCK_WORK_ORDERS } from './WorkOrdersPage';
import type { WorkOrderStatus } from '../../theme/tokens';

// ─── Mock sub-data ────────────────────────────────────────────────────────────

interface OperationStep {
  step: number;
  operation: string;
  machine: string;
  status: 'Pending' | 'Running' | 'Done' | 'Skipped';
  startedAt?: string;
  endedAt?: string;
  output?: number;
}

const MOCK_OPERATIONS: OperationStep[] = [
  { step: 10, operation: 'Raw Material Prep',    machine: 'MC-01', status: 'Done',    startedAt: '2026-06-01 08:00', endedAt: '2026-06-01 10:30', output: 500 },
  { step: 20, operation: 'CNC Machining',        machine: 'MC-03', status: 'Done',    startedAt: '2026-06-02 07:30', endedAt: '2026-06-03 17:00', output: 498 },
  { step: 30, operation: 'Surface Treatment',    machine: 'MC-05', status: 'Running', startedAt: '2026-06-08 08:00', endedAt: undefined,          output: 387 },
  { step: 40, operation: 'Sub-assembly',         machine: 'MC-04', status: 'Pending', startedAt: undefined,          endedAt: undefined,          output: undefined },
  { step: 50, operation: 'Final Inspection',     machine: 'MC-06', status: 'Pending', startedAt: undefined,          endedAt: undefined,          output: undefined },
];

const MOCK_ACTIVITY = [
  { time: '2026-06-12 11:42', event: 'Output logged: 12 units OK, 0 NG', user: 'Nguyen Van A' },
  { time: '2026-06-12 10:15', event: 'Job JOB-4421 resumed after coolant refill', user: 'System' },
  { time: '2026-06-12 09:00', event: 'WO released to shop floor', user: 'Tran Thi B (Supervisor)' },
  { time: '2026-06-11 17:30', event: 'Downtime DT-0089 resolved — spindle replaced', user: 'Le Van C' },
  { time: '2026-06-11 14:00', event: 'Machine MC-01 went offline — spindle fault', user: 'System' },
];

const OP_STATUS_COLORS: Record<OperationStep['status'], { color: string; bg: string }> = {
  Pending: { color: '#64748B', bg: alpha('#64748B', 0.1) },
  Running: { color: '#15803D', bg: alpha('#15803D', 0.1) },
  Done:    { color: '#166534', bg: alpha('#166534', 0.1) },
  Skipped: { color: '#94A3B8', bg: alpha('#94A3B8', 0.1) },
};

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function WorkOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const wo = MOCK_WORK_ORDERS.find((w) => w.id === id) ?? MOCK_WORK_ORDERS[0];
  const progress = wo.targetQty > 0 ? Math.round((wo.actualQty / wo.targetQty) * 100) : 0;
  const defects = 7;
  const oee = 74.2;

  return (
    <PageRoot>
      <PageHeader
        title={wo.no}
        subtitle={`${wo.productCode} — ${wo.productName}`}
        breadcrumbs={[
          { label: 'Production', href: '/production' },
          { label: 'Work Orders', href: '/production/work-orders' },
          { label: wo.no },
        ]}
        actions={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              color="error"
              size="small"
              startIcon={<SolarIcon name="cancel" size={16} />}
              disabled={wo.status === 'CANCELLED' || wo.status === 'CLOSED' || wo.status === 'COMPLETED'}
            >
              Cancel WO
            </Button>
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="edit" size={16} />}
              onClick={() => navigate('/production/work-orders')}
            >
              Edit
            </Button>
          </Stack>
        }
      />

      <Grid container spacing={2.5}>
        {/* Left column */}
        <Grid size={{ xs: 12, md: 8 }}>
          {/* Details card */}
          <Card sx={{ mb: 2.5 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Work Order Details</Typography>
              <Grid container spacing={2}>
                {[
                  { label: 'Status',          value: <StatusChip status={wo.status as WorkOrderStatus} /> },
                  { label: 'Product Code',    value: <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600, color: 'primary.main' }}>{wo.productCode}</Typography> },
                  { label: 'Product Name',    value: wo.productName },
                  { label: 'Assigned Machine', value: <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>{wo.assignedMachine}</Typography> },
                  { label: 'Target Qty',      value: wo.targetQty.toLocaleString() },
                  { label: 'Actual Qty',      value: wo.actualQty.toLocaleString() },
                  { label: 'Planned Start',   value: wo.plannedStart },
                  { label: 'Planned End',     value: wo.plannedEnd },
                ].map(({ label, value }) => (
                  <Grid key={label} size={{ xs: 6 }}>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', letterSpacing: '0.05em', fontWeight: 600 }}>
                      {label}
                    </Typography>
                    <Box sx={{ mt: 0.25 }}>
                      {typeof value === 'string' ? (
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>{value}</Typography>
                      ) : value}
                    </Box>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>

          {/* Operations card */}
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Operations / Job Tracking</Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    {['Step', 'Operation', 'Machine', 'Status', 'Start', 'End', 'Output'].map((h) => (
                      <TableCell key={h} sx={{ py: 0.75, fontWeight: 700, fontSize: '0.75rem', color: 'text.secondary', borderColor: 'divider' }}>{h}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {MOCK_OPERATIONS.map((op) => {
                    const sc = OP_STATUS_COLORS[op.status];
                    return (
                      <TableRow key={op.step} hover>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600, fontSize: 12 }}>{op.step}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                          <Typography variant="body2" sx={{ fontSize: 12 }}>{op.operation}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12 }}>{op.machine}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                          <Chip
                            label={op.status}
                            size="small"
                            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: sc.bg, color: sc.color, border: 'none', '& .MuiChip-label': { px: 0.75 } }}
                          />
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                          <Typography variant="caption" color="text.secondary">{op.startedAt ?? '—'}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider' }}>
                          <Typography variant="caption" color="text.secondary">{op.endedAt ?? '—'}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider' }} align="right">
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>{op.output?.toLocaleString() ?? '—'}</Typography>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>

        {/* Right column */}
        <Grid size={{ xs: 12, md: 4 }}>
          {/* Progress card */}
          <Card sx={{ mb: 2.5 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Progress</Typography>
              <Stack spacing={1} sx={{ mb: 2.5 }}>
                <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2" color="text.secondary">Overall completion</Typography>
                  <Typography variant="h5" sx={{ fontWeight: 700, color: progress >= 100 ? 'success.main' : 'primary.main' }}>{progress}%</Typography>
                </Stack>
                <LinearProgress
                  variant="determinate"
                  value={progress}
                  sx={{ height: 12, borderRadius: 6, bgcolor: (t) => alpha(t.palette.primary.main, 0.1) }}
                />
                <Typography variant="caption" color="text.secondary">
                  {wo.actualQty.toLocaleString()} of {wo.targetQty.toLocaleString()} units produced
                </Typography>
              </Stack>
              <Grid container spacing={1.5}>
                <Grid size={{ xs: 6 }}>
                  <KpiCard label="Target" value={wo.targetQty.toLocaleString()} icon="quantity" accentColor="#1D4ED8" />
                </Grid>
                <Grid size={{ xs: 6 }}>
                  <KpiCard label="Actual" value={wo.actualQty.toLocaleString()} icon="quantity" accentColor="#15803D" />
                </Grid>
                <Grid size={{ xs: 6 }}>
                  <KpiCard label="Defects" value={defects} icon="warning" accentColor="#DC2626" />
                </Grid>
                <Grid size={{ xs: 6 }}>
                  <KpiCard label="OEE" value={`${oee}%`} icon="oee" accentColor="#D97706" />
                </Grid>
              </Grid>
            </CardContent>
          </Card>

          {/* Recent activity */}
          <Card>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Recent Activity</Typography>
              <Stack spacing={1.5}>
                {MOCK_ACTIVITY.map((a, i) => (
                  <Stack key={i} direction="row" spacing={1.5} sx={{ alignItems: 'flex-start' }}>
                    <Box sx={{ width: 6, height: 6, borderRadius: '50%', bgcolor: 'primary.main', mt: 0.75, flexShrink: 0 }} />
                    <Box>
                      <Typography variant="body2" sx={{ fontSize: 12 }}>{a.event}</Typography>
                      <Stack direction="row" spacing={1}>
                        <Typography variant="caption" color="text.secondary">{a.time}</Typography>
                        <Typography variant="caption" color="text.disabled">·</Typography>
                        <Typography variant="caption" color="text.secondary">{a.user}</Typography>
                      </Stack>
                    </Box>
                  </Stack>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
