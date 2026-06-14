import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Stack,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { postApiV1DowntimeDowntimeLogIdEnd } from '../../api/downtime/downtime';
import { useTabletSession } from '../../contexts/TabletSessionContext';
import { getErrorMessage } from '../../lib/apiClient';

function formatElapsed(startIso: string | null): string {
  if (!startIso) return '00:00:00';
  const elapsed = Math.floor((Date.now() - new Date(startIso).getTime()) / 1000);
  const h = Math.floor(elapsed / 3600);
  const m = Math.floor((elapsed % 3600) / 60);
  const s = elapsed % 60;
  return [h, m, s].map((v) => String(v).padStart(2, '0')).join(':');
}

export default function DowntimeActivePage() {
  const navigate = useNavigate();
  const { session, update } = useTabletSession();
  const [elapsed, setElapsed] = useState(() => formatElapsed(session.downtimeStartTime));
  const [resolveError, setResolveError] = useState('');

  useEffect(() => {
    const id = setInterval(() => setElapsed(formatElapsed(session.downtimeStartTime)), 1000);
    return () => clearInterval(id);
  }, [session.downtimeStartTime]);

  const resolveMutation = useMutation({
    mutationFn: () => {
      const id = session.downtimeLogId ?? 0;
      return postApiV1DowntimeDowntimeLogIdEnd(id, { endTime: new Date().toISOString() });
    },
    onSuccess: () => {
      update({ downtimeLogId: null, downtimeReason: '', downtimeStartTime: null });
      navigate('/tablet/station/output');
    },
    onError: (err) => setResolveError(getErrorMessage(err)),
  });

  const startedAt = session.downtimeStartTime
    ? new Date(session.downtimeStartTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    : '—';

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      {/* Active downtime header card */}
      <Card
        sx={{
          mb: 2,
          bgcolor: alpha('#DC2626', 0.05),
          border: '2px solid',
          borderColor: '#DC2626',
        }}
      >
        <CardContent>
          <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center' }}>
            <Box>
              <Typography variant="caption" sx={{ color: '#DC2626', fontWeight: 700, display: 'block' }}>
                MACHINE DOWN
              </Typography>
              <Typography variant="subtitle2">{session.downtimeReason || 'Downtime'}</Typography>
            </Box>
            <Chip
              label="● ACTIVE"
              sx={{ bgcolor: alpha('#DC2626', 0.12), color: '#DC2626', fontWeight: 700, fontSize: '0.75rem' }}
            />
          </Stack>

          <Typography
            sx={{
              fontSize: '3rem', fontWeight: 700, fontFamily: 'monospace',
              color: '#DC2626', mt: 2, textAlign: 'center',
            }}
          >
            {elapsed}
          </Typography>

          <Typography variant="caption" sx={{ color: 'text.secondary', textAlign: 'center', display: 'block' }}>
            Started {startedAt}
          </Typography>
        </CardContent>
      </Card>

      {/* Detail card */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 1 }}>Machine</Typography>
          <Typography variant="body1" sx={{ fontWeight: 600 }}>
            {session.machineCode}{session.machineName ? ` · ${session.machineName}` : ''}
          </Typography>

          <Divider sx={{ my: 1.5 }} />

          <Typography variant="subtitle2" sx={{ mb: 0.5 }}>Reason</Typography>
          <Typography variant="body1">{session.downtimeReason || '—'}</Typography>
        </CardContent>
      </Card>

      {resolveError && <Alert severity="error" sx={{ mb: 2 }}>{resolveError}</Alert>}

      <Stack spacing={1.5}>
        <Button
          variant="contained"
          color="success"
          fullWidth
          sx={{ minHeight: 56, fontSize: '1rem' }}
          disabled={resolveMutation.isPending}
          onClick={() => resolveMutation.mutate()}
        >
          {resolveMutation.isPending ? <CircularProgress size={22} color="inherit" /> : 'Resolve Downtime'}
        </Button>
      </Stack>
    </Box>
  );
}
