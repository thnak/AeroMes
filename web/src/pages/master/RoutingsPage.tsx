import Autocomplete from '@mui/material/Autocomplete';
import {
  Alert,
  Box,
  Button,
  Chip,
  FormControlLabel,
  Grid,
  IconButton,
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
  ConfirmDialog,
  EmptyState,
  ExportButton,
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1Routings,
  getGetApiV1RoutingsQueryKey,
  postApiV1Routings,
  putApiV1RoutingsId,
  deleteApiV1RoutingsId,
} from '../../api/routings/routings';
import { useGetApiV1Products } from '../../api/products/products';
import type { RoutingDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Form schema ──────────────────────────────────────────────────────────────

const RoutingSchema = z.object({
  code:        z.string().min(1, 'Code is required').max(20)
    .regex(/^[A-Za-z0-9\-_]+$/, 'Letters, digits, hyphens, and underscores only'),
  name:        z.string().min(1, 'Name is required').max(200),
  productCode: z.string().min(1, 'Product is required'),
  isDefault:   z.boolean(),
});

type RoutingFormValues = z.infer<typeof RoutingSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function RoutingForm({
  defaultValues,
  isEdit,
  productOptions,
  onSubmit,
}: {
  defaultValues: Partial<RoutingFormValues>;
  isEdit: boolean;
  productOptions: { code: string; name: string }[];
  onSubmit: (data: RoutingFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<RoutingFormValues>({
    resolver: zodResolver(RoutingSchema),
    defaultValues: { isDefault: false, ...defaultValues },
  });

  return (
    <Box component="form" id="routing-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 8 }}>
          <TextField
            {...register('code')}
            label="Routing Code"
            fullWidth
            required
            disabled={isEdit}
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <Controller
            name="isDefault"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={<Switch checked={field.value} onChange={field.onChange} color="primary" />}
                label="Default"
                sx={{ mt: 1, ml: 0 }}
              />
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Routing Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="productCode"
            control={control}
            render={({ field }) => (
              <Autocomplete
                disabled={isEdit}
                options={productOptions}
                getOptionLabel={(opt) => typeof opt === 'string' ? opt : `${opt.code} — ${opt.name}`}
                isOptionEqualToValue={(opt, val) => opt.code === val.code}
                value={productOptions.find((p) => p.code === field.value) ?? null}
                onChange={(_, val) => field.onChange(val?.code ?? '')}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Product"
                    required
                    error={!!errors.productCode}
                    helperText={errors.productCode?.message}
                  />
                )}
              />
            )}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function RoutingsPage() {
  const navigate    = useNavigate();
  const queryClient = useQueryClient();

  const [search, setSearch]             = useState('');
  const [productFilter, setProductFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<RoutingDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<RoutingDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: routings = [], isLoading, error, refetch } = useGetApiV1Routings({ activeOnly: false });
  const { data: allProducts = [] } = useGetApiV1Products({ activeOnly: false });

  const productNameMap = useMemo(
    () => new Map(allProducts.map((p) => [p.productCode, p.productName])),
    [allProducts],
  );

  const productOptions = useMemo(
    () => allProducts.map((p) => ({ code: p.productCode, name: p.productName })),
    [allProducts],
  );

  const productFilterOptions = useMemo(() => {
    const used = new Set(routings.map((r) => r.productCode));
    return allProducts.filter((p) => used.has(p.productCode)).map((p) => ({ label: `${p.productCode} — ${p.productName}`, value: p.productCode }));
  }, [allProducts, routings]);

  const filtered = useMemo(() => {
    let r = routings;
    if (search)        r = r.filter((rt) => rt.routingCode.toLowerCase().includes(search.toLowerCase()) || rt.routingName.toLowerCase().includes(search.toLowerCase()) || rt.productCode.toLowerCase().includes(search.toLowerCase()));
    if (productFilter) r = r.filter((rt) => rt.productCode === productFilter);
    if (statusFilter)  r = r.filter((rt) => statusFilter === 'active' ? rt.isActive : !rt.isActive);
    return r;
  }, [routings, search, productFilter, statusFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1RoutingsQueryKey({ activeOnly: false }) });

  const createMutation = useMutation({
    mutationFn: (data: RoutingFormValues) =>
      postApiV1Routings({ code: data.code, name: data.name, productCode: data.productCode, isDefault: data.isDefault }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: RoutingFormValues) =>
      putApiV1RoutingsId(Number(editTarget!.routingID), { name: data.name, isDefault: data.isDefault }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1RoutingsId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(rt: RoutingDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(rt); setDrawerOpen(true); }
  function handleSave(data: RoutingFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  const columns: GridColDef<RoutingDto>[] = [
    {
      field: 'routingCode',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<RoutingDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'routingName', headerName: 'Name', flex: 1, minWidth: 160 },
    {
      field: 'productCode',
      headerName: 'Product',
      flex: 1,
      minWidth: 200,
      renderCell: (params: GridRenderCellParams<RoutingDto>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'primary.main' }}>
            {params.value}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>
            {productNameMap.get(params.value as string) ?? ''}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'isDefault',
      headerName: 'Default',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<RoutingDto>) => (
        params.value ? (
          <Chip
            label="Default"
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha('#7C3AED', 0.1), color: '#7C3AED', border: 'none', '& .MuiChip-label': { px: 0.75 } }}
          />
        ) : null
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<RoutingDto>) => (
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
      width: 110,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<RoutingDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View Steps">
            <IconButton size="small" onClick={() => navigate(`/master/routings/${params.row.routingID}/steps`)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="eye" size={16} />
            </IconButton>
          </Tooltip>
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

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Routings" breadcrumbs={[{ label: 'Master Data' }, { label: 'Routings' }]} />
      <EmptyState icon="emptyTable" title="Failed to load routings" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Routings"
        subtitle="Manage production routing templates that define the sequence of operations for each product"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Routings' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Routing
            </Button>
          </>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search code, name, or product…"
        filters={[
          {
            label: 'Product',
            value: productFilter,
            options: productFilterOptions,
            onChange: setProductFilter,
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
          getRowId={(row) => row.routingID}
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
                icon={search || productFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || productFilter || statusFilter ? 'No routings match your filters' : 'No routings yet'}
                description={search || productFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first routing to get started.'}
                action={!search && !productFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Routing</Button>
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
        title={drawerMode === 'create' ? 'New Routing' : `Edit ${editTarget?.routingCode}`}
        subtitle={drawerMode === 'create' ? 'Enter routing details below' : (productNameMap.get(editTarget?.productCode ?? '') ?? editTarget?.productCode)}
        onSubmit={() => document.getElementById('routing-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Routing' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <RoutingForm
          key={editTarget ? String(editTarget.routingID) : 'new'}
          isEdit={drawerMode === 'edit'}
          productOptions={productOptions}
          defaultValues={editTarget ? {
            code:        editTarget.routingCode,
            name:        editTarget.routingName,
            productCode: editTarget.productCode,
            isDefault:   editTarget.isDefault,
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.routingID))}
        title="Delete Routing"
        description={
          <>
            Delete routing <strong>{deleteTarget?.routingCode}</strong> — {deleteTarget?.routingName}?
            This cannot be undone and may affect work orders using this routing.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
