import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import Button from '@mui/material/Button';
import Avatar from '@mui/material/Avatar';
import TextField from '@mui/material/TextField';
import Stack from '@mui/material/Stack';
import Alert from '@mui/material/Alert';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Table from '@mui/material/Table';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TableCell from '@mui/material/TableCell';
import TableBody from '@mui/material/TableBody';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Snackbar from '@mui/material/Snackbar';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import CircularProgress from '@mui/material/CircularProgress';
import Divider from '@mui/material/Divider';
import LinearProgress from '@mui/material/LinearProgress';
import InputAdornment from '@mui/material/InputAdornment';

import PageHeader, { PageRoot } from '../../components/PageHeader';
import SolarIcon from '../../components/SolarIcon';
import ConfirmDialog from '../../components/ConfirmDialog';

import {
  useGetApiV1AuthMe,
  useGetApiV1AuthSessions,
  getGetApiV1AuthMeQueryKey,
  putApiV1AuthMe,
  deleteApiV1AuthSessionsTokenId,
  postApiV1AuthLogoutAll,
  postApiV1AuthChangePassword,
} from '../../api/auth/auth';
import type { ChangePasswordRequest } from '../../api/model/changePasswordRequest';
import {
  useGetApiV1AuthMfaRecoveryCodes,
  deleteApiV1AuthMfa,
  postApiV1AuthMfaRecoveryCodesRegenerate,
} from '../../api/mfa/mfa';
import {
  useGetApiV1AuthPasskey,
  deleteApiV1AuthPasskeyCredentialIdBase64,
  getApiV1AuthPasskeyAttestationOptions,
  postApiV1AuthPasskeyRegister,
  getGetApiV1AuthPasskeyQueryKey,
} from '../../api/passkey/passkey';

import type { UpdateMeRequest } from '../../api/model/updateMeRequest';
import type { TotpConfirmRequest } from '../../api/model/totpConfirmRequest';
import type { RecoveryCodesResult } from '../../api/model/recoveryCodesResult';

import { getErrorMessage } from '../../lib/apiClient';

dayjs.extend(relativeTime);

// ─── Zod schemas ──────────────────────────────────────────────────────────────

const profileSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  preferredLanguage: z.enum(['en', 'vi']),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

const totpCodeSchema = z.object({
  code: z
    .string()
    .length(6, 'Code must be exactly 6 digits')
    .regex(/^\d+$/, 'Digits only'),
});

type TotpCodeFormValues = z.infer<typeof totpCodeSchema>;

// ─── TOTP Code Dialog ─────────────────────────────────────────────────────────

interface TotpDialogProps {
  open: boolean;
  title: string;
  description?: string;
  submitLabel: string;
  submitColor?: 'error' | 'primary';
  isPending: boolean;
  error: string | null;
  onSubmit: (code: string) => void;
  onClose: () => void;
  children?: React.ReactNode;
}

