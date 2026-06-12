import {
  Box,
  Button,
  Grid,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams, GridRowSelectionModel } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  ConfirmDialog,
  EmptyState,
  ExportButton,
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TableToolbar,
} from '../../components';

// ─── Types & mock data ────────────────────────────────────────────────────────

interface InventoryItem {
  id: string;
  productCode: string;
  productName: string;
  lotNo: string;
  locationCode: string;
  locationName: string;
  quantity: number;
  reservedQty: number;
  uom: string;
  expiresAt?: string;
  receivedAt: string;
}

const MOCK_INVENTORY: InventoryItem[] = [
  { id: '1',  productCode: 'FRM-A001', productName: 'Frame Assembly A',     lotNo: 'LOT-2026-0441', locationCode: 'WH-02', locationName: 'WIP',               quantity: 120, reservedQty: 100, uom: 'EA',  receivedAt: '2026-06-01' },
  { id: '2',  productCode: 'PNL-B002', productName: 'Panel Sub-assembly B', lotNo: 'LOT-2026-0442', locationCode: 'WH-01', locationName: 'Raw Materials',      quantity: 500, reservedQty: 200, uom: 'EA',  receivedAt: '2026-05-28' },
  { id: '3',  productCode: 'SHT-C003', productName: 'Shaft Housing C',      lotNo: 'LOT-2026-0438', locationCode: 'WH-02', locationName: 'WIP',               quantity: 8,   reservedQty: 0,   uom: 'EA',  receivedAt: '2026-06-03' },
  { id: '4',  productCode: 'BRK-D004', productName: 'Bracket Set D',        lotNo: 'LOT-2026-0430', locationCode: 'WH-03', locationName: 'Finished Goods',    quantity: 1200,reservedQty: 800, uom: 'SET', receivedAt: '2026-06-05' },
  { id: '5',  productCode: 'MTR-E005', productName: 'Motor Mount E',        lotNo: 'LOT-2026-0425', locationCode: 'WH-FG', locationName: 'Final Inspection',  quantity: 298, reservedQty: 298, uom: 'EA',  receivedAt: '2026-05-30' },
  { id: '6',  productCode: 'COV-F006', productName: 'Cover Plate F',        lotNo: 'LOT-2026-0450', locationCode: 'WH-01', locationName: 'Raw Materials',      quantity: 600, reservedQty: 0,   uom: 'EA',  receivedAt: '2026-06-10' },
  { id: '7',  productCode: 'SPR-H008', productName: 'Sprocket Set H',       lotNo: 'LOT-2026-0436', locationCode: 'WH-02', locationName: 'WIP',               quantity: 5,   reservedQty: 5,   uom: 'SET', receivedAt: '2026-06-02' },
  { id: '8',  productCode: 'HNG-J010', productName: 'Hinge Assembly J',     lotNo: 'LOT-2026-0448', locationCode: 'WH-01', locationName: 'Raw Materials',      quantity: 750, reservedQty: 0,   uom: 'EA',  receivedAt: '2026-06-11' },
  { id: '9',  productCode: 'FRM-A001', productName: 'Frame Assembly A',     lotNo: 'LOT-2026-0444', locationCode: 'WH-03', locationName: 'Finished Goods',    quantity: 387, reservedQty: 200, uom: 'EA',  receivedAt: '2026-06-08' },
  { id: '10', productCode: 'BRK-D004', productName: 'Bracket Set D',        lotNo: 'LOT-2026-0431', locationCode: 'WH-FG', locationName: 'Final Inspection',  quantity: 2,   reservedQty: 0,   uom: 'SET', receivedAt: '2026-06-06' },
  { id: '11', productCode: 'MTR-E005', productName: 'Motor Mount E',        lotNo: 'LOT-2026-0422', locationCode: 'WH-01', locationName: 'Raw Materials',      quantity: 100, reservedQty: 0,   uom: 'EA',  receivedAt: '2026-05-20' },
  { id: '12', productCode: 'SHT-C003', productName: 'Shaft Housing C',      lotNo: 'LOT-2026-0440', locationCode: 'WH-02', locationName: 'WIP',               quantity: 352, reservedQty: 300, uom: 'EA',  receivedAt: '2026-05-29' },
];

const LOCATIONS = [
  { code: 'WH-01', name: 'Raw Materials' },
  { code: 'WH-02', name: 'WIP' },
  { code: 'WH-03', name: 'Finished Goods' },
  { code: 'WH-FG', name: 'Final Inspection' },
];

const UNIQUE_PRODUCTS = [...new Map(MOCK_INVENTORY.map((i) => [i.productCode, i])).values()];

