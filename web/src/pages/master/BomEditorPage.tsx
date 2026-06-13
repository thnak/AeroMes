import Autocomplete from '@mui/material/Autocomplete';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
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
import { useState, useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ConfirmDialog,
  EmptyState,
  FormDrawer,
  PageHeader,
  PageRoot,
  SolarIcon,
  TablePageSkeleton,
} from '../../components';
import {
  useGetApiV1BomItemsParentCode,
  getGetApiV1BomItemsParentCodeQueryKey,
  postApiV1BomItems,
  putApiV1BomItemsId,
  deleteApiV1BomItemsId,
} from '../../api/bom-items/bom-items';
import {
  useGetApiV1Products,
  useGetApiV1ProductsCode,
} from '../../api/products/products';
import type { BomItemDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Form schema ──────────────────────────────────────────────────────────────

const BomItemSchema = z.object({
  childProductCode: z.string().min(1, 'Child product is required'),
  requiredQty:      z.coerce.number().positive('Quantity must be positive'),
  scrapFactor:      z.coerce.number().min(0, 'Cannot be negative').max(100, 'Max 100%'),
});

type BomItemFormValues = z.infer<typeof BomItemSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function BomItemForm({
  defaultValues,
  isEdit,
  productOptions,
  onSubmit,
}: {
  defaultValues: Partial<BomItemFormValues>;
  isEdit: boolean;
  productOptions: { code: string; name: string }[];
  onSubmit: (data: BomItemFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<BomItemFormValues>({
    resolver: zodResolver(BomItemSchema) as any,
    defaultValues: { scrapFactor: 0, ...defaultValues },
  });

  return (
    <Box component="form" id="bom-item-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="childProductCode"
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
                    label="Child Product"
                    required
                    error={!!errors.childProductCode}
                    helperText={errors.childProductCode?.message}
                  />
                )}
              />
            )}
          />
        </Grid>
        <Grid size={{ xs: 6 }}>
          <TextField
            {...register('requiredQty')}
            label="Required Qty"
            type="number"
            fullWidth
            required
            error={!!errors.requiredQty}
            helperText={errors.requiredQty?.message}
            slotProps={{ htmlInput: { min: 0.0001, step: 0.0001 } }}
          />
        </Grid>
        <Grid size={{ xs: 6 }}>
          <TextField
            {...register('scrapFactor')}
            label="Scrap Factor (%)"
            type="number"
            fullWidth
            error={!!errors.scrapFactor}
            helperText={errors.scrapFactor?.message ?? '0 = no scrap'}
            slotProps={{ htmlInput: { min: 0, max: 100, step: 0.01 } }}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function BomEditorPage() {
  const { id: productCode } = useParams<{ id: string }>();
  const navigate             = useNavigate();
  const queryClient          = useQueryClient();

  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<BomItemDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<BomItemDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: product }                                          = useGetApiV1ProductsCode(productCode ?? '');
  const { data: bomItems = [], isLoading, error, refetch }         = useGetApiV1BomItemsParentCode(productCode ?? '');
  const { data: allProducts = [] }                                 = useGetApiV1Products({ activeOnly: false });

  const productNameMap = useMemo(
    () => new Map(allProducts.map((p) => [p.productCode, p.productName])),
    [allProducts],
  );

  const existingChildCodes = useMemo(
    () => new Set(bomItems.map((b) => b.childProductCode)),
    [bomItems],
  );

  const availableForAdd = useMemo(
    () =>
      allProducts
        .filter((p) => p.isActive && p.productCode !== productCode && !existingChildCodes.has(p.productCode))
        .map((p) => ({ code: p.productCode, name: p.productName })),
    [allProducts, productCode, existingChildCodes],
  );

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1BomItemsParentCodeQueryKey(productCode ?? '') });

  const createMutation = useMutation({
    mutationFn: (data: BomItemFormValues) =>
      postApiV1BomItems({
        parentProductCode: productCode!,
        childProductCode:  data.childProductCode,
        requiredQty:       data.requiredQty,
        scrapFactor:       data.scrapFactor,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (data: BomItemFormValues) =>
      putApiV1BomItemsId(Number(editTarget!.bomID), {
        requiredQty: data.requiredQty,
        scrapFactor: data.scrapFactor,
      }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => deleteApiV1BomItemsId(id),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  const saving = createMutation.isPending || updateMutation.isPending;

  function openCreate() {
    setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true);
  }
  function openEdit(row: BomItemDto) {
    setSaveError(''); setDrawerMode('edit'); setEditTarget(row); setDrawerOpen(true);
  }
  function handleSave(data: BomItemFormValues) {
    setSaveError('');
    if (drawerMode === 'create') createMutation.mutate(data);
    else updateMutation.mutate(data);
  }

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader
        title="BOM Editor"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Products', href: '/master/products' }, { label: productCode ?? '' }, { label: 'BOM' }]}
      />
      <EmptyState icon="emptyTable" title="Failed to load BOM" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title={`BOM — ${productCode}`}
        subtitle={product?.productName ?? ''}
        breadcrumbs={[
          { label: 'Master Data' },
          { label: 'Products', href: '/master/products' },
          { label: productCode ?? '' },
          { label: 'BOM' },
        ]}
        actions={
          <>
            <Button variant="outlined" size="small" startIcon={<SolarIcon name="back" size={16} />} onClick={() => navigate(-1)}>
              Back
            </Button>
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              Add Component
            </Button>
          </>
        }
      />

      {/* Summary */}
      <Card variant="outlined" sx={{ borderRadius: 2, mb: 2 }}>
        <CardContent sx={{ py: '12px !important' }}>
          <Stack direction="row" spacing={1.5} sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5, fontSize: '0.6875rem' }}>
              BOM Summary
            </Typography>
            {[
              { label: `${bomItems.length} component${bomItems.length !== 1 ? 's' : ''}`, color: '#1D4ED8' },
              { label: `${bomItems.filter((b) => b.isActive).length} active`, color: '#15803D' },
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
          <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', fontSize: '0.6875rem', letterSpacing: 0.5 }}>
              Components
            </Typography>
            <Tooltip title="Refresh">
              <IconButton size="small" onClick={() => refetch()} sx={{ color: 'text.secondary' }}>
                <SolarIcon name="refresh" size={16} />
              </IconButton>
            </Tooltip>
          </Stack>

          {bomItems.length === 0 ? (
            <Box sx={{ py: 4 }}>
              <EmptyState
                icon="emptyTable"
                title="No components yet"
                description="Click 'Add Component' to define the bill of materials for this product."
                action={<Button variant="contained" size="small" onClick={openCreate}>Add Component</Button>}
                compact
              />
            </Box>
          ) : (
            <Table size="small">
              <TableHead>
                <TableRow sx={{ '& th': { py: 0.75, fontWeight: 600, fontSize: '0.75rem', borderColor: 'divider', color: 'text.secondary', bgcolor: (t) => alpha(t.palette.primary.main, 0.03) } }}>
                  <TableCell width={160}>Component Code</TableCell>
                  <TableCell>Component Name</TableCell>
                  <TableCell width={110} align="right">Req. Qty</TableCell>
                  <TableCell width={100} align="right">Scrap %</TableCell>
                  <TableCell width={90} align="center">Status</TableCell>
                  <TableCell width={80} align="center" />
                </TableRow>
              </TableHead>
              <TableBody>
                {bomItems.map((row) => (
                  <TableRow
                    key={String(row.bomID)}
                    sx={{
                      '& td': { py: 0.5, fontSize: '0.8125rem', borderColor: 'divider' },
                      '&:last-child td': { border: 0 },
                      '&:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.02) },
                    }}
                  >
                    <TableCell>
                      <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
                        {row.childProductCode}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontSize: 12 }}>
                        {productNameMap.get(row.childProductCode) ?? (
                          <Box component="span" sx={{ color: 'text.disabled', fontStyle: 'italic' }}>Unknown</Box>
                        )}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 500 }}>
                        {Number(row.requiredQty)}
                      </Typography>
                    </TableCell>
                    <TableCell align="right">
                      <Typography variant="body2" sx={{ fontSize: 12, color: Number(row.scrapFactor) > 0 ? '#D97706' : 'text.disabled' }}>
                        {Number(row.scrapFactor) > 0 ? `${Number(row.scrapFactor)}%` : '—'}
                      </Typography>
                    </TableCell>
                    <TableCell align="center">
                      <Chip
                        label={row.isActive ? 'Active' : 'Inactive'}
                        size="small"
                        sx={{
                          height: 20,
                          fontSize: '0.6875rem',
                          fontWeight: 600,
                          bgcolor: row.isActive ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
                          color: row.isActive ? '#15803D' : '#94A3B8',
                          border: 'none',
                          '& .MuiChip-label': { px: 0.75 },
                        }}
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Stack direction="row" spacing={0.25} sx={{ justifyContent: 'center' }}>
                        <Tooltip title="Edit">
                          <IconButton size="small" onClick={() => openEdit(row)} sx={{ color: 'text.secondary' }}>
                            <SolarIcon name="edit" size={14} />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Remove">
                          <IconButton size="small" onClick={() => setDeleteTarget(row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
                            <SolarIcon name="delete" size={14} />
                          </IconButton>
                        </Tooltip>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}

          <Box sx={{ mt: 2, p: 1.5, borderRadius: 1, bgcolor: (t) => alpha(t.palette.warning.main, 0.06), border: '1px solid', borderColor: (t) => alpha(t.palette.warning.main, 0.2) }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
              <SolarIcon name="warning" size={14} color="#D97706" />
              <Typography variant="caption" color="text.secondary">
                BOM changes take effect from the next released Work Order.
              </Typography>
            </Stack>
          </Box>
        </CardContent>
      </Card>

      {/* Drawer */}
      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'Add Component' : `Edit ${editTarget?.childProductCode}`}
        subtitle={
          drawerMode === 'create'
            ? 'Select a child product and set the required quantity'
            : (productNameMap.get(editTarget?.childProductCode ?? '') ?? '')
        }
        onSubmit={() => document.getElementById('bom-item-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Add Component' : 'Save Changes'}
        loading={saving}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <BomItemForm
          key={drawerMode === 'edit' ? String(editTarget?.bomID) : 'new'}
          isEdit={drawerMode === 'edit'}
          productOptions={
            drawerMode === 'create'
              ? availableForAdd
              : (editTarget ? [{ code: editTarget.childProductCode, name: productNameMap.get(editTarget.childProductCode) ?? '' }] : [])
          }
          defaultValues={
            editTarget
              ? { childProductCode: editTarget.childProductCode, requiredQty: Number(editTarget.requiredQty), scrapFactor: Number(editTarget.scrapFactor) }
              : {}
          }
          onSubmit={handleSave}
        />
      </FormDrawer>

      {/* Confirm delete */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(Number(deleteTarget.bomID))}
        title="Remove BOM Component"
        description={
          <>
            Remove <strong>{productNameMap.get(deleteTarget?.childProductCode ?? '') ?? deleteTarget?.childProductCode}</strong> ({deleteTarget?.childProductCode}) from this BOM?
            This cannot be undone.
          </>
        }
        confirmLabel="Remove"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