function TotpDialog({
  open,
  title,
  description,
  submitLabel,
  submitColor = 'primary',
  isPending,
  error,
  onSubmit,
  onClose,
  children,
}: TotpDialogProps) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TotpCodeFormValues>({
    resolver: zodResolver(totpCodeSchema),
  });

  const handleClose = () => {
    if (!isPending) {
      reset();
      onClose();
    }
  };

  const doSubmit = (values: TotpCodeFormValues) => {
    onSubmit(values.code);
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        {description && (
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            {description}
          </Typography>
        )}
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}
        <TextField
          label="6-digit code"
          fullWidth
          slotProps={{ htmlInput: { maxLength: 6, inputMode: 'numeric' } }}
          error={!!errors.code}
          helperText={errors.code?.message}
          autoFocus
          autoComplete="one-time-code"
          disabled={isPending}
          {...register('code')}
        />
        {children}
      </DialogContent>
      <DialogActions sx={{ px: 2.5, pb: 2 }}>
        <Button variant="outlined" onClick={handleClose} disabled={isPending}>
          Cancel
        </Button>
        <Button
          variant="contained"
          color={submitColor}
          onClick={handleSubmit(doSubmit)}
          disabled={isPending}
          startIcon={isPending ? <CircularProgress size={16} color="inherit" /> : undefined}
        >
          {submitLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

// ─── Profile Tab ──────────────────────────────────────────────────────────────

function ProfileTab() {
  const queryClient = useQueryClient();
  const [snackbarOpen, setSnackbarOpen] = useState(false);

  const { data: profile, isLoading } = useGetApiV1AuthMe();

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isDirty },
    reset,
  } = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    values: {
      fullName: profile?.fullName ?? '',
      preferredLanguage: (profile?.preferredLanguage as 'en' | 'vi') ?? 'en',
    },
  });

  const updateMutation = useMutation<void, Error, UpdateMeRequest>({
    mutationFn: (body) => putApiV1AuthMe(body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: getGetApiV1AuthMeQueryKey() });
      reset(undefined, { keepValues: true });
      setSnackbarOpen(true);
    },
  });

  const onSubmit = (values: ProfileFormValues) => {
    updateMutation.mutate({
      fullName: values.fullName,
      preferredLanguage: values.preferredLanguage,
      avatarUrl: profile?.avatarUrl ?? null,
    });
  };

  const avatarLetter = (profile?.fullName ?? profile?.email ?? '?')[0]?.toUpperCase() ?? '?';

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 560 }}>
      <Card variant="outlined">
        <CardContent sx={{ p: 3 }}>
          {/* Avatar + identity */}
          <Stack direction="row" spacing={2.5} sx={{ alignItems: 'center', mb: 3 }}>
            <Avatar sx={{ width: 56, height: 56, bgcolor: 'primary.main', fontSize: '1.5rem' }}>
              {avatarLetter}
            </Avatar>
            <Box>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, lineHeight: 1.2 }}>
                {profile?.fullName ?? '—'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {profile?.email ?? '—'}
              </Typography>
            </Box>
          </Stack>

          {/* Roles */}
          {(profile?.roles?.length ?? 0) > 0 && (
            <Box sx={{ mb: 3 }}>
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.75 }}>
                Roles
              </Typography>
              <Stack direction="row" spacing={0.75} sx={{ flexWrap: 'wrap', gap: 0.75 }}>
                {profile!.roles.map((role) => (
                  <Chip key={role} label={role} size="small" variant="outlined" />
                ))}
              </Stack>
            </Box>
          )}

          <Divider sx={{ mb: 3 }} />

          {/* Editable fields */}
          <Box component="form" onSubmit={handleSubmit(onSubmit)}>
            <Stack spacing={2.5}>
              {updateMutation.error && (
                <Alert severity="error">{getErrorMessage(updateMutation.error)}</Alert>
              )}

              <TextField
                {...register('fullName')}
                label="Full name"
                fullWidth
                error={!!errors.fullName}
                helperText={errors.fullName?.message}
                disabled={updateMutation.isPending}
              />

              <Controller
                name="preferredLanguage"
                control={control}
                render={({ field }) => (
                  <FormControl fullWidth>
                    <InputLabel>Preferred language</InputLabel>
                    <Select {...field} label="Preferred language" disabled={updateMutation.isPending}>
                      <MenuItem value="en">English</MenuItem>
                      <MenuItem value="vi">Vietnamese</MenuItem>
                    </Select>
                  </FormControl>
                )}
              />

              <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={!isDirty || updateMutation.isPending}
                  startIcon={
                    updateMutation.isPending ? <CircularProgress size={16} color="inherit" /> : undefined
                  }
                >
                  Save changes
                </Button>
              </Box>
            </Stack>
          </Box>
        </CardContent>
      </Card>

      <Snackbar
        open={snackbarOpen}
        autoHideDuration={3000}
        onClose={() => setSnackbarOpen(false)}
        message="Profile updated"
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      />
    </Box>
  );
}

// ─── MFA Card ─────────────────────────────────────────────────────────────────

