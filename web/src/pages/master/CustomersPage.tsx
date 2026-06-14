import {
  Alert,
  Box,
  Button,
  Chip,
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
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
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
  useGetApiV1Customers,
  getGetApiV1CustomersQueryKey,
  postApiV1Customers,
  putApiV1CustomersCode,
  deleteApiV1CustomersCode,
} from '../../api/customers/customers';
import type { CustomerDto } from '../../api/model';
import { CustomerType } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const CUSTOMER_TYPE_LABELS: Record<string, string> = {
  Oem: 'OEM', Distributor: 'Distributor', Direct: 'Direct', Internal: 'Internal',
};

const CUSTOMER_TYPE_COLORS: Record<string, string> = {
  Oem: '#7C3AED', Distributor: '#0369A1', Direct: '#15803D', Internal: '#64748B',
};

const CustomerSchema = z.object({
  code:            z.string().min(1, 'Required').max(20),
  name:            z.string().min(1, 'Required').max(200),
  customerType:    z.string().min(1, 'Required'),
  taxId:           z.string().max(50).optional().nullable(),
  country:         z.string().max(100).optional().nullable(),
  contactName:     z.string().max(200).optional().nullable(),
  contactPhone:    z.string().max(50).optional().nullable(),
  contactEmail:    z.string().email('Invalid email').max(200).optional().nullable(),
  currency:        z.string().max(10).optional().nullable(),
  creditTermsDays: z.number().int().min(0).optional(),
});
type CustomerFormValues = z.infer<typeof CustomerSchema>;

function CustomerForm({ defaultValues, isEdit, onSubmit }: {
  defaultValues: Partial<CustomerFormValues>;
  isEdit: boolean;
  onSubmit: (d: CustomerFormValues) => void;
}) {
  const { register, control, handleSubmit, formState: { errors } } =
    useForm<CustomerFormValues>({ resolver: zodResolver(CustomerSchema), defaultValues });

  return (
    <Box component="form" id="customer-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 5 }}>
          <TextField
            {...register('code')} label="Customer Code" fullWidth required disabled={isEdit}
            error={!!errors.code} helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 7 }}>
          <TextField {...register('name')} label="Customer Name" fullWidth required
            error={!!errors.name} helperText={errors.name?.message} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller name="customerType" control={control} render={({ field }) => (
            <TextField {...field} select label="Customer Type" fullWidth required
              error={!!errors.customerType} helperText={errors.customerType?.message}>
              {Object.entries(CustomerType).map(([k, v]) => (
                <MenuItem key={k} value={v}>{CUSTOMER_TYPE_LABELS[k] ?? k}</MenuItem>
              ))}
            </TextField>
          )} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('country')} label="Country" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('taxId')} label="Tax ID" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('currency')} label="Currency" fullWidth placeholder="e.g. USD" />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('creditTermsDays', { valueAsNumber: true })} label="Credit Terms (days)" fullWidth type="number"
            slotProps={{ htmlInput: { min: 0 } }} />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('contactName')} label="Contact Name" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('contactPhone')} label="Contact Phone" fullWidth />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField {...register('contactEmail')} label="Contact Email" fullWidth type="email"
            error={!!errors.contactEmail} helperText={errors.contactEmail?.message} />
        </Grid>
      </Grid>
    </Box>
  );
}

type DrawerMode = 'create' | 'edit';

