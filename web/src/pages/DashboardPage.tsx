import { Box, Card, CardContent, Chip, Grid, LinearProgress, Stack, Typography } from '@mui/material';
import { alpha } from '@mui/material/styles';
import PageHeader, { PageRoot } from '../components/PageHeader';
import KpiCard from '../components/KpiCard';
import StatusChip from '../components/StatusChip';
import { oeeZoneColor } from '../theme/tokens';
import type { WorkOrderStatus } from '../theme/tokens';

const kpis = [
  { label: 'Active Work Orders', value: 12,      icon: 'workOrders' as const, accentColor: '#1D4ED8' },
  { label: 'OEE Today',          value: '82.4%', icon: 'oee'        as const, accentColor: oeeZoneColor(82.4), trend: 2.1, trendLabel: 'vs yesterday' },
  { label: 'Output (OK)',         value: '4,812', icon: 'success'    as const, accentColor: '#15803D', trend: 5.4, trendLabel: 'vs shift avg' },
  { label: 'Defect Rate',         value: '1.3%', icon: 'warning'    as const, accentColor: '#B91C1C', trend: -0.2, trendLabel: 'vs yesterday' },
];

const workOrders = [
  { no: 'WO-2026-0089', product: 'Frame Assembly A',     status: 'RUNNING'   as WorkOrderStatus, progress: 78 },
  { no: 'WO-2026-0090', product: 'Panel Sub-assembly B', status: 'RELEASED'  as WorkOrderStatus, progress: 0  },
  { no: 'WO-2026-0088', product: 'Shaft Housing C',      status: 'PAUSED'    as WorkOrderStatus, progress: 45 },
  { no: 'WO-2026-0091', product: 'Bracket Set D',        status: 'COMPLETED' as WorkOrderStatus, progress: 100 },
];

const machineStatus = [
  { id: 'MC-01', name: 'CNC Lathe 1',    status: 'RUNNING', oee: 88 },
  { id: 'MC-02', name: 'Press Line A',   status: 'RUNNING', oee: 74 },
  { id: 'MC-03', name: 'Welding Bay 1',  status: 'IDLE',    oee: 0  },
  { id: 'MC-04', name: 'Assembly St. 2', status: 'SETUP',   oee: 0  },
  { id: 'MC-05', name: 'Paint Booth',    status: 'DOWN',    oee: 0  },
  { id: 'MC-06', name: 'CNC Mill 2',     status: 'RUNNING', oee: 91 },
];

const machineStatusColor: Record<string, string> = {
  RUNNING: '#15803D',
  IDLE:    '#94A3B8',
  SETUP:   '#1D4ED8',
  DOWN:    '#B91C1C',
  OFFLINE: '#374151',
};

export default function DashboardPage() {
  return (
    <PageRoot>
      <PageHeader
        title="Production Overview"
        subtitle={`Real-time shop floor status · ${new Date().toLocaleDateString('en-GB', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}`}
      />

      {/* KPI row */}
      <Grid container spacing={2.5} sx={{ mb: 3 }}>
        {kpis.map((kpi) => (
          <Grid key={kpi.label} size={{ xs: 12, sm: 6, md: 3 }}>
            <KpiCard {...kpi} />
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={2.5}>
        {/* Work Orders */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent sx={{ p: '20px !important' }}>
              <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                <Typography variant="subtitle1">Active Work Orders</Typography>
                <Chip label="4 orders" size="small" variant="outlined" />
              </Stack>

              <Stack spacing={1.5}>
                {workOrders.map((wo) => (
                  <Box
                    key={wo.no}
                    sx={(theme) => ({
                      p: 1.5,
                      borderRadius: 2,
                      border: '1px solid',
                      borderColor: 'divider',
                      bgcolor: alpha(theme.palette.primary.main, 0.02),
                    })}
                  >
                    <Stack direction="row" spacing={1} sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
                      <Box sx={{ minWidth: 0 }}>
                        <Typography
                          variant="body2"
                          sx={{ fontWeight: 600, fontFamily: 'ui-monospace, monospace', fontSize: '0.8125rem' }}
                        >
                          {wo.no}
                        </Typography>
                        <Typography variant="caption" color="text.secondary" noWrap>
                          {wo.product}
                        </Typography>
                      </Box>
                      <StatusChip status={wo.status} />
                    </Stack>
                    {wo.progress > 0 && wo.status !== 'COMPLETED' && (
                      <Box sx={{ mt: 1 }}>
                        <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" color="text.disabled">Progress</Typography>
                          <Typography variant="caption" sx={{ fontWeight: 600 }}>{wo.progress}%</Typography>
                        </Stack>
                        <LinearProgress
                          variant="determinate"
                          value={wo.progress}
                          color={wo.status === 'PAUSED' ? 'warning' : 'primary'}
                          sx={{ borderRadius: 1, height: 4 }}
                        />
                      </Box>
                    )}
                    {wo.status === 'COMPLETED' && (
                      <LinearProgress
                        variant="determinate"
                        value={100}
                        color="success"
                        sx={{ borderRadius: 1, height: 4, mt: 1 }}
                      />
                    )}
                  </Box>
                ))}
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Machine status */}
        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent sx={{ p: '20px !important' }}>
              <Typography variant="subtitle1" sx={{ mb: 2 }}>Machine Status</Typography>
              <Stack spacing={1}>
                {machineStatus.map((m) => (
                  <Stack
                    key={m.id}
                    direction="row"
                    spacing={1.5}
                    sx={(theme) => ({
                      alignItems: 'center',
                      p: 1.25,
                      borderRadius: 1.5,
                      border: '1px solid',
                      borderColor: 'divider',
                      '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.03) },
                    })}
                  >
                    {/* Status dot */}
                    <Box
                      sx={{
                        width: 10,
                        height: 10,
                        borderRadius: '50%',
                        bgcolor: machineStatusColor[m.status],
                        flexShrink: 0,
                        boxShadow: m.status === 'RUNNING'
                          ? `0 0 0 3px ${alpha(machineStatusColor[m.status], 0.2)}`
                          : 'none',
                      }}
                    />
                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <Typography variant="caption" noWrap sx={{ display: 'block', fontWeight: 600 }}>
                        {m.name}
                      </Typography>
                      <Typography variant="caption" color="text.disabled">
                        {m.id}
                      </Typography>
                    </Box>
                    {m.oee > 0 && (
                      <Typography
                        variant="caption"
                        sx={{ fontWeight: 700, color: oeeZoneColor(m.oee), flexShrink: 0 }}
                      >
                        {m.oee}%
                      </Typography>
                    )}
                    <Chip
                      label={m.status}
                      size="small"
                      sx={{
                        height: 20,
                        fontSize: '0.625rem',
                        fontWeight: 600,
                        bgcolor: alpha(machineStatusColor[m.status], 0.1),
                        color: machineStatusColor[m.status],
                        border: 'none',
                        '& .MuiChip-label': { px: 0.75 },
                      }}
                    />
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
