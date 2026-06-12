import {
  alpha,
  AppBar,
  Badge,
  Box,
  Card,
  CardActionArea,
  Chip,
  Grid,
  IconButton,
  InputBase,
  Stack,
  Toolbar,
  Tooltip,
  Typography,
} from '@mui/material';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import SolarIcon from '../components/SolarIcon';
import UserMenu from '../components/UserMenu';
import { useAuth } from '../contexts/AuthContext';
import { MODULES, type ModuleConfig } from '../modules';
import { APPBAR_HEIGHT } from '../theme/tokens';

const PINNED_KEY = 'aeromes:pinned-modules';

function loadPinned(): Set<string> {
  try {
    const raw = localStorage.getItem(PINNED_KEY);
    return new Set(raw ? JSON.parse(raw) : []);
  } catch {
    return new Set();
  }
}

function savePinned(ids: Set<string>) {
  localStorage.setItem(PINNED_KEY, JSON.stringify([...ids]));
}

export default function LaunchpadPage() {
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const [search, setSearch] = useState('');
  const [pinned, setPinned] = useState<Set<string>>(loadPinned);

  function togglePin(id: string) {
    setPinned((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      savePinned(next);
      return next;
    });
  }

  function handleOpen(mod: ModuleConfig) {
    navigate(mod.tabs[0]?.path ?? mod.path);
  }

  // Role-filtered modules
  const visible = useMemo(
    () => MODULES.filter((m) => !m.roles || hasRole(m.roles)),
    [hasRole],
  );

  // Search filter
  const q = search.trim().toLowerCase();
  const filtered = q
    ? visible.filter(
        (m) =>
          m.label.toLowerCase().includes(q) ||
          m.description.toLowerCase().includes(q),
      )
    : visible;

  const pinnedMods = filtered.filter((m) => pinned.has(m.id) && m.available);
  const availableMods = filtered.filter((m) => m.available && !pinned.has(m.id));
  const upcomingMods = filtered.filter((m) => !m.available);

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh', bgcolor: 'background.default' }}>
      {/* Shell bar */}
      <AppBar
        position="static"
        color="default"
        elevation={0}
        sx={{ bgcolor: 'background.paper', borderBottom: '1px solid', borderColor: 'divider' }}
      >
        <Toolbar variant="dense" sx={{ minHeight: APPBAR_HEIGHT, gap: 1 }}>
          <Box
            sx={{
              width: 28,
              height: 28,
              borderRadius: 1.5,
              bgcolor: 'primary.main',
              color: 'primary.contrastText',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              flexShrink: 0,
            }}
          >
            <SolarIcon name="production" size={16} />
          </Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 700, color: 'primary.main' }}>
            AeroMes
          </Typography>

          {/* Global search */}
          <Box
            sx={(theme) => ({
              display: 'flex',
              alignItems: 'center',
              gap: 0.5,
              px: 1.5,
              ml: 2,
              height: 32,
              borderRadius: 2,
              bgcolor: alpha(theme.palette.text.primary, 0.06),
              width: 260,
              flexShrink: 0,
            })}
          >
            <SolarIcon name="search" size={16} sx={{ color: 'text.disabled', flexShrink: 0 }} />
            <InputBase
              placeholder="Find application…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              fullWidth
              sx={{ fontSize: '0.875rem' }}
            />
            {search && (
              <IconButton size="small" onClick={() => setSearch('')} sx={{ color: 'text.disabled', p: 0.25 }}>
                <SolarIcon name="close" size={16} />
              </IconButton>
            )}
          </Box>

          <Box sx={{ flex: 1 }} />

          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
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
      </AppBar>

      {/* Module grid */}
      <Box sx={{ flex: 1, p: { xs: 2, sm: 4 }, maxWidth: 1200, width: '100%', mx: 'auto' }}>

        {/* Pinned section */}
        {pinnedMods.length > 0 && !q && (
          <Box sx={{ mb: 4 }}>
            <SectionHeading label="Pinned" />
            <Grid container spacing={2}>
              {pinnedMods.map((mod) => (
                <Grid key={mod.id} size={{ xs: 6, sm: 4, md: 3, lg: 2 }}>
                  <ModuleTile
                    mod={mod}
                    pinned
                    onOpen={() => handleOpen(mod)}
                    onPin={() => togglePin(mod.id)}
                  />
                </Grid>
              ))}
            </Grid>
          </Box>
        )}

        {/* Available */}
        {availableMods.length > 0 && (
          <Box sx={{ mb: 4 }}>
            {(pinnedMods.length > 0 || upcomingMods.length > 0) && (
              <SectionHeading label="Applications" />
            )}
            <Grid container spacing={2}>
              {availableMods.map((mod) => (
                <Grid key={mod.id} size={{ xs: 6, sm: 4, md: 3, lg: 2 }}>
                  <ModuleTile
                    mod={mod}
                    pinned={pinned.has(mod.id)}
                    onOpen={() => handleOpen(mod)}
                    onPin={() => togglePin(mod.id)}
                  />
                </Grid>
              ))}
            </Grid>
          </Box>
        )}

        {/* Coming soon */}
        {upcomingMods.length > 0 && (
          <Box>
            <SectionHeading label="Coming soon" />
            <Grid container spacing={2}>
              {upcomingMods.map((mod) => (
                <Grid key={mod.id} size={{ xs: 6, sm: 4, md: 3, lg: 2 }}>
                  <ModuleTile mod={mod} disabled pinned={false} onOpen={() => {}} onPin={() => {}} />
                </Grid>
              ))}
            </Grid>
          </Box>
        )}

        {/* Empty search result */}
        {filtered.length === 0 && (
          <Box sx={{ textAlign: 'center', py: 8 }}>
            <SolarIcon name="emptySearch" size={48} sx={{ color: 'text.disabled', mb: 2 }} />
            <Typography color="text.secondary">No applications match "{search}"</Typography>
          </Box>
        )}
      </Box>
    </Box>
  );
}

