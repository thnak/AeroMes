import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import CircularProgress from '@mui/material/CircularProgress';
import { useAuth, type AuthUser } from '../../contexts/AuthContext';
import SolarIcon from '../../components/SolarIcon';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string })?.from ?? '/dashboard';

  const [loading, setLoading] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  // Dev shortcut — bypasses real auth until M1 API is wired
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    await new Promise((r) => setTimeout(r, 600));
    const mockUser: AuthUser = {
      id: 'dev-001',
      name: 'Dev User',
      email,
      roles: ['Admin'],
    };
    login(mockUser, 'mock-token-dev');
    navigate(from, { replace: true });
  };

  return (
    <Box component="form" onSubmit={handleSubmit}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 3 }}>
        <Box
          sx={{
            width: 40,
            height: 40,
            borderRadius: 2,
            bgcolor: 'primary.main',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'primary.contrastText',
          }}
        >
          <SolarIcon name="machineOn" size={22} />
        </Box>
        <Box>
          <Typography variant="h6" sx={{ lineHeight: 1.2 }}>AeroMes</Typography>
          <Typography variant="caption" color="text.secondary">Manufacturing Execution System</Typography>
        </Box>
      </Box>

      <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>Sign in</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Enter your credentials to continue
      </Typography>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField
          label="Email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          fullWidth
          autoFocus
          autoComplete="email"
        />
        <TextField
          label="Password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          fullWidth
          autoComplete="current-password"
        />
      </Box>

      <Button
        type="submit"
        variant="contained"
        fullWidth
        size="large"
        disabled={loading}
        sx={{ mt: 3, mb: 2 }}
        startIcon={loading ? <CircularProgress size={16} color="inherit" /> : undefined}
      >
        {loading ? 'Signing in…' : 'Sign in'}
      </Button>

      <Divider sx={{ my: 2 }}>
        <Typography variant="caption" color="text.secondary">or</Typography>
      </Divider>

      <Button
        variant="outlined"
        fullWidth
        startIcon={<SolarIcon name="profile" size={18} />}
        disabled={loading}
      >
        Sign in with Passkey
      </Button>
    </Box>
  );
}
