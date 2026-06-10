import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  Step,
  StepLabel,
  Stepper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { motion } from 'framer-motion';
import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import {
  FormDrawer,
  KpiCard,
  PageHeader,
  PageRoot,
  SolarIcon,
  StatusDot,
} from '../../components';
import { machineColors } from '../../theme/tokens';

// ─── Types & mock data ────────────────────────────────────────────────────────

type MachineStatus = 'RUNNING' | 'IDLE' | 'SETUP' | 'DOWN';

interface Machine {
  id: string;
  code: string;
  name: string;
  type: string;
  status: MachineStatus;
  currentWo?: string;
  lastMaintenance?: string;
  openIssues: number;
}

const MACHINES: Machine[] = [
  { id: 'm1', code: 'MC-01', name: 'CNC Lathe 1',    type: 'CNC',      status: 'RUNNING', currentWo: 'WO-2026-0089', lastMaintenance: '2026-05-20', openIssues: 0 },
  { id: 'm2', code: 'MC-02', name: 'Press Line A',   type: 'Press',    status: 'RUNNING', currentWo: 'WO-2026-0090', lastMaintenance: '2026-06-01', openIssues: 1 },
  { id: 'm3', code: 'MC-03', name: 'Welding Bay 1',  type: 'Welding',  status: 'IDLE',    lastMaintenance: '2026-06-05', openIssues: 0 },
  { id: 'm4', code: 'MC-04', name: 'Assembly St. 2', type: 'Assembly', status: 'SETUP',   currentWo: 'WO-2026-0091', lastMaintenance: '2026-05-28', openIssues: 0 },
  { id: 'm5', code: 'MC-05', name: 'Paint Booth',    type: 'Finishing',status: 'DOWN',    lastMaintenance: '2026-06-08', openIssues: 2 },
  { id: 'm6', code: 'MC-06', name: 'CNC Mill 2',     type: 'CNC',      status: 'RUNNING', currentWo: 'WO-2026-0092', lastMaintenance: '2026-06-03', openIssues: 0 },
  { id: 'm7', code: 'MC-07', name: 'Drill Press B',  type: 'Drill',    status: 'IDLE',    lastMaintenance: '2026-06-07', openIssues: 0 },
  { id: 'm8', code: 'MC-08', name: 'Grinder 1',      type: 'Grinding', status: 'RUNNING', currentWo: 'WO-2026-0093', lastMaintenance: '2026-05-25', openIssues: 0 },
];

type IssueStatus = 'OPEN' | 'IN_PROGRESS' | 'RESOLVED' | 'CLOSED';
type IssueSeverity = 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';

interface MaintenanceIssue {
  id: string;
  machineCode: string;
  machineName: string;
  category: string;
  severity: IssueSeverity;
  description: string;
  status: IssueStatus;
  reportedAt: string;
  reportedBy: string;
  stoppedProduction: boolean;
  notes?: string;
}

const MOCK_ISSUES: MaintenanceIssue[] = [
  { id: 'i1', machineCode: 'MC-05', machineName: 'Paint Booth',  category: 'Mechanical', severity: 'CRITICAL', description: 'Main conveyor belt seized — production stopped. Belt torn at drive sprocket.', status: 'IN_PROGRESS', reportedAt: '2026-06-09 07:30', reportedBy: 'Operator 01', stoppedProduction: true },
  { id: 'i2', machineCode: 'MC-05', machineName: 'Paint Booth',  category: 'Electrical', severity: 'HIGH',     description: 'Temp sensor reading erratic — paint cure cycle unreliable.', status: 'OPEN', reportedAt: '2026-06-09 08:15', reportedBy: 'Operator 01', stoppedProduction: false },
  { id: 'i3', machineCode: 'MC-02', machineName: 'Press Line A', category: 'Hydraulic',  severity: 'MEDIUM',   description: 'Minor oil leak at left ram seal. Pressure holding but needs seal replacement.', status: 'OPEN', reportedAt: '2026-06-09 09:00', reportedBy: 'Supervisor', stoppedProduction: false },
];

const SEVERITY_CONFIG: Record<IssueSeverity, { label: string; color: string }> = {
  LOW:      { label: 'Low',      color: '#94A3B8' },
  MEDIUM:   { label: 'Medium',   color: '#B45309' },
  HIGH:     { label: 'High',     color: '#DC2626' },
  CRITICAL: { label: 'Critical', color: '#7C3AED' },
};

