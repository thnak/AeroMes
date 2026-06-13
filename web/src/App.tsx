import { Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';

import WebLayout from './layouts/WebLayout';
import AuthLayout from './layouts/AuthLayout';
import ModuleLayout from './layouts/ModuleLayout';
import AuthGuard from './guards/AuthGuard';
import ErrorPage from './pages/ErrorPage';

// Auth
import LoginPage from './pages/auth/LoginPage';
const MfaPage = lazy(() => import('./pages/auth/MfaPage'));
const SetupMfaPage = lazy(() => import('./pages/auth/SetupMfaPage'));
const ChangePasswordPage = lazy(() => import('./pages/auth/ChangePasswordPage'));

// Launchpad
const LaunchpadPage = lazy(() => import('./pages/LaunchpadPage'));

// Account
const AccountPage = lazy(() => import('./pages/account/AccountPage'));

// Master data
const ProductsPage = lazy(() => import('./pages/master/ProductsPage'));
const WorkCentersPage = lazy(() => import('./pages/master/WorkCentersPage'));
const WorkCenterDetailPage = lazy(() => import('./pages/master/WorkCenterDetailPage'));
const MachinesPage = lazy(() => import('./pages/master/MachinesPage'));
const BomEditorPage = lazy(() => import('./pages/master/BomEditorPage'));
const OperationsPage = lazy(() => import('./pages/master/OperationsPage'));
const RoutingsPage = lazy(() => import('./pages/master/RoutingsPage'));
const RoutingStepsPage = lazy(() => import('./pages/master/RoutingStepsPage'));
const StorageLocationsPage = lazy(() => import('./pages/master/StorageLocationsPage'));
const DefectCodesPage = lazy(() => import('./pages/master/DefectCodesPage'));

// Integration
const SalesOrdersPage = lazy(() => import('./pages/integration/SalesOrdersPage'));
const SalesOrderDetailPage = lazy(() => import('./pages/integration/SalesOrderDetailPage'));
const ProductionOrdersPage = lazy(() => import('./pages/integration/ProductionOrdersPage'));
const ProductionOrderDetailPage = lazy(() => import('./pages/integration/ProductionOrderDetailPage'));

// Production
const WorkOrdersPage = lazy(() => import('./pages/production/WorkOrdersPage'));
const WorkOrderDetailPage = lazy(() => import('./pages/production/WorkOrderDetailPage'));
const JobsPage = lazy(() => import('./pages/production/JobsPage'));
const JobDetailPage = lazy(() => import('./pages/production/JobDetailPage'));
const DowntimePage = lazy(() => import('./pages/production/DowntimePage'));
const DowntimeDetailPage = lazy(() => import('./pages/production/DowntimeDetailPage'));
const InventoryPage = lazy(() => import('./pages/production/InventoryPage'));
const InventoryTracePage = lazy(() => import('./pages/production/InventoryTracePage'));
const SchedulePage = lazy(() => import('./pages/schedule/SchedulePage'));

// Quality
const InspectionOrdersPage = lazy(() => import('./pages/quality/InspectionOrdersPage'));
const InspectionPlanPage = lazy(() => import('./pages/quality/InspectionPlanPage'));
const NCRPage = lazy(() => import('./pages/quality/NCRPage'));
const DefectAnalysisPage = lazy(() => import('./pages/quality/DefectAnalysisPage'));

// IoT
const IotAdaptersPage          = lazy(() => import('./pages/iot/IotAdaptersPage'));
const IotSignalsPage           = lazy(() => import('./pages/iot/IotSignalsPage'));
const IotStateRulesPage        = lazy(() => import('./pages/iot/IotStateRulesPage'));
const IotTagsPage              = lazy(() => import('./pages/iot/IotTagsPage'));
const IotAdapterHealthPage     = lazy(() => import('./pages/iot/IotAdapterHealthPage'));
const MachineFleetsPage        = lazy(() => import('./pages/iot/MachineFleetsPage'));
const MachineSignalMonitorPage = lazy(() => import('./pages/iot/MachineSignalMonitorPage'));
const RuleEnginePage           = lazy(() => import('./pages/iot/RuleEnginePage'));

// Lab
const LabRequestsPage = lazy(() => import('./pages/lab/LabRequestsPage'));

// Maintenance
const MaintenancePage = lazy(() => import('./pages/maintenance/MaintenancePage'));

// Reports
const ProductionReportPage = lazy(() => import('./pages/reports/ProductionReportPage'));
const OeeReportPage = lazy(() => import('./pages/reports/OeeReportPage'));
const DowntimeReportPage = lazy(() => import('./pages/reports/DowntimeReportPage'));
const QualityReportPage = lazy(() => import('./pages/reports/QualityReportPage'));

// Admin
const SettingsPage = lazy(() => import('./pages/admin/SettingsPage'));
const UsersPage = lazy(() => import('./pages/admin/UsersPage'));
const UserDetailPage = lazy(() => import('./pages/admin/UserDetailPage'));
const RolesPage = lazy(() => import('./pages/admin/RolesPage'));
const AuditLogPage = lazy(() => import('./pages/admin/AuditLogPage'));
const ReleasePage = lazy(() => import('./pages/admin/ReleasePage'));

// Tablet (separate layout — M5)
const TabletLaunchpad = lazy(() => import('./pages/tablet/TabletLaunchpad'));
const TabletLoginPage = lazy(() => import('./pages/tablet/TabletLoginPage'));
const StationPage = lazy(() => import('./pages/tablet/StationPage'));
const StartJobPage = lazy(() => import('./pages/tablet/StartJobPage'));
const OutputPage = lazy(() => import('./pages/tablet/OutputPage'));
const DowntimeStartPage = lazy(() => import('./pages/tablet/DowntimeStartPage'));
const DowntimeActivePage = lazy(() => import('./pages/tablet/DowntimeActivePage'));
const FinishJobPage = lazy(() => import('./pages/tablet/FinishJobPage'));

function PageLoader() {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', flex: 1, minHeight: 200 }}>
      <CircularProgress size={32} thickness={3} />
    </Box>
  );
}

