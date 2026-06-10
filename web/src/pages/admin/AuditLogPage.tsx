import { useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import type { GridPaginationModel } from '@mui/x-data-grid';
import dayjs, { type Dayjs } from 'dayjs';
import { Icon } from '@iconify/react';
import PageHeader, { PageRoot } from '../../components/PageHeader';
import { useGetApiV1AuditLog } from '../../api/audit-log/audit-log';
import type { SecurityAuditLog } from '../../api/model/securityAuditLog';
import type { GetApiV1AuditLogParams } from '../../api/model/getApiV1AuditLogParams';
import { apiClient, getErrorMessage } from '../../lib/apiClient';

// ─── Constants ────────────────────────────────────────────────────────────────

const EVENT_TYPE_OPTIONS = [
  'AuthLoginSuccess',
  'AuthLoginFailure',
  'AuthMfaSuccess',
  'AuthMfaFailure',
  'AuthMfaSetup',
  'AuthMfaDisabled',
  'AuthPasswordChanged',
  'AuthLogout',
  'AuthTokenRefresh',
  'AuthTokenReuseAttack',
  'UserCreated',
  'UserDeactivated',
  'UserActivated',
  'RoleAssigned',
  'RolePermissionChanged',
] as const;

const TARGET_TYPE_OPTIONS = [
  'User',
  'Role',
  'Permission',
  'Session',
  'MfaDevice',
] as const;

const OUTCOME_OPTIONS = ['SUCCESS', 'FAILURE'] as const;

// ─── Helpers ──────────────────────────────────────────────────────────────────

function getEventTypeColor(
  eventType: string | undefined,
): 'error' | 'success' | 'default' {
  if (!eventType) return 'default';
  if (eventType.includes('Failure') || eventType.includes('Attack')) return 'error';
  if (
    eventType.includes('Success') ||
    (eventType.includes('Login') && !eventType.includes('Failure'))
  )
    return 'success';
  return 'default';
}

function getOutcomeColor(outcome: string | undefined): 'success' | 'error' | 'default' {
  if (!outcome) return 'default';
  if (outcome === 'SUCCESS') return 'success';
  if (outcome === 'FAILURE') return 'error';
  return 'default';
}

function buildExportUrl(from?: string, to?: string): string {
  const base = (apiClient.defaults.baseURL ?? '/api').replace(/\/$/, '');
  const params = new URLSearchParams();
  if (from) params.set('from', from);
  if (to) params.set('to', to);
  const qs = params.toString();
  return `${base}/v1/audit-log/export${qs ? `?${qs}` : ''}`;
}

function formatJson(raw: string | null | undefined): string {
  if (!raw) return '';
  try {
    return JSON.stringify(JSON.parse(raw), null, 2);
  } catch {
    return raw;
  }
}

// ─── Detail dialog ────────────────────────────────────────────────────────────

interface DetailDialogProps {
  row: SecurityAuditLog | null;
  onClose: () => void;
}

function DetailDialog({ row, onClose }: DetailDialogProps) {
  if (!row) return null;

  const hasChanges = row.oldValues || row.newValues;

  return (
    <Dialog open onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h6">Audit Entry Detail</Typography>
        <IconButton size="small" onClick={onClose}>
          <Icon icon="solar:close-circle-linear" width={20} />
        </IconButton>
      </DialogTitle>
      <DialogContent dividers>
        <Stack spacing={2}>
          {/* Meta row */}
          <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap' }}>
            <Box>
              <Typography variant="caption" color="text.secondary">Audit ID</Typography>
              <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                {row.auditId ?? '—'}
              </Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary">Occurred At</Typography>
              <Typography variant="body2">
                {row.occurredAt ? dayjs(row.occurredAt).format('MMM D, YYYY HH:mm:ss') : '—'}
              </Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary">Outcome</Typography>
              <Box sx={{ mt: 0.25 }}>
                <Chip
                  label={row.outcome ?? '—'}
                  color={getOutcomeColor(row.outcome)}
                  size="small"
                />
              </Box>
            </Box>
          </Stack>

          <Divider />

          {/* Event */}
          <Box>
            <Typography variant="caption" color="text.secondary">Event Type</Typography>
            <Box sx={{ mt: 0.25 }}>
              <Chip
                label={
                  <Typography component="span" sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>
                    {row.eventType ?? '—'}
                  </Typography>
                }
                color={getEventTypeColor(row.eventType)}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>

          <Divider />

          {/* Actor */}
          <Box>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>Actor</Typography>
            <Stack direction="row" spacing={3} sx={{ flexWrap: 'wrap' }}>
              <Box>
                <Typography variant="caption" color="text.secondary">Type</Typography>
                <Typography variant="body2">{row.actorType ?? '—'}</Typography>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">ID</Typography>
                <Typography variant="body2" sx={{ fontFamily: 'monospace', wordBreak: 'break-all' }}>
                  {row.actorId ?? '—'}
                </Typography>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">IP Address</Typography>
                <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                  {row.actorIp ?? '—'}
                </Typography>
              </Box>
            </Stack>
            {row.actorUserAgent && (
              <Box sx={{ mt: 1 }}>
                <Typography variant="caption" color="text.secondary">User Agent</Typography>
                <Typography
                  variant="body2"
                  sx={{ wordBreak: 'break-all', color: 'text.secondary', fontSize: '0.75rem' }}
                >
                  {row.actorUserAgent}
                </Typography>
              </Box>
            )}
          </Box>

          {/* Target */}
          {(row.targetType || row.targetId) && (
            <>
              <Divider />
              <Box>
                <Typography variant="subtitle2" sx={{ mb: 1 }}>Target</Typography>
                <Stack direction="row" spacing={3}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Type</Typography>
                    <Typography variant="body2">{row.targetType ?? '—'}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">ID</Typography>
                    <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                      {row.targetId ?? '—'}
                    </Typography>
                  </Box>
                </Stack>
              </Box>
            </>
          )}

          {/* Failure reason */}
          {row.failureReason && (
            <>
              <Divider />
              <Box>
                <Typography variant="caption" color="text.secondary">Failure Reason</Typography>
                <Typography variant="body2" color="error.main">
                  {row.failureReason}
                </Typography>
              </Box>
            </>
          )}

          {/* Old / New values */}
          {hasChanges && (
            <>
              <Divider />
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                {row.oldValues && (
                  <Box sx={{ flex: 1 }}>
                    <Typography variant="subtitle2" sx={{ mb: 0.5 }}>Old Values</Typography>
                    <Paper
                      variant="outlined"
                      sx={{
                        p: 1.5,
                        bgcolor: 'error.50',
                        borderColor: 'error.200',
                        overflowX: 'auto',
                      }}
                    >
                      <Typography
                        component="pre"
                        variant="caption"
                        sx={{
                          fontFamily: 'monospace',
                          whiteSpace: 'pre-wrap',
                          wordBreak: 'break-all',
                          m: 0,
                          color: 'text.primary',
                        }}
                      >
                        {formatJson(row.oldValues)}
                      </Typography>
                    </Paper>
                  </Box>
                )}
                {row.newValues && (
                  <Box sx={{ flex: 1 }}>
                    <Typography variant="subtitle2" sx={{ mb: 0.5 }}>New Values</Typography>
                    <Paper
                      variant="outlined"
                      sx={{
                        p: 1.5,
                        bgcolor: 'success.50',
                        borderColor: 'success.200',
                        overflowX: 'auto',
                      }}
                    >
                      <Typography
                        component="pre"
                        variant="caption"
                        sx={{
                          fontFamily: 'monospace',
                          whiteSpace: 'pre-wrap',
                          wordBreak: 'break-all',
                          m: 0,
                          color: 'text.primary',
                        }}
                      >
                        {formatJson(row.newValues)}
                      </Typography>
                    </Paper>
                  </Box>
                )}
              </Stack>
            </>
          )}
        </Stack>
      </DialogContent>
    </Dialog>
  );
}

// ─── Column definitions ───────────────────────────────────────────────────────

function buildColumns(onDetail: (row: SecurityAuditLog) => void): GridColDef<SecurityAuditLog>[] {
  return [
    {
      field: 'occurredAt',
      headerName: 'Occurred At',
      width: 180,
      valueFormatter: (value: string | undefined) =>
        value ? dayjs(value).format('MMM D, YYYY HH:mm:ss') : '—',
    },
    {
      field: 'eventType',
      headerName: 'Event Type',
      width: 220,
      renderCell: ({ value }) => {
        const color = getEventTypeColor(value as string | undefined);
        return (
          <Chip
            label={
              <Typography
                component="span"
                sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}
              >
                {value ?? '—'}
              </Typography>
            }
            color={color}
            size="small"
            variant="outlined"
          />
        );
      },
    },
    {
      field: 'actor',
      headerName: 'Actor',
      width: 200,
      sortable: false,
      valueGetter: (_value: unknown, row: SecurityAuditLog) => {
        const type = row.actorType ?? '';
        const id = row.actorId ?? '';
        if (!type && !id) return '—';
        return `${type} / ${id}`;
      },
      renderCell: ({ value }) => (
        <Typography
          variant="body2"
          sx={{
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            maxWidth: '100%',
          }}
          title={value as string}
        >
          {value}
        </Typography>
      ),
    },
    {
      field: 'actorIp',
      headerName: 'IP Address',
      width: 140,
      renderCell: ({ value }) => (
        <Typography
          variant="body2"
          sx={{ fontFamily: 'monospace' }}
          color={value ? 'text.primary' : 'text.disabled'}
        >
          {value ?? '—'}
        </Typography>
      ),
    },
    {
      field: 'target',
      headerName: 'Target',
      width: 200,
      sortable: false,
      valueGetter: (_value: unknown, row: SecurityAuditLog) => {
        const type = row.targetType ?? '';
        const id = row.targetId ?? '';
        if (!type && !id) return '';
        return `${type} / ${id}`;
      },
      renderCell: ({ value }) =>
        value ? (
          <Typography
            variant="body2"
            sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              maxWidth: '100%',
            }}
            title={value as string}
          >
            {value}
          </Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">
            —
          </Typography>
        ),
    },
    {
      field: 'outcome',
      headerName: 'Outcome',
      width: 110,
      renderCell: ({ value }) => {
        const color = getOutcomeColor(value as string | undefined);
        return value ? (
          <Chip label={value} color={color} size="small" />
        ) : (
          <Typography variant="body2" color="text.disabled">
            —
          </Typography>
        );
      },
    },
    {
      field: 'failureReason',
      headerName: 'Failure Reason',
      flex: 1,
      minWidth: 160,
      renderCell: ({ value }) => (
        <Typography
          variant="body2"
          color={value ? 'text.primary' : 'text.disabled'}
          sx={{
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            maxWidth: '100%',
          }}
          title={value ?? undefined}
        >
          {value ?? '—'}
        </Typography>
      ),
    },
    {
      field: '__detail',
      headerName: '',
      width: 48,
      sortable: false,
      disableColumnMenu: true,
      renderCell: ({ row }) => (
        <Tooltip title="View details">
          <IconButton size="small" onClick={() => onDetail(row)}>
            <Icon icon="solar:eye-linear" width={16} />
          </IconButton>
        </Tooltip>
      ),
    },
  ];
}

