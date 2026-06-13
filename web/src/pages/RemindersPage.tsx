import { useState } from 'react';
import {
  Badge,
  Box,
  Button,
  Chip,
  Divider,
  FormControlLabel,
  List,
  ListItem,
  ListItemText,
  Paper,
  Stack,
  Switch,
  Tab,
  Tabs,
  TextField,
  Typography,
} from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import {
  getGetApiV1RemindersAlertsQueryKey,
  getGetApiV1RemindersAlertsUnreadCountQueryKey,
  getGetApiV1RemindersConfigurationsQueryKey,
  postApiV1RemindersAlertsIdRead,
  postApiV1RemindersAlertsReadAll,
  putApiV1RemindersConfigurationsReminderType,
  useGetApiV1RemindersAlerts,
  useGetApiV1RemindersConfigurations,
} from '../api/reminders/reminders';
import type { ReminderAlertDto, ReminderConfigDto } from '../api/model';
import { EmptyState, PageHeader, SolarIcon, TablePageSkeleton } from '../components';

const SEVERITY_COLORS: Record<string, 'error' | 'warning' | 'info' | 'success'> = {
  Error: 'error',
  Warning: 'warning',
  Info: 'info',
  Success: 'success',
};

export default function RemindersPage() {
  const [tab, setTab] = useState(0);

  return (
    <Box sx={{ p: 3 }}>
      <PageHeader
        title="Smart Reminders"
        breadcrumbs={[{ label: 'Reminders' }]}
      />
      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
        <Tab label="Alerts" />
        <Tab label="Settings" />
      </Tabs>
      {tab === 0 && <AlertsTab />}
      {tab === 1 && <ConfigurationsTab />}
    </Box>
  );
}

function AlertsTab() {
  const [isReadFilter, setIsReadFilter] = useState<boolean | undefined>(undefined);
  const qc = useQueryClient();

  const { data: alerts, isLoading, error } = useGetApiV1RemindersAlerts(
    { isRead: isReadFilter, limit: 100 }
  );

  const alertList = alerts as ReminderAlertDto[] | undefined;

  async function markRead(id: string) {
    await postApiV1RemindersAlertsIdRead(id);
    qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsQueryKey() });
    qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsUnreadCountQueryKey() });
  }

  async function markAllRead() {
    await postApiV1RemindersAlertsReadAll();
    qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsQueryKey() });
    qc.invalidateQueries({ queryKey: getGetApiV1RemindersAlertsUnreadCountQueryKey() });
  }

  if (isLoading) return <TablePageSkeleton />;
  if (error) return <EmptyState title="Failed to load alerts" />;

  return (
    <Box>
      <Stack direction="row" spacing={2} sx={{ mb: 2, alignItems: 'center' }}>
        <Button
          size="small"
          variant={isReadFilter === undefined ? 'contained' : 'outlined'}
          onClick={() => setIsReadFilter(undefined)}
        >
          All
        </Button>
        <Button
          size="small"
          variant={isReadFilter === false ? 'contained' : 'outlined'}
          onClick={() => setIsReadFilter(false)}
        >
          Unread
        </Button>
        <Button
          size="small"
          variant={isReadFilter === true ? 'contained' : 'outlined'}
          onClick={() => setIsReadFilter(true)}
        >
          Read
        </Button>
        <Box sx={{ flexGrow: 1 }} />
        <Button
          size="small"
          startIcon={<SolarIcon name="complete" size={16} />}
          onClick={markAllRead}
        >
          Mark all read
        </Button>
      </Stack>

      {!alertList?.length ? (
        <EmptyState title="No alerts" description="No reminders to show." />
      ) : (
        <Paper variant="outlined">
          <List disablePadding>
            {alertList.map((alert, i) => (
              <Box key={String(alert.id)}>
                {i > 0 && <Divider />}
                <ListItem
                  sx={{ opacity: alert.isRead ? 0.6 : 1 }}
                  secondaryAction={
                    !alert.isRead && (
                      <Button size="small" onClick={() => markRead(String(alert.id))}>
                        Mark read
                      </Button>
                    )
                  }
                >
                  <ListItemText
                    primary={
                      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                        <Chip
                          label={alert.severity}
                          color={SEVERITY_COLORS[alert.severity] ?? 'default'}
                          size="small"
                        />
                        <Chip label={alert.reminderType} size="small" variant="outlined" />
                        {!alert.isRead && (
                          <Badge color="primary" variant="dot">
                            <span />
                          </Badge>
                        )}
                        <Typography variant="body2">{alert.message}</Typography>
                      </Stack>
                    }
                    secondary={
                      <Typography variant="caption" color="text.secondary">
                        {alert.entityCode} · {new Date(alert.triggeredAt).toLocaleString()}
                        {alert.isRead && alert.readAt && ` · Read ${new Date(alert.readAt).toLocaleString()}`}
                      </Typography>
                    }
                  />
                </ListItem>
              </Box>
            ))}
          </List>
        </Paper>
      )}
    </Box>
  );
}

const DEFAULT_TYPES = [
  { type: 'ProductionOrderOverdue', label: 'Production Order Overdue' },
  { type: 'ProductionOrderDeadlineApproaching', label: 'Deadline Approaching (≤2 days)' },
];

function ConfigurationsTab() {
  const qc = useQueryClient();
  const { data: configs, isLoading } = useGetApiV1RemindersConfigurations();
  const configList = configs as ReminderConfigDto[] | undefined;

  async function upsert(type: string, patch: { isEnabled?: boolean; leadTimeDays?: number; notificationChannel?: string }) {
    const existing = configList?.find(c => c.reminderType === type);
    await putApiV1RemindersConfigurationsReminderType(type, {
      isEnabled: patch.isEnabled ?? existing?.isEnabled ?? true,
      leadTimeDays: patch.leadTimeDays ?? existing?.leadTimeDays ?? 1,
      notificationChannel: patch.notificationChannel ?? existing?.notificationChannel ?? 'InApp',
    });
    qc.invalidateQueries({ queryKey: getGetApiV1RemindersConfigurationsQueryKey() });
  }

  if (isLoading) return <TablePageSkeleton />;

  return (
    <Paper variant="outlined">
      <List disablePadding>
        {DEFAULT_TYPES.map((t, i) => {
          const cfg = configList?.find(c => c.reminderType === t.type);
          return (
            <Box key={t.type}>
              {i > 0 && <Divider />}
              <ListItem>
                <ListItemText
                  primary={t.label}
                  secondary={`Type: ${t.type}`}
                />
                <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
                  <TextField
                    label="Lead days"
                    type="number"
                    size="small"
                    sx={{ width: 100 }}
                    value={cfg?.leadTimeDays ?? 1}
                    onChange={e => upsert(t.type, { leadTimeDays: Number(e.target.value) })}
                    slotProps={{ htmlInput: { min: 0, max: 30 } }}
                  />
                  <FormControlLabel
                    control={
                      <Switch
                        checked={cfg?.isEnabled ?? true}
                        onChange={e => upsert(t.type, { isEnabled: e.target.checked })}
                      />
                    }
                    label="Enabled"
                  />
                </Stack>
              </ListItem>
            </Box>
          );
        })}
      </List>
    </Paper>
  );
}
