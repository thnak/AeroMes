import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { alpha } from '@mui/material/styles';
import PageHeader, { PageRoot } from './PageHeader';
import SolarIcon from './SolarIcon';
import type { IconKey } from '../lib/icons';

interface StubPageProps {
  title: string;
  subtitle?: string;
  icon?: IconKey;
  milestone?: string;
}

export default function StubPage({ title, subtitle, icon = 'production', milestone }: StubPageProps) {
  return (
    <PageRoot>
      <PageHeader title={title} subtitle={subtitle} />
      <Box
        sx={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: 320,
          gap: 2,
          borderRadius: 3,
          border: '1.5px dashed',
          borderColor: 'divider',
          bgcolor: (t) => alpha(t.palette.primary.main, 0.02),
        }}
      >
        <Box
          sx={{
            width: 64,
            height: 64,
            borderRadius: 3,
            bgcolor: (t) => alpha(t.palette.primary.main, 0.08),
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'primary.main',
          }}
        >
          <SolarIcon name={icon} size={32} />
        </Box>
        <Box sx={{ textAlign: 'center' }}>
          <Typography variant="subtitle1" color="text.secondary">
            TODO: wire API
          </Typography>
          {milestone && (
            <Typography variant="caption" color="text.disabled">
              Planned for {milestone}
            </Typography>
          )}
        </Box>
      </Box>
    </PageRoot>
  );
}