function MfaCard() {
  const navigate = useNavigate();
  const [regenerateOpen, setRegenerateOpen] = useState(false);
  const [disableOpen, setDisableOpen] = useState(false);
  const [newCodes, setNewCodes] = useState<string[]>([]);
  const [regenerateError, setRegenerateError] = useState<string | null>(null);
  const [disableError, setDisableError] = useState<string | null>(null);

  const { data: recoveryCodesData, refetch: refetchCodes } = useGetApiV1AuthMfaRecoveryCodes();

  const remaining = Number(recoveryCodesData?.remaining ?? 0);
  const mfaEnabled = remaining > 0;

  const regenerateMutation = useMutation<RecoveryCodesResult, Error, TotpConfirmRequest>({
    mutationFn: (body) => postApiV1AuthMfaRecoveryCodesRegenerate(body),
    onSuccess: (data) => {
      setNewCodes(data.recoveryCodes);
      setRegenerateError(null);
      refetchCodes();
    },
    onError: (err) => {
      setRegenerateError(getErrorMessage(err));
    },
  });

  const disableMutation = useMutation<void, Error, TotpConfirmRequest>({
    mutationFn: (body) => deleteApiV1AuthMfa(body),
    onSuccess: () => {
      setDisableOpen(false);
      setDisableError(null);
      refetchCodes();
    },
    onError: (err) => {
      setDisableError(getErrorMessage(err));
    },
  });

  const handleRegenerate = (code: string) => {
    setRegenerateError(null);
    regenerateMutation.mutate({ code });
  };

  const handleDisable = (code: string) => {
    setDisableError(null);
    disableMutation.mutate({ code });
  };

  const handleRegenerateClose = () => {
    if (!regenerateMutation.isPending) {
      setRegenerateOpen(false);
      setNewCodes([]);
      setRegenerateError(null);
      regenerateMutation.reset();
    }
  };

  return (
    <Card variant="outlined">
      <CardContent sx={{ p: 3 }}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center', mb: 2 }}>
          <SolarIcon name="quality" size={22} color="text.secondary" />
          <Typography variant="subtitle1" sx={{ fontWeight: 600, flex: 1 }}>
            Two-Factor Authentication
          </Typography>
          <Chip
            label={mfaEnabled ? 'Enabled' : 'Disabled'}
            color={mfaEnabled ? 'success' : 'default'}
            size="small"
          />
        </Stack>

        {mfaEnabled ? (
          <Stack spacing={2}>
            <Typography variant="body2" color="text.secondary">
              {remaining} recovery code{remaining !== 1 ? 's' : ''} remaining.
            </Typography>
            <Stack direction="row" spacing={1}>
              <Button
                size="small"
                variant="outlined"
                onClick={() => {
                  setNewCodes([]);
                  setRegenerateError(null);
                  regenerateMutation.reset();
                  setRegenerateOpen(true);
                }}
              >
                Regenerate codes
              </Button>
              <Button
                size="small"
                variant="outlined"
                color="error"
                onClick={() => {
                  setDisableError(null);
                  disableMutation.reset();
                  setDisableOpen(true);
                }}
              >
                Disable 2FA
              </Button>
            </Stack>
          </Stack>
        ) : (
          <Stack spacing={2}>
            <Typography variant="body2" color="text.secondary">
              Protect your account with a time-based one-time password (TOTP) authenticator app.
            </Typography>
            <Box>
              <Button
                size="small"
                variant="contained"
                onClick={() => navigate('/auth/setup-mfa')}
                startIcon={<SolarIcon name="add" size={16} />}
              >
                Set up 2FA
              </Button>
            </Box>
          </Stack>
        )}
      </CardContent>

      {/* Regenerate codes dialog */}
      <TotpDialog
        open={regenerateOpen}
        title="Regenerate recovery codes"
        description="Enter your authenticator code to generate a new set of recovery codes. Old codes will be invalidated."
        submitLabel="Regenerate"
        isPending={regenerateMutation.isPending}
        error={regenerateError}
        onSubmit={handleRegenerate}
        onClose={handleRegenerateClose}
      >
        {newCodes.length > 0 && (
          <Box sx={{ mt: 2 }}>
            <Alert severity="success" sx={{ mb: 1.5 }}>
              New recovery codes generated. Store them safely — they will not be shown again.
            </Alert>
            <Box
              sx={{
                bgcolor: 'action.hover',
                borderRadius: 1,
                p: 1.5,
                fontFamily: 'monospace',
                fontSize: '0.875rem',
                lineHeight: 2,
              }}
            >
              {newCodes.map((c) => (
                <Typography key={c} variant="body2" sx={{ fontFamily: 'monospace' }}>
                  {c}
                </Typography>
              ))}
            </Box>
          </Box>
        )}
      </TotpDialog>

      {/* Disable MFA dialog */}
      <TotpDialog
        open={disableOpen}
        title="Disable two-factor authentication"
        description="Enter your authenticator code to disable 2FA. Your account will be less secure."
        submitLabel="Disable 2FA"
        submitColor="error"
        isPending={disableMutation.isPending}
        error={disableError}
        onSubmit={handleDisable}
        onClose={() => {
          if (!disableMutation.isPending) {
            setDisableOpen(false);
            setDisableError(null);
          }
        }}
      />
    </Card>
  );
}

