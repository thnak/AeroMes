import { useRef } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import { QRCodeSVG } from 'qrcode.react';
import SolarIcon from './SolarIcon';

interface Props {
  open: boolean;
  onClose: () => void;
  /** The text / URL to encode in the QR code */
  value: string;
  /** Label shown below the QR code */
  label?: string;
  /** Dialog title (default: "QR Code") */
  title?: string;
}

export default function QrCodeDialog({ open, onClose, value, label, title = 'QR Code' }: Props) {
  const svgRef = useRef<SVGSVGElement>(null);

  function handleDownload() {
    const svg = svgRef.current;
    if (!svg) return;
    const blob = new Blob([svg.outerHTML], { type: 'image/svg+xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `qr-${(label ?? 'code').replace(/\s+/g, '-')}.svg`;
    a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        {title}
        <IconButton size="small" onClick={onClose}>
          <SolarIcon name="close" size={18} />
        </IconButton>
      </DialogTitle>

      <DialogContent>
        <Stack sx={{ alignItems: 'center', gap: 2 }}>
          <Box
            sx={{
              p: 2,
              bgcolor: '#fff',
              borderRadius: 2,
              border: '1px solid',
              borderColor: 'divider',
              display: 'inline-flex',
            }}
          >
            <QRCodeSVG
              ref={svgRef}
              value={value}
              size={200}
              level="M"
              includeMargin={false}
            />
          </Box>

          {label && (
            <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 700, textAlign: 'center' }}>
              {label}
            </Typography>
          )}

          <Tooltip title="Copy link">
            <Typography
              variant="caption"
              color="text.secondary"
              sx={{
                maxWidth: 260,
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
                cursor: 'pointer',
              }}
              onClick={() => navigator.clipboard.writeText(value)}
            >
              {value}
            </Typography>
          </Tooltip>
        </Stack>
      </DialogContent>

      <DialogActions>
        <Button onClick={handleDownload} startIcon={<SolarIcon name="download" size={16} />}>
          Download SVG
        </Button>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
