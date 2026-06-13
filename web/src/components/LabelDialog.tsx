import { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material';
import SolarIcon from './SolarIcon';
import { apiClient } from '../lib/apiClient';

type Format = 'compact' | 'composite';
type Output = 'png' | 'pdf' | 'zpl';

interface Props {
  open: boolean;
  onClose: () => void;
  contentType: string;
  entityId: string;
  entityLabel?: string;
}

export default function LabelDialog({ open, onClose, contentType, entityId, entityLabel }: Props) {
  const [format, setFormat] = useState<Format>('compact');
  const [output, setOutput] = useState<Output>('png');
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const buildUrl = (out: Output) =>
    `/api/v1/labels/render/${contentType}/${entityId}?format=${format}&output=${out}`;

  const loadPreview = async () => {
    setLoading(true);
    setError(null);
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    try {
      const res = await apiClient.get<Blob>(buildUrl('png'), { responseType: 'blob' });
      setPreviewUrl(URL.createObjectURL(res.data));
    } catch {
      setError('Preview failed. Try a different format or check the entity ID.');
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiClient.get<Blob>(buildUrl(output), { responseType: 'blob' });
      const url  = URL.createObjectURL(res.data);
      const a    = document.createElement('a');
      a.href = url;
      a.download = `label_${contentType}_${entityId}.${output}`;
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      setError('Download failed.');
    } finally {
      setLoading(false);
    }
  };

  const handlePrint = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiClient.get<Blob>(buildUrl('pdf'), { responseType: 'blob' });
      const url = URL.createObjectURL(res.data);
      const win = window.open(url);
      if (win) win.onload = () => { win.print(); URL.revokeObjectURL(url); };
    } catch {
      setError('Print failed.');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    setPreviewUrl(null);
    setError(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        Print Label — {entityLabel ?? entityId}
        <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
          ({contentType})
        </Typography>
      </DialogTitle>

      <DialogContent>
        <Stack spacing={2.5}>
          {/* Template toggle */}
          <Box>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
              Layout
            </Typography>
            <ToggleButtonGroup
              value={format}
              exclusive
              onChange={(_, v) => v && setFormat(v)}
              size="small"
            >
              <ToggleButton value="compact">Compact</ToggleButton>
              <ToggleButton value="composite">Composite</ToggleButton>
            </ToggleButtonGroup>
          </Box>

          {/* Output format */}
          <FormControl size="small" sx={{ width: 160 }}>
            <InputLabel>Download as</InputLabel>
            <Select
              value={output}
              label="Download as"
              onChange={(e) => setOutput(e.target.value as Output)}
            >
              <MenuItem value="png">PNG Image</MenuItem>
              <MenuItem value="pdf">PDF Document</MenuItem>
              <MenuItem value="zpl">ZPL (Zebra)</MenuItem>
            </Select>
          </FormControl>

          {/* Preview */}
          <Button
            variant="outlined"
            size="small"
            startIcon={<SolarIcon name="view" size={16} />}
            onClick={loadPreview}
            disabled={loading}
            sx={{ alignSelf: 'flex-start' }}
          >
            Preview
          </Button>

          {previewUrl && (
            <Box
              sx={{
                border: '1px solid',
                borderColor: 'divider',
                borderRadius: 1,
                p: 1,
                textAlign: 'center',
                bgcolor: 'background.default',
              }}
            >
              <img
                src={previewUrl}
                alt="Label preview"
                style={{ maxWidth: '100%', maxHeight: 200, objectFit: 'contain' }}
              />
            </Box>
          )}

          {error && (
            <Typography variant="caption" color="error">
              {error}
            </Typography>
          )}
        </Stack>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2, gap: 1 }}>
        <Button onClick={handleClose} variant="outlined" size="small">
          Close
        </Button>
        <Button
          onClick={handleDownload}
          variant="outlined"
          size="small"
          startIcon={<SolarIcon name="download" size={16} />}
          disabled={loading}
        >
          Download {output.toUpperCase()}
        </Button>
        <Button
          onClick={handlePrint}
          variant="contained"
          size="small"
          startIcon={<SolarIcon name="order" size={16} />}
          disabled={loading}
          loading={loading}
        >
          Print
        </Button>
      </DialogActions>
    </Dialog>
  );
}