const ISSUE_STATUS_CONFIG: Record<IssueStatus, { label: string; color: string; step: number }> = {
  OPEN:        { label: 'Open',        color: '#1D4ED8', step: 0 },
  IN_PROGRESS: { label: 'In Progress', color: '#B45309', step: 1 },
  RESOLVED:    { label: 'Resolved',    color: '#15803D', step: 2 },
  CLOSED:      { label: 'Closed',      color: '#64748B', step: 3 },
};

const WORKFLOW_STEPS = ['Reported', 'Diagnosed & In Progress', 'Repaired', 'Verified & Closed'];

// ─── Report Issue form ────────────────────────────────────────────────────────

const ReportSchema = z.object({
  machineId:          z.string().min(1),
  category:           z.enum(['Mechanical', 'Electrical', 'Hydraulic', 'Software', 'Other']),
  severity:           z.enum(['LOW', 'MEDIUM', 'HIGH', 'CRITICAL']),
  description:        z.string().min(10, 'Describe the issue in at least 10 characters'),
  stoppedProduction:  z.boolean(),
  notes:              z.string().optional(),
});

type ReportFormValues = z.infer<typeof ReportSchema>;

function ReportIssueForm({
  preselectedMachine,
  onSuccess,
}: {
  preselectedMachine?: Machine;
  onSuccess: () => void;
}) {
  const [step, setStep] = useState(0);
  const { register, control, handleSubmit, watch, formState: { errors } } = useForm<ReportFormValues>({
    resolver: zodResolver(ReportSchema),
    defaultValues: {
      machineId: preselectedMachine?.id ?? '',
      severity: 'MEDIUM',
      stoppedProduction: false,
    },
  });

  const severity = watch('severity');

  function onSubmit(_data: ReportFormValues) {
    onSuccess();
  }

  return (
    <Box>
      <Stepper activeStep={step} orientation="horizontal" sx={{ mb: 3 }}>
        {['Issue Details', 'Impact & Submit'].map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      <Box component="form" id="report-form" onSubmit={handleSubmit(onSubmit)} noValidate>
        {step === 0 && (
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
                    {MACHINES.map((m) => (
                      <MenuItem key={m.id} value={m.id}>
                        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                          <StatusDot color={machineColors[m.status]} size={8} />
                          <span>{m.code} — {m.name}</span>
                        </Stack>
                      </MenuItem>
                    ))}
                  </TextField>
                )}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Controller
                name="category"
                control={control}
                render={({ field }) => (
                  <TextField {...field} select label="Category" fullWidth required error={!!errors.category}>
                    {['Mechanical', 'Electrical', 'Hydraulic', 'Software', 'Other'].map((c) => (
                      <MenuItem key={c} value={c}>{c}</MenuItem>
                    ))}
                  </TextField>
                )}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <FormControl fullWidth size="small" error={!!errors.severity}>
                <InputLabel>Severity</InputLabel>
                <Controller
                  name="severity"
                  control={control}
                  render={({ field }) => (
                    <Select {...field} label="Severity">
                      {(['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'] as IssueSeverity[]).map((s) => (
                        <MenuItem key={s} value={s}>
                          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                            <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: SEVERITY_CONFIG[s].color }} />
                            <span>{SEVERITY_CONFIG[s].label}</span>
                          </Stack>
                        </MenuItem>
                      ))}
                    </Select>
                  )}
                />
              </FormControl>
            </Grid>
            {(severity === 'HIGH' || severity === 'CRITICAL') && (
              <Grid size={{ xs: 12 }}>
                <Alert severity="warning" sx={{ py: 0.75 }}>
                  {severity === 'CRITICAL' ? 'Critical severity — supervisor will be notified immediately.' : 'High severity — supervisor will be notified.'}
                </Alert>
              </Grid>
            )}
            <Grid size={{ xs: 12 }}>
              <TextField
                {...register('description')}
                label="Describe the Issue"
                fullWidth
                required
                multiline
                rows={4}
                error={!!errors.description}
                helperText={errors.description?.message ?? 'Include symptoms, when it started, and what you observed.'}
              />
            </Grid>
          </Grid>
        )}

        {step === 1 && (
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle2" gutterBottom>Is production stopped?</Typography>
              <Controller
                name="stoppedProduction"
                control={control}
                render={({ field }) => (
                  <ToggleButtonGroup
                    value={field.value ? 'yes' : 'no'}
                    exclusive
                    onChange={(_, v) => { if (v !== null) field.onChange(v === 'yes'); }}
                    size="small"
                  >
                    <ToggleButton value="yes" color="error">
                      <SolarIcon name="warning" size={16} sx={{ mr: 0.75 }} />
                      Yes — stopped
                    </ToggleButton>
                    <ToggleButton value="no" color="primary">
                      <SolarIcon name="success" size={16} sx={{ mr: 0.75 }} />
                      No — still running
                    </ToggleButton>
                  </ToggleButtonGroup>
                )}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                {...register('notes')}
                label="Additional Notes"
                fullWidth
                multiline
                rows={3}
                placeholder="Parts you think are needed, safety concerns, etc."
              />
            </Grid>
          </Grid>
        )}
      </Box>

      <Stack direction="row" spacing={1.5} sx={{ justifyContent: 'flex-end', mt: 3 }}>
        {step > 0 && (
          <Button variant="outlined" size="small" onClick={() => setStep(0)}>
            Back
          </Button>
        )}
        {step === 0 && (
          <Button variant="contained" size="small" onClick={() => setStep(1)}>
            Next →
          </Button>
        )}
        {step === 1 && (
          <Button
            variant="contained"
            size="small"
            color="warning"
            startIcon={<SolarIcon name="warning" size={16} />}
            onClick={handleSubmit(onSubmit)}
          >
            Submit Report
          </Button>
        )}
      </Stack>
    </Box>
  );
}

