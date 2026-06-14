import {
  Alert,
  Box,
  Button,
  Card,
  CardActionArea,
  CircularProgress,
  Grid,
  IconButton,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import SolarIcon from '../../components/SolarIcon';
import { useGetApiV1DowntimeReasonCodes } from '../../api/downtime-reason-codes/downtime-reason-codes';
import { postApiV1DowntimeStart } from '../../api/downtime/downtime';
import type { DowntimeReasonCodeDto } from '../../api/model';
import { useTabletSession } from '../../contexts/TabletSessionContext';
import { getErrorMessage } from '../../lib/apiClient';
import type { IconKey } from '../../lib/icons';

const CATEGORY_ICON: Record<string, IconKey> = {
  Breakdown:   'machineDown',
  Maintenance: 'maintenance',
  Material:    'quantity',
  Setup:       'settings',
  Quality:     'quality',
  Break:       'operator',
  Other:       'info',
};

function categoryIcon(category: string): IconKey {
  for (const [key, icon] of Object.entries(CATEGORY_ICON)) {
    if (category.toLowerCase().includes(key.toLowerCase())) return icon;
  }
  return 'info';
}

const CATEGORY_COLOR: Record<string, string> = {
  Breakdown:   '#DC2626',
  Maintenance: '#1D4ED8',
  Material:    '#D97706',
  Setup:       '#7C3AED',
  Quality:     '#0D9488',
  Break:       '#94A3B8',
  Other:       '#64748B',
};

function categoryColor(category: string): string {
  for (const [key, color] of Object.entries(CATEGORY_COLOR)) {
    if (category.toLowerCase().includes(key.toLowerCase())) return color;
  }
  return '#64748B';
}

export default function DowntimeStartPage() {
  const navigate = useNavigate();
  const { session, update } = useTabletSession();
  const [selected, setSelected] = useState<DowntimeReasonCodeDto | null>(null);
  const [notes, setNotes] = useState('');
  const [submitError, setSubmitError] = useState('');

  const { data: reasons = [], isLoading } = useGetApiV1DowntimeReasonCodes();
  const activeReasons = (reasons as DowntimeReasonCodeDto[]).filter((r) => r.isActive);

  const startMutation = useMutation({
    mutationFn: () =>
      postApiV1DowntimeStart({
        machineCode: session.machineCode,
        reasonCode: selected!.reasonCode,
        reasonName: selected!.reasonName,
        startTime: new Date().toISOString(),
        operatorId: session.operatorId || 'OPERATOR',
        notes: notes || null,
      }),
    onSuccess: (resp) => {
      const result = (resp as { data?: { downtimeLogId?: number | string } })?.data;
      const downtimeLogId = result?.downtimeLogId ? (typeof result.downtimeLogId === 'number' ? result.downtimeLogId : parseInt(result.downtimeLogId as string, 10)) : null;
      update({
        downtimeLogId,
        downtimeReason: selected!.reasonName,
        downtimeStartTime: new Date().toISOString(),
      });
      navigate('/tablet/station/downtime/active');
    },
    onError: (err) => setSubmitError(getErrorMessage(err)),
  });

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', p: 3 }}>
      <Stack direction="row" sx={{ mb: 3, alignItems: 'center', gap: 1 }}>
        <IconButton onClick={() => navigate(-1)} size="small">
          <SolarIcon name="back" size={22} />
        </IconButton>
        <Typography variant="h6" sx={{ fontWeight: 600 }}>Log Downtime</Typography>
      </Stack>

      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Select the reason for stopping production:
      </Typography>

      {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Grid container spacing={1.5}>
          {activeReasons.map((reason) => {
            const isSelected = selected?.reasonCode === reason.reasonCode;
            const color = categoryColor(reason.category as string);
            const icon = categoryIcon(reason.category as string);
            return (
              <Grid size={{ xs: 6 }} key={reason.reasonCode}>
                <Card
                  variant="outlined"
                  sx={{
                    cursor: 'pointer',
                    border: isSelected ? `2px solid ${color}` : '1px solid',
                    borderColor: isSelected ? color : 'divider',
                    transition: 'border-color 0.15s',
                  }}
                >
                  <CardActionArea
                    sx={{ p: 2.5, display: 'flex', gap: 1.5, alignItems: 'center', minHeight: 80 }}
                    onClick={() => setSelected(reason)}
                  >
                    <Box
                      sx={{
                        width: 40, height: 40, borderRadius: 1.5,
                        bgcolor: alpha(color, 0.12), color,
                        display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0,
                      }}
                    >
                      <SolarIcon name={icon} size={22} />
                    </Box>
                    <Typography variant="subtitle2" sx={{ fontWeight: isSelected ? 700 : 500 }}>
                      {reason.reasonName}
                    </Typography>
                  </CardActionArea>
                </Card>
              </Grid>
            );
          })}
        </Grid>
      )}

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
        disabled={selected === null || startMutation.isPending}
        sx={{ mt: 2, minHeight: 52 }}
        onClick={() => startMutation.mutate()}
      >
        {startMutation.isPending ? <CircularProgress size={20} color="inherit" /> : 'Start Downtime Timer'}
      </Button>
    </Box>
  );
}
