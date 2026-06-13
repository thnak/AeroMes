import {
  Box,
  Breadcrumbs,
  Link,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import type { SxProps, Theme } from '@mui/material/styles';
import type { ReactNode } from 'react';

// ─── Page-level breadcrumb item ──────────────────────────────────────────────
export interface BreadcrumbItem {
  label: string;
  href?: string;
}

// ─── PageHeader ───────────────────────────────────────────────────────────────
//
//  Standard anatomy:
//  ┌──────────────────────────────────────────────────────┐
//  │ [Home > Section > Current page]  (breadcrumbs)       │
//  │ Page Title                   [secondary] [primary]   │
//  │ Subtitle / description text                          │
//  └──────────────────────────────────────────────────────┘
//
//  Usage:
//  <PageHeader
//    title="Work Orders"
//    subtitle="Manage and monitor all production work orders"
//    breadcrumbs={[{ label: 'Production' }, { label: 'Work Orders' }]}
//    actions={<Button variant="contained" startIcon={<SolarIcon name="add" />}>New WO</Button>}
//  />

interface PageHeaderProps {
  title: string;
  subtitle?: string;
  breadcrumbs?: BreadcrumbItem[];
  actions?: ReactNode;
  loading?: boolean;
}

export default function PageHeader({
  title,
  subtitle,
  breadcrumbs,
  actions,
  loading = false,
}: PageHeaderProps) {
  return (
    <Box sx={{ mb: 3 }}>
      {/* Breadcrumbs */}
      {breadcrumbs && breadcrumbs.length > 0 && (
        <Breadcrumbs
          aria-label="breadcrumb"
          sx={{ mb: 1, '& .MuiBreadcrumbs-separator': { mx: 0.5 } }}
        >
          {breadcrumbs.map((crumb, i) => {
            const isLast = i === breadcrumbs.length - 1;
            return isLast || !crumb.href ? (
              <Typography
                key={i}
                variant="caption"
                color={isLast ? 'text.primary' : 'text.secondary'}
                sx={{ fontWeight: isLast ? 600 : 400 }}
              >
                {crumb.label}
              </Typography>
            ) : (
              <Link
                key={i}
                href={crumb.href}
                underline="hover"
                color="text.secondary"
                sx={{ fontSize: '0.75rem' }}
              >
                {crumb.label}
              </Link>
            );
          })}
        </Breadcrumbs>
      )}

      {/* Title row */}
      <Stack
        direction="row"
        spacing={2}
        sx={{ alignItems: 'flex-start', justifyContent: 'space-between', flexWrap: 'wrap' }}
      >
        <Box sx={{ flex: 1, minWidth: 0 }}>
          {loading ? (
            <Skeleton variant="text" width={220} height={32} />
          ) : (
            <Typography variant="h5" component="h1" sx={{ lineHeight: 1.3 }}>
              {title}
            </Typography>
          )}
          {subtitle && (
            loading ? (
              <Skeleton variant="text" width={340} height={20} sx={{ mt: 0.5 }} />
            ) : (
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {subtitle}
              </Typography>
            )
          )}
        </Box>

        {actions && (
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexShrink: 0, pt: 0.25 }}>
            {actions}
          </Stack>
        )}
      </Stack>
    </Box>
  );
}

// ─── PageRoot ──────────────────────────────────────────────────────────────────
//  Wraps page content with consistent padding. Use inside each route component.

interface PageRootProps {
  children: ReactNode;
  /** Remove horizontal padding for full-bleed tables/grids */
  noPadding?: boolean;
  /** Extra sx overrides — use to add bottom padding on form pages with FormActionBar */
  sx?: SxProps<Theme>;
}

export function PageRoot({ children, noPadding = false, sx }: PageRootProps) {
  return (
    <Box
      sx={{
        flex: 1,
        display: 'flex',
        flexDirection: 'column',
        p: noPadding ? 0 : 3,
        minHeight: 0,
        ...sx,
      }}
    >
      {children}
    </Box>
  );
}

// ─── PageSection ──────────────────────────────────────────────────────────────
//  Divides a page into named sections (e.g., General / Details / History).

interface PageSectionProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
  actions?: ReactNode;
}

export function PageSection({ title, subtitle, children, actions }: PageSectionProps) {
  return (
    <Box sx={{ mb: 3 }}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
        <Box>
          <Typography variant="subtitle1" sx={{ lineHeight: 1.3 }}>
            {title}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="text.secondary">
              {subtitle}
            </Typography>
          )}
        </Box>
        {actions}
      </Stack>
      {children}
    </Box>
  );
}