// ─── Base64url helpers (WebAuthn) ─────────────────────────────────────────────

function base64UrlDecode(str: string): ArrayBuffer {
  const b64 = str.replace(/-/g, '+').replace(/_/g, '/');
  const binary = atob(b64);
  const buf = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) buf[i] = binary.charCodeAt(i);
  return buf.buffer;
}

function base64UrlEncode(buf: ArrayBuffer): string {
  const bytes = new Uint8Array(buf);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i]);
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

// ─── Passkeys Card ────────────────────────────────────────────────────────────

function PasskeysCard() {
  const qc = useQueryClient();
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null);
  const [registering, setRegistering]   = useState(false);
  const [regError, setRegError]         = useState<string | null>(null);
  const [regName, setRegName]           = useState('');
  const [namePrompt, setNamePrompt]     = useState(false);

  const { data: passkeys, isLoading, refetch } = useGetApiV1AuthPasskey();

  const deleteMutation = useMutation<void, Error, string>({
    mutationFn: (credentialId) => deleteApiV1AuthPasskeyCredentialIdBase64(credentialId),
    onSuccess: () => {
      setDeleteTarget(null);
      refetch();
    },
  });

  const startRegistration = async () => {
    setRegistering(true);
    setRegError(null);
    try {
      const opts = await getApiV1AuthPasskeyAttestationOptions();
      if (!opts?.creationOptionsJson || !opts?.attestationState) throw new Error('No options');

      const parsedOptions = JSON.parse(opts.creationOptionsJson) as PublicKeyCredentialCreationOptions;
      // Decode ArrayBuffer fields from base64url
      parsedOptions.challenge = base64UrlDecode(parsedOptions.challenge as unknown as string);
      parsedOptions.user.id   = base64UrlDecode(parsedOptions.user.id as unknown as string);
      if (parsedOptions.excludeCredentials) {
        parsedOptions.excludeCredentials = parsedOptions.excludeCredentials.map((c) => ({
          ...c,
          id: base64UrlDecode(c.id as unknown as string),
        }));
      }

      const credential = await navigator.credentials.create({ publicKey: parsedOptions }) as PublicKeyCredential;
      if (!credential) throw new Error('User cancelled or device not supported');

      const response = credential.response as AuthenticatorAttestationResponse;
      const credentialJson = JSON.stringify({
        id:       credential.id,
        rawId:    base64UrlEncode(credential.rawId),
        type:     credential.type,
        response: {
          attestationObject: base64UrlEncode(response.attestationObject),
          clientDataJSON:    base64UrlEncode(response.clientDataJSON),
          transports:        response.getTransports?.() ?? [],
        },
      });

      await postApiV1AuthPasskeyRegister({
        credentialJson,
        attestationState: opts.attestationState,
        name: regName || null,
      });

      qc.invalidateQueries({ queryKey: getGetApiV1AuthPasskeyQueryKey() });
      setNamePrompt(false);
      setRegName('');
    } catch (e) {
      setRegError(e instanceof Error ? e.message : 'Registration failed');
    } finally {
      setRegistering(false);
    }
  };

  const rows = passkeys ?? [];

  return (
    <Card variant="outlined">
      <CardContent sx={{ p: 3 }}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center', mb: 2 }}>
          <SolarIcon name="forbidden" size={22} color="text.secondary" />
          <Typography variant="subtitle1" sx={{ fontWeight: 600, flex: 1 }}>
            Passkeys
          </Typography>
          <Button
            size="small"
            variant="outlined"
            startIcon={<SolarIcon name="add" size={16} />}
            onClick={() => setNamePrompt(true)}
            disabled={registering}
          >
            Add passkey
          </Button>
        </Stack>

        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
            <CircularProgress size={24} />
          </Box>
        ) : rows.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No passkeys registered.
          </Typography>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Created</TableCell>
                <TableCell>Transports</TableCell>
                <TableCell align="right" />
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.map((pk) => (
                <TableRow key={pk.credentialId} hover>
                  <TableCell>
                    <Typography variant="body2">{pk.name || 'Passkey'}</Typography>
                  </TableCell>
                  <TableCell>
                    <Tooltip title={dayjs(pk.createdAt).format('YYYY-MM-DD HH:mm')}>
                      <Typography variant="body2" color="text.secondary">
                        {dayjs(pk.createdAt).fromNow()}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <Stack direction="row" spacing={0.5} sx={{ flexWrap: 'wrap', gap: 0.5 }}>
                      {(pk.transports ?? []).map((t) => (
                        <Chip key={t} label={t} size="small" variant="outlined" />
                      ))}
                    </Stack>
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Delete passkey">
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => setDeleteTarget(pk.credentialId)}
                      >
                        <SolarIcon name="delete" size={18} />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Delete passkey"
        description="This passkey will be removed from your account. You won't be able to sign in with it anymore."
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
        onClose={() => {
          if (!deleteMutation.isPending) setDeleteTarget(null);
        }}
        onConfirm={() => {
          if (deleteTarget) deleteMutation.mutate(deleteTarget);
        }}
      />

      {/* Device name prompt before WebAuthn ceremony */}
      <Dialog open={namePrompt} onClose={() => !registering && setNamePrompt(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Register passkey</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Give this device a name so you can identify it later (optional).
          </Typography>
          {regError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {regError}
            </Alert>
          )}
          <TextField
            label="Device name"
            placeholder="e.g. MacBook Touch ID"
            fullWidth
            value={regName}
            onChange={(e) => setRegName(e.target.value)}
            disabled={registering}
            autoFocus
            onKeyDown={(e) => e.key === 'Enter' && !registering && startRegistration()}
          />
        </DialogContent>
        <DialogActions sx={{ px: 2.5, pb: 2 }}>
          <Button variant="outlined" onClick={() => { setNamePrompt(false); setRegName(''); setRegError(null); }} disabled={registering}>
            Cancel
          </Button>
          <Button
            variant="contained"
            onClick={startRegistration}
            disabled={registering}
            startIcon={registering ? <CircularProgress size={16} color="inherit" /> : <SolarIcon name="add" size={16} />}
          >
            {registering ? 'Waiting for device…' : 'Register'}
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  );
}

