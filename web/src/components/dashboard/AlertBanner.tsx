import { useState } from 'react';
import {
  Alert,
  Box,
  Chip,
  Collapse,
  Divider,
  IconButton,
  Skeleton,
  Stack,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';

type AlertSeverity = 'error' | 'warning' | 'info' | 'success';

export interface AlertItem {
  label: string;
  value: string | number;
  severity: AlertSeverity;
}

interface AlertBannerProps {
  items: AlertItem[];
  title?: string;
  loading?: boolean;
  defaultExpanded?: boolean;
}

export default function AlertBanner({
  items,
  title = 'Alerts',
  loading = false,
  defaultExpanded = true,
}: AlertBannerProps) {
  const [expanded, setExpanded] = useState(defaultExpanded);

  if (loading) return <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 2 }} />;
  if (items.length === 0) return null;

  const errorCount = items.filter((i) => i.severity === 'error').length;
  const warnCount = items.filter((i) => i.severity === 'warning').length;

  const worstSeverity: AlertSeverity =
    errorCount > 0 ? 'error' : warnCount > 0 ? 'warning' : 'info';

  return (
    <Alert
      severity={worstSeverity}
      sx={{ borderRadius: 2, p: 0, overflow: 'hidden' }}
      action={
        <IconButton size="small" onClick={() => setExpanded((v) => !v)} sx={{ mr: 0.5, mt: 0.5 }}>
          {expanded ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
        </IconButton>
      }
    >
      <Box sx={{ px: 1, py: 0.5 }}>
        <Stack direction="row" sx={{ alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
          <Typography variant="body2" sx={{ fontWeight: 600 }}>
            {title}
          </Typography>
          {errorCount > 0 && (
            <Chip label={`${errorCount} critical`} color="error" size="small" />
          )}
          {warnCount > 0 && (
            <Chip label={`${warnCount} warning`} color="warning" size="small" />
          )}
        </Stack>
      </Box>

      <Collapse in={expanded}>
        <Divider />
        <Stack spacing={0} sx={{ px: 1, py: 0.5 }}>
          {items.map((item, i) => (
            <Stack
              key={i}
              direction="row"
              sx={{ justifyContent: 'space-between', alignItems: 'center', py: 0.5 }}
            >
              <Typography variant="body2">{item.label}</Typography>
              <Chip
                label={item.value}
                color={item.severity}
                size="small"
                sx={{ minWidth: 48, justifyContent: 'center' }}
              />
            </Stack>
          ))}
        </Stack>
      </Collapse>
    </Alert>
  );
}
