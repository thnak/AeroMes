import { Box, Card, CardContent, Grid, Skeleton, Stack } from '@mui/material';

// ─── KPI row skeleton (4 cards) ──────────────────────────────────────────────
export function KpiRowSkeleton({ count = 4 }: { count?: number }) {
  return (
    <Grid container spacing={2.5} sx={{ mb: 3 }}>
      {Array.from({ length: count }).map((_, i) => (
        <Grid key={i} size={{ xs: 12, sm: 6, md: 12 / count }}>
          <Card>
            <CardContent sx={{ p: '20px !important' }}>
              <Skeleton variant="text" width="50%" height={14} sx={{ mb: 1 }} />
              <Skeleton variant="text" width="40%" height={36} />
              <Skeleton variant="text" width="30%" height={14} sx={{ mt: 0.5 }} />
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
}

// ─── Table page skeleton ──────────────────────────────────────────────────────
export function TablePageSkeleton({ rows = 8 }: { rows?: number }) {
  return (
    <Box>
      {/* Page header */}
      <Box sx={{ mb: 3 }}>
        <Skeleton variant="text" width={120} height={14} sx={{ mb: 1 }} />
        <Stack direction="row" sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Skeleton variant="text" width={220} height={30} />
            <Skeleton variant="text" width={300} height={18} sx={{ mt: 0.5 }} />
          </Box>
          <Skeleton variant="rounded" width={110} height={34} />
        </Stack>
      </Box>

      {/* Toolbar */}
      <Stack direction="row" spacing={1.5} sx={{ mb: 2 }}>
        <Skeleton variant="rounded" width={240} height={36} />
        <Skeleton variant="rounded" width={160} height={36} />
        <Box sx={{ flex: 1 }} />
        <Skeleton variant="rounded" width={90} height={36} />
      </Stack>

      {/* Table rows */}
      <Card>
        <CardContent sx={{ p: '0 !important' }}>
          {/* Header row */}
          <Stack
            direction="row"
            spacing={2}
            sx={{ px: 2, py: 1.25, borderBottom: '1px solid', borderColor: 'divider' }}
          >
            {[16, 20, 14, 10, 14, 12].map((w, i) => (
              <Skeleton key={i} variant="text" width={`${w}%`} height={14} />
            ))}
          </Stack>
          {/* Data rows */}
          {Array.from({ length: rows }).map((_, i) => (
            <Stack
              key={i}
              direction="row"
              spacing={2}
              sx={{
                px: 2,
                py: 1.5,
                borderBottom: '1px solid',
                borderColor: 'divider',
                opacity: 1 - i * 0.08,
              }}
            >
              {[16, 20, 14, 10, 14, 12].map((w, j) => (
                <Skeleton key={j} variant="text" width={`${w}%`} height={16} />
              ))}
            </Stack>
          ))}
        </CardContent>
      </Card>
    </Box>
  );
}

// ─── Detail page skeleton (form layout) ──────────────────────────────────────
export function DetailPageSkeleton() {
  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Skeleton variant="text" width={120} height={14} sx={{ mb: 1 }} />
        <Skeleton variant="text" width={280} height={30} />
        <Skeleton variant="text" width={360} height={18} sx={{ mt: 0.5 }} />
      </Box>

      <Grid container spacing={2.5}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Card>
            <CardContent>
              <Skeleton variant="text" width={140} height={22} sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                {Array.from({ length: 6 }).map((_, i) => (
                  <Grid key={i} size={{ xs: 12, sm: 6 }}>
                    <Skeleton variant="text" width="40%" height={14} sx={{ mb: 0.75 }} />
                    <Skeleton variant="rounded" width="100%" height={36} />
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Skeleton variant="text" width={120} height={22} sx={{ mb: 2 }} />
              {Array.from({ length: 4 }).map((_, i) => (
                <Box key={i} sx={{ mb: 1.5 }}>
                  <Skeleton variant="text" width="35%" height={14} sx={{ mb: 0.5 }} />
                  <Skeleton variant="text" width="65%" height={18} />
                </Box>
              ))}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
