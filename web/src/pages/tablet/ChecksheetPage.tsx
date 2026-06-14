import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  InputAdornment,
  LinearProgress,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import SolarIcon from '../../components/SolarIcon';
import { useGetApiV1SopInstancesJobId, useGetApiV1SopDocumentsId, putApiV1SopInstancesInstanceIdItemsItemId } from '../../api/sop/sop';
import type { CheckItemDto, CheckItemResultDto } from '../../api/model';
import { useTabletSession } from '../../contexts/TabletSessionContext';

// ── Types ─────────────────────────────────────────────────────────────────────

type FieldResult = 'PASS' | 'FAIL' | 'NA' | '';

interface ItemState {
  result: FieldResult;
  measuredValue: string;
  notes: string;
  photoUrl: string;
  touched: boolean;
}

// ── Helpers ───────────────────────────────────────name────────────────────────

function numVal(v: number | string | null | undefined): number {
  if (v == null) return 0;
  return typeof v === 'number' ? v : parseFloat(v as string) || 0;
}

function isNumericItem(item: CheckItemDto): boolean {
  return item.specMin !== null || item.specMax !== null;
}

function resultForNumeric(value: string, item: CheckItemDto): FieldResult {
  const v = parseFloat(value);
  if (isNaN(v)) return '';
  const min = item.specMin !== null ? numVal(item.specMin) : -Infinity;
  const max = item.specMax !== null ? numVal(item.specMax) : Infinity;
  return v >= min && v <= max ? 'PASS' : 'FAIL';
}

// ── Item Card ─────────────────────────────────────────────────────────────────

interface ItemCardProps {
  item: CheckItemDto;
  existingResult?: CheckItemResultDto;
  state: ItemState;
  onChange: (state: Partial<ItemState>) => void;
  onRecord: () => void;
  recording: boolean;
  submitAttempted: boolean;
}

