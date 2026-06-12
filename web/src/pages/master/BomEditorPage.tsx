import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  IconButton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  ConfirmDialog,
  PageHeader,
  PageRoot,
  SolarIcon,
} from '../../components';

// ─── Types ────────────────────────────────────────────────────────────────────

type BomComponentType = 'Raw' | 'Semi' | 'Purchased';

interface BomRow {
  id: string;
  level: number;
  componentCode: string;
  componentName: string;
  qty: number;
  uom: string;
  type: BomComponentType;
  unitCost: number;
  totalCost: number;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const PRODUCT_META: Record<string, { code: string; name: string; version: string }> = {
  '1': { code: 'FRM-A001', name: 'Frame Assembly A',      version: 'Rev.C' },
  '2': { code: 'PNL-B002', name: 'Panel Sub-assembly B',  version: 'Rev.B' },
  '3': { code: 'SHT-C003', name: 'Shaft Housing C',       version: 'Rev.A' },
};

const INITIAL_BOM: BomRow[] = [
  { id: '1', level: 1, componentCode: 'FRM-A001-01', componentName: 'Main Frame Body',      qty: 1,   uom: 'EA',  type: 'Semi',      unitCost: 45.00, totalCost: 45.00 },
  { id: '2', level: 2, componentCode: 'SHT-001',     componentName: 'Steel Sheet 3mm',      qty: 4,   uom: 'KG',  type: 'Raw',       unitCost: 2.50,  totalCost: 10.00 },
  { id: '3', level: 2, componentCode: 'WLD-001',     componentName: 'Welding Wire ER70S-6', qty: 0.5, uom: 'KG',  type: 'Raw',       unitCost: 8.00,  totalCost: 4.00  },
  { id: '4', level: 1, componentCode: 'FRM-A001-02', componentName: 'Mounting Bracket Set', qty: 4,   uom: 'EA',  type: 'Semi',      unitCost: 12.00, totalCost: 48.00 },
  { id: '5', level: 2, componentCode: 'BRK-D004',    componentName: 'Bracket D',            qty: 4,   uom: 'EA',  type: 'Semi',      unitCost: 8.00,  totalCost: 32.00 },
  { id: '6', level: 2, componentCode: 'FAB-001',     componentName: 'Fastener M8x25',       qty: 16,  uom: 'EA',  type: 'Purchased', unitCost: 0.15,  totalCost: 2.40  },
  { id: '7', level: 1, componentCode: 'FRM-A001-03', componentName: 'Cover Plate Assembly', qty: 1,   uom: 'EA',  type: 'Semi',      unitCost: 28.00, totalCost: 28.00 },
  { id: '8', level: 2, componentCode: 'COV-F006',    componentName: 'Cover Plate F',        qty: 1,   uom: 'EA',  type: 'Semi',      unitCost: 18.00, totalCost: 18.00 },
  { id: '9', level: 2, componentCode: 'GSK-001',     componentName: 'Gasket Set',           qty: 1,   uom: 'SET', type: 'Purchased', unitCost: 6.50,  totalCost: 6.50  },
];

const TYPE_COLORS: Record<BomComponentType, string> = {
  Raw:       '#15803D',
  Semi:      '#1D4ED8',
  Purchased: '#7C3AED',
};

// ─── Inline edit state ────────────────────────────────────────────────────────

interface EditingQty {
  id: string;
  qty: string;
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function BomEditorPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const meta = PRODUCT_META[id ?? ''] ?? PRODUCT_META['1'];

  const [rows, setRows]             = useState<BomRow[]>(INITIAL_BOM);
  const [editingQty, setEditingQty] = useState<EditingQty | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<BomRow | null>(null);
  const [saved, setSaved]           = useState(false);

  const totalCost = rows.reduce((sum, r) => sum + (r.level === 1 ? r.totalCost : 0), 0);
  const componentCount = rows.length;
  const maxLevel = Math.max(...rows.map((r) => r.level));

  function startEditQty(row: BomRow) {
    setEditingQty({ id: row.id, qty: String(row.qty) });
  }

