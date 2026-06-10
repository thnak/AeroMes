import {
  alpha,
  Box,
  Collapse,
  Divider,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import { motion, AnimatePresence } from 'framer-motion';
import { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import SolarIcon from '../components/SolarIcon';
import type { IconKey } from '../lib/icons';
import { SIDEBAR_COLLAPSED_WIDTH, SIDEBAR_WIDTH } from '../theme/tokens';

// ─── Nav tree types ───────────────────────────────────────────────────────────

export interface NavItem {
  label: string;
  href?: string;
  icon: IconKey;
  children?: NavItem[];
  badge?: string | number;
}

export interface NavSection {
  title?: string;
  items: NavItem[];
}

// ─── Navigation config ────────────────────────────────────────────────────────

export const NAV_SECTIONS: NavSection[] = [
  {
    items: [
      { label: 'Dashboard',    href: '/dashboard',   icon: 'dashboard' },
    ],
  },
  {
    title: 'Production',
    items: [
      { label: 'Schedule',     href: '/production/schedule',    icon: 'oee' },
      { label: 'Work Orders',  href: '/production/work-orders', icon: 'workOrders' },
      { label: 'Jobs',         href: '/production/jobs',        icon: 'jobs' },
    ],
  },
  {
    title: 'Maintenance',
    items: [
      { label: 'Issues',       href: '/maintenance',            icon: 'maintenance' },
    ],
  },
  {
    title: 'Quality',
    items: [
      { label: 'Inspection',   href: '/quality/inspection',     icon: 'quality' },
      { label: 'Defect Codes', href: '/quality/defect-codes',   icon: 'error' },
    ],
  },
  {
    title: 'Master Data',
    items: [
      { label: 'Products',     href: '/master/products',    icon: 'products' },
      { label: 'BOM',          href: '/master/bom',         icon: 'bom' },
      { label: 'Routings',     href: '/master/routings',    icon: 'routing' },
      { label: 'Machines',     href: '/master/machines',    icon: 'machines' },
    ],
  },
  {
    title: 'Integration',
    items: [
      { label: 'Production Orders', href: '/integration/orders', icon: 'integration' },
    ],
  },
  {
    title: 'Reports',
    items: [
      { label: 'OEE',          href: '/reports/oee',       icon: 'oee' },
      { label: 'Output',       href: '/reports/output',    icon: 'reports' },
    ],
  },
  {
    title: 'Admin',
    items: [
      { label: 'Users',        href: '/admin/users',       icon: 'admin' },
      { label: 'Settings',     href: '/admin/settings',    icon: 'settings' },
    ],
  },
];

// ─── Single nav item ──────────────────────────────────────────────────────────

function NavItemRow({
  item,
  collapsed,
  depth = 0,
}: {
  item: NavItem;
  collapsed: boolean;
  depth?: number;
}) {
  const { pathname } = useLocation();
  const [open, setOpen] = useState(false);
  const isActive = item.href ? pathname === item.href || pathname.startsWith(item.href + '/') : false;
  const hasChildren = !!item.children?.length;

  const button = (
    <ListItemButton
      component={item.href && !hasChildren ? NavLink : 'div'}
      to={item.href && !hasChildren ? item.href : undefined}
      onClick={hasChildren ? () => setOpen((v) => !v) : undefined}
      selected={isActive}
      sx={(theme) => ({
        pl: collapsed ? 1 : 1.5 + depth * 1.5,
        py: 0.75,
        gap: collapsed ? 0 : 1.25,
        minHeight: 40,
        borderRadius: 2,
        mx: '6px',
        width: `calc(100% - 12px)`,
        justifyContent: collapsed ? 'center' : 'flex-start',
        color: isActive ? 'primary.main' : 'text.secondary',
        bgcolor: isActive ? alpha(theme.palette.primary.main, 0.1) : 'transparent',
        '&:hover': {
          bgcolor: alpha(theme.palette.primary.main, 0.07),
          color: 'primary.main',
        },
        '&.active': {
          color: 'primary.main',
          bgcolor: alpha(theme.palette.primary.main, 0.1),
        },
        transition: 'all 0.15s ease',
      })}
    >
      <ListItemIcon
        sx={{
          minWidth: 0,
          color: 'inherit',
          flexShrink: 0,
        }}
      >
        <SolarIcon name={item.icon} size={20} />
      </ListItemIcon>

      {!collapsed && (
        <>
          <ListItemText
            primary={
              <Typography variant="body2" noWrap sx={{ fontWeight: isActive ? 600 : 400 }}>
                {item.label}
              </Typography>
            }
          />
          {hasChildren && (
            <SolarIcon
              name={open ? 'collapse' : 'expand'}
              size={16}
              sx={{ color: 'text.disabled', ml: 'auto' }}
            />
          )}
          {item.badge !== undefined && (
            <Box
              sx={{
                ml: 'auto',
                px: 0.75,
                py: 0.1,
                borderRadius: 1,
                bgcolor: 'primary.main',
                color: 'primary.contrastText',
                fontSize: '0.625rem',
                fontWeight: 700,
                lineHeight: 1.6,
              }}
            >
              {item.badge}
            </Box>
          )}
        </>
      )}
    </ListItemButton>
  );

  if (collapsed) {
    return (
      <Tooltip title={item.label} placement="right" arrow>
        <span>{button}</span>
      </Tooltip>
    );
  }

  if (hasChildren) {
    return (
      <>
        {button}
        <Collapse in={open} unmountOnExit>
          <List disablePadding>
            {item.children!.map((child) => (
              <NavItemRow key={child.label} item={child} collapsed={false} depth={depth + 1} />
            ))}
          </List>
        </Collapse>
      </>
    );
  }

  return button;
}

// ─── Sidebar ─────────────────────────────────────────────────────────────────

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

export default function Sidebar({ collapsed, onToggle }: SidebarProps) {
  const width = collapsed ? SIDEBAR_COLLAPSED_WIDTH : SIDEBAR_WIDTH;

  return (
    <motion.div
      animate={{ width }}
      transition={{ duration: 0.22, ease: [0.4, 0, 0.2, 1] }}
      style={{ flexShrink: 0, position: 'relative' }}
    >
      <Box
        sx={(theme) => ({
          width,
          height: '100vh',
          position: 'fixed',
          top: 0,
          left: 0,
          display: 'flex',
          flexDirection: 'column',
          bgcolor: 'background.paper',
          borderRight: '1px solid',
          borderColor: 'divider',
          overflowX: 'hidden',
          overflowY: 'auto',
          zIndex: theme.zIndex.drawer,
          transition: 'width 0.22s cubic-bezier(0.4, 0, 0.2, 1)',
        })}
      >
        {/* Logo row */}
        <Stack
          direction="row"
          sx={{
            alignItems: 'center',
            height: 56,
            px: 1.5,
            flexShrink: 0,
            gap: 1.25,
            borderBottom: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Box
            sx={{
              width: 32,
              height: 32,
              borderRadius: 1.5,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: 'primary.main',
              color: 'primary.contrastText',
              flexShrink: 0,
            }}
          >
            <SolarIcon name="production" size={18} />
          </Box>
          <AnimatePresence>
            {!collapsed && (
              <motion.div
                initial={{ opacity: 0, x: -8 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -8 }}
                transition={{ duration: 0.15 }}
                style={{ overflow: 'hidden', whiteSpace: 'nowrap' }}
              >
                <Typography variant="subtitle1" sx={{ fontWeight: 700, color: 'primary.main' }}>
                  AeroMes
                </Typography>
              </motion.div>
            )}
          </AnimatePresence>

          <Box sx={{ ml: 'auto', flexShrink: 0 }}>
            <IconButton size="small" onClick={onToggle} sx={{ color: 'text.secondary' }}>
              <SolarIcon name={collapsed ? 'chevronRight' : 'chevronLeft'} size={18} />
            </IconButton>
          </Box>
        </Stack>

        {/* Nav sections */}
        <Box sx={{ flex: 1, py: 1, overflowY: 'auto', overflowX: 'hidden' }}>
          {NAV_SECTIONS.map((section, si) => (
            <Box key={si} sx={{ mb: 0.5 }}>
              {section.title && !collapsed && (
                <Typography
                  variant="overline"
                  sx={{
                    px: 2,
                    py: 0.5,
                    display: 'block',
                    color: 'text.disabled',
                    fontSize: '0.625rem',
                  }}
                >
                  {section.title}
                </Typography>
              )}
              {section.title && collapsed && si > 0 && (
                <Divider sx={{ mx: 1, my: 0.75 }} />
              )}
              <List disablePadding dense>
                {section.items.map((item) => (
                  <NavItemRow key={item.label} item={item} collapsed={collapsed} />
                ))}
              </List>
            </Box>
          ))}
        </Box>
      </Box>
    </motion.div>
  );
}
