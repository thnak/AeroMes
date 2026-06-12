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

interface InspectionPlan {
  id: string;
  planCode: string;
  productCode: string;
  productName: string;
  inspectionType: string;
  aqlLevel: string;
  inspectionLevel: 'I' | 'II' | 'III' | 'S1' | 'S2' | 'S3' | 'S4';
  checkpoints: number;
  isActive: boolean;
  version: string;
  updatedAt: string;
}

// ─── Mock data ────────────────────────────────────────────────────────────────

const MOCK_PLANS: InspectionPlan[] = [
  { id: '1', planCode: 'QP-001', productCode: 'FRM-A001', productName: 'Frame Assembly A',    inspectionType: 'Final + Incoming', aqlLevel: '1.0',  inspectionLevel: 'II',  checkpoints: 12, isActive: true,  version: 'Rev.B', updatedAt: '2026-05-10' },
  { id: '2', planCode: 'QP-002', productCode: 'PNL-B002', productName: 'Panel Sub-assembly B', inspectionType: 'Incoming',         aqlLevel: '0.65', inspectionLevel: 'II',  checkpoints: 8,  isActive: true,  version: 'Rev.A', updatedAt: '2026-04-22' },
  { id: '3', planCode: 'QP-003', productCode: 'SHT-C003', productName: 'Shaft Housing C',      inspectionType: 'Final',            aqlLevel: '0.25', inspectionLevel: 'III', checkpoints: 15, isActive: true,  version: 'Rev.C', updatedAt: '2026-05-18' },
  { id: '4', planCode: 'QP-004', productCode: 'BRK-D004', productName: 'Bracket Set D',        inspectionType: 'Incoming',         aqlLevel: '1.5',  inspectionLevel: 'I',   checkpoints: 5,  isActive: true,  version: 'Rev.A', updatedAt: '2026-03-15' },
  { id: '5', planCode: 'QP-005', productCode: 'MTR-E005', productName: 'Motor Mount E',         inspectionType: 'In-Process + Final', aqlLevel: '1.0', inspectionLevel: 'II', checkpoints: 10, isActive: true,  version: 'Rev.B', updatedAt: '2026-05-28' },
  { id: '6', planCode: 'QP-006', productCode: 'HNG-J010', productName: 'Hinge Assembly J',     inspectionType: 'Final',            aqlLevel: '0.65', inspectionLevel: 'III', checkpoints: 12, isActive: true,  version: 'Rev.A', updatedAt: '2026-04-05' },
  { id: '7', planCode: 'QP-007', productCode: 'WHL-L012', productName: 'Wheel & Hub L',         inspectionType: 'Final',            aqlLevel: '0.25', inspectionLevel: 'III', checkpoints: 18, isActive: true,  version: 'Rev.D', updatedAt: '2026-06-01' },
  { id: '8', planCode: 'QP-008', productCode: 'GRD-G007', productName: 'Guard Assembly G',     inspectionType: 'Incoming',         aqlLevel: '1.5',  inspectionLevel: 'I',   checkpoints: 6,  isActive: false, version: 'Rev.A', updatedAt: '2026-02-14' },
];

const AQL_LEVELS = ['0.065', '0.1', '0.25', '0.65', '1.0', '1.5', '2.5', '4.0'];
const INSPECTION_LEVELS = ['I', 'II', 'III', 'S1', 'S2', 'S3', 'S4'] as const;

// ─── Page ─────────────────────────────────────────────────────────────────────

type DrawerMode = 'create' | 'edit';