// ─── Filter draft state ───────────────────────────────────────────────────────

interface FilterDraft {
  eventType: string;
  from: Dayjs | null;
  to: Dayjs | null;
  actorId: string;
  targetType: string;
  outcome: string;
}

const DEFAULT_DRAFT: FilterDraft = {
  eventType: '',
  from: null,
  to: null,
  actorId: '',
  targetType: '',
  outcome: '',
};

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function AuditLogPage() {
  const [draft, setDraft] = useState<FilterDraft>(DEFAULT_DRAFT);
  const [filters, setFilters] = useState<GetApiV1AuditLogParams>({
    page: 1,
    pageSize: 50,
  });
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    page: 0,
    pageSize: 50,
  });
  const [selectedRow, setSelectedRow] = useState<SecurityAuditLog | null>(null);

  const { data, isLoading, isError, error } = useGetApiV1AuditLog({
    ...filters,
    page: paginationModel.page + 1,
    pageSize: paginationModel.pageSize,
  });

  const rows: SecurityAuditLog[] = data?.items ?? [];
  const total =
    typeof data?.total === 'string' ? parseInt(data.total, 10) : (data?.total ?? 0);

  function applyFilters() {
    setFilters({
      page: 1,
      pageSize: paginationModel.pageSize,
      eventType: draft.eventType || undefined,
      from: draft.from ? draft.from.toISOString() : undefined,
      to: draft.to ? draft.to.toISOString() : undefined,
      actorId: draft.actorId || undefined,
      targetType: draft.targetType || undefined,
      outcome: draft.outcome || undefined,
    });
    setPaginationModel((prev) => ({ ...prev, page: 0 }));
  }

  function clearFilters() {
    setDraft(DEFAULT_DRAFT);
    setFilters({ page: 1, pageSize: paginationModel.pageSize });
    setPaginationModel((prev) => ({ ...prev, page: 0 }));
  }

  const exportUrl = buildExportUrl(filters.from, filters.to);
  const columns = buildColumns(setSelectedRow);

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <PageRoot>
        <PageHeader
          title="Audit Log"
          subtitle="Immutable security event log — all actor activity across the platform"
          breadcrumbs={[{ label: 'Admin' }, { label: 'Audit Log' }]}
        />

        {/* ── Filter Toolbar ─────────────────────────────────────────────── */}
        <Paper sx={{ p: 2, mb: 2 }} variant="outlined">
          <Stack spacing={2}>
            {/* Row 1 */}
            <Stack
              direction="row"
              spacing={2}
              sx={{ flexWrap: 'wrap', alignItems: 'center' }}
            >
              <FormControl size="small" sx={{ minWidth: 210 }}>
                <InputLabel id="audit-event-type-label">Event Type</InputLabel>
                <Select
                  labelId="audit-event-type-label"
                  label="Event Type"
                  value={draft.eventType}
                  onChange={(e) =>
                    setDraft((prev) => ({ ...prev, eventType: e.target.value }))
                  }
                >
                  <MenuItem value="">
                    <em>All</em>
                  </MenuItem>
                  {EVENT_TYPE_OPTIONS.map((opt) => (
                    <MenuItem key={opt} value={opt}>
                      {opt}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 140 }}>
                <InputLabel id="audit-outcome-label">Outcome</InputLabel>
                <Select
                  labelId="audit-outcome-label"
                  label="Outcome"
                  value={draft.outcome}
                  onChange={(e) =>
                    setDraft((prev) => ({ ...prev, outcome: e.target.value }))
                  }
                >
                  <MenuItem value="">
                    <em>All</em>
                  </MenuItem>
                  {OUTCOME_OPTIONS.map((opt) => (
                    <MenuItem key={opt} value={opt}>
                      {opt}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <DatePicker
                label="From"
                value={draft.from}
                onChange={(val) => setDraft((prev) => ({ ...prev, from: val }))}
                slotProps={{ textField: { size: 'small' } }}
              />

              <DatePicker
                label="To"
                value={draft.to}
                onChange={(val) => setDraft((prev) => ({ ...prev, to: val }))}
                slotProps={{ textField: { size: 'small' } }}
              />

              <TextField
                label="Actor ID"
                size="small"
                value={draft.actorId}
                onChange={(e) =>
                  setDraft((prev) => ({ ...prev, actorId: e.target.value }))
                }
                sx={{ minWidth: 160 }}
              />

              <FormControl size="small" sx={{ minWidth: 160 }}>
                <InputLabel id="audit-target-type-label">Target Type</InputLabel>
                <Select
                  labelId="audit-target-type-label"
                  label="Target Type"
                  value={draft.targetType}
                  onChange={(e) =>
                    setDraft((prev) => ({ ...prev, targetType: e.target.value }))
                  }
                >
                  <MenuItem value="">
                    <em>All</em>
                  </MenuItem>
                  {TARGET_TYPE_OPTIONS.map((opt) => (
                    <MenuItem key={opt} value={opt}>
                      {opt}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <Button variant="contained" size="small" onClick={applyFilters}>
                Apply
              </Button>
            </Stack>

            {/* Row 2 */}
            <Stack direction="row" spacing={1}>
              <Button variant="outlined" size="small" onClick={clearFilters}>
                Clear Filters
              </Button>
              <Button
                variant="outlined"
                size="small"
                component="a"
                href={exportUrl}
                download
              >
                Export CSV
              </Button>
            </Stack>
          </Stack>
        </Paper>

        {/* ── Error banner ───────────────────────────────────────────────── */}
        {isError && (
          <Box sx={{ mb: 2 }}>
            <Typography color="error" variant="body2">
              {getErrorMessage(error, 'Failed to load audit log.')}
            </Typography>
          </Box>
        )}

        {/* ── Data Grid ──────────────────────────────────────────────────── */}
        <Box sx={{ flex: 1, minHeight: 0 }}>
          <DataGrid<SecurityAuditLog>
            rows={rows}
            columns={columns}
            getRowId={(row) =>
              row.auditId != null ? String(row.auditId) : Math.random().toString()
            }
            rowCount={total}
            loading={isLoading}
            paginationMode="server"
            paginationModel={paginationModel}
            onPaginationModelChange={setPaginationModel}
            pageSizeOptions={[25, 50, 100]}
            disableRowSelectionOnClick
            density="compact"
            sx={{
              height: '100%',
              minHeight: 400,
              '& .MuiDataGrid-cell': { alignItems: 'center' },
            }}
          />
        </Box>
      </PageRoot>

      {/* ── Detail dialog ──────────────────────────────────────────────────── */}
      {selectedRow && (
        <DetailDialog row={selectedRow} onClose={() => setSelectedRow(null)} />
      )}
    </LocalizationProvider>
  );
}
