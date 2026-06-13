import {
  Alert,
  Box,
  Button,
  Chip,
  Divider,
  Drawer,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useState } from 'react';
import { useForm, Controller, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  DataGrid,
  type GridColDef,
  type GridRenderCellParams,
} from '@mui/x-data-grid';
import {
  AttachmentList,
  EmptyState,
  FileUpload,
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
  useGetApiV1SopDocuments,
  postApiV1SopDocuments,
  patchApiV1SopDocumentsIdStatus,
  getGetApiV1SopDocumentsQueryKey,
} from '../../api/sop/sop';
import { getErrorMessage } from '../../lib/apiClient';
import type { CreateSopDocumentRequest, SopDocumentListDto } from '../../api/model';

// ── Constants ──────────────────────────────────────────────────────────────

const CATEGORIES = ['SAFETY', 'SETUP', 'PROCESS', 'QUALITY', 'CLEANUP'];
const COMPLETION_MODES = ['MANUAL', 'AUTO_SIGNAL', 'AUTO_EVENT'];

const STATUS_COLORS: Record<string, 'default' | 'info' | 'success' | 'error'> = {
  DRAFT: 'default',
  ACTIVE: 'success',
  SUPERSEDED: 'error',
};

// ── Form schema ────────────────────────────────────────────────────────────

const itemSchema = z.object({
  category: z.string().min(1),
  itemText: z.string().min(1),
  isRequired: z.boolean(),
  completionMode: z.string().min(1),
  autoConfig: z.string().optional(),
  specMin: z.number().optional(),
  specMax: z.number().optional(),
  unit: z.string().optional(),
  photoRequired: z.boolean(),
});

const schema = z.object({
  code: z.string().min(1),
  title: z.string().min(1),
  version: z.string().min(1),
  routingStepId: z.number().int().positive(),
  productCode: z.string().optional(),
  effectiveFrom: z.string().min(1),
  notes: z.string().optional(),
  items: z.array(itemSchema).min(1, 'At least one check item required'),
});

type FormValues = z.infer<typeof schema>;

// ── Page ───────────────────────────────────────────────────────────────────

