import {
  Box,
  Button,
  Chip,
  Divider,
  Drawer,
  IconButton,
  Stack,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetApiV1RemindersAlerts,
  postApiV1RemindersAlertsReadAll,
  postApiV1RemindersAlertsIdRead,
  getGetApiV1RemindersAlertsUnreadCountQueryKey,
  getGetApiV1RemindersAlertsQueryKey,
} from '../api/reminders/reminders';
import SolarIcon from './SolarIcon';
import { useNavigate } from 'react-router-dom';

interface ReminderAlert {
  id: string;
  reminderType: string;
  entityType: string;
  entityId: string;
  entityCode: string;
  message: string;
  isRead: boolean;
  severity: string;
  triggeredAt: string;
}

const SEVERITY_COLORS: Record<string, string> = {
  Error: '#DC2626',
  Warning: '#D97706',
  Info: '#1D4ED8',
};

function severityIcon(severity: string) {
  if (severity === 'Error') return 'error';
  if (severity === 'Warning') return 'warning';
  return 'info';
}

interface Props {
  open: boolean;
  onClose: () => void;
}

export default function NotificationsDrawer({ open, onClose }: Props) {
  const qc = useQueryClient();
  const navigate = useNavigate();
  const { data: rawData, isLoading } = useGetApiV1RemindersAlerts(
    { isRead: false },
    { query: { refetchInterval: 60_000 } }
  );
  const alerts: ReminderAlert[] = (rawData as unknown as ReminderAlert[]) ?? [];

  async function markAllRead() {
    await postApiV1RemindersAlertsReadAll();
    await qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsUnreadCountQueryKey() });
    await qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsQueryKey({}) });
  }

  async function markRead(id: string) {
    await postApiV1RemindersAlertsIdRead(id);
    await qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsUnreadCountQueryKey() });
    await qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsQueryKey({}) });
  }

  function navigateTo(alert: ReminderAlert) {
    markRead(alert.id);
    navigate(`/${alert.entityType}/${alert.entityId}`);
    onClose();
  }

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      slotProps={{ paper: { sx: { width: { xs: '100%', sm: 400 } } } }}
    >
      <Stack sx={{ height: '100%' }}>
        {/* Header */}
        <Stack direction="row" sx={{ p: 2, alignItems: 'center', justifyContent: 'space-between', borderBottom: 1, borderColor: 'divider' }}>
          <Typography variant="h6" sx={{ fontWeight: 700 }}>
            Notifications
          </Typography>
          <Stack direction="row" spacing={0.5}>
            {alerts.length > 0 && (
              <Button size="small" onClick={markAllRead}>
                Mark all read
              </Button>
            )}
            <IconButton size="small" onClick={onClose}>
              <SolarIcon name="close" size={18} />
            </IconButton>
          </Stack>
        </Stack>

        {/* Alert list */}
        <Box sx={{ flex: 1, overflow: 'auto' }}>
          {isLoading ? (
            <Box sx={{ p: 3, textAlign: 'center' }}>
              <Typography variant="body2" color="text.secondary">Loading...</Typography>
            </Box>
          ) : alerts.length === 0 ? (
            <Stack sx={{ alignItems: 'center', justifyContent: 'center', height: 200, gap: 1 }}>
              <SolarIcon name="success" size={40} color="#15803D" />
              <Typography variant="body2" color="text.secondary">No unread notifications</Typography>
            </Stack>
          ) : (
            alerts.map((alert, i) => {
              const color = SEVERITY_COLORS[alert.severity] ?? '#1D4ED8';
              return (
                <Box key={alert.id}>
                  {i > 0 && <Divider />}
                  <Box
                    sx={{
                      p: 2,
                      cursor: 'pointer',
                      '&:hover': { bgcolor: (t) => alpha(t.palette.primary.main, 0.04) },
                    }}
                    onClick={() => navigateTo(alert)}
                  >
                    <Stack direction="row" spacing={1.5} sx={{ alignItems: 'flex-start' }}>
                      <Box sx={{ color, mt: 0.25, flexShrink: 0 }}>
                        <SolarIcon name={severityIcon(alert.severity) as never} size={20} />
                      </Box>
                      <Box sx={{ flex: 1, minWidth: 0 }}>
                        <Stack direction="row" spacing={1} sx={{ alignItems: 'center', mb: 0.5 }}>
                          <Chip
                            label={alert.reminderType.replace(/([A-Z])/g, ' $1').trim()}
                            size="small"
                            sx={{ height: 18, fontSize: 9, bgcolor: alpha(color, 0.12), color }}
                          />
                          <Typography variant="caption" color="text.disabled" sx={{ fontSize: 10, ml: 'auto' }}>
                            {new Date(alert.triggeredAt).toLocaleString()}
                          </Typography>
                        </Stack>
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {alert.message}
                        </Typography>
                        <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'monospace' }}>
                          {alert.entityCode}
                        </Typography>
                      </Box>
                    </Stack>
                  </Box>
                </Box>
              );
            })
          )}
        </Box>
      </Stack>
    </Drawer>
  );
}
