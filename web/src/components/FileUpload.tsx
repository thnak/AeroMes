import { useCallback, useRef, useState } from 'react';
import {
  Box,
  Button,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material';
import SolarIcon from './SolarIcon';
import { apiClient } from '../lib/apiClient';
import type { FileUploadResult } from '../api/model';

const ACCEPTED_TYPES = [
  'image/jpeg', 'image/png', 'image/webp', 'image/gif',
  'application/pdf',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  'text/csv', 'text/plain',
];
const MAX_SIZE = 50 * 1024 * 1024;

interface Props {
  ownerType: string;
  ownerId: string;
  onUploaded: (result: FileUploadResult) => void;
  disabled?: boolean;
}

export default function FileUpload({ ownerType, ownerId, onUploaded, disabled }: Props) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const upload = useCallback(async (file: File) => {
    if (!ACCEPTED_TYPES.includes(file.type)) {
      setError(`File type ${file.type} not allowed.`);
      return;
    }
    if (file.size > MAX_SIZE) {
      setError('File exceeds 50 MB limit.');
      return;
    }
    setError(null);
    setUploading(true);
    try {
      const form = new FormData();
      form.append('file', file);
      form.append('ownerType', ownerType);
      form.append('ownerId', ownerId);
      const res = await apiClient.post<FileUploadResult>('/api/v1/files', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      onUploaded(res.data);
    } catch {
      setError('Upload failed. Please try again.');
    } finally {
      setUploading(false);
    }
  }, [ownerType, ownerId, onUploaded]);

  const onDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragging(false);
    const file = e.dataTransfer.files[0];
    if (file) upload(file);
  }, [upload]);

  return (
    <Box>
      <Box
        onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
        onDragLeave={() => setDragging(false)}
        onDrop={onDrop}
        onClick={() => !disabled && inputRef.current?.click()}
        sx={{
          border: '2px dashed',
          borderColor: dragging ? 'primary.main' : 'divider',
          borderRadius: 2,
          p: 3,
          textAlign: 'center',
          cursor: disabled ? 'not-allowed' : 'pointer',
          transition: 'border-color 0.2s',
          bgcolor: dragging ? 'action.hover' : 'transparent',
          '&:hover': disabled ? {} : { borderColor: 'primary.light', bgcolor: 'action.hover' },
        }}
      >
        <SolarIcon name="upload" size={32} color="action" />
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Drag & drop a file here, or{' '}
          <Typography component="span" color="primary" sx={{ fontWeight: 600 }}>
            browse
          </Typography>
        </Typography>
        <Typography variant="caption" color="text.disabled">
          Images, PDF, Office docs, CSV — up to 50 MB
        </Typography>
      </Box>

      <input
        ref={inputRef}
        type="file"
        hidden
        accept={ACCEPTED_TYPES.join(',')}
        onChange={(e) => {
          const file = e.target.files?.[0];
          if (file) upload(file);
          e.target.value = '';
        }}
        disabled={disabled}
      />

      {uploading && <LinearProgress sx={{ mt: 1 }} />}
      {error && (
        <Typography variant="caption" color="error" sx={{ mt: 0.5, display: 'block' }}>
          {error}
        </Typography>
      )}

      <Stack direction="row" sx={{ justifyContent: 'flex-end', mt: 1 }}>
        <Button
          size="small"
          startIcon={<SolarIcon name="upload" size={16} />}
          onClick={() => inputRef.current?.click()}
          disabled={disabled || uploading}
        >
          Upload file
        </Button>
      </Stack>
    </Box>
  );
}
