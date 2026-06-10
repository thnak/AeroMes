import { Chip } from '@mui/material';
import { alpha } from '@mui/material/styles';
import type { WorkOrderStatus } from '../theme/tokens';
import { statusColors } from '../theme/tokens';

const STATUS_LABELS: Record<WorkOrderStatus, string> = {
  DRAFT: 'Draft',
  RELEASED: 'Released',
  RUNNING: 'Running',
  PAUSED: 'Paused',
  COMPLETED: 'Completed',
  CLOSED: 'Closed',
  CANCELLED: 'Cancelled',
  ON_HOLD: 'On Hold',
};

interface StatusChipProps {
  status: WorkOrderStatus;
  size?: 'small' | 'medium';
}

export default function StatusChip({ status, size = 'small' }: StatusChipProps) {
  const c = statusColors[status];
  return (
    <Chip
      label={STATUS_LABELS[status]}
      size={size}
      sx={(theme) => {
        const isDark = theme.palette.mode === 'dark';
        const dc = statusColors[`${status}_DARK` as keyof typeof statusColors] as typeof c;
        const colors = isDark && dc ? dc : c;
        return {
          bgcolor: colors.bg,
          color: colors.text,
          border: `1px solid ${colors.border}`,
          fontWeight: 600,
          fontSize: '0.6875rem',
          height: size === 'small' ? 22 : 28,
          '& .MuiChip-label': { px: 1 },
        };
      }}
    />
  );
}

// Generic colored dot for machine/inline use
interface StatusDotProps {
  color: string;
  size?: number;
  pulse?: boolean;
}

export function StatusDot({ color, size = 8, pulse = false }: StatusDotProps) {
  return (
    <span
      style={{
        display: 'inline-block',
        width: size,
        height: size,
        borderRadius: '50%',
        background: color,
        flexShrink: 0,
        boxShadow: pulse ? `0 0 0 ${size * 0.5}px ${alpha(color, 0.2)}` : undefined,
        animation: pulse ? 'pulse-ring 1.6s ease-out infinite' : undefined,
      }}
    />
  );
}
