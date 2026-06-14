import {
  Alert,
  Box,
  Button,
  Chip,
  Grid,
  IconButton,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useForm } from 'react-hook-form';
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
  useGetApiV1Suppliers,
  getGetApiV1SuppliersQueryKey,
  postApiV1Suppliers,
  putApiV1SuppliersCode,
  deleteApiV1SuppliersCode,
} from '../../api/suppliers/suppliers';
import type { SupplierDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const SupplierSchema = z.object({
  code:        z.string().min(1, 'Required').max(20),
  name:        z.string().min(1, 'Required').max(200),
  country:     z.string().max(100).optional().nullable(),
  city:        z.string().max(100).optional().nullable(),
  address:     z.string().max(500).optional().nullable(),
  phone:       z.string().max(50).optional().nullable(),
  email:       z.string().email('Invalid email').max(200).optional().nullable(),
  contactName: z.string().max(200).optional().nullable(),
  taxCode:     z.string().max(50).optional().nullable(),
});
type SupplierFormValues = z.infer<typeof SupplierSchema>;

function SupplierForm({ defaultValues, isEdit, onSubmit }: {
  defaultValues: Partial<SupplierFormValues>;
  isEdit: boolean;
  onSubmit: (d: SupplierFormValues) => void;
}) {
  const { register, handleSubmit, formState: { errors } } =
    useForm<SupplierFormValues>({ resolver: zodResolver(SupplierSchema), defaultValues });

  return (
    <Box component="form" id="supplier-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 5 }}>
          <TextField
            {...register('code')} label="Supplier Code" fullWidth required disabled={isEdit}
            error={!!errors.code} helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 7 }}>
          <TextField {...register('name')} label="Supplier Name" fullWidth required
            error={!!errors.name} helperText={errors.name?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('country')} label="Country" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('city')} label="City" fullWidth />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField {...register('address')} label="Address" fullWidth multiline rows={2} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('phone')} label="Phone" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('email')} label="Email" fullWidth type="email"
            error={!!errors.email} helperText={errors.email?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('contactName')} label="Contact Name" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('taxCode')} label="Tax Code" fullWidth />
        </Grid>
      </Grid>
    </Box>
  );
}

type DrawerMode = 'create' | 'edit';