// ─── Sessions Card ────────────────────────────────────────────────────────────

function SessionsCard() {
  const [revokeAllOpen, setRevokeAllOpen] = useState(false);

  const { data: sessions, isLoading, refetch } = useGetApiV1AuthSessions();

  const revokeOneMutation = useMutation<void, Error, number>({
    mutationFn: (tokenId) => deleteApiV1AuthSessionsTokenId(tokenId),
    onSuccess: () => refetch(),
  });

  const revokeAllMutation = useMutation<void, Error, void>({
    mutationFn: () => postApiV1AuthLogoutAll(),
    onSuccess: () => {
      setRevokeAllOpen(false);
      refetch();
    },
  });

  const rows = sessions ?? [];
  const otherSessions = rows.filter((s) => !s.isCurrent);

  return (
    <Card variant="outlined">
      <CardContent sx={{ p: 3 }}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center', mb: 2 }}>
          <SolarIcon name="profile" size={22} color="text.secondary" />
          <Typography variant="subtitle1" sx={{ fontWeight: 600, flex: 1 }}>
            Active Sessions
          </Typography>
          {otherSessions.length > 0 && (
            <Button
              size="small"
              variant="outlined"
              color="error"
              onClick={() => setRevokeAllOpen(true)}
            >
              Revoke all other
            </Button>
          )}
        </Stack>

        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
            <CircularProgress size={24} />
          </Box>
        ) : rows.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No active sessions found.
          </Typography>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Device / Browser</TableCell>
                <TableCell>IP address</TableCell>
                <TableCell>Created</TableCell>
                <TableCell>Expires</TableCell>
                <TableCell align="right" />
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.map((session) => (
                <TableRow key={String(session.tokenId)} hover>
                  <TableCell>
                    <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                      <Typography variant="body2">
                        {session.deviceInfo ?? 'Unknown device'}
                      </Typography>
                      {session.isCurrent && (
                        <Chip label="Current" color="primary" size="small" />
                      )}
                    </Stack>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2" color="text.secondary">
                      {session.ipAddress ?? '—'}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Tooltip title={dayjs(session.createdAt).format('YYYY-MM-DD HH:mm')}>
                      <Typography variant="body2" color="text.secondary">
                        {dayjs(session.createdAt).fromNow()}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell>
                    <Tooltip title={dayjs(session.expiresAt).format('YYYY-MM-DD HH:mm')}>
                      <Typography variant="body2" color="text.secondary">
                        {dayjs(session.expiresAt).fromNow()}
                      </Typography>
                    </Tooltip>
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title={session.isCurrent ? 'Cannot revoke current session' : 'Revoke session'}>
                      <span>
                        <IconButton
                          size="small"
                          color="error"
                          disabled={
                            session.isCurrent ||
                            (revokeOneMutation.isPending &&
                              revokeOneMutation.variables === Number(session.tokenId))
                          }
                          onClick={() => revokeOneMutation.mutate(Number(session.tokenId))}
                        >
                          {revokeOneMutation.isPending &&
                          revokeOneMutation.variables === Number(session.tokenId) ? (
                            <CircularProgress size={16} color="inherit" />
                          ) : (
                            <SolarIcon name="logout" size={18} />
                          )}
                        </IconButton>
                      </span>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>

      <ConfirmDialog
        open={revokeAllOpen}
        title="Revoke all other sessions"
        description="All sessions except the current one will be signed out immediately."
        confirmLabel="Revoke all"
        confirmColor="error"
        loading={revokeAllMutation.isPending}
        onClose={() => {
          if (!revokeAllMutation.isPending) setRevokeAllOpen(false);
        }}
        onConfirm={() => revokeAllMutation.mutate()}
      />
    </Card>
  );
}

// ─── Security Tab ─────────────────────────────────────────────────────────────

function SecurityTab() {
  return (
    <Box sx={{ maxWidth: 720 }}>
      <Stack spacing={3}>
        <MfaCard />
        <PasskeysCard />
        <SessionsCard />
      </Stack>
    </Box>
  );
}

// ─── Change Password Tab ──────────────────────────────────────────────────────

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Required'),
    newPassword: z
      .string()
      .min(8, 'Minimum 8 characters')
      .regex(/[A-Z]/, 'Need uppercase')
      .regex(/[0-9]/, 'Need digit'),
    confirmPassword: z.string(),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>;

function getPwStrengthScore(pw: string) {
  let s = 0;
  if (pw.length >= 8) s++;
  if (/[A-Z]/.test(pw)) s++;
  if (/[a-z]/.test(pw)) s++;
  if (/[0-9]/.test(pw)) s++;
  if (/[^A-Za-z0-9]/.test(pw)) s++;
  return s;
}

function ChangePasswordTab() {
  const [showCurrent, setShowCurrent] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [snack, setSnack] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    reset,
    formState: { errors, isValid },
  } = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    mode: 'onChange',
  });

  const newPw = watch('newPassword') ?? '';
  const score = useMemo(() => getPwStrengthScore(newPw), [newPw]);
  const strengthColor = score <= 1 ? 'error' : score <= 3 ? 'warning' : 'success';
  const strengthLabel = score === 0 ? '' : score <= 1 ? 'Very weak' : score <= 3 ? 'Moderate' : score === 4 ? 'Strong' : 'Very strong';

  const { mutate, isPending, error: serverError, reset: resetMutation } = useMutation({
    mutationFn: (body: ChangePasswordRequest) => postApiV1AuthChangePassword(body),
    onSuccess: () => {
      reset();
      resetMutation();
      setSnack(true);
    },
  });

  return (
    <Box sx={{ maxWidth: 480 }}>
      <Card variant="outlined">
        <CardContent sx={{ p: 3 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 2.5 }}>
            Change password
          </Typography>
          <Box component="form" onSubmit={handleSubmit((v) => mutate({ currentPassword: v.currentPassword, newPassword: v.newPassword }))} noValidate>
            <Stack spacing={2.5}>
              <TextField
                label="Current password"
                type={showCurrent ? 'text' : 'password'}
                fullWidth
                autoComplete="current-password"
                error={!!errors.currentPassword}
                helperText={errors.currentPassword?.message}
                disabled={isPending}
                slotProps={{ input: { endAdornment: (
                  <InputAdornment position="end">
                    <IconButton size="small" onClick={() => setShowCurrent((v) => !v)} edge="end">
                      <SolarIcon name="view" size={18} />
                    </IconButton>
                  </InputAdornment>
                )}}}
                {...register('currentPassword')}
              />
              <Box>
                <TextField
                  label="New password"
                  type={showNew ? 'text' : 'password'}
                  fullWidth
                  autoComplete="new-password"
                  error={!!errors.newPassword}
                  helperText={errors.newPassword?.message}
                  disabled={isPending}
                  slotProps={{ input: { endAdornment: (
                    <InputAdornment position="end">
                      <IconButton size="small" onClick={() => setShowNew((v) => !v)} edge="end">
                        <SolarIcon name="view" size={18} />
                      </IconButton>
                    </InputAdornment>
                  )}}}
                  {...register('newPassword')}
                />
                {newPw.length > 0 && (
                  <Box sx={{ mt: 1 }}>
                    <LinearProgress
                      variant="determinate"
                      value={(score / 5) * 100}
                      color={strengthColor}
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                    <Typography variant="caption" color={`${strengthColor}.main`} sx={{ mt: 0.5, display: 'block' }}>
                      {strengthLabel}
                    </Typography>
                  </Box>
                )}
              </Box>
              <TextField
                label="Confirm new password"
                type={showConfirm ? 'text' : 'password'}
                fullWidth
                autoComplete="new-password"
                error={!!errors.confirmPassword}
                helperText={errors.confirmPassword?.message}
                disabled={isPending}
                slotProps={{ input: { endAdornment: (
                  <InputAdornment position="end">
                    <IconButton size="small" onClick={() => setShowConfirm((v) => !v)} edge="end">
                      <SolarIcon name="view" size={18} />
                    </IconButton>
                  </InputAdornment>
                )}}}
                {...register('confirmPassword')}
              />
              {serverError && <Alert severity="error">{getErrorMessage(serverError)}</Alert>}
              <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={!isValid || isPending}
                  startIcon={isPending ? <CircularProgress size={16} color="inherit" /> : undefined}
                >
                  {isPending ? 'Saving…' : 'Change password'}
                </Button>
              </Box>
            </Stack>
          </Box>
        </CardContent>
      </Card>
      <Snackbar
        open={snack}
        autoHideDuration={3000}
        onClose={() => setSnack(false)}
        message="Password changed successfully"
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      />
    </Box>
  );
}

