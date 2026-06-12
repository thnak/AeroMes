import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
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
import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
  PageHeader,
  PageRoot,
  SolarIcon,
} from '../../components';

// ─── Types & mock data ────────────────────────────────────────────────────────

interface LotInfo {
  lotNo: string;
  productCode: string;
  productName: string;
  quantity: number;
  uom: string;
  location: string;
  receivedAt: string;
  status: 'Active' | 'Consumed' | 'Quarantine';
}

interface MovementRecord {
  date: string;
  from: string;
  to: string;
  qty: number;
  type: 'Receipt' | 'Transfer' | 'Issue' | 'Return' | 'Adjustment';
  operator: string;
}

interface GenealogyNode {
  lotNo: string;
  productCode: string;
  productName: string;
  type: 'parent' | 'current' | 'child';
}

const LOT_DB: Record<string, { info: LotInfo; movements: MovementRecord[]; parents: GenealogyNode[]; children: GenealogyNode[] }> = {
  'LOT-2026-0441': {
    info: {
      lotNo: 'LOT-2026-0441',
      productCode: 'FRM-A001',
      productName: 'Frame Assembly A',
      quantity: 120,
      uom: 'EA',
      location: 'WH-02 WIP',
      receivedAt: '2026-06-01',
      status: 'Active',
    },
    movements: [
      { date: '2026-06-01 08:00', from: 'WH-01 Raw Materials',   to: 'WH-02 WIP',             qty: 200, type: 'Transfer',    operator: 'Nguyen Van A' },
      { date: '2026-06-03 10:30', from: 'WH-02 WIP',             to: 'WO-2026-0089 (Job)',    qty: 80,  type: 'Issue',       operator: 'System' },
      { date: '2026-06-08 14:00', from: 'WH-02 WIP',             to: 'WO-2026-0094 (Job)',    qty: 0,   type: 'Issue',       operator: 'System' },
      { date: '2026-06-10 09:00', from: 'Receiving Dock',        to: 'WH-02 WIP',             qty: 20,  type: 'Receipt',     operator: 'Tran Thi B' },
      { date: '2026-06-11 15:30', from: 'WH-02 WIP',             to: 'WH-02 WIP',             qty: -20, type: 'Adjustment',  operator: 'Le Van C' },
    ],
    parents: [
      { lotNo: 'LOT-2025-8812', productCode: 'RAW-STL-01', productName: 'Raw Steel Billet A', type: 'parent' },
      { lotNo: 'LOT-2025-8813', productCode: 'RAW-ALU-02', productName: 'Aluminium Sheet B',  type: 'parent' },
    ],
    children: [
      { lotNo: 'LOT-2026-0461', productCode: 'FRM-A001', productName: 'Frame Assembly A (FG)', type: 'child' },
    ],
  },
  'LOT-2026-0442': {
    info: {
      lotNo: 'LOT-2026-0442',
      productCode: 'PNL-B002',
      productName: 'Panel Sub-assembly B',
      quantity: 300,
      uom: 'EA',
      location: 'WH-01 Raw Materials',
      receivedAt: '2026-05-28',
      status: 'Active',
    },
    movements: [
      { date: '2026-05-28 09:00', from: 'Supplier Dock', to: 'WH-01 Raw Materials', qty: 500, type: 'Receipt',  operator: 'Tran Thi B' },
      { date: '2026-06-02 11:00', from: 'WH-01 Raw Materials', to: 'WH-02 WIP',     qty: 200, type: 'Transfer', operator: 'Le Van C' },
    ],
    parents: [],
    children: [],
  },
};

const FALLBACK_RESULT = LOT_DB['LOT-2026-0441'];

const STATUS_COLORS: Record<string, { color: string; bg: string; border: string }> = {
  Active:     { color: '#15803D', bg: alpha('#15803D', 0.1), border: alpha('#15803D', 0.3) },
  Consumed:   { color: '#64748B', bg: alpha('#64748B', 0.1), border: alpha('#64748B', 0.3) },
  Quarantine: { color: '#B91C1C', bg: alpha('#B91C1C', 0.1), border: alpha('#B91C1C', 0.3) },
};

const MOVE_TYPE_COLORS: Record<string, string> = {
  Receipt:    '#1D4ED8',
  Transfer:   '#15803D',
  Issue:      '#D97706',
  Return:     '#6D28D9',
  Adjustment: '#64748B',
};

// ─── Genealogy tree component ─────────────────────────────────────────────────

