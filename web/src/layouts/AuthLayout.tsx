import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import { alpha } from '@mui/material/styles';
import { Outlet } from 'react-router-dom';

export default function AuthLayout() {
  return (
    <Box
      sx={{
        minHeight: '100svh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: (t) =>
          `radial-gradient(ellipse at 30% 40%, ${alpha(t.palette.primary.main, 0.12)} 0%, transparent 60%),
           radial-gradient(ellipse at 70% 70%, ${alpha(t.palette.secondary.main, 0.08)} 0%, transparent 50%),
           ${t.palette.background.default}`,
        p: 2,
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
  );
}
