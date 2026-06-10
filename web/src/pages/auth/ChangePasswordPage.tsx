import { useState, useMemo } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import Typography from '@mui/material/Typography';
import LinearProgress from '@mui/material/LinearProgress';
import InputAdornment from '@mui/material/InputAdornment';
import IconButton from '@mui/material/IconButton';
import Stack from '@mui/material/Stack';
import { usePostApiV1AuthChangePassword } from '../../api/auth/auth';
import { getErrorMessage } from '../../lib/apiClient';
import type { ChangePasswordRequest } from '../../api/model/changePasswordRequest';
import SolarIcon from '../../components/SolarIcon';

const schema = z
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

type FormValues = z.infer<typeof schema>;

function getStrengthScore(password: string): number {
  let score = 0;
  if (password.length >= 8) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[a-z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;
  return score;
}

function strengthColor(score: number): 'error' | 'warning' | 'success' {
  if (score <= 1) return 'error';
  if (score <= 3) return 'warning';
  return 'success';
}

function strengthLabel(score: number): string {
  if (score === 0) return '';
  if (score <= 1) return 'Very weak';
  if (score <= 3) return 'Moderate';
  if (score === 4) return 'Strong';
  return 'Very strong';
}

export default function ChangePasswordPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as { from?: string; forced?: boolean } | null;
  const isForced = state?.forced === true;

  const [showCurrent, setShowCurrent] = useState(false);
  const [showNew, setShowNew] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isValid },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    mode: 'onChange',
  });

  const newPasswordValue = watch('newPassword') ?? '';
  const strengthScore = useMemo(() => getStrengthScore(newPasswordValue), [newPasswordValue]);

  const { mutate, isPending, error: serverError } = usePostApiV1AuthChangePassword({
    mutation: {
      onSuccess: () => {
        navigate(state?.from ?? '/dashboard', { replace: true });
      },
    },
  });

  const onSubmit = (values: FormValues) => {
    const body: ChangePasswordRequest = {
      currentPassword: values.currentPassword,
      newPassword: values.newPassword,
    };
    mutate({ data: body });
  };

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 3 }}>
        <Box
          sx={{
            width: 40,
            height: 40,
            borderRadius: 2,
            bgcolor: 'primary.main',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'primary.contrastText',
          }}
        >
          <SolarIcon name="forbidden" size={22} />
        </Box>
        <Box>
          <Typography variant="h6" sx={{ lineHeight: 1.2 }}>AeroMes</Typography>
          <Typography variant="caption" color="text.secondary">Manufacturing Execution System</Typography>
        </Box>
      </Box>

      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>
        Change your password
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Choose a strong password to keep your account secure.
      </Typography>

      {isForced && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Your account requires a password change.
        </Alert>
      )}

      <Stack spacing={2}>
        <TextField
          label="Current password"
          type={showCurrent ? 'text' : 'password'}
          fullWidth
          autoComplete="current-password"
          error={!!errors.currentPassword}
          helperText={errors.currentPassword?.message}
          InputProps={{
            endAdornment: (
              <InputAdornment position="end">
                <IconButton
                  aria-label="Toggle current password visibility"
                  onClick={() => setShowCurrent((v) => !v)}
                  edge="end"
                  size="small"
                >
                  <SolarIcon name="view" size={18} />
                </IconButton>
              </InputAdornment>
            ),
          }}
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
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    aria-label="Toggle new password visibility"
                    onClick={() => setShowNew((v) => !v)}
                    edge="end"
                    size="small"
                  >
                    <SolarIcon name="view" size={18} />
                  </IconButton>
                </InputAdornment>
              ),
            }}
            {...register('newPassword')}
          />
          {newPasswordValue.length > 0 && (
            <Box sx={{ mt: 1 }}>
              <LinearProgress
                variant="determinate"
                value={(strengthScore / 5) * 100}
                color={strengthColor(strengthScore)}
                sx={{ height: 6, borderRadius: 3 }}
              />
              <Typography
                variant="caption"
                color={`${strengthColor(strengthScore)}.main`}
                sx={{ mt: 0.5, display: 'block' }}
              >
                {strengthLabel(strengthScore)}
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
          InputProps={{
            endAdornment: (
              <InputAdornment position="end">
                <IconButton
                  aria-label="Toggle confirm password visibility"
                  onClick={() => setShowConfirm((v) => !v)}
                  edge="end"
                  size="small"
                >
                  <SolarIcon name="view" size={18} />
                </IconButton>
              </InputAdornment>
            ),
          }}
          {...register('confirmPassword')}
        />

        {serverError && (
          <Alert severity="error">
            {getErrorMessage(serverError)}
          </Alert>
        )}

        <Button
          type="submit"
          variant="contained"
          fullWidth
          size="large"
          disabled={!isValid || isPending}
          startIcon={<SolarIcon name="complete" size={18} />}
        >
          {isPending ? 'Saving…' : 'Change password'}
        </Button>

        {!isForced && (
          <Box sx={{ textAlign: 'center' }}>
            <Link
              to={state?.from ?? '/dashboard'}
              style={{ display: 'inline-flex', alignItems: 'center', gap: 4, textDecoration: 'none' }}
            >
              <SolarIcon name="back" size={16} />
              <Typography variant="body2" color="text.secondary" component="span">
                Back
              </Typography>
            </Link>
          </Box>
        )}
      </Stack>
    </Box>
  );
}
