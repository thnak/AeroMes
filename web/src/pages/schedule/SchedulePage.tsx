import {
  Box,
  Card,
  CardContent,
  Chip,
  Divider,
  LinearProgress,
  MenuItem,
  Skeleton,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import type { ApexOptions } from 'apexcharts';
import ReactApexChart from 'react-apexcharts';
import { useState, useMemo } from 'react';
import {
  EmptyState,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  StatusChip,
} from '../../components';

import { useGetApiV1IntegrationProductionOrders } from '../../api/integration/integration';
import { useGetApiV1WorkOrders } from '../../api/work-orders/work-orders';
import type { ProductionOrderDto, WorkOrderDto } from '../../api/model';
import type { WorkOrderStatus } from '../../theme/tokens';

const STATUS_COLORS: Record<string, string> = {
  DRAFT:     '#94A3B8',
  RELEASED:  '#60A5FA',
  RUNNING:   '#22C55E',
  PAUSED:    '#F59E0B',
  COMPLETED: '#4ADE80',
  CLOSED:    '#94A3B8',
  CANCELLED: '#F87171',
  ON_HOLD:   '#FDE68A',
};

const PO_STATUS_COLORS: Record<string, string> = {
  RELEASED:  '#60A5FA',
  RUNNING:   '#22C55E',
  COMPLETED: '#4ADE80',
  CANCELLED: '#F87171',
  PENDING:   '#94A3B8',
};

function numVal(v: number | string): number {
  return typeof v === 'number' ? v : parseFloat(v) || 0;
}

function fmtDate(iso: string | null) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
}

// ─── Gantt chart (Production Orders) ─────────────────────────────────────────

function PoGanttChart({ orders }: { orders: ProductionOrderDto[] }) {
  const withDates = orders.filter((o) => o.plannedStartDate && o.plannedEndDate);

  const statuses = [...new Set(withDates.map((o) => o.status))];

  const series = statuses
    .map((status) => {
      const items = withDates.filter((o) => o.status === status);
      return {
        name: status.charAt(0) + status.slice(1).toLowerCase(),
        data: items.map((po) => ({
          x: po.productCode,
          y: [
            new Date(po.plannedStartDate!).getTime(),
            new Date(po.plannedEndDate!).getTime(),
          ] as [number, number],
          fillColor: PO_STATUS_COLORS[status] ?? '#94A3B8',
          extra: po,
        })),
      };
    })
    .filter((s) => s.data.length > 0);

  if (series.length === 0) {
    return (
      <Box sx={{ height: 300, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <EmptyState icon="emptyTable" title="No production orders with planned dates" compact />
      </Box>
    );
  }

  const options: ApexOptions = {
    chart: {
      type: 'rangeBar', height: 340,
      toolbar: { show: false },
      fontFamily: '"Inter", "Segoe UI", sans-serif',
      background: 'transparent',
      animations: { enabled: true, speed: 300 },
    },
    plotOptions: {
      bar: { horizontal: true, barHeight: '55%', rangeBarGroupRows: true },
    },
    xaxis: {
      type: 'datetime',
      labels: {
        style: { fontSize: '11px', fontWeight: 500 },
        datetimeUTC: false,
        format: 'dd MMM',
      },
      axisBorder: { show: false },
    },
    yaxis: {
      labels: { style: { fontSize: '12px', fontWeight: 500 }, maxWidth: 140 },
    },
    grid: {
      borderColor: 'rgba(4,74,66,0.08)',
      xaxis: { lines: { show: true } },
      yaxis: { lines: { show: false } },
    },
    legend: {
      show: true, position: 'top', horizontalAlign: 'left',
      fontSize: '12px', markers: { size: 8 },
      itemMargin: { horizontal: 10 },
    },
    tooltip: {
      custom: ({ seriesIndex: _si, dataPointIndex: _dpi, w }) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const seriesArr = w.config.series as any[];
        const item = seriesArr?.[_si]?.data?.[_dpi];
        if (!item?.extra) return '<div></div>';
        const po = item.extra as ProductionOrderDto;
        const color = PO_STATUS_COLORS[po.status] ?? '#94A3B8';
        return `
          <div style="padding:10px 14px;font-family:Inter,sans-serif;font-size:12px;min-width:200px">
            <div style="font-weight:700;font-size:13px;margin-bottom:6px;color:#044A42">${po.poCode}</div>
            <div style="color:#475569;margin-bottom:4px">${po.productCode}</div>
            <div style="display:flex;align-items:center;gap:6px;margin-bottom:4px">
              <span style="background:${alpha(color, 0.15)};color:${color};padding:2px 6px;border-radius:4px;font-weight:600;font-size:11px">${po.status}</span>
            </div>
            <div style="color:#64748B">${fmtDate(po.plannedStartDate)} → ${fmtDate(po.plannedEndDate)} · Qty: ${numVal(po.targetQuantity).toLocaleString()}</div>
          </div>
        `;
      },
    },
  };

  return (
    <ReactApexChart
      options={options}
      series={series as ApexOptions['series']}
      type="rangeBar"
      height={340}
    />
  );
}

// ─── WO dispatch row ──────────────────────────────────────────────────────────

function WoRow({ wo, idx }: { wo: WorkOrderDto; idx: number }) {
  const ok = numVal(wo.actualOK);
  const target = numVal(wo.targetQty);
  const pct = target > 0 ? (ok / target) * 100 : 0;
  const color = STATUS_COLORS[wo.status] ?? '#94A3B8';

  return (
    <Stack
      direction="row"
      spacing={2}
      sx={(theme) => ({
        alignItems: 'center',
        px: 2, py: 1.25,
        borderBottom: '1px solid', borderColor: 'divider',
        '&:last-child': { borderBottom: 'none' },
        '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.03) },
      })}
    >
      <Typography variant="caption" sx={{ fontWeight: 600, color: 'text.disabled', width: 28, flexShrink: 0 }}>
        {idx + 1}
      </Typography>
      <Typography variant="caption" sx={{ fontWeight: 700, fontFamily: 'ui-monospace, monospace', color: 'primary.main', width: 88, flexShrink: 0 }}>
        {wo.woCode}
      </Typography>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography variant="caption" color="text.secondary">{wo.workCenterName ?? `WC-${wo.workCenterID}`}</Typography>
      </Box>
      <StatusChip status={wo.status as WorkOrderStatus} />
      <Box sx={{ width: 100, flexShrink: 0 }}>
        <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.25 }}>
          <Typography variant="caption" color="text.disabled" sx={{ fontSize: 10 }}>{ok.toLocaleString()} / {target.toLocaleString()}</Typography>
          <Typography variant="caption" sx={{ fontWeight: 700, color, fontSize: 10 }}>{pct.toFixed(0)}%</Typography>
        </Stack>
        <LinearProgress variant="determinate" value={Math.min(pct, 100)}
          sx={{
            height: 4, borderRadius: 2,
            bgcolor: alpha(color, 0.15),
            '& .MuiLinearProgress-bar': { borderRadius: 2, bgcolor: color },
          }}
        />
      </Box>
    </Stack>
  );
}