export default function CustomersPage() {
  const queryClient = useQueryClient();
  const [search, setSearch]             = useState('');
  const [typeFilter, setTypeFilter]     = useState('');
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<CustomerDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<CustomerDto | null>(null);
  const [saveError, setSaveError]       = useState('');

  const { data: customers = [], isLoading, error, refetch } = useGetApiV1Customers();

  const filtered = useMemo(() => {
    let r = customers;
    if (search) {
      const q = search.toLowerCase();
      r = r.filter((c) => c.customerCode.toLowerCase().includes(q) || c.customerName.toLowerCase().includes(q));
    }
    if (typeFilter) r = r.filter((c) => c.customerType === typeFilter);
    return r;
  }, [customers, search, typeFilter]);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: getGetApiV1CustomersQueryKey() });

  const numVal = (v: number | string) => typeof v === 'number' ? v : parseInt(v, 10);

  const createMutation = useMutation({
    mutationFn: (d: CustomerFormValues) => postApiV1Customers({
      code: d.code, name: d.name, customerType: d.customerType as never,
      taxId: d.taxId ?? null, country: d.country ?? null, address: null,
      shippingAddress: null, contactName: d.contactName ?? null,
      contactPhone: d.contactPhone ?? null, contactEmail: d.contactEmail ?? null,
      creditTermsDays: d.creditTermsDays, currency: d.currency ?? null,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const updateMutation = useMutation({
    mutationFn: (d: CustomerFormValues) => putApiV1CustomersCode(editTarget!.customerCode, {
      name: d.name, customerType: d.customerType as never,
      taxId: d.taxId ?? null, country: d.country ?? null, address: null,
      shippingAddress: null, contactName: d.contactName ?? null,
      contactPhone: d.contactPhone ?? null, contactEmail: d.contactEmail ?? null,
      creditTermsDays: d.creditTermsDays ?? 0, currency: d.currency ?? null,
      notes: null, isActive: editTarget!.isActive,
    }),
    onSuccess: () => { invalidate(); setDrawerOpen(false); },
    onError: (err) => setSaveError(getErrorMessage(err)),
  });

  const deleteMutation = useMutation({
    mutationFn: (code: string) => deleteApiV1CustomersCode(code),
    onSuccess: () => { invalidate(); setDeleteTarget(null); },
  });

  function openCreate() { setSaveError(''); setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(c: CustomerDto) { setSaveError(''); setDrawerMode('edit'); setEditTarget(c); setDrawerOpen(true); }
  function handleSave(d: CustomerFormValues) { setSaveError(''); drawerMode === 'create' ? createMutation.mutate(d) : updateMutation.mutate(d); }

  const columns: GridColDef<CustomerDto>[] = [
    {
      field: 'customerCode', headerName: 'Code', width: 120,
      renderCell: (p: GridRenderCellParams<CustomerDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    { field: 'customerName', headerName: 'Customer Name', flex: 1, minWidth: 160 },
    {
      field: 'customerType', headerName: 'Type', width: 110,
      renderCell: (p: GridRenderCellParams<CustomerDto>) => {
        const t = p.value as string;
        const color = CUSTOMER_TYPE_COLORS[t] ?? '#64748B';
        return (
          <Chip label={CUSTOMER_TYPE_LABELS[t] ?? t} size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        );
      },
    },
    {
      field: 'country', headerName: 'Country', width: 100,
      renderCell: (p: GridRenderCellParams<CustomerDto>) =>
        p.value
          ? <Typography variant="body2" sx={{ fontSize: 12 }}>{p.value}</Typography>
          : <Typography variant="body2" color="text.disabled" sx={{ fontSize: 12, fontStyle: 'italic' }}>—</Typography>,
    },
    {
      field: 'contactName', headerName: 'Contact', width: 150,
      renderCell: (p: GridRenderCellParams<CustomerDto>) => {
        const name  = p.row.contactName;
        const phone = p.row.contactPhone;
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
      field: 'currency', headerName: 'Currency', width: 80, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<CustomerDto>) => (
        <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600, fontFamily: 'ui-monospace, monospace' }}>
          {p.value ?? '—'}
        </Typography>
      ),
    },
    {
      field: 'partNumberCount', headerName: 'Parts', width: 70, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<CustomerDto>) => {
        const n = numVal(p.value as number);
        return n > 0
          ? <Chip label={n} size="small" sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: (t) => alpha(t.palette.primary.main, 0.08), color: 'primary.main', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
          : <Typography variant="caption" color="text.disabled">—</Typography>;
      },
    },
    {
      field: 'isActive', headerName: 'Active', width: 80, align: 'center', headerAlign: 'center',
      renderCell: (p: GridRenderCellParams<CustomerDto>) => (
        <Chip label={p.value ? 'Yes' : 'No'} size="small"
          sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: p.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1), color: p.value ? '#15803D' : '#94A3B8', border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
      ),
    },
    {
      field: 'actions', headerName: '', width: 80, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<CustomerDto>) => (
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
      <PageHeader title="Customers" breadcrumbs={[{ label: 'Master Data' }, { label: 'Customers' }]} />
      <EmptyState icon="emptyTable" title="Failed to load customers" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Customers"
        subtitle="Manage customer accounts, contacts and part number cross-references"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Customers' }]}
        actions={
          <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
            Add Customer
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search code or name…"
        filters={[
          { label: 'Type', value: typeFilter, options: Object.entries(CustomerType).map(([k, v]) => ({ label: CUSTOMER_TYPE_LABELS[k] ?? k, value: v })), onChange: setTypeFilter },
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
          getRowId={(r) => r.customerCode}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || typeFilter ? 'emptySearch' : 'emptyTable'}
                title={search || typeFilter ? 'No customers match your filters' : 'No customers yet'}
                description={search || typeFilter ? 'Try adjusting your filters.' : 'Add your first customer to get started.'}
                action={!search && !typeFilter ? <Button variant="contained" size="small" onClick={openCreate}>Add Customer</Button> : undefined}
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
        title={drawerMode === 'create' ? 'Add Customer' : `Edit ${editTarget?.customerCode}`}
        subtitle={drawerMode === 'create' ? 'Enter customer details below' : editTarget?.customerName}
        onSubmit={() => document.getElementById('customer-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Add Customer' : 'Save Changes'}
        loading={createMutation.isPending || updateMutation.isPending}
      >
        {saveError && <Alert severity="error" sx={{ mb: 2 }}>{saveError}</Alert>}
        <CustomerForm
          key={editTarget?.customerCode ?? 'new'} isEdit={drawerMode === 'edit'}
          defaultValues={editTarget ? {
            code: editTarget.customerCode, name: editTarget.customerName,
            customerType: editTarget.customerType,
            country: editTarget.country ?? '',
            contactName: editTarget.contactName ?? '',
            contactPhone: editTarget.contactPhone ?? '',
            contactEmail: editTarget.contactEmail ?? '',
            currency: editTarget.currency ?? '',
          } : {}}
          onSubmit={handleSave}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget} onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.customerCode)}
        title="Delete Customer"
        description={<>Delete <strong>{deleteTarget?.customerName}</strong> ({deleteTarget?.customerCode})?</>}
        confirmLabel="Delete" confirmColor="error" loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
