import {
  Box,
  Chip,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams, GridRowSelectionModel } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  EmptyState,
  ExportButton,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TableToolbar,
} from '../../components';

// ─── Types & mock data ────────────────────────────────────────────────────────

type JobStatus = 'QUEUED' | 'RUNNING' | 'PAUSED' | 'COMPLETED' | 'CANCELLED';

interface Job {
  id: string;
  no: string;
  woNo: string;
  productCode: string;
  productName: string;
  machine: string;
  operator: string;
  status: JobStatus;
  startedAt?: string;
  completedAt?: string;
  targetQty: number;
  actualQty: number;
  defects: number;
}

const JOB_STATUS_COLORS: Record<JobStatus, { bg: string; text: string; border: string }> = {
  QUEUED:    { bg: alpha('#475569', 0.1), text: '#475569', border: alpha('#475569', 0.3) },
  RUNNING:   { bg: alpha('#15803D', 0.1), text: '#15803D', border: alpha('#15803D', 0.3) },
  PAUSED:    { bg: alpha('#B45309', 0.1), text: '#B45309', border: alpha('#B45309', 0.3) },
  COMPLETED: { bg: alpha('#166534', 0.1), text: '#166534', border: alpha('#166534', 0.3) },
  CANCELLED: { bg: alpha('#B91C1C', 0.1), text: '#B91C1C', border: alpha('#B91C1C', 0.3) },
};

const JOB_STATUS_LABELS: Record<JobStatus, string> = {
  QUEUED:    'Queued',
  RUNNING:   'Running',
  PAUSED:    'Paused',
  COMPLETED: 'Completed',
  CANCELLED: 'Cancelled',
};

const MOCK_JOBS: Job[] = [
  { id: '1',  no: 'JOB-4421', woNo: 'WO-2026-0089', productCode: 'FRM-A001', productName: 'Frame Assembly A',     machine: 'MC-01', operator: 'Nguyen Van A', status: 'RUNNING',   startedAt: '2026-06-12 07:30', completedAt: undefined,        targetQty: 100, actualQty: 78,  defects: 2 },
  { id: '2',  no: 'JOB-4422', woNo: 'WO-2026-0089', productCode: 'FRM-A001', productName: 'Frame Assembly A',     machine: 'MC-01', operator: 'Tran Thi B',   status: 'QUEUED',    startedAt: undefined,          completedAt: undefined,        targetQty: 100, actualQty: 0,   defects: 0 },
  { id: '3',  no: 'JOB-4418', woNo: 'WO-2026-0088', productCode: 'SHT-C003', productName: 'Shaft Housing C',      machine: 'MC-03', operator: 'Le Van C',     status: 'PAUSED',    startedAt: '2026-06-11 14:00', completedAt: undefined,        targetQty: 200, actualQty: 155, defects: 5 },
  { id: '4',  no: 'JOB-4415', woNo: 'WO-2026-0091', productCode: 'BRK-D004', productName: 'Bracket Set D',        machine: 'MC-04', operator: 'Nguyen Van A', status: 'COMPLETED', startedAt: '2026-06-05 08:00', completedAt: '2026-06-05 17:00', targetQty: 300, actualQty: 300, defects: 1 },
  { id: '5',  no: 'JOB-4416', woNo: 'WO-2026-0091', productCode: 'BRK-D004', productName: 'Bracket Set D',        machine: 'MC-04', operator: 'Tran Thi B',   status: 'COMPLETED', startedAt: '2026-06-06 08:00', completedAt: '2026-06-06 16:30', targetQty: 300, actualQty: 298, defects: 2 },
  { id: '6',  no: 'JOB-4420', woNo: 'WO-2026-0094', productCode: 'FRM-A001', productName: 'Frame Assembly A',     machine: 'MC-01', operator: 'Le Van C',     status: 'RUNNING',   startedAt: '2026-06-12 08:15', completedAt: undefined,        targetQty: 120, actualQty: 42,  defects: 0 },
  { id: '7',  no: 'JOB-4410', woNo: 'WO-2026-0085', productCode: 'MTR-E005', productName: 'Motor Mount E',        machine: 'MC-05', operator: 'Nguyen Van A', status: 'COMPLETED', startedAt: '2026-05-28 07:30', completedAt: '2026-05-28 18:00', targetQty: 150, actualQty: 149, defects: 1 },
  { id: '8',  no: 'JOB-4423', woNo: 'WO-2026-0090', productCode: 'PNL-B002', productName: 'Panel Sub-assembly B', machine: 'MC-02', operator: 'Tran Thi B',   status: 'QUEUED',    startedAt: undefined,          completedAt: undefined,        targetQty: 100, actualQty: 0,   defects: 0 },
  { id: '9',  no: 'JOB-4408', woNo: 'WO-2026-0087', productCode: 'GRD-G007', productName: 'Guard Assembly G',     machine: 'MC-02', operator: 'Le Van C',     status: 'CANCELLED', startedAt: undefined,          completedAt: undefined,        targetQty: 200, actualQty: 0,   defects: 0 },
  { id: '10', no: 'JOB-4419', woNo: 'WO-2026-0086', productCode: 'SPR-H008', productName: 'Sprocket Set H',       machine: 'MC-06', operator: 'Nguyen Van A', status: 'PAUSED',    startedAt: '2026-06-10 09:00', completedAt: undefined,        targetQty: 125, actualQty: 80,  defects: 3 },
];

