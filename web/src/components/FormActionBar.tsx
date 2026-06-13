import { useEffect } from 'react';
import { useBlocker } from 'react-router-dom';
import { alpha, Box, Button, CircularProgress, Stack, Typography, useTheme } from '@mui/material';
import ConfirmDialog from './ConfirmDialog';
import { FORM_ACTION_BAR_HEIGHT } from '../theme/tokens';

// ─── FormActionBar ─────────────────────────────────────────────────────────────
//
//  Sticky bottom bar for create/edit pages. Replaces FormDrawer for full-page
//  form routes (/new, /:id/edit).
//
//  Anatomy:
//  ┌──────────────────────────────────────────────────────────────────────────┐
//  │  ● Unsaved changes          (spacer)          [Cancel]  [Save / Submit] │
//  └──────────────────────────────────────────────────────────────────────────┘
//  position: fixed, bottom: 0, full-width, height: FORM_ACTION_BAR_HEIGHT
//
//  Navigation guard:
//  - Blocks react-router transitions while isDirty via useBlocker
//  - Blocks browser tab-close/reload via beforeunload
//  - Shows ConfirmDialog (not native browser confirm) when blocked
//
//  Usage (inside a react-hook-form page):
//  <FormActionBar
//    isDirty={isDirty}
//    isSubmitting={isSubmitting}
//    onCancel={() => navigate(-1)}
//    onSubmit={handleSubmit(onSave)}
//  />
//
//  Add `pb: \`${FORM_ACTION_BAR_HEIGHT + 24}px\`` to PageRoot so content
//  isn't hidden behind the bar when scrolled to the bottom.

export { FORM_ACTION_BAR_HEIGHT };

interface FormActionBarProps {
  isDirty: boolean;
  isSubmitting: boolean;
  onCancel: () => void;
  onSubmit: () => void;
  submitLabel?: string;
  cancelLabel?: string;
}

export default function FormActionBar({
  isDirty,
  isSubmitting,
  onCancel,
  onSubmit,
  submitLabel = 'Save',
  cancelLabel = 'Cancel',
}: FormActionBarProps) {
  const theme = useTheme();

  // Block react-router navigations (link clicks, navigate(), browser Back)
  const blocker = useBlocker(
    ({ currentLocation, nextLocation }) =>
      isDirty && currentLocation.pathname !== nextLocation.pathname,
  );

  // Block browser tab-close / hard reload
  useEffect(() => {
    if (!isDirty) return;
    const handler = (e: BeforeUnloadEvent) => {
      e.preventDefault();
    };
    window.addEventListener('beforeunload', handler);
    return () => window.removeEventListener('beforeunload', handler);
  }, [isDirty]);

  return (
    <>
      <Box
        sx={{
          position: 'fixed',
          bottom: 0,
          left: 0,
          right: 0,
          height: FORM_ACTION_BAR_HEIGHT,
          bgcolor: alpha(theme.palette.background.paper, 0.92),
          backdropFilter: 'blur(8px)',
          borderTop: '1px solid',
          borderColor: 'divider',
          display: 'flex',
          alignItems: 'center',
          px: 3,
          gap: 2,
          zIndex: theme.zIndex.appBar,
        }}
      >
        {/* Left — unsaved changes indicator */}
        <Box sx={{ flex: 1 }}>
          {isDirty && (
            <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
              <Box
                sx={{
                  width: 7,
                  height: 7,
                  borderRadius: '50%',
                  bgcolor: 'warning.main',
                  flexShrink: 0,
                }}
              />
              <Typography variant="caption" color="warning.main" sx={{ fontWeight: 500 }}>
                Unsaved changes
              </Typography>
            </Stack>
          )}
        </Box>

        {/* Right — Cancel + Save */}
        <Stack direction="row" spacing={1.5}>
          <Button variant="outlined" onClick={onCancel} disabled={isSubmitting}>
            {cancelLabel}
          </Button>
          <Button
            variant="contained"
            onClick={onSubmit}
            disabled={isSubmitting}
            startIcon={
              isSubmitting ? <CircularProgress size={14} color="inherit" /> : undefined
            }
            sx={{ minWidth: 88 }}
          >
            {isSubmitting ? 'Saving…' : submitLabel}
          </Button>
        </Stack>
      </Box>

      {/* Navigation blocker dialog — shown instead of native browser confirm */}
      <ConfirmDialog
        open={blocker.state === 'blocked'}
        onClose={() => blocker.reset?.()}
        onConfirm={() => blocker.proceed?.()}
        title="Discard unsaved changes?"
        description="You have unsaved changes that will be lost if you leave this page."
        confirmLabel="Discard changes"
        confirmColor="warning"
      />
    </>
  );
}
