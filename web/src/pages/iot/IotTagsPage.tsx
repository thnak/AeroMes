import {
  Alert,
  Box,
  Chip,
  IconButton,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState } from 'react';
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
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1IotTags,
  getGetApiV1IotTagsQueryKey,
  postApiV1IotTags,
  putApiV1IotTagsKey,
  deleteApiV1IotTagsKey,
} from '../../api/iot/iot';
import type { SignalTagDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const CATEGORIES = ['MOTION', 'ELECTRICAL', 'THERMAL', 'COUNTER', 'STATUS', 'CUSTOM'];
const DATA_TYPES  = ['FLOAT', 'INT', 'BOOL', 'STRING'];

const CATEGORY_COLORS: Record<string, string> = {
  MOTION:     '#0891B2',
  ELECTRICAL: '#D97706',
  THERMAL:    '#DC2626',
  COUNTER:    '#7C3AED',
  STATUS:     '#15803D',
  CUSTOM:     '#64748B',
};

const DATATYPE_COLORS: Record<string, string> = {
  FLOAT:  '#4F46E5',
  INT:    '#0891B2',
  BOOL:   '#15803D',
  STRING: '#64748B',
};

const TagSchema = z.object({
  key:          z.string().min(1, 'Required').max(100).regex(/^[a-z0-9_]+$/, 'Only lowercase letters, digits, underscores'),
  displayName:  z.string().min(1, 'Required').max(200),
  category:     z.enum(['MOTION', 'ELECTRICAL', 'THERMAL', 'COUNTER', 'STATUS', 'CUSTOM']),
  dataType:     z.enum(['FLOAT', 'INT', 'BOOL', 'STRING']),
  defaultUnit:  z.string().max(30).optional(),
  typicalMin:   z.coerce.number().optional(),
  typicalMax:   z.coerce.number().optional(),
  description:  z.string().max(500).optional(),
});
type TagForm = z.infer<typeof TagSchema>;

export default function IotTagsPage() {
  const queryClient = useQueryClient();

  const [search, setSearch]               = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [createOpen, setCreateOpen]       = useState(false);
  const [editTarget, setEditTarget]       = useState<SignalTagDto | null>(null);
  const [deleteTarget, setDeleteTarget]   = useState<SignalTagDto | null>(null);
  const [toastError, setToastError]       = useState<string | null>(null);

  const queryParams = categoryFilter ? { category: categoryFilter } : undefined;
  const { data: tags = [], isLoading, error, refetch } = useGetApiV1IotTags(queryParams);

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1IotTagsQueryKey(queryParams) });

  const filtered = search
    ? tags.filter((t) =>
        t.key.toLowerCase().includes(search.toLowerCase()) ||
        t.displayName.toLowerCase().includes(search.toLowerCase()) ||
        (t.defaultUnit ?? '').toLowerCase().includes(search.toLowerCase()))
    : tags;

  const form = useForm<TagForm>({
    resolver: zodResolver(TagSchema) as any,
    defaultValues: { category: 'CUSTOM', dataType: 'FLOAT' },
  });

  const createMutation = useMutation({
    mutationFn: (v: TagForm) =>
      postApiV1IotTags({
        key: v.key, displayName: v.displayName, category: v.category,
        dataType: v.dataType, defaultUnit: v.defaultUnit ?? null,
        typicalMin: v.typicalMin ?? null, typicalMax: v.typicalMax ?? null,
        description: v.description ?? null,
      }),
    onSuccess: () => { invalidate(); setCreateOpen(false); form.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const updateMutation = useMutation({
    mutationFn: ({ key, v }: { key: string; v: TagForm }) =>
      putApiV1IotTagsKey(key, {
        displayName: v.displayName, category: v.category,
        dataType: v.dataType, defaultUnit: v.defaultUnit ?? null,
        typicalMin: v.typicalMin ?? null, typicalMax: v.typicalMax ?? null,
        description: v.description ?? null,
      }),
    onSuccess: () => { invalidate(); setEditTarget(null); form.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const deleteMutation = useMutation({
    mutationFn: (key: string) => deleteApiV1IotTagsKey(key),
    onSuccess:  () => { invalidate(); setDeleteTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  function openEdit(tag: SignalTagDto) {
    form.reset({
      key: tag.key, displayName: tag.displayName,
      category: tag.category as TagForm['category'],
      dataType: tag.dataType as TagForm['dataType'],
      defaultUnit: tag.defaultUnit ?? undefined,
      typicalMin: tag.typicalMin != null ? Number(tag.typicalMin) : undefined,
      typicalMax: tag.typicalMax != null ? Number(tag.typicalMax) : undefined,
      description: tag.description ?? undefined,
    });
    setEditTarget(tag);
  }

  const columns: GridColDef<SignalTagDto>[] = [
    {
      field: 'key',
      headerName: 'Tag Key',
      width: 180,
      renderCell: (p: GridRenderCellParams<SignalTagDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600, color: 'primary.main' }}>
          {p.value as string}
        </Typography>
      ),
    },
    { field: 'displayName', headerName: 'Display Name', flex: 1, minWidth: 160 },
    {
      field: 'category',
      headerName: 'Category',
      width: 110,
      renderCell: (p: GridRenderCellParams<SignalTagDto>) => {
        const color = CATEGORY_COLORS[p.value as string] ?? '#64748B';
        return (
          <Chip
            label={p.value as string}
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
      field: 'dataType',
      headerName: 'Type',
      width: 80,
      renderCell: (p: GridRenderCellParams<SignalTagDto>) => {
        const color = DATATYPE_COLORS[p.value as string] ?? '#64748B';
        return (
          <Chip
            label={p.value as string}
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
      field: 'defaultUnit',
      headerName: 'Unit',
      width: 80,
      renderCell: (p: GridRenderCellParams<SignalTagDto>) =>
        p.value ? (
          <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.8125rem' }}>
            {p.value as string}
          </Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">—</Typography>
        ),
    },
    {
      field: 'typicalMin',
      headerName: 'Min',
      width: 80,
      renderCell: (p: GridRenderCellParams<SignalTagDto>) =>
        p.value != null ? (
          <Typography variant="body2" sx={{ fontSize: '0.8125rem' }}>{Number(p.value)}</Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">—</Typography>
        ),
    },
    {
      field: 'typicalMax',
      headerName: 'Max',
      width: 80,
      renderCell: (p: GridRenderCellParams<SignalTagDto>) =>
        p.value != null ? (
          <Typography variant="body2" sx={{ fontSize: '0.8125rem' }}>{Number(p.value)}</Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">—</Typography>
        ),
    },
    {
      field: 'isSystemDefined',
      headerName: 'System',
      width: 80,
      renderCell: (p: GridRenderCellParams<SignalTagDto>) =>
        p.value ? (
          <Chip
            label="System"
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha('#4F46E5', 0.1), color: '#4F46E5', border: 'none',
              '& .MuiChip-label': { px: 0.75 } }}
          />
        ) : null,
    },
    {
      field: 'actions',
      headerName: '',
      width: 90,
      sortable: false,
      align: 'center',
      renderCell: (p: GridRenderCellParams<SignalTagDto>) => (
        <Stack direction="row" spacing={0.25}>
          {!p.row.isSystemDefined && (
            <>
              <Tooltip title="Edit">
                <IconButton size="small" onClick={() => openEdit(p.row)} sx={{ color: '#4F46E5' }}>
                  <SolarIcon name="edit" size={15} />
                </IconButton>
              </Tooltip>
              <Tooltip title="Delete">
                <IconButton size="small" onClick={() => setDeleteTarget(p.row)} sx={{ color: '#DC2626' }}>
                  <SolarIcon name="delete" size={15} />
                </IconButton>
              </Tooltip>
            </>
          )}
        </Stack>
      ),
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Signal Tag Catalog" breadcrumbs={[{ label: 'IoT' }, { label: 'Tags' }]} />
      <EmptyState icon="emptyTable" title="Failed to load tags" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Signal Tag Catalog"
        subtitle="Shared vocabulary of well-known signal types across all machines"
        breadcrumbs={[{ label: 'IoT' }, { label: 'Tags' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => refetch()} />
            <IconButton
              size="small"
              onClick={() => { form.reset({ category: 'CUSTOM', dataType: 'FLOAT' }); setCreateOpen(true); }}
              sx={{ bgcolor: 'primary.main', color: 'white', borderRadius: 1, '&:hover': { bgcolor: 'primary.dark' } }}
            >
              <SolarIcon name="add" size={16} />
            </IconButton>
          </Stack>
        }
      />

      {toastError && (
        <Alert severity="error" onClose={() => setToastError(null)} sx={{ mb: 2 }}>
          {toastError}
        </Alert>
      )}

      <Stack direction="row" spacing={1.5} sx={{ mb: 1.5, alignItems: 'center' }}>
        <Box sx={{ flex: 1 }}>
          <TableToolbar
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Filter by key, display name or unit…"
          />
        </Box>
        <Select
          value={categoryFilter}
          onChange={(e) => setCategoryFilter(e.target.value)}
          size="small"
          displayEmpty
          sx={{ minWidth: 140, flexShrink: 0 }}
        >
          <MenuItem value="">All Categories</MenuItem>
          {CATEGORIES.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
        </Select>
      </Stack>

      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(row) => row.key}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon="emptyTable"
                title="No signal tags"
                description="System-defined tags are seeded on first run. Add custom tags with the + button."
              />
            ),
          }}
          sx={{ border: 'none', '& .MuiDataGrid-cell': { alignItems: 'center' } }}
        />
      </Box>

      {/* ── Create / Edit drawer ── */}
      <FormDrawer
        open={createOpen || !!editTarget}
        onClose={() => { setCreateOpen(false); setEditTarget(null); form.reset(); }}
        title={editTarget ? `Edit Tag — ${editTarget.key}` : 'New Signal Tag'}
        onSubmit={() => void form.handleSubmit((v) => {
          if (editTarget) updateMutation.mutate({ key: editTarget.key, v });
          else createMutation.mutate(v);
        })()}
        loading={createMutation.isPending || updateMutation.isPending}
      >
        <Controller name="key" control={form.control} render={({ field, fieldState }) => (
          <TextField
            {...field}
            label="Tag Key"
            fullWidth
            disabled={!!editTarget}
            placeholder="e.g. spindle_rpm"
            error={!!fieldState.error}
            helperText={fieldState.error?.message ?? 'Unique identifier — lowercase letters, digits, underscores only'}
            slotProps={{ htmlInput: { style: { fontFamily: 'monospace' } } }}
          />
        )} />
        <Controller name="displayName" control={form.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Display Name" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Stack direction="row" spacing={2}>
          <Controller name="category" control={form.control} render={({ field }) => (
            <TextField {...field} label="Category" select fullWidth>
              {CATEGORIES.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
            </TextField>
          )} />
          <Controller name="dataType" control={form.control} render={({ field }) => (
            <TextField {...field} label="Data Type" select fullWidth>
              {DATA_TYPES.map((d) => <MenuItem key={d} value={d}>{d}</MenuItem>)}
            </TextField>
          )} />
        </Stack>
        <Controller name="defaultUnit" control={form.control} render={({ field }) => (
          <TextField {...field} label="Default Unit (optional)" fullWidth placeholder="e.g. rpm, °C, kW" />
        )} />
        <Stack direction="row" spacing={2}>
          <Controller name="typicalMin" control={form.control} render={({ field }) => (
            <TextField {...field} label="Typical Min" type="number" fullWidth />
          )} />
          <Controller name="typicalMax" control={form.control} render={({ field }) => (
            <TextField {...field} label="Typical Max" type="number" fullWidth />
          )} />
        </Stack>
        <Controller name="description" control={form.control} render={({ field }) => (
          <TextField {...field} label="Description (optional)" fullWidth multiline rows={2} />
        )} />
      </FormDrawer>

      {/* ── Delete confirm ── */}
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Signal Tag"
        description={
          <>Delete tag <strong>{deleteTarget?.key}</strong>?
          This will fail if any machine signal uses this key.</>
        }
        onClose={() => setDeleteTarget(null)}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.key)}
        loading={deleteMutation.isPending}
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
