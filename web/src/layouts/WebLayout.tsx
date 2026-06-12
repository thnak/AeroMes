import Box from '@mui/material/Box';
import { Outlet } from 'react-router-dom';

export default function WebLayout() {
  return (
    <Box sx={{ height: '100vh', overflow: 'hidden' }}>
      <Outlet />
    </Box>
  );
}