  function saveQty(row: BomRow) {
    if (!editingQty) return;
    const newQty = Number(editingQty.qty);
    if (Number.isNaN(newQty) || newQty <= 0) return;
    setRows((prev) => prev.map((r) =>
      r.id === row.id
        ? { ...r, qty: newQty, totalCost: +(newQty * r.unitCost).toFixed(2) }
        : r
    ));
    setEditingQty(null);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((r) => r.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  function addComponent() {
    const newRow: BomRow = {
      id:            String(Date.now()),
      level:         1,
      componentCode: 'NEW-001',
      componentName: 'New Component',
      qty:           1,
      uom:           'EA',
      type:          'Purchased',
      unitCost:      0,
      totalCost:     0,
    };
    setRows((prev) => [...prev, newRow]);
    setEditingQty({ id: newRow.id, qty: '1' });
  }

  function handleSaveBom() {
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  }

  return (
    <PageRoot>
      <PageHeader
        title={`BOM — ${meta.code}`}
        subtitle={`${meta.name} · ${meta.version}`}
        breadcrumbs={[
          { label: 'Master Data' },
          { label: 'Products', href: '/master/products' },
          { label: meta.code },
          { label: 'BOM' },
        ]}
        actions={
          <>
            <Button
              variant="outlined"
              size="small"
              startIcon={<SolarIcon name="back" size={16} />}
              onClick={() => navigate('/master/products')}
            >
              Back
            </Button>
            <Button
              variant="outlined"
              size="small"
              startIcon={<SolarIcon name="add" size={16} />}
              onClick={addComponent}
            >
              Add Component
            </Button>
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name={saved ? 'success' : 'download'} size={16} />}
              onClick={handleSaveBom}
              color={saved ? 'success' : 'primary'}
            >
              {saved ? 'Saved' : 'Save BOM'}
            </Button>
          </>
        }
      />

      {/* Summary chips */}
      <Card variant="outlined" sx={{ borderRadius: 2, mb: 2 }}>
        <CardContent sx={{ py: '12px !important' }}>
          <Stack direction="row" spacing={1.5} sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5, fontSize: '0.6875rem' }}>
              BOM Summary
            </Typography>
            {[
              { label: `${componentCount} components`, color: '#1D4ED8' },
              { label: `${maxLevel} levels`,           color: '#0D9488' },
              { label: `Total cost ~$${totalCost.toFixed(2)}`, color: '#15803D' },
            ].map((item) => (
              <Chip
                key={item.label}
                label={item.label}
                size="small"
                sx={{
                  height: 22,
                  fontSize: '0.6875rem',
                  fontWeight: 600,
                  bgcolor: alpha(item.color, 0.1),
                  color: item.color,
                  border: `1px solid ${alpha(item.color, 0.2)}`,
                  '& .MuiChip-label': { px: 0.75 },
                }}
              />
            ))}
          </Stack>
        </CardContent>
      </Card>

