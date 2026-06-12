import { Box, Button, Card, CardContent, CircularProgress, TextField, Typography } from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export default function TabletLoginPage() {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [pin, setPin] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSignIn = () => {
    setLoading(true);
    setTimeout(() => {
      navigate('/tablet/station');
      setLoading(false);
    }, 1000);
  };

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        bgcolor: 'background.default',
      }}
    >
      <Card sx={{ width: 400, p: 1 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1.5 }}>
            <Box
              sx={{
                width: 36,
                height: 36,
                borderRadius: 1.5,
                bgcolor: 'primary.main',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              <Typography sx={{ color: 'white', fontWeight: 700, fontSize: '0.85rem' }}>AM</Typography>
            </Box>
            <Typography variant="h5" sx={{ fontWeight: 700 }}>
              AeroMes
            </Typography>
          </Box>

          <Typography variant="subtitle1" color="text.secondary" sx={{ textAlign: 'center', mt: 2, mb: 3 }}>
            Operator Login
          </Typography>

          <TextField
            label="Badge ID / Username"
            fullWidth
            autoFocus
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            label="PIN"
            type="password"
            fullWidth
            value={pin}
            onChange={(e) => setPin(e.target.value)}
            sx={{ mb: 3 }}
            onKeyDown={(e) => e.key === 'Enter' && handleSignIn()}
          />

          <Button
            variant="contained"
            fullWidth
            size="large"
            sx={{ minHeight: 52 }}
            disabled={loading}
            onClick={handleSignIn}
          >
            {loading ? <CircularProgress size={22} color="inherit" /> : 'Sign In'}
          </Button>

          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ display: 'block', textAlign: 'center', mt: 2 }}
          >
            Scan badge or enter credentials
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
