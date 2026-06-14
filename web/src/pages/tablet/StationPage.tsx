import {
  Box,
  Button,
  Card,
  CardActionArea,
  Chip,
  CircularProgress,
  Grid,
  Stack,
  Typography,
} from '@mui/material';
import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGetApiV1Machines } from '../../api/machines/machines';
import { useGetApiV1IotMachinesStates } from '../../api/iot/iot';
import type { MachineDto } from '../../api/model';
import { useTabletSession } from '../../contexts/TabletSessionContext';

type StationStatus = 'IDLE' | 'RUNNING' | 'SETUP' | 'DOWN';

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

function resolveStatus(iotState?: string, machineIsActive?: boolean): StationStatus {
  if (!machineIsActive) return 'DOWN';
  const s = (iotState ?? '').toUpperCase();
  if (s === 'RUNNING') return 'RUNNING';
  if (s === 'SETUP') return 'SETUP';
  if (s === 'DOWN' || s === 'OFFLINE') return 'DOWN';
  return 'IDLE';
}

function numVal(v: number | string): number {
  return typeof v === 'number' ? v : parseInt(v as string, 10) || 0;
}

export default function StationPage() {
  const navigate = useNavigate();
  const { session, update } = useTabletSession();

  const { data: machines = [], isLoading: loadingMachines } = useGetApiV1Machines();
  const { data: iotStates = [] } = useGetApiV1IotMachinesStates({
    query: { refetchInterval: 10_000 },
  });

  const stateByCode = useMemo(() => {
    const m: Record<string, string> = {};
    for (const s of iotStates) m[s.machineCode] = s.currentState;
    return m;
  }, [iotStates]);

  const handleSelect = (machine: MachineDto) => {
    const status = resolveStatus(stateByCode[machine.machineCode], machine.isActive);
    if (status === 'RUNNING' || status === 'DOWN') return;
    update({
      machineCode: machine.machineCode,
      machineName: machine.machineName,
      workCenterId: numVal(machine.workCenterID),
      workOrderId: null,
      woCode: '',
      jobId: null,
      downtimeLogId: null,
      downtimeStartTime: null,
    });
    navigate('/tablet/station/start');
  };

  if (loadingMachines) {
    return (
      <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'background.default' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>
            Select Station
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            {session.operatorId ? `Operator: ${session.operatorId} · ` : ''}Tap your workstation to begin
          </Typography>
        </Box>
        <Button variant="outlined" size="small" onClick={() => navigate('/tablet/login')}>
          Sign Out
        </Button>
      </Stack>

      {machines.length === 0 ? (
        <Typography color="text.secondary" sx={{ textAlign: 'center', mt: 8 }}>
          No machines configured.
        </Typography>
      ) : (
        <Grid container spacing={2}>
          {(machines as MachineDto[]).map((machine) => {
            const status = resolveStatus(stateByCode[machine.machineCode], machine.isActive);
            const selectable = status === 'IDLE' || status === 'SETUP';
            const dotColor = STATUS_COLOR[status];
            const isSelected = session.machineCode === machine.machineCode;
            return (
              <Grid size={{ xs: 12, sm: 6 }} key={machine.machineCode}>
                <Card
                  variant="outlined"
                  sx={{
                    cursor: selectable ? 'pointer' : 'default',
                    border: status === 'RUNNING'
                      ? '2px solid #DC2626'
                      : isSelected
                      ? '2px solid'
                      : '1px solid',
                    borderColor: status === 'RUNNING'
                      ? '#DC2626'
                      : isSelected
                      ? 'primary.main'
                      : 'divider',
                    opacity: selectable ? 1 : 0.65,
                  }}
                >
                  <CardActionArea
                    sx={{ p: 3, minHeight: 140 }}
                    disabled={!selectable}
                    onClick={() => handleSelect(machine)}
                  >
                    <Stack direction="row" sx={{ alignItems: 'center', gap: 2 }}>
                      <Box sx={{ width: 14, height: 14, borderRadius: '50%', bgcolor: dotColor, flexShrink: 0 }} />
                      <Box>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                          {machine.machineName}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {machine.machineCode}{machine.workCenterName ? ` · ${machine.workCenterName}` : ''}
                        </Typography>
                      </Box>
                    </Stack>

                    <Box sx={{ mt: 2, display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                      <Chip
                        label={STATUS_LABEL[status]}
                        size="small"
                        sx={{
                          bgcolor: `${dotColor}20`,
                          color: dotColor,
                          fontWeight: 600,
                          fontSize: '0.72rem',
                        }}
                      />
                    </Box>
                  </CardActionArea>
                </Card>
              </Grid>
            );
          })}
        </Grid>
      )}
    </Box>
  );
}
