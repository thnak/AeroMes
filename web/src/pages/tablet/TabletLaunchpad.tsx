import { Box, ButtonBase, Chip, Stack, Typography } from '@mui/material';
import { alpha } from '@mui/material/styles';
import { keyframes } from '@mui/system';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import SolarIcon from '../../components/SolarIcon';
import type { IconKey } from '../../lib/icons';

// ─── Language strings ─────────────────────────────────────────────────────────

const LANG_KEY = 'aeromes:tablet-lang';

const STRINGS = {
  en: {
    myJobs:          'My Jobs',
    myJobsSub:       'Tap to select a station',
    checksheet:      'Checksheet',
    checksheetSub:   'No active job',
    downtime:        'Downtime',
    downtimeSub:     'Report an issue',
    myOutput:        'My Output',
    myOutputSub:     'Today: — pcs',
    active:          'Active',
    noStation:       'No station',
  },
  vi: {
    myJobs:          'Công việc của tôi',
    myJobsSub:       'Nhấn để chọn trạm',
    checksheet:      'Phiếu kiểm tra',
    checksheetSub:   'Không có công việc',
    downtime:        'Sự cố',
    downtimeSub:     'Báo cáo sự cố',
    myOutput:        'Sản lượng',
    myOutputSub:     'Hôm nay: — cái',
    active:          'Đang chạy',
    noStation:       'Chưa chọn trạm',
  },
} as const;

type Lang = keyof typeof STRINGS;

// ─── Pulse animation for active downtime ──────────────────────────────────────

const pulse = keyframes`
  0%, 100% { box-shadow: 0 0 0 0 rgba(220, 38, 38, 0.4); }
  50%       { box-shadow: 0 0 0 8px rgba(220, 38, 38, 0); }
`;

// ─── Tile definition ──────────────────────────────────────────────────────────

interface TileDef {
  id:      string;
  icon:    IconKey;
  color:   string;
  route:   string;
  labelKey:  keyof typeof STRINGS['en'];
  subKey:    keyof typeof STRINGS['en'];
  pulse?:  boolean;
  badge?:  { count: number; color: 'success' | 'warning' | 'error' };
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function TabletLaunchpad() {
  const navigate = useNavigate();

  const [lang, setLang] = useState<Lang>(() => {
    const stored = localStorage.getItem(LANG_KEY);
    return (stored === 'vi' || stored === 'en') ? stored : 'en';
  });

  const [time, setTime] = useState(() => new Date());

  useEffect(() => {
    const id = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(id);
  }, []);

  function toggleLang() {
    const next: Lang = lang === 'en' ? 'vi' : 'en';
    setLang(next);
    localStorage.setItem(LANG_KEY, next);
  }

  const t = STRINGS[lang];

  const HH = time.getHours().toString().padStart(2, '0');
  const MM = time.getMinutes().toString().padStart(2, '0');

  // Tile definitions — sub-labels and badge are placeholders for live API wiring
  const TILES: TileDef[] = [
    { id: 'jobs',       icon: 'jobs',       color: '#1D4ED8', route: '/tablet/station',               labelKey: 'myJobs',     subKey: 'myJobsSub'     },
    { id: 'checksheet', icon: 'complete',   color: '#15803D', route: '/tablet/station/checksheet',    labelKey: 'checksheet', subKey: 'checksheetSub' },
    { id: 'downtime',   icon: 'machineDown',color: '#DC2626', route: '/tablet/station/downtime/start',labelKey: 'downtime',   subKey: 'downtimeSub',  pulse: false },
    { id: 'output',     icon: 'quantity',   color: '#D97706', route: '/tablet/station/output',         labelKey: 'myOutput',  subKey: 'myOutputSub'   },
  ];

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        height: '100dvh',
        bgcolor: '#0F172A',
        overflow: 'hidden',
        userSelect: 'none',
      }}
    >
      {/* ── Header bar ─────────────────────────────────────────────────────── */}
      <Stack
        direction="row"
        sx={{
          alignItems: 'center',
          px: 3,
          height: 56,
          borderBottom: '1px solid rgba(255,255,255,0.08)',
          flexShrink: 0,
        }}
      >
        {/* Brand */}
        <Stack direction="row" sx={{ alignItems: 'center', gap: 1, flexShrink: 0 }}>
          <Box sx={{ width: 28, height: 28, borderRadius: 1.5, bgcolor: 'primary.main', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <SolarIcon name="production" size={16} sx={{ color: '#fff' }} />
          </Box>
          <Typography variant="subtitle1" sx={{ fontWeight: 700, color: '#fff', fontSize: '1rem' }}>
            AeroMes
          </Typography>
        </Stack>

        <Box sx={{ flex: 1 }} />

        {/* Work center label — placeholder; wire to tablet session context */}
        <Typography variant="body2" sx={{ color: 'rgba(255,255,255,0.5)', mr: 2, fontSize: '0.8125rem' }}>
          Shop Floor
        </Typography>

        {/* Clock */}
        <Typography
          variant="h6"
          sx={{ fontFamily: 'monospace', fontWeight: 700, color: '#fff', fontSize: '1.25rem', letterSpacing: 2, mr: 2, minWidth: 70 }}
        >
          {HH}:{MM}
        </Typography>

        {/* Language toggle */}
        <ButtonBase
          onClick={toggleLang}
          sx={{
            px: 1.5, py: 0.5, borderRadius: 1.5,
            border: '1px solid rgba(255,255,255,0.2)',
            color: 'rgba(255,255,255,0.7)',
            fontSize: '0.8125rem', fontWeight: 700,
            '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' },
            '&:active': { transform: 'scale(0.96)' },
          }}
        >
          {lang.toUpperCase()}
        </ButtonBase>
      </Stack>

      {/* ── Tile grid ──────────────────────────────────────────────────────── */}
      <Box
        sx={{
          flex: 1,
          display: 'grid',
          gridTemplateColumns: 'repeat(2, 1fr)',
          gridTemplateRows:    'repeat(2, 1fr)',
          gap: { xs: 1.5, sm: 2.5 },
          p:   { xs: 1.5, sm: 3 },
          minHeight: 0,
        }}
      >
        {TILES.map((tile) => (
          <Tile
            key={tile.id}
            tile={tile}
            label={t[tile.labelKey]}
            sublabel={t[tile.subKey]}
            onTap={() => navigate(tile.route)}
          />
        ))}
      </Box>
    </Box>
  );
}

