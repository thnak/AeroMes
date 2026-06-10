import { Routes, Route, Navigate } from 'react-router-dom';
import WebLayout from './layouts/WebLayout';
import DashboardPage from './pages/DashboardPage';
import ProductsPage from './pages/master/ProductsPage';
import MaintenancePage from './pages/maintenance/MaintenancePage';
import SchedulePage from './pages/schedule/SchedulePage';
import SettingsPage from './pages/admin/SettingsPage';
import ErrorPage from './pages/ErrorPage';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<WebLayout />}>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard"                    element={<DashboardPage />} />
        <Route path="master/products"              element={<ProductsPage />} />
        <Route path="production/schedule"          element={<SchedulePage />} />
        <Route path="maintenance"                  element={<MaintenancePage />} />
        <Route path="admin/settings"               element={<SettingsPage />} />
        <Route path="403"                          element={<ErrorPage code={403} />} />
        <Route path="404"                          element={<ErrorPage code={404} />} />
        <Route path="500"                          element={<ErrorPage code={500} />} />
        <Route path="*"                            element={<ErrorPage code={404} />} />
      </Route>
    </Routes>
  );
}
