import {
  Box,
  Button,
  Chip,
  FormControlLabel,
  Grid,
  IconButton,
  LinearProgress,
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

type StorageType = 'Rack' | 'Floor' | 'Bin' | 'Tank' | 'Refrigerated';

interface StorageLocation {
  id: string;
  code: string;
  name: string;
  zone: string;
  type: StorageType;
  maxCapacity: number;
  currentStock: number;
  uom: string;
  isActive: boolean;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_LOCATIONS: StorageLocation[] = [
  { id: '1',  code: 'WH-001', name: 'Raw Materials Rack 1', zone: 'RM',   type: 'Rack',         maxCapacity: 100,  currentStock: 72,  uom: 'EA',  isActive: true  },
  { id: '2',  code: 'WH-002', name: 'Raw Materials Rack 2', zone: 'RM',   type: 'Rack',         maxCapacity: 100,  currentStock: 45,  uom: 'EA',  isActive: true  },
  { id: '3',  code: 'WH-003', name: 'WIP Station A',        zone: 'WIP',  type: 'Floor',        maxCapacity: 50,   currentStock: 28,  uom: 'EA',  isActive: true  },
  { id: '4',  code: 'WH-004', name: 'WIP Station B',        zone: 'WIP',  type: 'Floor',        maxCapacity: 50,   currentStock: 31,  uom: 'EA',  isActive: true  },
  { id: '5',  code: 'WH-005', name: 'FG Rack A',            zone: 'FG',   type: 'Rack',         maxCapacity: 200,  currentStock: 156, uom: 'EA',  isActive: true  },
  { id: '6',  code: 'WH-006', name: 'FG Rack B',            zone: 'FG',   type: 'Rack',         maxCapacity: 200,  currentStock: 89,  uom: 'EA',  isActive: true  },
  { id: '7',  code: 'WH-007', name: 'Inspection Hold Area', zone: 'QC',   type: 'Floor',        maxCapacity: 30,   currentStock: 7,   uom: 'EA',  isActive: true  },
  { id: '8',  code: 'WH-008', name: 'Bulk Material Tank',   zone: 'RM',   type: 'Tank',         maxCapacity: 1000, currentStock: 620, uom: 'KG',  isActive: true  },
  { id: '9',  code: 'WH-009', name: 'Cold Storage',         zone: 'RM',   type: 'Refrigerated', maxCapacity: 200,  currentStock: 80,  uom: 'EA',  isActive: true  },
  { id: '10', code: 'WH-010', name: 'Overflow Floor',       zone: 'MISC', type: 'Floor',        maxCapacity: 500,  currentStock: 12,  uom: 'EA',  isActive: false },
];

const ZONES   = [...new Set(MOCK_LOCATIONS.map((l) => l.zone))].sort();
const TYPES: StorageType[] = ['Rack', 'Floor', 'Bin', 'Tank', 'Refrigerated'];

const ZONE_COLORS: Record<string, string> = {
  RM:   '#1D4ED8',
  WIP:  '#D97706',
  FG:   '#15803D',
  QC:   '#7C3AED',
  MISC: '#64748B',
};

const TYPE_COLORS: Record<StorageType, string> = {
  Rack:         '#0D9488',
  Floor:        '#64748B',
  Bin:          '#1D4ED8',
  Tank:         '#B45309',
  Refrigerated: '#0EA5E9',
};

function capacityColor(pct: number): string {
  if (pct >= 0.8) return '#DC2626';
  if (pct >= 0.5) return '#D97706';
  return '#15803D';
}

// ─── Form schema ──────────────────────────────────────────────────────────────

const StorageSchema = z.object({
  code:        z.string().min(1, 'Code is required').max(20),
  name:        z.string().min(1, 'Name is required').max(200),
  zone:        z.string().min(1, 'Zone is required'),
  type:        z.enum(['Rack', 'Floor', 'Bin', 'Tank', 'Refrigerated']),
  maxCapacity: z.number().min(1),
  uom:         z.string().min(1, 'UOM is required'),
  isActive:    z.boolean(),
});

type StorageFormValues = z.infer<typeof StorageSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function StorageForm({
  defaultValues,
  onSubmit,
  loading: _loading,
}: {
  defaultValues: Partial<StorageFormValues>;
  onSubmit: (data: StorageFormValues) => void;
  loading: boolean;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<StorageFormValues>({
    resolver: zodResolver(StorageSchema),
    defaultValues: { isActive: true, uom: 'EA', ...defaultValues },
  });

  return (
    <Box component="form" id="storage-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Location Code"
            fullWidth
            required
            error={!!errors.code}
            helperText={errors.code?.message}
            slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <Controller
            name="type"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Type"
                fullWidth
                required
                error={!!errors.type}
                helperText={errors.type?.message}
              >
                {TYPES.map((t) => (
                  <MenuItem key={t} value={t}>{t}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Location Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('zone')}
            label="Zone"
            fullWidth
            required
            placeholder="e.g. RM, WIP, FG"
            error={!!errors.zone}
            helperText={errors.zone?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <TextField
            {...register('maxCapacity', { valueAsNumber: true })}
            label="Max Capacity"
            fullWidth
            required
            type="number"
            error={!!errors.maxCapacity}
            helperText={errors.maxCapacity?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 3 }}>
          <Controller
            name="uom"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="UOM"
                fullWidth
                required
                error={!!errors.uom}
                helperText={errors.uom?.message}
              >
                {['EA', 'SET', 'KG', 'L', 'M3'].map((u) => (
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
                control={<Switch checked={field.value} onChange={field.onChange} color="primary" />}
                label="Active"
                sx={{ mt: 0.5, ml: 0 }}
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

export default function StorageLocationsPage() {
  const [rows, setRows]                 = useState<StorageLocation[]>(MOCK_LOCATIONS);
  const [search, setSearch]             = useState('');
  const [zoneFilter, setZoneFilter]     = useState('');
  const [typeFilter, setTypeFilter]     = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<StorageLocation | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<StorageLocation | null>(null);
  const [saving, setSaving]             = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)       r = r.filter((l) => l.name.toLowerCase().includes(search.toLowerCase()) || l.code.toLowerCase().includes(search.toLowerCase()));
    if (zoneFilter)   r = r.filter((l) => l.zone === zoneFilter);
    if (typeFilter)   r = r.filter((l) => l.type === typeFilter);
    if (statusFilter) r = r.filter((l) => statusFilter === 'active' ? l.isActive : !l.isActive);
    return r;
  }, [rows, search, zoneFilter, typeFilter, statusFilter]);

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(l: StorageLocation) { setDrawerMode('edit'); setEditTarget(l); setDrawerOpen(true); }

  function handleSave(data: StorageFormValues) {
    setSaving(true);
    setTimeout(() => {
      if (drawerMode === 'create') {
        setRows((prev) => [...prev, { id: String(Date.now()), currentStock: 0, ...data }]);
      } else if (editTarget) {
        setRows((prev) => prev.map((l) => l.id === editTarget.id ? { ...l, ...data } : l));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((l) => l.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<StorageLocation>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 110,
      renderCell: (params: GridRenderCellParams<StorageLocation>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 180 },
    {
      field: 'zone',
      headerName: 'Zone',
      width: 80,
      renderCell: (params: GridRenderCellParams<StorageLocation>) => {
        const color = ZONE_COLORS[params.value as string] ?? '#64748B';
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{
              height: 20,
              fontSize: '0.6875rem',
              fontWeight: 700,
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
      field: 'type',
      headerName: 'Type',
      width: 110,
      renderCell: (params: GridRenderCellParams<StorageLocation>) => {
        const color = TYPE_COLORS[params.value as StorageType] ?? '#64748B';
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
      field: 'currentStock',
      headerName: 'Capacity',
      width: 180,
      renderCell: (params: GridRenderCellParams<StorageLocation>) => {
        const pct = params.row.maxCapacity > 0 ? params.row.currentStock / params.row.maxCapacity : 0;
        const color = capacityColor(pct);
        return (
          <Box sx={{ width: '100%', pr: 1 }}>
            <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.25 }}>
              <Typography variant="caption" sx={{ color, fontWeight: 600, fontSize: 10 }}>
                {params.row.currentStock} / {params.row.maxCapacity} {params.row.uom}
              </Typography>
              <Typography variant="caption" sx={{ color, fontSize: 10 }}>
                {Math.round(pct * 100)}%
              </Typography>
            </Stack>
            <LinearProgress
              variant="determinate"
              value={pct * 100}
              sx={{
                height: 4,
                borderRadius: 2,
                bgcolor: alpha(color, 0.12),
                '& .MuiLinearProgress-bar': { bgcolor: color, borderRadius: 2 },
              }}
            />
          </Box>
        );
      },
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<StorageLocation>) => (
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
      renderCell: (params: GridRenderCellParams<StorageLocation>) => (
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
        title="Storage Locations"
        subtitle="Manage warehouse and production storage locations, zones, and capacity"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Storage' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              New Location
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
            label: 'Zone',
            value: zoneFilter,
            options: ZONES.map((z) => ({ label: z, value: z })),
            onChange: setZoneFilter,
          },
          {
            label: 'Type',
            value: typeFilter,
            options: TYPES.map((t) => ({ label: t, value: t })),
            onChange: setTypeFilter,
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
                icon={search || zoneFilter || typeFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || zoneFilter || typeFilter || statusFilter ? 'No locations match your filters' : 'No storage locations yet'}
                description={search || zoneFilter || typeFilter || statusFilter ? 'Try adjusting your search or filters.' : 'Add your first location to get started.'}
                action={!search && !zoneFilter && !typeFilter && !statusFilter ? (
                  <Button variant="contained" size="small" onClick={openCreate}>Add Location</Button>
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
        title={drawerMode === 'create' ? 'New Location' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter location details below' : editTarget?.name}
        onSubmit={() => document.getElementById('storage-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Create Location' : 'Save Changes'}
        loading={saving}
      >
        <StorageForm
          key={editTarget?.id ?? 'new'}
          defaultValues={editTarget ? {
            code:        editTarget.code,
            name:        editTarget.name,
            zone:        editTarget.zone,
            type:        editTarget.type,
            maxCapacity: editTarget.maxCapacity,
            uom:         editTarget.uom,
            isActive:    editTarget.isActive,
          } : {}}
          onSubmit={handleSave}
          loading={saving}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Location"
        description={
          <>
            Delete <strong>{deleteTarget?.name}</strong> ({deleteTarget?.code})?
            This cannot be undone and will affect inventory assignments.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
