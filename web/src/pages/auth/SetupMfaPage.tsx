import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';

import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import Alert from '@mui/material/Alert';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import IconButton from '@mui/material/IconButton';
import Skeleton from '@mui/material/Skeleton';
import Stepper from '@mui/material/Stepper';
import Step from '@mui/material/Step';
import StepLabel from '@mui/material/StepLabel';
import TextField from '@mui/material/TextField';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import SaveAltIcon from '@mui/icons-material/SaveAlt';

import { useGetApiV1AuthMfaSetup, postApiV1AuthMfaSetupConfirm } from '../../api/mfa/mfa';
import type { TotpSetupResult } from '../../api/model/totpSetupResult';
import type { TotpEnabledResult } from '../../api/model/totpEnabledResult';
import type { TotpConfirmRequest } from '../../api/model/totpConfirmRequest';

// ─── Constants ────────────────────────────────────────────────────────────────

const WIZARD_STEPS = ['Scan secret', 'Verify code', 'Save recovery codes'];

// ─── Zod schema ───────────────────────────────────────────────────────────────

const confirmSchema = z.object({
  code: z
    .string()
    .length(6, 'Code must be exactly 6 digits')
    .regex(/^\d+$/, 'Code must contain only digits'),
});

type ConfirmFormValues = z.infer<typeof confirmSchema>;

// ─── Step 1 — Show secret ────────────────────────────────────────────────────

interface StepSecretProps {
  setup: TotpSetupResult | undefined;
  isLoading: boolean;
  onContinue: () => void;
}

function StepSecret({ setup, isLoading, onContinue }: StepSecretProps) {
  const handleCopySecret = () => {
    if (setup?.secret) {
      navigator.clipboard.writeText(setup.secret);
    }
  };

  const handleCopyUri = () => {
    if (setup?.otpauthUri) {
      navigator.clipboard.writeText(setup.otpauthUri);
    }
  };

  return (
    <Box>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Open your authenticator app, add account manually, enter the key above.
      </Typography>

      <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5 }}>
        Secret key
      </Typography>

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1,
          mt: 0.5,
          mb: 2,
          p: 1.5,
          borderRadius: 1,
          bgcolor: 'action.hover',
          border: '1px solid',
          borderColor: 'divider',
        }}
      >
        {isLoading ? (
          <Skeleton variant="text" width="100%" height={24} />
        ) : (
          <Typography
            component="span"
            sx={{ fontFamily: 'monospace', fontSize: '0.95rem', flex: 1, wordBreak: 'break-all' }}
          >
            {setup?.secret ?? '—'}
          </Typography>
        )}
        <IconButton size="small" onClick={handleCopySecret} disabled={isLoading || !setup?.secret} aria-label="Copy secret key">
          <ContentCopyIcon fontSize="small" />
        </IconButton>
      </Box>

      <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600, textTransform: 'uppercase', letterSpacing: 0.5 }}>
        OTP auth URI
      </Typography>

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1,
          mt: 0.5,
          mb: 3,
          p: 1.5,
          borderRadius: 1,
          bgcolor: 'action.hover',
          border: '1px solid',
          borderColor: 'divider',
        }}
      >
        {isLoading ? (
          <Skeleton variant="text" width="100%" height={24} />
        ) : (
          <Typography
            component="span"
            variant="caption"
            color="text.secondary"
            sx={{ fontFamily: 'monospace', flex: 1, wordBreak: 'break-all' }}
          >
            {setup?.otpauthUri ?? '—'}
          </Typography>
        )}
        <IconButton size="small" onClick={handleCopyUri} disabled={isLoading || !setup?.otpauthUri} aria-label="Copy OTP auth URI">
          <ContentCopyIcon fontSize="small" />
        </IconButton>
      </Box>

      <Button
        variant="contained"
        fullWidth
        size="large"
        disabled={isLoading || !setup}
        onClick={onContinue}
      >
        Continue
      </Button>
    </Box>
  );
}

// ─── Step 2 — Confirm code ───────────────────────────────────────────────────

interface StepConfirmProps {
  onSuccess: (result: TotpEnabledResult) => void;
  onBack: () => void;
}

