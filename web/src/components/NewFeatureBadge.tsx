import { useState } from 'react';
import Chip from '@mui/material/Chip';

const SEEN_KEY = 'aeromes:seen-features';

function loadSeen(): Set<string> {
  try { return new Set(JSON.parse(localStorage.getItem(SEEN_KEY) ?? '[]')); }
  catch { return new Set(); }
}

function markSeen(key: string) {
  const seen = loadSeen();
  seen.add(key);
  localStorage.setItem(SEEN_KEY, JSON.stringify([...seen]));
}

interface Props {
  /** Unique key for this feature — once dismissed it stays hidden for this user/device */
  featureKey: string;
}

/**
 * Displays a "NEW" pill badge next to a nav item or feature label.
 * Clicking it (or the parent calling markSeen) hides it permanently.
 */
export default function NewFeatureBadge({ featureKey }: Props) {
  const [visible, setVisible] = useState(() => !loadSeen().has(featureKey));

  if (!visible) return null;

  return (
    <Chip
      label="NEW"
      size="small"
      color="primary"
      onClick={(e) => {
        e.stopPropagation();
        markSeen(featureKey);
        setVisible(false);
      }}
      sx={{
        height: 18,
        fontSize: '0.625rem',
        fontWeight: 800,
        letterSpacing: '0.06em',
        cursor: 'pointer',
        flexShrink: 0,
        '& .MuiChip-label': { px: 0.75 },
      }}
    />
  );
}

