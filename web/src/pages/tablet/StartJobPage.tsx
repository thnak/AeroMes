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

const CHECKLIST = [
  'Safety PPE equipped',
  'Machine homed and calibrated',
  'First article inspection done',
];

export default function StartJobPage() {
  const navigate = useNavigate();
  const [checks, setChecks] = useState([false, false, false]);

  const allChecked = checks.every(Boolean);

  const toggleCheck = (index: number) => {
    setChecks((prev) => prev.map((v, i) => (i === index ? !v : v)));
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ mb: 3, alignItems: 'center', gap: 1 }}>
        <IconButton onClick={() => navigate(-1)} size="small">
          <SolarIcon name="back" size={22} />
        </IconButton>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Start Job
        </Typography>
      </Stack>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>
            ASSIGNED JOB
          </Typography>
          <Typography
            variant="h4"
            sx={{ fontFamily: 'monospace', mt: 0.5, fontWeight: 700 }}
          >
            JOB-2026-0441
          </Typography>

          <Divider sx={{ my: 2 }} />

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Field label="Work Order" value="WO-2026-0094" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Field label="Product" value="Frame Assembly A · FRM-A001" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Field label="Target Qty" value="500 EA" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Field label="Machine" value="MC-01 · CNC Lathe 1" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Field label="Operation" value="OP-001 · CNC Turning" />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Field label="Planned Start" value="Today 08:00" />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 1 }}>
            Pre-Start Checklist
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
            Complete before starting:
          </Typography>
          <Stack spacing={0.5}>
            {CHECKLIST.map((item, index) => (
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

      <Button
        variant="contained"
        fullWidth
        size="large"
        color="success"
        disabled={!allChecked}
        sx={{ minHeight: 56, fontSize: '1rem' }}
        onClick={() => navigate('/tablet/station/output')}
      >
        Start Job
      </Button>
    </Box>
  );
}
