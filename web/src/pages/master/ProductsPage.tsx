import {
  Box,
  Button,
  Chip,
  FormControlLabel,
  Grid,
  IconButton,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams, GridRowSelectionModel } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
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

// ─── Mock data ────────────────────────────────────────────────────────────────

interface Product {
  id: string;
  code: string;
  name: string;
  category: string;
  unitOfMeasure: string;
  hasBom: boolean;
  hasRouting: boolean;
  isActive: boolean;
  description?: string;
}

const MOCK_PRODUCTS: Product[] = [
  { id: '1',  code: 'FRM-A001', name: 'Frame Assembly A',        category: 'Assembly',   unitOfMeasure: 'EA', hasBom: true,  hasRouting: true,  isActive: true  },
  { id: '2',  code: 'PNL-B002', name: 'Panel Sub-assembly B',    category: 'Sub-assy',  unitOfMeasure: 'EA', hasBom: true,  hasRouting: true,  isActive: true  },
  { id: '3',  code: 'SHT-C003', name: 'Shaft Housing C',         category: 'Machined',  unitOfMeasure: 'EA', hasBom: true,  hasRouting: true,  isActive: true  },
  { id: '4',  code: 'BRK-D004', name: 'Bracket Set D',           category: 'Stamped',   unitOfMeasure: 'SET',hasBom: false, hasRouting: false, isActive: true  },
  { id: '5',  code: 'MTR-E005', name: 'Motor Mount E',            category: 'Machined',  unitOfMeasure: 'EA', hasBom: true,  hasRouting: false, isActive: true  },
  { id: '6',  code: 'COV-F006', name: 'Cover Plate F',            category: 'Stamped',   unitOfMeasure: 'EA', hasBom: false, hasRouting: true,  isActive: true  },
  { id: '7',  code: 'GRD-G007', name: 'Guard Assembly G',         category: 'Assembly',  unitOfMeasure: 'EA', hasBom: true,  hasRouting: true,  isActive: false },
  { id: '8',  code: 'SPR-H008', name: 'Sprocket Set H',           category: 'Purchased', unitOfMeasure: 'SET',hasBom: false, hasRouting: false, isActive: true  },
  { id: '9',  code: 'BSH-I009', name: 'Bushing I',                category: 'Purchased', unitOfMeasure: 'EA', hasBom: false, hasRouting: false, isActive: true  },
  { id: '10', code: 'HNG-J010', name: 'Hinge Assembly J',         category: 'Assembly',  unitOfMeasure: 'EA', hasBom: true,  hasRouting: true,  isActive: true  },
  { id: '11', code: 'CAP-K011', name: 'Capacitor Bank K',         category: 'Electrical',unitOfMeasure: 'EA', hasBom: true,  hasRouting: false, isActive: true  },
  { id: '12', code: 'WHL-L012', name: 'Wheel & Hub Assembly L',   category: 'Assembly',  unitOfMeasure: 'EA', hasBom: true,  hasRouting: true,  isActive: false },
];

const CATEGORIES = [...new Set(MOCK_PRODUCTS.map((p) => p.category))].sort();

// ─── Form schema ──────────────────────────────────────────────────────────────

const ProductSchema = z.object({
  code:           z.string().min(1, 'Code is required').max(50),
  name:           z.string().min(1, 'Name is required').max(200),
  category:       z.string().min(1, 'Category is required'),
  unitOfMeasure:  z.string().min(1, 'Unit is required'),
  description:    z.string().optional(),
  isActive:       z.boolean(),
});

type ProductFormValues = z.infer<typeof ProductSchema>;

// ─── Product form ─────────────────────────────────────────────────────────────

