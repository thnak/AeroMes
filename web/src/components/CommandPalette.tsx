import {
  alpha,
  Box,
  Chip,
  CircularProgress,
  Dialog,
  Divider,
  InputAdornment,
  InputBase,
  List,
  ListItemButton,
  ListItemText,
  Typography,
} from '@mui/material';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../lib/apiClient';

interface SearchResultDto {
  id: string;
  entityType: string;
  code: string;
  title: string;
  subtitle?: string;
  highlight?: string;
}
import type { IconKey } from '../lib/icons';
import SolarIcon from './SolarIcon';

const MAX_RECENT = 5;
const RECENT_KEY = 'aeromes_recent_searches';

function getEntityIcon(type: string): IconKey {
  switch (type) {
    case 'product': return 'products';
    case 'customer': return 'masterData';
    case 'employee': return 'admin';
    case 'work_order': return 'workOrders';
    case 'production_order': return 'production';
    default: return 'search';
  }
}

function getEntityPath(type: string, id: string): string | null {
  switch (type) {
    case 'product': return `/master/products/${id}`;
    case 'customer': return `/master/customers/${id}`;
    case 'employee': return `/master/employees/${id}`;
    case 'work_order': return `/production/work-orders/${id}`;
    case 'production_order': return `/integration/production-orders/${id}`;
    default: return null;
  }
}

function loadRecent(): string[] {
  try { return JSON.parse(localStorage.getItem(RECENT_KEY) ?? '[]'); }
  catch { return []; }
}

function saveRecent(q: string) {
  const prev = loadRecent().filter((x) => x !== q);
  localStorage.setItem(RECENT_KEY, JSON.stringify([q, ...prev].slice(0, MAX_RECENT)));
}

interface CommandPaletteProps {
  open: boolean;
  onClose: () => void;
}

