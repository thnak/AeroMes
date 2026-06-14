import {
  Alert,
  Button,
  Chip,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRowSelectionModel } from '@mui/x-data-grid';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { EmptyState, FormDrawer, PageHeader, PageRoot, RefreshButton } from '../../components';
import { apiClient } from '../../lib/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface LotHoldDto {
  holdId: string;
  lotNumber: string;
  holdReason: string;
  holdType: string;
  placedBy: string;
  placedAt: string;
  dispositionDecision?: string;
  releasedAt?: string;
  notes?: string;
}

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchActiveHolds = (): Promise<LotHoldDto[]> =>
  apiClient.get('/api/v1/trace/holds').then((r) => Array.isArray(r.data) ? r.data : r.data.items ?? []);

const placeHold = (payload: object) =>
  apiClient.post('/api/v1/trace/holds', payload).then((r) => r.data);

const releaseHold = (holdId: string, payload: object) =>
  apiClient.post(`/api/v1/trace/holds/${holdId}/release`, payload).then((r) => r.data);

// ─── Form schema ──────────────────────────────────────────────────────────────

const placeSchema = z.object({
  lotNumber: z.string().min(1, 'Required'),
  holdType: z.enum(['FailedInspection', 'SupplierAlert', 'RecallInvestigation', 'Other']),
  holdReason: z.string().min(1, 'Required'),
  notes: z.string().optional(),
});
type PlaceFormValues = z.infer<typeof placeSchema>;

const releaseSchema = z.object({
  dispositionDecision: z.enum(['UseAsIs', 'Rework', 'Destroy', 'ReturnToSupplier', 'Downgrade']),
  releasedBy: z.string().min(1, 'Required'),
  notes: z.string().optional(),
});
type ReleaseFormValues = z.infer<typeof releaseSchema>;

// ─── Columns ──────────────────────────────────────────────────────────────────

const HOLD_TYPE_COLORS: Record<string, string> = {
  FailedInspection: '#DC2626',
  SupplierAlert: '#D97706',
  RecallInvestigation: '#7C3AED',
  Other: '#64748B',
};

