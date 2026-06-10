import {
  Box,
  Button,
  CircularProgress,
  Divider,
  Drawer,
  IconButton,
  Stack,
  Typography,
} from '@mui/material';
import type { ReactNode } from 'react';
import SolarIcon from './SolarIcon';

// ─── FormDrawer ───────────────────────────────────────────────────────────────
//
//  Standard right-side drawer for Create / Edit forms.
//
//  Anatomy:
//  ┌─────────────────────────────────────┐
//  │ [✕]  Drawer Title          subtitle │  ← 56px header
//  ├─────────────────────────────────────┤
//  │                                     │
//  │  form content (scrollable)          │  ← flex: 1, overflow-y: auto
//  │                                     │
//  ├─────────────────────────────────────┤
//  │  [Cancel]           [Save / Submit] │  ← 60px footer, sticky
//  └─────────────────────────────────────┘
//
//  Usage:
//  <FormDrawer
//    open={open}
//    onClose={() => setOpen(false)}
//    title="New Work Order"
//    subtitle="Fill in the details below"
//    onSubmit={handleSubmit}
//    submitLabel="Create"
//    loading={isPending}
//  >
//    <RHF form fields...>
//  </FormDrawer>

interface FormDrawerProps {
  open: boolean;
  onClose: () => void;
  title: string;
  subtitle?: string;
  children: ReactNode;
  onSubmit?: () => void;
  submitLabel?: string;
  loading?: boolean;
  /** Width in px. Default: 480 */
  width?: number;
}

export default function FormDrawer({
  open,
  onClose,
  title,
  subtitle,
  children,
  onSubmit,
  submitLabel = 'Save',
  loading = false,
  width = 480,
}: FormDrawerProps) {
  return (
    <Drawer
      open={open}
      onClose={loading ? undefined : onClose}
      anchor="right"
      slotProps={{
        paper: {
          sx: {
            width: { xs: '100vw', sm: width },
            display: 'flex',
            flexDirection: 'column',
          },
        },
      }}
    >
      {/* Header */}
      <Stack
        direction="row"
        spacing={1.5}
        sx={{
          alignItems: 'center',
          px: 2.5,
          height: 56,
          flexShrink: 0,
          borderBottom: '1px solid',
          borderColor: 'divider',
        }}
      >
        <IconButton size="small" onClick={onClose} disabled={loading} sx={{ color: 'text.secondary' }}>
          <SolarIcon name="close" size={18} />
        </IconButton>
        <Divider orientation="vertical" flexItem sx={{ mx: 0.25 }} />
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography variant="subtitle2" noWrap>
            {title}
          </Typography>
          {subtitle && (
            <Typography variant="caption" color="text.secondary" noWrap sx={{ display: 'block' }}>
              {subtitle}
            </Typography>
          )}
        </Box>
      </Stack>

      {/* Scrollable form body */}
      <Box
        sx={{
          flex: 1,
          overflowY: 'auto',
          p: 2.5,
        }}
      >
        {children}
      </Box>

      {/* Footer */}
      {onSubmit && (
        <Stack
          direction="row"
          spacing={1.5}
          sx={{
            justifyContent: 'flex-end',
            px: 2.5,
            py: 1.5,
            flexShrink: 0,
            borderTop: '1px solid',
            borderColor: 'divider',
            bgcolor: 'background.paper',
          }}
        >
          <Button variant="outlined" size="small" onClick={onClose} disabled={loading}>
            Cancel
          </Button>
          <Button
            variant="contained"
            size="small"
            onClick={onSubmit}
            disabled={loading}
            startIcon={loading ? <CircularProgress size={14} color="inherit" /> : undefined}
          >
            {loading ? 'Saving…' : submitLabel}
          </Button>
        </Stack>
      )}
    </Drawer>
  );
}
