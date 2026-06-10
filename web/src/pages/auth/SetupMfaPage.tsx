import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import Alert from '@mui/material/Alert';

export default function SetupMfaPage() {
  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>Set up two-factor authentication</Typography>
      <Alert severity="info" sx={{ mt: 2 }}>TOTP QR code setup — M1</Alert>
    </Box>
  );
}