function ItemCard({ item, existingResult, state, onChange, onRecord, recording, submitAttempted }: ItemCardProps) {
  const isCompleted = existingResult && existingResult.result !== 'IN_PROGRESS';
  const isMissing = submitAttempted && item.isRequired && !isCompleted && !state.result;

  const borderColor = isCompleted
    ? existingResult.result === 'PASS' ? '#10B981' : '#EF4444'
    : isMissing ? '#EF4444' : undefined;

  function handlePassFail(r: 'PASS' | 'FAIL') {
    onChange({ result: r, touched: true });
  }

  return (
    <Card
      variant="outlined"
      sx={{
        borderColor,
        borderWidth: borderColor ? 2 : 1,
        opacity: isCompleted ? 0.85 : 1,
      }}
    >
      <CardContent sx={{ pb: '12px !important' }}>
        {/* Header */}
        <Stack direction="row" sx={{ alignItems: 'flex-start', justifyContent: 'space-between', mb: 1 }}>
          <Stack spacing={0.25} sx={{ flex: 1 }}>
            <Stack direction="row" sx={{ gap: 0.75, flexWrap: 'wrap', alignItems: 'center' }}>
              <Chip label={item.category} size="small" sx={{ fontSize: 11 }} />
              {item.isRequired && <Chip label="Required" size="small" color="error" sx={{ fontSize: 11 }} />}
            </Stack>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, mt: 0.5 }}>
              {numVal(item.sequence)}. {item.itemText}
            </Typography>
            {isNumericItem(item) && (
              <Typography variant="caption" color="text.secondary">
                Spec: {item.specMin !== null ? numVal(item.specMin) : '—'} to {item.specMax !== null ? numVal(item.specMax) : '—'} {item.unit ?? ''}
              </Typography>
            )}
          </Stack>
          {isCompleted && (
            <Chip
              label={existingResult.result}
              color={existingResult.result === 'PASS' ? 'success' : 'error'}
              size="small"
              icon={<SolarIcon name={existingResult.result === 'PASS' ? 'complete' : 'close'} size={14} />}
            />
          )}
        </Stack>

        {isMissing && (
          <Alert severity="error" sx={{ mb: 1, py: 0 }}>Required field — please complete before submitting</Alert>
        )}

        {!isCompleted && (
          <>
            {/* Numeric input */}
            {isNumericItem(item) && (
              <TextField
                fullWidth
                size="small"
                type="number"
                label="Measured value"
                value={state.measuredValue}
                onChange={(e) => {
                  const v = e.target.value;
                  onChange({ measuredValue: v, result: v ? resultForNumeric(v, item) : '', touched: true });
                }}
                slotProps={{
                  input: {
                    endAdornment: item.unit ? <InputAdornment position="end">{item.unit}</InputAdornment> : undefined,
                  },
                }}
                sx={{ mb: 1 }}
                color={state.result === 'PASS' ? 'success' : state.result === 'FAIL' ? 'error' : undefined}
              />
            )}

            {/* Pass / Fail toggle */}
            {!isNumericItem(item) && (
              <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Button
                  variant={state.result === 'PASS' ? 'contained' : 'outlined'}
                  color="success"
                  size="large"
                  onClick={() => handlePassFail('PASS')}
                  sx={{ flex: 1, height: 52, fontSize: 16 }}
                  startIcon={<SolarIcon name="complete" size={20} />}
                >
                  PASS
                </Button>
                <Button
                  variant={state.result === 'FAIL' ? 'contained' : 'outlined'}
                  color="error"
                  size="large"
                  onClick={() => handlePassFail('FAIL')}
                  sx={{ flex: 1, height: 52, fontSize: 16 }}
                  startIcon={<SolarIcon name="close" size={20} />}
                >
                  FAIL
                </Button>
              </Stack>
            )}

            {/* Notes */}
            <TextField
              fullWidth
              size="small"
              multiline
              rows={1}
              placeholder="Notes (optional)"
              value={state.notes}
              onChange={(e) => onChange({ notes: e.target.value, touched: true })}
              sx={{ mb: 1 }}
            />

            {/* Photo upload */}
            {item.photoRequired && (
              <Box sx={{ mb: 1 }}>
                <Button
                  component="label"
                  variant="outlined"
                  size="small"
                  startIcon={<SolarIcon name="quality" size={16} />}
                >
                  {item.photoRequired ? 'Upload photo *' : 'Upload photo'}
                  <input
                    type="file"
                    accept="image/*"
                    capture="environment"
                    hidden
                    onChange={(e) => {
                      const file = e.target.files?.[0];
                      if (file) onChange({ photoUrl: `uploaded:${file.name}`, touched: true });
                    }}
                  />
                </Button>
                {state.photoUrl && (
                  <Typography variant="caption" color="success.main" sx={{ ml: 1 }}>{state.photoUrl.replace('uploaded:', '')}</Typography>
                )}
              </Box>
            )}

            {/* Record button */}
            <Button
              fullWidth
              variant="contained"
              size="large"
              disabled={!state.result || recording || (item.photoRequired && !state.photoUrl)}
              onClick={onRecord}
              startIcon={recording ? <CircularProgress size={18} color="inherit" /> : <SolarIcon name="complete" size={18} />}
            >
              Record
            </Button>
          </>
        )}

        {isCompleted && existingResult.notes && (
          <Typography variant="caption" color="text.secondary">
            Notes: {existingResult.notes}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function ChecksheetPage() {
  const navigate = useNavigate();
  const { session } = useTabletSession();
  const [itemStates, setItemStates] = useState<Record<number, ItemState>>({});
  const [submitAttempted, setSubmitAttempted] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const [recordingItem, setRecordingItem] = useState<number | null>(null);

  const jobId = session.jobId;

  const { data: instanceResp, isLoading: instanceLoading, refetch } = useGetApiV1SopInstancesJobId(
    jobId ?? 0,
    { query: { enabled: !!jobId } },
  );

  const instance = (instanceResp as { data?: typeof instanceResp })?.data ?? instanceResp as any;

  const sopId = instance ? numVal((instance as any).sopId) : 0;

  const { data: docResp, isLoading: docLoading } = useGetApiV1SopDocumentsId(
    sopId,
    { query: { enabled: !!sopId && sopId > 0 } },
  );
  const doc = (docResp as { data?: typeof docResp })?.data ?? docResp as any;
  const items: CheckItemDto[] = doc?.items ?? [];
  const results: CheckItemResultDto[] = (instance as any)?.results ?? [];

  const instanceId = instance ? numVal((instance as any).instanceId) : 0;

  const recordMutation = useMutation({
    mutationFn: ({ itemId, state }: { itemId: number; state: ItemState }) =>
      putApiV1SopInstancesInstanceIdItemsItemId(instanceId, itemId, {
        result: state.result || 'PASS',
        measuredValue: state.measuredValue ? parseFloat(state.measuredValue) : null,
        notes: state.notes || null,
        photoUrl: state.photoUrl || null,
      }),
    onSuccess: () => refetch(),
  });

  const getState = useCallback((itemId: number): ItemState =>
    itemStates[itemId] ?? { result: '', measuredValue: '', notes: '', photoUrl: '', touched: false },
  [itemStates]);

  const updateState = useCallback((itemId: number, patch: Partial<ItemState>) =>
    setItemStates((prev) => ({ ...prev, [itemId]: { ...getState(itemId), ...patch } })),
  [getState]);

  function getResult(itemId: number): CheckItemResultDto | undefined {
    return results.find((r) => numVal(r.checkItemId) === itemId);
  }

  async function handleRecord(item: CheckItemDto) {
    const itemId = numVal(item.checkItemId);
    const state = getState(itemId);
    setRecordingItem(itemId);
    try {
      await recordMutation.mutateAsync({ itemId, state });
    } finally {
      setRecordingItem(null);
    }
  }

  function handleSubmitAll() {
    setSubmitAttempted(true);
    const unfinishedRequired = items.filter((item) => {
      const itemId = numVal(item.checkItemId);
      const existing = getResult(itemId);
      const isComplete = existing && existing.result !== 'IN_PROGRESS';
      return item.isRequired && !isComplete;
    });
    if (unfinishedRequired.length > 0) {
      setSubmitError(`${unfinishedRequired.length} required field(s) not completed.`);
      return;
    }
    setSubmitError('');
    navigate('/tablet/station');
  }

  const isLoading = instanceLoading || docLoading;

  const completedCount = items.filter((item) => {
    const existing = getResult(numVal(item.checkItemId));
    return existing && existing.result !== 'IN_PROGRESS';
  }).length;

  if (!jobId) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning">No active job. Please start a job first.</Alert>
        <Button sx={{ mt: 2 }} onClick={() => navigate('/tablet/station')}>Back</Button>
      </Box>
    );
  }

  if (isLoading) {
    return (
      <Box sx={{ p: 3, display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
        <CircularProgress />
        <Typography color="text.secondary">Loading checksheet…</Typography>
      </Box>
    );
  }

  if (!instance || !doc) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="info">No checksheet attached to this job.</Alert>
        <Button sx={{ mt: 2 }} onClick={() => navigate('/tablet/station')}>Back</Button>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100dvh', bgcolor: 'background.default', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Box
        sx={{
          bgcolor: 'primary.main',
          color: 'primary.contrastText',
          px: 2, py: 1.5,
          display: 'flex', alignItems: 'center', gap: 1.5,
        }}
      >
        <SolarIcon name="complete" size={22} />
        <Box sx={{ flex: 1 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 700, lineHeight: 1 }}>
            Checksheet
          </Typography>
          <Typography variant="caption" sx={{ opacity: 0.85 }}>
            {(doc as any).code} — {(doc as any).title}
          </Typography>
        </Box>
        <Typography variant="caption" sx={{ opacity: 0.85 }}>
          {completedCount}/{items.length}
        </Typography>
      </Box>

      {/* Progress */}
      <LinearProgress
        variant="determinate"
        value={items.length > 0 ? (completedCount / items.length) * 100 : 0}
        sx={{ height: 4 }}
        color={completedCount === items.length ? 'success' : 'primary'}
      />

      {/* Items */}
      <Box sx={{ flex: 1, overflowY: 'auto', p: 2 }}>
        <Stack spacing={2}>
          {items.map((item) => {
            const itemId = numVal(item.checkItemId);
            return (
              <ItemCard
                key={itemId}
                item={item}
                existingResult={getResult(itemId)}
                state={getState(itemId)}
                onChange={(patch) => updateState(itemId, patch)}
                onRecord={() => handleRecord(item)}
                recording={recordingItem === itemId}
                submitAttempted={submitAttempted}
              />
            );
          })}
        </Stack>

        {items.length === 0 && (
          <Alert severity="info">This SOP has no check items.</Alert>
        )}
      </Box>

      {/* Footer */}
      <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider', bgcolor: 'background.paper' }}>
        {submitError && <Alert severity="error" sx={{ mb: 1.5 }}>{submitError}</Alert>}
        <Stack direction="row" spacing={1.5}>
          <Button
            variant="outlined"
            size="large"
            onClick={() => navigate('/tablet/station')}
            sx={{ flex: 1 }}
          >
            Back
          </Button>
          <Button
            variant="contained"
            size="large"
            color={completedCount === items.length ? 'success' : 'primary'}
            onClick={handleSubmitAll}
            sx={{ flex: 2 }}
            startIcon={<SolarIcon name="complete" size={20} />}
          >
            {completedCount === items.length ? 'Complete Checksheet' : `Submit (${completedCount}/${items.length})`}
          </Button>
        </Stack>
      </Box>
    </Box>
  );
}
