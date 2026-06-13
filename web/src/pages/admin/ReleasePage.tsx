import {
  Box,
  Card,
  CardContent,
  Chip,
  Divider,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import { alpha } from '@mui/material/styles';
import { useEffect } from 'react';
import { PageHeader, PageRoot, SolarIcon } from '../../components';
import { Icons } from '../../lib/icons';
import {
  useAppInfo,
  useChangelog,
  markReleasesSeen,
  type ChangelogChange,
} from '../../lib/useAppInfo';

// ─── Change type badge ────────────────────────────────────────────────────────

const TYPE_CONFIG: Record<
  ChangelogChange['type'],
  { label: string; color: string }
> = {
  feature:     { label: 'Feature',     color: '#0D9488' },
  fix:         { label: 'Fix',         color: '#DC2626' },
  improvement: { label: 'Improvement', color: '#1D4ED8' },
  breaking:    { label: 'Breaking',    color: '#9333EA' },
};

function ChangeTypeBadge({ type }: { type: ChangelogChange['type'] }) {
  const cfg = TYPE_CONFIG[type] ?? TYPE_CONFIG.improvement;
  return (
    <Chip
      label={cfg.label}
      size="small"
      sx={{
        height: 20,
        fontSize: '0.65rem',
        fontWeight: 700,
        flexShrink: 0,
        bgcolor: alpha(cfg.color, 0.1),
        color: cfg.color,
        border: `1px solid ${alpha(cfg.color, 0.22)}`,
        '& .MuiChip-label': { px: 0.75 },
      }}
    />
  );
}

// ─── Info field ───────────────────────────────────────────────────────────────

function InfoField({
  icon,
  label,
  value,
  mono,
}: {
  icon: keyof typeof Icons;
  label: string;
  value: string;
  mono?: boolean;
}) {
  return (
    <Box>
      <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', mb: 0.25 }}>
        <SolarIcon name={icon} size={12} sx={{ color: 'text.disabled' }} />
        <Typography
          variant="caption"
          color="text.disabled"
          sx={{ textTransform: 'uppercase', letterSpacing: '0.06em', fontSize: '0.625rem' }}
        >
          {label}
        </Typography>
      </Stack>
      <Typography
        variant="body2"
        sx={{ fontWeight: 600, fontFamily: mono ? 'ui-monospace, monospace' : undefined }}
      >
        {value}
      </Typography>
    </Box>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function ReleasePage() {
  const { data: appInfo, isLoading: infoLoading } = useAppInfo();
  const { data: changelog, isLoading: logLoading } = useChangelog();

  // Mark all releases as seen when the user views this page
  useEffect(() => {
    if (changelog?.[0]?.version) {
      markReleasesSeen(changelog[0].version);
    }
  }, [changelog]);

  return (
    <PageRoot>
      <PageHeader
        title="Release Notes"
        subtitle="Version history and feature updates for AeroMes"
        breadcrumbs={[{ label: 'Admin' }, { label: 'Settings' }, { label: 'Release Notes' }]}
      />

      {/* Current version banner */}
      <Card sx={{ mb: 3 }}>
        <CardContent sx={{ p: '20px !important' }}>
          {infoLoading ? (
            <Stack direction="row" spacing={4} sx={{ flexWrap: 'wrap' }}>
              {Array.from({ length: 4 }).map((_, i) => (
                <Box key={i}>
                  <Skeleton width={70} height={12} />
                  <Skeleton width={110} height={20} sx={{ mt: 0.5 }} />
                </Box>
              ))}
            </Stack>
          ) : appInfo ? (
            <Stack direction="row" spacing={4} sx={{ flexWrap: 'wrap', gap: 2.5 }}>
              <InfoField icon="version"  label="Version"     value={`v${appInfo.version}`} mono />
              <InfoField icon="date"     label="Build date"  value={appInfo.buildDate !== 'unknown' ? appInfo.buildDate : '—'} />
              <InfoField icon="server"   label="Environment" value={appInfo.environment} />
              <InfoField icon="instance" label="Instance"    value={appInfo.instanceId} mono />
              {appInfo.commitSha && (
                <InfoField icon="commit" label="Commit" value={appInfo.commitSha.slice(0, 8)} mono />
              )}
            </Stack>
          ) : null}
        </CardContent>
      </Card>

      {/* Changelog timeline */}
      {logLoading ? (
        <Stack spacing={2}>
          {Array.from({ length: 3 }).map((_, i) => (
            <Card key={i}>
              <CardContent sx={{ p: '20px !important' }}>
                <Skeleton width={90} height={28} />
                <Skeleton width={150} height={14} sx={{ mt: 0.5, mb: 2 }} />
                <Skeleton width="85%" height={14} />
                <Skeleton width="70%" height={14} sx={{ mt: 1 }} />
                <Skeleton width="75%" height={14} sx={{ mt: 1 }} />
              </CardContent>
            </Card>
          ))}
        </Stack>
      ) : (
        <Stack spacing={2}>
          {(changelog ?? []).map((entry) => (
            <Card key={entry.version}>
              <CardContent sx={{ p: '20px !important' }}>
                {/* Version header */}
                <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center', mb: 0.5 }}>
                  <Typography
                    variant="h6"
                    sx={{ fontWeight: 700, fontFamily: 'ui-monospace, monospace' }}
                  >
                    v{entry.version}
                  </Typography>
                  {appInfo?.version === entry.version && (
                    <Chip
                      label="Current"
                      size="small"
                      color="success"
                      sx={{
                        height: 18,
                        fontSize: '0.65rem',
                        fontWeight: 700,
                        '& .MuiChip-label': { px: 0.75 },
                      }}
                    />
                  )}
                </Stack>
                <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 2 }}>
                  Released {entry.date}
                </Typography>

                <Divider sx={{ mb: 2 }} />

                {/* Change list */}
                <Stack spacing={1.5}>
                  {entry.changes.map((change, i) => (
                    <Stack key={i} direction="row" spacing={1.5} sx={{ alignItems: 'flex-start' }}>
                      <Box sx={{ pt: 0.125 }}>
                        <ChangeTypeBadge type={change.type} />
                      </Box>
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 600 }}>
                          {change.title}
                        </Typography>
                        <Typography
                          variant="caption"
                          color="text.secondary"
                          sx={{ lineHeight: 1.55, display: 'block' }}
                        >
                          {change.description}
                        </Typography>
                      </Box>
                    </Stack>
                  ))}
                </Stack>
              </CardContent>
            </Card>
          ))}
        </Stack>
      )}
    </PageRoot>
  );
}
