import {
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import SolarIcon from '../../components/SolarIcon';

interface FieldProps {
  label: string;
  value: string;
}

function Field({ label, value }: FieldProps) {
  return (
    <Box>
      <Typography variant="caption" color="text.disabled" sx={{ display: 'block', mb: 0.25 }}>
        {label.toUpperCase()}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 600 }}>
        {value}
      </Typography>
    </Box>
  );
}

const FINAL_CHECKS = [
  'Output quantities confirmed',
  'Machine cleaned and secured',
  'Material returned to storage',
];

const OK_QTY = 324;
const NG_QTY = 12;
const TOTAL = OK_QTY + NG_QTY;
const TARGET = 500;
const PROGRESS = (OK_QTY / TARGET) * 100;

export default function FinishJobPage() {
  const navigate = useNavigate();
  const [checks, setChecks] = useState([false, false, false]);

  const allChecked = checks.every(Boolean);

  const toggleCheck = (index: number) => {
    setChecks((prev) => prev.map((v, i) => (i === index ? !v : v)));
  };

  const handleComplete = () => {
    setTimeout(() => {
      navigate('/tablet/station');
    }, 800);
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ mb: 3, alignItems: 'center', gap: 1 }}>
        <IconButton onClick={() => navigate(-1)} size="small">
          <SolarIcon name="back" size={22} />
        </IconButton>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Finish Job
        </Typography>
      </Stack>

      {/* Job Summary */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>
            JOB SUMMARY
          </Typography>
          <Typography
            variant="h5"
            sx={{ fontFamily: 'monospace', mt: 0.5, fontWeight: 700 }}
          >
            JOB-2026-0441
          </Typography>

          <Divider sx={{ my: 2 }} />

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Field label="Work Order" value="WO-2026-0094" />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Field label="Product" value="Frame Assembly A · FRM-A001" />
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Field label="Machine" value="MC-01 · CNC Lathe 1" />
            </Grid>
          </Grid>

          <Divider sx={{ my: 2 }} />

          {/* Output Summary */}
          <Grid container spacing={2}>
            <Grid size={{ xs: 4 }}>
              <Typography variant="h4" sx={{ fontWeight: 700, color: '#15803D' }}>
                {OK_QTY}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                OK Qty
              </Typography>
            </Grid>
            <Grid size={{ xs: 4 }}>
              <Typography variant="h4" sx={{ fontWeight: 700, color: '#DC2626' }}>
                {NG_QTY}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                NG Qty
              </Typography>
            </Grid>
            <Grid size={{ xs: 4 }}>
              <Typography variant="h4" sx={{ fontWeight: 700, color: '#1D4ED8' }}>
                {TOTAL}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Total
              </Typography>
            </Grid>
          </Grid>

          <LinearProgress
            variant="determinate"
            value={PROGRESS}
            color="success"
            sx={{ mt: 2, height: 8, borderRadius: 1 }}
          />
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
            {PROGRESS.toFixed(1)}% of target ({TARGET} EA)
          </Typography>
        </CardContent>
      </Card>

      {/* Final Checks */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 1.5 }}>
            Final check before closing
          </Typography>
          <Stack spacing={0.5}>
            {FINAL_CHECKS.map((item, index) => (
              <FormControlLabel
                key={item}
                control={
                  <Checkbox
                    checked={checks[index]}
                    onChange={() => toggleCheck(index)}
                    color="success"
                  />
                }
                label={
                  <Typography variant="body2" sx={{ fontWeight: checks[index] ? 600 : 400 }}>
                    {item}
                  </Typography>
                }
                sx={{ mx: 0 }}
              />
            ))}
          </Stack>
        </CardContent>
      </Card>

      <Stack direction="row" sx={{ gap: 2 }}>
        <Button
          variant="outlined"
          sx={{ flex: 1, minHeight: 52 }}
          onClick={() => navigate(-1)}
        >
          Cancel
        </Button>
        <Button
          variant="contained"
          color="success"
          disabled={!allChecked}
          sx={{ flex: 2, minHeight: 56, fontSize: '1rem' }}
          onClick={handleComplete}
        >
          Complete Job
        </Button>
      </Stack>
    </Box>
  );
}
