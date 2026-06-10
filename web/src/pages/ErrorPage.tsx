import { Box, Button, Stack, Typography } from '@mui/material';
import { alpha } from '@mui/material/styles';
import { Icon } from '@iconify/react';
import { useNavigate } from 'react-router-dom';
import SolarIcon from '../components/SolarIcon';
import { illustrations } from '../assets/illustrations';

type ErrorType = 403 | 404 | 500;

interface ErrorConfig {
  icon: string;
  title: string;
  description: string;
  iconColor: string;
  illustration: string;
}

const configs: Record<ErrorType, ErrorConfig> = {
  403: {
    icon: 'solar:lock-keyhole-bold-duotone',
    title: 'Access Denied',
    description: 'You don\'t have permission to view this page. Contact your administrator if you believe this is a mistake.',
    iconColor: '#B45309',
    illustration: illustrations.error.forbidden,
  },
  404: {
    icon: 'solar:ghost-bold-duotone',
    title: 'Page Not Found',
    description: 'The page you\'re looking for doesn\'t exist or has been moved.',
    iconColor: '#1D4ED8',
    illustration: illustrations.error.notFound,
  },
  500: {
    icon: 'solar:server-broken-bold-duotone',
    title: 'Server Error',
    description: 'Something went wrong on our end. Our team has been notified. Please try again in a moment.',
    iconColor: '#B91C1C',
    illustration: illustrations.error.serverError,
  },
};

interface ErrorPageProps {
  code?: ErrorType;
}

export default function ErrorPage({ code = 404 }: ErrorPageProps) {
  const navigate = useNavigate();
  const config = configs[code];

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        flex: 1,
        minHeight: '60vh',
        p: 4,
      }}
    >
      <Stack spacing={3} sx={{ alignItems: 'center', maxWidth: 420, textAlign: 'center' }}>
        {/* Illustration */}
        <Box
          component="img"
          src={config.illustration}
          alt={config.title}
          sx={{ width: 260, height: 'auto', objectFit: 'contain' }}
        />

        {/* Code badge */}
        <Typography
          variant="overline"
          sx={{
            fontSize: '0.6875rem',
            fontWeight: 700,
            letterSpacing: '0.12em',
            color: config.iconColor,
            bgcolor: alpha(config.iconColor, 0.08),
            px: 1.5,
            py: 0.5,
            borderRadius: 1,
            display: 'flex',
            alignItems: 'center',
            gap: 0.75,
          }}
        >
          <Icon icon={config.icon} width={14} height={14} />
          Error {code}
        </Typography>

        <Box>
          <Typography variant="h5" gutterBottom>
            {config.title}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {config.description}
          </Typography>
        </Box>

        <Stack direction="row" spacing={1.5}>
          <Button
            variant="outlined"
            startIcon={<SolarIcon name="back" size={16} />}
            onClick={() => navigate(-1)}
          >
            Go back
          </Button>
          <Button
            variant="contained"
            startIcon={<SolarIcon name="dashboard" size={16} />}
            onClick={() => navigate('/dashboard')}
          >
            Dashboard
          </Button>
        </Stack>
      </Stack>
    </Box>
  );
}