function LotNode({ lotNo, productCode, productName, type }: GenealogyNode) {
  const isCurrent = type === 'current';
  return (
    <Box
      sx={{
        px: 1.5,
        py: 1,
        borderRadius: 1.5,
        border: '1px solid',
        borderColor: isCurrent ? 'primary.main' : 'divider',
        bgcolor: isCurrent ? (t: { palette: { primary: { main: string } } }) => alpha(t.palette.primary.main, 0.06) : 'background.paper',
        minWidth: 160,
        maxWidth: 200,
      }}
    >
      <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 700, color: isCurrent ? 'primary.main' : 'text.secondary', display: 'block' }}>
        {lotNo}
      </Typography>
      <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 10, color: 'text.disabled', display: 'block' }}>
        {productCode}
      </Typography>
      <Typography variant="body2" sx={{ fontSize: 11, mt: 0.25 }}>{productName}</Typography>
      {isCurrent && (
        <Chip label="This lot" size="small" sx={{ mt: 0.5, height: 16, fontSize: '0.6rem', fontWeight: 700, '& .MuiChip-label': { px: 0.75 } }} color="primary" />
      )}
    </Box>
  );
}

function ArrowDown() {
  return (
    <Stack sx={{ alignItems: 'center', py: 0.25 }}>
      <Box sx={{ width: 1, height: 16, bgcolor: 'divider' }} />
      <SolarIcon name="back" size={12} color="#94A3B8" />
    </Stack>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function InventoryTracePage() {
  const [searchParams] = useSearchParams();
  const [lotInput, setLotInput] = useState(searchParams.get('lot') ?? '');
  const [tracedLot, setTracedLot] = useState<string | null>(searchParams.get('lot'));

  function handleTrace() {
    if (lotInput.trim()) setTracedLot(lotInput.trim());
  }

  const result = tracedLot ? (LOT_DB[tracedLot] ?? FALLBACK_RESULT) : null;

  return (
    <PageRoot>
      <PageHeader
        title="Inventory Trace"
        subtitle="Lot genealogy and movement history"
        breadcrumbs={[
          { label: 'Production', href: '/production' },
          { label: 'Inventory', href: '/production/inventory' },
          { label: 'Trace' },
        ]}
      />

      {/* Search bar */}
      <Card sx={{ mb: 2.5 }}>
        <CardContent sx={{ p: 2 }}>
          <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
            <TextField
              value={lotInput}
              onChange={(e) => setLotInput(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleTrace()}
              label="Lot Number"
              placeholder="e.g. LOT-2026-0441"
              size="small"
              sx={{ width: 300 }}
              slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
            />
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="search" size={16} />}
              onClick={handleTrace}
              disabled={!lotInput.trim()}
            >
              Trace
            </Button>
            {tracedLot && (
              <Typography variant="caption" color="text.secondary">
                Showing trace for <strong>{tracedLot}</strong>
                {!LOT_DB[tracedLot] && tracedLot && ' — not found, showing example'}
              </Typography>
            )}
          </Stack>
        </CardContent>
      </Card>

      {!result && (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <SolarIcon name="serial" size={48} color="#CBD5E1" />
          <Typography variant="h6" color="text.secondary" sx={{ mt: 2, fontWeight: 500 }}>Enter a lot number to trace</Typography>
          <Typography variant="body2" color="text.disabled" sx={{ mt: 0.5 }}>
            Search for a lot to view its genealogy, movement history and current status.
          </Typography>
        </Box>
      )}

      {result && (
        <Grid container spacing={2.5}>
          {/* Left: lot info + movements */}
          <Grid size={{ xs: 12, md: 5 }}>
            {/* Lot Info */}
            <Card sx={{ mb: 2.5 }}>
              <CardContent sx={{ p: 3 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Lot Info</Typography>
                <Stack spacing={1.25}>
                  {[
                    { label: 'Lot #',        value: result.info.lotNo, mono: true },
                    { label: 'Product Code', value: result.info.productCode, mono: true },
                    { label: 'Product Name', value: result.info.productName },
                    { label: 'Quantity',     value: `${result.info.quantity.toLocaleString()} ${result.info.uom}` },
                    { label: 'Location',     value: result.info.location },
                    { label: 'Received',     value: result.info.receivedAt },
                  ].map(({ label, value, mono }) => (
                    <Stack key={label} direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                      <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>{label}</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 500, fontFamily: mono ? 'ui-monospace, monospace' : undefined, color: mono ? 'primary.main' : 'text.primary' }}>
                        {value}
                      </Typography>
                    </Stack>
                  ))}
                  <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600 }}>Status</Typography>
                    <Chip
                      label={result.info.status}
                      size="small"
                      sx={{
                        height: 22,
                        fontSize: '0.6875rem',
                        fontWeight: 600,
                        bgcolor: STATUS_COLORS[result.info.status].bg,
                        color: STATUS_COLORS[result.info.status].color,
                        border: `1px solid ${STATUS_COLORS[result.info.status].border}`,
                        '& .MuiChip-label': { px: 1 },
                      }}
                    />
                  </Stack>
                </Stack>
              </CardContent>
            </Card>

            {/* Movement history */}
            <Card>
              <CardContent sx={{ p: 3 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Movement History</Typography>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      {['Date', 'From → To', 'Qty', 'Type', 'By'].map((h) => (
                        <TableCell key={h} sx={{ py: 0.75, fontWeight: 700, fontSize: '0.7rem', color: 'text.secondary', borderColor: 'divider', px: 1 }}>{h}</TableCell>
                      ))}
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {result.movements.map((m, i) => (
                      <TableRow key={i} hover>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider', px: 1 }}>
                          <Typography variant="caption" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 10 }}>{m.date.slice(0, 10)}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider', px: 1, maxWidth: 140 }}>
                          <Typography variant="caption" sx={{ fontSize: 10, display: 'block' }}>{m.from}</Typography>
                          <Typography variant="caption" sx={{ fontSize: 10, color: 'text.disabled' }}>→ {m.to}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider', px: 1 }} align="right">
                          <Typography variant="caption" sx={{ fontWeight: 700 }}>{m.qty > 0 ? `+${m.qty}` : m.qty}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider', px: 1 }}>
                          <Typography variant="caption" sx={{ fontWeight: 600, color: MOVE_TYPE_COLORS[m.type] }}>{m.type}</Typography>
                        </TableCell>
                        <TableCell sx={{ py: 0.75, borderColor: 'divider', px: 1 }}>
                          <Typography variant="caption">{m.operator}</Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </Grid>

          {/* Right: genealogy */}
          <Grid size={{ xs: 12, md: 7 }}>
            <Card sx={{ height: '100%' }}>
              <CardContent sx={{ p: 3 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 2 }}>Lot Genealogy</Typography>

                <Stack sx={{ alignItems: 'center' }}>
                  {/* Parent lots */}
                  {result.parents.length > 0 && (
                    <>
                      <Box>
                        <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600, display: 'block', textAlign: 'center', mb: 1 }}>
                          Parent Lots ({result.parents.length})
                        </Typography>
                        <Stack direction="row" spacing={1.5} sx={{ justifyContent: 'center', flexWrap: 'wrap' }}>
                          {result.parents.map((p) => (
                            <LotNode key={p.lotNo} {...p} />
                          ))}
                        </Stack>
                      </Box>

                      <ArrowDown />
                    </>
                  )}

                  {result.parents.length === 0 && (
                    <Box sx={{ mb: 1.5, textAlign: 'center' }}>
                      <Box
                        sx={{
                          px: 2,
                          py: 1,
                          borderRadius: 1.5,
                          border: '1px dashed',
                          borderColor: 'divider',
                          display: 'inline-block',
                        }}
                      >
                        <Typography variant="caption" color="text.disabled">No parent lots — this is a received/purchased lot</Typography>
                      </Box>
                      <ArrowDown />
                    </Box>
                  )}

                  {/* Current lot */}
                  <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600, display: 'block', textAlign: 'center', mb: 1 }}>
                      This Lot
                    </Typography>
                    <LotNode
                      lotNo={result.info.lotNo}
                      productCode={result.info.productCode}
                      productName={result.info.productName}
                      type="current"
                    />
                  </Box>

                  {/* Child lots */}
                  {result.children.length > 0 && (
                    <>
                      <ArrowDown />
                      <Box>
                        <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: '0.65rem', fontWeight: 600, display: 'block', textAlign: 'center', mb: 1 }}>
                          Child Lots ({result.children.length})
                        </Typography>
                        <Stack direction="row" spacing={1.5} sx={{ justifyContent: 'center', flexWrap: 'wrap' }}>
                          {result.children.map((c) => (
                            <LotNode key={c.lotNo} {...c} />
                          ))}
                        </Stack>
                      </Box>
                    </>
                  )}

                  {result.children.length === 0 && (
                    <>
                      <ArrowDown />
                      <Box
                        sx={{
                          px: 2,
                          py: 1,
                          borderRadius: 1.5,
                          border: '1px dashed',
                          borderColor: 'divider',
                        }}
                      >
                        <Typography variant="caption" color="text.disabled">No child lots yet</Typography>
                      </Box>
                    </>
                  )}
                </Stack>

                {/* Legend */}
                <Stack direction="row" spacing={2} sx={{ mt: 4, justifyContent: 'center', flexWrap: 'wrap' }}>
                  {[
                    { label: 'Current lot',   color: 'primary.main',   dashed: false },
                    { label: 'Related lots',  color: 'divider',        dashed: false },
                    { label: 'No data',       color: 'divider',        dashed: true },
                  ].map((l) => (
                    <Stack key={l.label} direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                      <Box sx={{ width: 24, height: 1.5, bgcolor: l.color, borderStyle: l.dashed ? 'dashed' : 'solid', borderColor: l.color, borderWidth: l.dashed ? 1 : 0 }} />
                      <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11 }}>{l.label}</Typography>
                    </Stack>
                  ))}
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}
    </PageRoot>
  );
}
