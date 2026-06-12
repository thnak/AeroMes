import {
  alpha,
  AppBar,
  Badge,
  Box,
  Collapse,
  IconButton,
  InputBase,
  Stack,
  Tab,
  Tabs,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material';
import { useRef, useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { AnimatePresence, motion, type Variants } from 'framer-motion';
import SolarIcon from '../components/SolarIcon';
import UserMenu from '../components/UserMenu';
import { useAuth } from '../contexts/AuthContext';
import { MODULES } from '../modules';
import { APPBAR_HEIGHT } from '../theme/tokens';

const pageVariants: Variants = {
  initial: { opacity: 0, y: 8 },
  enter:   { opacity: 1, y: 0, transition: { duration: 0.18, ease: [0.4, 0, 0.2, 1] } },
  exit:    { opacity: 0, transition: { duration: 0.1 } },
};

export default function ModuleLayout() {
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const { hasRole } = useAuth();
  const [searchOpen, setSearchOpen] = useState(false);
  const searchRef = useRef<HTMLInputElement>(null);

  const mod = MODULES.find(
    (m) => pathname === m.path || pathname.startsWith(m.path + '/'),
  );

  // Filter tabs by user role
  const visibleTabs = mod
    ? mod.tabs.filter((t) => !t.roles || hasRole(t.roles))
    : [];

  const tabIndex = Math.max(
    0,
    visibleTabs.findIndex(
      (t) => pathname === t.path || pathname.startsWith(t.path + '/'),
    ),
  );

  function openSearch() {
    setSearchOpen(true);
    setTimeout(() => searchRef.current?.focus(), 50);
  }

  function closeSearch() {
    setSearchOpen(false);
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh', overflow: 'hidden' }}>
      <AppBar
        position="static"
        color="default"
        elevation={0}
        sx={{ bgcolor: 'background.paper', borderBottom: '1px solid', borderColor: 'divider' }}
      >
        <Toolbar variant="dense" sx={{ minHeight: APPBAR_HEIGHT, gap: 1 }}>
          {/* Home button */}
          <Tooltip title="Home">
            <IconButton size="small" onClick={() => navigate('/')} sx={{ color: 'text.secondary' }}>
              <SolarIcon name="dashboard" size={18} />
            </IconButton>
          </Tooltip>

          {/* Module identity */}
          {mod && !searchOpen && (
            <>
              <Box
                sx={(theme) => ({
                  width: 28,
                  height: 28,
                  borderRadius: 1.5,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: alpha(mod.color, theme.palette.mode === 'dark' ? 0.25 : 0.12),
                  color: mod.color,
                  flexShrink: 0,
                })}
              >
                <SolarIcon name={mod.icon} size={16} />
              </Box>
              <Typography variant="subtitle2" sx={{ fontWeight: 700, mr: 1 }}>
                {mod.label}
              </Typography>
            </>
          )}

          {/* Inline search field */}
          <Collapse in={searchOpen} orientation="horizontal" sx={{ flex: searchOpen ? 1 : 0, overflow: 'hidden' }}>
            <Box
              sx={(theme) => ({
                display: 'flex',
                alignItems: 'center',
                gap: 0.5,
                px: 1.5,
                height: 32,
                borderRadius: 2,
                bgcolor: alpha(theme.palette.text.primary, 0.06),
                width: '100%',
                maxWidth: 480,
              })}
            >
              <SolarIcon name="search" size={16} sx={{ color: 'text.disabled', flexShrink: 0 }} />
              <InputBase
                inputRef={searchRef}
                placeholder={mod ? `Search ${mod.label}…` : 'Search…'}
                fullWidth
                onKeyDown={(e) => e.key === 'Escape' && closeSearch()}
                sx={{ fontSize: '0.875rem' }}
              />
              <IconButton size="small" onClick={closeSearch} sx={{ color: 'text.disabled', p: 0.25 }}>
                <SolarIcon name="close" size={16} />
              </IconButton>
            </Box>
          </Collapse>

          <Box sx={{ flex: 1 }} />

          {/* Actions */}
          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
            {!searchOpen && (
              <Tooltip title="Search">
                <IconButton size="small" onClick={openSearch} sx={{ color: 'text.secondary' }}>
                  <SolarIcon name="search" size={20} />
                </IconButton>
              </Tooltip>
            )}

            <Tooltip title="Notifications">
              <IconButton size="small" sx={{ color: 'text.secondary' }}>
                <Badge
                  badgeContent={3}
                  color="error"
                  sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16 } }}
                >
                  <SolarIcon name="notifications" size={20} />
                </Badge>
              </IconButton>
            </Tooltip>

            <UserMenu />
          </Stack>
        </Toolbar>

        {/* Horizontal tabs — only when module has more than one visible tab */}
        {visibleTabs.length > 1 && (
          <Tabs
            value={tabIndex}
            onChange={(_, v) => navigate(visibleTabs[v].path)}
            variant="scrollable"
            scrollButtons="auto"
            sx={{
              minHeight: 36,
              px: 1,
              '& .MuiTab-root': {
                minHeight: 36,
                py: 0,
                fontSize: '0.8125rem',
                textTransform: 'none',
              },
              '& .MuiTabs-indicator': { height: 2 },
            }}
          >
            {visibleTabs.map((tab) => (
              <Tab key={tab.path} label={tab.label} />
            ))}
          </Tabs>
        )}
      </AppBar>

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
  );
}
