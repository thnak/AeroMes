import { Icon } from '@iconify/react';
import type { SxProps, Theme } from '@mui/material/styles';
import { Box } from '@mui/material';
import type { IconKey } from '../lib/icons';
import { Icons } from '../lib/icons';

interface SolarIconProps {
  /** Key from the Icons constant map */
  name: IconKey;
  /** Pixel size (width = height). Default: 20 */
  size?: number;
  /** MUI color token or raw CSS color, e.g. "primary.main", "#3A9188" */
  color?: string;
  sx?: SxProps<Theme>;
}

export default function SolarIcon({ name, size = 20, color, sx }: SolarIconProps) {
  return (
    <Box
      component="span"
      sx={{
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
        flexShrink: 0,
        color: color ?? 'inherit',
        ...sx,
      }}
    >
      <Icon icon={Icons[name]} width={size} height={size} />
    </Box>
  );
}
