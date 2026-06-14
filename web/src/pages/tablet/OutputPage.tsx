import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  IconButton,
  LinearProgress,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import SolarIcon from '../../components/SolarIcon';
import { useGetApiV1JobsId } from '../../api/jobs/jobs';
import { postApiV1ProductionSubmitOutput } from '../../api/production/production';
import { useTabletSession } from '../../contexts/TabletSessionContext';
import { getErrorMessage } from '../../lib/apiClient';

function numVal(v: number | string | null | undefined): number {
  if (v == null) return 0;
  return typeof v === 'number' ? v : parseFloat(v as string) || 0;
}

export default function OutputPage() {
  const navigate = useNavigate();
  const { session } = useTabletSession();
  const [okQty, setOkQty] = useState(0);
  const [ngQty, setNgQty] = useState(0);
  const [note, setNote] = useState('');
  const [submitError, setSubmitError] = useState('');
  const [submitted, setSubmitted] = useState(false);

  const { data: jobResp, isLoading } = useGetApiV1JobsId(session.jobId ?? 0, {
    query: { enabled: !!session.jobId, refetchInterval: 30_000 },
  });

  const job = (jobResp as { data?: { productionLogs?: { qtyOK: number | string; qtyNG: number | string }[] } })?.data;

  const loggedOK = useMemo(() => {
    const logs = job?.productionLogs ?? [];
    return logs.reduce((s, l) => s + numVal(l.qtyOK), 0);
  }, [job]);

  const target = session.targetQty || 1;
  const progress = Math.min(100, (loggedOK / target) * 100);

  const submitMutation = useMutation({
    mutationFn: () =>
      postApiV1ProductionSubmitOutput({
        jobId: session.jobId!,
        qtyOk: okQty,
        qtyNg: ngQty,
        deviceIp: null,
        notes: note || null,
        timestamp: null,
        defects: [],
      }),
    onSuccess: () => {
      setSubmitted(true);
      setOkQty(0);
      setNgQty(0);
      setNote('');
      setSubmitError('');
    },
    onError: (err) => setSubmitError(getErrorMessage(err)),
  });

  const adjustOk = (delta: number) => setOkQty((v) => Math.max(0, v + delta));
  const adjustNg = (delta: number) => setNgQty((v) => Math.max(0, v + delta));

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
        <Typography variant="h6" sx={{ fontWeight: 600 }}>Log Output</Typography>
      </Stack>

      {/* Job Status */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1.5 }}>
            {session.woCode} · {session.machineCode} · RUNNING
          </Typography>
          {isLoading ? (
            <CircularProgress size={24} />
          ) : (
            <>
              <Grid container spacing={2}>
                <Grid size={{ xs: 4 }}>
                  <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>TARGET</Typography>
                  <Typography variant="h5" sx={{ fontWeight: 700 }}>{target}</Typography>
                </Grid>
                <Grid size={{ xs: 4 }}>
                  <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>OK LOGGED</Typography>
                  <Typography variant="h5" sx={{ fontWeight: 700, color: '#15803D' }}>{loggedOK}</Typography>
                </Grid>
                <Grid size={{ xs: 4 }}>
                  <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>REMAINING</Typography>
                  <Typography variant="h5" sx={{ fontWeight: 700 }} color="text.secondary">
                    {Math.max(0, target - loggedOK)}
                  </Typography>
                </Grid>
              </Grid>
              <LinearProgress variant="determinate" value={progress} sx={{ mt: 1.5, height: 8, borderRadius: 1 }} />
            </>
          )}
        </CardContent>
      </Card>

      {submitted && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSubmitted(false)}>
          Output submitted successfully.
        </Alert>
      )}
      {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}

      {/* Log Output */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid size={{ xs: 6 }}>
              <Typography variant="subtitle2" sx={{ mb: 1, color: '#15803D' }}>OK Qty</Typography>
              <TextField
                type="number"
                fullWidth
                value={okQty}
                onChange={(e) => setOkQty(Math.max(0, parseInt(e.target.value) || 0))}
                sx={{ '& input': { fontSize: '2rem', textAlign: 'center', py: 2 } }}
              />
              <Stack direction="row" sx={{ gap: 1, mt: 1 }}>
                <Button variant="outlined" sx={{ flex: 1, minHeight: 48 }} onClick={() => adjustOk(-1)}>-1</Button>
                <Button variant="outlined" sx={{ flex: 1, minHeight: 48 }} onClick={() => adjustOk(1)}>+1</Button>
              </Stack>
            </Grid>
            <Grid size={{ xs: 6 }}>
              <Typography variant="subtitle2" sx={{ mb: 1, color: '#DC2626' }}>NG Qty</Typography>
              <TextField
                type="number"
                fullWidth
                value={ngQty}
                onChange={(e) => setNgQty(Math.max(0, parseInt(e.target.value) || 0))}
                sx={{ '& input': { fontSize: '2rem', textAlign: 'center', py: 2 } }}
              />
              <Stack direction="row" sx={{ gap: 1, mt: 1 }}>
                <Button variant="outlined" color="error" sx={{ flex: 1, minHeight: 48 }} onClick={() => adjustNg(-1)}>-1</Button>
                <Button variant="outlined" color="error" sx={{ flex: 1, minHeight: 48 }} onClick={() => adjustNg(1)}>+1</Button>
              </Stack>
            </Grid>
          </Grid>

          <TextField
            label="Note (optional)"
            fullWidth
            multiline
            rows={2}
            value={note}
            onChange={(e) => setNote(e.target.value)}
            sx={{ mt: 2 }}
          />
        </CardContent>
      </Card>

      <Stack direction="row" sx={{ gap: 2 }}>
        <Button variant="outlined" color="warning" sx={{ flex: 1, minHeight: 52 }} onClick={() => navigate('/tablet/station/downtime/start')}>
          Log Downtime
        </Button>
        <Button
          variant="contained"
          color="success"
          sx={{ flex: 2, minHeight: 52 }}
          disabled={okQty === 0 && ngQty === 0 || submitMutation.isPending}
          onClick={() => submitMutation.mutate()}
        >
          {submitMutation.isPending ? <CircularProgress size={20} color="inherit" /> : 'Submit Output'}
        </Button>
        <Button variant="outlined" sx={{ flex: 1, minHeight: 52 }} onClick={() => navigate('/tablet/station/finish')}>
          Finish Job
        </Button>
      </Stack>
    </Box>
  );
}
