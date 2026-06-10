// Illustration asset registry
// PNG/WEBP files live alongside this file.
// Prompts for AI image generation are in ./prompts/*.txt

// Auth
import authHero from './auth-hero.png';

// Dashboard
import dashboardAmbientBg from './dashboard-ambient-bg.png';

// Empty states
import emptyNoData from './empty-no-data.png';
import emptyNoResults from './empty-no-results.png';
import emptyNoOrders from './empty-no-orders.png';

// Errors
import error403 from './error-403.png';
import error404 from './error-404.png';
import error500 from './error-500.png';

// Tablet
import tabletIdleWelcome from './tablet-idle-welcome.png';

export const illustrations = {
  authHero,
  dashboardAmbientBg,
  empty: {
    noData: emptyNoData,
    noResults: emptyNoResults,
    noOrders: emptyNoOrders,
  },
  error: {
    forbidden: error403,
    notFound: error404,
    serverError: error500,
  },
  tablet: {
    idleWelcome: tabletIdleWelcome,
  },
} as const;

export type IllustrationKey = keyof typeof illustrations;
