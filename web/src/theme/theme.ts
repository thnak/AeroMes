import { createTheme } from '@mui/material/styles';
import { seafoamPrimary, seafoamSecondary } from './palette';

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
          neutral: '#0E2A27',
        },
      },
    },
  },
  typography: {
    fontFamily: ['"Inter"', '"Segoe UI"', 'sans-serif'].join(','),
    h4: { fontWeight: 700 },
    h5: { fontWeight: 600 },
    h6: { fontWeight: 600 },
  },
  shape: { borderRadius: 10 },
  components: {
    MuiButton: {
      styleOverrides: {
        root: { textTransform: 'none', fontWeight: 600 },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: { boxShadow: '0 1px 4px 0 rgba(4,74,66,0.08)' },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: { fontWeight: 500 },
      },
    },
  },
});

export default theme;
