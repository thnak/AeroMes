import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import InputAdornment from '@mui/material/InputAdornment';
import IconButton from '@mui/material/IconButton';
import { useAuth, type AuthUser, type UserRole } from '../../contexts/AuthContext';
import { usePostApiV1AuthLogin } from '../../api/auth/auth';
import { getErrorMessage } from '../../lib/apiClient';
import type { LoginResponse } from '../../api/model/loginResponse';
import type { MfaPendingResult } from '../../api/model/mfaPendingResult';
import SolarIcon from '../../components/SolarIcon';

const schema = z.object({
  email: z.string().email('Enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
});

type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string })?.from ?? '/dashboard';

  const [showPassword, setShowPassword] = useState(false);

  const { mutate, isPending, error } = usePostApiV1AuthLogin();

  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (values: FormValues) => {
    mutate(
      { data: { email: values.email, password: values.password } },
      {
        onSuccess: (data) => {
          const result = data as LoginResponse | MfaPendingResult;

          if ('requiresMfa' in result && result.requiresMfa) {
            navigate('/auth/login/mfa', {
              state: { mfaToken: result.mfaToken, email: getValues('email') },
            });
            return;
          }

          const loginResponse = result as LoginResponse;
          const authUser: AuthUser = {
            id: loginResponse.email,
            name: loginResponse.fullName,
            email: loginResponse.email,
            roles: loginResponse.roles as UserRole[],
          };

          login(authUser, loginResponse.accessToken);

          if (loginResponse.forcePasswordChange) {
            navigate('/auth/change-password', { state: { forced: true } });
          } else {
            navigate(from, { replace: true });
          }
        },
      },
    );
  };

  const errorMessage = error
    ? (() => {
        const axErr = error as { response?: { status?: number } };
        if (axErr.response?.status === 429) {
          return 'Account locked due to too many failed attempts.';
        }
        return getErrorMessage(error);
      })()
    : null;

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)}>
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
          <SolarIcon name="machineOn" size={22} />
        </Box>
        <Box>
          <Typography variant="h6" sx={{ lineHeight: 1.2 }}>AeroMes</Typography>
          <Typography variant="caption" color="text.secondary">Manufacturing Execution System</Typography>
        </Box>
      </Box>

      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>Sign in</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Enter your credentials to continue
      </Typography>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField
          label="Email"
          type="email"
          autoFocus
          autoComplete="email"
          fullWidth
          error={!!errors.email}
          helperText={errors.email?.message}
          {...register('email')}
        />
        <TextField
          label="Password"
          type={showPassword ? 'text' : 'password'}
          autoComplete="current-password"
          fullWidth
          error={!!errors.password}
          helperText={errors.password?.message}
          InputProps={{
            endAdornment: (
              <InputAdornment position="end">
                <IconButton
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                  onClick={() => setShowPassword((v) => !v)}
                  edge="end"
                  size="small"
                >
                  <SolarIcon name={showPassword ? 'eyeClosed' : 'eye'} size={20} />
                </IconButton>
              </InputAdornment>
            ),
          }}
          {...register('password')}
        />
      </Box>

      {errorMessage && (
        <Alert severity="error" sx={{ mt: 2 }}>
          {errorMessage}
        </Alert>
      )}

      <Button
        type="submit"
        variant="contained"
        fullWidth
        size="large"
        disabled={isPending}
        sx={{ mt: 3, mb: 2 }}
        startIcon={isPending ? <CircularProgress size={16} color="inherit" /> : undefined}
      >
        {isPending ? 'Signing in…' : 'Sign in'}
      </Button>

      <Divider sx={{ my: 2 }}>
        <Typography variant="caption" color="text.secondary">or</Typography>
      </Divider>

      <Button
        variant="outlined"
        fullWidth
        startIcon={<SolarIcon name="profile" size={18} />}
        disabled={isPending}
      >
        Sign in with Passkey
      </Button>
    </Box>
  );
}
