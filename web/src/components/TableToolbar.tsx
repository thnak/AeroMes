import {
  Box,
  Button,
  Divider,
  InputAdornment,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import type { ReactNode, ChangeEvent } from 'react';
import SolarIcon from './SolarIcon';

// ─── TableToolbar ─────────────────────────────────────────────────────────────
//
//  Standard data-page toolbar. Lives between <PageHeader> and the DataGrid.
//
//  Layout:
//  [ Search input ]  [ Filter 1 ▾ ]  [ Filter 2 ▾ ]  ──  [ Export ]  [ Refresh ]
//
//  All elements are optional. Renders nothing if all are undefined.

interface FilterOption {
  label: string;
  value: string;
}

interface FilterConfig {
  label: string;
  value: string;
  options: FilterOption[];
  onChange: (value: string) => void;
  width?: number;
}

interface TableToolbarProps {
  /** Controlled search value */
  search?: string;
  onSearchChange?: (value: string) => void;
  searchPlaceholder?: string;
  /** Dropdown filters */
  filters?: FilterConfig[];
  /** Right-side action buttons */
  actions?: ReactNode;
  /** Total count shown as helper text (e.g. "128 records") */
  totalCount?: number;
}

export default function TableToolbar({
  search,
  onSearchChange,
  searchPlaceholder = 'Search…',
  filters,
  actions,
  totalCount,
}: TableToolbarProps) {
  const hasLeft = search !== undefined || (filters && filters.length > 0);
  const hasRight = !!actions || totalCount !== undefined;

  if (!hasLeft && !hasRight) return null;

  return (
    <Stack
      direction="row"
      spacing={1.5}
      sx={{ alignItems: 'center', flexWrap: 'wrap', mb: 2, gap: 1 }}
    >
      {/* Search */}
      {search !== undefined && onSearchChange && (
        <TextField
          value={search}
          onChange={(e: ChangeEvent<HTMLInputElement>) => onSearchChange(e.target.value)}
          placeholder={searchPlaceholder}
          size="small"
          sx={{ width: 240, flexShrink: 0 }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SolarIcon name="search" size={16} color="text.disabled" />
                </InputAdornment>
              ),
              ...(search && {
                endAdornment: (
                  <InputAdornment position="end">
                    <Box
                      component="span"
                      onClick={() => onSearchChange('')}
                      sx={{ cursor: 'pointer', color: 'text.disabled', display: 'flex', '&:hover': { color: 'text.secondary' } }}
                    >
                      <SolarIcon name="close" size={14} />
                    </Box>
                  </InputAdornment>
                ),
              }),
            },
          }}
        />
      )}

      {/* Dropdown filters */}
      {filters?.map((f) => (
        <Select
          key={f.label}
          value={f.value}
          onChange={(e) => f.onChange(e.target.value as string)}
          size="small"
          displayEmpty
          sx={{ width: f.width ?? 160, flexShrink: 0 }}
        >
          <MenuItem value="">
            <Typography variant="body2" color="text.secondary">
              {f.label}: All
            </Typography>
          </MenuItem>
          {f.options.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>
              <Typography variant="body2">{opt.label}</Typography>
            </MenuItem>
          ))}
        </Select>
      ))}

      {/* Spacer */}
      <Box sx={{ flex: 1 }} />

      {/* Count */}
      {totalCount !== undefined && (
        <Typography variant="caption" color="text.disabled" sx={{ flexShrink: 0 }}>
          {totalCount.toLocaleString()} records
        </Typography>
      )}

      {/* Right actions */}
      {actions && (
        <>
          {totalCount !== undefined && <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />}
          {actions}
        </>
      )}
    </Stack>
  );
}

// ─── Common toolbar action buttons ───────────────────────────────────────────

export function ExportButton({ onClick }: { onClick?: () => void }) {
  return (
    <Tooltip title="Export to Excel">
      <Button
        size="small"
        variant="outlined"
        startIcon={<SolarIcon name="export" size={16} />}
        onClick={onClick}
        sx={{ flexShrink: 0 }}
      >
        Export
      </Button>
    </Tooltip>
  );
}

export function RefreshButton({ onClick, loading = false }: { onClick?: () => void; loading?: boolean }) {
  return (
    <Tooltip title="Refresh">
      <Button
        size="small"
        variant="text"
        onClick={onClick}
        disabled={loading}
        sx={{ minWidth: 0, px: 1, color: 'text.secondary' }}
      >
        <SolarIcon name="refresh" size={18} />
      </Button>
    </Tooltip>
  );
}