const COLUMNS: GridColDef<LotHoldDto>[] = [
  { field: 'lotNumber', headerName: 'Lot Number', width: 150 },
  {
    field: 'holdType',
    headerName: 'Hold Type',
    width: 160,
    renderCell: ({ value }) => (
      <Chip
        label={value}
        size="small"
        sx={{
          bgcolor: alpha(HOLD_TYPE_COLORS[value] ?? '#64748B', 0.1),
          color: HOLD_TYPE_COLORS[value] ?? '#64748B',
          fontWeight: 600,
        }}
      />
    ),
  },
  { field: 'holdReason', headerName: 'Reason', flex: 1, minWidth: 200 },
  { field: 'placedBy', headerName: 'Placed By', width: 120 },
  {
    field: 'placedAt',
    headerName: 'Placed At',
    width: 160,
    valueFormatter: (v: string) => new Date(v).toLocaleString(),
  },
  {
    field: 'dispositionDecision',
    headerName: 'Disposition',
    width: 130,
    valueGetter: (v) => v ?? 'Pending',
    renderCell: ({ value }) => (
      <Chip
        label={value}
        size="small"
        sx={{
          bgcolor: value === 'Pending'
            ? (t) => alpha(t.palette.warning.main, 0.1)
            : (t) => alpha(t.palette.success.main, 0.1),
          color: value === 'Pending' ? 'warning.dark' : 'success.dark',
        }}
      />
    ),
  },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function HoldsPage() {
  const [placeDrawer, setPlaceDrawer] = useState(false);
  const [releaseDrawer, setReleaseDrawer] = useState(false);
  const [selection, setSelection] = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const qc = useQueryClient();

  const { data: holds = [], isLoading, error, refetch } = useQuery({
    queryKey: ['active-holds'],
    queryFn: fetchActiveHolds,
  });

  const placeForm = useForm<PlaceFormValues>({
    resolver: zodResolver(placeSchema),
    defaultValues: { holdType: 'FailedInspection' },
  });

  const releaseForm = useForm<ReleaseFormValues>({
    resolver: zodResolver(releaseSchema),
    defaultValues: { dispositionDecision: 'UseAsIs' },
  });

  const placeMutation = useMutation({
    mutationFn: placeHold,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['active-holds'] });
      setPlaceDrawer(false);
      placeForm.reset();
    },
  });

  const releaseMutation = useMutation({
    mutationFn: ({ holdId, data }: { holdId: string; data: object }) => releaseHold(holdId, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['active-holds'] });
      setReleaseDrawer(false);
      releaseForm.reset();
    },
  });

  const selectedHoldId = selection.type === 'include' ? [...selection.ids][0] as string | undefined : undefined;
  const selectedHold = holds.find((h) => h.holdId === selectedHoldId);

  return (
    <PageRoot>
      <PageHeader
        title="Hold Management"
        subtitle="Place, track and release lot holds"
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => void refetch()} loading={isLoading} />
            {selectedHold && (
              <Button
                variant="outlined"
                color="success"
                onClick={() => setReleaseDrawer(true)}
              >
                Release Hold
              </Button>
            )}
            <Button
              variant="contained"
              onClick={() => setPlaceDrawer(true)}
              color="warning"
            >
              Place Hold
            </Button>
          </Stack>
        }
      />

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to load holds.
        </Alert>
      )}

      {holds.length === 0 && !isLoading ? (
        <EmptyState
          icon="quality"
          title="No active holds"
          description="All lots are clear. Place a hold to quarantine a lot for investigation."
        />
      ) : (
        <DataGrid
          rows={holds}
          columns={COLUMNS}
          getRowId={(r) => r.holdId}
          loading={isLoading}
          rowSelectionModel={selection}
          onRowSelectionModelChange={setSelection}
          pageSizeOptions={[25, 50]}
          sx={{ flex: 1 }}
        />
      )}

      {/* ── Place Hold Drawer ─────────────────────────────────────────────── */}
      <FormDrawer
        open={placeDrawer}
        title="Place Lot Hold"
        onSubmit={placeForm.handleSubmit((data) => placeMutation.mutate(data))}
        submitLabel="Place Hold"
        loading={placeMutation.isPending}
        onClose={() => { setPlaceDrawer(false); placeForm.reset(); }}
      >
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Lot Number"
              {...placeForm.register('lotNumber')}
              error={!!placeForm.formState.errors.lotNumber}
              helperText={placeForm.formState.errors.lotNumber?.message}
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              select
              fullWidth
              label="Hold Type"
              defaultValue="FailedInspection"
              {...placeForm.register('holdType')}
            >
              {(['FailedInspection', 'SupplierAlert', 'RecallInvestigation', 'Other'] as const).map((t) => (
                <MenuItem key={t} value={t}>{t}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Hold Reason"
              {...placeForm.register('holdReason')}
              error={!!placeForm.formState.errors.holdReason}
              helperText={placeForm.formState.errors.holdReason?.message}
              multiline
              rows={2}
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField fullWidth label="Notes" {...placeForm.register('notes')} multiline rows={2} />
          </Grid>
          {placeMutation.isError && (
            <Grid size={{ xs: 12 }}>
              <Typography color="error" variant="body2">Failed to place hold.</Typography>
            </Grid>
          )}
        </Grid>
      </FormDrawer>

      {/* ── Release Hold Drawer ───────────────────────────────────────────── */}
      {selectedHold && (
        <FormDrawer
          open={releaseDrawer}
          title={`Release Hold — ${selectedHold.lotNumber}`}
          onSubmit={releaseForm.handleSubmit((data) =>
            releaseMutation.mutate({ holdId: selectedHold.holdId, data })
          )}
          submitLabel="Release"
          loading={releaseMutation.isPending}
          onClose={() => { setReleaseDrawer(false); releaseForm.reset(); }}
        >
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <TextField
                select
                fullWidth
                label="Disposition Decision"
                defaultValue="UseAsIs"
                {...releaseForm.register('dispositionDecision')}
              >
                {(['UseAsIs', 'Rework', 'Destroy', 'ReturnToSupplier', 'Downgrade'] as const).map((d) => (
                  <MenuItem key={d} value={d}>{d}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                label="Released By"
                {...releaseForm.register('releasedBy')}
                error={!!releaseForm.formState.errors.releasedBy}
                helperText={releaseForm.formState.errors.releasedBy?.message}
                required
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField fullWidth label="Notes" {...releaseForm.register('notes')} multiline rows={2} />
            </Grid>
          </Grid>
        </FormDrawer>
      )}
    </PageRoot>
  );
}
