import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
} from '@mui/material';
import type { ReactNode } from 'react';
import SolarIcon from './SolarIcon';

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description?: ReactNode;
  confirmLabel?: string;
  confirmColor?: 'error' | 'warning' | 'primary';
  loading?: boolean;
}

export default function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel = 'Confirm',
  confirmColor = 'error',
  loading = false,
}: ConfirmDialogProps) {
  return (
    <Dialog
      open={open}
      onClose={loading ? undefined : onClose}
      maxWidth="xs"
      fullWidth
      slotProps={{ paper: { sx: { p: 0.5 } } }}
    >
      <DialogTitle sx={{ pb: 1 }}>
        <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
          <SolarIcon
            name={confirmColor === 'error' ? 'delete' : 'warning'}
            size={22}
            color={confirmColor === 'error' ? 'error.main' : 'warning.main'}
          />
          <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
            {title}
          </Typography>
        </Stack>
      </DialogTitle>

      {description && (
        <DialogContent sx={{ py: 1 }}>
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        </DialogContent>
      )}

      <DialogActions sx={{ px: 2, pb: 1.5, gap: 1 }}>
        <Button variant="outlined" size="small" onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <Button
          variant="contained"
          size="small"
          color={confirmColor}
          onClick={onConfirm}
          disabled={loading}
        >
          {confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
