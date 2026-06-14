import {
  Alert,
  Box,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Checkbox,
  CircularProgress,
  Divider,
  FormControlLabel,
  Grid,
  IconButton,
  Stack,
  Typography,
} from '@mui/material';
import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import SolarIcon from '../../components/SolarIcon';
import { useGetApiV1WorkOrders } from '../../api/work-orders/work-orders';
import { useGetApiV1WorkShifts } from '../../api/work-shifts/work-shifts';
import { postApiV1Jobs } from '../../api/jobs/jobs';
import type { WorkOrderDto } from '../../api/model';
import { useTabletSession } from '../../contexts/TabletSessionContext';
import { getErrorMessage } from '../../lib/apiClient';

const CHECKLIST = [
  'Safety PPE equipped',
  'Machine homed and calibrated',
  'First article inspection done',
];

function numVal(v: number | string): number {
  return typeof v === 'number' ? v : parseInt(v as string, 10) || 0;
}

function findCurrentShift(shifts: { shiftCode: string; startTime: string; endTime: string; isNightShift: boolean; isActive: boolean }[]): string {
  const now = new Date();
  const hhmm = now.getHours() * 60 + now.getMinutes();
  for (const shift of shifts) {
    if (!shift.isActive) continue;
    const sp = shift.startTime.split(':').map(Number);
    const ep = shift.endTime.split(':').map(Number);
    const start = sp[0] * 60 + (sp[1] || 0);
    const end = ep[0] * 60 + (ep[1] || 0);
    if (shift.isNightShift) {
      if (hhmm >= start || hhmm < end) return shift.shiftCode;
    } else {
      if (hhmm >= start && hhmm < end) return shift.shiftCode;
    }
  }
  return shifts[0]?.shiftCode ?? 'DEFAULT';
}

export default function StartJobPage() {
  const navigate = useNavigate();
  const { session, update } = useTabletSession();
  const [selectedWoId, setSelectedWoId] = useState<number | null>(null);
  const [checks, setChecks] = useState([false, false, false]);
  const [submitError, setSubmitError] = useState('');

  const { data: woResp, isLoading: loadingWOs } = useGetApiV1WorkOrders({ status: 'PENDING' });
  const allWOs: WorkOrderDto[] = ((woResp as { data?: WorkOrderDto[] })?.data ?? []) as WorkOrderDto[];

  const pendingWOs = useMemo(() => {
    if (!session.workCenterId) return allWOs;
    return allWOs.filter((wo) => numVal(wo.workCenterID) === session.workCenterId);
  }, [allWOs, session.workCenterId]);

  const { data: shifts = [] } = useGetApiV1WorkShifts();

  const selectedWO = pendingWOs.find((wo) => numVal(wo.woid) === selectedWoId) ?? null;
  const allChecked = checks.every(Boolean);
  const canStart = selectedWO !== null && allChecked;

  const toggleCheck = (index: number) => setChecks((prev) => prev.map((v, i) => (i === index ? !v : v)));

  const startMutation = useMutation({
    mutationFn: (wo: WorkOrderDto) => {
      const shiftCode = findCurrentShift(shifts as unknown as { shiftCode: string; startTime: string; endTime: string; isNightShift: boolean; isActive: boolean }[]);
      return postApiV1Jobs({
        workOrderId: wo.woid,
        machineCode: session.machineCode,
        shiftCode,
        operatorId: session.operatorId || 'OPERATOR',
        startTime: null,
      });
    },
    onSuccess: (resp, wo) => {
      const result = (resp as { data?: { jobID?: number | string } })?.data;
      const jobId = result?.jobID ? numVal(result.jobID) : null;
      update({
        workOrderId: numVal(wo.woid),
        woCode: wo.woCode,
        targetQty: numVal(wo.targetQty),
        jobId,
      });
      navigate('/tablet/station/output');
    },
    onError: (err) => setSubmitError(getErrorMessage(err)),
  });

  if (!session.machineCode) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
        <Typography color="text.secondary" sx={{ mb: 2 }}>No station selected.</Typography>
        <Button variant="outlined" onClick={() => navigate('/tablet/station')}>Back to Stations</Button>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ mb: 3, alignItems: 'center', gap: 1 }}>
        <IconButton onClick={() => navigate(-1)} size="small">
          <SolarIcon name="back" size={22} />
        </IconButton>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>Start Job</Typography>
          <Typography variant="caption" color="text.secondary">{session.machineName} · {session.machineCode}</Typography>
        </Box>
      </Stack>

      {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}

      {/* Work Order Selection */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 1.5 }}>
            Select Work Order
          </Typography>
          {loadingWOs ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
              <CircularProgress size={28} />
            </Box>
          ) : pendingWOs.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
              No pending work orders for this work center.
            </Typography>
          ) : (
            <Stack spacing={1}>
              {pendingWOs.map((wo) => {
                const id = numVal(wo.woid);
                const isSelected = selectedWoId === id;
                return (
                  <Card
                    key={id}
                    variant="outlined"
                    sx={{
                      border: isSelected ? '2px solid' : '1px solid',
                      borderColor: isSelected ? 'primary.main' : 'divider',
                    }}
                  >
                    <CardActionArea sx={{ p: 2 }} onClick={() => setSelectedWoId(id)}>
                      <Grid container spacing={1.5}>
                        <Grid size={{ xs: 12, sm: 4 }}>
                          <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>WORK ORDER</Typography>
                          <Typography variant="body2" sx={{ fontWeight: 700, fontFamily: 'monospace' }}>{wo.woCode}</Typography>
                        </Grid>
                        <Grid size={{ xs: 6, sm: 4 }}>
                          <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>WORK CENTER</Typography>
                          <Typography variant="body2">{wo.workCenterName ?? '—'}</Typography>
                        </Grid>
                        <Grid size={{ xs: 6, sm: 4 }}>
                          <Typography variant="caption" color="text.disabled" sx={{ display: 'block' }}>TARGET QTY</Typography>
                          <Typography variant="body2" sx={{ fontWeight: 600 }}>{numVal(wo.targetQty)} EA</Typography>
                        </Grid>
                      </Grid>
                    </CardActionArea>
                  </Card>
                );
              })}
            </Stack>
          )}
        </CardContent>
      </Card>

      {/* Pre-start checklist */}
      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="subtitle2" sx={{ mb: 1 }}>Pre-Start Checklist</Typography>
          <Divider sx={{ mb: 1.5 }} />
          <Stack spacing={0.5}>
            {CHECKLIST.map((item, index) => (
              <FormControlLabel
                key={item}
                control={<Checkbox checked={checks[index]} onChange={() => toggleCheck(index)} color="success" />}
                label={<Typography variant="body2" sx={{ fontWeight: checks[index] ? 600 : 400 }}>{item}</Typography>}
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
        disabled={!canStart || startMutation.isPending}
        sx={{ minHeight: 56, fontSize: '1rem' }}
        onClick={() => selectedWO && startMutation.mutate(selectedWO)}
      >
        {startMutation.isPending ? <CircularProgress size={22} color="inherit" /> : 'Start Job'}
      </Button>
    </Box>
  );
}
