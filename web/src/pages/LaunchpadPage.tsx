import {
  alpha,
  AppBar,
  Badge,
  Box,
  ButtonBase,
  Chip,
  Divider,
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
import { useColorScheme } from '@mui/material/styles';
import SolarIcon from '../components/SolarIcon';
import UserMenu from '../components/UserMenu';
import { useAuth } from '../contexts/AuthContext';
import { MODULES, type ModuleConfig } from '../modules';
import { moduleCardImages, moduleCardImagesLight } from '../assets/illustrations';
import { APPBAR_HEIGHT } from '../theme/tokens';

// ─── Persistence ─────────────────────────────────────────────────────────────

const VISITS_KEY = 'aeromes:module-visits';
const PINNED_KEY  = 'aeromes:pinned-modules';

function loadVisits(): Record<string, number> {
  try { return JSON.parse(localStorage.getItem(VISITS_KEY) ?? '{}'); }
  catch { return {}; }
}
function recordVisit(id: string) {
  const v = loadVisits();
  v[id] = (v[id] ?? 0) + 1;
  localStorage.setItem(VISITS_KEY, JSON.stringify(v));
}
function loadPinned(): Set<string> {
  try { const r = localStorage.getItem(PINNED_KEY); return new Set(r ? JSON.parse(r) : []); }
  catch { return new Set(); }
}
function savePinned(s: Set<string>) {
  localStorage.setItem(PINNED_KEY, JSON.stringify([...s]));
}

// ─── Sidebar categories ───────────────────────────────────────────────────────

const SIDEBAR_CATEGORIES = [
  { label: 'Operations',  color: '#1D4ED8', ids: ['production', 'quality', 'maintenance'] },
  { label: 'Management',  color: '#DC2626', ids: ['admin', 'integration'] },
  { label: 'Analytics',   color: '#0D9488', ids: ['reports'] },
  { label: 'Data',        color: '#7C3AED', ids: ['master'] },
  { label: 'Monitoring',  color: '#0891B2', ids: ['iot', 'planning', 'warehouse', 'lab', 'traceability'] },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

type ViewMode = 'grid' | 'list';

export default function LaunchpadPage() {
  const navigate = useNavigate();
  const { user, hasRole } = useAuth();
  const [search, setSearch]               = useState('');
  const [viewMode, setViewMode]           = useState<ViewMode>('grid');
  const [selectedCategory, setCategory]  = useState<string | null>(null);
  const [pinned, setPinned]              = useState<Set<string>>(loadPinned);
  const [visits]                         = useState<Record<string, number>>(loadVisits);

  function togglePin(id: string) {
    setPinned((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      savePinned(next);
      return next;
    });
  }

  function handleOpen(mod: ModuleConfig) {
    recordVisit(mod.id);
    navigate(mod.tabs[0]?.path ?? mod.path);
  }

  // Role-filtered modules
  const visible = useMemo(
    () => MODULES.filter((m) => !m.roles || hasRole(m.roles)),
    [hasRole],
  );

  // Apply search + category filter
  const q = search.trim().toLowerCase();
  const filtered = useMemo(() => {
    let r = visible;
    if (q) r = r.filter((m) => m.label.toLowerCase().includes(q) || m.description.toLowerCase().includes(q));
    if (selectedCategory) {
      const cat = SIDEBAR_CATEGORIES.find((c) => c.label === selectedCategory);
      if (cat) r = r.filter((m) => cat.ids.includes(m.id));
    }
    return r;
  }, [visible, q, selectedCategory]);

  const availableMods  = filtered.filter((m) => m.available);
  const upcomingMods   = filtered.filter((m) => !m.available);

  // Sidebar: top-5 accessed modules (fall back to first 5 available)
  const mostAccessed = useMemo(() => {
    const avail = visible.filter((m) => m.available);
    return [...avail]
      .sort((a, b) => (visits[b.id] ?? 0) - (visits[a.id] ?? 0))
      .slice(0, 5);
  }, [visible, visits]);

  const firstName = user?.name?.split(' ')[0] ?? 'Back';

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh', overflow: 'hidden', bgcolor: 'background.default' }}>

      {/* ── Shell bar ──────────────────────────────────────────────────────── */}
      <AppBar
        position="static"
        color="default"
        elevation={0}
        sx={{ bgcolor: 'background.paper', borderBottom: '1px solid', borderColor: 'divider', zIndex: 10 }}
      >
        <Toolbar variant="dense" sx={{ minHeight: APPBAR_HEIGHT, px: { xs: 1.5, sm: 2 }, gap: 1 }}>
          {/* Brand */}
          <Stack direction="row" sx={{ alignItems: 'center', gap: 0.75, flexShrink: 0 }}>
            <Box sx={{ width: 26, height: 26, borderRadius: 1.25, bgcolor: 'primary.main', color: 'primary.contrastText', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <SolarIcon name="production" size={15} />
            </Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, color: 'primary.main' }}>
              AeroMes
            </Typography>
          </Stack>

          {/* Search */}
          <Box
            sx={(theme) => ({
              display: 'flex', alignItems: 'center', gap: 0.5,
              px: 1.5, ml: 2, height: 32, borderRadius: 2,
              bgcolor: alpha(theme.palette.text.primary, 0.06),
              width: 240, flexShrink: 0,
            })}
          >
            <SolarIcon name="search" size={15} sx={{ color: 'text.disabled', flexShrink: 0 }} />
            <InputBase
              placeholder="Find application…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              fullWidth
              sx={{ fontSize: '0.875rem' }}
            />
            {search && (
              <IconButton size="small" onClick={() => setSearch('')} sx={{ color: 'text.disabled', p: 0.25 }}>
                <SolarIcon name="close" size={14} />
              </IconButton>
            )}
          </Box>

          <Box sx={{ flex: 1 }} />

          <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
            <Tooltip title="Notifications">
              <IconButton size="small" sx={{ color: 'text.secondary' }}>
                <Badge badgeContent={3} color="error" sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16 } }}>
                  <SolarIcon name="notifications" size={20} />
                </Badge>
              </IconButton>
            </Tooltip>
            <UserMenu />
          </Stack>
        </Toolbar>
      </AppBar>

      {/* ── Body ───────────────────────────────────────────────────────────── */}
      <Box sx={{ display: 'flex', flex: 1, overflow: 'hidden' }}>

        {/* ── Sidebar ──────────────────────────────────────────────────────── */}
        <Box
          sx={{
            width: 230,
            flexShrink: 0,
            display: { xs: 'none', md: 'flex' },
            flexDirection: 'column',
            borderRight: '1px solid',
            borderColor: 'divider',
            bgcolor: 'background.paper',
            overflowY: 'auto',
          }}
        >
          {/* Welcome */}
          <Box sx={{ px: 2.5, pt: 3, pb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 700, lineHeight: 1.3 }}>
              Welcome Back {firstName} 👋
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
              Select a module to get started
            </Typography>
          </Box>

          <Divider />

          {/* Quick Access */}
          <Box sx={{ px: 2, pt: 2, pb: 1 }}>
            <Typography variant="overline" color="text.disabled" sx={{ fontSize: '0.6875rem', letterSpacing: '0.08em' }}>
              Most Accessed
            </Typography>
            <Stack sx={{ mt: 1, gap: 0.25 }}>
              {mostAccessed.map((mod) => (
                <ButtonBase
                  key={mod.id}
                  onClick={() => handleOpen(mod)}
                  sx={{
                    display: 'flex', alignItems: 'center', gap: 1,
                    px: 1, py: 0.625,
                    borderRadius: 1.25,
                    width: '100%', textAlign: 'left',
                    '&:hover': { bgcolor: 'action.hover' },
                  }}
                >
                  <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: mod.color, flexShrink: 0 }} />
                  <Typography variant="body2" sx={{ flex: 1, fontSize: '0.8125rem', fontWeight: 500, color: 'text.primary' }} noWrap>
                    {mod.label}
                  </Typography>
                  {(visits[mod.id] ?? 0) > 0 && (
                    <Typography variant="caption" color="text.disabled" sx={{ fontSize: '0.6875rem', flexShrink: 0 }}>
                      {visits[mod.id]}×
                    </Typography>
                  )}
                </ButtonBase>
              ))}
            </Stack>
          </Box>

          <Box sx={{ px: 2, pt: 1.5, pb: 1 }}>
            <Typography variant="overline" color="text.disabled" sx={{ fontSize: '0.6875rem', letterSpacing: '0.08em' }}>
              Browse by Category
            </Typography>
            <Stack sx={{ mt: 1, gap: 0.25 }}>
              {SIDEBAR_CATEGORIES.map((cat) => {
                const active = selectedCategory === cat.label;
                return (
                  <ButtonBase
                    key={cat.label}
                    onClick={() => setCategory(active ? null : cat.label)}
                    sx={{
                      display: 'flex', alignItems: 'center', gap: 1,
                      px: 1, py: 0.625,
                      borderRadius: 1.25,
                      width: '100%', textAlign: 'left',
                      bgcolor: active ? alpha(cat.color, 0.1) : 'transparent',
                      '&:hover': { bgcolor: active ? alpha(cat.color, 0.14) : 'action.hover' },
                    }}
                  >
                    <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: cat.color, flexShrink: 0 }} />
                    <Typography
                      variant="body2"
                      sx={{ fontSize: '0.8125rem', fontWeight: active ? 600 : 500, color: active ? cat.color : 'text.primary' }}
                    >
                      {cat.label}
                    </Typography>
                  </ButtonBase>
                );
              })}
            </Stack>
          </Box>

          {/* System Status — pinned to bottom */}
          <Box sx={{ mt: 'auto' }}>
            <Divider />
            <Box sx={{ px: 2.5, py: 2 }}>
              <Typography variant="overline" color="text.disabled" sx={{ fontSize: '0.6875rem', letterSpacing: '0.08em' }}>
                System Status
              </Typography>
              <Stack direction="row" sx={{ alignItems: 'center', gap: 1, mt: 0.75 }}>
                <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: 'success.main', boxShadow: '0 0 0 3px rgba(21,128,61,0.2)' }} />
                <Typography variant="body2" sx={{ fontSize: '0.8125rem', color: 'success.main', fontWeight: 500 }}>
                  All systems operational
                </Typography>
              </Stack>
            </Box>
          </Box>
        </Box>

        {/* ── Main content ─────────────────────────────────────────────────── */}
        <Box sx={{ flex: 1, overflowY: 'auto', overflowX: 'hidden' }}>
          <Box sx={{ maxWidth: 1040, mx: 'auto', px: { xs: 2, sm: 3 }, py: 3 }}>

            {/* Toolbar row */}
            <Stack direction="row" sx={{ alignItems: 'center', mb: 3, gap: 1 }}>
              <Typography variant="body2" color="text.secondary">
                {availableMods.length} module{availableMods.length !== 1 ? 's' : ''}
                {selectedCategory && ` in ${selectedCategory}`}
              </Typography>
              <Box sx={{ flex: 1 }} />
              {/* Grid / List toggle */}
              <Stack direction="row" sx={{ alignItems: 'center', border: '1px solid', borderColor: 'divider', borderRadius: 1.5, overflow: 'hidden' }}>
                <Tooltip title="Grid view">
                  <IconButton
                    size="small"
                    onClick={() => setViewMode('grid')}
                    sx={{
                      borderRadius: 0, px: 1,
                      bgcolor: viewMode === 'grid' ? 'action.selected' : 'transparent',
                      color: viewMode === 'grid' ? 'primary.main' : 'text.secondary',
                    }}
                  >
                    <SolarIcon name="dashboard" size={16} />
                  </IconButton>
                </Tooltip>
                <Divider orientation="vertical" flexItem />
                <Tooltip title="List view">
                  <IconButton
                    size="small"
                    onClick={() => setViewMode('list')}
                    sx={{
                      borderRadius: 0, px: 1,
                      bgcolor: viewMode === 'list' ? 'action.selected' : 'transparent',
                      color: viewMode === 'list' ? 'primary.main' : 'text.secondary',
                    }}
                  >
                    <SolarIcon name="order" size={16} />
                  </IconButton>
                </Tooltip>
              </Stack>
            </Stack>

            {/* Available modules */}
            {availableMods.length > 0 && (
              viewMode === 'grid' ? (
                <Grid container spacing={2}>
                  {availableMods.map((mod, i) => (
                    <Grid key={mod.id} size={{ xs: 12, sm: 6, md: 4 }}>
                      <ModuleCard
                        mod={mod}
                        index={i}
                        pinned={pinned.has(mod.id)}
                        onOpen={() => handleOpen(mod)}
                        onPin={() => togglePin(mod.id)}
                      />
                    </Grid>
                  ))}
                </Grid>
              ) : (
                <Stack sx={{ gap: 1 }}>
                  {availableMods.map((mod) => (
                    <ModuleListRow key={mod.id} mod={mod} onOpen={() => handleOpen(mod)} />
                  ))}
                </Stack>
              )
            )}

            {/* Upcoming modules */}
            {upcomingMods.length > 0 && !q && (
              <Box sx={{ mt: 4 }}>
                <Typography variant="overline" color="text.disabled" sx={{ letterSpacing: '0.08em' }}>
                  Coming Soon
                </Typography>
                <Grid container spacing={2} sx={{ mt: 0.5 }}>
                  {upcomingMods.map((mod, i) => (
                    <Grid key={mod.id} size={{ xs: 12, sm: 6, md: 4 }}>
                      <ModuleCard
                        mod={mod}
                        index={availableMods.length + i}
                        pinned={false}
                        disabled
                        onOpen={() => {}}
                        onPin={() => {}}
                      />
                    </Grid>
                  ))}
                </Grid>
              </Box>
            )}

            {/* Empty state */}
            {filtered.length === 0 && (
              <Box sx={{ textAlign: 'center', py: 10 }}>
                <SolarIcon name="emptySearch" size={48} sx={{ color: 'text.disabled', mb: 1.5 }} />
                <Typography variant="subtitle1" color="text.secondary">
                  No modules match "{search}"
                </Typography>
                <Typography variant="body2" color="text.disabled" sx={{ mt: 0.5 }}>
                  Try a different search term or browse by category
                </Typography>
              </Box>
            )}
          </Box>
        </Box>
      </Box>
    </Box>
  );
}