export default function CommandPalette({ open, onClose }: CommandPaletteProps) {
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [debouncedQuery, setDebouncedQuery] = useState('');
  const [results, setResults] = useState<SearchResultDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [activeIdx, setActiveIdx] = useState(0);
  const recent = useMemo(() => (open ? loadRecent() : []), [open]);

  useEffect(() => {
    const t = setTimeout(() => setDebouncedQuery(query), 280);
    return () => clearTimeout(t);
  }, [query]);

  useEffect(() => {
    if (!debouncedQuery.trim()) { setResults([]); return; }
    let cancelled = false;
    setLoading(true);
    api
      .get<{ results: SearchResultDto[]; totalHits: number }>('/api/v1/search', {
        q: debouncedQuery, page: 1, pageSize: 12,
      })
      .then((data) => { if (!cancelled) { setResults(data.results ?? []); setActiveIdx(0); } })
      .catch(() => { if (!cancelled) setResults([]); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [debouncedQuery]);

  const handleClose = useCallback(() => {
    setQuery(''); setResults([]); onClose();
  }, [onClose]);

  function navigateTo(path: string | null, label: string) {
    if (!path) return;
    saveRecent(query || label);
    handleClose();
    navigate(path);
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    const total = results.length;
    if (total === 0) return;
    if (e.key === 'ArrowDown') { e.preventDefault(); setActiveIdx((i) => (i + 1) % total); }
    else if (e.key === 'ArrowUp') { e.preventDefault(); setActiveIdx((i) => (i - 1 + total) % total); }
    else if (e.key === 'Enter') {
      const hit = results[activeIdx];
      if (hit) navigateTo(getEntityPath(hit.entityType, hit.id), hit.title);
    }
  }

  const grouped = useMemo(() => {
    const map = new Map<string, SearchResultDto[]>();
    for (const r of results) { const list = map.get(r.entityType) ?? []; list.push(r); map.set(r.entityType, list); }
    return [...map.entries()];
  }, [results]);

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      slotProps={{
        paper: {
          sx: {
            width: 600, maxWidth: '95vw', maxHeight: '70vh', borderRadius: 3,
            overflow: 'hidden', display: 'flex', flexDirection: 'column',
            m: 0, position: 'fixed', top: '12vh',
          },
        },
        backdrop: { sx: { backdropFilter: 'blur(2px)' } },
      }}
    >
      {/* Search input */}
      <Box sx={(theme) => ({
        display: 'flex', alignItems: 'center', gap: 1.5, px: 2, py: 1.5,
        borderBottom: `1px solid ${theme.palette.divider}`,
        bgcolor: alpha(theme.palette.background.paper, 0.98),
      })}>
        <SolarIcon name="search" size={20} sx={{ color: 'text.disabled', flexShrink: 0 }} />
        <InputBase
          placeholder="Search products, orders, customers…"
          fullWidth autoFocus
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          sx={{ fontSize: '1rem' }}
          endAdornment={loading
            ? <InputAdornment position="end"><CircularProgress size={16} /></InputAdornment>
            : null}
        />
        <Chip label="Esc" size="small" sx={{ fontSize: '0.7rem', height: 20 }} onClick={handleClose} />
      </Box>

      {/* Results / recent */}
      <Box sx={{ overflowY: 'auto', flex: 1 }}>
        {!query && recent.length > 0 && (
          <>
            <Typography variant="caption" sx={{ px: 2, pt: 1.5, pb: 0.5, display: 'block', color: 'text.disabled', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.06em' }}>
              Recent
            </Typography>
            <List dense disablePadding>
              {recent.map((r) => (
                <ListItemButton key={r} onClick={() => setQuery(r)} sx={{ px: 2, py: 0.75 }}>
                  <SolarIcon name="close" size={14} sx={{ color: 'text.disabled', mr: 1.5 }} />
                  <ListItemText primary={r} slotProps={{ primary: { sx: { fontSize: '0.875rem' } } }} />
                </ListItemButton>
              ))}
            </List>
          </>
        )}

        {query && !loading && results.length === 0 && (
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 6, gap: 1 }}>
            <SolarIcon name="search" size={32} sx={{ color: 'text.disabled' }} />
            <Typography variant="body2" color="text.disabled">No results for "{query}"</Typography>
          </Box>
        )}

        {grouped.map(([type, items], gIdx) => (
          <Box key={type}>
            {gIdx > 0 && <Divider />}
            <Typography variant="caption" sx={{ px: 2, pt: 1.5, pb: 0.5, display: 'block', color: 'text.disabled', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.06em' }}>
              {type.replace(/_/g, ' ')}
            </Typography>
            <List dense disablePadding>
              {items.map((hit) => {
                const globalIdx = results.indexOf(hit);
                const path = getEntityPath(hit.entityType, hit.id);
                return (
                  <ListItemButton
                    key={hit.id}
                    selected={globalIdx === activeIdx}
                    onClick={() => navigateTo(path, hit.title)}
                    sx={{ px: 2, py: 0.75 }}
                  >
                    <SolarIcon name={getEntityIcon(hit.entityType)} size={18} sx={{ color: 'primary.main', mr: 1.5, flexShrink: 0 }} />
                    <ListItemText
                      primary={hit.title}
                      secondary={hit.subtitle ?? hit.code}
                      slotProps={{
                        primary: { sx: { fontSize: '0.875rem', fontWeight: 500 } },
                        secondary: { sx: { fontSize: '0.75rem' } },
                      }}
                    />
                    {hit.code && hit.code !== hit.title && (
                      <Chip label={hit.code} size="small" sx={{ ml: 1, fontSize: '0.7rem', height: 20 }} />
                    )}
                  </ListItemButton>
                );
              })}
            </List>
          </Box>
        ))}
      </Box>

      {/* Footer keyboard hints */}
      <Box sx={(theme) => ({
        display: 'flex', gap: 2, px: 2, py: 1,
        borderTop: `1px solid ${theme.palette.divider}`,
        bgcolor: alpha(theme.palette.background.default, 0.8),
      })}>
        {([['↑↓', 'navigate'], ['↵', 'open'], ['esc', 'close']] as const).map(([key, action]) => (
          <Box key={key} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Chip label={key} size="small" sx={{ fontSize: '0.65rem', height: 18, fontFamily: 'monospace' }} />
            <Typography variant="caption" color="text.disabled">{action}</Typography>
          </Box>
        ))}
      </Box>
    </Dialog>
  );
}
