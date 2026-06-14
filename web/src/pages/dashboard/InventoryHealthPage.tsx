import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Grid,
  Typography,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { KpiCard, PageHeader, PageRoot, RefreshButton } from '../../components';
import { StatusDonut, TrendLineChart } from '../../components/dashboard';
import { apiClient } from '../../lib/apiClient';

// ── Types ─────────────────────────────────────────────────────────────────────

interface LowStockAlertDto {
  productCode: string;
  availableQty: number;
  locationCode: string;
}

interface ExpiringLotAlertDto {
  productCode: string;
  lotNumber: string;
  expiryDate: string;
  qty: number;
  daysUntilExpiry: number;
}

interface InventoryAlertSummaryDto {
  lowStock: LowStockAlertDto[];
  expiringLots: ExpiringLotAlertDto[];
}

interface StockByLocationDto {
  locationCode: string;
  locationName: string;
  totalQty: number;
  reservedQty: number;
  lotCount: number;
}

interface GrnTrendDto {
  date: string;
  grnCount: number;
  lineCount: number;
  totalQty: number;
}

// ── API ───────────────────────────────────────────────────────────────────────

const fetchAlerts = (): Promise<InventoryAlertSummaryDto> =>
  apiClient.get('/api/v1/dashboard/inventory-alerts').then((r) => r.data);

const fetchStockByLocation = (): Promise<StockByLocationDto[]> =>
  apiClient.get('/api/v1/dashboard/stock-by-location').then((r) => r.data);

const fetchGrnTrend = (from: string, to: string): Promise<GrnTrendDto[]> =>
  apiClient.get(`/api/v1/dashboard/grn-trend?from=${from}&to=${to}`).then((r) => r.data);

// ── Helpers ───────────────────────────────────────────────────────────────────

function todayIso() { return new Date().toISOString().split('T')[0]; }
function daysAgoIso(n: number) {
  const d = new Date();
  d.setDate(d.getDate() - n);
  return d.toISOString().split('T')[0];
}

const LOCATION_COLORS = ['#3B82F6', '#10B981', '#F59E0B', '#8B5CF6', '#EF4444', '#64748B'];