export default function SopDocumentsPage() {
  const qc = useQueryClient();
  const [filterStatus, setFilterStatus] = useState('');
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [viewTarget, setViewTarget] = useState<SopDocumentListDto | null>(null);

  const { data, isLoading, error, refetch } = useGetApiV1SopDocuments({
    status: filterStatus || undefined,
  });

  const docs: SopDocumentListDto[] = data ?? [];
  const filtered = search
    ? docs.filter(
        (d) =>
          d.code.toLowerCase().includes(search.toLowerCase()) ||
          d.title.toLowerCase().includes(search.toLowerCase()),
      )
    : docs;

  const createMut = useMutation({
    mutationFn: (body: CreateSopDocumentRequest) => postApiV1SopDocuments(body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: getGetApiV1SopDocumentsQueryKey() });
      setCreateOpen(false);
      reset();
    },
  });

  const activateMut = useMutation({
    mutationFn: ({ id }: { id: number }) =>
      patchApiV1SopDocumentsIdStatus(id, { status: 'ACTIVE' }),
    onSuccess: () => qc.invalidateQueries({ queryKey: getGetApiV1SopDocumentsQueryKey() }),
  });

  const { control, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      code: '',
      title: '',
      version: 'v1.0',
      routingStepId: 1,
      effectiveFrom: new Date().toISOString().split('T')[0],
      items: [{ category: 'PROCESS', itemText: '', isRequired: true, completionMode: 'MANUAL', photoRequired: false }],
    },
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'items' });

  const onSubmit = (values: FormValues) => {
    createMut.mutate({
      code: values.code,
      title: values.title,
      version: values.version,
      routingStepId: values.routingStepId,
      productCode: values.productCode || null,
      effectiveFrom: values.effectiveFrom,
      notes: values.notes || null,
      items: values.items.map((i) => ({
        category: i.category,
        itemText: i.itemText,
        isRequired: i.isRequired,
        completionMode: i.completionMode,
        autoConfig: i.autoConfig || null,
        specMin: i.specMin ?? null,
        specMax: i.specMax ?? null,
        unit: i.unit || null,
        photoRequired: i.photoRequired,
      })),
    });
  };

  const tableFilters = [
    {
      label: 'Status',
      value: filterStatus,
      options: [
        { value: '', label: 'All statuses' },
        ...Object.keys(STATUS_COLORS).map((s) => ({ value: s, label: s })),
      ],
      onChange: setFilterStatus,
      width: 150,
    },
  ];

  const columns: GridColDef<SopDocumentListDto>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 140,
      renderCell: (p: GridRenderCellParams<SopDocumentListDto, string>) => (
        <Typography variant="caption" sx={{ fontFamily: 'monospace', fontWeight: 700 }}>
          {p.value}
        </Typography>
      ),
    },
    { field: 'title', headerName: 'Title', flex: 1, minWidth: 200 },
    { field: 'version', headerName: 'Version', width: 90 },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      renderCell: (p: GridRenderCellParams<SopDocumentListDto, string>) => (
        <Chip label={p.value} color={STATUS_COLORS[p.value ?? ''] ?? 'default'} size="small" />
      ),
    },
    {
      field: 'routingStepId',
      headerName: 'Routing Step',
      width: 120,
      type: 'number',
    },
    {
      field: 'productCode',
      headerName: 'Product',
      width: 120,
      valueFormatter: (v: unknown) => (v as string | null) ?? 'All',
    },
    {
      field: 'itemCount',
      headerName: 'Items',
      width: 80,
      type: 'number',
    },
    {
      field: 'effectiveFrom',
      headerName: 'Effective From',
      width: 130,
    },
    {
      field: '__activate',
      headerName: '',
      width: 100,
      sortable: false,
      renderCell: (p: GridRenderCellParams<SopDocumentListDto>) => {
        if (p.row.status !== 'DRAFT') return null;
        return (
          <Button
            size="small"
            variant="outlined"
            color="success"
            onClick={() => activateMut.mutate({ id: Number(p.row.sopId) })}
            sx={{ fontSize: 11 }}
          >
            Activate
          </Button>
        );
      },
    },
  ];

  if (isLoading) return <TablePageSkeleton />;

  return (
    <PageRoot>
      <PageHeader
        title="SOP Documents"
        subtitle="Standard Operating Procedures & Digital Checksheets"
        breadcrumbs={[{ label: 'Lab' }, { label: 'SOP Documents' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => refetch()} />
            <Button
              variant="contained"
              size="small"
              startIcon={<SolarIcon name="add" size={16} />}
              onClick={() => setCreateOpen(true)}
            >
              New SOP
            </Button>
          </Stack>
        }
      />

      {!!error && <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(error as Error)}</Alert>}
      {!!createMut.error && (
        <Alert severity="error" sx={{ mb: 2 }}>{getErrorMessage(createMut.error as Error)}</Alert>
      )}

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search code or title…"
        filters={tableFilters}
      />

      {filtered.length === 0 ? (
        <EmptyState
          title="No SOP documents"
          description="Create an SOP to attach a digital checksheet to a routing step."
          action={
            <Button variant="contained" onClick={() => setCreateOpen(true)}>
              New SOP
            </Button>
          }
        />
      ) : (
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(r) => Number(r.sopId)}
          pageSizeOptions={[25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          disableRowSelectionOnClick
          onRowClick={(params) => setViewTarget(params.row as SopDocumentListDto)}
          sx={{ bgcolor: 'background.paper', borderRadius: 2, cursor: 'pointer' }}
        />
      )}

      {/* ── Detail Drawer ──────────────────────────────────────────────── */}
      <Drawer
        anchor="right"
        open={!!viewTarget}
        onClose={() => setViewTarget(null)}
        slotProps={{ paper: { sx: { width: { xs: '100%', sm: 480 }, p: 3 } } }}
      >
        {viewTarget && (
          <Stack spacing={2}>
            <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
              <Box>
                <Typography variant="h6" sx={{ fontWeight: 700 }}>{viewTarget.title}</Typography>
                <Typography variant="caption" color="text.secondary">{viewTarget.code} · v{viewTarget.version}</Typography>
              </Box>
              <IconButton onClick={() => setViewTarget(null)} size="small">
                <SolarIcon name="close" size={18} />
              </IconButton>
            </Stack>
            <Stack direction="row" spacing={1}>
              <Chip label={viewTarget.status} size="small"
                color={STATUS_COLORS[viewTarget.status] ?? 'default'} />
            </Stack>
            <Divider />
            <Stack spacing={0.75}>
              {viewTarget.productCode && <SopDetailRow label="Product" value={viewTarget.productCode} />}
              <SopDetailRow label="Routing Step" value={String(Number(viewTarget.routingStepId))} />
              <SopDetailRow label="Items" value={String(Number(viewTarget.itemCount))} />
              <SopDetailRow label="Effective" value={new Date(viewTarget.effectiveFrom).toLocaleDateString()} />
              {viewTarget.approvedBy && <SopDetailRow label="Approved By" value={viewTarget.approvedBy} />}
            </Stack>
            <Divider />
            <Typography variant="subtitle2" color="text.secondary">Attachments</Typography>
            <AttachmentList ownerType="sop" ownerId={String(Number(viewTarget.sopId))} canDelete />
            <FileUpload
              ownerType="sop"
              ownerId={String(Number(viewTarget.sopId))}
              onUploaded={(_r: FileUploadResult) => {
                qc.invalidateQueries({ queryKey: getGetApiV1FilesQueryKey({ ownerType: 'sop', ownerId: String(Number(viewTarget.sopId)) }) });
              }}
            />
          </Stack>
        )}
      </Drawer>

      {/* ── Create Drawer ──────────────────────────────────────────────── */}
      <Drawer
        anchor="right"
        open={createOpen}
        onClose={() => { setCreateOpen(false); reset(); }}
        slotProps={{ paper: { sx: { width: { xs: '100%', md: 600 }, p: 3, overflowY: 'auto' } } }}
      >
        <Typography variant="h6" sx={{ fontWeight: 700, mb: 3 }}>New SOP Document</Typography>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Stack spacing={2}>
            <Stack direction="row" spacing={1.5}>
              <Controller name="code" control={control}
                render={({ field }) => (
                  <TextField fullWidth label="Code *" size="small" {...field}
                    error={!!errors.code} helperText={errors.code?.message} />
                )} />
              <Controller name="version" control={control}
                render={({ field }) => (
                  <TextField sx={{ width: 100 }} label="Version *" size="small" {...field}
                    error={!!errors.version} />
                )} />
            </Stack>
            <Controller name="title" control={control}
              render={({ field }) => (
                <TextField fullWidth label="Title *" size="small" {...field}
                  error={!!errors.title} helperText={errors.title?.message} />
              )} />
            <Stack direction="row" spacing={1.5}>
              <Controller name="routingStepId" control={control}
                render={({ field }) => (
                  <TextField fullWidth label="Routing Step ID *" size="small" type="number"
                    {...field} onChange={(e) => field.onChange(parseInt(e.target.value, 10) || 0)}
                    error={!!errors.routingStepId} />
                )} />
              <Controller name="productCode" control={control}
                render={({ field }) => (
                  <TextField fullWidth label="Product Code" size="small" {...field} />
                )} />
            </Stack>
            <Stack direction="row" spacing={1.5}>
              <Controller name="effectiveFrom" control={control}
                render={({ field }) => (
                  <TextField fullWidth type="date" label="Effective From *" size="small" {...field}
                    slotProps={{ inputLabel: { shrink: true } }} error={!!errors.effectiveFrom} />
                )} />
            </Stack>
            <Controller name="notes" control={control}
              render={({ field }) => (
                <TextField fullWidth label="Notes" size="small" multiline rows={2} {...field} />
              )} />

            {/* ── Check Items ─────────────────────────────────────────── */}
            <Box sx={{ borderTop: '1px solid', borderColor: 'divider', pt: 2 }}>
              <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 1.5 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>Check Items</Typography>
                <Button size="small" onClick={() => append({
                  category: 'PROCESS', itemText: '', isRequired: true,
                  completionMode: 'MANUAL', photoRequired: false,
                })}>
                  + Add Item
                </Button>
              </Stack>
              {errors.items && typeof errors.items === 'object' && 'message' in errors.items && (
                <Typography variant="caption" color="error" sx={{ mb: 1, display: 'block' }}>
                  {String(errors.items.message)}
                </Typography>
              )}
              <Stack spacing={2}>
                {fields.map((field, idx) => (
                  <Box key={field.id} sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 1, p: 1.5 }}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 1 }}>
                      <Typography variant="caption" sx={{ fontWeight: 600, color: 'text.secondary' }}>
                        Item {idx + 1}
                      </Typography>
                      <Box onClick={() => remove(idx)} sx={{ cursor: 'pointer', color: 'error.main' }}>
                        <SolarIcon name="delete" size={16} />
                      </Box>
                    </Stack>
                    <Stack spacing={1.5}>
                      <Stack direction="row" spacing={1}>
                        <Controller name={`items.${idx}.category`} control={control}
                          render={({ field: f }) => (
                            <TextField select size="small" label="Category" sx={{ width: 140 }} {...f}>
                              {CATEGORIES.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
                            </TextField>
                          )} />
                        <Controller name={`items.${idx}.completionMode`} control={control}
                          render={({ field: f }) => (
                            <TextField select size="small" label="Mode" sx={{ flex: 1 }} {...f}>
                              {COMPLETION_MODES.map((m) => <MenuItem key={m} value={m}>{m}</MenuItem>)}
                            </TextField>
                          )} />
                      </Stack>
                      <Controller name={`items.${idx}.itemText`} control={control}
                        render={({ field: f }) => (
                          <TextField fullWidth size="small" label="Instruction *" multiline rows={2} {...f}
                            error={!!errors.items?.[idx]?.itemText} />
                        )} />
                    </Stack>
                  </Box>
                ))}
              </Stack>
            </Box>
          </Stack>

          <Stack direction="row" spacing={1.5} sx={{ mt: 3, justifyContent: 'flex-end' }}>
            <Button variant="outlined" onClick={() => { setCreateOpen(false); reset(); }}>
              Cancel
            </Button>
            <Button type="submit" variant="contained" loading={createMut.isPending}>
              Create SOP
            </Button>
          </Stack>
        </form>
      </Drawer>
    </PageRoot>
  );
}

function SopDetailRow({ label, value }: { label: string; value: string }) {
  return (
    <Stack direction="row" sx={{ justifyContent: 'space-between' }} spacing={2}>
      <Typography variant="body2" color="text.secondary" sx={{ flexShrink: 0 }}>{label}</Typography>
      <Typography variant="body2" sx={{ textAlign: 'right' }}>{value}</Typography>
    </Stack>
  );
}