// ─── Form ─────────────────────────────────────────────────────────────────────

const AdjustSchema = z.object({
  productCode:  z.string().min(1, 'Product is required'),
  lotNo:        z.string().min(1, 'Lot number is required'),
  locationCode: z.string().min(1, 'Location is required'),
  quantity:     z.coerce.number().int('Must be a whole number'),
  uom:          z.string().min(1, 'Unit is required'),
  reason:       z.string().min(1, 'Reason is required'),
});
type AdjustFormValues = z.infer<typeof AdjustSchema>;

function AdjustForm({ defaultValues, onSubmit }: { defaultValues: Partial<AdjustFormValues>; onSubmit: (d: AdjustFormValues) => void }) {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const { register, control, handleSubmit, formState: { errors } } = useForm<AdjustFormValues>({
    resolver: zodResolver(AdjustSchema) as any,
    defaultValues,
  });

  return (
    <Box component="form" id="adjust-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="productCode"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Product"
                fullWidth
                required
                error={!!errors.productCode}
                helperText={errors.productCode?.message}
              >
                {UNIQUE_PRODUCTS.map((p) => (
                  <MenuItem key={p.productCode} value={p.productCode}>{p.productCode} — {p.productName}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('lotNo')}
            label="Lot Number"
            fullWidth
            required
            error={!!errors.lotNo}
            helperText={errors.lotNo?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="locationCode"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Location"
                fullWidth
                required
                error={!!errors.locationCode}
                helperText={errors.locationCode?.message}
              >
                {LOCATIONS.map((l) => (
                  <MenuItem key={l.code} value={l.code}>{l.code} — {l.name}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('quantity')}
            label="Adjust Qty (+ / -)"
            type="number"
            fullWidth
            required
            error={!!errors.quantity}
            helperText={errors.quantity?.message ?? 'Positive to add, negative to reduce'}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="uom"
            control={control}
            render={({ field }) => (
              <TextField {...field} select label="UOM" fullWidth required error={!!errors.uom} helperText={errors.uom?.message}>
                {['EA', 'SET', 'KG', 'M', 'BOX'].map((u) => <MenuItem key={u} value={u}>{u}</MenuItem>)}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('reason')}
            label="Adjustment Reason"
            fullWidth
            required
            multiline
            rows={2}
            error={!!errors.reason}
            helperText={errors.reason?.message}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function InventoryPage() {
  const navigate = useNavigate();
  const [rows, setRows]                   = useState<InventoryItem[]>(MOCK_INVENTORY);
  const [search, setSearch]               = useState('');
  const [productFilter, setProductFilter] = useState('');
  const [locationFilter, setLocationFilter] = useState('');
  const [selection, setSelection]         = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]       = useState(false);
  const [deleteTarget, setDeleteTarget]   = useState<InventoryItem | null>(null);
  const [saving, setSaving]               = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)         r = r.filter((i) => i.productCode.toLowerCase().includes(search.toLowerCase()) || i.productName.toLowerCase().includes(search.toLowerCase()) || i.lotNo.toLowerCase().includes(search.toLowerCase()));
    if (productFilter)  r = r.filter((i) => i.productCode === productFilter);
    if (locationFilter) r = r.filter((i) => i.locationCode === locationFilter);
    return r;
  }, [rows, search, productFilter, locationFilter]);

  function handleAdjust(data: AdjustFormValues) {
    setSaving(true);
    const location = LOCATIONS.find((l) => l.code === data.locationCode);
    setTimeout(() => {
      const existing = rows.find((r) => r.lotNo === data.lotNo && r.locationCode === data.locationCode);
      if (existing) {
        setRows((prev) => prev.map((r) => r.id === existing.id ? { ...r, quantity: Math.max(0, r.quantity + data.quantity) } : r));
      } else {
        const product = UNIQUE_PRODUCTS.find((p) => p.productCode === data.productCode);
        setRows((prev) => [...prev, {
          id: String(Date.now()),
          productCode: data.productCode,
          productName: product?.productName ?? '',
          lotNo: data.lotNo,
          locationCode: data.locationCode,
          locationName: location?.name ?? data.locationCode,
          quantity: Math.max(0, data.quantity),
          reservedQty: 0,
          uom: data.uom,
          receivedAt: new Date().toISOString().slice(0, 10),
        }]);
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((i) => i.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<InventoryItem>[] = [
    {
      field: 'productName',
      headerName: 'Product',
      flex: 1,
      minWidth: 170,
      renderCell: (params: GridRenderCellParams<InventoryItem>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600 }}>{params.value as string}</Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11 }}>{params.row.productCode}</Typography>
        </Stack>
      ),
    },
    {
      field: 'lotNo',
      headerName: 'Lot #',
      width: 150,
      renderCell: (params: GridRenderCellParams<InventoryItem>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value as string}
        </Typography>
      ),
    },
    {
      field: 'locationName',
      headerName: 'Location',
      width: 150,
      renderCell: (params: GridRenderCellParams<InventoryItem>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600 }}>{params.value as string}</Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11 }}>{params.row.locationCode}</Typography>
        </Stack>
      ),
    },
    {
      field: 'quantity',
      headerName: 'Available',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams<InventoryItem>) => {
        const avail = params.row.quantity - params.row.reservedQty;
        const low = avail < 10;
        return (
          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', justifyContent: 'flex-end' }}>
            {low && (
              <Tooltip title="Low stock">
                <Box component="span" sx={{ color: 'warning.main', display: 'flex', alignItems: 'center' }}>
                  <SolarIcon name="warning" size={14} />
                </Box>
              </Tooltip>
            )}
            <Tooltip title={`${params.row.reservedQty} reserved`}>
              <Typography variant="body2" sx={{ fontWeight: 600, color: low ? 'warning.main' : 'text.primary' }}>
                {avail.toLocaleString()}
              </Typography>
            </Tooltip>
          </Stack>
        );
      },
    },
    {
      field: 'uom',
      headerName: 'UOM',
      width: 65,
      align: 'center',
      headerAlign: 'center',
    },
    {
      field: 'receivedAt',
      headerName: 'Received',
      width: 100,
      renderCell: (params: GridRenderCellParams<InventoryItem>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>{params.value as string}</Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 80,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<InventoryItem>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View">
            <IconButton size="small" onClick={() => navigate(`/production/inventory/trace?lot=${params.row.lotNo}`)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="view" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Adjust">
            <IconButton size="small" onClick={() => setDrawerOpen(true)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Inventory"
        subtitle="Stock levels by product, lot and location"
        breadcrumbs={[{ label: 'Production' }, { label: 'Inventory' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              size="small"
              startIcon={<SolarIcon name="serial" size={16} />}
              onClick={() => navigate('/production/inventory/trace')}
            >
              Trace Lot
            </Button>
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="add" size={16} />}
              onClick={() => setDrawerOpen(true)}
            >
              Adjust Stock
            </Button>
          </Stack>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search product or lot number…"
        filters={[
          {
            label: 'Product',
            value: productFilter,
            options: UNIQUE_PRODUCTS.map((p) => ({ label: p.productName, value: p.productCode })),
            onChange: setProductFilter,
          },
          {
            label: 'Location',
            value: locationFilter,
            options: LOCATIONS.map((l) => ({ label: l.name, value: l.code })),
            onChange: setLocationFilter,
          },
        ]}
        totalCount={filtered.length}
        actions={
          <Stack direction="row" spacing={0.5}>
            <ExportButton />
            <RefreshButton />
          </Stack>
        }
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          density="compact"
          checkboxSelection
          disableRowSelectionOnClick
          rowSelectionModel={selection}
          onRowSelectionModelChange={setSelection}
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || productFilter || locationFilter ? 'emptySearch' : 'emptyTable'}
                title="No inventory records"
                description="Adjust stock to create your first inventory record."
                action={
                  !search && !productFilter && !locationFilter ? (
                    <Button variant="contained" size="small" onClick={() => setDrawerOpen(true)}>Adjust Stock</Button>
                  ) : undefined
                }
                compact
              />
            ),
          }}
          sx={{
            border: '1px solid', borderColor: 'divider', borderRadius: 2, bgcolor: 'background.paper',
            '& .MuiDataGrid-columnHeaders': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04), borderBottom: '1px solid', borderColor: 'divider' },
            '& .MuiDataGrid-row:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.03) },
            '& .MuiDataGrid-cell:focus, & .MuiDataGrid-cell:focus-within': { outline: 'none' },
            '& .MuiDataGrid-columnHeader:focus, & .MuiDataGrid-columnHeader:focus-within': { outline: 'none' },
          }}
        />
      </Box>

      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title="Adjust Stock"
        subtitle="Add or subtract inventory for a lot at a location"
        onSubmit={() => document.getElementById('adjust-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Apply Adjustment"
        loading={saving}
      >
        <AdjustForm defaultValues={{}} onSubmit={handleAdjust} />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Remove Inventory Record"
        description={
          <>Remove lot <strong>{deleteTarget?.lotNo}</strong> from <strong>{deleteTarget?.locationName}</strong>? This cannot be undone.</>
        }
        confirmLabel="Remove"
        confirmColor="error"
      />
    </PageRoot>
  );
}
