import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useNavigate, useParams } from 'react-router-dom';
import {
  PageHeader,
  PageRoot,
  SolarIcon,
  StatusDot,
} from '../../components';
import { machineColors } from '../../theme/tokens';

// ─── Types & mock data ────────────────────────────────────────────────────────

interface WorkCenterInfo {
  id: string;
  code: string;
  name: string;
  type: string;
  capacity: number;
  capacityUnit: string;
  location: string;
  isActive: boolean;
  description: string;
}

interface MockMachine {
  code: string;
  name: string;
  type: string;
  status: keyof typeof machineColors;
  oee: number;
}

const MOCK_WC_MAP: Record<string, WorkCenterInfo> = {
  '1':  { id: '1',  code: 'WC-001', name: 'CNC Machining Bay',    type: 'Machining',  capacity: 8, capacityUnit: 'machines/shift',  location: 'Floor A', isActive: true,  description: 'Primary CNC machining bay for turning and milling operations.' },
  '2':  { id: '2',  code: 'WC-002', name: 'Assembly Station 1',   type: 'Assembly',   capacity: 4, capacityUnit: 'operators/shift', location: 'Floor B', isActive: true,  description: 'Main assembly station for final product assembly.' },
  '3':  { id: '3',  code: 'WC-003', name: 'MIG Welding Bay',      type: 'Welding',    capacity: 6, capacityUnit: 'machines/shift',  location: 'Floor A', isActive: true,  description: 'MIG/MAG welding bay with robotic and manual cells.' },
  '4':  { id: '4',  code: 'WC-004', name: 'Final Inspection',     type: 'Inspection', capacity: 3, capacityUnit: 'operators/shift', location: 'Floor C', isActive: true,  description: 'Final quality inspection before shipment.' },
  '5':  { id: '5',  code: 'WC-005', name: 'Paint Booth Line',     type: 'Painting',   capacity: 2, capacityUnit: 'lines/shift',     location: 'Floor C', isActive: true,  description: 'Automated and manual paint booth for surface finishing.' },
  '6':  { id: '6',  code: 'WC-006', name: 'Sub-assembly Line',    type: 'Assembly',   capacity: 8, capacityUnit: 'operators/shift', location: 'Floor B', isActive: true,  description: 'Sub-assembly line for component-level assembly work.' },
  '7':  { id: '7',  code: 'WC-007', name: 'CNC Milling Center',   type: 'Machining',  capacity: 6, capacityUnit: 'machines/shift',  location: 'Floor A', isActive: true,  description: 'Dedicated CNC milling center for precision parts.' },
  '8':  { id: '8',  code: 'WC-008', name: 'QC Incoming',          type: 'Inspection', capacity: 4, capacityUnit: 'operators/shift', location: 'Floor C', isActive: false, description: 'Incoming quality control inspection station.' },
  '9':  { id: '9',  code: 'WC-009', name: 'Turning Center',       type: 'Machining',  capacity: 4, capacityUnit: 'machines/shift',  location: 'Floor A', isActive: true,  description: 'Turning center for shaft and cylindrical parts.' },
  '10': { id: '10', code: 'WC-010', name: 'Press Shop',           type: 'Machining',  capacity: 3, capacityUnit: 'presses/shift',   location: 'Floor A', isActive: true,  description: 'Press shop for stamping and forming operations.' },
};

const MOCK_MACHINES: MockMachine[] = [
  { code: 'MC-01', name: 'CNC Lathe 1',  type: 'CNC Lathe', status: 'RUNNING', oee: 88 },
  { code: 'MC-02', name: 'CNC Lathe 2',  type: 'CNC Lathe', status: 'IDLE',    oee: 72 },
  { code: 'MC-03', name: 'CNC Mill 1',   type: 'CNC Mill',  status: 'RUNNING', oee: 91 },
  { code: 'MC-04', name: 'Coolant Unit', type: 'Auxiliary', status: 'RUNNING', oee: 94 },
  { code: 'MC-05', name: 'Part Loader',  type: 'Robot',     status: 'IDLE',    oee: 65 },
];

const TYPE_COLORS: Record<string, string> = {
  Machining:  '#1D4ED8',
  Assembly:   '#0D9488',
  Welding:    '#D97706',
  Inspection: '#7C3AED',
  Painting:   '#DC2626',
};