// ─── Module card (dark, gradient) ─────────────────────────────────────────────

function ModuleCard({
  mod, index, pinned, onOpen, onPin, disabled,
}: {
  mod: ModuleConfig;
  index: number;
  pinned: boolean;
  onOpen: () => void;
  onPin: () => void;
  disabled?: boolean;
}) {
  const [hovered, setHovered] = useState(false);
  const { mode } = useColorScheme();
  const isLight = mode === 'light';
  const cardImages = isLight ? moduleCardImagesLight : moduleCardImages;
  const cardBg = cardImages[mod.id];

  return (
    <ButtonBase
      onClick={disabled ? undefined : onOpen}
      onMouseEnter={() => !disabled && setHovered(true)}
      onMouseLeave={() => setHovered(false)}
      disabled={disabled}
      sx={{
        display: 'block',
        width: '100%',
        textAlign: 'left',
        borderRadius: 2.5,
        overflow: 'hidden',
        position: 'relative',
        height: 200,
        backgroundImage: cardBg
          ? `linear-gradient(160deg, ${alpha(isLight ? '#f8fafc' : '#0d1117', isLight ? 0.40 : 0.55)} 0%, ${alpha(mod.color, isLight ? 0.35 : 0.55)} 100%), url(${cardBg})`
          : undefined,
        background: cardBg ? undefined : (t) =>
          t.palette.mode === 'dark'
            ? `radial-gradient(ellipse at 80% 10%, ${alpha(mod.color, 0.45)} 0%, transparent 55%), #0d1117`
            : `radial-gradient(ellipse at 80% 10%, ${alpha(mod.color, 0.35)} 0%, transparent 55%), #f1f5f9`,
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        border: '1px solid',
        borderColor: hovered ? alpha(mod.color, 0.45) : alpha(mod.color, 0.18),
        transition: 'all 0.2s cubic-bezier(0.4,0,0.2,1)',
        transform: hovered ? 'translateY(-3px)' : 'translateY(0)',
        boxShadow: hovered
          ? `0 16px 40px ${alpha(mod.color, 0.22)}, 0 2px 8px rgba(0,0,0,0.4)`
          : '0 2px 8px rgba(0,0,0,0.25)',
        opacity: disabled ? 0.4 : 1,
        cursor: disabled ? 'default' : 'pointer',
        '&.Mui-disabled': { opacity: 0.4 },
      }}
    >
      {/* Watermark number */}
      <Typography
        component="span"
        sx={{
          position: 'absolute', right: 14, bottom: 2,
          fontSize: '5.5rem', fontWeight: 900,
          color: isLight ? '#000' : 'white', opacity: 0.055, lineHeight: 1,
          fontFamily: 'ui-monospace, monospace',
          userSelect: 'none', pointerEvents: 'none',
        }}
      >
        {String(index + 1).padStart(2, '0')}
      </Typography>

      {/* Subtle grid lines overlay */}
      <Box
        sx={{
          position: 'absolute', inset: 0, pointerEvents: 'none',
          backgroundImage: isLight
            ? `linear-gradient(${alpha('#000', 0.025)} 1px, transparent 1px), linear-gradient(90deg, ${alpha('#000', 0.025)} 1px, transparent 1px)`
            : `linear-gradient(${alpha('#fff', 0.03)} 1px, transparent 1px), linear-gradient(90deg, ${alpha('#fff', 0.03)} 1px, transparent 1px)`,
          backgroundSize: '32px 32px',
        }}
      />

      {/* Content */}
      <Box sx={{ position: 'relative', p: 2.5, height: '100%', display: 'flex', flexDirection: 'column' }}>
        {/* Icon */}
        <Box
          sx={{
            width: 44, height: 44, borderRadius: 2,
            bgcolor: alpha(mod.color, isLight ? 0.18 : 0.25),
            border: '1px solid', borderColor: alpha(mod.color, isLight ? 0.35 : 0.45),
            display: 'flex', alignItems: 'center', justifyContent: 'center',
          }}
        >
          <SolarIcon name={mod.icon} size={22} sx={{ color: isLight ? mod.color : 'white' }} />
        </Box>

        <Box sx={{ mt: 'auto' }}>
          <Stack direction="row" sx={{ alignItems: 'center', gap: 1, mb: 0.5 }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 700, color: isLight ? '#1e293b' : 'white', lineHeight: 1.2 }}>
              {mod.label}
            </Typography>
            {disabled && (
              <Chip label="Soon" size="small" sx={{ height: 16, fontSize: '0.6rem', bgcolor: alpha(isLight ? '#000' : '#fff', 0.10), color: alpha(isLight ? '#000' : '#fff', 0.45), border: 'none' }} />
            )}
          </Stack>
          <Typography
            variant="caption"
            sx={{ color: isLight ? alpha('#1e293b', 0.60) : alpha('#fff', 0.55), display: 'block', mb: 1.25, lineHeight: 1.45, fontSize: '0.75rem' }}
          >
            {mod.description}
          </Typography>
          {!disabled && (
            <Stack direction="row" sx={{ alignItems: 'center', gap: 0.5 }}>
              <Typography variant="caption" sx={{ color: mod.color, fontWeight: 600, fontSize: '0.75rem' }}>
                Open module
              </Typography>
              <SolarIcon name="forward" size={12} sx={{ color: mod.color }} />
            </Stack>
          )}
        </Box>
      </Box>

      {/* Pin button */}
      {!disabled && (hovered || pinned) && (
        <Tooltip title={pinned ? 'Unpin' : 'Pin to quick access'}>
          <IconButton
            size="small"
            onClick={(e) => { e.stopPropagation(); onPin(); }}
            sx={{
              position: 'absolute', top: 8, right: 8, p: 0.5,
              bgcolor: alpha(isLight ? '#fff' : '#000', 0.35),
              color: pinned ? mod.color : alpha(isLight ? '#000' : '#fff', 0.55),
              '&:hover': { bgcolor: alpha(isLight ? '#fff' : '#000', 0.55) },
            }}
          >
            <SolarIcon name={pinned ? 'complete' : 'add'} size={14} />
          </IconButton>
        </Tooltip>
      )}
    </ButtonBase>
  );
}

