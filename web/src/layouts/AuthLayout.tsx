import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import { alpha } from '@mui/material/styles';
import { Outlet } from 'react-router-dom';
import { illustrations } from '../assets/illustrations';

export default function AuthLayout() {
  return (
    <Box
      sx={{
        minHeight: '100svh',
        display: 'flex',
        alignItems: 'stretch',
        background: (t) =>
          `radial-gradient(ellipse at 30% 40%, ${alpha(t.palette.primary.main, 0.12)} 0%, transparent 60%),
           radial-gradient(ellipse at 70% 70%, ${alpha(t.palette.secondary.main, 0.08)} 0%, transparent 50%),
           ${t.palette.background.default}`,
      }}
    >
      {/* Hero panel — visible on md+ */}
      <Box
        sx={{
          display: { xs: 'none', md: 'flex' },
          flex: 1,
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'flex-end',
          overflow: 'hidden',
          bgcolor: 'primary.dark',
          position: 'relative',
          minHeight: '100svh',
        }}
      >
        <Box
          component="img"
          src={illustrations.authHero}
          alt="AeroMes manufacturing floor"
          sx={{
            position: 'absolute',
            inset: 0,
            width: '100%',
            height: '100%',
            objectFit: 'cover',
            objectPosition: 'center',
          }}
        />
        {/* gradient overlay for readability */}
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            background: 'linear-gradient(to top, rgba(3,21,20,0.85) 0%, rgba(3,21,20,0.2) 60%, transparent 100%)',
          }}
        />
        <Box sx={{ position: 'relative', p: 5, zIndex: 1, width: '100%' }}>
          <Typography variant="h4" sx={{ color: '#fff', fontWeight: 700, mb: 1 }}>
            AeroMes
          </Typography>
          <Typography variant="body1" sx={{ color: 'rgba(255,255,255,0.72)', maxWidth: 360 }}>
            Manufacturing Execution System — real-time visibility from order to shipment.
          </Typography>
        </Box>
      </Box>

      {/* Form panel */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          width: { xs: '100%', md: 480 },
          flexShrink: 0,
          p: { xs: 2, sm: 4 },
        }}
      >
        <Paper
          elevation={0}
          sx={{
            width: '100%',
            maxWidth: 440,
            p: { xs: 3, sm: 4 },
            borderRadius: 3,
            border: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Outlet />
        </Paper>
      </Box>
    </Box>
  );
}
