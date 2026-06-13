import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Divider,
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
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  AttachmentList,
  ConfirmDialog,
  EmptyState,
  ExportButton,
  FileUpload,
  FormDrawer,
  LabelDialog,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import type { FileUploadResult } from '../../api/model';
import { getGetApiV1FilesQueryKey } from '../../api/files/files';
import {
  useGetApiV1Products,
  getGetApiV1ProductsQueryKey,
  postApiV1Products,
  putApiV1ProductsCode,
  deleteApiV1ProductsCode,
  useGetApiV1ProductsCode,
} from '../../api/products/products';
import type { ProductDto } from '../../api/model';
import { ItemType, ProcurementType, TrackingMethod, ProductClass } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Constants ────────────────────────────────────────────────────────────────

const ITEM_TYPE_LABELS: Record<string, string> = {
  FG: 'Finished Good', SEMI: 'Semi-finished', RM: 'Raw Material',
  CONS: 'Consumable', PKG: 'Packaging', SPARE: 'Spare Part', TOOL: 'Tooling',
};

const ITEM_TYPE_COLORS: Record<string, string> = {
  FG: '#1D4ED8', SEMI: '#0369A1', RM: '#15803D',
  CONS: '#D97706', PKG: '#7C3AED', SPARE: '#0D9488', TOOL: '#64748B',
};

const LIFECYCLE_LABELS: Record<string, string> = {
  Development: 'Dev', Active: 'Active', PhasingOut: 'Phasing Out',
  Discontinued: 'Discont.', Obsolete: 'Obsolete',
};

const LIFECYCLE_COLORS: Record<string, string> = {
  Development: '#0369A1', Active: '#15803D', PhasingOut: '#D97706',
  Discontinued: '#DC2626', Obsolete: '#64748B',
};

const UOM_OPTIONS = ['EA', 'PCS', 'SET', 'KG', 'G', 'M', 'M2', 'M3', 'L', 'BOX', 'ROLL'];

const TRACKING_COLORS: Record<string, string> = {
  None: '#64748B', Lot: '#1D4ED8', Serial: '#7C3AED',
};

const PRODUCT_CLASS_LABELS: Record<string, string> = {
  Standard: 'Standard', RawMaterial: 'Raw Material', Fabric: 'Fabric',
  Resin: 'Resin', FinishedGood: 'Finished Good', SemiFinished: 'Semi-Finished', Consumable: 'Consumable',
};

// ─── Form schema ──────────────────────────────────────────────────────────────

const ProductSchema = z.object({
  code:             z.string().min(1, 'Code is required').max(50)
    .regex(/^[A-Za-z0-9\-_.]+$/, 'Letters, digits, hyphens, underscores, and dots only'),
  name:             z.string().min(1, 'Name is required').max(200),
  baseUoMCode:      z.string().min(1, 'Unit of measure is required').max(20),
  itemType:         z.string().min(1, 'Item type is required'),
  procurementType:  z.string().optional(),
  lotControlled:    z.boolean(),
  serialControlled: z.boolean(),
  customerPartNo:   z.string().max(100).optional(),
  drawingNo:        z.string().max(100).optional(),
  revision:         z.string().max(50).optional(),
  trackingMethod:   z.string().optional(),
  productClass:     z.string().optional(),
});

