import {
  Avatar,
  Badge,
  Box,
  Divider,
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  Tooltip,
  Typography,
  useColorScheme,
} from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import SolarIcon from './SolarIcon';
import { useAppInfo, useChangelog, getLastSeenRelease } from '../lib/useAppInfo';

export default function UserMenu() {
  const { mode, setMode } = useColorScheme();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const initials = user?.name?.split(' ').map((n) => n[0]).join('').slice(0, 2).toUpperCase() ?? 'U';
  const { data: appInfo } = useAppInfo();
  const { data: changelog } = useChangelog();

  // Show unread dot if latest release version hasn't been seen yet
  const latestVersion = changelog?.[0]?.version;
  const hasUnread = latestVersion != null && getLastSeenRelease() !== latestVersion;

  return (
    <>
      <Tooltip title={mode === 'dark' ? 'Light mode' : 'Dark mode'}>
        <IconButton
          size="small"
          onClick={() => setMode(mode === 'dark' ? 'light' : 'dark')}
          sx={{ color: 'text.secondary' }}
        >
          <SolarIcon name={mode === 'dark' ? 'lightMode' : 'darkMode'} size={20} />
        </IconButton>
      </Tooltip>

      <Avatar
        onClick={(e) => setAnchorEl(e.currentTarget)}
        sx={{
          width: 30,
          height: 30,
          ml: 0.5,
          bgcolor: 'primary.main',
          fontSize: '0.75rem',
          fontWeight: 700,
          cursor: 'pointer',
        }}
      >
        {initials}
      </Avatar>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={() => setAnchorEl(null)}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        slotProps={{ paper: { sx: { minWidth: 200, mt: 0.5 } } }}
      >
        <MenuItem disabled sx={{ opacity: '1 !important' }}>
          <Box>
            <Typography variant="body2" sx={{ fontWeight: 600 }}>
              {user?.name ?? 'User'}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {user?.email}
            </Typography>
          </Box>
        </MenuItem>
        <Divider />
        <MenuItem onClick={() => { setAnchorEl(null); navigate('/account'); }}>
          <ListItemIcon><SolarIcon name="profile" size={18} /></ListItemIcon>
          <ListItemText>My Account</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => { setAnchorEl(null); navigate('/auth/change-password'); }}>
          <ListItemIcon><SolarIcon name="settings" size={18} /></ListItemIcon>
          <ListItemText>Change Password</ListItemText>
        </MenuItem>
        <MenuItem onClick={() => { setAnchorEl(null); navigate('/admin/release-notes'); }}>
          <ListItemIcon>
            <Badge variant="dot" color="primary" invisible={!hasUnread}>
              <SolarIcon name="release" size={18} />
            </Badge>
          </ListItemIcon>
          <ListItemText>What's New</ListItemText>
          {hasUnread && (
            <Typography variant="caption" color="primary.main" sx={{ fontWeight: 700, fontSize: '0.625rem' }}>
              NEW
            </Typography>
          )}
        </MenuItem>
        <Divider />
        <MenuItem
          onClick={() => { setAnchorEl(null); logout(); navigate('/auth/login', { replace: true }); }}
          sx={{ color: 'error.main' }}
        >
          <ListItemIcon sx={{ color: 'error.main' }}><SolarIcon name="logout" size={18} /></ListItemIcon>
          <ListItemText>Sign out</ListItemText>
        </MenuItem>
        {appInfo && (
          <Box sx={{ px: 2, py: 1, borderTop: '1px solid', borderColor: 'divider' }}>
            <Typography variant="caption" color="text.disabled" sx={{ fontFamily: 'ui-monospace, monospace', fontSize: '0.625rem' }}>
              AeroMes v{appInfo.version} · {appInfo.environment}
            </Typography>
          </Box>
        )}
      </Menu>
    </>
  );
}
