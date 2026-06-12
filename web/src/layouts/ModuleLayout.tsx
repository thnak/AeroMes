import {
  alpha,
  AppBar,
  Badge,
  Box,
  ButtonBase,
  Divider,
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
        sx={{ bgcolor: 'background.paper', height: '85px', borderBottom: '1px solid', borderColor: 'divider' }}
      >
        {/* ── Shell bar ─────────────────────────────────────────────────────── */}
        <Toolbar
          variant="dense"
          sx={{ minHeight: APPBAR_HEIGHT, px: { xs: 1.5, sm: 2 }, gap: 1 }}
        >
          {/* Brand — always visible, clickable → home */}
          <ButtonBase
            onClick={() => navigate('/')}
            sx={{
              borderRadius: 1.5,
              display: 'flex',
              alignItems: 'center',
              gap: 0.75,
              px: 0.75,
              py: 0.5,
              flexShrink: 0,
              color: 'primary.main',
              transition: 'opacity 0.15s',
              '&:hover': { opacity: 0.8 },
            }}
          >
            <Box
              sx={{
                width: 26,
                height: 26,
                borderRadius: 1.25,
                bgcolor: 'primary.main',
                color: 'primary.contrastText',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                flexShrink: 0,
              }}
            >
              <SolarIcon name="production" size={15} />
            </Box>
            <Typography
              variant="subtitle2"
              sx={{ fontWeight: 700, color: 'primary.main', display: { xs: 'none', sm: 'block' } }}
            >
              AeroMes
            </Typography>
          </ButtonBase>

          {/* Module breadcrumb — hidden when search is open */}
          {mod && !searchOpen && (
            <>
              <Divider orientation="vertical" flexItem sx={{ mx: 0.25, my: 1 }} />
              <Box
                sx={(theme) => ({
                  display: 'flex',
                  alignItems: 'center',
                  gap: 0.75,
                  px: 1,
                  py: 0.375,
                  borderRadius: 1.5,
                  bgcolor: alpha(mod.color, theme.palette.mode === 'dark' ? 0.18 : 0.09),
                  flexShrink: 0,
                })}
              >
                <SolarIcon name={mod.icon} size={14} sx={{ color: mod.color, flexShrink: 0 }} />
                <Typography
                  variant="body2"
                  sx={{ fontWeight: 600, color: mod.color, fontSize: '0.8125rem', whiteSpace: 'nowrap' }}
                >
                  {mod.label}
                </Typography>
              </Box>
            </>
          )}

          {/* Search input — expands to fill centre when open */}
          {searchOpen && (
            <Box
              sx={(theme) => ({
                flex: 1,
                display: 'flex',
                alignItems: 'center',
                gap: 0.5,
                px: 1.5,
                height: 32,
                borderRadius: 2,
                bgcolor: alpha(theme.palette.text.primary, 0.06),
                minWidth: 0,
              })}
            >
              <SolarIcon name="search" size={16} sx={{ color: 'text.disabled', flexShrink: 0 }} />
              <InputBase
                inputRef={searchRef}
                placeholder={mod ? `Search in ${mod.label}…` : 'Search…'}
                fullWidth
                onKeyDown={(e) => e.key === 'Escape' && closeSearch()}
                sx={{ fontSize: '0.875rem' }}
              />
              <IconButton size="small" onClick={closeSearch} sx={{ color: 'text.disabled', p: 0.25, flexShrink: 0 }}>
                <SolarIcon name="close" size={16} />
              </IconButton>
            </Box>
          )}

          <Box sx={{ flex: 1 }} />

          {/* Right actions */}
          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', flexShrink: 0 }}>
            {!searchOpen && (
              <Tooltip title="Search (/)">
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

        {/* ── Tab bar ──────────────────────────────────────────────────────── */}
        {visibleTabs.length > 1 && (
          <Tabs
            value={tabIndex}
            onChange={(_, v) => navigate(visibleTabs[v].path)}
            variant="scrollable"
            scrollButtons="auto"
            sx={{
              minHeight: 36,
              px: 2,
              borderTop: '1px solid',
              borderColor: 'divider',
              '& .MuiTab-root': {
                minHeight: 36,
                py: 0,
                fontSize: '0.8125rem',
                textTransform: 'none',
                fontWeight: 500,
              },
              '& .MuiTab-root.Mui-selected': { fontWeight: 600 },
              '& .MuiTabs-indicator': { height: 2 },
            }}
          >
            {visibleTabs.map((tab) => (
              <Tab key={tab.path} label={tab.label} />
            ))}
          </Tabs>
        )}
      </AppBar>

      {/* ── Page content ─────────────────────────────────────────────────── */}
      <Box sx={{ flex: 1, overflowY: 'auto', overflowX: 'hidden', bgcolor: 'background.default' }}>
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
