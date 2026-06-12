import Box from '@mui/material/Box';
import { Outlet } from 'react-router-dom';

export default function WebLayout() {
  return (
    <Box sx={{ display: 'flex', height: '100vh', overflow: 'hidden' }}>
      <Outlet />
    </Box>
  );
}