export default function InspectionPlanPage() {
  const [rows, setRows]                 = useState<InspectionPlan[]>(MOCK_PLANS);
  const [search, setSearch]             = useState('');
  const [productFilter, setProductFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [selection, setSelection]       = useState<GridRowSelectionModel>({ type: 'include', ids: new Set() });
  const [drawerOpen, setDrawerOpen]     = useState(false);
  const [drawerMode, setDrawerMode]     = useState<DrawerMode>('create');
  const [editTarget, setEditTarget]     = useState<InspectionPlan | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<InspectionPlan | null>(null);
  const [saving, setSaving]             = useState(false);

  // Form state
  const [formProductCode, setFormProductCode]     = useState('');
  const [formProductName, setFormProductName]     = useState('');
  const [formInspectionType, setFormInspectionType] = useState('');
  const [formAqlLevel, setFormAqlLevel]           = useState('1.0');
  const [formLevel, setFormLevel]                 = useState<InspectionPlan['inspectionLevel']>('II');
  const [formCheckpoints, setFormCheckpoints]     = useState('');
  const [formVersion, setFormVersion]             = useState('Rev.A');
  const [formIsActive, setFormIsActive]           = useState(true);

  const productOptions = [...new Set(MOCK_PLANS.map((p) => p.productCode))].sort();

  const filtered = useMemo(() => {
    let r = rows;
    if (search)        r = r.filter((p) => p.planCode.toLowerCase().includes(search.toLowerCase()) || p.productCode.toLowerCase().includes(search.toLowerCase()) || p.productName.toLowerCase().includes(search.toLowerCase()));
    if (productFilter) r = r.filter((p) => p.productCode === productFilter);
    if (statusFilter)  r = r.filter((p) => statusFilter === 'active' ? p.isActive : !p.isActive);
    return r;
  }, [rows, search, productFilter, statusFilter]);

  function openCreate() {
    setDrawerMode('create');
    setEditTarget(null);
    setFormProductCode('');
    setFormProductName('');
    setFormInspectionType('');
    setFormAqlLevel('1.0');
    setFormLevel('II');
    setFormCheckpoints('');
    setFormVersion('Rev.A');
    setFormIsActive(true);
    setDrawerOpen(true);
  }

  function openEdit(p: InspectionPlan) {
    setDrawerMode('edit');
    setEditTarget(p);
    setFormProductCode(p.productCode);
    setFormProductName(p.productName);
    setFormInspectionType(p.inspectionType);
    setFormAqlLevel(p.aqlLevel);
    setFormLevel(p.inspectionLevel);
    setFormCheckpoints(String(p.checkpoints));
    setFormVersion(p.version);
    setFormIsActive(p.isActive);
    setDrawerOpen(true);
  }

  function handleSave() {
    setSaving(true);
    setTimeout(() => {
      const newRow: Partial<InspectionPlan> = {
        productCode: formProductCode,
        productName: formProductName,
        inspectionType: formInspectionType,
        aqlLevel: formAqlLevel,
        inspectionLevel: formLevel,
        checkpoints: Number(formCheckpoints) || 0,
        version: formVersion,
        isActive: formIsActive,
        updatedAt: new Date().toISOString().slice(0, 10),
      };
      if (drawerMode === 'create') {
        const nextNum = rows.length + 1;
        setRows((prev) => [
          ...prev,
          {
            id: String(Date.now()),
            planCode: `QP-${String(nextNum).padStart(3, '0')}`,
            ...newRow,
          } as InspectionPlan,
        ]);
      } else if (editTarget) {
        setRows((prev) => prev.map((p) => p.id === editTarget.id ? { ...p, ...newRow } : p));
      }
      setSaving(false);
      setDrawerOpen(false);
    }, 800);
  }

  function handleDelete() {
    if (deleteTarget) {
      setRows((prev) => prev.filter((p) => p.id !== deleteTarget.id));
      setDeleteTarget(null);
    }
  }

  const columns: GridColDef<InspectionPlan>[] = [
    {
      field: 'planCode',
      headerName: 'Plan Code',
      width: 105,
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'productCode',
      headerName: 'Product',
      width: 200,
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Stack spacing={0}>
          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 600, color: 'primary.main', lineHeight: 1.3 }}>
            {params.row.productCode}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11, lineHeight: 1.3 }}>
            {params.row.productName}
          </Typography>
        </Stack>
      ),
    },
    {
      field: 'inspectionType',
      headerName: 'Inspection Type',
      width: 175,
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Chip
          label={params.value}
          size="small"
          sx={(t) => ({
            height: 20, fontSize: '0.6875rem', fontWeight: 600,
            bgcolor: alpha(t.palette.primary.main, 0.08), color: 'primary.main',
            border: 'none', '& .MuiChip-label': { px: 0.75 },
          })}
        />
      ),
    },
    {
      field: 'aqlLevel',
      headerName: 'AQL',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Box
          component="span"
          sx={{
            fontFamily: 'ui-monospace, monospace', fontSize: 11, fontWeight: 700,
            px: 0.75, py: 0.25, borderRadius: 0.5,
            bgcolor: alpha('#7C3AED', 0.08), color: '#7C3AED',
          }}
        >
          {params.value}
        </Box>
      ),
    },
    {
      field: 'inspectionLevel',
      headerName: 'Level',
      width: 70,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600 }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'checkpoints',
      headerName: 'Checkpoints',
      width: 105,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Typography variant="body2" sx={{ fontSize: 12 }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'version',
      headerName: 'Version',
      width: 80,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11, color: 'text.secondary' }}>
          {params.value}
        </Typography>
      ),
    },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 90,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
        <Chip
          label={params.value ? 'Active' : 'Inactive'}
          size="small"
          sx={{
            height: 20, fontSize: '0.6875rem', fontWeight: 600,
            bgcolor: params.value ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
            color: params.value ? '#15803D' : '#94A3B8',
            border: 'none', '& .MuiChip-label': { px: 0.75 },
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
      renderCell: (params: GridRenderCellParams<InspectionPlan>) => (
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
        title="Inspection Plans"
        subtitle="Define AQL sampling plans and inspection checkpoints per product"
        breadcrumbs={[{ label: 'Quality' }, { label: 'Inspection Plans' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={openCreate}
          >
            New Plan
          </Button>
        }
      />

      <TableToolbar
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search plan code or product…"
        filters={[
          {
            label: 'Product',
            value: productFilter,
            options: productOptions.map((p) => ({ label: p, value: p })),
            onChange: setProductFilter,
          },
          {
            label: 'Status',
            value: statusFilter,
            options: [
              { label: 'Active',   value: 'active' },
              { label: 'Inactive', value: 'inactive' },
            ],
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
                icon={search || productFilter || statusFilter ? 'emptySearch' : 'emptyTable'}
                title={search || productFilter || statusFilter ? 'No plans match your filters' : 'No inspection plans yet'}
                description={
                  search || productFilter || statusFilter
                    ? 'Try adjusting your search or filters.'
                    : 'Create your first inspection plan to get started.'
                }
                action={
                  !search && !productFilter && !statusFilter ? (
                    <Button variant="contained" size="small" onClick={openCreate}>
                      New Plan
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
        title={drawerMode === 'create' ? 'New Inspection Plan' : `Edit ${editTarget?.planCode}`}
        subtitle={drawerMode === 'create' ? 'Define AQL sampling and inspection checkpoints' : editTarget?.productName}
        onSubmit={handleSave}
        submitLabel={drawerMode === 'create' ? 'Create Plan' : 'Save Changes'}
        loading={saving}
      >
        <Grid container spacing={2}>
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
              label="Product Name"
              value={formProductName}
              onChange={(e) => setFormProductName(e.target.value)}
              fullWidth
              required
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              label="Inspection Type"
              value={formInspectionType}
              onChange={(e) => setFormInspectionType(e.target.value)}
              fullWidth
              required
              placeholder="e.g. Final, Incoming, In-Process + Final"
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="AQL Level"
              value={formAqlLevel}
              onChange={(e) => setFormAqlLevel(e.target.value)}
              fullWidth
              required
            >
              {AQL_LEVELS.map((a) => (
                <MenuItem key={a} value={a}>{a}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              select
              label="Inspection Level"
              value={formLevel}
              onChange={(e) => setFormLevel(e.target.value as InspectionPlan['inspectionLevel'])}
              fullWidth
              required
            >
              {INSPECTION_LEVELS.map((l) => (
                <MenuItem key={l} value={l}>{l}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Checkpoints"
              type="number"
              value={formCheckpoints}
              onChange={(e) => setFormCheckpoints(e.target.value)}
              fullWidth
              required
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField
              label="Version"
              value={formVersion}
              onChange={(e) => setFormVersion(e.target.value)}
              fullWidth
              required
              placeholder="e.g. Rev.A"
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField
              select
              label="Status"
              value={formIsActive ? 'active' : 'inactive'}
              onChange={(e) => setFormIsActive(e.target.value === 'active')}
              fullWidth
            >
              <MenuItem value="active">Active</MenuItem>
              <MenuItem value="inactive">Inactive</MenuItem>
            </TextField>
          </Grid>
        </Grid>
      </FormDrawer>

      {/* Delete confirmation */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Inspection Plan"
        description={
          <>
            Delete plan <strong>{deleteTarget?.planCode}</strong> for <strong>{deleteTarget?.productCode}</strong>?
            This cannot be undone and may affect active inspection orders.
          </>
        }
        confirmLabel="Delete"
        confirmColor="error"
      />
    </PageRoot>
  );
}
