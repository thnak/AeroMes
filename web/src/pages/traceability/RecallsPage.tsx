import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
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
import { FormDrawer, KpiCard, PageHeader, PageRoot, RefreshButton } from '../../components';
import { apiClient } from '../../lib/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface RecallDto {
  recallId: number;
  recallCode: string;
  recallType: string;
  severity: string;
  productCode: string;
  lotNumbers: string[];
  affectedQty: number;
  initiatedBy: string;
  initiatedAt: string;
  status: string;
  description?: string;
  closedAt?: string;
}

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchRecalls = (): Promise<{ items: RecallDto[]; total: number }> =>
  apiClient.get('/api/v1/recalls?pageSize=50').then((r) =>
    Array.isArray(r.data) ? { items: r.data, total: r.data.length } : r.data
  );

const createRecall = (payload: object) =>
  apiClient.post('/api/v1/recalls', payload).then((r) => r.data);

const closeRecall = (recallId: number) =>
  apiClient.post(`/api/v1/recalls/${recallId}/close`).then((r) => r.data);

// ─── Form schema ──────────────────────────────────────────────────────────────

const schema = z.object({
  recallCode: z.string().min(1, 'Required'),
  recallType: z.enum(['SupplierDefect', 'ProductionError', 'CustomerComplaint', 'Regulatory', 'Other']),
  severity: z.enum(['Critical', 'Major', 'Minor']),
  productCode: z.string().min(1, 'Required'),
  lotNumbers: z.string().min(1, 'At least one lot number'),
  affectedQty: z.number().min(1, 'Required'),
  description: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

// ─── Columns ──────────────────────────────────────────────────────────────────

const SEVERITY_COLOR: Record<string, string> = {
  Critical: '#DC2626',
  Major: '#D97706',
  Minor: '#64748B',
};

const COLUMNS: GridColDef<RecallDto>[] = [
  { field: 'recallCode', headerName: 'Code', width: 130 },
  { field: 'productCode', headerName: 'Product', width: 120 },
  {
    field: 'severity',
    headerName: 'Severity',
    width: 110,
    renderCell: ({ value }) => (
      <Chip
        label={value}
        size="small"
        sx={{
          bgcolor: alpha(SEVERITY_COLOR[value] ?? '#64748B', 0.1),
          color: SEVERITY_COLOR[value] ?? '#64748B',
          fontWeight: 600,
        }}
      />
    ),
  },
  { field: 'recallType', headerName: 'Type', width: 140 },
  { field: 'affectedQty', headerName: 'Affected Qty', width: 120, type: 'number' },
  {
    field: 'lotNumbers',
    headerName: 'Lots',
    width: 80,
    valueGetter: (v: string[]) => v?.length ?? 0,
    valueFormatter: (v: number) => `${v} lot(s)`,
  },
  { field: 'initiatedBy', headerName: 'Initiated By', width: 130 },
  {
    field: 'initiatedAt',
    headerName: 'Initiated At',
    width: 160,
    valueFormatter: (v: string) => new Date(v).toLocaleDateString(),
  },
  {
    field: 'status',
    headerName: 'Status',
    width: 110,
    renderCell: ({ value }) => (
      <Chip
        label={value}
        size="small"
        sx={{
          bgcolor: (t) =>
            alpha(value === 'Closed' ? t.palette.success.main : t.palette.warning.main, 0.1),
          color: value === 'Closed' ? 'success.main' : 'warning.main',
          fontWeight: 600,
        }}
      />
    ),
  },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function RecallsPage() {
  const [createDrawer, setCreateDrawer] = useState(false);
  const [selection, setSelection] = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const qc = useQueryClient();

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['recalls'],
    queryFn: fetchRecalls,
  });
  const recalls = data?.items ?? [];

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { recallType: 'ProductionError', severity: 'Major' },
  });

  const createMutation = useMutation({
    mutationFn: createRecall,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['recalls'] });
      setCreateDrawer(false);
      form.reset();
    },
  });

  const closeMutation = useMutation({
    mutationFn: (id: number) => closeRecall(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['recalls'] }),
  });

  // ── KPI derivations ─────────────────────────────────────────────────────────
  const openRecalls = recalls.filter((r) => r.status !== 'Closed').length;
  const affectedLots = new Set(recalls.flatMap((r) => r.lotNumbers)).size;
  const totalAffectedQty = recalls.reduce((s, r) => s + r.affectedQty, 0);
  const criticalCount = recalls.filter((r) => r.severity === 'Critical' && r.status !== 'Closed').length;

  const selectedRecallId = selection.type === 'include' ? [...selection.ids][0] as number | undefined : undefined;
  const selectedRecall = recalls.find((r) => r.recallId === selectedRecallId);

  return (
    <PageRoot>
      <PageHeader
        title="Recall Dashboard"
        subtitle="Manage product recalls and affected lot quarantine"
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => void refetch()} loading={isLoading} />
            {selectedRecall && selectedRecall.status !== 'Closed' && (
              <Button
                variant="outlined"
                color="success"
                onClick={() => closeMutation.mutate(selectedRecall.recallId)}
                disabled={closeMutation.isPending}
              >
                Close Recall
              </Button>
            )}
            <Button
              variant="contained"
              onClick={() => setCreateDrawer(true)}
              color="error"
            >
              Initiate Recall
            </Button>
          </Stack>
        }
      />

      {/* ── KPI Row ─────────────────────────────────────────────────────────── */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard icon="warning" label="Open Recalls" value={String(openRecalls)} accentColor="#D97706" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard icon="quality" label="Critical (Open)" value={String(criticalCount)} accentColor="#DC2626" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard icon="bom" label="Affected Lots" value={String(affectedLots)} accentColor="#7C3AED" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <KpiCard
            icon="production"
            label="Total Affected Qty"
            value={totalAffectedQty.toLocaleString()}
            accentColor="#1D4ED8"
          />
        </Grid>
      </Grid>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to load recalls.
        </Alert>
      )}

      {/* ── Recall List ─────────────────────────────────────────────────────── */}
      <DataGrid
        rows={recalls}
        columns={COLUMNS}
        getRowId={(r) => r.recallId}
        loading={isLoading}
        rowSelectionModel={selection}
        onRowSelectionModelChange={setSelection}
        pageSizeOptions={[25, 50]}
        sx={{ flex: 1 }}
      />

      {/* ── Lot Details Panel ────────────────────────────────────────────────── */}
      {selectedRecall && (
        <Card variant="outlined" sx={{ borderRadius: 3, mt: 2 }}>
          <CardContent>
            <Typography variant="subtitle2" sx={{ fontWeight: 600, mb: 1 }}>
              Affected Lots — {selectedRecall.recallCode}
            </Typography>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
              {selectedRecall.lotNumbers.map((lot) => (
                <Chip key={lot} label={lot} size="small" variant="outlined" />
              ))}
              {selectedRecall.lotNumbers.length === 0 && (
                <Typography variant="caption" color="text.disabled">
                  No lots recorded
                </Typography>
              )}
            </Box>
            {selectedRecall.description && (
              <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                {selectedRecall.description}
              </Typography>
            )}
          </CardContent>
        </Card>
      )}

      {/* ── Initiate Recall Drawer ────────────────────────────────────────── */}
      <FormDrawer
        open={createDrawer}
        title="Initiate Recall"
        onSubmit={form.handleSubmit((data) =>
          createMutation.mutate({
            ...data,
            lotNumbers: data.lotNumbers.split(',').map((s) => s.trim()).filter(Boolean),
          })
        )}
        submitLabel="Initiate"
        loading={createMutation.isPending}
        onClose={() => { setCreateDrawer(false); form.reset(); }}
      >
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Recall Code"
              {...form.register('recallCode')}
              error={!!form.formState.errors.recallCode}
              helperText={form.formState.errors.recallCode?.message}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              fullWidth
              label="Recall Type"
              defaultValue="ProductionError"
              {...form.register('recallType')}
            >
              {(['SupplierDefect', 'ProductionError', 'CustomerComplaint', 'Regulatory', 'Other'] as const).map(
                (t) => <MenuItem key={t} value={t}>{t}</MenuItem>
              )}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              fullWidth
              label="Severity"
              defaultValue="Major"
              {...form.register('severity')}
            >
              {(['Critical', 'Major', 'Minor'] as const).map((s) => (
                <MenuItem key={s} value={s}>{s}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Product Code"
              {...form.register('productCode')}
              error={!!form.formState.errors.productCode}
              helperText={form.formState.errors.productCode?.message}
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Lot Numbers (comma-separated)"
              {...form.register('lotNumbers')}
              error={!!form.formState.errors.lotNumbers}
              helperText={form.formState.errors.lotNumbers?.message ?? 'e.g. LOT-001, LOT-002'}
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Affected Qty"
              type="number"
              {...form.register('affectedQty', { valueAsNumber: true })}
              error={!!form.formState.errors.affectedQty}
              helperText={form.formState.errors.affectedQty?.message}
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Description"
              {...form.register('description')}
              multiline
              rows={2}
            />
          </Grid>
          {createMutation.isError && (
            <Grid size={{ xs: 12 }}>
              <Typography color="error" variant="body2">Failed to initiate recall.</Typography>
            </Grid>
          )}
        </Grid>
      </FormDrawer>
    </PageRoot>
  );
}
