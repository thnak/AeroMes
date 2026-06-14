import {
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
import type { GridColDef } from '@mui/x-data-grid';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { EmptyState, FormDrawer, PageHeader, PageRoot, RefreshButton } from '../../components';
import { apiClient } from '../../lib/apiClient';

// ─── Types ────────────────────────────────────────────────────────────────────

interface ScrapTransaction {
  scrapTxID: number;
  woid: number;
  productCode: string;
  lotNumber?: string;
  scrapQty: number;
  totalCost: number;
  disposalMethod: string;
  approvedBy?: string;
  postedAt: string;
}

type DisposalMethod = 'Scrap' | 'Rework' | 'Salvage' | 'ReturnToSupplier';

// ─── API ──────────────────────────────────────────────────────────────────────

const fetchScrapList = (): Promise<ScrapTransaction[]> =>
  apiClient.get('/api/v1/cost/scrap?pageSize=50').then((r) => Array.isArray(r.data) ? r.data : r.data.items ?? []);

const postScrap = (payload: object) =>
  apiClient.post('/api/v1/cost/scrap', payload).then((r) => r.data);

// ─── Form ─────────────────────────────────────────────────────────────────────

const schema = z.object({
  woid: z.number().min(1, 'Required'),
  productCode: z.string().min(1, 'Required'),
  lotNumber: z.string().optional(),
  scrapQty: z.number().min(1, 'Required'),
  materialCostPerUnit: z.number().min(0),
  laborCostSunk: z.number().min(0),
  machineCostSunk: z.number().min(0),
  disposalMethod: z.enum(['Scrap', 'Rework', 'Salvage', 'ReturnToSupplier']),
  approvedBy: z.string().optional(),
  notes: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

// ─── Columns ──────────────────────────────────────────────────────────────────

const COLUMNS: GridColDef<ScrapTransaction>[] = [
  { field: 'scrapTxID', headerName: 'ID', width: 80 },
  { field: 'productCode', headerName: 'Product', width: 120 },
  { field: 'lotNumber', headerName: 'Lot', width: 120, valueGetter: (v) => v ?? '—' },
  { field: 'scrapQty', headerName: 'Qty', width: 80, type: 'number' },
  {
    field: 'totalCost',
    headerName: 'Total Cost',
    width: 120,
    type: 'number',
    valueFormatter: (v: number) => `₫ ${(v / 1000).toFixed(0)}K`,
  },
  {
    field: 'disposalMethod',
    headerName: 'Disposal',
    width: 130,
    renderCell: ({ value }) => (
      <Chip
        label={value}
        size="small"
        sx={{
          bgcolor: (t) => alpha(t.palette.warning.main, 0.1),
          color: 'warning.dark',
          fontWeight: 600,
        }}
      />
    ),
  },
  { field: 'approvedBy', headerName: 'Approved By', width: 130, valueGetter: (v) => v ?? '—' },
  {
    field: 'postedAt',
    headerName: 'Posted At',
    width: 160,
    valueFormatter: (v: string) => (v ? new Date(v).toLocaleString() : '—'),
  },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ScrapPage() {
  const [drawerOpen, setDrawerOpen] = useState(false);
  const qc = useQueryClient();

  const { data: rows = [], isLoading, refetch } = useQuery({
    queryKey: ['scrap-transactions'],
    queryFn: fetchScrapList,
  });

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      woid: undefined as unknown as number,
      materialCostPerUnit: 0,
      laborCostSunk: 0,
      machineCostSunk: 0,
      disposalMethod: 'Scrap',
    },
  });

  const mutation = useMutation({
    mutationFn: postScrap,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['scrap-transactions'] });
      setDrawerOpen(false);
      form.reset();
    },
  });

  return (
    <PageRoot>
      <PageHeader
        title="Scrap Transactions"
        subtitle="Post and track scrap events against work orders"
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => void refetch()} loading={isLoading} />
            <Button
              variant="contained"
              onClick={() => setDrawerOpen(true)}
            >
              Post Scrap
            </Button>
          </Stack>
        }
      />

      {rows.length === 0 && !isLoading ? (
        <EmptyState
          icon="quality"
          title="No scrap transactions"
          description="Post a scrap event to start tracking quality costs."
        />
      ) : (
        <DataGrid
          rows={rows}
          columns={COLUMNS}
          getRowId={(r) => r.scrapTxID}
          loading={isLoading}
          pageSizeOptions={[25, 50]}
          disableRowSelectionOnClick
          sx={{ flex: 1 }}
        />
      )}

      {/* ── Post Scrap Drawer ─────────────────────────────────────────────── */}
      <FormDrawer
        open={drawerOpen}
        title="Post Scrap"
        onSubmit={form.handleSubmit((data) => mutation.mutate(data))}
        submitLabel="Post"
        loading={mutation.isPending}
        onClose={() => { setDrawerOpen(false); form.reset(); }}
      >
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Work Order ID"
              type="number"
              {...form.register('woid', { valueAsNumber: true })}
              error={!!form.formState.errors.woid}
              helperText={form.formState.errors.woid?.message}
              required
            />
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
            <TextField fullWidth label="Lot Number" {...form.register('lotNumber')} />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Scrap Qty"
              type="number"
              {...form.register('scrapQty', { valueAsNumber: true })}
              error={!!form.formState.errors.scrapQty}
              helperText={form.formState.errors.scrapQty?.message}
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Material Cost / Unit (₫)"
              type="number"
              {...form.register('materialCostPerUnit', { valueAsNumber: true })}
              error={!!form.formState.errors.materialCostPerUnit}
              helperText={form.formState.errors.materialCostPerUnit?.message}
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              fullWidth
              label="Labor Cost Sunk (₫)"
              type="number"
              {...form.register('laborCostSunk', { valueAsNumber: true })}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              fullWidth
              label="Machine Cost Sunk (₫)"
              type="number"
              {...form.register('machineCostSunk', { valueAsNumber: true })}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              select
              fullWidth
              label="Disposal Method"
              defaultValue="Scrap"
              {...form.register('disposalMethod')}
            >
              {(['Scrap', 'Rework', 'Salvage', 'ReturnToSupplier'] as DisposalMethod[]).map((m) => (
                <MenuItem key={m} value={m}>{m}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField fullWidth label="Approved By" {...form.register('approvedBy')} />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField fullWidth label="Notes" {...form.register('notes')} multiline rows={2} />
          </Grid>
          {mutation.isError && (
            <Grid size={{ xs: 12 }}>
              <Typography color="error" variant="body2">
                Failed to post scrap. Please try again.
              </Typography>
            </Grid>
          )}
        </Grid>
      </FormDrawer>
    </PageRoot>
  );
}
