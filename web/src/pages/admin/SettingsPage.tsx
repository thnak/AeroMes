import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Divider,
  FormControlLabel,
  Grid,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useState } from 'react';
import { PageHeader, PageRoot, SolarIcon } from '../../components';
import { Icons } from '../../lib/icons';
import { Icon } from '@iconify/react';

// ─── Settings nav ─────────────────────────────────────────────────────────────

interface SettingsSection {
  key: string;
  label: string;
  icon: string;
  description: string;
}

const SECTIONS: SettingsSection[] = [
  { key: 'general',      label: 'General',          icon: Icons.settings,      description: 'Site name, timezone, locale' },
  { key: 'shifts',       label: 'Shift Config',     icon: Icons.time,          description: 'Shift times and schedules' },
  { key: 'notifications',label: 'Notifications',    icon: Icons.notifications, description: 'Alert rules and recipients' },
  { key: 'security',     label: 'Security & Auth',  icon: Icons.forbidden,     description: 'Password policy, MFA, sessions' },
  { key: 'integrations', label: 'Integrations',     icon: Icons.integration,   description: 'ERP / API connections' },
];

// ─── Section panels ───────────────────────────────────────────────────────────

function GeneralSettings() {
  const [saved, setSaved] = useState(false);
  function save() { setSaved(true); setTimeout(() => setSaved(false), 2000); }
  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="subtitle1" gutterBottom>Site Information</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField label="Site Name" defaultValue="AeroMes — Plant 01" fullWidth />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField label="Site Code" defaultValue="PLT-01" fullWidth
              slotProps={{ htmlInput: { style: { fontFamily: 'ui-monospace, monospace' } } }} />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField select label="Timezone" defaultValue="Asia/Ho_Chi_Minh" fullWidth>
              {['Asia/Ho_Chi_Minh', 'Asia/Bangkok', 'Asia/Tokyo', 'UTC'].map((tz) => (
                <MenuItem key={tz} value={tz}>{tz}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField select label="Date Format" defaultValue="DD/MM/YYYY" fullWidth>
              {['DD/MM/YYYY', 'MM/DD/YYYY', 'YYYY-MM-DD'].map((f) => (
                <MenuItem key={f} value={f}>{f}</MenuItem>
              ))}
            </TextField>
          </Grid>
        </Grid>
      </Box>

      <Box>
        <Typography variant="subtitle1" gutterBottom>Display</Typography>
        <Divider sx={{ mb: 2 }} />
        <Stack spacing={1.5}>
          <FormControlLabel control={<Switch defaultChecked color="primary" />}
            label={<><Typography variant="body2">Show OEE on dashboard</Typography><Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>Display OEE gauge on the main dashboard</Typography></>} />
          <FormControlLabel control={<Switch defaultChecked color="primary" />}
            label={<><Typography variant="body2">Auto-refresh dashboard</Typography><Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>Refresh KPIs every 30 seconds</Typography></>} />
          <FormControlLabel control={<Switch color="primary" />}
            label={<><Typography variant="body2">Compact table density</Typography><Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>Use smaller row height in all data grids</Typography></>} />
        </Stack>
      </Box>

      {saved && <Alert severity="success" sx={{ py: 0.75 }}>Settings saved successfully.</Alert>}
      <Box>
        <Button variant="contained" size="small" onClick={save}>Save Changes</Button>
      </Box>
    </Stack>
  );
}

function ShiftSettings() {
  const shifts = [
    { name: 'Day Shift',   start: '06:00', end: '14:00', color: '#F59E0B' },
    { name: 'Eve Shift',   start: '14:00', end: '22:00', color: '#3A9188' },
    { name: 'Night Shift', start: '22:00', end: '06:00', color: '#1D4ED8' },
  ];
  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="subtitle1" gutterBottom>Shift Schedule</Typography>
        <Divider sx={{ mb: 2 }} />
        <Stack spacing={2}>
          {shifts.map((s) => (
            <Stack key={s.name} direction="row" spacing={2} sx={{ alignItems: 'center' }}>
              <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: s.color, flexShrink: 0 }} />
              <Box sx={{ width: 130, flexShrink: 0 }}>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>{s.name}</Typography>
              </Box>
              <TextField size="small" type="time" defaultValue={s.start} label="Start" sx={{ width: 130 }} />
              <Typography variant="body2" color="text.secondary">to</Typography>
              <TextField size="small" type="time" defaultValue={s.end}  label="End"   sx={{ width: 130 }} />
            </Stack>
          ))}
        </Stack>
      </Box>
      <Box>
        <Button variant="contained" size="small">Save Shifts</Button>
      </Box>
    </Stack>
  );
}

function NotificationSettings() {
  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="subtitle1" gutterBottom>Alert Rules</Typography>
        <Divider sx={{ mb: 2 }} />
        <Stack spacing={1.5}>
          {[
            { label: 'Machine goes DOWN',          defaultChecked: true  },
            { label: 'OEE drops below 60%',        defaultChecked: true  },
            { label: 'Work order overdue',         defaultChecked: true  },
            { label: 'Critical maintenance issue', defaultChecked: true  },
            { label: 'Production target missed',   defaultChecked: false },
            { label: 'New integration order received', defaultChecked: true },
          ].map((r) => (
            <FormControlLabel key={r.label} control={<Switch defaultChecked={r.defaultChecked} color="primary" />}
              label={<Typography variant="body2">{r.label}</Typography>} />
          ))}
        </Stack>
      </Box>
      <Box>
        <Typography variant="subtitle1" gutterBottom>Notification Recipients</Typography>
        <Divider sx={{ mb: 2 }} />
        <TextField
          label="Email addresses (comma-separated)"
          fullWidth
          multiline
          rows={3}
          defaultValue="supervisor@aeromes.vn, manager@aeromes.vn"
          helperText="Alerts will be sent to these addresses"
        />
      </Box>
      <Box><Button variant="contained" size="small">Save Notifications</Button></Box>
    </Stack>
  );
}

