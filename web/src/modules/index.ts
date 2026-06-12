import type { UserRole } from '../contexts/AuthContext';
import type { IconKey } from '../lib/icons';

export interface ModuleTab {
  label: string;
  path: string;
  /** If set, only users with at least one of these roles see this tab. */
  roles?: UserRole[];
}

export interface ModuleConfig {
  id: string;
  label: string;
  description: string;
  icon: IconKey;
  color: string;
  path: string;
  tabs: ModuleTab[];
  available: boolean;
  /** If set, only users with at least one of these roles see this tile. */
  roles?: UserRole[];
}

export const MODULES: ModuleConfig[] = [
  {
    id: 'production',
    label: 'Production',
    description: 'Work orders, jobs, downtime & inventory',
    icon: 'production',
    color: '#1D4ED8',
    path: '/production',
    available: true,
    tabs: [
      { label: 'Schedule',    path: '/production/schedule' },
      { label: 'Work Orders', path: '/production/work-orders' },
      { label: 'Jobs',        path: '/production/jobs' },
      { label: 'Downtime',    path: '/production/downtime' },
      { label: 'Inventory',   path: '/production/inventory' },
    ],
  },
  {
    id: 'quality',
    label: 'Quality',
    description: 'Inspection orders, NCR & defect management',
    icon: 'quality',
    color: '#15803D',
    path: '/quality',
    available: true,
    tabs: [
      { label: 'Inspection Orders', path: '/quality/inspection-orders' },
      { label: 'Inspection Plans',  path: '/quality/inspection-plans' },
      { label: 'NCR',              path: '/quality/ncr' },
      { label: 'Defect Analysis',  path: '/quality/defect-analysis' },
    ],
  },
  {
    id: 'master',
    label: 'Master Data',
    description: 'Products, BOM, routings, work centers & machines',
    icon: 'masterData',
    color: '#7C3AED',
    path: '/master',
    available: true,
    tabs: [
      { label: 'Products',         path: '/master/products' },
      { label: 'Work Centers',     path: '/master/work-centers' },
      { label: 'Machines',         path: '/master/machines' },
      { label: 'Operations',       path: '/master/operations' },
      { label: 'Routings',         path: '/master/routings' },
      { label: 'Storage',          path: '/master/storage-locations' },
      { label: 'Defect Codes',     path: '/master/defect-codes' },
    ],
  },
  {
    id: 'integration',
    label: 'Integration',
    description: 'Sales orders & production orders from ERP',
    icon: 'integration',
    color: '#D97706',
    path: '/integration',
    available: true,
    tabs: [
      { label: 'Sales Orders',      path: '/integration/sales-orders' },
      { label: 'Production Orders', path: '/integration/production-orders' },
    ],
  },
  {
    id: 'maintenance',
    label: 'Maintenance',
    description: 'Machine issues, repair logs & preventive tasks',
    icon: 'maintenance',
    color: '#DC2626',
    path: '/maintenance',
    available: true,
    tabs: [
      { label: 'Issues', path: '/maintenance' },
    ],
  },
  {
    id: 'reports',
    label: 'Reports',
    description: 'OEE, production output, downtime & quality reports',
    icon: 'reports',
    color: '#0D9488',
    path: '/reports',
    available: true,
    tabs: [
      { label: 'Production', path: '/reports/production' },
      { label: 'OEE',        path: '/reports/oee' },
      { label: 'Downtime',   path: '/reports/downtime' },
      { label: 'Quality',    path: '/reports/quality' },
    ],
  },
  {
    id: 'admin',
    label: 'Admin',
    description: 'Users, roles, audit log & system settings',
    icon: 'admin',
    color: '#475569',
    path: '/admin',
    available: true,
    roles: ['Admin'],
    tabs: [
      { label: 'Users',     path: '/admin/users' },
      { label: 'Roles',     path: '/admin/roles', roles: ['Admin'] },
      { label: 'Audit Log', path: '/admin/audit-log', roles: ['Admin', 'Manager'] },
      { label: 'Settings',  path: '/admin/settings', roles: ['Admin'] },
    ],
  },
  {
    id: 'planning',
    label: 'Planning',
    description: 'Capacity planning, scheduling & demand forecasts',
    icon: 'oee',
    color: '#4F46E5',
    path: '/planning',
    available: false,
    tabs: [],
  },
  {
    id: 'warehouse',
    label: 'Warehouse',
    description: 'Stock movements, receipts, transfers & locations',
    icon: 'bom',
    color: '#92400E',
    path: '/warehouse',
    available: false,
    tabs: [],
  },
  {
    id: 'iot',
    label: 'IoT & Machines',
    description: 'Live signals, machine state, OEE & alert rules',
    icon: 'machineOn',
    color: '#0891B2',
    path: '/iot',
    available: false,
    tabs: [],
  },
  {
    id: 'lab',
    label: 'Lab',
    description: 'Test requests, measurement results & certificates',
    icon: 'qualityOee',
    color: '#BE185D',
    path: '/lab',
    available: false,
    tabs: [],
  },
  {
    id: 'traceability',
    label: 'Traceability',
    description: 'Lot genealogy, serial history & full trace graph',
    icon: 'routing',
    color: '#65A30D',
    path: '/traceability',
    available: false,
    tabs: [],
  },
];
