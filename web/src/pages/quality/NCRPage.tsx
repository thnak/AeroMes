import {
  Alert,
  Box,
  Chip,
  Divider,
  Drawer,
  IconButton,
  MenuItem,
  Select,
  Stack,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ConfirmDialog,
  EmptyState,
  FormDrawer,
  PageHeader,
  PageRoot,
  RefreshButton,
  SolarIcon,
  TablePageSkeleton,
  TableToolbar,
} from '../../components';
import {
  useGetApiV1QualityNcrs,
  getGetApiV1QualityNcrsQueryKey,
  postApiV1QualityNcrs,
  patchApiV1QualityNcrsIdDisposition,
  patchApiV1QualityNcrsIdEscalate,
  patchApiV1QualityNcrsIdClose,
  patchApiV1QualityNcrsIdCancel,
  patchApiV1QualityNcrsId,
  useGetApiV1QualityNcrsId,
} from '../../api/quality/quality';
import type { NcrListDto } from '../../api/model';
import { getErrorMessage } from '../../lib/apiClient';

const STATUS_CFG: Record<string, { label: string; color: string }> = {
  OPEN:             { label: 'Open',            color: '#DC2626' },
  UNDER_REVIEW:     { label: 'Under Review',    color: '#D97706' },
  DISPOSITION_SET:  { label: 'Disposition Set', color: '#4F46E5' },
  CLOSED:           { label: 'Closed',          color: '#15803D' },
  CANCELLED:        { label: 'Cancelled',       color: '#64748B' },
};

const SEVERITY_CFG: Record<string, { label: string; color: string }> = {
  CRITICAL: { label: 'Critical', color: '#DC2626' },
  MAJOR:    { label: 'Major',    color: '#D97706' },
  MINOR:    { label: 'Minor',    color: '#64748B' },
};

const DISPOSITIONS = [
  { value: 'REWORK',             label: 'Rework' },
  { value: 'SCRAP',              label: 'Scrap' },
  { value: 'USE_AS_IS',          label: 'Use As-Is' },
  { value: 'RETURN_TO_SUPPLIER', label: 'Return to Supplier' },
  { value: 'RE_INSPECT',         label: 'Re-Inspect' },
];

const CreateSchema = z.object({
  workOrderId:  z.coerce.number().int().min(1, 'Required'),
  productCode:  z.string().min(1, 'Required').max(50),
  lotNumber:    z.string().max(100).optional(),
  qtyAffected:  z.coerce.number().positive('Must be > 0'),
  severity:     z.enum(['CRITICAL', 'MAJOR', 'MINOR']),
});
type CreateForm = z.infer<typeof CreateSchema>;

const DispositionSchema = z.object({
  dispositionCode: z.string().min(1, 'Required'),
  setBy:           z.string().min(1, 'Required').max(100),
});
type DispositionForm = z.infer<typeof DispositionSchema>;

const CloseSchema = z.object({
  closedBy:          z.string().min(1, 'Required').max(100),
  rootCause:         z.string().max(500).optional(),
  correctiveAction:  z.string().max(500).optional(),
  preventiveAction:  z.string().max(500).optional(),
});
type CloseForm = z.infer<typeof CloseSchema>;

const UpdateSchema = z.object({
  assignedTo: z.string().max(100).optional(),
  dueDate:    z.string().optional(),
});
type UpdateForm = z.infer<typeof UpdateSchema>;

