import {
  AppBar,
  Avatar,
  Badge,
  Box,
  Breadcrumbs,
  IconButton,
  Stack,
  Toolbar,
  Tooltip,
  Typography,
  useColorScheme,
} from '@mui/material';
import { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { motion, AnimatePresence, type Variants } from 'framer-motion';
import SolarIcon from '../components/SolarIcon';
import Sidebar from './Sidebar';
import { APPBAR_HEIGHT, SIDEBAR_COLLAPSED_WIDTH, SIDEBAR_WIDTH } from '../theme/tokens';
import { NAV_SECTIONS } from './Sidebar';

// ─── Breadcrumb derivation from route ─────────────────────────────────────────

function useBreadcrumbs() {
  const { pathname } = useLocation();
  const segments = pathname.split('/').filter(Boolean);

  // Find matching nav labels
  const allItems = NAV_SECTIONS.flatMap((s) => s.items);
  const crumbs: { label: string; href?: string }[] = [{ label: 'Home', href: '/' }];

  let accumulated = '';
  for (const seg of segments) {
    accumulated += '/' + seg;
    const match = allItems.find(
      (item) => item.href === accumulated || item.href?.startsWith(accumulated)
    );
    crumbs.push({
      label: match?.label ?? seg.charAt(0).toUpperCase() + seg.slice(1).replace(/-/g, ' '),
      href: match?.href ?? accumulated,
    });
  }
  return crumbs;
}

// ─── Top AppBar ───────────────────────────────────────────────────────────────

function TopBar({ sidebarWidth }: { sidebarWidth: number }) {
  const { mode, setMode } = useColorScheme();
  const breadcrumbs = useBreadcrumbs();
  const current = breadcrumbs[breadcrumbs.length - 1];

  return (
    <AppBar
      position="fixed"
      color="default"
      elevation={0}
      sx={(theme) => ({
        left: sidebarWidth,
        width: `calc(100% - ${sidebarWidth}px)`,
        bgcolor: 'background.paper',
        transition: theme.transitions.create(['left', 'width'], {
          easing: theme.transitions.easing.easeInOut,
          duration: 220,
        }),
        zIndex: theme.zIndex.drawer - 1,
      })}
    >
      <Toolbar>
        {/* Breadcrumb */}
        <Breadcrumbs
          aria-label="breadcrumb"
          sx={{ flex: 1, minWidth: 0, '& .MuiBreadcrumbs-separator': { mx: 0.5 } }}
        >
          {breadcrumbs.slice(0, -1).map((crumb, i) => (
            <Typography key={i} variant="caption" color="text.disabled">
              {crumb.label}
            </Typography>
          ))}
          <Typography variant="caption" color="text.primary" sx={{ fontWeight: 600 }}>
            {current.label}
          </Typography>
        </Breadcrumbs>

        {/* Quick actions */}
        <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
          <Tooltip title="Notifications">
            <IconButton size="small" sx={{ color: 'text.secondary' }}>
              <Badge badgeContent={3} color="error" sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16 } }}>
                <SolarIcon name="notifications" size={20} />
              </Badge>
            </IconButton>
          </Tooltip>

          <Tooltip title="Settings">
            <IconButton size="small" sx={{ color: 'text.secondary' }}>
              <SolarIcon name="settings" size={20} />
            </IconButton>
          </Tooltip>

          <Tooltip title={mode === 'dark' ? 'Light mode' : 'Dark mode'}>
            <IconButton
              size="small"
              onClick={() => setMode(mode === 'dark' ? 'light' : 'dark')}
              sx={{ color: 'text.secondary' }}
            >
              <SolarIcon name={mode === 'dark' ? 'lightMode' : 'darkMode'} size={20} />
            </IconButton>
          </Tooltip>

          <Tooltip title="Account">
            <Avatar
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
              OP
            </Avatar>
          </Tooltip>
        </Stack>
      </Toolbar>
    </AppBar>
  );
}

// ─── Page transition wrapper ──────────────────────────────────────────────────

const pageVariants: Variants = {
  initial: { opacity: 0, y: 8 },
  enter:   { opacity: 1, y: 0, transition: { duration: 0.18, ease: [0.4, 0, 0.2, 1] } },
  exit:    { opacity: 0, transition: { duration: 0.1 } },
};

// ─── WebLayout ────────────────────────────────────────────────────────────────

export default function WebLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const { pathname } = useLocation();
  const sidebarWidth = collapsed ? SIDEBAR_COLLAPSED_WIDTH : SIDEBAR_WIDTH;

  return (
    <Box sx={{ display: 'flex', height: '100vh', overflow: 'hidden' }}>
      <Sidebar collapsed={collapsed} onToggle={() => setCollapsed((v) => !v)} />

      {/* Main area */}
      <Box
        component="main"
        sx={(theme) => ({
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          minWidth: 0,
          marginLeft: `${sidebarWidth}px`,
          transition: theme.transitions.create('margin-left', {
            easing: theme.transitions.easing.easeInOut,
            duration: 220,
          }),
        })}
      >
        <TopBar sidebarWidth={sidebarWidth} />

        {/* Spacer for fixed AppBar */}
        <Box sx={{ height: APPBAR_HEIGHT, flexShrink: 0 }} />

        {/* Scrollable content */}
        <Box
          sx={{
            flex: 1,
            overflowY: 'auto',
            overflowX: 'hidden',
            bgcolor: 'background.default',
          }}
        >
          <AnimatePresence mode="wait">
            <motion.div
              key={pathname}
              variants={pageVariants}
              initial="initial"
              animate="enter"
              exit="exit"
              style={{ minHeight: '100%', display: 'flex', flexDirection: 'column' }}
            >
              <Outlet />
            </motion.div>
          </AnimatePresence>
        </Box>
      </Box>
    </Box>
  );
}