// ─── AccountPage ──────────────────────────────────────────────────────────────

export default function AccountPage() {
  const [activeTab, setActiveTab] = useState(0);

  return (
    <PageRoot>
      <PageHeader
        title="My account"
        subtitle="Manage your profile and security settings"
        breadcrumbs={[{ label: 'Account' }]}
      />

      <Tabs
        value={activeTab}
        onChange={(_e, v: number) => setActiveTab(v)}
        sx={{ mb: 3, borderBottom: 1, borderColor: 'divider' }}
      >
        <Tab label="Profile" id="account-tab-0" aria-controls="account-panel-0" />
        <Tab label="Security" id="account-tab-1" aria-controls="account-panel-1" />
        <Tab label="Change password" id="account-tab-2" aria-controls="account-panel-2" />
      </Tabs>

      <Box role="tabpanel" id="account-panel-0" aria-labelledby="account-tab-0" hidden={activeTab !== 0}>
        {activeTab === 0 && <ProfileTab />}
      </Box>
      <Box role="tabpanel" id="account-panel-1" aria-labelledby="account-tab-1" hidden={activeTab !== 1}>
        {activeTab === 1 && <SecurityTab />}
      </Box>
      <Box role="tabpanel" id="account-panel-2" aria-labelledby="account-tab-2" hidden={activeTab !== 2}>
        {activeTab === 2 && <ChangePasswordTab />}
      </Box>
    </PageRoot>
  );
}
