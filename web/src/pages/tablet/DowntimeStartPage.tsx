import {
  Box,
  Button,
  Card,
  CardActionArea,
  Grid,
  IconButton,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import SolarIcon from '../../components/SolarIcon';
import type { IconKey } from '../../lib/icons';

interface DowntimeReason {
  id: number;
  label: string;
  icon: IconKey;
  color: string;
}

const REASONS: DowntimeReason[] = [
  { id: 1, label: 'Machine Breakdown',   icon: 'machineDown',  color: '#DC2626' },
  { id: 2, label: 'Planned Maintenance', icon: 'maintenance',  color: '#1D4ED8' },
  { id: 3, label: 'Material Shortage',   icon: 'quantity',     color: '#D97706' },
  { id: 4, label: 'Setup / Changeover',  icon: 'settings',     color: '#7C3AED' },
  { id: 5, label: 'Quality Hold',        icon: 'quality',      color: '#0D9488' },
  { id: 6, label: 'Operator Break',      icon: 'operator',     color: '#94A3B8' },
  { id: 7, label: 'Waiting for WO',      icon: 'workOrders',   color: '#475569' },
  { id: 8, label: 'Other',               icon: 'info',         color: '#64748B' },
];

export default function DowntimeStartPage() {
  const navigate = useNavigate();
  const [selected, setSelected] = useState<number | null>(null);
  const [notes, setNotes] = useState('');

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ mb: 3, alignItems: 'center', gap: 1 }}>
        <IconButton onClick={() => navigate(-1)} size="small">
          <SolarIcon name="back" size={22} />
        </IconButton>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>
          Log Downtime
        </Typography>
      </Stack>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Select the reason for stopping production:
      </Typography>

      <Grid container spacing={1.5}>
        {REASONS.map((reason) => {
          const isSelected = selected === reason.id;
          return (
            <Grid size={{ xs: 6 }} key={reason.id}>
              <Card
                variant="outlined"
                sx={{
                  cursor: 'pointer',
                  border: isSelected ? `2px solid ${reason.color}` : '1px solid',
                  borderColor: isSelected ? reason.color : 'divider',
                  transition: 'border-color 0.15s',
                }}
              >
                <CardActionArea
                  sx={{
                    p: 2.5,
                    display: 'flex',
                    gap: 1.5,
                    alignItems: 'center',
                    minHeight: 80,
                  }}
                  onClick={() => setSelected(reason.id)}
                >
                  <Box
                    sx={{
                      width: 40,
                      height: 40,
                      borderRadius: 1.5,
                      bgcolor: alpha(reason.color, 0.12),
                      color: reason.color,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      flexShrink: 0,
                    }}
                  >
                    <SolarIcon name={reason.icon} size={22} />
                  </Box>
                  <Typography variant="subtitle2" sx={{ fontWeight: isSelected ? 700 : 500 }}>
                    {reason.label}
                  </Typography>
                </CardActionArea>
              </Card>
            </Grid>
          );
        })}
      </Grid>

      <TextField
        label="Additional notes"
        fullWidth
        multiline
        rows={2}
        value={notes}
        onChange={(e) => setNotes(e.target.value)}
        sx={{ mt: 2 }}
      />

      <Button
        variant="contained"
        fullWidth
        disabled={selected === null}
        sx={{ mt: 2, minHeight: 52 }}
        onClick={() => navigate('/tablet/station/downtime/active')}
      >
        Start Downtime Timer
      </Button>
    </Box>
  );
}
