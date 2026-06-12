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

// ─── Types ────────────────────────────────────────────────────────────────────

type DefectCategory = 'Dimensional' | 'Visual' | 'Functional' | 'Material' | 'Process';

interface DefectCode {
  id: string;
  code: string;
  name: string;
  category: DefectCategory;
  description?: string;
  isActive: boolean;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_DEFECT_CODES: DefectCode[] = [
  { id: '1',  code: 'DC-001', name: 'Scratch Surface',   category: 'Visual',      description: 'Visible surface scratch on finished area.',    isActive: true  },
  { id: '2',  code: 'DC-002', name: 'Dimension OOT',     category: 'Dimensional', description: 'Dimension out of tolerance per drawing spec.',  isActive: true  },
  { id: '3',  code: 'DC-003', name: 'Weld Porosity',     category: 'Process',     description: 'Gas pores detected in weld joint.',            isActive: true  },
  { id: '4',  code: 'DC-004', name: 'Burr Present',      category: 'Visual',      description: 'Sharp burr remaining on machined edge.',        isActive: true  },
  { id: '5',  code: 'DC-005', name: 'Wrong Material',    category: 'Material',    description: 'Incorrect material used for part.',             isActive: true  },
  { id: '6',  code: 'DC-006', name: 'Assembly Gap',      category: 'Functional',  description: 'Gap between mating parts exceeds specification.',isActive: true  },
  { id: '7',  code: 'DC-007', name: 'Delamination',      category: 'Material',    description: 'Layer separation in composite or coated part.', isActive: true  },
  { id: '8',  code: 'DC-008', name: 'Oxidation',         category: 'Visual',      description: 'Surface oxidation or rust on metal part.',      isActive: true  },
  { id: '9',  code: 'DC-009', name: 'Misalignment',      category: 'Dimensional', description: 'Part misaligned during assembly operation.',    isActive: true  },
  { id: '10', code: 'DC-010', name: 'Incomplete Paint',  category: 'Visual',      description: 'Paint coverage insufficient per spec.',         isActive: true  },
  { id: '11', code: 'DC-011', name: 'Crack',             category: 'Material',    description: 'Structural crack detected in part.',            isActive: true  },
  { id: '12', code: 'DC-012', name: 'Thread Damage',     category: 'Functional',  description: 'Thread stripped, cross-threaded, or damaged.',  isActive: false },
];

const CATEGORIES: DefectCategory[] = ['Dimensional', 'Visual', 'Functional', 'Material', 'Process'];

const CATEGORY_COLORS: Record<DefectCategory, string> = {
  Dimensional: '#1D4ED8',
  Visual:      '#D97706',
  Functional:  '#DC2626',
  Material:    '#7C3AED',
  Process:     '#0D9488',
};

// ─── Form schema ──────────────────────────────────────────────────────────────

const DefectCodeSchema = z.object({
  code:        z.string().min(1, 'Code is required').max(20),
  name:        z.string().min(1, 'Name is required').max(200),
  category:    z.enum(['Dimensional', 'Visual', 'Functional', 'Material', 'Process']),
  description: z.string().optional(),
  isActive:    z.boolean(),
});

type DefectCodeFormValues = z.infer<typeof DefectCodeSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function DefectCodeForm({
  defaultValues,
  onSubmit,
  loading: _loading,
}: {
  defaultValues: Partial<DefectCodeFormValues>;
  onSubmit: (data: DefectCodeFormValues) => void;
  loading: boolean;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<DefectCodeFormValues>({
    resolver: zodResolver(DefectCodeSchema),
    defaultValues: { isActive: true, ...defaultValues },
  });

  return (
    <Box component="form" id="defect-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Defect Code"
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
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Defect Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="isActive"
            control={control}
            render={({ field }) => (
              <FormControlLabel
                control={<Switch checked={field.value} onChange={field.onChange} color="primary" />}
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
            placeholder="Optional description of this defect type…"
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function DefectCodesPage() {
  const [rows, setRows]                     = useState<DefectCode[]>(MOCK_DEFECT_CODES);
  const [search, setSearch]                 = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter]     = useState('');
  const [selection, setSelection]           = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]         = useState(false);
  const [drawerMode, setDrawerMode]         = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]         = useState<DefectCode | null>(null);
  const [deleteTarget, setDeleteTarget]     = useState<DefectCode | null>(null);
  const [saving, setSaving]                 = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)         r = r.filter((d) => d.name.toLowerCase().includes(search.toLowerCase()) || d.code.toLowerCase().includes(search.toLowerCase()));
    if (categoryFilter) r = r.filter((d) => d.category === categoryFilter);
    if (statusFilter)   r = r.filter((d) => statusFilter === 'active' ? d.isActive : !d.isActive);
    return r;
  }, [rows, search, categoryFilter, statusFilter]);

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(d: DefectCode) { setDrawerMode('edit'); setEditTarget(d); setDrawerOpen(true); }

  function handleSave(data: DefectCodeFormValues) {
    setSaving(true);
    setTimeout(() => {
      if (drawerMode === 'create') {
        setRows((prev) => [...prev, { id: String(Date.now()), ...data }]);
      } else if (editTarget) {
        setRows((prev) => prev.map((d) => d.id === editTarget.id ? { ...d, ...data } : d));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((d) => d.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<DefectCode>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<DefectCode>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'name', headerName: 'Name', width: 200 },
    {
      field: 'category',
      headerName: 'Category',
      width: 130,
      renderCell: (params: GridRenderCellParams<DefectCode>) => {
        const color = CATEGORY_COLORS[params.value as DefectCategory] ?? '#64748B';
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{
              height: 20,
              fontSize: '0.6875rem',
              fontWeight: 600,
              bgcolor: alpha(color, 0.1),
              color,
              border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'description',
      headerName: 'Description',
      flex: 1,
      minWidth: 200,
      renderCell: (params: GridRenderCellParams<DefectCode>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
          {params.value ?? '—'}
        </Typography>
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<DefectCode>) => (
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
      width: 80,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<DefectCode>) => (
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
        title="Defect Codes"
        subtitle="Manage the defect code library used in quality inspections and non-conformance reports"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Defect Codes' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Defect Code
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
            options: [{ label: 'Active', value: 'active' }, { label: 'Inactive', value: 'inactive' }],
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
                title={search || categoryFilter || statusFilter ? 'No defect codes match your filters' : 'No defect codes yet'}
                description={search || categoryFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first defect code to get started.'}
                action={!search && !categoryFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Defect Code</Button>
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
        title={drawerMode === 'create' ? 'New Defect Code' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter defect code details below' : editTarget?.name}
        onSubmit={() => document.getElementById('defect-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Defect Code' : 'Save Changes'}
        loading={saving}
      >
        <DefectCodeForm
          key={editTarget?.id ?? 'new'}
          defaultValues={editTarget ?? {}}
          onSubmit={handleSave}
          loading={saving}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Defect Code"
        description={
          <>
            Delete <strong>{deleteTarget?.name}</strong> ({deleteTarget?.code})?
            This cannot be undone and may affect quality records that reference this code.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