const STATUS_LABEL: Record<keyof typeof machineColors, string> = {
  RUNNING: 'Running',
  IDLE:    'Idle',
  SETUP:   'Setup',
  DOWN:    'Down',
  OFFLINE: 'Offline',
};

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function WorkCenterDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const wc: WorkCenterInfo = MOCK_WC_MAP[id ?? ''] ?? MOCK_WC_MAP['1'];
  const typeColor = TYPE_COLORS[wc.type] ?? '#64748B';

  return (
    <PageRoot>
      <PageHeader
        title={wc.name}
        subtitle={`${wc.code} · ${wc.type}`}
        breadcrumbs={[
          { label: 'Master Data' },
          { label: 'Work Centers', href: '/master/work-centers' },
          { label: wc.code },
        ]}
        actions={
          <Button
            variant="contained"
            size="small"
            startIcon={<SolarIcon name="edit" size={16} />}
            onClick={() => navigate('/master/work-centers')}
          >
            Edit
          </Button>
        }
      />

      <Grid container spacing={2.5}>
        {/* Info card */}
        <Grid size={{ xs: 12, md: 4 }}>
          <Card variant="outlined" sx={{ borderRadius: 2 }}>
            <CardContent>
              <Typography
                variant="subtitle2"
                sx={{ mb: 2, fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', fontSize: '0.6875rem', letterSpacing: 0.5 }}
              >
                Work Center Info
              </Typography>

              <Stack spacing={2}>
                <Box>
                  <Typography variant="caption" color="text.secondary">Code</Typography>
                  <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 13, fontWeight: 600, color: 'primary.main' }}>
                    {wc.code}
                  </Typography>
                </Box>

                <Box>
                  <Typography variant="caption" color="text.secondary">Name</Typography>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>{wc.name}</Typography>
                </Box>

                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>Type</Typography>
                  <Chip
                    label={wc.type}
                    size="small"
                    sx={{
                      height: 22,
                      fontSize: '0.6875rem',
                      fontWeight: 600,
                      bgcolor: alpha(typeColor, 0.1),
                      color: typeColor,
                      border: 'none',
                      '& .MuiChip-label': { px: 0.75 },
                    }}
                  />
                </Box>

                <Box>
                  <Typography variant="caption" color="text.secondary">Location</Typography>
                  <Typography variant="body2">{wc.location}</Typography>
                </Box>

                <Box>
                  <Typography variant="caption" color="text.secondary">Capacity</Typography>
                  <Typography variant="body2">{wc.capacity} {wc.capacityUnit}</Typography>
                </Box>

                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>Status</Typography>
                  <Chip
                    label={wc.isActive ? 'Active' : 'Inactive'}
                    size="small"
                    sx={{
                      height: 20,
                      fontSize: '0.6875rem',
                      fontWeight: 600,
                      bgcolor: wc.isActive ? alpha('#15803D', 0.1) : alpha('#94A3B8', 0.1),
                      color: wc.isActive ? '#15803D' : '#94A3B8',
                      border: 'none',
                      '& .MuiChip-label': { px: 0.75 },
                    }}
                  />
                </Box>

                <Box>
                  <Typography variant="caption" color="text.secondary">Description</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
                    {wc.description}
                  </Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Right column */}
        <Grid size={{ xs: 12, md: 8 }}>
          <Stack spacing={2.5}>
            {/* Machines table */}
            <Card variant="outlined" sx={{ borderRadius: 2 }}>
              <CardContent sx={{ pb: '12px !important' }}>
                <Typography
                  variant="subtitle2"
                  sx={{ mb: 2, fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', fontSize: '0.6875rem', letterSpacing: 0.5 }}
                >
                  Machines
                </Typography>
                <Table size="small">
                  <TableHead>
                    <TableRow
                      sx={{ '& th': { py: 0.75, fontWeight: 600, fontSize: '0.75rem', borderColor: 'divider', color: 'text.secondary' } }}
                    >
                      <TableCell>Machine Code</TableCell>
                      <TableCell>Name</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell align="right">OEE %</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {MOCK_MACHINES.map((m) => (
                      <TableRow
                        key={m.code}
                        sx={{ '& td': { py: 0.75, fontSize: '0.8125rem', borderColor: 'divider' }, '&:last-child td': { border: 0 } }}
                      >
                        <TableCell>
                          <Typography variant="body2" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: 12, fontWeight: 600, color: 'primary.main' }}>
                            {m.code}
                          </Typography>
                        </TableCell>
                        <TableCell>{m.name}</TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary" sx={{ fontSize: 12 }}>{m.type}</Typography>
                        </TableCell>
                        <TableCell>
                          <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                            <StatusDot color={machineColors[m.status]} size={7} pulse={m.status === 'RUNNING'} />
                            <Typography variant="body2" sx={{ fontSize: 12, color: machineColors[m.status], fontWeight: 500 }}>
                              {STATUS_LABEL[m.status]}
                            </Typography>
                          </Stack>
                        </TableCell>
                        <TableCell align="right">
                          <Typography
                            variant="body2"
                            sx={{
                              fontSize: 12,
                              fontWeight: 600,
                              color: m.oee >= 85 ? '#15803D' : m.oee >= 65 ? '#3A9188' : m.oee >= 45 ? '#B45309' : '#B91C1C',
                            }}
                          >
                            {m.oee > 0 ? `${m.oee}%` : '—'}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            {/* Performance KPIs */}
            <Card variant="outlined" sx={{ borderRadius: 2 }}>
              <CardContent>
                <Typography
                  variant="subtitle2"
                  sx={{ mb: 2, fontWeight: 700, color: 'text.secondary', textTransform: 'uppercase', fontSize: '0.6875rem', letterSpacing: 0.5 }}
                >
                  Recent Performance
                </Typography>
                <Stack direction="row" spacing={1.5} sx={{ flexWrap: 'wrap' }}>
                  {[
                    { label: 'Utilization',       value: '76%',   color: '#1D4ED8' },
                    { label: 'Avg OEE',           value: '81.2%', color: '#15803D' },
                    { label: 'Downtime this week', value: '4.2 h', color: '#D97706' },
                  ].map((kpi) => (
                    <Box
                      key={kpi.label}
                      sx={{
                        px: 2,
                        py: 1.25,
                        borderRadius: 1.5,
                        border: '1px solid',
                        borderColor: 'divider',
                        bgcolor: alpha(kpi.color, 0.04),
                        minWidth: 140,
                      }}
                    >
                      <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.25 }}>
                        {kpi.label}
                      </Typography>
                      <Typography variant="h6" sx={{ fontWeight: 700, color: kpi.color, lineHeight: 1.2 }}>
                        {kpi.value}
                      </Typography>
                    </Box>
                  ))}
                </Stack>
              </CardContent>
            </Card>
          </Stack>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