const MACHINE_OPTIONS = ['MC-01', 'MC-02', 'MC-03', 'MC-04', 'MC-05', 'MC-06'];
const STATUS_FILTER_OPTIONS: { label: string; value: string }[] = [
  { label: 'Queued',    value: 'QUEUED' },
  { label: 'Running',   value: 'RUNNING' },
  { label: 'Paused',    value: 'PAUSED' },
  { label: 'Completed', value: 'COMPLETED' },
  { label: 'Cancelled', value: 'CANCELLED' },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function JobsPage() {
  const navigate = useNavigate();
  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [machineFilter, setMachineFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });

  const filtered = useMemo(() => {
    let r = MOCK_JOBS;
    if (search)        r = r.filter((j) => j.no.toLowerCase().includes(search.toLowerCase()) || j.woNo.toLowerCase().includes(search.toLowerCase()) || j.productName.toLowerCase().includes(search.toLowerCase()));
    if (statusFilter)  r = r.filter((j) => j.status === statusFilter);
    if (machineFilter) r = r.filter((j) => j.machine === machineFilter);
    return r;
  }, [search, statusFilter, machineFilter]);

  const columns: GridColDef<Job>[] = [
    {
      field: 'no',
      headerName: 'Job #',
      width: 110,
      renderCell: (params: GridRenderCellParams<Job>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
            {params.value as string}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 10 }}>
            {params.row.woNo}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'productName',
      headerName: 'Product',
      flex: 1,
      minWidth: 160,
      renderCell: (params: GridRenderCellParams<Job>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600 }}>{params.value as string}</Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11 }}>{params.row.productCode}</Typography>
        </Stack>
      ),
    },
    {
      field: 'machine',
      headerName: 'Machine',
      width: 90,
      renderCell: (params: GridRenderCellParams<Job>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600 }}>{params.value as string}</Typography>
      ),
    },
    {
      field: 'operator',
      headerName: 'Operator',
      width: 140,
      renderCell: (params: GridRenderCellParams<Job>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>{params.value as string}</Typography>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 110,
      renderCell: (params: GridRenderCellParams<Job>) => {
        const s = params.value as JobStatus;
        const c = JOB_STATUS_COLORS[s];
        return (
          <Chip
            label={JOB_STATUS_LABELS[s]}
            size="small"
            sx={{ height: 22, fontSize: '0.6875rem', fontWeight: 600, bgcolor: c.bg, color: c.text, border: `1px solid ${c.border}`, '& .MuiChip-label': { px: 1 } }}
          />
        );
      },
    },
    {
      field: 'output',
      headerName: 'Output',
      width: 90,
      align: 'right',
      headerAlign: 'right',
      sortable: false,
      renderCell: (params: GridRenderCellParams<Job>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          <strong>{params.row.actualQty}</strong>
          <Typography component="span" variant="caption" color="text.secondary"> / {params.row.targetQty}</Typography>
        </Typography>
      ),
    },
    {
      field: 'defects',
      headerName: 'Defects',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<Job>) => {
        const d = params.value as number;
        return d > 0 ? (
          <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 700, color: 'error.main' }}>{d}</Typography>
        ) : (
          <Typography variant="body2" sx={{ fontSize: 12, color: 'text.disabled' }}>—</Typography>
        );
      },
    },
    {
      field: 'actions',
      headerName: '',
      width: 60,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<Job>) => (
        <Tooltip title="View Job">
          <IconButton size="small" onClick={() => navigate(`/production/jobs/${params.row.id}`)} sx={{ color: 'text.secondary' }}>
            <SolarIcon name="view" size={16} />
          </IconButton>
        </Tooltip>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Jobs"
        subtitle="Active and historical production jobs across all machines"
        breadcrumbs={[{ label: 'Production' }, { label: 'Jobs' }]}
        actions={
          <Stack direction="row" spacing={0.5}>
            <ExportButton />
            <RefreshButton />
          </Stack>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search job #, WO or product…"
        filters={[
          {
            label: 'Status',
            value: statusFilter,
            options: STATUS_FILTER_OPTIONS,
            onChange: setStatusFilter,
          },
          {
            label: 'Machine',
            value: machineFilter,
            options: MACHINE_OPTIONS.map((m) => ({ label: m, value: m })),
            onChange: setMachineFilter,
          },
        ]}
        totalCount={filtered.length}
        actions={<RefreshButton />}
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
                icon={search || statusFilter || machineFilter ? 'emptySearch' : 'emptyTable'}
                title="No jobs found"
                description="Jobs are created automatically when work orders are released."
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
    </PageRoot>
  );
}
