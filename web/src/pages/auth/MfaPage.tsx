import { useState, useEffect, useRef } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import { useMutation } from '@tanstack/react-query';
import { useAuth, type AuthUser, type UserRole } from '../../contexts/AuthContext';
import {
  postApiV1AuthMfaVerify,
  postApiV1AuthMfaSendOtp,
} from '../../api/mfa/mfa';
import { getErrorMessage } from '../../lib/apiClient';
import type { LoginResponse } from '../../api/model/loginResponse';

interface LocationState {
  mfaToken?: string;
  email?: string;
  from?: string;
}

const OTP_COOLDOWN_SECONDS = 60;

function buildAuthUser(res: LoginResponse): AuthUser {
  return {
    id: res.email,
    name: res.fullName,
    email: res.email,
    roles: res.roles as UserRole[],
  };
}

export default function MfaPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const { login } = useAuth();

  const state = (location.state ?? {}) as LocationState;
  const mfaToken = state.mfaToken ?? '';
  const email = state.email ?? '';
  const from = state.from ?? '/dashboard';

  const [activeTab, setActiveTab] = useState(0);

  // Authenticator tab
  const [totpCode, setTotpCode] = useState('');

  // Email tab
  const [emailCode, setEmailCode] = useState('');
  const [otpCooldown, setOtpCooldown] = useState(0);
  const cooldownRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // Recovery tab
  const [recoveryCode, setRecoveryCode] = useState('');

  const [error, setError] = useState('');

  // Redirect if no mfaToken in state
  useEffect(() => {
    if (!mfaToken) {
      navigate('/auth/login', { replace: true });
    }
  }, [mfaToken, navigate]);

  // Cleanup interval on unmount
  useEffect(() => {
    return () => {
      if (cooldownRef.current !== null) clearInterval(cooldownRef.current);
    };
  }, []);

  const verifyMutation = useMutation({
    mutationFn: (code: string) =>
      postApiV1AuthMfaVerify({ mfaToken, code }),
    onSuccess: (res: LoginResponse) => {
      const user = buildAuthUser(res);
      login(user, res.accessToken);
      if (res.forcePasswordChange) {
        navigate('/auth/change-password', { state: { forced: true }, replace: true });
      } else {
        navigate(from, { replace: true });
      }
    },
    onError: (err: unknown) => {
      setError(getErrorMessage(err, 'Invalid verification code.'));
    },
  });

  const sendOtpMutation = useMutation({
    mutationFn: () => postApiV1AuthMfaSendOtp({ mfaToken }),
    onSuccess: () => {
      setError('');
      setOtpCooldown(OTP_COOLDOWN_SECONDS);
      cooldownRef.current = setInterval(() => {
        setOtpCooldown((prev) => {
          if (prev <= 1) {
            if (cooldownRef.current !== null) {
              clearInterval(cooldownRef.current);
              cooldownRef.current = null;
            }
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    },
    onError: (err: unknown) => {
      setError(getErrorMessage(err, 'Failed to send code.'));
    },
  });

  function handleTabChange(_: React.SyntheticEvent, newValue: number) {
    setActiveTab(newValue);
    setError('');
    verifyMutation.reset();
  }

  function submitCode(code: string) {
    if (!code.trim()) return;
    setError('');
    verifyMutation.mutate(code.trim());
  }

  function handleTotpChange(value: string) {
    const digits = value.replace(/\D/g, '').slice(0, 6);
    setTotpCode(digits);
    if (digits.length === 6) {
      submitCode(digits);
    }
  }

  function handleEmailCodeChange(value: string) {
    const digits = value.replace(/\D/g, '').slice(0, 6);
    setEmailCode(digits);
    if (digits.length === 6) {
      submitCode(digits);
    }
  }

  function handleRecoverySubmit(e: React.FormEvent) {
    e.preventDefault();
    submitCode(recoveryCode);
  }

  const isLoading = verifyMutation.isPending;
  const displayError =
    error ||
    (verifyMutation.isError ? getErrorMessage(verifyMutation.error, 'Invalid verification code.') : '');

  if (!mfaToken) return null;

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>
        Two-factor verification
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Verify your identity to continue signing in.
      </Typography>

      {displayError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {displayError}
        </Alert>
      )}

      <Tabs
        value={activeTab}
        onChange={handleTabChange}
        variant="fullWidth"
        sx={{ mb: 3, borderBottom: 1, borderColor: 'divider' }}
      >
        <Tab label="Authenticator app" />
        <Tab label="Email" />
        <Tab label="Recovery code" />
      </Tabs>

      {/* Tab 0: Authenticator app */}
      {activeTab === 0 && (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Enter the 6-digit code from your authenticator app.
          </Typography>
          <TextField
            label="6-digit code"
            value={totpCode}
            onChange={(e) => handleTotpChange(e.target.value)}
            inputMode="numeric"
            slotProps={{ htmlInput: { maxLength: 6 } }}
            fullWidth
            autoFocus
            disabled={isLoading}
            placeholder="000000"
          />
          <Button
            variant="contained"
            fullWidth
            size="large"
            disabled={isLoading || totpCode.length !== 6}
            onClick={() => submitCode(totpCode)}
            startIcon={isLoading ? <CircularProgress size={16} color="inherit" /> : undefined}
          >
            {isLoading ? 'Verifying…' : 'Verify'}
          </Button>
        </Box>
      )}

      {/* Tab 1: Email OTP */}
      {activeTab === 1 && (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          <Typography variant="body2" color="text.secondary">
            {email
              ? `We will send a verification code to ${email}.`
              : 'We will send a verification code to your registered email.'}
          </Typography>
          <Button
            variant="outlined"
            fullWidth
            disabled={sendOtpMutation.isPending || otpCooldown > 0}
            onClick={() => sendOtpMutation.mutate()}
            startIcon={
              sendOtpMutation.isPending ? (
                <CircularProgress size={16} color="inherit" />
              ) : undefined
            }
          >
            {sendOtpMutation.isPending
              ? 'Sending…'
              : otpCooldown > 0
                ? `Resend code (${otpCooldown}s)`
                : email
                  ? `Send code to ${email}`
                  : 'Send code'}
          </Button>
          <TextField
            label="6-digit code"
            value={emailCode}
            onChange={(e) => handleEmailCodeChange(e.target.value)}
            inputMode="numeric"
            slotProps={{ htmlInput: { maxLength: 6 } }}
            fullWidth
            disabled={isLoading}
            placeholder="000000"
          />
          <Button
            variant="contained"
            fullWidth
            size="large"
            disabled={isLoading || emailCode.length !== 6}
            onClick={() => submitCode(emailCode)}
            startIcon={isLoading ? <CircularProgress size={16} color="inherit" /> : undefined}
          >
            {isLoading ? 'Verifying…' : 'Verify'}
          </Button>
        </Box>
      )}

      {/* Tab 2: Recovery code */}
      {activeTab === 2 && (
        <Box
          component="form"
          onSubmit={handleRecoverySubmit}
          sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
        >
          <Typography variant="body2" color="text.secondary">
            Enter one of your saved recovery codes. Each code can only be used once.
          </Typography>
          <TextField
            label="Recovery code"
            value={recoveryCode}
            onChange={(e) => setRecoveryCode(e.target.value)}
            fullWidth
            autoFocus
            disabled={isLoading}
            placeholder="xxxxxxxx-xxxx"
          />
          <Button
            type="submit"
            variant="contained"
            fullWidth
            size="large"
            disabled={isLoading || !recoveryCode.trim()}
            startIcon={isLoading ? <CircularProgress size={16} color="inherit" /> : undefined}
          >
            {isLoading ? 'Verifying…' : 'Verify'}
          </Button>
        </Box>
      )}

      <Box sx={{ mt: 3, textAlign: 'center' }}>
        <Typography
          component={Link}
          to="/auth/login"
          variant="body2"
          sx={{ color: 'text.secondary', textDecoration: 'none', '&:hover': { textDecoration: 'underline' } }}
        >
          &larr; Back to sign in
        </Typography>
      </Box>
    </Box>
  );
}