function StepConfirm({ onSuccess, onBack }: StepConfirmProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ConfirmFormValues>({
    resolver: zodResolver(confirmSchema),
  });

  const confirmMutation = useMutation<TotpEnabledResult, Error, TotpConfirmRequest>({
    mutationFn: (body) => postApiV1AuthMfaSetupConfirm(body),
    onSuccess,
  });

  const onSubmit = (values: ConfirmFormValues) => {
    confirmMutation.mutate({ code: values.code });
  };

  const errorMessage = confirmMutation.error?.message ?? null;

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)}>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Enter the 6-digit code from your authenticator app to confirm setup.
      </Typography>

      {errorMessage && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {errorMessage}
        </Alert>
      )}

      <TextField
        label="6-digit code"
        fullWidth
        autoFocus
        autoComplete="one-time-code"
        slotProps={{ htmlInput: { maxLength: 6, inputMode: 'numeric', pattern: '[0-9]*' } }}
        error={!!errors.code}
        helperText={errors.code?.message}
        disabled={confirmMutation.isPending}
        {...register('code')}
        sx={{ mb: 3 }}
      />

      <Box sx={{ display: 'flex', gap: 1.5 }}>
        <Button
          variant="outlined"
          fullWidth
          onClick={onBack}
          disabled={confirmMutation.isPending}
        >
          Back
        </Button>
        <Button
          type="submit"
          variant="contained"
          fullWidth
          disabled={confirmMutation.isPending}
          startIcon={confirmMutation.isPending ? <CircularProgress size={16} color="inherit" /> : undefined}
        >
          {confirmMutation.isPending ? 'Verifying…' : 'Verify'}
        </Button>
      </Box>
    </Box>
  );
}

// ─── Step 3 — Recovery codes ─────────────────────────────────────────────────

interface StepRecoveryProps {
  result: TotpEnabledResult;
}

function StepRecovery({ result }: StepRecoveryProps) {
  const navigate = useNavigate();
  const codes = result.recoveryCodes ?? [];

  const handleCopyAll = () => {
    navigator.clipboard.writeText(codes.join('\n'));
  };

  return (
    <Box>
      <Alert severity="warning" sx={{ mb: 2 }}>
        Save these codes in a secure place. Each can only be used once.
      </Alert>

      {codes.length > 0 ? (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: '1fr 1fr',
            gap: 1,
            mb: 3,
          }}
        >
          {codes.map((code) => (
            <Chip
              key={code}
              label={code}
              variant="outlined"
              sx={{ fontFamily: 'monospace', fontSize: '0.8rem', justifyContent: 'flex-start' }}
            />
          ))}
        </Box>
      ) : (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          No recovery codes were generated.
        </Typography>
      )}

      <Button
        variant="outlined"
        fullWidth
        startIcon={<SaveAltIcon />}
        onClick={handleCopyAll}
        disabled={codes.length === 0}
        sx={{ mb: 1.5 }}
      >
        Copy all codes
      </Button>

      <Button
        variant="contained"
        fullWidth
        onClick={() => navigate('/dashboard')}
      >
        I've saved my codes
      </Button>
    </Box>
  );
}

// ─── Main component ───────────────────────────────────────────────────────────

export default function SetupMfaPage() {
  const [step, setStep] = useState(0);
  const [enabledResult, setEnabledResult] = useState<TotpEnabledResult | null>(null);

  const setup = useGetApiV1AuthMfaSetup({
    query: { enabled: step === 0 },
  });

  const handleConfirmSuccess = (result: TotpEnabledResult) => {
    setEnabledResult(result);
    setStep(2);
  };

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>
        Set up two-factor authentication
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Protect your account with a TOTP authenticator app.
      </Typography>

      <Stepper activeStep={step} sx={{ mb: 4 }}>
        {WIZARD_STEPS.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {step === 0 && (
        <StepSecret
          setup={setup.data}
          isLoading={setup.isLoading}
          onContinue={() => setStep(1)}
        />
      )}

      {step === 1 && (
        <StepConfirm
          onSuccess={handleConfirmSuccess}
          onBack={() => setStep(0)}
        />
      )}

      {step === 2 && enabledResult && (
        <StepRecovery result={enabledResult} />
      )}
    </Box>
  );
}
