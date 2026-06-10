import { Box, Card, CardContent, Skeleton, Stack, Typography } from '@mui/material';
import { alpha } from '@mui/material/styles';
import SolarIcon from './SolarIcon';
import type { IconKey } from '../lib/icons';

interface KpiCardProps {
  label: string;
  value: string | number;
  unit?: string;
  icon?: IconKey;
  /** CSS color for the accent/icon tint */
  accentColor?: string;
  /** Trend percentage relative to previous period, e.g. +2.4 or -1.1 */
  trend?: number;
  trendLabel?: string;
  loading?: boolean;
}

export default function KpiCard({
  label,
  value,
  unit,
  icon,
  accentColor,
  trend,
  trendLabel,
  loading = false,
}: KpiCardProps) {
  const trendPositive = trend !== undefined && trend >= 0;

  return (
    <Card sx={{ height: '100%' }}>
      <CardContent sx={{ p: '20px !important' }}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography
              variant="caption"
              color="text.secondary"
              sx={{ textTransform: 'uppercase', letterSpacing: '0.06em', fontWeight: 600, display: 'block', mb: 0.75 }}
            >
              {label}
            </Typography>

            {loading ? (
              <Skeleton variant="text" width="60%" height={40} />
            ) : (
              <Stack direction="row" spacing={0.5} sx={{ alignItems: 'baseline' }}>
                <Typography variant="h4" sx={{ fontWeight: 700, lineHeight: 1, color: accentColor }}>
                  {value}
                </Typography>
                {unit && (
                  <Typography variant="subtitle2" color="text.secondary">
                    {unit}
                  </Typography>
                )}
              </Stack>
            )}

            {trend !== undefined && !loading && (
              <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', mt: 1 }}>
                <Typography
                  variant="caption"
                  sx={{
                    fontWeight: 700,
                    color: trendPositive ? 'success.main' : 'error.main',
                  }}
                >
                  {trendPositive ? '+' : ''}{trend}%
                </Typography>
                {trendLabel && (
                  <Typography variant="caption" color="text.disabled">
                    {trendLabel}
                  </Typography>
                )}
              </Stack>
            )}
          </Box>

          {icon && (
            <Box
              sx={{
                p: 1.25,
                borderRadius: 2,
                bgcolor: accentColor ? alpha(accentColor, 0.1) : 'action.hover',
                color: accentColor ?? 'text.secondary',
                flexShrink: 0,
              }}
            >
              <SolarIcon name={icon} size={24} />
            </Box>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
}
