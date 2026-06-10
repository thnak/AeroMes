// Illustration asset registry
// PNG/WEBP files live alongside this file.
// Prompts for AI image generation are in ./prompts/*.txt

// Auth
import authHero from './auth-hero.webp';

// Dashboard
import dashboardAmbientBg from './dashboard-ambient-bg.webp';

// Empty states
import emptyNoData from './empty-no-data.webp';
import emptyNoResults from './empty-no-results.webp';
import emptyNoOrders from './empty-no-orders.webp';

// Errors
import error403 from './error-403.webp';
import error404 from './error-404.webp';
import error500 from './error-500.webp';

// Tablet
import tabletIdleWelcome from './tablet-idle-welcome.webp';

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
