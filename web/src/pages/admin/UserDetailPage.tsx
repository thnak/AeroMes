import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link as RouterLink } from 'react-router-dom';
import { useQueryClient, useMutation } from '@tanstack/react-query';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import dayjs from 'dayjs';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  Snackbar,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';

import PageHeader, { PageRoot, PageSection } from '../../components/PageHeader';
import { DetailPageSkeleton } from '../../components/LoadingSkeleton';
import ConfirmDialog from '../../components/ConfirmDialog';
import SolarIcon from '../../components/SolarIcon';

import {
  useGetApiV1UsersId,
  getGetApiV1UsersIdQueryKey,
  putApiV1UsersId,
  deleteApiV1UsersId,
  postApiV1UsersIdActivate,
  postApiV1UsersIdRoles,
  postApiV1UsersIdResetPassword,
} from '../../api/users/users';
import { deleteApiV1UsersUserIdMfa } from '../../api/mfa/mfa';
import { useGetApiV1Roles } from '../../api/roles/roles';
import { useGetApiV1AuditLogUserUserId } from '../../api/audit-log/audit-log';

import type { UpdateUserRequest } from '../../api/model/updateUserRequest';
import type { SetRolesRequest } from '../../api/model/setRolesRequest';
import type { ResetPasswordResult } from '../../api/model/resetPasswordResult';
import type { RoleDto } from '../../api/model/roleDto';
import type { SecurityAuditLog } from '../../api/model/securityAuditLog';

import { getErrorMessage } from '../../lib/apiClient';

// ─── Zod schema ──────────────────────────────────────────────────────────────

const profileSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  department: z.string().nullable().optional(),
  employeeCode: z.string().nullable().optional(),
  preferredLanguage: z.enum(['en', 'vi', '']).nullable().optional(),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

// ─── TempPasswordDialog ───────────────────────────────────────────────────────

interface TempPasswordDialogProps {
  open: boolean;
  tempPassword: string;
  onClose: () => void;
}

function TempPasswordDialog({ open, tempPassword, onClose }: TempPasswordDialogProps) {
  const [copied, setCopied] = useState(false);

  function handleCopy() {
    navigator.clipboard.writeText(tempPassword).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth slotProps={{ paper: { sx: { p: 0.5 } } }}>
      <DialogTitle>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
          <SolarIcon name="lock" size={22} color="warning.main" />
          <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
            Temporary Password
          </Typography>
        </Stack>
      </DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Share this temporary password with the user. They will be required to change it on next login.
        </Typography>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1,
            p: 1.5,
            bgcolor: 'action.hover',
            borderRadius: 1,
            border: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Typography
            variant="body1"
            sx={{ flex: 1, fontFamily: 'monospace', fontWeight: 600, letterSpacing: 1, wordBreak: 'break-all' }}
          >
            {tempPassword}
          </Typography>
          <Tooltip title={copied ? 'Copied!' : 'Copy'}>
            <Button size="small" variant="outlined" onClick={handleCopy} sx={{ flexShrink: 0 }}>
              {copied ? 'Copied' : 'Copy'}
            </Button>
          </Tooltip>
        </Box>
      </DialogContent>
      <DialogActions sx={{ px: 2, pb: 1.5 }}>
        <Button variant="contained" size="small" onClick={onClose}>
          Done
        </Button>
      </DialogActions>
    </Dialog>
  );
}

// ─── Outcome chip helper ──────────────────────────────────────────────────────