// ─── Production Order card ────────────────────────────────────────────────────

function PoCard({ po }: { po: ProductionOrderDto }) {
  const qty = numVal(po.targetQuantity);
  const color = PO_STATUS_COLORS[po.status] ?? '#94A3B8';
  return (
    <Box
      sx={{
        p: 1.5, borderRadius: 2, border: '1px solid', borderColor: 'divider',
        borderLeft: '3px solid', borderLeftColor: color,
        bgcolor: alpha(color, 0.03),
      }}
    >
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', mb: 0.5 }}>
        <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontWeight: 700, color: 'primary.main', fontSize: 11 }}>
          {po.poCode}
        </Typography>
        <Chip label={po.status} size="small"
          sx={{ height: 18, fontSize: '0.6rem', fontWeight: 700, bgcolor: alpha(color, 0.12), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      </Stack>
      <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
        {po.productCode}
      </Typography>
      <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
        <Typography variant="caption" color="text.disabled" sx={{ fontSize: 10 }}>
          {fmtDate(po.plannedStartDate)} → {fmtDate(po.plannedEndDate)}
        </Typography>
        <Typography variant="caption" sx={{ fontWeight: 600, fontSize: 10 }}>
          {qty.toLocaleString()} ea
        </Typography>
      </Stack>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

const WO_STATUSES: WorkOrderStatus[] = ['RUNNING', 'RELEASED', 'PAUSED', 'DRAFT', 'COMPLETED'];

export default function SchedulePage() {
  const [wcFilter, setWcFilter]       = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [poStatus, setPoStatus]       = useState('');

  const { data: woResp, isLoading: woLoading, refetch: refetchWo } = useGetApiV1WorkOrders(
    statusFilter ? { status: statusFilter } : undefined,
    { query: { refetchInterval: 60_000 } },
  );
  const workOrders: WorkOrderDto[] = (woResp?.data ?? []) as WorkOrderDto[];

  const { data: poResp, isLoading: poLoading, refetch: refetchPo } = useGetApiV1IntegrationProductionOrders(
    undefined,
    { query: { staleTime: 60_000 } },
  );
  const productionOrders: ProductionOrderDto[] = (poResp?.data ?? []) as ProductionOrderDto[];

  const workCenters = useMemo(() => [...new Set(workOrders.map((w) => w.workCenterName).filter(Boolean))].sort(), [workOrders]);

  const filteredWo = useMemo(() => {
    let r = workOrders;
    if (wcFilter) r = r.filter((w) => w.workCenterName === wcFilter);
    return r;
  }, [workOrders, wcFilter]);

  const filteredPo = useMemo(() => {
    if (!poStatus) return productionOrders;
    return productionOrders.filter((p) => p.status === poStatus);
  }, [productionOrders, poStatus]);

  const poStatuses = useMemo(() => [...new Set(productionOrders.map((p) => p.status))], [productionOrders]);

  const running   = workOrders.filter((w) => w.status === 'RUNNING').length;
  const released  = workOrders.filter((w) => w.status === 'RELEASED').length;
  const completed = workOrders.filter((w) => w.status === 'COMPLETED').length;
  const paused    = workOrders.filter((w) => w.status === 'PAUSED').length;

  return (
    <PageRoot>
      <PageHeader
        title="Production Schedule"
        subtitle="Gantt timeline of production orders and work order dispatch list"
        breadcrumbs={[{ label: 'Production' }, { label: 'Schedule' }]}
        actions={<RefreshButton onClick={() => { refetchWo(); refetchPo(); }} />}
      />

      {/* PO Gantt */}
      <Card sx={{ mb: 2.5 }}>
        <CardContent sx={{ p: '20px !important', pb: '8px !important' }}>
          <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1.5 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>Production Order Timeline</Typography>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <TextField select size="small" label="PO Status" value={poStatus} onChange={(e) => setPoStatus(e.target.value)} sx={{ width: 140 }}>
                <MenuItem value="">All statuses</MenuItem>
                {poStatuses.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
              </TextField>
            </Stack>
          </Stack>
          {poLoading
            ? <Skeleton variant="rectangular" height={200} sx={{ borderRadius: 1 }} />
            : <PoGanttChart orders={filteredPo} />}
        </CardContent>
      </Card>

      {/* WO Dispatch list */}
      <Card>
        <CardContent sx={{ p: '0 !important' }}>
          {/* Toolbar */}
          <Stack direction="row" spacing={1.5} sx={{ px: 2, py: 1.5, alignItems: 'center', borderBottom: '1px solid', borderColor: 'divider', flexWrap: 'wrap', gap: 1 }}>
            <TextField
              select size="small" label="Work Center" value={wcFilter}
              onChange={(e) => setWcFilter(e.target.value)} sx={{ width: 180 }}>
              <MenuItem value="">All work centers</MenuItem>
              {workCenters.map((wc) => <MenuItem key={wc} value={wc!}>{wc}</MenuItem>)}
            </TextField>

            <TextField
              select size="small" label="Status" value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)} sx={{ width: 140 }}>
              <MenuItem value="">All statuses</MenuItem>
              {WO_STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
            </TextField>

            <Divider orientation="vertical" flexItem />

            {/* Status summary */}
            <Stack direction="row" spacing={0.75} sx={{ flexWrap: 'wrap' }}>
              {[
                { label: `${running} Running`,   color: '#22C55E' },
                { label: `${released} Released`, color: '#60A5FA' },
                { label: `${paused} Paused`,     color: '#F59E0B' },
                { label: `${completed} Done`,    color: '#4ADE80' },
              ].map(({ label, color }) => (
                <Chip key={label} label={label} size="small"
                  sx={{ height: 22, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none' }} />
              ))}
            </Stack>
          </Stack>

          {/* Column headers */}
          <Stack direction="row" spacing={2}
            sx={(theme) => ({
              px: 2, py: 1,
              bgcolor: alpha(theme.palette.primary.main, 0.04),
              borderBottom: '1px solid', borderColor: 'divider',
            })}
          >
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 28 }}>#</Typography>
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 88 }}>WO Code</Typography>
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', flex: 1 }}>Work Center</Typography>
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 80 }}>Status</Typography>
            <Typography variant="caption" sx={{ fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.06em', width: 100, textAlign: 'right' }}>Progress</Typography>
          </Stack>

          {woLoading ? (
            <Stack spacing={0} sx={{ px: 2 }}>
              {[1, 2, 3, 4, 5].map((i) => <Skeleton key={i} height={52} />)}
            </Stack>
          ) : filteredWo.length === 0 ? (
            <Box sx={{ py: 6 }}>
              <EmptyState icon="emptyTable" title="No work orders match your filters" compact />
            </Box>
          ) : (
            filteredWo.map((wo, idx) => <WoRow key={String(wo.woid)} wo={wo} idx={idx} />)
          )}
        </CardContent>
      </Card>

      {/* Production Order Board */}
      {productionOrders.length > 0 && (
        <Card sx={{ mt: 2.5 }}>
          <CardContent sx={{ p: 2 }}>
            <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1.5 }}>
              <Box>
                <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>Production Order Board</Typography>
                <Typography variant="caption" color="text.secondary">{filteredPo.length} of {productionOrders.length} orders</Typography>
              </Box>
              <SolarIcon name="reports" size={18} />
            </Stack>
            <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 1.5 }}>
              {filteredPo.slice(0, 24).map((po) => <PoCard key={String(po.poid)} po={po} />)}
            </Box>
          </CardContent>
        </Card>
      )}
    </PageRoot>
  );
}
