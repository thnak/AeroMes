import { useState, useEffect, useCallback } from 'react';
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
  IconButton,
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

import { getErrorMessage, api } from '../../lib/apiClient';

// ─── Inline types for endpoints not yet in Orval models ──────────────────────

interface PermissionOverrideDto {
  overrideId: number;
  permissionCode: string;
  effect: 'Grant' | 'Deny';
  grantedBy: string | null;
  grantedAt: string;
  expiresAt: string | null;
}

interface SessionDto {
  tokenId: number;
  deviceInfo: string | null;
  ipAddress: string | null;
  createdAt: string;
  expiresAt: string;
  isCurrent: boolean;
}

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
          <SolarIcon name="forbidden" size={22} color="warning.main" />
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

  // ── Permission overrides state ────────────────────────────────────────────
  const [overrides, setOverrides] = useState<PermissionOverrideDto[]>([]);
  const [overridesLoading, setOverridesLoading] = useState(false);
  const [addOverrideOpen, setAddOverrideOpen] = useState(false);
  const [addOverrideCode, setAddOverrideCode] = useState('');
  const [addOverrideEffect, setAddOverrideEffect] = useState<'Grant' | 'Deny'>('Grant');
  const [addOverrideSubmitting, setAddOverrideSubmitting] = useState(false);

  // ── Sessions state ────────────────────────────────────────────────────────
  const [sessions, setSessions] = useState<SessionDto[]>([]);
  const [sessionsLoading, setSessionsLoading] = useState(false);
  const [revokeAllSessionsLoading, setRevokeAllSessionsLoading] = useState(false);
  const [revokeAllOpen, setRevokeAllOpen] = useState(false);

  const fetchOverrides = useCallback(async () => {
    if (!id) return;
    setOverridesLoading(true);
    try {
      const data = await api.get<PermissionOverrideDto[]>(`/api/v1/users/${id}/permissions/overrides`);
      setOverrides(data);
    } catch {
      // silently fail — not critical
    } finally {
      setOverridesLoading(false);
    }
  }, [id]);

  const fetchSessions = useCallback(async () => {
    if (!id) return;
    setSessionsLoading(true);
    try {
      const data = await api.get<SessionDto[]>(`/api/v1/users/${id}/sessions`);
      setSessions(data);
    } catch {
      // silently fail
    } finally {
      setSessionsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    if (id) {
      void fetchOverrides();
      void fetchSessions();
    }
  }, [id, fetchOverrides, fetchSessions]);

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

  // ── Permission override handlers ──────────────────────────────────────────
  async function handleAddOverride() {
    if (!id || !addOverrideCode.trim()) return;
    setAddOverrideSubmitting(true);
    try {
      await api.post(`/api/v1/users/${id}/permissions/override`, {
        permissionCode: addOverrideCode.trim(),
        effect: addOverrideEffect,
        expiresAt: null,
      });
      setSnackbar({ message: 'Permission override added.', severity: 'success' });
      setAddOverrideOpen(false);
      setAddOverrideCode('');
      setAddOverrideEffect('Grant');
      await fetchOverrides();
    } catch (err) {
      setSnackbar({ message: getErrorMessage(err, 'Failed to add override.'), severity: 'error' });
    } finally {
      setAddOverrideSubmitting(false);
    }
  }

  async function handleRemoveOverride(overrideId: number) {
    if (!id) return;
    try {
      await api.delete(`/api/v1/users/${id}/permissions/override/${overrideId}`);
      setSnackbar({ message: 'Override removed.', severity: 'success' });
      setOverrides((prev) => prev.filter((o) => o.overrideId !== overrideId));
    } catch (err) {
      setSnackbar({ message: getErrorMessage(err, 'Failed to remove override.'), severity: 'error' });
    }
  }

  // ── Session handlers ──────────────────────────────────────────────────────
  async function handleRevokeSession(tokenId: number) {
    if (!id) return;
    try {
      await api.delete(`/api/v1/users/${id}/sessions/${tokenId}`);
      setSnackbar({ message: 'Session revoked.', severity: 'success' });
      setSessions((prev) => prev.filter((s) => s.tokenId !== tokenId));
    } catch (err) {
      setSnackbar({ message: getErrorMessage(err, 'Failed to revoke session.'), severity: 'error' });
    }
  }

  async function handleRevokeAllSessions() {
    if (!id) return;
    setRevokeAllSessionsLoading(true);
    try {
      await api.post(`/api/v1/users/${id}/sessions/revoke-all`);
      setSnackbar({ message: 'All sessions revoked.', severity: 'success' });
      setRevokeAllOpen(false);
      setSessions([]);
    } catch (err) {
      setSnackbar({ message: getErrorMessage(err, 'Failed to revoke all sessions.'), severity: 'error' });
    } finally {
      setRevokeAllSessionsLoading(false);
    }
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
          <SolarIcon name="error" size={48} color="text.secondary" />
          <Typography variant="h6" color="text.secondary">
            {status === 404 ? 'User not found.' : 'Failed to load user.'}
          </Typography>
          <Button
            variant="outlined"
            onClick={() => navigate('/admin/users')}
            startIcon={<SolarIcon name="back" size={18} />}
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
            startIcon={<SolarIcon name="back" size={16} />}
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
                      startIcon={<SolarIcon name="edit" size={16} />}
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
              startIcon={<SolarIcon name="quality" size={16} />}
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

        {/* ── Section 3: Permission Overrides ── */}
        <PageSection
          title="Permission Overrides"
          subtitle="Individual grant or deny overrides beyond role-based permissions"
          actions={
            <Button
              variant="contained"
              size="small"
              onClick={() => setAddOverrideOpen(true)}
              startIcon={<SolarIcon name="add" size={16} />}
            >
              Add Override
            </Button>
          }
        >
          <Card>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Permission Code</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Effect</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Granted By</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Granted At</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Expires</TableCell>
                    <TableCell />
                  </TableRow>
                </TableHead>
                <TableBody>
                  {overridesLoading ? (
                    <TableRow>
                      <TableCell colSpan={6}>
                        <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>Loading…</Typography>
                      </TableCell>
                    </TableRow>
                  ) : overrides.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6}>
                        <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
                          No permission overrides.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    overrides.map((o) => (
                      <TableRow key={o.overrideId} hover>
                        <TableCell>
                          <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>
                            {o.permissionCode}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={o.effect}
                            size="small"
                            color={o.effect === 'Grant' ? 'success' : 'error'}
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">{o.grantedBy ?? '—'}</Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {dayjs(o.grantedAt).format('DD/MM/YYYY HH:mm')}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {o.expiresAt ? dayjs(o.expiresAt).format('DD/MM/YYYY HH:mm') : 'Never'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ textAlign: 'right' }}>
                          <Tooltip title="Remove override">
                            <IconButton size="small" color="error" onClick={() => void handleRemoveOverride(o.overrideId)}>
                              <SolarIcon name="cancel" size={16} />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </PageSection>

        {/* ── Section 4: Account Status ── */}
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
                        startIcon={<SolarIcon name="cancel" size={16} />}
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
                        startIcon={<SolarIcon name="success" size={16} />}
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
                    startIcon={<SolarIcon name="forbidden" size={16} />}
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
                    startIcon={<SolarIcon name="warning" size={16} />}
                  >
                    Disable MFA
                  </Button>
                </Stack>
              </Stack>
            </CardContent>
          </Card>
        </PageSection>

        {/* ── Section 5: Active Sessions ── */}
        <PageSection
          title="Active Sessions"
          subtitle="Currently active refresh token sessions for this user"
          actions={
            sessions.length > 0 ? (
              <Button
                variant="outlined"
                color="error"
                size="small"
                onClick={() => setRevokeAllOpen(true)}
                startIcon={<SolarIcon name="cancel" size={16} />}
              >
                Revoke All
              </Button>
            ) : undefined
          }
        >
          <Card>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Device</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>IP Address</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Created</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>Expires</TableCell>
                    <TableCell />
                  </TableRow>
                </TableHead>
                <TableBody>
                  {sessionsLoading ? (
                    <TableRow>
                      <TableCell colSpan={5}>
                        <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>Loading…</Typography>
                      </TableCell>
                    </TableRow>
                  ) : sessions.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={5}>
                        <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
                          No active sessions.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    sessions.map((s) => (
                      <TableRow key={s.tokenId} hover>
                        <TableCell>
                          <Typography variant="caption">{s.deviceInfo ?? '—'}</Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>{s.ipAddress ?? '—'}</Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {dayjs(s.createdAt).format('DD/MM/YYYY HH:mm')}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" color="text.secondary">
                            {dayjs(s.expiresAt).format('DD/MM/YYYY HH:mm')}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ textAlign: 'right' }}>
                          <Tooltip title="Revoke session">
                            <IconButton size="small" color="error" onClick={() => void handleRevokeSession(s.tokenId)}>
                              <SolarIcon name="cancel" size={16} />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </PageSection>

        {/* ── Section 7: Recent Activity ── */}
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

      {/* ── Dialog: Add Permission Override ── */}
      <Dialog open={addOverrideOpen} onClose={() => setAddOverrideOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Add Permission Override</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField
              label="Permission Code"
              size="small"
              fullWidth
              value={addOverrideCode}
              onChange={(e) => setAddOverrideCode(e.target.value)}
              placeholder="e.g. work-order:read"
              helperText="Must match an existing permission code"
            />
            <FormControl size="small" fullWidth>
              <InputLabel>Effect</InputLabel>
              <Select
                value={addOverrideEffect}
                label="Effect"
                onChange={(e) => setAddOverrideEffect(e.target.value as 'Grant' | 'Deny')}
              >
                <MenuItem value="Grant">Grant</MenuItem>
                <MenuItem value="Deny">Deny</MenuItem>
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 2, pb: 1.5 }}>
          <Button size="small" onClick={() => setAddOverrideOpen(false)}>Cancel</Button>
          <Button
            size="small"
            variant="contained"
            disabled={!addOverrideCode.trim() || addOverrideSubmitting}
            onClick={() => void handleAddOverride()}
          >
            {addOverrideSubmitting ? 'Adding…' : 'Add Override'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* ── Confirm: Revoke All Sessions ── */}
      <ConfirmDialog
        open={revokeAllOpen}
        onClose={() => setRevokeAllOpen(false)}
        onConfirm={() => void handleRevokeAllSessions()}
        title="Revoke All Sessions"
        description={`This will immediately invalidate all active sessions for ${user.fullName}. They will be signed out of all devices.`}
        confirmLabel="Revoke All"
        confirmColor="error"
        loading={revokeAllSessionsLoading}
      />

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
