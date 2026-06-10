import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import Alert from '@mui/material/Alert';

export default function ChangePasswordPage() {
  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>Change password</Typography>
      <Alert severity="info" sx={{ mt: 2 }}>Forced password change gate — M1</Alert>
    </Box>
  );
}
