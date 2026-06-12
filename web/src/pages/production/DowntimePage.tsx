import {
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
import type { GridColDef, GridRenderCellParams, GridRowSelectionModel } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
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

// ─── Types & mock data ────────────────────────────────────────────────────────

type DowntimeCategory = 'Breakdown' | 'Planned' | 'Setup' | 'Minor Stop';
type DowntimeStatus = 'Active' | 'Resolved';

interface DowntimeEvent {
  id: string;
  machine: string;
  machineId: string;
  startedAt: string;
  endedAt?: string;
  duration?: string;
  category: DowntimeCategory;
  reason: string;
  status: DowntimeStatus;
  reportedBy: string;
}

const CATEGORY_COLORS: Record<DowntimeCategory, { bg: string; text: string; border: string }> = {
  Breakdown:    { bg: alpha('#DC2626', 0.1), text: '#DC2626', border: alpha('#DC2626', 0.3) },
  Planned:      { bg: alpha('#1D4ED8', 0.1), text: '#1D4ED8', border: alpha('#1D4ED8', 0.3) },
  Setup:        { bg: alpha('#D97706', 0.1), text: '#D97706', border: alpha('#D97706', 0.3) },
  'Minor Stop': { bg: alpha('#EA580C', 0.1), text: '#EA580C', border: alpha('#EA580C', 0.3) },
};

const MOCK_DOWNTIME: DowntimeEvent[] = [
  { id: '1',  machine: 'CNC Lathe 1',     machineId: 'MC-01', startedAt: '2026-06-12 06:45', endedAt: '2026-06-12 08:20', duration: '1h 35m', category: 'Breakdown',    reason: 'Spindle bearing failure',          status: 'Resolved', reportedBy: 'Nguyen Van A' },
  { id: '2',  machine: 'Press Line B',    machineId: 'MC-02', startedAt: '2026-06-12 09:00', endedAt: undefined,          duration: undefined,  category: 'Breakdown',    reason: 'Hydraulic pressure loss',          status: 'Active',   reportedBy: 'Tran Thi B' },
  { id: '3',  machine: 'CNC Mill 3',      machineId: 'MC-03', startedAt: '2026-06-12 07:30', endedAt: '2026-06-12 08:00', duration: '30m',     category: 'Setup',        reason: 'Fixture change for new batch',     status: 'Resolved', reportedBy: 'Le Van C' },
  { id: '4',  machine: 'Robot Welder 4',  machineId: 'MC-04', startedAt: '2026-06-11 14:00', endedAt: '2026-06-11 16:00', duration: '2h 00m',  category: 'Planned',      reason: 'Scheduled preventive maintenance', status: 'Resolved', reportedBy: 'Nguyen Van A' },
  { id: '5',  machine: 'Assembly Stn 5',  machineId: 'MC-05', startedAt: '2026-06-12 10:30', endedAt: undefined,          duration: undefined,  category: 'Minor Stop',   reason: 'Material jam at feed belt',        status: 'Active',   reportedBy: 'Tran Thi B' },
  { id: '6',  machine: 'Grinder 6',       machineId: 'MC-06', startedAt: '2026-06-11 11:00', endedAt: '2026-06-11 11:45', duration: '45m',     category: 'Setup',        reason: 'Wheel change and balancing',       status: 'Resolved', reportedBy: 'Le Van C' },
  { id: '7',  machine: 'CNC Lathe 1',     machineId: 'MC-01', startedAt: '2026-06-10 15:00', endedAt: '2026-06-10 18:00', duration: '3h 00m',  category: 'Planned',      reason: 'Tool replacement — scheduled',     status: 'Resolved', reportedBy: 'Nguyen Van A' },
  { id: '8',  machine: 'Press Line B',    machineId: 'MC-02', startedAt: '2026-06-09 08:00', endedAt: '2026-06-09 09:30', duration: '1h 30m',  category: 'Breakdown',    reason: 'PLC communication fault',          status: 'Resolved', reportedBy: 'Tran Thi B' },
  { id: '9',  machine: 'CNC Mill 3',      machineId: 'MC-03', startedAt: '2026-06-12 11:00', endedAt: undefined,          duration: undefined,  category: 'Minor Stop',   reason: 'Coolant level low — refilling',   status: 'Active',   reportedBy: 'Le Van C' },
  { id: '10', machine: 'Robot Welder 4',  machineId: 'MC-04', startedAt: '2026-06-08 13:30', endedAt: '2026-06-08 14:00', duration: '30m',     category: 'Minor Stop',   reason: 'Wire spool replacement',           status: 'Resolved', reportedBy: 'Nguyen Van A' },
];

const MACHINE_OPTIONS = [
  { id: 'MC-01', name: 'CNC Lathe 1' },
  { id: 'MC-02', name: 'Press Line B' },
  { id: 'MC-03', name: 'CNC Mill 3' },
  { id: 'MC-04', name: 'Robot Welder 4' },
  { id: 'MC-05', name: 'Assembly Stn 5' },
  { id: 'MC-06', name: 'Grinder 6' },
];

const CATEGORIES: DowntimeCategory[] = ['Breakdown', 'Planned', 'Setup', 'Minor Stop'];

// ─── Form ─────────────────────────────────────────────────────────────────────

const DowntimeSchema = z.object({
  machineId:  z.string().min(1, 'Machine is required'),
  category:   z.enum(['Breakdown', 'Planned', 'Setup', 'Minor Stop']),
  reason:     z.string().min(1, 'Reason is required'),
  startedAt:  z.string().min(1, 'Start time is required'),
  reportedBy: z.string().min(1, 'Reporter is required'),
});
type DowntimeFormValues = z.infer<typeof DowntimeSchema>;

function DowntimeForm({ defaultValues, onSubmit }: { defaultValues: Partial<DowntimeFormValues>; onSubmit: (d: DowntimeFormValues) => void }) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<DowntimeFormValues>({
    resolver: zodResolver(DowntimeSchema),
    defaultValues: { category: 'Breakdown', ...defaultValues },
  });

  return (
    <Box component="form" id="downtime-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <Controller
            name="machineId"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                select
                label="Machine"
                fullWidth
                required
                error={!!errors.machineId}
                helperText={errors.machineId?.message}
              >
                {MACHINE_OPTIONS.map((m) => (
                  <MenuItem key={m.id} value={m.id}>{m.id} — {m.name}</MenuItem>
                ))}
              </TextField>
            )}
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
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
            {...register('reason')}
            label="Reason"
            fullWidth
            required
            multiline
            rows={2}
            error={!!errors.reason}
            helperText={errors.reason?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('startedAt')}
            label="Started At"
            fullWidth
            required
            placeholder="YYYY-MM-DD HH:MM"
            error={!!errors.startedAt}
            helperText={errors.startedAt?.message}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            {...register('reportedBy')}
            label="Reported By"
            fullWidth
            required
            error={!!errors.reportedBy}
            helperText={errors.reportedBy?.message}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function DowntimePage() {
  const navigate = useNavigate();
  const [rows, setRows]                   = useState<DowntimeEvent[]>(MOCK_DOWNTIME);
  const [search, setSearch]               = useState('');
  const [machineFilter, setMachineFilter] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [selection, setSelection]         = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]       = useState(false);
  const [deleteTarget, setDeleteTarget]   = useState<DowntimeEvent | null>(null);
  const [saving, setSaving]               = useState(false);

  const filtered = useMemo(() => {
    let r = rows;
    if (search)         r = r.filter((d) => d.machine.toLowerCase().includes(search.toLowerCase()) || d.reason.toLowerCase().includes(search.toLowerCase()));
    if (machineFilter)  r = r.filter((d) => d.machineId === machineFilter);
    if (categoryFilter) r = r.filter((d) => d.category === categoryFilter);
    return r;
  }, [rows, search, machineFilter, categoryFilter]);

  function handleSave(data: DowntimeFormValues) {
    setSaving(true);
    const machine = MACHINE_OPTIONS.find((m) => m.id === data.machineId);
    setTimeout(() => {
      setRows((prev) => [
        ...prev,
        {
          id: String(Date.now()),
          machine: machine?.name ?? data.machineId,
          machineId: data.machineId,
          startedAt: data.startedAt,
          endedAt: undefined,
          duration: undefined,
          category: data.category,
          reason: data.reason,
          status: 'Active',
          reportedBy: data.reportedBy,
        },
      ]);
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleResolve(row: DowntimeEvent) {
    setRows((prev) => prev.map((d) => d.id === row.id ? { ...d, status: 'Resolved', endedAt: '2026-06-12 12:00', duration: '1h 00m' } : d));
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((d) => d.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<DowntimeEvent>[] = [
    {
      field: 'machine',
      headerName: 'Machine',
      width: 155,
      renderCell: (params: GridRenderCellParams<DowntimeEvent>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600 }}>{params.row.machineId}</Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11 }}>{params.value as string}</Typography>
        </Stack>
      ),
    },
    {
      field: 'category',
      headerName: 'Category',
      width: 115,
      renderCell: (params: GridRenderCellParams<DowntimeEvent>) => {
        const cat = params.value as DowntimeCategory;
        const c = CATEGORY_COLORS[cat];
        return (
          <Chip
            label={cat}
            size="small"
            sx={{ height: 22, fontSize: '0.6875rem', fontWeight: 600, bgcolor: c.bg, color: c.text, border: `1px solid ${c.border}`, '& .MuiChip-label': { px: 1 } }}
          />
        );
      },
    },
    {
      field: 'reason',
      headerName: 'Reason',
      flex: 1,
      minWidth: 180,
      renderCell: (params: GridRenderCellParams<DowntimeEvent>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>{params.value as string}</Typography>
      ),
    },
    {
      field: 'startedAt',
      headerName: 'Started',
      width: 140,
      renderCell: (params: GridRenderCellParams<DowntimeEvent>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>{params.value as string}</Typography>
      ),
    },
    {
      field: 'duration',
      headerName: 'Duration',
      width: 90,
      renderCell: (params: GridRenderCellParams<DowntimeEvent>) => (
        <Typography variant="body2" sx={{ fontSize: 12, color: params.value ? 'text.primary' : 'text.disabled' }}>
          {(params.value as string | undefined) ?? '—'}
        </Typography>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 95,
      renderCell: (params: GridRenderCellParams<DowntimeEvent>) => {
        const active = params.value === 'Active';
        return (
          <Chip
            label={params.value as string}
            size="small"
            sx={{
              height: 22,
              fontSize: '0.6875rem',
              fontWeight: 600,
              bgcolor: active ? alpha('#DC2626', 0.1) : alpha('#15803D', 0.1),
              color: active ? '#DC2626' : '#15803D',
              border: `1px solid ${active ? alpha('#DC2626', 0.3) : alpha('#15803D', 0.3)}`,
              '& .MuiChip-label': { px: 1 },
            }}
          />
        );
      },
    },
    {
      field: 'actions',
      headerName: '',
      width: 90,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<DowntimeEvent>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View">
            <IconButton size="small" onClick={() => navigate(`/production/downtime/${params.row.id}`)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="view" size={16} />
            </IconButton>
          </Tooltip>
          {params.row.status === 'Active' && (
            <Tooltip title="Resolve">
              <IconButton size="small" onClick={() => handleResolve(params.row)} sx={{ color: 'text.secondary', '&:hover': { color: 'success.main' } }}>
                <SolarIcon name="complete" size={16} />
              </IconButton>
            </Tooltip>
          )}
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
        title="Downtime Log"
        subtitle="Record and track machine downtime events across the shop floor"
        breadcrumbs={[{ label: 'Production' }, { label: 'Downtime' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => setDrawerOpen(true)}
          >
            Log Downtime
          </Button>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search machine or reason…"
        filters={[
          {
            label: 'Machine',
            value: machineFilter,
            options: MACHINE_OPTIONS.map((m) => ({ label: `${m.id} — ${m.name}`, value: m.id })),
            onChange: setMachineFilter,
          },
          {
            label: 'Category',
            value: categoryFilter,
            options: CATEGORIES.map((c) => ({ label: c, value: c })),
            onChange: setCategoryFilter,
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
                icon={search || machineFilter || categoryFilter ? 'emptySearch' : 'emptyTable'}
                title="No downtime events"
                description="Log a downtime event when a machine goes offline or requires maintenance."
                action={
                  !search && !machineFilter && !categoryFilter ? (
                    <Button variant="contained" size="small" onClick={() => setDrawerOpen(true)}>Log Downtime</Button>
                  ) : undefined
                }
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
        title="Log Downtime Event"
        subtitle="Record a new machine downtime incident"
        onSubmit={() => document.getElementById('downtime-form')?.dispatchEvent(new Event('submit', { bubbles: true }))}
        submitLabel="Log Event"
        loading={saving}
      >
        <DowntimeForm defaultValues={{}} onSubmit={handleSave} />
      </FormDrawer>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Downtime Event"
        description={
          <>Delete downtime record for <strong>{deleteTarget?.machine}</strong> on {deleteTarget?.startedAt}? This cannot be undone.</>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