export default function App() {
  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* ── Auth ────────────────────────────────────────────── */}
        <Route path="/auth" element={<AuthLayout />}>
          <Route path="login" element={<LoginPage />} />
          <Route path="login/mfa" element={<MfaPage />} />
          <Route path="setup-mfa" element={<SetupMfaPage />} />
          <Route path="change-password" element={<ChangePasswordPage />} />
        </Route>

        {/* ── Tablet OI (M5) ──────────────────────────────────── */}
        <Route path="/tablet">
          <Route index element={<TabletLaunchpad />} />
          <Route path="login" element={<TabletLoginPage />} />
          <Route path="station">
            <Route index element={<StationPage />} />
            <Route path="start" element={<StartJobPage />} />
            <Route path="output" element={<OutputPage />} />
            <Route path="downtime/start" element={<DowntimeStartPage />} />
            <Route path="downtime/active" element={<DowntimeActivePage />} />
            <Route path="finish" element={<FinishJobPage />} />
          </Route>
        </Route>

        {/* ── Web app (AuthGuard) ──────────────────────────────── */}

        {/* Launchpad — standalone, no module chrome */}
        <Route
          path="/"
          element={
            <AuthGuard>
              <LaunchpadPage />
            </AuthGuard>
          }
        />

        {/* Module routes — all share ModuleLayout chrome (AppBar + tabs) */}
        <Route
          element={
            <AuthGuard>
              <WebLayout />
            </AuthGuard>
          }
        >
          {/* Legacy redirects */}
          <Route path="dashboard" element={<Navigate to="/" replace />} />
          <Route path="production/inventory" element={<Navigate to="/warehouse/inventory" replace />} />
          <Route path="master/defect-codes" element={<Navigate to="/quality/defect-catalog" replace />} />
          <Route path="reports/quality" element={<Navigate to="/quality/reports" replace />} />

          {/* Account / Profile */}
          <Route element={<ModuleLayout />}>
            <Route path="account" element={<AccountPage />} />
            <Route path="profile" element={<Navigate to="/account" replace />} />
          </Route>

          {/* ── Production module ─────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="production/schedule" element={<SchedulePage />} />
            <Route path="production/work-orders" element={<WorkOrdersPage />} />
            <Route path="production/work-orders/:id" element={<WorkOrderDetailPage />} />
            <Route path="production/jobs" element={<JobsPage />} />
            <Route path="production/jobs/:id" element={<JobDetailPage />} />
            <Route path="production/downtime" element={<DowntimePage />} />
            <Route path="production/downtime/:id" element={<DowntimeDetailPage />} />
          </Route>

          {/* ── Warehouse module ──────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="warehouse/inventory" element={<InventoryPage />} />
            <Route path="warehouse/inventory/trace" element={<InventoryTracePage />} />
          </Route>

          {/* ── Master Data module ────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="master/products" element={<ProductsPage />} />
            <Route path="master/products/:id/bom" element={<BomEditorPage />} />
            <Route path="master/work-centers" element={<WorkCentersPage />} />
            <Route path="master/work-centers/:id" element={<WorkCenterDetailPage />} />
            <Route path="master/machines" element={<MachinesPage />} />
            <Route path="master/operations" element={<OperationsPage />} />
            <Route path="master/routings" element={<RoutingsPage />} />
            <Route path="master/routings/:id/steps" element={<RoutingStepsPage />} />
            <Route path="master/storage-locations" element={<StorageLocationsPage />} />
          </Route>

          {/* ── Quality module ────────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="quality/inspection-orders" element={<InspectionOrdersPage />} />
            <Route path="quality/inspection-plans" element={<InspectionPlanPage />} />
            <Route path="quality/ncr" element={<NCRPage />} />
            <Route path="quality/defect-catalog" element={<DefectCodesPage />} />
            <Route path="quality/defect-analysis" element={<DefectAnalysisPage />} />
            <Route path="quality/reports" element={<QualityReportPage />} />
          </Route>

          {/* ── Integration module ────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="integration/sales-orders" element={<SalesOrdersPage />} />
            <Route path="integration/sales-orders/:id" element={<SalesOrderDetailPage />} />
            <Route path="integration/production-orders" element={<ProductionOrdersPage />} />
            <Route path="integration/production-orders/:id" element={<ProductionOrderDetailPage />} />
          </Route>

          {/* ── IoT module ───────────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="iot/tags"                                element={<IotTagsPage />} />
            <Route path="iot/adapter-health"                      element={<IotAdapterHealthPage />} />
            <Route path="iot/machines"                            element={<MachineFleetsPage />} />
            <Route path="iot/machines/:machineCode/signals"       element={<MachineSignalMonitorPage />} />
            <Route path="iot/machines/:machineCode/adapters"      element={<IotAdaptersPage />} />
            <Route path="iot/adapters/:adapterId/signals"         element={<IotSignalsPage />} />
            <Route path="iot/machines/:machineCode/state-rules"   element={<IotStateRulesPage />} />
            <Route path="iot/rules"                               element={<RuleEnginePage />} />
          </Route>

          {/* ── Lab module ────────────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="lab/requests" element={<LabRequestsPage />} />
          </Route>

          {/* ── Maintenance module ────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="maintenance" element={<MaintenancePage />} />
          </Route>

          {/* ── Reports module ────────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="reports/production" element={<ProductionReportPage />} />
            <Route path="reports/oee" element={<OeeReportPage />} />
            <Route path="reports/downtime" element={<DowntimeReportPage />} />
          </Route>

          {/* ── Admin module ──────────────────────────────────── */}
          <Route element={<ModuleLayout />}>
            <Route path="admin/users" element={<UsersPage />} />
            <Route path="admin/users/:id" element={<UserDetailPage />} />
            <Route path="admin/roles" element={<RolesPage />} />
            <Route path="admin/audit-log" element={<AuditLogPage />} />
            <Route path="admin/settings" element={<SettingsPage />} />
            <Route path="admin/release-notes" element={<ReleasePage />} />
          </Route>

          {/* Errors */}
          <Route path="403" element={<ErrorPage code={403} />} />
          <Route path="404" element={<ErrorPage code={404} />} />
          <Route path="500" element={<ErrorPage code={500} />} />
          <Route path="*" element={<ErrorPage code={404} />} />
        </Route>
      </Routes>
    </Suspense>
  );
}
