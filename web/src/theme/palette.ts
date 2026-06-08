import type { PaletteColorOptions } from '@mui/material/styles';

// Augment MUI palette types to include 'lighter' and 'darker' shades
declare module '@mui/material/styles' {
  interface PaletteColor {
    lighter: string;
    darker: string;
  }
  interface SimplePaletteColorOptions {
    lighter?: string;
    darker?: string;
  }
  interface TypeBackground {
    neutral: string;
  }
}

export const seafoamPrimary: PaletteColorOptions = {
  lighter: '#B8E1DD',
  light: '#3A9188',
  main: '#044A42',
  dark: '#06402B',
  darker: '#031514',
  contrastText: '#FFFFFF',
};

export const seafoamSecondary: PaletteColorOptions = {
  lighter: '#E6F7F5',
  light: '#B8E1DD',
  main: '#3A9188',
  dark: '#044A42',
  darker: '#062925',
  contrastText: '#FFFFFF',
};