// ─── Tile component ───────────────────────────────────────────────────────────

interface TileProps {
  tile:     TileDef;
  label:    string;
  sublabel: string;
  onTap:    () => void;
}

function Tile({ tile, label, sublabel, onTap }: TileProps) {
  const [pressed, setPressed] = useState(false);

  return (
    <ButtonBase
      onPointerDown={() => setPressed(true)}
      onPointerUp={() => { setPressed(false); onTap(); }}
      onPointerLeave={() => setPressed(false)}
      onContextMenu={(e) => e.preventDefault()}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 1.5,
        borderRadius: 3,
        bgcolor: alpha(tile.color, 0.1),
        border: `2px solid ${alpha(tile.color, tile.pulse ? 0.7 : 0.3)}`,
        transition: 'transform 120ms ease, background-color 120ms ease, box-shadow 120ms ease',
        transform: pressed ? 'scale(0.97)' : 'scale(1)',
        boxShadow: pressed ? 'none' : `0 4px 24px ${alpha(tile.color, 0.15)}`,
        animation: tile.pulse ? `${pulse} 1.5s ease-in-out infinite` : 'none',
        cursor: 'pointer',
        WebkitTapHighlightColor: 'transparent',
        position: 'relative',
        overflow: 'hidden',
        minHeight: { xs: 140, sm: 160 },
      }}
    >
      {/* Badge */}
      {tile.badge && tile.badge.count > 0 && (
        <Chip
          label={tile.badge.count}
          size="small"
          sx={{
            position: 'absolute', top: 12, right: 12,
            bgcolor: tile.badge.color === 'error' ? '#DC2626' : tile.badge.color === 'warning' ? '#D97706' : '#15803D',
            color: '#fff', fontWeight: 700, height: 22, fontSize: '0.75rem',
            '& .MuiChip-label': { px: 1 },
          }}
        />
      )}

      {/* Icon */}
      <Box sx={{
        width: 64, height: 64,
        borderRadius: '50%',
        bgcolor: alpha(tile.color, 0.15),
        display: 'flex', alignItems: 'center', justifyContent: 'center',
      }}>
        <SolarIcon name={tile.icon} size={32} sx={{ color: tile.color }} />
      </Box>

      {/* Labels */}
      <Stack sx={{ alignItems: 'center', gap: 0.5, px: 2 }}>
        <Typography
          variant="h6"
          sx={{ fontWeight: 700, color: '#fff', fontSize: { xs: '1rem', sm: '1.125rem' }, textAlign: 'center' }}
        >
          {label}
        </Typography>
        <Typography
          variant="body2"
          sx={{ color: 'rgba(255,255,255,0.5)', fontSize: { xs: '0.75rem', sm: '0.8125rem' }, textAlign: 'center' }}
        >
          {sublabel}
        </Typography>
      </Stack>
    </ButtonBase>
  );
}
