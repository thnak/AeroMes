import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Stack,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useNavigate } from 'react-router-dom';

export default function DowntimeActivePage() {
  const navigate = useNavigate();

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
              <Typography
                variant="caption"
                sx={{ color: '#DC2626', fontWeight: 700, display: 'block' }}
              >
                MACHINE DOWN
              </Typography>
              <Typography variant="subtitle2">Machine Breakdown</Typography>
            </Box>
            <Chip
              label="● ACTIVE"
              sx={{
                bgcolor: alpha('#DC2626', 0.12),
                color: '#DC2626',
                fontWeight: 700,
                fontSize: '0.75rem',
              }}
            />
          </Stack>

          <Typography
            sx={{
              fontSize: '3rem',
              fontWeight: 700,
              fontFamily: 'monospace',
              color: '#DC2626',
              mt: 2,
              textAlign: 'center',
            }}
          >
            00:42:17
          </Typography>

          <Typography
            variant="caption"
            sx={{ color: 'text.secondary', textAlign: 'center', display: 'block' }}
          >
            Started 10:23:45
          </Typography>
        </CardContent>
      </Card>

      {/* Detail card */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 1 }}>
            Machine
          </Typography>
          <Typography variant="body1" sx={{ fontWeight: 600 }}>
            MC-04 · CNC Mill 2
          </Typography>

          <Divider sx={{ my: 1.5 }} />

          <Typography variant="subtitle2" sx={{ mb: 0.5 }}>
            Reason
          </Typography>
          <Typography variant="body1">Machine Breakdown</Typography>
          <Typography variant="caption" color="text.secondary">
            No additional notes
          </Typography>
        </CardContent>
      </Card>

      {/* Actions */}
      <Stack spacing={1.5}>
        <Button variant="outlined" fullWidth sx={{ minHeight: 52 }}>
          Update Notes
        </Button>
        <Button
          variant="contained"
          color="success"
          fullWidth
          sx={{ minHeight: 56, fontSize: '1rem' }}
          onClick={() => navigate('/tablet/station/output')}
        >
          Resolve Downtime
        </Button>
      </Stack>
    </Box>
  );
}
