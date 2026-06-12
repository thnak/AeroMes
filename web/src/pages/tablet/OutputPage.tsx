import {
  Box,
  Button,
  Card,
  CardContent,
  Grid,
  IconButton,
  LinearProgress,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import SolarIcon from '../../components/SolarIcon';

const TARGET = 500;
const LOGGED = 287;

export default function OutputPage() {
  const navigate = useNavigate();
  const [okQty, setOkQty] = useState(0);
  const [ngQty, setNgQty] = useState(0);
  const [note, setNote] = useState('');

  const progress = (LOGGED / TARGET) * 100;

  const adjustOk = (delta: number) => setOkQty((v) => Math.max(0, v + delta));
  const adjustNg = (delta: number) => setNgQty((v) => Math.max(0, v + delta));

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ mb: 3, alignItems: 'center', gap: 1 }}>
        <IconButton onClick={() => navigate(-1)} size="small">
          <SolarIcon name="back" size={22} />
        </IconButton>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Log Output
        </Typography>
      </Stack>

      {/* Job Status */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1.5 }}>
            JOB-2026-0441 · WO-2026-0094 · RUNNING
          </Typography>
          <Grid container spacing={2}>
            <Grid size={{ xs: 4 }}>
              <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>
                TARGET
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700 }}>
                {TARGET}
              </Typography>
            </Grid>
            <Grid size={{ xs: 4 }}>
              <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>
                LOGGED
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#15803D' }}>
                {LOGGED}
              </Typography>
            </Grid>
            <Grid size={{ xs: 4 }}>
              <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>
                REMAINING
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700 }} color="text.secondary">
                {TARGET - LOGGED}
              </Typography>
            </Grid>
          </Grid>
          <LinearProgress
            variant="determinate"
            value={progress}
            sx={{ mt: 1.5, height: 8, borderRadius: 1 }}
          />
        </CardContent>
      </Card>

      {/* Log Output */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid size={{ xs: 6 }}>
              <Typography variant="subtitle2" sx={{ mb: 1, color: '#15803D' }}>
                OK Qty
              </Typography>
              <TextField
                type="number"
                fullWidth
                value={okQty}
                onChange={(e) => setOkQty(Math.max(0, parseInt(e.target.value) || 0))}
                sx={{
                  '& input': {
                    fontSize: '2rem',
                    textAlign: 'center',
                    py: 2,
                  },
                }}
              />
              <Stack direction="row" sx={{ gap: 1, mt: 1 }}>
                <Button
                  variant="outlined"
                  sx={{ flex: 1, minHeight: 48 }}
                  onClick={() => adjustOk(-1)}
                >
                  -1
                </Button>
                <Button
                  variant="outlined"
                  sx={{ flex: 1, minHeight: 48 }}
                  onClick={() => adjustOk(1)}
                >
                  +1
                </Button>
              </Stack>
            </Grid>
            <Grid size={{ xs: 6 }}>
              <Typography variant="subtitle2" sx={{ mb: 1, color: '#DC2626' }}>
                NG Qty
              </Typography>
              <TextField
                type="number"
                fullWidth
                value={ngQty}
                onChange={(e) => setNgQty(Math.max(0, parseInt(e.target.value) || 0))}
                sx={{
                  '& input': {
                    fontSize: '2rem',
                    textAlign: 'center',
                    py: 2,
                  },
                }}
              />
              <Stack direction="row" sx={{ gap: 1, mt: 1 }}>
                <Button
                  variant="outlined"
                  color="error"
                  sx={{ flex: 1, minHeight: 48 }}
                  onClick={() => adjustNg(-1)}
                >
                  -1
                </Button>
                <Button
                  variant="outlined"
                  color="error"
                  sx={{ flex: 1, minHeight: 48 }}
                  onClick={() => adjustNg(1)}
                >
                  +1
                </Button>
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
        <Button
          variant="outlined"
          color="warning"
          sx={{ flex: 1, minHeight: 52 }}
          onClick={() => navigate('/tablet/station/downtime/start')}
        >
          Log Downtime
        </Button>
        <Button
          variant="contained"
          color="success"
          sx={{ flex: 2, minHeight: 52 }}
        >
          Submit Output
        </Button>
        <Button
          variant="outlined"
          sx={{ flex: 1, minHeight: 52 }}
          onClick={() => navigate('/tablet/station/finish')}
        >
          Finish Job
        </Button>
      </Stack>
    </Box>
  );
}