function ProductForm({
  defaultValues,
  onSubmit,
  loading: _loading,
}: {
  defaultValues: Partial<ProductFormValues>;
  onSubmit: (data: ProductFormValues) => void;
  loading: boolean;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<ProductFormValues>({
    resolver: zodResolver(ProductSchema),
    defaultValues: { isActive: true, ...defaultValues },
  });

  return (
    <Box component="form" id="product-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Product Code"
            fullWidth
            required
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="category"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Category"
                fullWidth
                required
                error={!!errors.category}
                helperText={errors.category?.message}
              >
                {CATEGORIES.map((c) => (
                  <MenuItem key={c} value={c}>{c}</MenuItem>
                ))}
                <MenuItem value="Other">Other</MenuItem>
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Product Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="unitOfMeasure"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Unit of Measure"
                fullWidth
                required
                error={!!errors.unitOfMeasure}
                helperText={errors.unitOfMeasure?.message}
              >
                {['EA', 'SET', 'KG', 'M', 'M2', 'L', 'BOX'].map((u) => (
                  <MenuItem key={u} value={u}>{u}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="isActive"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={
                  <Switch
                    checked={field.value}
                    onChange={field.onChange}
                    color="primary"
                  />
                }
                label="Active"
                sx={{ mt: 0.5, ml: 0 }}
              />
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('description')}
            label="Description"
            fullWidth
            multiline
            rows={3}
            placeholder="Optional product description…"
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function ProductsPage() {
  const [rows, setRows]                   = useState<Product[]>(MOCK_PRODUCTS);
  const [search, setSearch]               = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter]   = useState('');
  const [selection, setSelection]         = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]       = useState(false);
  const [drawerMode, setDrawerMode]       = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]       = useState<Product | null>(null);
  const [deleteTarget, setDeleteTarget]   = useState<Product | null>(null);
  const [bulkDeleteOpen, setBulkDeleteOpen] = useState(false);
  const [saving, setSaving]               = useState(false);

  // Filtered rows
  const filtered = useMemo(() => {
    let r = rows;
    if (search)         r = r.filter((p) => p.name.toLowerCase().includes(search.toLowerCase()) || p.code.toLowerCase().includes(search.toLowerCase()));
    if (categoryFilter) r = r.filter((p) => p.category === categoryFilter);
    if (statusFilter)   r = r.filter((p) => statusFilter === 'active' ? p.isActive : !p.isActive);
    return r;
  }, [rows, search, categoryFilter, statusFilter]);

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  // CRUD handlers
  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(p: Product) { setDrawerMode('edit'); setEditTarget(p); setDrawerOpen(true); }

  function handleSave(data: ProductFormValues) {
    setSaving(true);
    setTimeout(() => {
      if (drawerMode === 'create') {
        setRows((prev) => [...prev, { id: String(Date.now()), hasBom: false, hasRouting: false, ...data }]);
      } else if (editTarget) {
        setRows((prev) => prev.map((p) => p.id === editTarget.id ? { ...p, ...data } : p));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((p) => p.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  function handleBulkDelete() {
    setRows((prev) => prev.filter((p) => !selectedIds.has(p.id)));
    setSelection({ type: 'include', ids: new Set() });
    setBulkDeleteOpen(false);
  }

  // Columns
  const columns: GridColDef<Product>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 130,
      renderCell: (params: GridRenderCellParams<Product>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 180 },
    {
      field: 'category',
      headerName: 'Category',
      width: 120,
      renderCell: (params: GridRenderCellParams<Product>) => (
        <Chip
          label={params.value}
          size="small"
          sx={(theme) => ({
            height: 20,
            fontSize: '0.6875rem',
            fontWeight: 600,
            bgcolor: alpha(theme.palette.primary.main, 0.08),
            color: 'primary.main',
            border: 'none',
            '& .MuiChip-label': { px: 0.75 },
          })}
        />
      ),
    },
    { field: 'unitOfMeasure', headerName: 'Unit', width: 70, align: 'center', headerAlign: 'center' },
    {
      field: 'hasBom',
      headerName: 'BOM',
      width: 70,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Product>) => (
        <SolarIcon
          name={params.value ? 'success' : 'close'}
          size={16}
          color={params.value ? '#15803D' : '#94A3B8'}
        />
      ),
    },
    {
      field: 'hasRouting',
      headerName: 'Routing',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Product>) => (
        <SolarIcon
          name={params.value ? 'success' : 'close'}
          size={16}
          color={params.value ? '#15803D' : '#94A3B8'}
        />
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Product>) => (
        <Chip
          label={params.value ? 'Active' : 'Inactive'}
          size="small"
          sx={{
            height: 20,
            fontSize: '0.6875rem',
            fontWeight: 600,
            bgcolor: params.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
            color: params.value ? '#15803D' : '#94A3B8',
            border: 'none',
            '& .MuiChip-label': { px: 0.75 },
          }}
        />
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 90,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<Product>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => openEdit(params.row)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton size="small" onClick={() => setDeleteTarget(params.row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
              <SolarIcon name="delete" size={16} />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Products"
        subtitle="Manage product master data — codes, categories, BOM and routing assignments"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Products' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button
                variant="outlined"
                color="error"
                size="small"
                startIcon={<SolarIcon name="delete" size={16} />}
                onClick={() => setBulkDeleteOpen(true)}
              >
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="add" size={16} />}
              onClick={openCreate}
            >
              New Product
            </Button>
          </>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search code or name…"
        filters={[
          {
            label: 'Category',
            value: categoryFilter,
            options: CATEGORIES.map((c) => ({ label: c, value: c })),
            onChange: setCategoryFilter,
          },
          {
            label: 'Status',
            value: statusFilter,
            options: [
              { label: 'Active', value: 'active' },
              { label: 'Inactive', value: 'inactive' },
            ],
            onChange: setStatusFilter,
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
                icon={search || categoryFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || categoryFilter || statusFilter ? 'No products match your filters' : 'No products yet'}
                description={
                  search || categoryFilter || statusFilter
                    ? 'Try adjusting your search or filters.'
                    : 'Add your first product to get started.'
                }
                action={
                  !search && !categoryFilter && !statusFilter ? (
                    <Button variant="contained" size="small" onClick={openCreate}>
                      Add Product
                    </Button>
                  ) : undefined
                }
                compact
              />
            ),
          }}
          sx={{
            border: '1px solid',
            borderColor: 'divider',
            borderRadius: 2,
            bgcolor: 'background.paper',
            '& .MuiDataGrid-columnHeaders': {
              bgcolor: (theme) => alpha(theme.palette.primary.main, 0.04),
              borderBottom: '1px solid',
              borderColor: 'divider',
            },
            '& .MuiDataGrid-row:hover': {
              bgcolor: (theme) => alpha(theme.palette.primary.main, 0.03),
            },
            '& .MuiDataGrid-cell:focus, & .MuiDataGrid-cell:focus-within': { outline: 'none' },
            '& .MuiDataGrid-columnHeader:focus, & .MuiDataGrid-columnHeader:focus-within': { outline: 'none' },
          }}
        />
      </Box>

      {/* Create / Edit drawer */}
      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'New Product' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter product details below' : editTarget?.name}
        onSubmit={() => document.getElementById('product-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Product' : 'Save Changes'}
        loading={saving}
      >
        <ProductForm
          key={editTarget?.id ?? 'new'}
          defaultValues={editTarget ?? {}}
          onSubmit={handleSave}
          loading={saving}
        />
      </FormDrawer>

      {/* Delete confirmation */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Product"
        description={
          <>
            Delete <strong>{deleteTarget?.name}</strong> ({deleteTarget?.code})?
            This cannot be undone and may affect existing BOMs and routings.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />

      {/* Bulk delete confirmation */}
      <ConfirmDialog
        open={bulkDeleteOpen}
        onClose={() => setBulkDeleteOpen(false)}
        onConfirm={handleBulkDelete}
        title={`Delete ${selectedIds.size} Products`}
        description={`Are you sure you want to delete ${selectedIds.size} selected products? This cannot be undone.`}
        confirmLabel={`Delete ${selectedIds.size} Products`}
        confirmColor="error"
      />
    </PageRoot>
  );
}
