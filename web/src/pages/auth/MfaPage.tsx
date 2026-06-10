import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';

export default function MfaPage() {
  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>Two-factor verification</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Enter the 6-digit code from your authenticator app or email.
      </Typography>
      <TextField label="Verification code" fullWidth slotProps={{ htmlInput: { maxLength: 6 } }} />
      <Button variant="contained" fullWidth sx={{ mt: 2 }}>Verify</Button>
    </Box>
  );
}