export default function InventoryHealthPage() {
  const today = todayIso();
  const from30 = daysAgoIso(30);

  const { data: alerts, isLoading: alertsLoading, refetch: refetchAlerts } = useQuery({
    queryKey: ['inv-alerts'],
    queryFn: fetchAlerts,
  });

  const { data: stockByLoc = [], isLoading: stockLoading, refetch: refetchStock } = useQuery({
    queryKey: ['inv-stock-loc'],
    queryFn: fetchStockByLocation,
  });

  const { data: grnTrend = [], isLoading: grnLoading } = useQuery({
    queryKey: ['inv-grn-trend', from30, today],
    queryFn: () => fetchGrnTrend(from30, today),
  });

  function refetchAll() { refetchAlerts(); refetchStock(); }

  const totalQty = stockByLoc.reduce((s, l) => s + l.totalQty, 0);
  const reservedQty = stockByLoc.reduce((s, l) => s + l.reservedQty, 0);
  const availableQty = totalQty - reservedQty;

  const locationSegments = stockByLoc.slice(0, 6).map((l, i) => ({
    label: l.locationCode,
    value: Number(l.totalQty.toFixed(0)),
    color: LOCATION_COLORS[i % LOCATION_COLORS.length],
  }));

  const grnTrendData = grnTrend.map((d) => ({
    label: d.date.slice(5),
    value: d.grnCount,
    secondary: d.lineCount,
  }));

  return (
    <PageRoot>
      <PageHeader
        title="Inventory Health"
        actions={<RefreshButton onClick={refetchAll} />}
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard label="Total Stock (qty)" value={totalQty.toFixed(0)} icon="bom" loading={stockLoading} />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard label="Reserved" value={reservedQty.toFixed(0)} icon="bom" loading={stockLoading} />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard label="Available" value={availableQty.toFixed(0)} icon="bom" accentColor="#10B981" loading={stockLoading} />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="Low Stock Alerts"
            value={alerts?.lowStock.length ?? 0}
            icon="bom"
            accentColor={(alerts?.lowStock.length ?? 0) > 0 ? '#EF4444' : '#10B981'}
            loading={alertsLoading}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="Expiring Lots (≤30d)"
            value={alerts?.expiringLots.length ?? 0}
            icon="bom"
            accentColor={(alerts?.expiringLots.length ?? 0) > 0 ? '#F59E0B' : '#10B981'}
            loading={alertsLoading}
          />
        </Grid>
        <Grid size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard
            label="Locations"
            value={stockByLoc.length}
            icon="bom"
            loading={stockLoading}
          />
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 5 }}>
          <Card>
            <CardHeader title="Stock by Location" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <StatusDonut segments={locationSegments} centerLabel="Locations" loading={stockLoading} />
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardHeader title="Inbound GRN Trend (30 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
            <CardContent sx={{ pt: 0 }}>
              <TrendLineChart
                data={grnTrendData}
                primaryLabel="GRNs Confirmed"
                secondaryLabel="Lines"
                loading={grnLoading}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Low stock table */}
      {(alerts?.lowStock.length ?? 0) > 0 && (
        <Card sx={{ mb: 2 }}>
          <CardHeader title="Low Stock Alerts" titleTypographyProps={{ variant: 'subtitle1' }} />
          <CardContent sx={{ pt: 0 }}>
            <Box sx={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid rgba(0,0,0,0.12)' }}>
                    {['Product Code', 'Location', 'Available Qty'].map((h) => (
                      <th key={h} style={{ textAlign: 'left', padding: '8px 12px', fontWeight: 600, color: '#64748B' }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {alerts!.lowStock.map((item, i) => (
                    <tr key={i} style={{ borderBottom: '1px solid rgba(0,0,0,0.06)' }}>
                      <td style={{ padding: '8px 12px', fontWeight: 600 }}>{item.productCode}</td>
                      <td style={{ padding: '8px 12px' }}>{item.locationCode}</td>
                      <td style={{ padding: '8px 12px', color: '#EF4444', fontWeight: 600 }}>{Number(item.availableQty).toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </Box>
          </CardContent>
        </Card>
      )}

      {/* Expiring lots */}
      {(alerts?.expiringLots.length ?? 0) > 0 && (
        <Card>
          <CardHeader title="Expiring Lots (next 30 days)" titleTypographyProps={{ variant: 'subtitle1' }} />
          <CardContent sx={{ pt: 0 }}>
            <Box sx={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid rgba(0,0,0,0.12)' }}>
                    {['Product', 'Lot Number', 'Expiry Date', 'Days Left', 'Qty'].map((h) => (
                      <th key={h} style={{ textAlign: 'left', padding: '8px 12px', fontWeight: 600, color: '#64748B' }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {alerts!.expiringLots.map((lot, i) => {
                    const rowColor =
                      lot.daysUntilExpiry <= 7 ? '#FEE2E2'
                        : lot.daysUntilExpiry <= 14 ? '#FEF3C7'
                        : undefined;
                    const textColor =
                      lot.daysUntilExpiry <= 7 ? '#DC2626'
                        : lot.daysUntilExpiry <= 14 ? '#D97706'
                        : '#15803D';
                    return (
                      <tr key={i} style={{ borderBottom: '1px solid rgba(0,0,0,0.06)', backgroundColor: rowColor }}>
                        <td style={{ padding: '8px 12px' }}>{lot.productCode}</td>
                        <td style={{ padding: '8px 12px', fontFamily: 'monospace' }}>{lot.lotNumber}</td>
                        <td style={{ padding: '8px 12px' }}>{lot.expiryDate.slice(0, 10)}</td>
                        <td style={{ padding: '8px 12px' }}>
                          <Typography variant="caption" sx={{ fontWeight: 700, color: textColor }}>
                            {lot.daysUntilExpiry}d
                          </Typography>
                        </td>
                        <td style={{ padding: '8px 12px' }}>{Number(lot.qty).toFixed(2)}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </Box>
          </CardContent>
        </Card>
      )}
    </PageRoot>
  );
}
