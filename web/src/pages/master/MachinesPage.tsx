import {
  Box,
  Button,
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
  StatusDot,
  TableToolbar,
} from '../../components';
import { machineColors, oeeZoneColor } from '../../theme/tokens';

// ─── Types ────────────────────────────────────────────────────────────────────

type MachineStatus = 'RUNNING' | 'IDLE' | 'SETUP' | 'DOWN' | 'OFFLINE';

interface Machine {
  id: string;
  code: string;
  name: string;
  type: string;
  workCenterCode: string;
  workCenterName: string;
  status: MachineStatus;
  oee: number;
  lastMaintenanceDate: string;
  isActive: boolean;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_MACHINES: Machine[] = [
  { id: '1',  code: 'MC-01', name: 'CNC Lathe 1',    type: 'Lathe',      workCenterCode: 'WC-001', workCenterName: 'CNC Machining Bay',  status: 'RUNNING', oee: 88, lastMaintenanceDate: '2026-05-15', isActive: true  },
  { id: '2',  code: 'MC-02', name: 'CNC Lathe 2',    type: 'Lathe',      workCenterCode: 'WC-001', workCenterName: 'CNC Machining Bay',  status: 'IDLE',    oee: 72, lastMaintenanceDate: '2026-04-20', isActive: true  },
  { id: '3',  code: 'MC-03', name: 'CNC Mill 1',     type: 'Mill',       workCenterCode: 'WC-007', workCenterName: 'CNC Milling Center', status: 'RUNNING', oee: 91, lastMaintenanceDate: '2026-05-01', isActive: true  },
  { id: '4',  code: 'MC-04', name: 'CNC Mill 2',     type: 'Mill',       workCenterCode: 'WC-007', workCenterName: 'CNC Milling Center', status: 'DOWN',    oee: 0,  lastMaintenanceDate: '2026-03-10', isActive: true  },
  { id: '5',  code: 'MC-05', name: 'Welding R1',     type: 'Welding',    workCenterCode: 'WC-003', workCenterName: 'MIG Welding Bay',    status: 'RUNNING', oee: 79, lastMaintenanceDate: '2026-05-20', isActive: true  },
  { id: '6',  code: 'MC-06', name: 'Welding R2',     type: 'Welding',    workCenterCode: 'WC-003', workCenterName: 'MIG Welding Bay',    status: 'SETUP',   oee: 0,  lastMaintenanceDate: '2026-05-18', isActive: true  },
  { id: '7',  code: 'MC-07', name: 'Press 100T',     type: 'Press',      workCenterCode: 'WC-010', workCenterName: 'Press Shop',         status: 'RUNNING', oee: 84, lastMaintenanceDate: '2026-04-25', isActive: true  },
  { id: '8',  code: 'MC-08', name: 'Press 200T',     type: 'Press',      workCenterCode: 'WC-010', workCenterName: 'Press Shop',         status: 'IDLE',    oee: 68, lastMaintenanceDate: '2026-03-15', isActive: true  },
  { id: '9',  code: 'MC-09', name: 'Assembly St.1',  type: 'Assembly',   workCenterCode: 'WC-002', workCenterName: 'Assembly Station 1', status: 'RUNNING', oee: 77, lastMaintenanceDate: '2026-05-10', isActive: true  },
  { id: '10', code: 'MC-10', name: 'Assembly St.2',  type: 'Assembly',   workCenterCode: 'WC-002', workCenterName: 'Assembly Station 1', status: 'RUNNING', oee: 82, lastMaintenanceDate: '2026-05-10', isActive: true  },
  { id: '11', code: 'MC-11', name: 'Paint Booth 1',  type: 'Painting',   workCenterCode: 'WC-005', workCenterName: 'Paint Booth Line',   status: 'OFFLINE', oee: 0,  lastMaintenanceDate: '2026-02-01', isActive: false },
  { id: '12', code: 'MC-12', name: 'Insp. Station',  type: 'Inspection', workCenterCode: 'WC-004', workCenterName: 'Final Inspection',   status: 'IDLE',    oee: 0,  lastMaintenanceDate: '2026-05-25', isActive: true  },
];

const STATUS_LABELS: Record<MachineStatus, string> = {
  RUNNING: 'Running',
  IDLE:    'Idle',
  SETUP:   'Setup',
  DOWN:    'Down',
  OFFLINE: 'Offline',
};

const MACHINE_TYPES = ['Lathe', 'Mill', 'Press', 'Welding', 'Assembly', 'Painting', 'Inspection'];

const WORK_CENTERS = [
  { code: 'WC-001', name: 'CNC Machining Bay' },
  { code: 'WC-002', name: 'Assembly Station 1' },
  { code: 'WC-003', name: 'MIG Welding Bay' },
  { code: 'WC-004', name: 'Final Inspection' },
  { code: 'WC-005', name: 'Paint Booth Line' },
  { code: 'WC-007', name: 'CNC Milling Center' },
  { code: 'WC-010', name: 'Press Shop' },
];

// ─── Form schema ──────────────────────────────────────────────────────────────

const MachineSchema = z.object({
  code:                z.string().min(1, 'Code is required'),
  name:                z.string().min(1, 'Name is required'),
  type:                z.string().min(1, 'Type is required'),
  workCenterCode:      z.string().min(1, 'Work center is required'),
  lastMaintenanceDate: z.string().min(1, 'Maintenance date is required'),
  isActive:            z.boolean(),
});

type MachineFormValues = z.infer<typeof MachineSchema>;

// ─── Form component ───────────────────────────────────────────────────────────

function MachineForm({
  defaultValues,
  onSubmit,
  loading: _loading,
}: {
  defaultValues: Partial<MachineFormValues>;
  onSubmit: (data: MachineFormValues) => void;
  loading: boolean;
}) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<MachineFormValues>({
    resolver: zodResolver(MachineSchema),
    defaultValues: { isActive: true, ...defaultValues },
  });

