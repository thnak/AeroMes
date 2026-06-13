import {
  Box,
  Button,
  Chip,
  FormControlLabel,
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
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  ConfirmDialog,
  EmptyState,
  FormDrawer,
  PageHeader,
  PageRoot,
  SolarIcon,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1LabelsTemplates,
  postApiV1LabelsTemplates,
  putApiV1LabelsTemplatesId,
  deleteApiV1LabelsTemplatesId,
  getGetApiV1LabelsTemplatesQueryKey,
} from '../../api/labels/labels';
import { useQueryClient } from '@tanstack/react-query';

// ── Types ─────────────────────────────────────────────────────────────────────

interface LabelTemplate {
  id: string;
  name: string;
  paperSize: string;
  orientation: string;
  barcodeType: string;
  barcodeWidth: number;
  barcodeHeight: number;
  selectedFields: string[];
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
}

// ── Schema ────────────────────────────────────────────────────────────────────

const PAPER_SIZES = ['100x50mm', '100x70mm', 'A4', '80x40mm', '60x40mm'];
const BARCODE_TYPES = ['QRCode', 'Barcode1D'];
const ORIENTATIONS = ['Portrait', 'Landscape'];
const AVAILABLE_FIELDS = [
  'orderNumber', 'productCode', 'productName', 'quantity', 'lotNumber',
  'plannedDate', 'dueDate', 'workCenter', 'status', 'qrCode',
];

const schema = z.object({
  name: z.string().min(1, 'Name required'),
  paperSize: z.string(),
  orientation: z.string(),
  barcodeType: z.string(),
  barcodeWidth: z.number().int().min(50).max(500),
  barcodeHeight: z.number().int().min(50).max(500),
  selectedFields: z.string(),
  isDefault: z.boolean(),
});

type FormData = z.infer<typeof schema>;

const DEFAULT_FORM: FormData = {
  name: '',
  paperSize: '100x50mm',
  orientation: 'Portrait',
  barcodeType: 'QRCode',
  barcodeWidth: 150,
  barcodeHeight: 150,
  selectedFields: 'orderNumber,productName,quantity,qrCode',
  isDefault: false,
};

// ── Page ──────────────────────────────────────────────────────────────────────

