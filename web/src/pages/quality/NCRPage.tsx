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

interface NCR {
  id: string;
  ncrNo: string;
  title: string;
  productCode: string;
  lotNo?: string;
  severity: 'Critical' | 'Major' | 'Minor';
  defectCode: string;
  defectName: string;
  detectedAt: string;
  detectedBy: string;
  status: 'Open' | 'Under Review' | 'Disposition Pending' | 'Closed';
  disposition?: 'Use As Is' | 'Rework' | 'Scrap' | 'Return to Supplier';
  affectedQty: number;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_NCRS: NCR[] = [
  { id: '1',  ncrNo: 'NCR-2026-0048', title: 'Surface scratches exceed limit',         productCode: 'FRM-A001', lotNo: 'LOT-A1042', severity: 'Minor',    defectCode: 'DC-001', defectName: 'Surface Scratch',      detectedAt: '2026-06-01', detectedBy: 'Nguyen Van A', status: 'Open',                affectedQty: 3 },
  { id: '2',  ncrNo: 'NCR-2026-0047', title: 'Dimension OOT on shaft diameter',        productCode: 'SHT-C003', lotNo: 'LOT-C0301', severity: 'Major',    defectCode: 'DC-002', defectName: 'Dimension OOT',        detectedAt: '2026-05-30', detectedBy: 'Le Van C',    status: 'Disposition Pending', disposition: 'Scrap',               affectedQty: 12 },
  { id: '3',  ncrNo: 'NCR-2026-0049', title: 'Weld porosity detected in joint area',   productCode: 'FRM-A001', lotNo: 'LOT-A1038', severity: 'Critical', defectCode: 'DC-003', defectName: 'Weld Porosity',        detectedAt: '2026-06-02', detectedBy: 'Tran Thi B',  status: 'Under Review',        affectedQty: 5 },
  { id: '4',  ncrNo: 'NCR-2026-0050', title: 'Missing anodizing on bracket surface',   productCode: 'BRK-D004', lotNo: 'LOT-D2201', severity: 'Minor',    defectCode: 'DC-004', defectName: 'Missing Treatment',    detectedAt: '2026-06-03', detectedBy: 'Nguyen Van A', status: 'Closed',              disposition: 'Rework',              affectedQty: 8 },
  { id: '5',  ncrNo: 'NCR-2026-0051', title: 'Motor mount hole misalignment',          productCode: 'MTR-E005', lotNo: 'LOT-E0091', severity: 'Major',    defectCode: 'DC-005', defectName: 'Hole Misalignment',    detectedAt: '2026-06-04', detectedBy: 'Pham Thi D',  status: 'Closed',              disposition: 'Scrap',               affectedQty: 8 },
  { id: '6',  ncrNo: 'NCR-2026-0052', title: 'Wrong material certificate received',    productCode: 'SPR-H008', lotNo: undefined,   severity: 'Critical', defectCode: 'DC-006', defectName: 'Wrong Material',       detectedAt: '2026-06-05', detectedBy: 'Le Van C',    status: 'Open',                affectedQty: 100 },
  { id: '7',  ncrNo: 'NCR-2026-0053', title: 'Hinge torque below specification',       productCode: 'HNG-J010', lotNo: 'LOT-J0218', severity: 'Major',    defectCode: 'DC-007', defectName: 'Torque Non-conformance', detectedAt: '2026-06-06', detectedBy: 'Nguyen Van A', status: 'Under Review',       affectedQty: 15 },
  { id: '8',  ncrNo: 'NCR-2026-0054', title: 'Wheel hub runout exceeds tolerance',     productCode: 'WHL-L012', lotNo: 'LOT-L0045', severity: 'Major',    defectCode: 'DC-002', defectName: 'Dimension OOT',        detectedAt: '2026-06-07', detectedBy: 'Tran Thi B',  status: 'Disposition Pending', disposition: 'Rework',              affectedQty: 4 },
  { id: '9',  ncrNo: 'NCR-2026-0055', title: 'Panel delamination at corner joints',    productCode: 'PNL-B002', lotNo: 'LOT-B0891', severity: 'Critical', defectCode: 'DC-008', defectName: 'Delamination',          detectedAt: '2026-06-08', detectedBy: 'Pham Thi D',  status: 'Open',                affectedQty: 2 },
  { id: '10', ncrNo: 'NCR-2026-0056', title: 'Guard edge burr exceeds safety limit',   productCode: 'GRD-G007', lotNo: 'LOT-G0019', severity: 'Minor',    defectCode: 'DC-001', defectName: 'Surface Scratch',      detectedAt: '2026-06-09', detectedBy: 'Le Van C',    status: 'Closed',              disposition: 'Use As Is',           affectedQty: 6 },
];

// ─── Color helpers ────────────────────────────────────────────────────────────

const SEVERITY_COLORS: Record<NCR['severity'], string> = {
  Critical: '#DC2626',
  Major:    '#D97706',
  Minor:    '#94A3B8',
};

const STATUS_COLORS: Record<NCR['status'], string> = {
  'Open':                 '#1D4ED8',
  'Under Review':         '#D97706',
  'Disposition Pending':  '#7C3AED',
  'Closed':               '#475569',
};

const DISPOSITION_COLORS: Record<NonNullable<NCR['disposition']>, string> = {
  'Use As Is':          '#15803D',
  'Rework':             '#D97706',
  'Scrap':              '#DC2626',
  'Return to Supplier': '#7C3AED',
};

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function NCRPage() {
  const [rows, setRows]                 = useState<NCR[]>(MOCK_NCRS);
  const [search, setSearch]             = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [severityFilter, setSeverityFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<NCR | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<NCR | null>(null);
  const [saving, setSaving]             = useState(false);

  // Form state
  const [formTitle, setFormTitle]           = useState('');
  const [formProductCode, setFormProductCode] = useState('');
  const [formLotNo, setFormLotNo]           = useState('');
  const [formSeverity, setFormSeverity]     = useState<NCR['severity']>('Major');
  const [formDefectCode, setFormDefectCode] = useState('');
  const [formDefectName, setFormDefectName] = useState('');
  const [formAffectedQty, setFormAffectedQty] = useState('');
  const [formDetectedBy, setFormDetectedBy] = useState('');

  const filtered = useMemo(() => {
    let r = rows;
    if (search)         r = r.filter((n) => n.ncrNo.toLowerCase().includes(search.toLowerCase()) || n.title.toLowerCase().includes(search.toLowerCase()) || n.productCode.toLowerCase().includes(search.toLowerCase()));
    if (statusFilter)   r = r.filter((n) => n.status === statusFilter);
    if (severityFilter) r = r.filter((n) => n.severity === severityFilter);
    return r;
  }, [rows, search, statusFilter, severityFilter]);

  function openCreate() {
    setDrawerMode('create');
    setEditTarget(null);
    setFormTitle('');
    setFormProductCode('');
    setFormLotNo('');
    setFormSeverity('Major');
    setFormDefectCode('');
    setFormDefectName('');
    setFormAffectedQty('');
    setFormDetectedBy('');
    setDrawerOpen(true);
  }

  function openEdit(n: NCR) {
    setDrawerMode('edit');
    setEditTarget(n);
    setFormTitle(n.title);
    setFormProductCode(n.productCode);
    setFormLotNo(n.lotNo ?? '');
    setFormSeverity(n.severity);
    setFormDefectCode(n.defectCode);
    setFormDefectName(n.defectName);
    setFormAffectedQty(String(n.affectedQty));
    setFormDetectedBy(n.detectedBy);
    setDrawerOpen(true);
  }

  function handleSave() {
    setSaving(true);
    setTimeout(() => {
      const newRow: Partial<NCR> = {
        title: formTitle,
        productCode: formProductCode,
        lotNo: formLotNo || undefined,
        severity: formSeverity,
        defectCode: formDefectCode,
        defectName: formDefectName,
        affectedQty: Number(formAffectedQty) || 0,
        detectedBy: formDetectedBy,
      };
      if (drawerMode === 'create') {
        setRows((prev) => [
          ...prev,
          {
            id: String(Date.now()),
            ncrNo: `NCR-2026-${String(Date.now()).slice(-4)}`,
            detectedAt: new Date().toISOString().slice(0, 10),
            status: 'Open',
            ...newRow,
          } as NCR,
        ]);
      } else if (editTarget) {
        setRows((prev) => prev.map((n) => n.id === editTarget.id ? { ...n, ...newRow } : n));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((n) => n.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<NCR>[] = [
    {
      field: 'ncrNo',
      headerName: 'NCR #',
      width: 140,
      renderCell: (params: GridRenderCellParams<NCR>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    { field: 'title', headerName: 'Title', flex: 1, minWidth: 200 },
    {
      field: 'productCode',
      headerName: 'Product',
      width: 110,
      renderCell: (params: GridRenderCellParams<NCR>) => (
        <Stack spacing={0}>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'primary.main', lineHeight: 1.3 }}>
            {params.row.productCode}
          </Typography>
          {params.row.lotNo && (
            <Typography variant="caption" color="text.secondary" sx={{ fontSize: 10, lineHeight: 1.3 }}>
              {params.row.lotNo}
            </Typography>
          )}
        </Stack>
      ),
    },
    {
      field: 'severity',
      headerName: 'Severity',
      width: 100,
      renderCell: (params: GridRenderCellParams<NCR>) => {
        const color = SEVERITY_COLORS[params.value as NCR['severity']];
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 700, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }}
          />
        );
      },
    },
    {
      field: 'defectCode',
      headerName: 'Defect Type',
      width: 160,
      renderCell: (params: GridRenderCellParams<NCR>) => (
        <Stack spacing={0}>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, lineHeight: 1.3 }}>
            {params.row.defectCode}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontSize: 10, lineHeight: 1.3 }}>
            {params.row.defectName}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'affectedQty',
      headerName: 'Qty',
      width: 60,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<NCR>) => (
        <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600 }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 155,
      renderCell: (params: GridRenderCellParams<NCR>) => {
        const color = STATUS_COLORS[params.value as NCR['status']];
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }}
          />
        );
      },
    },
    {
      field: 'disposition',
      headerName: 'Disposition',
      width: 145,
      renderCell: (params: GridRenderCellParams<NCR>) => {
        if (!params.value) return <Typography variant="body2" color="text.disabled" sx={{ fontSize: 11 }}>—</Typography>;
        const color = DISPOSITION_COLORS[params.value as NonNullable<NCR['disposition']>];
        return (
          <Chip
            label={params.value}
            size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }}
          />
        );
      },
    },
    { field: 'detectedBy', headerName: 'Detected By', width: 120 },
    {
      field: 'detectedAt',
      headerName: 'Date',
      width: 100,
      renderCell: (params: GridRenderCellParams<NCR>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 11 }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 80,
      sortable: false,
      align: 'center',
      renderCell: (params: GridRenderCellParams<NCR>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="View">
            <IconButton size="small" sx={{ color: 'text.secondary' }}>
              <SolarIcon name="view" size={16} />
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => openEdit(params.row)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="edit" size={16} />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Non-Conformance Reports"
        subtitle="Track and manage non-conforming materials, components and assemblies"
        breadcrumbs={[{ label: 'Quality' }, { label: 'NCR' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={openCreate}
          >
            New NCR
          </Button>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search NCR # or title…"
        filters={[
          {
            label: 'Status',
            value: statusFilter,
            options: [
              { label: 'Open',                value: 'Open' },
              { label: 'Under Review',        value: 'Under Review' },
              { label: 'Disposition Pending', value: 'Disposition Pending' },
              { label: 'Closed',              value: 'Closed' },
            ],
            onChange: setStatusFilter,
          },
          {
            label: 'Severity',
            value: severityFilter,
            options: [
              { label: 'Critical', value: 'Critical' },
              { label: 'Major',    value: 'Major' },
              { label: 'Minor',    value: 'Minor' },
            ],
            onChange: setSeverityFilter,
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
                icon={search || statusFilter || severityFilter ? 'emptySearch' : 'emptyTable'}
                title={search || statusFilter || severityFilter ? 'No NCRs match your filters' : 'No NCRs yet'}
                description={
                  search || statusFilter || severityFilter
                    ? 'Try adjusting your search or filters.'
                    : 'Create your first NCR to get started.'
                }
                action={
                  !search && !statusFilter && !severityFilter ? (
                    <Button variant="contained" size="small" onClick={openCreate}>
                      New NCR
                    </Button>
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

      {/* Create / Edit drawer */}
      <FormDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        title={drawerMode === 'create' ? 'New NCR' : `Edit ${editTarget?.ncrNo}`}
        subtitle={drawerMode === 'create' ? 'Enter non-conformance details below' : editTarget?.title}
        onSubmit={handleSave}
        submitLabel={drawerMode === 'create' ? 'Create NCR' : 'Save Changes'}
        loading={saving}
      >
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <TextField
              label="Title"
              value={formTitle}
              onChange={(e) => setFormTitle(e.target.value)}
              fullWidth
              required
              placeholder="Brief description of the non-conformance"
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Product Code"
              value={formProductCode}
              onChange={(e) => setFormProductCode(e.target.value)}
              fullWidth
              required
              slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Lot # (optional)"
              value={formLotNo}
              onChange={(e) => setFormLotNo(e.target.value)}
              fullWidth
              slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="Severity"
              value={formSeverity}
              onChange={(e) => setFormSeverity(e.target.value as NCR['severity'])}
              fullWidth
              required
            >
              {(['Critical', 'Major', 'Minor'] as const).map((s) => (
                <MenuItem key={s} value={s}>{s}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Affected Qty"
              type="number"
              value={formAffectedQty}
              onChange={(e) => setFormAffectedQty(e.target.value)}
              fullWidth
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Defect Code"
              value={formDefectCode}
              onChange={(e) => setFormDefectCode(e.target.value)}
              fullWidth
              required
              slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace', fontSize: 13 } } }}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Defect Name"
              value={formDefectName}
              onChange={(e) => setFormDefectName(e.target.value)}
              fullWidth
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              label="Detected By"
              value={formDetectedBy}
              onChange={(e) => setFormDetectedBy(e.target.value)}
              fullWidth
              required
            />
          </Grid>
        </Grid>
      </FormDrawer>

      {/* Delete confirmation (via context menu / future use) */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete NCR"
        description={
          <>
            Delete <strong>{deleteTarget?.ncrNo}</strong>?
            This action cannot be undone.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
