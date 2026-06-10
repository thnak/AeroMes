import { createTheme, alpha } from '@mui/material/styles';
import { seafoamPrimary, seafoamSecondary } from './palette';
import { APPBAR_HEIGHT } from './tokens';

const theme = createTheme({
  cssVariables: {
    colorSchemeSelector: 'data-color-scheme',
  },
  colorSchemes: {
    light: {
      palette: {
        primary: seafoamPrimary,
        secondary: seafoamSecondary,
        background: {
          default: '#F4FAF9',
          paper: '#FFFFFF',
          neutral: '#E6F7F5',
        },
        success: { main: '#15803D', light: '#86EFAC', dark: '#14532D', contrastText: '#fff' },
        warning: { main: '#B45309', light: '#FCD34D', dark: '#92400E', contrastText: '#fff' },
        error:   { main: '#B91C1C', light: '#FCA5A5', dark: '#7F1D1D', contrastText: '#fff' },
        info:    { main: '#1D4ED8', light: '#93C5FD', dark: '#1E3A8A', contrastText: '#fff' },
      },
    },
    dark: {
      palette: {
        primary: {
          ...seafoamPrimary,
          main: '#3A9188',
          light: '#B8E1DD',
          dark: '#044A42',
          contrastText: '#FFFFFF',
        },
        secondary: {
          ...seafoamSecondary,
          main: '#B8E1DD',
          light: '#E6F7F5',
          dark: '#3A9188',
        },
        background: {
          default: '#071A18',
          paper: '#0E2A27',
          neutral: '#122E2B',
        },
        success: { main: '#22C55E', light: '#86EFAC', dark: '#15803D', contrastText: '#fff' },
        warning: { main: '#F59E0B', light: '#FDE68A', dark: '#B45309', contrastText: '#fff' },
        error:   { main: '#EF4444', light: '#FCA5A5', dark: '#B91C1C', contrastText: '#fff' },
        info:    { main: '#60A5FA', light: '#BFDBFE', dark: '#1D4ED8', contrastText: '#fff' },
      },
    },
  },
  typography: {
    fontFamily: ['"Inter"', '"Segoe UI"', 'sans-serif'].join(','),
    h1: { fontWeight: 700, letterSpacing: '-0.02em' },
    h2: { fontWeight: 700, letterSpacing: '-0.01em' },
    h3: { fontWeight: 700, letterSpacing: '-0.01em' },
    h4: { fontWeight: 700, letterSpacing: '-0.01em' },
    h5: { fontWeight: 600 },
    h6: { fontWeight: 600 },
    subtitle1: { fontWeight: 600 },
    subtitle2: { fontWeight: 600, fontSize: '0.8125rem' },
    caption: { fontSize: '0.75rem', letterSpacing: '0.02em' },
    overline: { fontWeight: 700, letterSpacing: '0.1em', fontSize: '0.6875rem' },
  },
  shape: { borderRadius: 10 },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: { margin: 0, padding: 0 },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          height: APPBAR_HEIGHT,
          justifyContent: 'center',
          boxShadow: 'none',
          borderBottom: '1px solid',
          borderColor: 'divider',
          backdropFilter: 'blur(8px)',
        },
      },
    },
    MuiToolbar: {
      styleOverrides: {
        root: {
          minHeight: `${APPBAR_HEIGHT}px !important`,
          paddingLeft: '16px !important',
          paddingRight: '16px !important',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
          borderRadius: 8,
        },
        sizeSmall: { fontSize: '0.8125rem', padding: '4px 12px' },
        sizeMedium: { padding: '6px 16px' },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: ({ theme }) => ({
          boxShadow: '0 1px 4px 0 rgba(4,74,66,0.07), 0 0 0 1px rgba(4,74,66,0.06)',
          ...theme.applyStyles('dark', {
            boxShadow: '0 1px 4px 0 rgba(0,0,0,0.3), 0 0 0 1px rgba(58,145,136,0.12)',
          }),
        }),
      },
    },
    MuiChip: {
      styleOverrides: {
        root: { fontWeight: 600, fontSize: '0.75rem' },
        sizeSmall: { height: 22 },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        head: { fontWeight: 600, fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.06em' },
      },
    },
    MuiTextField: {
      defaultProps: { size: 'small' },
    },
    MuiSelect: {
      defaultProps: { size: 'small' },
    },
    MuiTooltip: {
      styleOverrides: {
        tooltip: { fontSize: '0.75rem' },
      },
    },
    MuiListItemButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          margin: '1px 8px',
          width: 'calc(100% - 16px)',
        },
      },
    },
    MuiDivider: {
      styleOverrides: {
        root: ({ theme }) => ({
          borderColor: alpha(theme.palette.primary.main, 0.08),
          ...theme.applyStyles('dark', {
            borderColor: alpha(theme.palette.primary.light, 0.1),
          }),
        }),
      },
    },
  },
});

export default theme;
