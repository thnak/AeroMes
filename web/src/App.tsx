import { Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';

import WebLayout from './layouts/WebLayout';
import AuthLayout from './layouts/AuthLayout';
import AuthGuard from './guards/AuthGuard';
import ErrorPage from './pages/ErrorPage';

// Auth
import LoginPage from './pages/auth/LoginPage';
const MfaPage = lazy(() => import('./pages/auth/MfaPage'));
const SetupMfaPage = lazy(() => import('./pages/auth/SetupMfaPage'));
const ChangePasswordPage = lazy(() => import('./pages/auth/ChangePasswordPage'));

// Web app (lazy)
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
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
const SalesOrdersPage = lazy(() => import('./pages/integration/SalesOrdersPage'));
const SalesOrderDetailPage = lazy(() => import('./pages/integration/SalesOrderDetailPage'));
const ProductionOrdersPage = lazy(() => import('./pages/integration/ProductionOrdersPage'));
const ProductionOrderDetailPage = lazy(() => import('./pages/integration/ProductionOrderDetailPage'));
const WorkOrdersPage = lazy(() => import('./pages/production/WorkOrdersPage'));
const WorkOrderDetailPage = lazy(() => import('./pages/production/WorkOrderDetailPage'));
const JobsPage = lazy(() => import('./pages/production/JobsPage'));
const JobDetailPage = lazy(() => import('./pages/production/JobDetailPage'));
const DowntimePage = lazy(() => import('./pages/production/DowntimePage'));
const DowntimeDetailPage = lazy(() => import('./pages/production/DowntimeDetailPage'));
const InventoryPage = lazy(() => import('./pages/production/InventoryPage'));
const InventoryTracePage = lazy(() => import('./pages/production/InventoryTracePage'));
const SchedulePage = lazy(() => import('./pages/schedule/SchedulePage'));
const MaintenancePage = lazy(() => import('./pages/maintenance/MaintenancePage'));
const ProductionReportPage = lazy(() => import('./pages/reports/ProductionReportPage'));
const OeeReportPage = lazy(() => import('./pages/reports/OeeReportPage'));
const DowntimeReportPage = lazy(() => import('./pages/reports/DowntimeReportPage'));
const QualityReportPage = lazy(() => import('./pages/reports/QualityReportPage'));
const SettingsPage = lazy(() => import('./pages/admin/SettingsPage'));
const UsersPage = lazy(() => import('./pages/admin/UsersPage'));
const UserDetailPage = lazy(() => import('./pages/admin/UserDetailPage'));
const RolesPage = lazy(() => import('./pages/admin/RolesPage'));
const AuditLogPage = lazy(() => import('./pages/admin/AuditLogPage'));

// Tablet (separate layout — M5)
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
        <Route
          path="/"
          element={
            <AuthGuard>
              <WebLayout />
            </AuthGuard>
          }
        >
          <Route index element={<Navigate to="/dashboard" replace />} />

          {/* Dashboard */}
          <Route path="dashboard" element={<DashboardPage />} />

          {/* Master data */}
          <Route path="master/work-centers" element={<WorkCentersPage />} />
          <Route path="master/work-centers/:id" element={<WorkCenterDetailPage />} />
          <Route path="master/machines" element={<MachinesPage />} />
          <Route path="master/products" element={<ProductsPage />} />
          <Route path="master/products/:id/bom" element={<BomEditorPage />} />
          <Route path="master/operations" element={<OperationsPage />} />
          <Route path="master/routings" element={<RoutingsPage />} />
          <Route path="master/routings/:id/steps" element={<RoutingStepsPage />} />
          <Route path="master/storage-locations" element={<StorageLocationsPage />} />
          <Route path="master/defect-codes" element={<DefectCodesPage />} />

          {/* Integration */}
          <Route path="integration/sales-orders" element={<SalesOrdersPage />} />
          <Route path="integration/sales-orders/:id" element={<SalesOrderDetailPage />} />
          <Route path="integration/production-orders" element={<ProductionOrdersPage />} />
          <Route path="integration/production-orders/:id" element={<ProductionOrderDetailPage />} />

          {/* Production */}
          <Route path="production/work-orders" element={<WorkOrdersPage />} />
          <Route path="production/work-orders/:id" element={<WorkOrderDetailPage />} />
          <Route path="production/jobs" element={<JobsPage />} />
          <Route path="production/jobs/:id" element={<JobDetailPage />} />
          <Route path="production/downtime" element={<DowntimePage />} />
          <Route path="production/downtime/:id" element={<DowntimeDetailPage />} />
          <Route path="production/inventory" element={<InventoryPage />} />
          <Route path="production/inventory/trace" element={<InventoryTracePage />} />
          <Route path="production/schedule" element={<SchedulePage />} />

          {/* Maintenance */}
          <Route path="maintenance" element={<MaintenancePage />} />

          {/* Reports */}
          <Route path="reports/production" element={<ProductionReportPage />} />
          <Route path="reports/oee" element={<OeeReportPage />} />
          <Route path="reports/downtime" element={<DowntimeReportPage />} />
          <Route path="reports/quality" element={<QualityReportPage />} />

          {/* Admin */}
          <Route path="admin/settings" element={<SettingsPage />} />
          <Route path="admin/users" element={<UsersPage />} />
          <Route path="admin/users/:id" element={<UserDetailPage />} />
          <Route path="admin/roles" element={<RolesPage />} />
          <Route path="admin/audit-log" element={<AuditLogPage />} />

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