export default function LabelTemplatesPage() {
  const qc = useQueryClient();
  const { data: rawData, isLoading, error } = useGetApiV1LabelsTemplates();
  const templates: LabelTemplate[] = (rawData as unknown as LabelTemplate[]) ?? [];


  const [search, setSearch] = useState('');
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerMode, setDrawerMode] = useState<'create' | 'edit'>('create');
  const [editTarget, setEditTarget] = useState<LabelTemplate | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<LabelTemplate | null>(null);
  const [saving, setSaving] = useState(false);

  const { control, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: DEFAULT_FORM,
  });

  const filtered = templates.filter((t) =>
    t.name.toLowerCase().includes(search.toLowerCase()),
  );

  function openCreate() {
    setDrawerMode('create');
    setEditTarget(null);
    reset(DEFAULT_FORM);
    setDrawerOpen(true);
  }

  function openEdit(t: LabelTemplate) {
    setDrawerMode('edit');
    setEditTarget(t);
    reset({
      name: t.name,
      paperSize: t.paperSize,
      orientation: t.orientation,
      barcodeType: t.barcodeType,
      barcodeWidth: t.barcodeWidth,
      barcodeHeight: t.barcodeHeight,
      selectedFields: t.selectedFields.join(','),
      isDefault: t.isDefault,
    });
    setDrawerOpen(true);
  }

  async function onSubmit(values: FormData) {
    setSaving(true);
    try {
      const body = {
        name: values.name,
        paperSize: values.paperSize,
        orientation: values.orientation,
        barcodeType: values.barcodeType,
        barcodeWidth: values.barcodeWidth,
        barcodeHeight: values.barcodeHeight,
        selectedFields: values.selectedFields.split(',').map((s) => s.trim()).filter(Boolean),
        isDefault: values.isDefault,
      };
      if (drawerMode === 'create') {
        await postApiV1LabelsTemplates(body);
      } else if (editTarget) {
        await putApiV1LabelsTemplatesId(editTarget.id, body);
      }
      await qc.invalidateQueries({ queryKey: getGetApiV1LabelsTemplatesQueryKey() });
      setDrawerOpen(false);
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    if (!deleteTarget) return;
    await deleteApiV1LabelsTemplatesId(deleteTarget.id);
    await qc.invalidateQueries({ queryKey: getGetApiV1LabelsTemplatesQueryKey() });
    setDeleteTarget(null);
  }

  const columns: GridColDef<LabelTemplate>[] = [
    {
      field: 'name', headerName: 'Name', flex: 1.5,
      renderCell: (params: GridRenderCellParams<LabelTemplate>) => (
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <Typography variant="body2" sx={{ fontWeight: 600 }}>{params.value}</Typography>
          {params.row.isDefault && <Chip label="Default" size="small" color="primary" sx={{ height: 18, fontSize: 10 }} />}
        </Stack>
      ),
    },
    { field: 'paperSize', headerName: 'Paper Size', width: 120 },
    { field: 'orientation', headerName: 'Orientation', width: 110 },
    { field: 'barcodeType', headerName: 'Barcode Type', width: 120 },
    {
      field: 'barcodeWidth', headerName: 'Size (px)', width: 110,
      renderCell: (params: GridRenderCellParams<LabelTemplate>) => (
        <Typography variant="caption">{params.row.barcodeWidth}×{params.row.barcodeHeight}</Typography>
      ),
    },
    {
      field: 'selectedFields', headerName: 'Fields', flex: 2,
      renderCell: (params: GridRenderCellParams<LabelTemplate>) => (
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.25 }}>
          {(params.value as string[]).slice(0, 4).map((f) => (
            <Chip key={f} label={f} size="small" sx={{ height: 16, fontSize: 9 }} />
          ))}
          {(params.value as string[]).length > 4 && (
            <Chip label={`+${(params.value as string[]).length - 4}`} size="small" sx={{ height: 16, fontSize: 9 }} />
          )}
        </Box>
      ),
    },
    {
      field: 'actions', headerName: '', width: 90, sortable: false, align: 'center',
      renderCell: (params: GridRenderCellParams<LabelTemplate>) => (
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
        title="Label Templates"
        subtitle="Configure QR / barcode label layouts for printing"
        actions={
          <Button variant="contained" startIcon={<SolarIcon name="add" size={18} />} onClick={openCreate}>
            New Template
          </Button>
        }
      />

      <TableToolbar
        searchPlaceholder="Search templates..."
        search={search}
        onSearchChange={setSearch}
      />

      <DataGrid
        rows={filtered}
        columns={columns}
        loading={isLoading}
        getRowId={(r) => r.id}
        autoHeight
        pageSizeOptions={[20, 50]}
        disableRowSelectionOnClick
        sx={{
          border: 'none',
          '& .MuiDataGrid-columnHeaders': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04) },
          '& .MuiDataGrid-cell:focus, & .MuiDataGrid-cell:focus-within': { outline: 'none' },
          '& .MuiDataGrid-columnHeader:focus, & .MuiDataGrid-columnHeader:focus-within': { outline: 'none' },
        }}
        slots={{
          noRowsOverlay: () => !isLoading && !error ? (
            <EmptyState
              title="No label templates"
              description="Create a template to start printing labels"
              action={<Button variant="contained" onClick={openCreate}>Create Template</Button>}
            />
          ) : null,
        }}
      />

      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'New Label Template' : 'Edit Label Template'}
        onSubmit={handleSubmit(onSubmit)}
        loading={saving}
      >
        <Stack spacing={2.5}>
          <Controller
            name="name"
            control={control}
            render={({ field }) => (
              <TextField {...field} label="Template Name" required fullWidth error={!!errors.name} helperText={errors.name?.message} />
            )}
          />

          <Stack direction="row" spacing={2}>
            <Controller
              name="paperSize"
              control={control}
              render={({ field }) => (
                <TextField {...field} select label="Paper Size" fullWidth>
                  {PAPER_SIZES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
                </TextField>
              )}
            />
            <Controller
              name="orientation"
              control={control}
              render={({ field }) => (
                <TextField {...field} select label="Orientation" fullWidth>
                  {ORIENTATIONS.map((o) => <MenuItem key={o} value={o}>{o}</MenuItem>)}
                </TextField>
              )}
            />
          </Stack>

          <Controller
            name="barcodeType"
            control={control}
            render={({ field }) => (
              <TextField {...field} select label="Barcode Type" fullWidth>
                {BARCODE_TYPES.map((bt) => <MenuItem key={bt} value={bt}>{bt}</MenuItem>)}
              </TextField>
            )}
          />

          <Stack direction="row" spacing={2}>
            <Controller
              name="barcodeWidth"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Width (px)"
                  type="number"
                  fullWidth
                  onChange={(e) => field.onChange(Number(e.target.value))}
                  error={!!errors.barcodeWidth}
                />
              )}
            />
            <Controller
              name="barcodeHeight"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  label="Height (px)"
                  type="number"
                  fullWidth
                  onChange={(e) => field.onChange(Number(e.target.value))}
                  error={!!errors.barcodeHeight}
                />
              )}
            />
          </Stack>

          <Controller
            name="selectedFields"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Data Fields (comma-separated)"
                fullWidth
                multiline
                minRows={2}
                helperText={`Available: ${AVAILABLE_FIELDS.join(', ')}`}
              />
            )}
          />

          <Controller
            name="isDefault"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={<Switch checked={field.value} onChange={(_, v) => field.onChange(v)} />}
                label="Set as default template"
              />
            )}
          />
        </Stack>
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Template"
        description={<>Delete template <strong>{deleteTarget?.name}</strong>?</>}
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