export default function SuppliersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch]             = useState('');
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<SupplierDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<SupplierDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: suppliers = [], isLoading, error, refetch } = useGetApiV1Suppliers();

  const filtered = useMemo(() => {
    if (!search) return suppliers;
    const q = search.toLowerCase();
    return suppliers.filter(
      (s) => s.supplierCode.toLowerCase().includes(q) || s.supplierName.toLowerCase().includes(q)
        || (s.country ?? '').toLowerCase().includes(q) || (s.contactName ?? '').toLowerCase().includes(q),
    );
  }, [suppliers, search]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1SuppliersQueryKey() });

  const numVal = (v: number | string) => typeof v === 'number' ? v : parseInt(v, 10);

  const createMutation = useMutation({
    mutationFn: (d: SupplierFormValues) => postApiV1Suppliers({
      code: d.code, name: d.name, country: d.country ?? null, city: d.city ?? null,
      address: d.address ?? null, phone: d.phone ?? null, email: d.email ?? null,
      contactName: d.contactName ?? null, taxCode: d.taxCode ?? null,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (d: SupplierFormValues) => putApiV1SuppliersCode(editTarget!.supplierCode, {
      name: d.name, country: d.country ?? null, city: d.city ?? null,
      address: d.address ?? null, phone: d.phone ?? null, email: d.email ?? null,
      contactName: d.contactName ?? null, taxCode: d.taxCode ?? null,
      isActive: editTarget!.isActive,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => deleteApiV1SuppliersCode(code),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(s: SupplierDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(s); setDrawerOpen(true); }
  function handleSave(d: SupplierFormValues) { setSaveError(''); drawerMode === 'create' ? createMutation.mutate(d) : updateMutation.mutate(d); }

  const columns: GridColDef<SupplierDto>[] = [
    {
      field: 'supplierCode', headerName: 'Code', width: 120,
      renderCell: (p: GridRenderCellParams<SupplierDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    { field: 'supplierName', headerName: 'Supplier Name', flex: 1, minWidth: 160 },
    {
      field: 'country', headerName: 'Country / City', width: 150,
      renderCell: (p: GridRenderCellParams<SupplierDto>) => {
        const country = p.row.country;
        const city    = p.row.city;
        if (!country && !city) return <Typography variant="body2" color="text.disabled" sx={{ fontSize: 12, fontStyle: 'italic' }}>—</Typography>;
        return (
          <Stack>
            {country && <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 500 }}>{country}</Typography>}
            {city && <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>{city}</Typography>}
          </Stack>
        );
      },
    },
    {
      field: 'contactName', headerName: 'Contact', width: 160,
      renderCell: (p: GridRenderCellParams<SupplierDto>) => {
        const name  = p.row.contactName;
        const phone = p.row.phone;
        if (!name && !phone) return <Typography variant="body2" color="text.disabled" sx={{ fontSize: 12, fontStyle: 'italic' }}>—</Typography>;
        return (
          <Stack>
            {name && <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 500 }}>{name}</Typography>}
            {phone && <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>{phone}</Typography>}
          </Stack>
        );
      },
    },
    {
      field: 'avlItemCount', headerName: 'AVL Items', width: 90, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<SupplierDto>) => {
        const n = numVal(p.value as number);
        return n > 0
          ? <Chip label={n} size="small" sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.primary.main, 0.08), color: 'primary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
          : <Typography variant="caption" color="text.disabled">—</Typography>;
      },
    },
    {
      field: 'isActive', headerName: 'Active', width: 80, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<SupplierDto>) => (
        <Chip label={p.value ? 'Yes' : 'No'} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: p.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1), color: p.value ? '#15803D' : '#94A3B8', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'actions', headerName: '', width: 80, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<SupplierDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => openEdit(p.row)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete">
            <IconButton size="small" onClick={() => setDeleteTarget(p.row)} sx={{ color: 'text.secondary', '&:hover': { color: 'error.main' } }}>
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
      <PageHeader title="Suppliers" breadcrumbs={[{ label: 'Master Data' }, { label: 'Suppliers' }]} />
      <EmptyState icon="emptyTable" title="Failed to load suppliers" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Suppliers"
        subtitle="Manage approved vendor list, contacts and purchasing data"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Suppliers' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
            Add Supplier
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search code, name, country…"
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
          getRowId={(r) => r.supplierCode}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search ? 'emptySearch' : 'emptyTable'}
                title={search ? 'No suppliers match your search' : 'No suppliers yet'}
                description={search ? 'Try a different search term.' : 'Add your first supplier to get started.'}
                action={!search ? <Button variant="contained" size="small" onClick={openCreate}>Add Supplier</Button> : undefined}
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
        open={drawerOpen} onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'Add Supplier' : `Edit ${editTarget?.supplierCode}`}
        subtitle={drawerMode === 'create' ? 'Enter supplier details below' : editTarget?.supplierName}
        onSubmit={() => document.getElementById('supplier-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Add Supplier' : 'Save Changes'}
        loading={createMutation.isPending || updateMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <SupplierForm
          key={editTarget?.supplierCode ?? 'new'} isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code: editTarget.supplierCode, name: editTarget.supplierName,
            country: editTarget.country ?? '', city: editTarget.city ?? '',
            phone: editTarget.phone ?? '', email: editTarget.email ?? '',
            contactName: editTarget.contactName ?? '', taxCode: '',
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget} onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.supplierCode)}
        title="Delete Supplier"
        description={<>Delete <strong>{deleteTarget?.supplierName}</strong> ({deleteTarget?.supplierCode})?</>}
        confirmLabel="Delete" confirmColor="error" loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
