import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  IconButton,
  MenuItem,
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

interface RoutingStep {
  id: string;
  seq: number;
  operationCode: string;
  operationName: string;
  machineType: string;
  workCenter: string;
  setupTimeMin: number;
  cycleTimeSec: number;
  isActive: boolean;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const ROUTING_META: Record<string, { code: string; productCode: string; productName: string; version: string }> = {
  '1': { code: 'RT-001', productCode: 'FRM-A001', productName: 'Frame Assembly A',      version: 'Rev.C' },
  '2': { code: 'RT-002', productCode: 'PNL-B002', productName: 'Panel Sub-assembly B',  version: 'Rev.B' },
  '3': { code: 'RT-003', productCode: 'SHT-C003', productName: 'Shaft Housing C',       version: 'Rev.A' },
  '4': { code: 'RT-004', productCode: 'BRK-D004', productName: 'Bracket Set D',         version: 'Rev.A' },
  '5': { code: 'RT-005', productCode: 'MTR-E005', productName: 'Motor Mount E',         version: 'Rev.B' },
  '6': { code: 'RT-006', productCode: 'COV-F006', productName: 'Cover Plate F',         version: 'Rev.A' },
  '7': { code: 'RT-007', productCode: 'HNG-J010', productName: 'Hinge Assembly J',      version: 'Rev.D' },
  '8': { code: 'RT-008', productCode: 'WHL-L012', productName: 'Wheel & Hub Assembly L',version: 'Rev.C' },
};

const INITIAL_STEPS: RoutingStep[] = [
  { id: '1', seq: 10, operationCode: 'OP-001', operationName: 'CNC Turning',       machineType: 'Lathe',      workCenter: 'WC-009', setupTimeMin: 30,  cycleTimeSec: 45,  isActive: true },
  { id: '2', seq: 20, operationCode: 'OP-007', operationName: 'Pressing',          machineType: 'Press',      workCenter: 'WC-010', setupTimeMin: 25,  cycleTimeSec: 30,  isActive: true },
  { id: '3', seq: 30, operationCode: 'OP-008', operationName: 'Deburring',         machineType: 'Manual',     workCenter: 'WC-001', setupTimeMin: 10,  cycleTimeSec: 90,  isActive: true },
  { id: '4', seq: 40, operationCode: 'OP-003', operationName: 'MIG Welding',       machineType: 'Welding',    workCenter: 'WC-003', setupTimeMin: 20,  cycleTimeSec: 180, isActive: true },
  { id: '5', seq: 50, operationCode: 'OP-005', operationName: 'Painting',          machineType: 'Painting',   workCenter: 'WC-005', setupTimeMin: 30,  cycleTimeSec: 300, isActive: true },
  { id: '6', seq: 60, operationCode: 'OP-004', operationName: 'Visual Inspection', machineType: 'Inspection', workCenter: 'WC-004', setupTimeMin: 5,   cycleTimeSec: 60,  isActive: true },
];

const AVAILABLE_OPS = [
  { code: 'OP-001', name: 'CNC Turning',       machineType: 'Lathe',      workCenter: 'WC-009' },
  { code: 'OP-002', name: 'CNC Milling',       machineType: 'Mill',       workCenter: 'WC-007' },
  { code: 'OP-003', name: 'MIG Welding',       machineType: 'Welding',    workCenter: 'WC-003' },
  { code: 'OP-004', name: 'Visual Inspection', machineType: 'Inspection', workCenter: 'WC-004' },
  { code: 'OP-005', name: 'Painting',          machineType: 'Painting',   workCenter: 'WC-005' },
  { code: 'OP-006', name: 'Assembly A',        machineType: 'Assembly',   workCenter: 'WC-002' },
  { code: 'OP-007', name: 'Pressing',          machineType: 'Press',      workCenter: 'WC-010' },
  { code: 'OP-008', name: 'Deburring',         machineType: 'Manual',     workCenter: 'WC-001' },
  { code: 'OP-009', name: 'Heat Treatment',    machineType: 'Furnace',    workCenter: 'WC-001' },
  { code: 'OP-010', name: 'CMM Inspection',    machineType: 'Inspection', workCenter: 'WC-004' },
];

// ─── Inline edit row ──────────────────────────────────────────────────────────

interface EditingValues {
  operationCode: string;
  setupTimeMin: string;
  cycleTimeSec: string;
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function RoutingStepsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const meta = ROUTING_META[id ?? ''] ?? ROUTING_META['1'];

