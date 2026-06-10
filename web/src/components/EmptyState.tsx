import { Box, Button, Stack, Typography } from '@mui/material';
import { alpha } from '@mui/material/styles';
import type { ReactNode } from 'react';
import SolarIcon from './SolarIcon';
import type { IconKey } from '../lib/icons';

interface EmptyStateProps {
  /** Solar icon key to display */
  icon?: IconKey;
  title: string;
  description?: string;
  /** Primary CTA button */
  action?: ReactNode;
  /** Compact mode for use inside table rows */
  compact?: boolean;
}

export default function EmptyState({
  icon = 'emptyTable',
  title,
  description,
  action,
  compact = false,
}: EmptyStateProps) {
  return (
    <Stack
      spacing={compact ? 1 : 2}
      sx={{
        alignItems: 'center',
        justifyContent: 'center',
        py: compact ? 4 : 8,
        px: 3,
        textAlign: 'center',
      }}
    >
      <Box
        sx={(theme) => ({
          width: compact ? 48 : 72,
          height: compact ? 48 : 72,
          borderRadius: '50%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: alpha(theme.palette.primary.main, 0.08),
          color: 'primary.main',
        })}
      >
        <SolarIcon name={icon} size={compact ? 24 : 36} />
      </Box>

      <Box>
        <Typography
          variant={compact ? 'subtitle2' : 'subtitle1'}
          sx={{ fontWeight: 600, mb: description ? 0.5 : 0 }}
        >
          {title}
        </Typography>
        {description && (
          <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 380, mx: 'auto' }}>
            {description}
          </Typography>
        )}
      </Box>

      {action && <Box sx={{ pt: 0.5 }}>{action}</Box>}
    </Stack>
  );
}

// ─── TableEmptyState — drop-in for DataGrid / Table no-rows overlay ──────────
interface TableEmptyStateProps {
  filtered?: boolean;
  onClear?: () => void;
}

export function TableEmptyState({ filtered = false, onClear }: TableEmptyStateProps) {
  return (
    <EmptyState
      icon={filtered ? 'emptySearch' : 'emptyTable'}
      title={filtered ? 'No results found' : 'No data yet'}
      description={
        filtered
          ? 'Try adjusting your search or filters to find what you\'re looking for.'
          : 'Records will appear here once data is available.'
      }
      action={
        filtered && onClear ? (
          <Button size="small" variant="outlined" onClick={onClear}>
            Clear filters
          </Button>
        ) : undefined
      }
      compact
    />
  );
}