// ─── Issue detail / repair workflow ──────────────────────────────────────────

function IssueWorkflowDrawer({
  issue,
  open,
  onClose,
}: {
  issue: MaintenanceIssue | null;
  open: boolean;
  onClose: () => void;
}) {
  if (!issue) return null;
  const currentStep = ISSUE_STATUS_CONFIG[issue.status].step;

  return (
    <FormDrawer
      open={open}
      onClose={onClose}
      title={`Issue — ${issue.machineCode}`}
      subtitle={issue.machineName}
      width={520}
    >
      {/* Severity + status bar */}
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 3 }}>
        <Chip
          label={SEVERITY_CONFIG[issue.severity].label}
          size="small"
          sx={{
            height: 22,
            fontSize: '0.6875rem',
            fontWeight: 700,
            bgcolor: alpha(SEVERITY_CONFIG[issue.severity].color, 0.1),
            color: SEVERITY_CONFIG[issue.severity].color,
            border: 'none',
          }}
        />
        <Chip
          label={ISSUE_STATUS_CONFIG[issue.status].label}
          size="small"
          sx={{
            height: 22,
            fontSize: '0.6875rem',
            fontWeight: 700,
            bgcolor: alpha(ISSUE_STATUS_CONFIG[issue.status].color, 0.1),
            color: ISSUE_STATUS_CONFIG[issue.status].color,
            border: 'none',
          }}
        />
        {issue.stoppedProduction && (
          <Chip
            label="Production stopped"
            size="small"
            color="error"
            sx={{ height: 22, fontSize: '0.6875rem', fontWeight: 700 }}
          />
        )}
      </Stack>

      {/* Workflow stepper */}
      <Stepper activeStep={currentStep} sx={{ mb: 3 }}>
        {WORKFLOW_STEPS.map((label, i) => (
          <Step key={label} completed={i < currentStep}>
            <StepLabel
              slotProps={{
                stepIcon: i === currentStep ? { sx: { color: 'warning.main' } } : undefined,
              }}
            >
              <Typography variant="caption" sx={{ fontWeight: i === currentStep ? 700 : 400 }}>
                {label}
              </Typography>
            </StepLabel>
          </Step>
        ))}
      </Stepper>

      <Divider sx={{ mb: 2.5 }} />

      {/* Issue info */}
      <Box sx={{ mb: 2.5 }}>
        <Stack direction="row" spacing={1.5} sx={{ mb: 0.75 }}>
          <Typography variant="caption" color="text.disabled" sx={{ width: 100, flexShrink: 0 }}>Category</Typography>
          <Typography variant="caption" sx={{ fontWeight: 600 }}>{issue.category}</Typography>
        </Stack>
        <Stack direction="row" spacing={1.5} sx={{ mb: 0.75 }}>
          <Typography variant="caption" color="text.disabled" sx={{ width: 100, flexShrink: 0 }}>Reported</Typography>
          <Typography variant="caption">{issue.reportedAt} by {issue.reportedBy}</Typography>
        </Stack>
        <Stack direction="row" spacing={1.5}>
          <Typography variant="caption" color="text.disabled" sx={{ width: 100, flexShrink: 0 }}>Description</Typography>
          <Typography variant="caption">{issue.description}</Typography>
        </Stack>
      </Box>

      <Divider sx={{ mb: 2.5 }} />

      {/* Repair notes field */}
      <Box sx={{ mb: 2.5 }}>
        <Typography variant="subtitle2" gutterBottom>Repair Notes</Typography>
        <TextField
          placeholder="Describe work performed, parts replaced, findings…"
          fullWidth
          multiline
          rows={4}
          defaultValue={issue.notes}
        />
      </Box>

      {/* Action based on current step */}
      {issue.status === 'OPEN' && (
        <Button variant="contained" color="warning" fullWidth startIcon={<SolarIcon name="maintenance" size={16} />}>
          Start Work — Mark In Progress
        </Button>
      )}
      {issue.status === 'IN_PROGRESS' && (
        <Stack spacing={1}>
          <Button variant="contained" color="success" fullWidth startIcon={<SolarIcon name="success" size={16} />}>
            Mark as Repaired — Awaiting Verification
          </Button>
          <Button variant="outlined" color="error" fullWidth size="small">
            Escalate Issue
          </Button>
        </Stack>
      )}
      {issue.status === 'RESOLVED' && (
        <Button variant="contained" color="primary" fullWidth startIcon={<SolarIcon name="complete" size={16} />}>
          Verify & Close Issue
        </Button>
      )}
    </FormDrawer>
  );
}