      {/* BOM Table */}
      <Card variant="outlined" sx={{ borderRadius: 2 }}>
        <CardContent sx={{ pb: '12px !important' }}>
          <Typography
            variant="subtitle2"
            sx={{ mb: 2, fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', fontSize: '0.6875rem', letterSpacing: 0.5 }}
          >
            BOM Structure
          </Typography>

          <Table size="small">
            <TableHead>
              <TableRow
                sx={{ '& th': { py: 0.75, fontWeight: 600, fontSize: '0.75rem', borderColor: 'divider', color: 'text.secondary', bgcolor: (t) => alpha(t.palette.primary.main, 0.03) } }}
              >
                <TableCell width={90}>Level</TableCell>
                <TableCell width={150}>Component Code</TableCell>
                <TableCell>Component Name</TableCell>
                <TableCell width={80} align="right">Qty</TableCell>
                <TableCell width={60} align="center">UOM</TableCell>
                <TableCell width={100} align="center">Type</TableCell>
                <TableCell width={100} align="right">Unit Cost</TableCell>
                <TableCell width={100} align="right">Total Cost</TableCell>
                <TableCell width={80} align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.map((row) => {
                const isEditingThisQty = editingQty?.id === row.id;
                const typeColor = TYPE_COLORS[row.type];
                return (
                  <TableRow
                    key={row.id}
                    sx={{
                      '& td': { py: 0.5, fontSize: '0.8125rem', borderColor: 'divider' },
                      '&:last-child td': { border: 0 },
                      bgcolor: row.level === 1 ? (t) => alpha(t.palette.primary.main, 0.015) : 'transparent',
                    }}
                  >
                    {/* Level */}
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, ml: (row.level - 1) * 2 }}>
                        <Chip
                          label={`L${row.level}`}
                          size="small"
                          sx={{
                            height: 18,
                            fontSize: '0.625rem',
                            fontWeight: 700,
                            bgcolor: row.level === 1 ? alpha('#1D4ED8', 0.12) : alpha('#94A3B8', 0.12),
                            color: row.level === 1 ? '#1D4ED8' : '#64748B',
                            border: 'none',
                            '& .MuiChip-label': { px: 0.5 },
                          }}
                        />
                      </Box>
                    </TableCell>

                    {/* Component Code */}
                    <TableCell>
                      <Box sx={{ ml: (row.level - 1) * 2 }}>
                        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
                          {row.componentCode}
                        </Typography>
                      </Box>
                    </TableCell>

                    {/* Component Name */}
                    <TableCell>
                      <Typography variant="body2" sx={{ fontSize: 12, fontWeight: row.level === 1 ? 600 : 400 }}>
                        {row.componentName}
                      </Typography>
                    </TableCell>

                    {/* Qty */}
                    <TableCell align="right">
                      {isEditingThisQty ? (
                        <TextField
                          size="small"
                          type="number"
                          value={editingQty.qty}
                          onChange={(e) => setEditingQty((prev) => prev ? { ...prev, qty: e.target.value } : null)}
                          onBlur={() => saveQty(row)}
                          onKeyDown={(e) => { if (e.key === 'Enter') saveQty(row); if (e.key === 'Escape') setEditingQty(null); }}
                          autoFocus
                          sx={{ width: 70 }}
                          slotProps={{ htmlInput: { style: { fontSize: 12, textAlign: 'right' }, min: 0.01, step: 0.01 } }}
                        />
                      ) : (
                        <Typography
                          variant="body2"
                          sx={{ fontSize: 12, cursor: 'pointer', '&:hover': { color: 'primary.main', textDecoration: 'underline' } }}
                          onClick={() => startEditQty(row)}
                        >
                          {row.qty}
                        </Typography>
                      )}
                    </TableCell>

                    {/* UOM */}
                    <TableCell align="center">
                      <Typography variant="body2" color="text.secondary" sx={{ fontSize: 11 }}>{row.uom}</Typography>
                    </TableCell>

                    {/* Type */}
                    <TableCell align="center">
                      <Chip
                        label={row.type}
                        size="small"
                        sx={{
                          height: 18,
                          fontSize: '0.625rem',
                          fontWeight: 600,
                          bgcolor: alpha(typeColor, 0.1),
                          color: typeColor,
                          border: 'none',
                          '& .MuiChip-label': { px: 0.5 },
                        }}
                      />
                    </TableCell>

                    {/* Unit Cost */}
                    <TableCell align="right">
                      <Typography variant="body2" sx={{ fontSize: 12 }}>
                        {row.unitCost > 0 ? `$${row.unitCost.toFixed(2)}` : '—'}
                      </Typography>
                    </TableCell>

                    {/* Total Cost */}
                    <TableCell align="right">
                      <Typography variant="body2" sx={{ fontSize: 12, fontWeight: row.level === 1 ? 600 : 400 }}>
                        {row.totalCost > 0 ? `$${row.totalCost.toFixed(2)}` : '—'}
                      </Typography>
                    </TableCell>

                    {/* Actions */}
                    <TableCell align="center">
                      <Stack direction="row" spacing={0.25} sx={{ justifyContent: 'center' }}>
                        <Tooltip title="Edit Qty">
                          <IconButton size="small" onClick={() => startEditQty(row)} sx={{ color: 'text.secondary' }}>
                            <SolarIcon name="edit" size={14} />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Delete">
                          <IconButton size="small" onClick={() => setDeleteTarget(row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
                            <SolarIcon name="delete" size={14} />
                          </IconButton>
                        </Tooltip>
                      </Stack>
                    </TableCell>
                  </TableRow>
                );
              })}

              {rows.length === 0 && (
                <TableRow>
                  <TableCell colSpan={9} sx={{ textAlign: 'center', py: 4, color: 'text.secondary', fontSize: 13 }}>
                    No BOM components defined. Click "Add Component" to start.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>

          {/* Total row */}
          {rows.length > 0 && (
            <Box
              sx={{
                mt: 1,
                pt: 1,
                borderTop: '2px solid',
                borderColor: 'divider',
                display: 'flex',
                justifyContent: 'flex-end',
                pr: 10,
              }}
            >
              <Typography variant="body2" sx={{ fontWeight: 700, fontSize: 13 }}>
                Total Material Cost:&nbsp;
                <Typography component="span" variant="body2" sx={{ fontWeight: 800, color: '#15803D', fontSize: 14 }}>
                  ${totalCost.toFixed(2)}
                </Typography>
              </Typography>
            </Box>
          )}

          {/* Footer note */}
          <Box
            sx={{
              mt: 2,
              p: 1.5,
              borderRadius: 1,
              bgcolor: (t) => alpha(t.palette.warning.main, 0.06),
              border: '1px solid',
              borderColor: (t) => alpha(t.palette.warning.main, 0.2),
            }}
          >
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <SolarIcon name="warning" size={14} color="#D97706" />
              <Typography variant="caption" color="text.secondary">
                BOM changes take effect from next released Work Order
              </Typography>
            </Stack>
          </Box>
        </CardContent>
      </Card>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Remove BOM Component"
        description={
          <>
            Remove <strong>{deleteTarget?.componentName}</strong> ({deleteTarget?.componentCode}) from this BOM?
            This action cannot be undone.
          </>
        }
        confirmLabel="Remove"
        confirmColor="error"
      />
    </PageRoot>
  );
}