  return (
    <Box component="form" id="machine-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('code')}
            label="Machine Code"
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
                {MACHINE_TYPES.map((t) => (
                  <MenuItem key={t} value={t}>{t}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            {...register('name')}
            label="Machine Name"
            fullWidth
            required
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="workCenterCode"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Work Center"
                fullWidth
                required
                error={!!errors.workCenterCode}
                helperText={errors.workCenterCode?.message}
              >
                {WORK_CENTERS.map((wc) => (
                  <MenuItem key={wc.code} value={wc.code}>{wc.code} — {wc.name}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('lastMaintenanceDate')}
            label="Last Maintenance Date"
            fullWidth
            required
            type="date"
            error={!!errors.lastMaintenanceDate}
            helperText={errors.lastMaintenanceDate?.message}
            slotProps={{ inputLabel: { shrink: true } }}
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

export default function MachinesPage() {
  const [rows, setRows]                 = useState<Machine[]>(MOCK_MACHINES);
  const [search, setSearch]             = useState('');
  const [wcFilter, setWcFilter]         = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter]     = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<Machine | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Machine | null>(null);
  const [saving, setSaving]             = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)       r = r.filter((m) => m.name.toLowerCase().includes(search.toLowerCase()) || m.code.toLowerCase().includes(search.toLowerCase()));
    if (wcFilter)     r = r.filter((m) => m.workCenterCode === wcFilter);
    if (statusFilter) r = r.filter((m) => m.status === statusFilter);
    if (typeFilter)   r = r.filter((m) => m.type === typeFilter);
    return r;
  }, [rows, search, wcFilter, statusFilter, typeFilter]);

  const selectedIds = selection.type === 'include' ? selection.ids : new Set<string | number>();

  function openCreate() { setDrawerMode('create'); setEditTarget(null); setDrawerOpen(true); }
  function openEdit(m: Machine) { setDrawerMode('edit'); setEditTarget(m); setDrawerOpen(true); }

  function handleSave(data: MachineFormValues) {
    setSaving(true);
    setTimeout(() => {
      const wc = WORK_CENTERS.find((w) => w.code === data.workCenterCode);
      if (drawerMode === 'create') {
        setRows((prev) => [...prev, {
          id: String(Date.now()),
          status: 'IDLE' as MachineStatus,
          oee: 0,
          workCenterName: wc?.name ?? '',
          ...data,
        }]);
      } else if (editTarget) {
        setRows((prev) => prev.map((m) =>
          m.id === editTarget.id
            ? { ...m, ...data, workCenterName: wc?.name ?? m.workCenterName }
            : m
        ));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((m) => m.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<Machine>[] = [
    {
      field: 'code',
      headerName: 'Code',
      width: 100,
      renderCell: (params: GridRenderCellParams<Machine>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'name', headerName: 'Name', flex: 1, minWidth: 150 },
    { field: 'type', headerName: 'Type', width: 110 },
    {
      field: 'workCenterCode',
      headerName: 'Work Center',
      width: 170,
      renderCell: (params: GridRenderCellParams<Machine>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'primary.main' }}>
            {params.value}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.2 }}>
            {params.row.workCenterName}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 110,
      renderCell: (params: GridRenderCellParams<Machine>) => {
        const status = params.value as MachineStatus;
        const color = machineColors[status];
        return (
          <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
            <StatusDot color={color} size={7} pulse={status === 'RUNNING'} />
            <Typography variant="body2" sx={{ fontSize: 12, color, fontWeight: 500 }}>
              {STATUS_LABELS[status]}
            </Typography>
          </Stack>
        );
      },
    },
    {
      field: 'oee',
      headerName: 'OEE %',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Machine>) => {
        const val = params.value as number;
        return (
          <Typography
            variant="body2"
            sx={{ fontSize: 12, fontWeight: 700, color: val > 0 ? oeeZoneColor(val) : '#94A3B8' }}
          >
            {val > 0 ? `${val}%` : '—'}
          </Typography>
        );
      },
    },
    {
      field: 'lastMaintenanceDate',
      headerName: 'Last Maintenance',
      width: 140,
      renderCell: (params: GridRenderCellParams<Machine>) => (
        <Typography variant="body2" sx={{ fontSize: 12, color: 'text.secondary' }}>{params.value}</Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 80,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<Machine>) => (
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
        title="Machines"
        subtitle="Monitor and manage all production machines across work centers"
        breadcrumbs={[{ label: 'Master Data' }, { label: 'Machines' }]}
        actions={
          <>
            {selectedIds.size > 0 && (
              <Button variant="outlined" color="error" size="small" startIcon={<SolarIcon name="delete" size={16} />}>
                Delete ({selectedIds.size})
              </Button>
            )}
            <Button variant="contained" size="small" startIcon={<SolarIcon name="add" size={16} />} onClick={openCreate}>
              Add Machine
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
            label: 'Work Center',
            value: wcFilter,
            options: WORK_CENTERS.map((w) => ({ label: `${w.code} — ${w.name}`, value: w.code })),
            onChange: setWcFilter,
          },
          {
            label: 'Status',
            value: statusFilter,
            options: (['RUNNING', 'IDLE', 'SETUP', 'DOWN', 'OFFLINE'] as MachineStatus[]).map((s) => ({ label: STATUS_LABELS[s], value: s })),
            onChange: setStatusFilter,
          },
          {
            label: 'Type',
            value: typeFilter,
            options: MACHINE_TYPES.map((t) => ({ label: t, value: t })),
            onChange: setTypeFilter,
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
                icon="emptySearch"
                title="No machines match your filters"
                description="Try adjusting your search or filters."
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
        title={drawerMode === 'create' ? 'Add Machine' : `Edit ${editTarget?.code}`}
        subtitle={drawerMode === 'create' ? 'Enter machine details below' : editTarget?.name}
        onSubmit={() => document.getElementById('machine-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel={drawerMode === 'create' ? 'Add Machine' : 'Save Changes'}
        loading={saving}
      >
        <MachineForm
          key={editTarget?.id ?? 'new'}
          defaultValues={editTarget ? {
            code:                editTarget.code,
            name:                editTarget.name,
            type:                editTarget.type,
            workCenterCode:      editTarget.workCenterCode,
            lastMaintenanceDate: editTarget.lastMaintenanceDate,
            isActive:            editTarget.isActive,
          } : {}}
          onSubmit={handleSave}
          loading={saving}
        />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Machine"
        description={
          <>
            Delete <strong>{deleteTarget?.name}</strong> ({deleteTarget?.code})?
            This cannot be undone and may affect production schedules.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
