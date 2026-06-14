import {
  Box,
  Button,
  Chip,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  EmptyState,
  ExportButton,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import { useGetApiV1Inventory } from '../../api/inventory/inventory';
import type { InventoryStockDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const LOC_TYPE_COLOR: Record<string, string> = {
  RAW_MATERIAL:    '#0369A1',
  WIP:             '#7C3AED',
  FINISHED_GOODS:  '#15803D',
  QUARANTINE:      '#DC2626',
};

const LOC_TYPE_LABEL: Record<string, string> = {
  RAW_MATERIAL:   'Raw Material',
  WIP:            'WIP',
  FINISHED_GOODS: 'Finished Goods',
  QUARANTINE:     'Quarantine',
};

export default function InventoryPage() {
  const navigate = useNavigate();
  const [search, setSearch]           = useState('');
  const [typeFilter, setTypeFilter]   = useState('');

  const { data: response, isLoading, error, refetch } = useGetApiV1Inventory();
  const inventory: InventoryStockDto[] = response?.data ?? [];

  const locationTypes = useMemo(() => [...new Set(inventory.map((i) => i.locationType))], [inventory]);

  const filtered = useMemo(() => {
    let r = inventory;
    if (search) {
      const q = search.toLowerCase();
      r = r.filter((i) => i.productCode.toLowerCase().includes(q) || i.lotNumber.toLowerCase().includes(q) || i.locationCode.toLowerCase().includes(q));
    }
    if (typeFilter) r = r.filter((i) => i.locationType === typeFilter);
    return r;
  }, [inventory, search, typeFilter]);

  const numVal = (v: number | string) => typeof v === 'number' ? v : parseFloat(v);

  const columns: GridColDef<InventoryStockDto>[] = [
    {
      field: 'productCode', headerName: 'Product Code', width: 150,
      renderCell: (p: GridRenderCellParams<InventoryStockDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'lotNumber', headerName: 'Lot #', width: 160,
      renderCell: (p: GridRenderCellParams<InventoryStockDto>) => (
        <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12 }}>
          {p.value}
        </Typography>
      ),
    },
    {
      field: 'locationName', headerName: 'Location', flex: 1, minWidth: 150,
      renderCell: (p: GridRenderCellParams<InventoryStockDto>) => (
        <Stack>
          <Typography variant="body2" sx={{ fontSize: 12, fontWeight: 600 }}>{p.value}</Typography>
          <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 11 }}>{p.row.locationCode}</Typography>
        </Stack>
      ),
    },
    {
      field: 'locationType', headerName: 'Type', width: 130,
      renderCell: (p: GridRenderCellParams<InventoryStockDto>) => {
        const t = p.value as string;
        const color = LOC_TYPE_COLOR[t] ?? '#94A3B8';
        return (
          <Chip label={LOC_TYPE_LABEL[t] ?? t} size="small"
            sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600, bgcolor: alpha(color, 0.1), color, border: 'none', '& .MuiChip-label': { px: 0.75 } }} />
        );
      },
    },
    {
      field: 'quantity', headerName: 'Qty on Hand', width: 120, align: 'right', headerAlign: 'right',
      renderCell: (p: GridRenderCellParams<InventoryStockDto>) => {
        const qty = numVal(p.value as number | string);
        const low = qty < 10;
        return (
          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', justifyContent: 'flex-end' }}>
            {low && (
              <Tooltip title="Low stock">
                <Box component="span" sx={{ color: 'warning.main', display: 'flex', alignItems: 'center' }}>
                  <SolarIcon name="warning" size={14} />
                </Box>
              </Tooltip>
            )}
            <Typography variant="body2" sx={{ fontWeight: 600, color: low ? 'warning.main' : 'text.primary' }}>
              {qty.toLocaleString()}
            </Typography>
          </Stack>
        );
      },
    },
    {
      field: 'updatedAt', headerName: 'Updated', width: 100,
      renderCell: (p: GridRenderCellParams<InventoryStockDto>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>
          {p.value ? new Date(p.value as string).toLocaleDateString() : '—'}
        </Typography>
      ),
    },
    {
      field: 'actions', headerName: '', width: 80, sortable: false, align: 'center',
      renderCell: (p: GridRenderCellParams<InventoryStockDto>) => (
        <Stack direction="row" spacing={0.25}>
          <Tooltip title="Trace Lot">
            <IconButton size="small" onClick={() => navigate(`/warehouse/inventory/trace?lot=${p.row.lotNumber}`)} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="serial" size={16} />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Inventory" breadcrumbs={[{ label: 'Warehouse' }, { label: 'Inventory' }]} />
      <EmptyState icon="emptyTable" title="Failed to load inventory" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Inventory"
        subtitle="Current stock levels by product, lot and location"
        breadcrumbs={[{ label: 'Warehouse' }, { label: 'Inventory' }]}
        actions={
          <Button variant="outlined" size="small" startIcon={<SolarIcon name="serial" size={16} />}
            onClick={() => navigate('/warehouse/inventory/trace')}>
            Trace Lot
          </Button>
        }
      />

      <TableToolbar
        search={search} onSearchChange={setSearch} searchPlaceholder="Search product, lot or location…"
        filters={[
          { label: 'Type', value: typeFilter, options: locationTypes.map((t) => ({ label: LOC_TYPE_LABEL[t] ?? t, value: t })), onChange: setTypeFilter },
        ]}
        totalCount={filtered.length}
        actions={
          <Stack direction="row" spacing={0.5}>
            <ExportButton />
            <RefreshButton onClick={() => refetch()} />
          </Stack>
        }
      />

      <Box sx={{ flex: 1, minHeight: 400 }}>
        <DataGrid
          rows={filtered}
          getRowId={(r) => r.stockID}
          columns={columns}
          density="compact"
          disableRowSelectionOnClick
          pageSizeOptions={[10, 25, 50]}
          initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon={search || typeFilter ? 'emptySearch' : 'emptyTable'}
                title={search || typeFilter ? 'No stock matches your filters' : 'No inventory records'}
                description={search || typeFilter ? 'Try adjusting your search or filters.' : 'Receive goods via the GRN module to populate inventory.'}
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
