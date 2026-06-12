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

// Module card backgrounds — dark variant
import moduleCardProduction   from './module-card-production.webp';
import moduleCardQuality      from './module-card-quality.webp';
import moduleCardMaster       from './module-card-master.webp';
import moduleCardIntegration  from './module-card-integration.webp';
import moduleCardMaintenance  from './module-card-maintenance.webp';
import moduleCardReports      from './module-card-reports.webp';
import moduleCardAdmin        from './module-card-admin.webp';
import moduleCardPlanning     from './module-card-planning.webp';
import moduleCardWarehouse    from './module-card-warehouse.webp';
import moduleCardIot          from './module-card-iot.webp';
import moduleCardLab          from './module-card-lab.webp';
import moduleCardTraceability from './module-card-traceability.webp';

// Module card backgrounds — light variant (falls back to dark when unavailable)
import moduleCardProductionLight   from './module-card-production-light.webp';
import moduleCardQualityLight      from './module-card-quality-light.webp';
import moduleCardMasterLight       from './module-card-master-light.webp';
import moduleCardIntegrationLight  from './module-card-integration-light.webp';
import moduleCardPlanningLight     from './module-card-planning-light.webp';
import moduleCardWarehouseLight    from './module-card-warehouse-light.webp';
import moduleCardReportsLight      from './module-card-reports-light.webp';
import moduleCardIotLight          from './module-card-iot-light.webp';
import moduleCardLabLight          from './module-card-lab-light.webp';
import moduleCardTraceabilityLight from './module-card-traceability-light.webp';

export const moduleCardImages: Record<string, string> = {
  production:   moduleCardProduction,
  quality:      moduleCardQuality,
  master:       moduleCardMaster,
  integration:  moduleCardIntegration,
  maintenance:  moduleCardMaintenance,
  reports:      moduleCardReports,
  admin:        moduleCardAdmin,
  planning:     moduleCardPlanning,
  warehouse:    moduleCardWarehouse,
  iot:          moduleCardIot,
  lab:          moduleCardLab,
  traceability: moduleCardTraceability,
};

export const moduleCardImagesLight: Record<string, string> = {
  ...moduleCardImages,
  production:   moduleCardProductionLight,
  quality:      moduleCardQualityLight,
  master:       moduleCardMasterLight,
  integration:  moduleCardIntegrationLight,
  planning:     moduleCardPlanningLight,
  warehouse:    moduleCardWarehouseLight,
  reports:      moduleCardReportsLight,
  iot:          moduleCardIotLight,
  lab:          moduleCardLabLight,
  traceability: moduleCardTraceabilityLight,
};

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