// ─── Machine card ─────────────────────────────────────────────────────────────

function MachineCard({
  machine,
  onReportIssue,
}: {
  machine: Machine;
  onReportIssue: (m: Machine) => void;
}) {
  const isDown = machine.status === 'DOWN';
  const color = machineColors[machine.status];

  return (
    <motion.div whileHover={{ y: -2 }} transition={{ duration: 0.15 }}>
      <Card
        sx={{
          height: '100%',
          border: '1px solid',
          borderColor: isDown ? alpha('#B91C1C', 0.3) : 'divider',
          bgcolor: isDown ? alpha('#B91C1C', 0.03) : 'background.paper',
          transition: 'all 0.15s ease',
        }}
      >
        <CardContent sx={{ p: '16px !important' }}>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'flex-start', justifyContent: 'space-between', mb: 1.5 }}>
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography variant="subtitle2" noWrap sx={{ fontWeight: 700 }}>{machine.name}</Typography>
              <Typography variant="caption" color="text.disabled" sx={{ fontFamily: 'ui-monospace, monospace' }}>
                {machine.code} · {machine.type}
              </Typography>
            </Box>
            <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center', flexShrink: 0 }}>
              <StatusDot color={color} size={9} pulse={machine.status === 'RUNNING'} />
              <Typography variant="caption" sx={{ fontWeight: 700, color }}>
                {machine.status}
              </Typography>
            </Stack>
          </Stack>

          {machine.currentWo && (
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
              WO: <span style={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>{machine.currentWo}</span>
            </Typography>
          )}

          {machine.openIssues > 0 && (
            <Chip
              label={`${machine.openIssues} open issue${machine.openIssues > 1 ? 's' : ''}`}
              size="small"
              sx={{
                height: 20,
                fontSize: '0.6875rem',
                fontWeight: 700,
                mb: 1,
                bgcolor: alpha('#B91C1C', 0.1),
                color: '#B91C1C',
                border: 'none',
              }}
            />
          )}

          <Button
            fullWidth
            size="small"
            variant={isDown ? 'contained' : 'outlined'}
            color={isDown ? 'error' : 'inherit'}
            startIcon={<SolarIcon name={isDown ? 'error' : 'maintenance'} size={15} />}
            onClick={() => onReportIssue(machine)}
            sx={{ mt: 0.5 }}
          >
            {isDown ? 'View Issues' : 'Report Issue'}
          </Button>
        </CardContent>
      </Card>
    </motion.div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function MaintenancePage() {
  const [reportTarget, setReportTarget]     = useState<Machine | null>(null);
  const [reportOpen, setReportOpen]         = useState(false);
  const [issueTarget, setIssueTarget]       = useState<MaintenanceIssue | null>(null);
  const [issueOpen, setIssueOpen]           = useState(false);

  function openReport(m: Machine) { setReportTarget(m); setReportOpen(true); }
  function openIssue(i: MaintenanceIssue) { setIssueTarget(i); setIssueOpen(true); }

  const downCount    = MACHINES.filter((m) => m.status === 'DOWN').length;
  const openIssues   = MOCK_ISSUES.filter((i) => i.status === 'OPEN' || i.status === 'IN_PROGRESS').length;
  const criticalCount = MOCK_ISSUES.filter((i) => i.severity === 'CRITICAL').length;

  return (
    <PageRoot>
      <PageHeader
        title="Maintenance"
        subtitle="Track machine health, report issues, and manage repair workflows"
        breadcrumbs={[{ label: 'Maintenance' }]}
        actions={
          <Button
            variant="contained"
            size="small"
            color="warning"
            startIcon={<SolarIcon name="warning" size={16} />}
            onClick={() => setReportOpen(true)}
          >
            Report Issue
          </Button>
        }
      />

      {/* KPI row */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}>
          <KpiCard label="Machines Down"   value={downCount}    icon="machineDown"   accentColor="#B91C1C" />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <KpiCard label="Open Issues"     value={openIssues}   icon="error"         accentColor="#B45309" />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <KpiCard label="Critical Issues" value={criticalCount} icon="warning"      accentColor="#7C3AED" />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <KpiCard label="Machines Running" value={MACHINES.filter((m) => m.status === 'RUNNING').length} icon="machineOn" accentColor="#15803D" />
        </Grid>
      </Grid>

      <Grid container spacing={2.5}>
        {/* Machine status board */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Typography variant="subtitle1" sx={{ mb: 1.5 }}>Machine Status Board</Typography>
          <Grid container spacing={1.5}>
            {MACHINES.map((m) => (
              <Grid key={m.id} size={{ xs: 12, sm: 6, lg: 4 }}>
                <MachineCard machine={m} onReportIssue={openReport} />
              </Grid>
            ))}
          </Grid>
        </Grid>

        {/* Active issues */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Typography variant="subtitle1" sx={{ mb: 1.5 }}>
            Active Issues
            <Chip
              label={MOCK_ISSUES.length}
              size="small"
              sx={{ ml: 1, height: 20, fontSize: '0.6875rem', fontWeight: 700 }}
            />
          </Typography>
          <Stack spacing={1.5}>
            {MOCK_ISSUES.map((issue) => (
              <Card
                key={issue.id}
                sx={(theme) => ({
                  border: '1px solid',
                  borderColor: issue.severity === 'CRITICAL'
                    ? alpha(SEVERITY_CONFIG.CRITICAL.color, 0.3)
                    : 'divider',
                  cursor: 'pointer',
                  '&:hover': { borderColor: 'primary.light', boxShadow: `0 0 0 1px ${alpha(theme.palette.primary.main, 0.2)}` },
                  transition: 'all 0.15s ease',
                })}
                onClick={() => openIssue(issue)}
              >
                <CardContent sx={{ p: '12px !important' }}>
                  <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 0.75 }}>
                    <Chip
                      label={SEVERITY_CONFIG[issue.severity].label}
                      size="small"
                      sx={{
                        height: 18,
                        fontSize: '0.625rem',
                        fontWeight: 700,
                        bgcolor: alpha(SEVERITY_CONFIG[issue.severity].color, 0.1),
                        color: SEVERITY_CONFIG[issue.severity].color,
                        border: 'none',
                        '& .MuiChip-label': { px: 0.6 },
                      }}
                    />
                    <Chip
                      label={ISSUE_STATUS_CONFIG[issue.status].label}
                      size="small"
                      sx={{
                        height: 18,
                        fontSize: '0.625rem',
                        fontWeight: 600,
                        bgcolor: alpha(ISSUE_STATUS_CONFIG[issue.status].color, 0.1),
                        color: ISSUE_STATUS_CONFIG[issue.status].color,
                        border: 'none',
                        '& .MuiChip-label': { px: 0.6 },
                      }}
                    />
                    <Typography variant="caption" color="text.disabled" sx={{ ml: 'auto', flexShrink: 0 }}>
                      {issue.machineCode}
                    </Typography>
                  </Stack>
                  <Typography
                    variant="caption"
                    sx={{
                      display: '-webkit-box',
                      WebkitLineClamp: 2,
                      WebkitBoxOrient: 'vertical',
                      overflow: 'hidden',
                      lineHeight: 1.5,
                    }}
                  >
                    {issue.description}
                  </Typography>
                  <Typography variant="caption" color="text.disabled" sx={{ display: 'block', mt: 0.5 }}>
                    {issue.reportedAt}
                  </Typography>
                </CardContent>
              </Card>
            ))}
          </Stack>
        </Grid>
      </Grid>

      {/* Report Issue drawer */}
      <FormDrawer
        open={reportOpen}
        onClose={() => { setReportOpen(false); setReportTarget(null); }}
        title="Report Maintenance Issue"
        subtitle="Fill in details about the equipment problem"
        width={520}
      >
        <ReportIssueForm
          key={reportTarget?.id}
          preselectedMachine={reportTarget ?? undefined}
          onSuccess={() => { setReportOpen(false); setReportTarget(null); }}
        />
      </FormDrawer>

      {/* Issue workflow drawer */}
      <IssueWorkflowDrawer
        issue={issueTarget}
        open={issueOpen}
        onClose={() => { setIssueOpen(false); setIssueTarget(null); }}
      />
    </PageRoot>
  );
}