  const [steps, setSteps]               = useState<RoutingStep[]>(INITIAL_STEPS);
  const [editingId, setEditingId]       = useState<string | null>(null);
  const [editValues, setEditValues]     = useState<EditingValues>({ operationCode: '', setupTimeMin: '', cycleTimeSec: '' });
  const [deleteTarget, setDeleteTarget] = useState<RoutingStep | null>(null);

  function startEdit(step: RoutingStep) {
    setEditingId(step.id);
    setEditValues({
      operationCode: step.operationCode,
      setupTimeMin:  String(step.setupTimeMin),
      cycleTimeSec:  String(step.cycleTimeSec),
    });
  }

  function cancelEdit() {
    setEditingId(null);
  }

  function saveEdit(step: RoutingStep) {
    const op = AVAILABLE_OPS.find((o) => o.code === editValues.operationCode);
    setSteps((prev) => prev.map((s) =>
      s.id === step.id
        ? {
            ...s,
            operationCode: editValues.operationCode,
            operationName: op?.name ?? s.operationName,
            machineType:   op?.machineType ?? s.machineType,
            workCenter:    op?.workCenter ?? s.workCenter,
            setupTimeMin:  Number(editValues.setupTimeMin) || 0,
            cycleTimeSec:  Number(editValues.cycleTimeSec) || 0,
          }
        : s
    ));
    setEditingId(null);
  }