function OutcomeChip({ outcome }: { outcome?: string }) {
  if (!outcome) return null;
  const success = outcome.toLowerCase() === 'success';
  return (
    <Chip
      label={outcome}
      size="small"
      color={success ? 'success' : 'error'}
      variant="outlined"
      sx={{ fontFamily: 'monospace', fontSize: '0.7rem' }}
    />
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // ── Data queries ──────────────────────────────────────────────────────────
  const {
    data: user,
    isLoading: userLoading,
    error: userError,
  } = useGetApiV1UsersId(id ?? '', { query: { enabled: !!id } });

  const { data: rolesData, isLoading: rolesLoading } = useGetApiV1Roles();
  const allRoles: RoleDto[] = rolesData ?? [];

  const { data: auditData } = useGetApiV1AuditLogUserUserId(id ?? '', undefined, {
    query: { enabled: !!id },
  });
  const auditLogs: SecurityAuditLog[] = auditData ?? [];

  // ── Local UI state ────────────────────────────────────────────────────────
  const [snackbar, setSnackbar] = useState<{ message: string; severity: 'success' | 'error' } | null>(null);
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [deactivateDialogOpen, setDeactivateDialogOpen] = useState(false);
  const [mfaDialogOpen, setMfaDialogOpen] = useState(false);
  const [tempPassword, setTempPassword] = useState<string | null>(null);

  // ── Profile form ──────────────────────────────────────────────────────────
  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isDirty, isSubmitting },
  } = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      fullName: '',
      department: '',
      employeeCode: '',
      preferredLanguage: '',
    },
  });

  // Populate form when user data loads
  useEffect(() => {
    if (user) {
      reset({
        fullName: user.fullName ?? '',
        department: user.department ?? '',
        employeeCode: user.employeeCode ?? '',
        preferredLanguage: (user.preferredLanguage as 'en' | 'vi' | '' | null) ?? '',
      });
      setSelectedRoles(user.roles ?? []);
    }
  }, [user, reset]);

  // ── Invalidate helper ─────────────────────────────────────────────────────
  function invalidateUser() {
    queryClient.invalidateQueries({ queryKey: getGetApiV1UsersIdQueryKey(id ?? '') });
  }

  // ── Mutation: update profile ──────────────────────────────────────────────
  const updateProfileMutation = useMutation({
    mutationFn: (data: UpdateUserRequest) => putApiV1UsersId(id ?? '', data),
    onSuccess: () => {
      setSnackbar({ message: 'Profile updated successfully.', severity: 'success' });
      invalidateUser();
    },
    onError: (err) => {
      setSnackbar({ message: getErrorMessage(err, 'Failed to update profile.'), severity: 'error' });
    },
  });

  function onProfileSubmit(values: ProfileFormValues) {
    const payload: UpdateUserRequest = {
      fullName: values.fullName,
      department: values.department ?? null,
      employeeCode: values.employeeCode ?? null,
      preferredLanguage: values.preferredLanguage || null,
      avatarUrl: user?.avatarUrl ?? null,
      defaultWorkCenterId: user?.defaultWorkCenterId ?? null,
    };
    updateProfileMutation.mutate(payload);
  }

  // ── Mutation: save roles ──────────────────────────────────────────────────
  const saveRolesMutation = useMutation({
    mutationFn: (req: SetRolesRequest) => postApiV1UsersIdRoles(id ?? '', req),
    onSuccess: () => {
      setSnackbar({ message: 'Roles updated successfully.', severity: 'success' });
      invalidateUser();
    },
    onError: (err) => {
      setSnackbar({ message: getErrorMessage(err, 'Failed to update roles.'), severity: 'error' });
    },
  });

  function handleRoleToggle(roleName: string) {
    setSelectedRoles((prev) =>
      prev.includes(roleName) ? prev.filter((r) => r !== roleName) : [...prev, roleName],
    );
  }

  function handleSaveRoles() {
    saveRolesMutation.mutate({ roleNames: selectedRoles });
  }

  // ── Mutation: deactivate ──────────────────────────────────────────────────
  const deactivateMutation = useMutation({
    mutationFn: () => deleteApiV1UsersId(id ?? ''),
    onSuccess: () => {
      setSnackbar({ message: 'User deactivated.', severity: 'success' });
      setDeactivateDialogOpen(false);
      invalidateUser();
    },
    onError: (err) => {
      setSnackbar({ message: getErrorMessage(err, 'Failed to deactivate user.'), severity: 'error' });
      setDeactivateDialogOpen(false);
    },
  });

  // ── Mutation: activate ────────────────────────────────────────────────────
  const activateMutation = useMutation({
    mutationFn: () => postApiV1UsersIdActivate(id ?? ''),
    onSuccess: () => {
      setSnackbar({ message: 'User activated.', severity: 'success' });
      invalidateUser();
    },
    onError: (err) => {
      setSnackbar({ message: getErrorMessage(err, 'Failed to activate user.'), severity: 'error' });
    },
  });

  // ── Mutation: reset password ──────────────────────────────────────────────
  const resetPasswordMutation = useMutation({
    mutationFn: () => postApiV1UsersIdResetPassword(id ?? ''),
    onSuccess: (result: ResetPasswordResult) => {
      setTempPassword(result.tempPassword);
      invalidateUser();
    },
    onError: (err) => {
      setSnackbar({ message: getErrorMessage(err, 'Failed to reset password.'), severity: 'error' });
    },
  });

  // ── Mutation: admin disable MFA ───────────────────────────────────────────
  const disableMfaMutation = useMutation({
    mutationFn: () => deleteApiV1UsersUserIdMfa(id ?? ''),
    onSuccess: () => {
      setSnackbar({ message: 'MFA disabled for this user.', severity: 'success' });
      setMfaDialogOpen(false);
      invalidateUser();
    },
    onError: (err) => {
      setSnackbar({ message: getErrorMessage(err, 'Failed to disable MFA.'), severity: 'error' });
      setMfaDialogOpen(false);
    },
  });

  // ── 404 handling ──────────────────────────────────────────────────────────
  if (!userLoading && userError) {
    const status = (userError as { response?: { status?: number } })?.response?.status;
    return (
      <PageRoot>
        <Stack sx={{ alignItems: 'center', justifyContent: 'center', py: 8, gap: 2 }}>
          <SolarIcon name="user-cross" size={48} color="text.secondary" />
          <Typography variant="h6" color="text.secondary">
            {status === 404 ? 'User not found.' : 'Failed to load user.'}
          </Typography>
          <Button
            variant="outlined"
            onClick={() => navigate('/admin/users')}
            startIcon={<SolarIcon name="arrow-left" size={18} />}
          >
            Back to Users
          </Button>
        </Stack>
      </PageRoot>
    );
  }

  // ── Loading ───────────────────────────────────────────────────────────────
  if (userLoading || !user) {
    return (
      <PageRoot>
        <DetailPageSkeleton />
      </PageRoot>
    );
  }

  // ── Render ────────────────────────────────────────────────────────────────
  return (
    <PageRoot>
      {/* ── Header ── */}
      <PageHeader
        title={user.fullName}
        subtitle={user.email}
        breadcrumbs={[
          { label: 'Admin' },
          { label: 'Users', href: '/admin/users' },
          { label: user.fullName },
        ]}
        actions={
          <Button
            variant="outlined"
            size="small"
            component={RouterLink}
            to="/admin/users"
            startIcon={<SolarIcon name="arrow-left" size={16} />}
          >
            Back
          </Button>
        }
      />

      <Stack spacing={3}>
        {/* ── Section 1: Profile ── */}
        <PageSection title="Profile" subtitle="Basic user information">
          <Card>
            <CardContent>
              <Box component="form" onSubmit={handleSubmit(onProfileSubmit)} noValidate>
                <Stack spacing={2.5}>
                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                    {/* fullName */}
                    <Controller
                      name="fullName"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          label="Full Name"
                          required
                          fullWidth
                          size="small"
                          error={!!errors.fullName}
                          helperText={errors.fullName?.message}
                        />
                      )}
                    />

                    {/* employeeCode */}
                    <Controller
                      name="employeeCode"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          value={field.value ?? ''}
                          label="Employee Code"
                          fullWidth
                          size="small"
                          error={!!errors.employeeCode}
                          helperText={errors.employeeCode?.message}
                        />
                      )}
                    />
                  </Stack>

                  <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                    {/* department */}
                    <Controller
                      name="department"
                      control={control}
                      render={({ field }) => (
                        <TextField
                          {...field}
                          value={field.value ?? ''}
                          label="Department"
                          fullWidth
                          size="small"
                          error={!!errors.department}
                          helperText={errors.department?.message}
                        />
                      )}
                    />

                    {/* preferredLanguage */}
                    <Controller
                      name="preferredLanguage"
                      control={control}
                      render={({ field }) => (
                        <FormControl fullWidth size="small" error={!!errors.preferredLanguage}>
                          <InputLabel>Preferred Language</InputLabel>
                          <Select {...field} value={field.value ?? ''} label="Preferred Language">
                            <MenuItem value=""><em>— None —</em></MenuItem>
                            <MenuItem value="en">English</MenuItem>
                            <MenuItem value="vi">Vietnamese</MenuItem>
                          </Select>
                          {errors.preferredLanguage && (
                            <FormHelperText>{errors.preferredLanguage.message}</FormHelperText>
                          )}
                        </FormControl>
                      )}
                    />
                  </Stack>

                  <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                    <Button
                      type="submit"
                      variant="contained"
                      size="small"
                      disabled={!isDirty || isSubmitting || updateProfileMutation.isPending}
                      startIcon={<SolarIcon name="floppy-disk" size={16} />}
                    >
                      {updateProfileMutation.isPending ? 'Saving…' : 'Save Changes'}
                    </Button>
                  </Box>
                </Stack>
              </Box>
            </CardContent>
          </Card>
        </PageSection>

        {/* ── Section 2: Roles ── */}
        <PageSection
          title="Roles"
          subtitle="Assign roles that determine the user's permissions"
          actions={
            <Button
              variant="contained"
              size="small"
              disabled={saveRolesMutation.isPending || rolesLoading}
              onClick={handleSaveRoles}
              startIcon={<SolarIcon name="shield-check" size={16} />}
            >
              {saveRolesMutation.isPending ? 'Saving…' : 'Save Roles'}
            </Button>
          }
        >
          <Card>
            <CardContent>
              {rolesLoading ? (
                <Typography variant="body2" color="text.secondary">Loading roles…</Typography>
              ) : allRoles.length === 0 ? (
                <Typography variant="body2" color="text.secondary">No roles available.</Typography>
              ) : (
                <FormGroup row sx={{ gap: 0 }}>
                  {allRoles.map((role) => {
                    const name = role.name ?? '';
                    const checked = selectedRoles.includes(name);
                    return (
                      <FormControlLabel
                        key={role.id}
                        control={
                          <Checkbox
                            size="small"
                            checked={checked}
                            onChange={() => handleRoleToggle(name)}
                          />
                        }
                        label={<Typography variant="body2">{name}</Typography>}
                        sx={{ mr: 3 }}
                      />
                    );
                  })}
                </FormGroup>
              )}
            </CardContent>
          </Card>
        </PageSection>

        {/* ── Section 3: Account Status ── */}
        <PageSection title="Account Status" subtitle="Manage account access and credentials">
          <Card>
            <CardContent>
              <Stack spacing={2.5} divider={<Divider flexItem />}>
                {/* Active / Inactive */}
                <Stack
                  direction="row"
                  sx={{ alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1 }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>Account Status</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Deactivated users cannot log in.
                    </Typography>
                  </Box>
                  <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
                    <Chip
                      label={user.isActive ? 'Active' : 'Inactive'}
                      color={user.isActive ? 'success' : 'default'}
                      size="small"
                      variant="outlined"
                    />
                    {user.isActive ? (
                      <Button
                        variant="outlined"
                        color="error"
                        size="small"
                        disabled={deactivateMutation.isPending}
                        onClick={() => setDeactivateDialogOpen(true)}
                        startIcon={<SolarIcon name="user-block" size={16} />}
                      >
                        Deactivate
                      </Button>
                    ) : (
                      <Button
                        variant="outlined"
                        color="success"
                        size="small"
                        disabled={activateMutation.isPending}
                        onClick={() => activateMutation.mutate()}
                        startIcon={<SolarIcon name="user-check" size={16} />}
                      >
                        {activateMutation.isPending ? 'Activating…' : 'Activate'}
                      </Button>
                    )}
                  </Stack>
                </Stack>

                {/* Force password change (read-only) */}
                <Stack
                  direction="row"
                  sx={{ alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1 }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>Force Password Change</Typography>
                    <Typography variant="caption" color="text.secondary">
                      User must change password on next login.
                    </Typography>
                  </Box>
                  <Chip
                    label={user.forcePasswordChange ? 'Required' : 'Not required'}
                    color={user.forcePasswordChange ? 'warning' : 'default'}
                    size="small"
                    variant="outlined"
                  />
                </Stack>

                {/* Reset password */}
                <Stack
                  direction="row"
                  sx={{ alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1 }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>Reset Password</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Generate a temporary password for the user.
                    </Typography>
                  </Box>
                  <Button
                    variant="outlined"
                    size="small"
                    disabled={resetPasswordMutation.isPending}
                    onClick={() => resetPasswordMutation.mutate()}
                    startIcon={<SolarIcon name="key" size={16} />}
                  >
                    {resetPasswordMutation.isPending ? 'Resetting…' : 'Reset Password'}
                  </Button>
                </Stack>

                {/* Disable MFA */}
                <Stack
                  direction="row"
                  sx={{ alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1 }}
                >
                  <Box>
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>Multi-Factor Authentication</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Admin override: disable MFA for this user.
                    </Typography>
                  </Box>
                  <Button
                    variant="outlined"
                    color="warning"
                    size="small"
                    disabled={disableMfaMutation.isPending}
                    onClick={() => setMfaDialogOpen(true)}
                    startIcon={<SolarIcon name="shield-warning" size={16} />}
                  >
                    Disable MFA
                  </Button>
                </Stack>
              </Stack>
            </CardContent>
          </Card>
        </PageSection>

        {/* ── Section 4: Recent Activity ── */}
        <PageSection title="Recent Activity" subtitle="Last 10 security events for this user">
          <Card>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Time</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Event Type</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Outcome</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Failure Reason</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {auditLogs.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={4}>
                        <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
                          No recent activity.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    auditLogs.map((log, i) => (
                      <TableRow key={String(log.auditId ?? i)} hover>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {log.occurredAt
                              ? dayjs(log.occurredAt).format('DD/MM/YYYY HH:mm:ss')
                              : '—'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>
                            {log.eventType ?? '—'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <OutcomeChip outcome={log.outcome} />
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {log.failureReason ?? '—'}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </PageSection>
      </Stack>

      {/* ── Confirm: Deactivate ── */}
      <ConfirmDialog
        open={deactivateDialogOpen}
        onClose={() => setDeactivateDialogOpen(false)}
        onConfirm={() => deactivateMutation.mutate()}
        title="Deactivate User"
        description={`Are you sure you want to deactivate ${user.fullName}? They will lose access immediately.`}
        confirmLabel="Deactivate"
        confirmColor="error"
        loading={deactivateMutation.isPending}
      />

      {/* ── Confirm: Disable MFA ── */}
      <ConfirmDialog
        open={mfaDialogOpen}
        onClose={() => setMfaDialogOpen(false)}
        onConfirm={() => disableMfaMutation.mutate()}
        title="Disable MFA"
        description={`This will remove MFA from ${user.fullName}'s account. They will be able to log in without a second factor until they re-enroll.`}
        confirmLabel="Disable MFA"
        confirmColor="warning"
        loading={disableMfaMutation.isPending}
      />

      {/* ── Temp Password dialog ── */}
      {tempPassword !== null && (
        <TempPasswordDialog
          open
          tempPassword={tempPassword}
          onClose={() => setTempPassword(null)}
        />
      )}

      {/* ── Snackbar ── */}
      <Snackbar
        open={!!snackbar}
        autoHideDuration={4000}
        onClose={() => setSnackbar(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setSnackbar(null)}
          severity={snackbar?.severity ?? 'info'}
          variant="filled"
          sx={{ width: '100%' }}
        >
          {snackbar?.message}
        </Alert>
      </Snackbar>
    </PageRoot>
  );
}
