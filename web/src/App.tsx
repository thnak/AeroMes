import {
  AppBar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Container,
  Grid,
  IconButton,
  LinearProgress,
  Stack,
  Toolbar,
  Typography,
  useColorScheme,
} from '@mui/material';
import {
  DarkMode,
  Factory,
  LightMode,
  Pause,
  PlayArrow,
  Warning,
} from '@mui/icons-material';

function ColorSchemeToggle() {
  const { mode, setMode } = useColorScheme();
  return (
    <IconButton
      color="inherit"
      onClick={() => setMode(mode === 'dark' ? 'light' : 'dark')}
    >
      {mode === 'dark' ? <LightMode /> : <DarkMode />}
    </IconButton>
  );
}

const statCards = [
  { label: 'Active Work Orders', value: '12', color: 'primary.main' },
  { label: 'OEE Today', value: '82.4%', color: 'success.main' },
  { label: 'Total Output (OK)', value: '4,812', color: 'info.main' },
  { label: 'Defect Rate', value: '1.3%', color: 'warning.main' },
];

const workOrders = [
  { no: 'WO-2026-0089', product: 'Frame Assembly A', status: 'RUNNING', progress: 78 },
  { no: 'WO-2026-0090', product: 'Panel Sub-assembly B', status: 'RELEASED', progress: 0 },
  { no: 'WO-2026-0088', product: 'Shaft Housing C', status: 'PAUSED', progress: 45 },
];

const statusColor: Record<string, 'success' | 'default' | 'warning'> = {
  RUNNING: 'success',
  RELEASED: 'default',
  PAUSED: 'warning',
};

export default function App() {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppBar position="static" color="primary" elevation={0}>
        <Toolbar>
          <Factory sx={{ mr: 1.5 }} />
          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            AeroMes
          </Typography>
          <ColorSchemeToggle />
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Typography variant="h4" gutterBottom>
          Production Overview
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 4 }}>
          Real-time shop floor dashboard — June 8, 2026
        </Typography>

        {/* Stat cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          {statCards.map((s) => (
            <Grid key={s.label} size={{ xs: 12, sm: 6, md: 3 }}>
              <Card>
                <CardContent>
                  <Typography variant="caption" color="text.secondary">
                    {s.label}
                  </Typography>
                  <Typography variant="h4" sx={{ color: s.color, mt: 0.5, fontWeight: 700 }}>
                    {s.value}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        {/* Work orders list */}
        <Typography variant="h5" gutterBottom>
          Work Orders
        </Typography>
        <Stack spacing={2}>
          {workOrders.map((wo) => (
            <Card key={wo.no}>
              <CardContent>
                <Stack
                  direction="row"
                  sx={{ alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}
                >
                  <Box>
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                      {wo.no}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {wo.product}
                    </Typography>
                  </Box>
                  <Stack direction="row" sx={{ alignItems: 'center' }} spacing={1.5}>
                    <Chip
                      label={wo.status}
                      color={statusColor[wo.status]}
                      size="small"
                    />
                    {wo.status === 'RUNNING' && (
                      <Button
                        variant="outlined"
                        color="warning"
                        size="small"
                        startIcon={<Pause />}
                      >
                        Pause
                      </Button>
                    )}
                    {wo.status === 'RELEASED' && (
                      <Button
                        variant="contained"
                        color="primary"
                        size="small"
                        startIcon={<PlayArrow />}
                      >
                        Start
                      </Button>
                    )}
                    {wo.status === 'PAUSED' && (
                      <Button
                        variant="outlined"
                        color="primary"
                        size="small"
                        startIcon={<Warning />}
                      >
                        Resume
                      </Button>
                    )}
                  </Stack>
                </Stack>
                {wo.progress > 0 && (
                  <Box sx={{ mt: 2 }}>
                    <Stack direction="row" sx={{ justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="caption" color="text.secondary">
                        Progress
                      </Typography>
                      <Typography variant="caption" sx={{ fontWeight: 600 }}>
                        {wo.progress}%
                      </Typography>
                    </Stack>
                    <LinearProgress
                      variant="determinate"
                      value={wo.progress}
                      color={wo.status === 'PAUSED' ? 'warning' : 'primary'}
                      sx={{ borderRadius: 1, height: 6 }}
                    />
                  </Box>
                )}
              </CardContent>
            </Card>
          ))}
        </Stack>
      </Container>
    </Box>
  );
}
