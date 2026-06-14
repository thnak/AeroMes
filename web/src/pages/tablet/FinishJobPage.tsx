import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  CircularProgress,
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import SolarIcon from '../../components/SolarIcon';
import { useGetApiV1JobsId } from '../../api/jobs/jobs';
import { postApiV1JobsJobIdFinish } from '../../api/jobs/jobs';
import { useTabletSession } from '../../contexts/TabletSessionContext';
import { getErrorMessage } from '../../lib/apiClient';

const FINAL_CHECKS = [
  'Output quantities confirmed',
  'Machine cleaned and secured',
  'Material returned to storage',
];

function numVal(v: number | string | null | undefined): number {
  if (v == null) return 0;
  return typeof v === 'number' ? v : parseFloat(v as string) || 0;
}

export default function FinishJobPage() {
  const navigate = useNavigate();
  const { session, update } = useTabletSession();
  const [checks, setChecks] = useState([false, false, false]);
  const [finishError, setFinishError] = useState('');

  const { data: jobResp, isLoading } = useGetApiV1JobsId(session.jobId ?? 0, {
    query: { enabled: !!session.jobId },
  });

  const job = (jobResp as { data?: { productionLogs?: { qtyOK: number | string; qtyNG: number | string }[] } })?.data;

  const { okQty, ngQty } = useMemo(() => {
    const logs = job?.productionLogs ?? [];
    return {
      okQty: logs.reduce((s, l) => s + numVal(l.qtyOK), 0),
      ngQty: logs.reduce((s, l) => s + numVal(l.qtyNG), 0),
    };
  }, [job]);

  const target = session.targetQty || 1;
  const progress = Math.min(100, (okQty / target) * 100);
  const allChecked = checks.every(Boolean);

  const toggleCheck = (index: number) => setChecks((prev) => prev.map((v, i) => (i === index ? !v : v)));

  const finishMutation = useMutation({
    mutationFn: () => postApiV1JobsJobIdFinish(session.jobId!, { endTime: null }),
    onSuccess: () => {
      update({ jobId: null, workOrderId: null, woCode: '', targetQty: 0, machineCode: '', machineName: '' });
      navigate('/tablet/station');
    },
    onError: (err) => setFinishError(getErrorMessage(err)),
  });

  if (!session.jobId) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
        <Typography color="text.secondary" sx={{ mb: 2 }}>No active job.</Typography>
        <Button variant="outlined" onClick={() => navigate('/tablet/station')}>Back to Stations</Button>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ mb: 3, alignItems: 'center', gap: 1 }}>
        <IconButton onClick={() => navigate(-1)} size="small">
          <SolarIcon name="back" size={22} />
        </IconButton>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>Finish Job</Typography>
      </Stack>

      {/* Job Summary */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>JOB SUMMARY</Typography>
          <Typography variant="h5" sx={{ fontFamily: 'monospace', mt: 0.5, fontWeight: 700 }}>
            {session.woCode}
          </Typography>

          <Divider sx={{ my: 2 }} />

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="caption" color="text.disabled" sx={{ display: 'block', mb: 0.25 }}>MACHINE</Typography>
              <Typography variant="body2" sx={{ fontWeight: 600 }}>
                {session.machineCode}{session.machineName ? ` · ${session.machineName}` : ''}
              </Typography>
            </Grid>
          </Grid>

          <Divider sx={{ my: 2 }} />

          {isLoading ? (
            <CircularProgress size={24} />
          ) : (
            <>
              <Grid container spacing={2}>
                <Grid size={{ xs: 4 }}>
                  <Typography variant="h4" sx={{ fontWeight: 700, color: '#15803D' }}>{okQty}</Typography>
                  <Typography variant="caption" color="text.secondary">OK Qty</Typography>
                </Grid>
                <Grid size={{ xs: 4 }}>
                  <Typography variant="h4" sx={{ fontWeight: 700, color: '#DC2626' }}>{ngQty}</Typography>
                  <Typography variant="caption" color="text.secondary">NG Qty</Typography>
                </Grid>
                <Grid size={{ xs: 4 }}>
                  <Typography variant="h4" sx={{ fontWeight: 700, color: '#1D4ED8' }}>{okQty + ngQty}</Typography>
                  <Typography variant="caption" color="text.secondary">Total</Typography>
                </Grid>
              </Grid>

              <LinearProgress
                variant="determinate"
                value={progress}
                color="success"
                sx={{ mt: 2, height: 8, borderRadius: 1 }}
              />
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                {progress.toFixed(1)}% of target ({target} EA)
              </Typography>
            </>
          )}
        </CardContent>
      </Card>

      {/* Final Checks */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 1.5 }}>Final check before closing</Typography>
          <Stack spacing={0.5}>
            {FINAL_CHECKS.map((item, index) => (
              <FormControlLabel
                key={item}
                control={<Checkbox checked={checks[index]} onChange={() => toggleCheck(index)} color="success" />}
                label={<Typography variant="body2" sx={{ fontWeight: checks[index] ? 600 : 400 }}>{item}</Typography>}
                sx={{ mx: 0 }}
              />
            ))}
          </Stack>
        </CardContent>
      </Card>

      {finishError && <Alert severity="error" sx={{ mb: 2 }}>{finishError}</Alert>}

      <Stack direction="row" sx={{ gap: 2 }}>
        <Button variant="outlined" sx={{ flex: 1, minHeight: 52 }} onClick={() => navigate(-1)}>
          Cancel
        </Button>
        <Button
          variant="contained"
          color="success"
          disabled={!allChecked || finishMutation.isPending}
          sx={{ flex: 2, minHeight: 56, fontSize: '1rem' }}
          onClick={() => finishMutation.mutate()}
        >
          {finishMutation.isPending ? <CircularProgress size={22} color="inherit" /> : 'Complete Job'}
        </Button>
      </Stack>
    </Box>
  );
}