function SectionHeading({ label }: { label: string }) {
  return (
    <Typography
      variant="overline"
      color="text.disabled"
      sx={{ display: 'block', mb: 1.5, letterSpacing: '0.08em' }}
    >
      {label}
    </Typography>
  );
}

function ModuleTile({
  mod,
  pinned,
  onOpen,
  onPin,
  disabled,
}: {
  mod: ModuleConfig;
  pinned: boolean;
  onOpen: () => void;
  onPin: () => void;
  disabled?: boolean;
}) {
  const [hovered, setHovered] = useState(false);

  return (
    <Card
      variant="outlined"
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      sx={{
        position: 'relative',
        opacity: disabled ? 0.45 : 1,
        transition: 'box-shadow 0.15s ease, transform 0.15s ease',
        '&:hover': disabled ? {} : { boxShadow: 4, transform: 'translateY(-2px)' },
      }}
    >
      {/* Pin button — visible on hover or when pinned */}
      {!disabled && (hovered || pinned) && (
        <Tooltip title={pinned ? 'Unpin' : 'Pin to top'}>
          <IconButton
            size="small"
            onClick={(e) => { e.stopPropagation(); onPin(); }}
            sx={{
              position: 'absolute',
              top: 6,
              right: 6,
              zIndex: 1,
              p: 0.5,
              color: pinned ? 'primary.main' : 'text.disabled',
              bgcolor: 'background.paper',
              '&:hover': { bgcolor: 'action.hover' },
            }}
          >
            <SolarIcon name={pinned ? 'complete' : 'add'} size={14} />
          </IconButton>
        </Tooltip>
      )}

      <CardActionArea
        onClick={onOpen}
        disabled={disabled}
        sx={{
          p: 2.5,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'flex-start',
          gap: 1.5,
          minHeight: 120,
        }}
      >
        <Box
          sx={(theme) => ({
            width: 44,
            height: 44,
            borderRadius: 2,
            bgcolor: alpha(mod.color, theme.palette.mode === 'dark' ? 0.2 : 0.1),
            color: mod.color,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0,
          })}
        >
          <SolarIcon name={mod.icon} size={24} />
        </Box>

        <Box sx={{ pr: (!disabled && (hovered || pinned)) ? 2 : 0, transition: 'padding 0.1s' }}>
          <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center', mb: 0.25 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, lineHeight: 1.3 }}>
              {mod.label}
            </Typography>
            {disabled && (
              <Chip label="Soon" size="small" sx={{ height: 16, fontSize: '0.6rem', px: 0.25 }} />
            )}
          </Stack>
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
            {mod.description}
          </Typography>
        </Box>
      </CardActionArea>
    </Card>
  );
}