function SecuritySettings() {
  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="subtitle1" gutterBottom>Password Policy</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField label="Minimum length" type="number" defaultValue={8} fullWidth />
          </Grid>
          <Grid size={{ xs: 12, sm: 6 }}>
            <TextField label="Session timeout (minutes)" type="number" defaultValue={480} fullWidth />
          </Grid>
        </Grid>
        <Stack spacing={1} sx={{ mt: 2 }}>
          {[
            { label: 'Require uppercase letter',     defaultChecked: true  },
            { label: 'Require number',               defaultChecked: true  },
            { label: 'Require special character',    defaultChecked: false },
          ].map((r) => (
            <FormControlLabel key={r.label} control={<Switch defaultChecked={r.defaultChecked} color="primary" />}
              label={<Typography variant="body2">{r.label}</Typography>} />
          ))}
        </Stack>
      </Box>
      <Box>
        <Typography variant="subtitle1" gutterBottom>Multi-Factor Authentication</Typography>
        <Divider sx={{ mb: 2 }} />
        <Stack spacing={1}>
          {[
            { label: 'Require MFA for admin accounts',  defaultChecked: true  },
            { label: 'Allow TOTP authenticator apps',   defaultChecked: true  },
            { label: 'Allow passkey (WebAuthn)',         defaultChecked: true  },
          ].map((r) => (
            <FormControlLabel key={r.label} control={<Switch defaultChecked={r.defaultChecked} color="primary" />}
              label={<Typography variant="body2">{r.label}</Typography>} />
          ))}
        </Stack>
      </Box>
      <Box><Button variant="contained" size="small">Save Security</Button></Box>
    </Stack>
  );
}

function IntegrationsSettings() {
  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="subtitle1" gutterBottom>ERP Connection</Typography>
        <Divider sx={{ mb: 2 }} />
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, sm: 8 }}>
            <TextField label="ERP API Base URL" defaultValue="https://erp.aeromes.vn/api" fullWidth />
          </Grid>
          <Grid size={{ xs: 12, sm: 4 }}>
            <TextField select label="Sync Interval" defaultValue="15" fullWidth>
              {['5', '10', '15', '30', '60'].map((v) => (
                <MenuItem key={v} value={v}>{v} min</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12 }}>
            <TextField label="API Key" defaultValue="••••••••••••••••••••••••••••••••" fullWidth type="password" />
          </Grid>
        </Grid>
        <Stack direction="row" spacing={1.5} sx={{ mt: 2, alignItems: 'center' }}>
          <Button variant="outlined" size="small" startIcon={<SolarIcon name="refresh" size={16} />}>
            Test Connection
          </Button>
          <Alert severity="success" sx={{ py: 0.5, flex: 1 }}>
            <Typography variant="caption">Last sync: 09 Jun 2026, 10:15 — 12 orders received</Typography>
          </Alert>
        </Stack>
      </Box>
      <Box><Button variant="contained" size="small">Save Integration</Button></Box>
    </Stack>
  );
}

const PANEL_MAP: Record<string, React.FC> = {
  general:      GeneralSettings,
  shifts:       ShiftSettings,
  notifications:NotificationSettings,
  security:     SecuritySettings,
  integrations: IntegrationsSettings,
};

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function SettingsPage() {
  const [activeSection, setActiveSection] = useState('general');
  const Panel = PANEL_MAP[activeSection] ?? GeneralSettings;

  return (
    <PageRoot>
      <PageHeader
        title="Settings"
        subtitle="Configure site preferences, shifts, notifications, and integrations"
        breadcrumbs={[{ label: 'Admin' }, { label: 'Settings' }]}
      />

      <Grid container spacing={2.5}>
        {/* Left nav */}
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <List disablePadding dense>
              {SECTIONS.map((s, i) => (
                <Box key={s.key}>
                  <ListItemButton
                    selected={activeSection === s.key}
                    onClick={() => setActiveSection(s.key)}
                    sx={(theme) => ({
                      px: 2,
                      py: 1.25,
                      mx: 0,
                      width: '100%',
                      borderRadius: 0,
                      bgcolor: activeSection === s.key ? alpha(theme.palette.primary.main, 0.08) : 'transparent',
                      borderLeft: '3px solid',
                      borderColor: activeSection === s.key ? 'primary.main' : 'transparent',
                      '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.05) },
                    })}
                  >
                    <ListItemIcon sx={{ minWidth: 36, color: activeSection === s.key ? 'primary.main' : 'text.secondary' }}>
                      <Icon icon={s.icon} width={18} height={18} />
                    </ListItemIcon>
                    <ListItemText
                      primary={<Typography variant="body2" sx={{ fontWeight: activeSection === s.key ? 600 : 400 }}>{s.label}</Typography>}
                      secondary={<Typography variant="caption" color="text.disabled">{s.description}</Typography>}
                    />
                  </ListItemButton>
                  {i < SECTIONS.length - 1 && <Divider />}
                </Box>
              ))}
            </List>
          </Card>
        </Grid>

        {/* Right panel */}
        <Grid size={{ xs: 12, md: 9 }}>
          <Card>
            <CardContent sx={{ p: '24px !important' }}>
              <Panel />
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </PageRoot>
  );
}