// ─── Module list row (light, compact) ─────────────────────────────────────────

function ModuleListRow({ mod, onOpen }: { mod: ModuleConfig; onOpen: () => void }) {
  return (
    <ButtonBase
      onClick={onOpen}
      sx={{
        display: 'flex', alignItems: 'center', gap: 2,
        p: 1.5, width: '100%', textAlign: 'left',
        borderRadius: 2,
        border: '1px solid', borderColor: 'divider',
        bgcolor: 'background.paper',
        transition: 'all 0.15s',
        '&:hover': {
          bgcolor: (t) => alpha(mod.color, t.palette.mode === 'dark' ? 0.08 : 0.04),
          borderColor: alpha(mod.color, 0.3),
        },
      }}
    >
      <Box
        sx={{
          width: 40, height: 40, borderRadius: 1.5, flexShrink: 0,
          bgcolor: alpha(mod.color, 0.12),
          display: 'flex', alignItems: 'center', justifyContent: 'center',
        }}
      >
        <SolarIcon name={mod.icon} size={20} sx={{ color: mod.color }} />
      </Box>
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography variant="body2" sx={{ fontWeight: 600 }}>{mod.label}</Typography>
        <Typography variant="caption" color="text.secondary">{mod.description}</Typography>
      </Box>
      <SolarIcon name="chevronRight" size={16} sx={{ color: 'text.disabled', flexShrink: 0 }} />
    </ButtonBase>
  );
}