export default function NcrPage() {
  const queryClient = useQueryClient();

  const [search, setSearch]               = useState('');
  const [statusFilter, setStatusFilter]   = useState('');
  const [createOpen, setCreateOpen]       = useState(false);
  const [dispositionTarget, setDispositionTarget] = useState<NcrListDto | null>(null);
  const [closeTarget, setCloseTarget]     = useState<NcrListDto | null>(null);
  const [cancelTarget, setCancelTarget]   = useState<NcrListDto | null>(null);
  const [escalateTarget, setEscalateTarget] = useState<NcrListDto | null>(null);
  const [detailId, setDetailId]           = useState<number | null>(null);
  const [updateTarget, setUpdateTarget]   = useState<NcrListDto | null>(null);
  const [toastError, setToastError]       = useState<string | null>(null);

  const queryParams = statusFilter ? { status: statusFilter } : undefined;
  const { data: ncrs = [], isLoading, error, refetch } = useGetApiV1QualityNcrs(queryParams);
  const { data: detail } = useGetApiV1QualityNcrsId(detailId ?? 0, {
    query: { enabled: detailId !== null },
  });

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: getGetApiV1QualityNcrsQueryKey(queryParams) });

  const filtered = search
    ? ncrs.filter((n) =>
        n.ncrNo.toLowerCase().includes(search.toLowerCase()) ||
        n.productCode.toLowerCase().includes(search.toLowerCase()) ||
        (n.assignedTo ?? '').toLowerCase().includes(search.toLowerCase()))
    : ncrs;

  const createForm = useForm<CreateForm>({
    resolver: zodResolver(CreateSchema) as any,
    defaultValues: { severity: 'MAJOR', qtyAffected: 1 },
  });
  const dispositionForm = useForm<DispositionForm>({
    resolver: zodResolver(DispositionSchema) as any,
    defaultValues: { dispositionCode: '', setBy: '' },
  });
  const closeForm = useForm<CloseForm>({
    resolver: zodResolver(CloseSchema) as any,
    defaultValues: { closedBy: '' },
  });
  const updateForm = useForm<UpdateForm>({
    resolver: zodResolver(UpdateSchema) as any,
    defaultValues: { assignedTo: '', dueDate: '' },
  });

  const createMutation = useMutation({
    mutationFn: (v: CreateForm) =>
      postApiV1QualityNcrs({
        workOrderId: v.workOrderId,
        productCode: v.productCode,
        lotNumber: v.lotNumber ?? null,
        qtyAffected: v.qtyAffected,
        severity: v.severity,
      }),
    onSuccess: () => { invalidate(); setCreateOpen(false); createForm.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const dispositionMutation = useMutation({
    mutationFn: ({ id, v }: { id: number; v: DispositionForm }) =>
      patchApiV1QualityNcrsIdDisposition(id, { dispositionCode: v.dispositionCode, setBy: v.setBy }),
    onSuccess: () => { invalidate(); setDispositionTarget(null); dispositionForm.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const escalateMutation = useMutation({
    mutationFn: (id: number) => patchApiV1QualityNcrsIdEscalate(id),
    onSuccess:  () => { invalidate(); setEscalateTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const closeMutation = useMutation({
    mutationFn: ({ id, v }: { id: number; v: CloseForm }) =>
      patchApiV1QualityNcrsIdClose(id, {
        closedBy: v.closedBy,
        rootCause: v.rootCause ?? null,
        correctiveAction: v.correctiveAction ?? null,
        preventiveAction: v.preventiveAction ?? null,
      }),
    onSuccess: () => { invalidate(); setCloseTarget(null); closeForm.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: number) => patchApiV1QualityNcrsIdCancel(id, { cancelledBy: 'system' }),
    onSuccess:  () => { invalidate(); setCancelTarget(null); },
    onError:    (e) => setToastError(getErrorMessage(e)),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, v }: { id: number; v: UpdateForm }) =>
      patchApiV1QualityNcrsId(id, {
        assignedTo: v.assignedTo ?? null,
        dueDate: v.dueDate ? v.dueDate : null,
      }),
    onSuccess: () => { invalidate(); setUpdateTarget(null); updateForm.reset(); },
    onError:   (e) => setToastError(getErrorMessage(e)),
  });

  const columns: GridColDef<NcrListDto>[] = [
    {
      field: 'ncrNo',
      headerName: 'NCR No.',
      width: 150,
      renderCell: (p: GridRenderCellParams<NcrListDto>) => (
        <Typography
          variant="body2"
          sx={{ fontFamily: 'monospace', fontWeight: 600, cursor: 'pointer', color: 'primary.main' }}
          onClick={() => setDetailId(Number(p.row.ncrId))}
        >
          {p.value as string}
        </Typography>
      ),
    },
    { field: 'productCode', headerName: 'Product', width: 110 },
    {
      field: 'severity',
      headerName: 'Severity',
      width: 100,
      renderCell: (p: GridRenderCellParams<NcrListDto>) => {
        const cfg = SEVERITY_CFG[p.value as string] ?? { label: p.value as string, color: '#64748B' };
        return (
          <Chip
            label={cfg.label}
            size="small"
            sx={{
              height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha(cfg.color, 0.1), color: cfg.color, border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 140,
      renderCell: (p: GridRenderCellParams<NcrListDto>) => {
        const cfg = STATUS_CFG[p.value as string] ?? { label: p.value as string, color: '#64748B' };
        return (
          <Chip
            label={cfg.label}
            size="small"
            sx={{
              height: 20, fontSize: '0.6875rem', fontWeight: 600,
              bgcolor: alpha(cfg.color, 0.1), color: cfg.color, border: 'none',
              '& .MuiChip-label': { px: 0.75 },
            }}
          />
        );
      },
    },
    {
      field: 'dispositionCode',
      headerName: 'Disposition',
      width: 160,
      renderCell: (p: GridRenderCellParams<NcrListDto>) =>
        p.value ? (
          <Typography variant="body2" sx={{ fontSize: '0.8125rem' }}>
            {DISPOSITIONS.find((d) => d.value === p.value)?.label ?? (p.value as string)}
          </Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">—</Typography>
        ),
    },
    {
      field: 'qtyAffected',
      headerName: 'Qty',
      width: 70,
      renderCell: (p: GridRenderCellParams<NcrListDto>) => (
        <Typography variant="body2">{Number(p.value)}</Typography>
      ),
    },
    {
      field: 'assignedTo',
      headerName: 'Assigned',
      flex: 1,
      renderCell: (p: GridRenderCellParams<NcrListDto>) =>
        p.value ? (
          <Typography variant="body2">{p.value as string}</Typography>
        ) : (
          <Typography variant="body2" color="text.disabled">—</Typography>
        ),
    },
    {
      field: 'createdAt',
      headerName: 'Created',
      width: 155,
      renderCell: (p: GridRenderCellParams<NcrListDto>) => (
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.8125rem' }}>
          {new Date(p.value as string).toLocaleString()}
        </Typography>
      ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 180,
      sortable: false,
      align: 'center',
      renderCell: (p: GridRenderCellParams<NcrListDto>) => {
        const id  = Number(p.row.ncrId);
        const st  = p.row.status;
        return (
          <Stack direction="row" spacing={0.25}>
            {st === 'OPEN' && (
              <Tooltip title="Escalate to Under Review">
                <IconButton size="small" onClick={() => setEscalateTarget(p.row)} sx={{ color: '#D97706' }}>
                  <SolarIcon name="warning" size={15} />
                </IconButton>
              </Tooltip>
            )}
            {(st === 'OPEN' || st === 'UNDER_REVIEW') && (
              <Tooltip title="Set Disposition">
                <IconButton size="small"
                  onClick={() => { dispositionForm.reset({ dispositionCode: '', setBy: '' }); setDispositionTarget(p.row); }}
                  sx={{ color: '#4F46E5' }}>
                  <SolarIcon name="complete" size={15} />
                </IconButton>
              </Tooltip>
            )}
            {st === 'DISPOSITION_SET' && (
              <Tooltip title="Close NCR">
                <IconButton size="small"
                  onClick={() => { closeForm.reset({ closedBy: '' }); setCloseTarget(p.row); }}
                  sx={{ color: '#15803D' }}>
                  <SolarIcon name="complete" size={15} />
                </IconButton>
              </Tooltip>
            )}
            {(st === 'OPEN' || st === 'UNDER_REVIEW') && (
              <Tooltip title="Assign / Due Date">
                <IconButton size="small"
                  onClick={() => {
                    updateForm.reset({ assignedTo: p.row.assignedTo ?? '', dueDate: p.row.dueDate ?? '' });
                    setUpdateTarget(p.row);
                  }}
                  sx={{ color: '#0891B2' }}>
                  <SolarIcon name="operator" size={15} />
                </IconButton>
              </Tooltip>
            )}
            {st !== 'CLOSED' && st !== 'CANCELLED' && (
              <Tooltip title="Cancel NCR">
                <IconButton size="small" onClick={() => setCancelTarget(p.row)} sx={{ color: '#DC2626' }}>
                  <SolarIcon name="cancel" size={15} />
                </IconButton>
              </Tooltip>
            )}
            <Tooltip title="View Details">
              <IconButton size="small" onClick={() => setDetailId(id)} sx={{ color: '#64748B' }}>
                <SolarIcon name="view" size={15} />
              </IconButton>
            </Tooltip>
          </Stack>
        );
      },
    },
  ];

  if (isLoading) return <TablePageSkeleton />;
  if (error) return (
    <PageRoot>
      <PageHeader title="Non-Conformance Reports" breadcrumbs={[{ label: 'Quality' }, { label: 'NCR' }]} />
      <EmptyState icon="emptyTable" title="Failed to load NCRs" description={getErrorMessage(error)} />
    </PageRoot>
  );

  return (
    <PageRoot>
      <PageHeader
        title="Non-Conformance Reports"
        subtitle="Track and disposition quality failures from inspection"
        breadcrumbs={[{ label: 'Quality' }, { label: 'NCR' }]}
        actions={
          <Stack direction="row" spacing={1}>
            <RefreshButton onClick={() => refetch()} />
            <IconButton
              size="small"
              onClick={() => { createForm.reset({ severity: 'MAJOR', qtyAffected: 1 }); setCreateOpen(true); }}
              sx={{ bgcolor: 'primary.main', color: 'white', borderRadius: 1, '&:hover': { bgcolor: 'primary.dark' } }}
            >
              <SolarIcon name="add" size={16} />
            </IconButton>
          </Stack>
        }
      />

      {toastError && (
        <Alert severity="error" onClose={() => setToastError(null)} sx={{ mb: 2 }}>
          {toastError}
        </Alert>
      )}

      <Stack direction="row" spacing={1.5} sx={{ mb: 1.5, alignItems: 'center' }}>
        <Box sx={{ flex: 1 }}>
          <TableToolbar
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Filter by NCR no, product or assignee…"
          />
        </Box>
        <Select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          size="small"
          displayEmpty
          sx={{ minWidth: 160, flexShrink: 0 }}
        >
          <MenuItem value="">All Status</MenuItem>
          {Object.entries(STATUS_CFG).map(([k, v]) => (
            <MenuItem key={k} value={k}>{v.label}</MenuItem>
          ))}
        </Select>
      </Stack>

      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={filtered}
          columns={columns}
          getRowId={(row) => row.ncrId}
          disableRowSelectionOnClick
          slots={{
            noRowsOverlay: () => (
              <EmptyState
                icon="emptyTable"
                title="No NCRs"
                description="NCRs are raised automatically when an inspection order fails, or can be created manually."
              />
            ),
          }}
          sx={{ border: 'none', '& .MuiDataGrid-cell': { alignItems: 'center' } }}
        />
      </Box>

      {/* ── Create Manual NCR ── */}
      <FormDrawer
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Create Manual NCR"
        onSubmit={() => void createForm.handleSubmit((v) => createMutation.mutate(v))()}
        loading={createMutation.isPending}
      >
        <Controller name="workOrderId" control={createForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Work Order ID" type="number" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Controller name="productCode" control={createForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Product Code" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Controller name="lotNumber" control={createForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Lot Number (optional)" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Controller name="qtyAffected" control={createForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Qty Affected" type="number" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Controller name="severity" control={createForm.control} render={({ field }) => (
          <TextField {...field} label="Severity" select fullWidth>
            <MenuItem value="CRITICAL">Critical</MenuItem>
            <MenuItem value="MAJOR">Major</MenuItem>
            <MenuItem value="MINOR">Minor</MenuItem>
          </TextField>
        )} />
      </FormDrawer>

      {/* ── Set Disposition ── */}
      <FormDrawer
        open={!!dispositionTarget}
        onClose={() => setDispositionTarget(null)}
        title={`Set Disposition — ${dispositionTarget?.ncrNo ?? ''}`}
        onSubmit={() => void dispositionForm.handleSubmit((v) =>
          dispositionMutation.mutate({ id: Number(dispositionTarget!.ncrId), v }))()}
        loading={dispositionMutation.isPending}
      >
        <Controller name="dispositionCode" control={dispositionForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Disposition" select fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message}>
            {DISPOSITIONS.map((d) => <MenuItem key={d.value} value={d.value}>{d.label}</MenuItem>)}
          </TextField>
        )} />
        <Controller name="setBy" control={dispositionForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Set By (supervisor code)" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
      </FormDrawer>

      {/* ── Close NCR ── */}
      <FormDrawer
        open={!!closeTarget}
        onClose={() => setCloseTarget(null)}
        title={`Close NCR — ${closeTarget?.ncrNo ?? ''}`}
        onSubmit={() => void closeForm.handleSubmit((v) =>
          closeMutation.mutate({ id: Number(closeTarget!.ncrId), v }))()}
        loading={closeMutation.isPending}
      >
        <Controller name="closedBy" control={closeForm.control} render={({ field, fieldState }) => (
          <TextField {...field} label="Closed By" fullWidth
            error={!!fieldState.error} helperText={fieldState.error?.message} />
        )} />
        <Controller name="rootCause" control={closeForm.control} render={({ field }) => (
          <TextField {...field} label="Root Cause (optional)" fullWidth multiline rows={2} />
        )} />
        <Controller name="correctiveAction" control={closeForm.control} render={({ field }) => (
          <TextField {...field} label="Corrective Action (optional)" fullWidth multiline rows={2} />
        )} />
        <Controller name="preventiveAction" control={closeForm.control} render={({ field }) => (
          <TextField {...field} label="Preventive Action (optional)" fullWidth multiline rows={2} />
        )} />
      </FormDrawer>

      {/* ── Update assignment ── */}
      <FormDrawer
        open={!!updateTarget}
        onClose={() => setUpdateTarget(null)}
        title={`Assign — ${updateTarget?.ncrNo ?? ''}`}
        onSubmit={() => void updateForm.handleSubmit((v) =>
          updateMutation.mutate({ id: Number(updateTarget!.ncrId), v }))()}
        loading={updateMutation.isPending}
      >
        <Controller name="assignedTo" control={updateForm.control} render={({ field }) => (
          <TextField {...field} label="Assign To (employee code)" fullWidth />
        )} />
        <Controller name="dueDate" control={updateForm.control} render={({ field }) => (
          <TextField {...field} label="Due Date" type="date" fullWidth
            slotProps={{ htmlInput: { max: '2099-12-31' } }} />
        )} />
      </FormDrawer>

      {/* ── Escalate confirm ── */}
      <ConfirmDialog
        open={!!escalateTarget}
        title="Escalate NCR"
        description={
          <>Escalate <strong>{escalateTarget?.ncrNo}</strong> to <strong>Under Review</strong>?</>
        }
        onClose={() => setEscalateTarget(null)}
        onConfirm={() => escalateTarget && escalateMutation.mutate(Number(escalateTarget.ncrId))}
        loading={escalateMutation.isPending}
      />

      {/* ── Cancel confirm ── */}
      <ConfirmDialog
        open={!!cancelTarget}
        title="Cancel NCR"
        description={
          <>Cancel <strong>{cancelTarget?.ncrNo}</strong>? This action cannot be undone.</>
        }
        onClose={() => setCancelTarget(null)}
        onConfirm={() => cancelTarget && cancelMutation.mutate(Number(cancelTarget.ncrId))}
        loading={cancelMutation.isPending}
        confirmLabel="Cancel NCR"
        confirmColor="error"
      />

      {/* ── Detail Drawer ── */}
      <Drawer
        anchor="right"
        open={detailId !== null}
        onClose={() => setDetailId(null)}
        slotProps={{ paper: { sx: { width: 480, p: 3 } } }}
      >
        {detail ? (
          <Stack spacing={2}>
            <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="h6" sx={{ fontFamily: 'monospace', fontWeight: 700 }}>
                {detail.ncrNo}
              </Typography>
              <IconButton onClick={() => setDetailId(null)} size="small">
                <SolarIcon name="close" size={18} />
              </IconButton>
            </Stack>
            <Stack direction="row" spacing={1}>
              {(() => {
                const scfg = STATUS_CFG[detail.status] ?? { label: detail.status, color: '#64748B' };
                return (
                  <Chip label={scfg.label} size="small"
                    sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600,
                      bgcolor: alpha(scfg.color, 0.1), color: scfg.color, border: 'none',
                      '& .MuiChip-label': { px: 0.75 } }} />
                );
              })()}
              {(() => {
                const sv = SEVERITY_CFG[detail.severity] ?? { label: detail.severity, color: '#64748B' };
                return (
                  <Chip label={sv.label} size="small"
                    sx={{ height: 20, fontSize: '0.6875rem', fontWeight: 600,
                      bgcolor: alpha(sv.color, 0.1), color: sv.color, border: 'none',
                      '& .MuiChip-label': { px: 0.75 } }} />
                );
              })()}
            </Stack>
            <Divider />
            <Stack spacing={1}>
              <Row label="Product" value={detail.productCode} />
              <Row label="Lot" value={detail.lotNumber ?? '—'} />
              <Row label="Qty Affected" value={String(Number(detail.qtyAffected))} />
              <Row label="Work Order" value={String(Number(detail.workOrderId))} />
              {detail.inspectionOrderId && <Row label="Inspection Order" value={`#${Number(detail.inspectionOrderId)}`} />}
              <Row label="Disposition" value={
                detail.dispositionCode
                  ? (DISPOSITIONS.find((d) => d.value === detail.dispositionCode)?.label ?? detail.dispositionCode)
                  : '—'
              } />
              {detail.dispositionSetBy && <Row label="Disposition By" value={`${detail.dispositionSetBy} @ ${new Date(detail.dispositionSetAt!).toLocaleDateString()}`} />}
              {detail.assignedTo && <Row label="Assigned To" value={detail.assignedTo} />}
              {detail.dueDate && <Row label="Due" value={new Date(detail.dueDate).toLocaleDateString()} />}
              {detail.closedBy && <Row label="Closed By" value={`${detail.closedBy} @ ${new Date(detail.closedAt!).toLocaleDateString()}`} />}
              {detail.rootCause && <Row label="Root Cause" value={detail.rootCause} />}
              {detail.correctiveAction && <Row label="Corrective Action" value={detail.correctiveAction} />}
              {detail.preventiveAction && <Row label="Preventive Action" value={detail.preventiveAction} />}
              <Row label="Raised By" value={detail.createdBy} />
              <Row label="Created" value={new Date(detail.createdAt).toLocaleString()} />
            </Stack>
            {detail.defectLines.length > 0 && (
              <>
                <Divider />
                <Typography variant="subtitle2" color="text.secondary">Defect Lines</Typography>
                <Stack spacing={0.75}>
                  {detail.defectLines.map((line) => (
                    <Box key={Number(line.lineId)}
                      sx={{ p: 1, borderRadius: 1, bgcolor: 'action.hover' }}>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>
                        {line.defectCode ?? `Code #${Number(line.defectCodeId)}`}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        Qty: {Number(line.qtyDefective)}{line.notes ? ` — ${line.notes}` : ''}
                      </Typography>
                    </Box>
                  ))}
                </Stack>
              </>
            )}
          </Stack>
        ) : (
          <Stack sx={{ height: '100%', alignItems: 'center', justifyContent: 'center' }}>
            <Typography color="text.secondary">Loading…</Typography>
          </Stack>
        )}
      </Drawer>
    </PageRoot>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <Stack direction="row" sx={{ justifyContent: 'space-between' }} spacing={2}>
      <Typography variant="body2" color="text.secondary" sx={{ flexShrink: 0 }}>{label}</Typography>
      <Typography variant="body2" sx={{ textAlign: 'right', wordBreak: 'break-word' }}>{value}</Typography>
    </Stack>
  );
}