type ProductFormValues = z.infer<typeof ProductSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function ProductForm({
  defaultValues,
  isEdit,
  onSubmit,
}: {
  defaultValues: Partial<ProductFormValues>;
  isEdit: boolean;
  onSubmit: (data: ProductFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<ProductFormValues>({
    resolver: zodResolver(ProductSchema),
    defaultValues: { lotControlled: false, serialControlled: false, ...defaultValues },
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
            disabled={isEdit}
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="itemType"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? ''}
                select
                label="Item Type"
                fullWidth
                required
                error={!!errors.itemType}
                helperText={errors.itemType?.message}
              >
                {Object.values(ItemType).map((t) => (
                  <MenuItem key={t} value={t}>{ITEM_TYPE_LABELS[t] ?? t}</MenuItem>
                ))}
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
            name="baseUoMCode"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? ''}
                select
                label="Base Unit of Measure"
                fullWidth
                required
                error={!!errors.baseUoMCode}
                helperText={errors.baseUoMCode?.message}
              >
                {UOM_OPTIONS.map((u) => (
                  <MenuItem key={u} value={u}>{u}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="procurementType"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? ''}
                select
                label="Procurement Type"
                fullWidth
                error={!!errors.procurementType}
                helperText={errors.procurementType?.message}
              >
                <MenuItem value=""><em>None</em></MenuItem>
                {Object.values(ProcurementType).map((p) => (
                  <MenuItem key={p} value={p}>{p}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('customerPartNo')}
            label="Customer Part No."
            fullWidth
            error={!!errors.customerPartNo}
            helperText={errors.customerPartNo?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('drawingNo')}
            label="Drawing No."
            fullWidth
            error={!!errors.drawingNo}
            helperText={errors.drawingNo?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('revision')}
            label="Revision"
            fullWidth
            error={!!errors.revision}
            helperText={errors.revision?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="trackingMethod"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? 'None'}
                select
                label="Tracking Method"
                fullWidth
              >
                {Object.values(TrackingMethod).map((t) => (
                  <MenuItem key={t} value={t}>{t}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="productClass"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                value={field.value ?? 'Standard'}
                select
                label="Product Class"
                fullWidth
              >
                {Object.values(ProductClass).map((c) => (
                  <MenuItem key={c} value={c}>{PRODUCT_CLASS_LABELS[c] ?? c}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Stack direction="row" spacing={1} sx={{ mt: 0.5 }}>
            <Controller
              name="lotControlled"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={<Switch checked={field.value} onChange={field.onChange} color="primary" size="small" />}
                  label="Lot Controlled"
                  sx={{ ml: 0 }}
                />
              )}
            />
            <Controller
              name="serialControlled"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={<Switch checked={field.value} onChange={field.onChange} color="primary" size="small" />}
                  label="Serial"
                  sx={{ ml: 0 }}
                />
              )}
            />
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function ProductsPage() {
  const navigate    = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch]                   = useState('');
  const [itemTypeFilter, setItemTypeFilter]   = useState('');
  const [statusFilter, setStatusFilter]       = useState('');
  const [selection, setSelection]             = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]           = useState(false);
  const [drawerMode, setDrawerMode]           = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]           = useState<ProductDto | null>(null);
  const [deleteTarget, setDeleteTarget]       = useState<ProductDto | null>(null);
  const [labelTarget, setLabelTarget]         = useState<ProductDto | null>(null);
  const [saveError, setSaveError]             = useState('');

  const { data: products = [], isLoading, error, refetch } =
    useGetApiV1Products({ activeOnly: false });

  // Fetch full detail when editing (to preserve fields not shown in the form)
  const { data: editDetail, isLoading: editDetailLoading } = useGetApiV1ProductsCode(
    editTarget?.productCode ?? '',
    { query: { enabled: drawerMode === 'edit' && !!editTarget?.productCode } },
  );

  const filtered = useMemo(() => {
    let r = products;
    if (search)        r = r.filter((p) => p.productName.toLowerCase().includes(search.toLowerCase()) || p.productCode.toLowerCase().includes(search.toLowerCase()));
    if (itemTypeFilter) r = r.filter((p) => p.itemType === itemTypeFilter);
    if (statusFilter)  r = r.filter((p) => statusFilter === 'active' ? p.isActive : !p.isActive);
    return r;
  }, [products, search, itemTypeFilter, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1ProductsQueryKey({ activeOnly: false }) });

  const createMutation = useMutation({
    mutationFn: (data: ProductFormValues) =>
      postApiV1Products({
        code: data.code,
        name: data.name,
        baseUoMCode: data.baseUoMCode,
        itemType: data.itemType as typeof ItemType[keyof typeof ItemType],
        procurementType: (data.procurementType || null) as typeof ProcurementType[keyof typeof ProcurementType] | null,
        lotControlled: data.lotControlled,
        serialControlled: data.serialControlled,
        customerPartNo: data.customerPartNo ?? null,
        drawingNo: data.drawingNo ?? null,
        revision: data.revision ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: ProductFormValues) =>
      putApiV1ProductsCode(editTarget!.productCode, {
        name: data.name,
        baseUoMCode: data.baseUoMCode,
        purchaseUoMCode: editDetail?.purchaseUoMCode ?? null,
        purchaseToBaseQty: editDetail?.purchaseToBaseQty ?? 1,
        itemType: data.itemType as typeof ItemType[keyof typeof ItemType],
        categoryId: editDetail?.categoryId ?? null,
        barcodePattern: editDetail?.barcodePattern ?? null,
        lotControlled: data.lotControlled,
        serialControlled: data.serialControlled,
        shelfLifeDays: editDetail?.shelfLifeDays ?? null,
        reorderPoint: editDetail?.reorderPoint ?? null,
        safetyStock: editDetail?.safetyStock ?? null,
        leadTimeDays: editDetail?.leadTimeDays ?? null,
        procurementType: (data.procurementType || null) as typeof ProcurementType[keyof typeof ProcurementType] | null,
        effectiveFrom: editDetail?.effectiveFrom ?? null,
        effectiveTo: editDetail?.effectiveTo ?? null,
        customerPartNo: data.customerPartNo ?? null,
        drawingNo: data.drawingNo ?? null,
        revision: data.revision ?? null,
        netWeight: editDetail?.netWeight ?? null,
        grossWeight: editDetail?.grossWeight ?? null,
        length: editDetail?.length ?? null,
        width: editDetail?.width ?? null,
        height: editDetail?.height ?? null,
        imageUrl: editDetail?.imageUrl ?? null,
        thumbnailUrl: editDetail?.thumbnailUrl ?? null,
        fixedPurchasePrice: editDetail?.fixedPurchasePrice ?? null,
        technicalStandard: editDetail?.technicalStandard ?? null,
        quantityFormula: editDetail?.quantityFormula ?? null,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => deleteApiV1ProductsCode(code),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(p: ProductDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(p); setDrawerOpen(true); }

  function handleSave(data: ProductFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  const columns: GridColDef<ProductDto>[] = [
    {
      field: 'productCode',
      headerName: 'Code',
      width: 140,
      renderCell: (params: GridRenderCellParams<ProductDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'productName', headerName: 'Name', flex: 1, minWidth: 180 },
    {
      field: 'itemType',
      headerName: 'Type',
      width: 110,
      renderCell: (params: GridRenderCellParams<ProductDto>) => {
        const color = ITEM_TYPE_COLORS[params.value as string] ?? '#64748B';
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{
              height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha(color, 0.1), color, border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'baseUoMCode',
      headerName: 'Unit',
      width: 65,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<ProductDto>) => (
        <Typography variant="body2" sx={{ fontSize: 11, fontWeight: 600, color: 'text.secondary', fontFamily: 'ui-monospace, monospace' }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'lifecycleStatus',
      headerName: 'Lifecycle',
      width: 110,
      renderCell: (params: GridRenderCellParams<ProductDto>) => {
        const color = LIFECYCLE_COLORS[params.value as string] ?? '#64748B';
        return (
          <Chip
            label={LIFECYCLE_LABELS[params.value as string] ?? params.value}
            size="small"
            sx={{
              height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha(color, 0.1), color, border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'trackingMethod',
      headerName: 'Tracking',
      width: 90,
      renderCell: (params: GridRenderCellParams<ProductDto>) => {
        const val = (params.value as string) ?? 'None';
        const color = TRACKING_COLORS[val] ?? '#64748B';
        return (
          <Chip
            label={val}
            size="small"
            sx={{
              height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha(color, 0.1), color, border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<ProductDto>) => (
        <Chip
          label={params.value ? 'Active' : 'Inactive'}
          size="small"
          sx={{
            height: 20, fontSize: '0.6875rem', fontWeight: 600,
            bgcolor: params.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
            color: params.value ? '#15803D' : '#94A3B8',
            border: 'none', '& .MuiChip-label': { px: 0.75 },
          }}
        />
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 140,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<ProductDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View Detail">
            <IconButton size="small" onClick={() => navigate(`/master/products/${params.row.productCode}`)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="eye" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit BOM">
            <IconButton size="small" onClick={() => navigate(`/master/products/${params.row.productCode}/bom`)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="bom" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => openEdit(params.row)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Print Label">
            <IconButton size="small" onClick={() => setLabelTarget(params.row)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="print" size={16} />
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

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Products" breadcrumbs={[{ label: 'Master Data' }, { label: 'Products' }]} />
      <EmptyState icon="emptyTable" title="Failed to load products" description={getErrorMessage(error)} />
    </PageRoot>
  );

  const isEditFormReady = drawerMode === 'create' || (drawerMode === 'edit' && !!editDetail && !editDetailLoading);

  return (
    <PageRoot>
      <PageHeader
        title="Products"
        subtitle="Manage product master data — codes, types, units, and procurement settings"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Products' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
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
            label: 'Type',
            value: itemTypeFilter,
            options: Object.values(ItemType).map((t) => ({ label: ITEM_TYPE_LABELS[t] ?? t, value: t })),
            onChange: setItemTypeFilter,
          },
          {
            label: 'Status',
            value: statusFilter,
            options: [{ label: 'Active', value: 'active' }, { label: 'Inactive', value: 'inactive' }],
            onChange: setStatusFilter,
          },
        ]}
        totalCount={filtered.length}
        actions={
          <Stack direction="row" spacing={0.5}>
            <ExportButton />
            <RefreshButton onClick={() => refetch()} />
          </Stack>
        }
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        <DataGrid
          rows={filtered}
          getRowId={(row) => row.productCode}
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
                icon={search || itemTypeFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || itemTypeFilter || statusFilter ? 'No products match your filters' : 'No products yet'}
                description={search || itemTypeFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first product to get started.'}
                action={!search && !itemTypeFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Product</Button>
                ) : undefined}
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
        title={drawerMode === 'create' ? 'New Product' : `Edit ${editTarget?.productCode}`}
        subtitle={drawerMode === 'create' ? 'Enter product details below' : editTarget?.productName}
        onSubmit={() => document.getElementById('product-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Product' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        {!isEditFormReady ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress size={32} />
          </Box>
        ) : (
          <ProductForm
            key={drawerMode === 'edit' ? editTarget?.productCode : 'new'}
            isEdit={drawerMode === 'edit'}
            defaultValues={editDetail ? {
              code:             editDetail.productCode,
              name:             editDetail.productName,
              baseUoMCode:      editDetail.baseUoMCode,
              itemType:         editDetail.itemType,
              procurementType:  editDetail.procurementType ?? undefined,
              lotControlled:    editDetail.lotControlled,
              serialControlled: editDetail.serialControlled,
              customerPartNo:   editDetail.customerPartNo ?? undefined,
              drawingNo:        editDetail.drawingNo ?? undefined,
              revision:         editDetail.revision ?? undefined,
            } : {}}
            onSubmit={handleSave}
          />
        )}
        {drawerMode === 'edit' && editTarget && (
          <>
            <Divider sx={{ my: 2 }} />
            <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>Attachments</Typography>
            <AttachmentList ownerType="product" ownerId={editTarget.productCode} canDelete layout="gallery" />
            <FileUpload
              ownerType="product"
              ownerId={editTarget.productCode}
              onUploaded={(_r: FileUploadResult) => {
                queryClient.invalidateQueries({ queryKey: getGetApiV1FilesQueryKey({ ownerType: 'product', ownerId: editTarget.productCode }) });
              }}
            />
          </>
        )}
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.productCode)}
        title="Delete Product"
        description={
          <>
            Delete <strong>{deleteTarget?.productName}</strong> ({deleteTarget?.productCode})?
            This cannot be undone and may affect BOMs and work orders referencing this product.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />

      <LabelDialog
        open={!!labelTarget}
        onClose={() => setLabelTarget(null)}
        contentType="PRD"
        entityId={labelTarget?.productCode ?? ''}
        entityLabel={labelTarget?.productName}
      />
    </PageRoot>
  );
}