  function handleDelete() {
    if (deleteTarget) {
      setSteps((prev) => prev.filter((s) => s.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  function moveStep(step: RoutingStep, direction: 'up' | 'down') {
    const sorted = [...steps].sort((a, b) => a.seq - b.seq);
    const idx = sorted.findIndex((s) => s.id === step.id);
    if (direction === 'up' && idx === 0) return;
    if (direction === 'down' && idx === sorted.length - 1) return;
    const swapIdx = direction === 'up' ? idx - 1 : idx + 1;
    const newSeq = sorted[swapIdx].seq;
    const swapSeq = sorted[idx].seq;
    setSteps((prev) => prev.map((s) => {
      if (s.id === step.id) return { ...s, seq: newSeq };
      if (s.id === sorted[swapIdx].id) return { ...s, seq: swapSeq };
      return s;
    }));
  }

  function addStep() {
    const maxSeq = steps.length > 0 ? Math.max(...steps.map((s) => s.seq)) : 0;
    const newStep: RoutingStep = {
      id:            String(Date.now()),
      seq:           maxSeq + 10,
      operationCode: 'OP-001',
      operationName: 'CNC Turning',
      machineType:   'Lathe',
      workCenter:    'WC-009',
      setupTimeMin:  0,
      cycleTimeSec:  0,
      isActive:      true,
    };
    setSteps((prev) => [...prev, newStep]);
    setEditingId(newStep.id);
    setEditValues({ operationCode: 'OP-001', setupTimeMin: '0', cycleTimeSec: '0' });
  }

  const sorted = [...steps].sort((a, b) => a.seq - b.seq);

  return (
    <PageRoot>
      <PageHeader
        title={`Routing Steps — ${meta.code}`}
        subtitle={`${meta.productName} · ${meta.version}`}
        breadcrumbs={[
          { label: 'Master Data' },
          { label: 'Routings', href: '/master/routings' },
          { label: meta.code, href: `/master/routings` },
          { label: 'Steps' },
        ]}
        actions={
          <>
            <Button
              variant="outlined"
              size="small"
              startIcon={<SolarIcon name="back" size={16} />}
              onClick={() => navigate('/master/routings')}
            >
              Back
            </Button>
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="add" size={16} />}
              onClick={addStep}
            >
              Add Step
            </Button>
          </>
        }
      />

      <Card variant="outlined" sx={{ borderRadius: 2 }}>
        <CardContent sx={{ pb: '12px !important' }}>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 2 }}>
            <Typography
              variant="subtitle2"
              sx={{ fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', fontSize: '0.6875rem', letterSpacing: 0.5 }}
            >
              Step List
            </Typography>
            <Chip
              label={`${sorted.length} steps`}
              size="small"
              sx={(theme) => ({
                height: 18,
                fontSize: '0.625rem',
                fontWeight: 600,
                bgcolor: alpha(theme.palette.primary.main, 0.08),
                color: 'primary.main',
                border: 'none',
                '& .MuiChip-label': { px: 0.5 },
              })}
            />
          </Stack>

          <Table size="small">
            <TableHead>
              <TableRow
                sx={{ '& th': { py: 0.75, fontWeight: 600, fontSize: '0.75rem', borderColor: 'divider', color: 'text.secondary', bgcolor: (t) => alpha(t.palette.primary.main, 0.03) } }}
              >
                <TableCell width={60}>Seq #</TableCell>
                <TableCell width={130}>Operation Code</TableCell>
                <TableCell>Operation Name</TableCell>
                <TableCell width={120}>Machine Type</TableCell>
                <TableCell width={110}>Work Center</TableCell>
                <TableCell width={100} align="right">Setup (min)</TableCell>
                <TableCell width={100} align="right">Cycle (sec)</TableCell>
                <TableCell width={70} align="center">Active</TableCell>
                <TableCell width={130} align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {sorted.map((step, idx) => {
                const isEditing = editingId === step.id;
                return (
                  <TableRow
                    key={step.id}
                    sx={{
                      '& td': { py: 0.5, fontSize: '0.8125rem', borderColor: 'divider' },
                      '&:last-child td': { border: 0 },
                      bgcolor: isEditing ? (t) => alpha(t.palette.primary.main, 0.03) : 'transparent',
                    }}
                  >
                    {/* Seq */}
                    <TableCell>
                      <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'text.secondary' }}>
                        {step.seq}
                      </Typography>
                    </TableCell>

                    {/* Operation Code */}
                    <TableCell>
                      {isEditing ? (
                        <TextField
                          select
                          size="small"
                          value={editValues.operationCode}
                          onChange={(e) => {
                            const op = AVAILABLE_OPS.find((o) => o.code === e.target.value);
                            setEditValues((prev) => ({ ...prev, operationCode: e.target.value }));
                            if (op) {
                              setSteps((prev) => prev.map((s) =>
                                s.id === step.id
                                  ? { ...s, operationName: op.name, machineType: op.machineType, workCenter: op.workCenter }
                                  : s
                              ));
                            }
                          }}
                          sx={{ width: 120 }}
                          slotProps={{ htmlInput: { style: { fontSize: 12, fontFamily: 'ui-monospace, monospace' } } }}
                        >
                          {AVAILABLE_OPS.map((o) => (
                            <MenuItem key={o.code} value={o.code} sx={{ fontSize: 12, fontFamily: 'ui-monospace, monospace' }}>{o.code}</MenuItem>
                          ))}
                        </TextField>
                      ) : (
                        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
                          {step.operationCode}
                        </Typography>
                      )}
                    </TableCell>

                    {/* Operation Name */}
                    <TableCell>
                      <Typography variant="body2" sx={{ fontSize: 12 }}>{step.operationName}</Typography>
                    </TableCell>

                    {/* Machine Type */}
                    <TableCell>
                      <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>{step.machineType}</Typography>
                    </TableCell>

                    {/* Work Center */}
                    <TableCell>
                      <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, color: 'text.secondary' }}>
                        {step.workCenter}
                      </Typography>
                    </TableCell>

                    {/* Setup Time */}
                    <TableCell align="right">
                      {isEditing ? (
                        <TextField
                          size="small"
                          type="number"
                          value={editValues.setupTimeMin}
                          onChange={(e) => setEditValues((prev) => ({ ...prev, setupTimeMin: e.target.value }))}
                          sx={{ width: 70 }}
                          slotProps={{ htmlInput: { style: { fontSize: 12, textAlign: 'right' }, min: 0 } }}
                        />
                      ) : (
                        <Typography variant="body2" sx={{ fontSize: 12 }}>{step.setupTimeMin}</Typography>
                      )}
                    </TableCell>

                    {/* Cycle Time */}
                    <TableCell align="right">
                      {isEditing ? (
                        <TextField
                          size="small"
                          type="number"
                          value={editValues.cycleTimeSec}
                          onChange={(e) => setEditValues((prev) => ({ ...prev, cycleTimeSec: e.target.value }))}
                          sx={{ width: 70 }}
                          slotProps={{ htmlInput: { style: { fontSize: 12, textAlign: 'right' }, min: 0 } }}
                        />
                      ) : (
                        <Typography variant="body2" sx={{ fontSize: 12 }}>
                          {step.cycleTimeSec > 0 ? step.cycleTimeSec : '—'}
                        </Typography>
                      )}
                    </TableCell>

                    {/* Active */}
                    <TableCell align="center">
                      <Chip
                        label={step.isActive ? 'Yes' : 'No'}
                        size="small"
                        sx={{
                          height: 18,
                          fontSize: '0.625rem',
                          fontWeight: 600,
                          bgcolor: step.isActive ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
                          color: step.isActive ? '#15803D' : '#94A3B8',
                          border: 'none',
                          '& .MuiChip-label': { px: 0.5 },
                        }}
                      />
                    </TableCell>

                    {/* Actions */}
                    <TableCell align="center">
                      <Stack direction="row" spacing={0.25} sx={{ justifyContent: 'center' }}>
                        {isEditing ? (
                          <>
                            <Tooltip title="Save">
                              <IconButton size="small" onClick={() => saveEdit(step)} sx={{ color: '#15803D' }}>
                                <SolarIcon name="success" size={15} />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Cancel">
                              <IconButton size="small" onClick={cancelEdit} sx={{ color: 'text.secondary' }}>
                                <SolarIcon name="close" size={15} />
                              </IconButton>
                            </Tooltip>
                          </>
                        ) : (
                          <>
                            <Tooltip title="Edit">
                              <IconButton size="small" onClick={() => startEdit(step)} sx={{ color: 'text.secondary' }}>
                                <SolarIcon name="edit" size={15} />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Move Up">
                              <span>
                                <IconButton size="small" onClick={() => moveStep(step, 'up')} disabled={idx === 0} sx={{ color: 'text.secondary' }}>
                                  <SolarIcon name="collapse" size={15} />
                                </IconButton>
                              </span>
                            </Tooltip>
                            <Tooltip title="Move Down">
                              <span>
                                <IconButton size="small" onClick={() => moveStep(step, 'down')} disabled={idx === sorted.length - 1} sx={{ color: 'text.secondary' }}>
                                  <SolarIcon name="expand" size={15} />
                                </IconButton>
                              </span>
                            </Tooltip>
                            <Tooltip title="Delete">
                              <IconButton size="small" onClick={() => setDeleteTarget(step)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
                                <SolarIcon name="delete" size={15} />
                              </IconButton>
                            </Tooltip>
                          </>
                        )}
                      </Stack>
                    </TableCell>
                  </TableRow>
                );
              })}

              {sorted.length === 0 && (
                <TableRow>
                  <TableCell colSpan={9} sx={{ textAlign: 'center', py: 4, color: 'text.secondary', fontSize: 13 }}>
                    No steps defined yet. Click "Add Step" to create the first step.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>

          {sorted.length > 0 && (
            <Box sx={{ mt: 1.5, p: 1.5, borderRadius: 1, bgcolor: (t) => alpha(t.palette.warning.main, 0.06), border: '1px solid', borderColor: (t) => alpha(t.palette.warning.main, 0.2) }}>
              <Typography variant="caption" color="text.secondary">
                BOM changes take effect from next released Work Order
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Step"
        description={
          <>
            Delete step <strong>#{deleteTarget?.seq} — {deleteTarget?.operationName}</strong>?
            This will remove it from the routing sequence.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
