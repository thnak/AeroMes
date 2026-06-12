import {
  Box,
  Button,
  Card,
  CardActionArea,
  Chip,
  Grid,
  Stack,
  Typography,
} from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

type StationStatus = 'IDLE' | 'RUNNING' | 'SETUP' | 'DOWN';

interface Station {
  id: string;
  name: string;
  code: string;
  status: StationStatus;
  woInfo?: string;
}

const STATIONS: Station[] = [
  { id: 'MC-01', name: 'CNC Lathe 1',   code: 'MC-01', status: 'IDLE' },
  { id: 'MC-02', name: 'CNC Lathe 2',   code: 'MC-02', status: 'SETUP' },
  { id: 'MC-03', name: 'CNC Mill 1',    code: 'MC-03', status: 'RUNNING', woInfo: 'WO-2026-0089 · 78% complete' },
  { id: 'MC-04', name: 'CNC Mill 2',    code: 'MC-04', status: 'IDLE' },
  { id: 'MC-05', name: 'Drill Press 1', code: 'MC-05', status: 'IDLE' },
  { id: 'MC-06', name: 'Grinder 1',     code: 'MC-06', status: 'RUNNING', woInfo: 'WO-2026-0091 · 42% complete' },
];

const STATUS_COLOR: Record<StationStatus, string> = {
  IDLE:    '#15803D',
  SETUP:   '#D97706',
  RUNNING: '#DC2626',
  DOWN:    '#6B7280',
};

const STATUS_LABEL: Record<StationStatus, string> = {
  IDLE:    'Available',
  SETUP:   'Setup',
  RUNNING: 'Occupied',
  DOWN:    'Down',
};

export default function StationPage() {
  const navigate = useNavigate();
  const [selected, setSelected] = useState<string | null>(null);

  const canSelect = (status: StationStatus) => status === 'IDLE' || status === 'SETUP';

  const handleSelect = (station: Station) => {
    if (!canSelect(station.status)) return;
    setSelected(station.id);
    navigate('/tablet/station/start');
  };

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>
            Select Station
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            Tap your workstation to begin
          </Typography>
        </Box>
        <Button
          variant="outlined"
          size="small"
          onClick={() => navigate('/tablet/login')}
        >
          Sign Out
        </Button>
      </Stack>

      <Grid container spacing={2}>
        {STATIONS.map((station) => {
          const selectable = canSelect(station.status);
          const dotColor = STATUS_COLOR[station.status];
          return (
            <Grid size={{ xs: 12, sm: 6 }} key={station.id}>
              <Card
                variant="outlined"
                sx={{
                  cursor: selectable ? 'pointer' : 'default',
                  border: station.status === 'RUNNING'
                    ? '2px solid #DC2626'
                    : selected === station.id
                    ? '2px solid'
                    : '1px solid',
                  borderColor: station.status === 'RUNNING'
                    ? '#DC2626'
                    : selected === station.id
                    ? 'primary.main'
                    : 'divider',
                  opacity: selectable ? 1 : 0.65,
                }}
              >
                <CardActionArea
                  sx={{ p: 3, minHeight: 140 }}
                  disabled={!selectable}
                  onClick={() => handleSelect(station)}
                >
                  <Stack direction="row" sx={{ alignItems: 'center', gap: 2 }}>
                    <Box
                      sx={{
                        width: 14,
                        height: 14,
                        borderRadius: '50%',
                        bgcolor: dotColor,
                        flexShrink: 0,
                      }}
                    />
                    <Box>
                      <Typography variant="h6" sx={{ fontWeight: 600 }}>
                        {station.name}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {station.code}
                      </Typography>
                    </Box>
                  </Stack>

                  <Box sx={{ mt: 2, display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                    <Chip
                      label={STATUS_LABEL[station.status]}
                      size="small"
                      sx={{
                        bgcolor: `${dotColor}20`,
                        color: dotColor,
                        fontWeight: 600,
                        fontSize: '0.72rem',
                      }}
                    />
                    {station.woInfo && (
                      <Typography variant="caption" color="text.secondary">
                        {station.woInfo}
                      </Typography>
                    )}
                  </Box>
                </CardActionArea>
              </Card>
            </Grid>
          );
        })}
      </Grid>
    </Box>
  );
}
