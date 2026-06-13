import { useState } from 'react';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { useTranslation } from 'react-i18next';
import { SUPPORTED_LANGUAGES, type SupportedLanguage } from '../lib/i18n';
import { useAuth } from '../contexts/AuthContext';
import { putApiV1AuthMe } from '../api/auth/auth';

export default function LanguageSwitcher() {
  const { i18n } = useTranslation();
  const { user } = useAuth();
  const [anchor, setAnchor] = useState<null | HTMLElement>(null);

  const currentLng = (i18n.language ?? 'vi') as SupportedLanguage;
  const currentLabel = SUPPORTED_LANGUAGES.find((l) => l.code === currentLng)?.label ?? currentLng;

  const handleSwitch = async (code: SupportedLanguage) => {
    setAnchor(null);
    if (code === currentLng) return;
    await i18n.changeLanguage(code);
    localStorage.setItem('i18n_lng', code);
    // Persist to user profile if authenticated
    if (user) {
      try {
        await putApiV1AuthMe({ preferredLanguage: code, fullName: null, avatarUrl: null });
      } catch {
        // non-fatal — localStorage already persists the preference
      }
    }
  };

  return (
    <>
      <Tooltip title={currentLabel}>
        <IconButton
          size="small"
          onClick={(e) => setAnchor(e.currentTarget)}
          sx={{ color: 'text.secondary', px: 0.75 }}
        >
          <Typography variant="caption" sx={{ fontWeight: 700, fontSize: '0.7rem', letterSpacing: 0.5 }}>
            {currentLng === 'vi' ? 'VI' : 'EN'}
          </Typography>
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchor}
        open={Boolean(anchor)}
        onClose={() => setAnchor(null)}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        slotProps={{ paper: { sx: { minWidth: 140, mt: 0.5 } } }}
      >
        {SUPPORTED_LANGUAGES.map((lang) => (
          <MenuItem
            key={lang.code}
            selected={lang.code === currentLng}
            onClick={() => handleSwitch(lang.code)}
          >
            {lang.label}
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
