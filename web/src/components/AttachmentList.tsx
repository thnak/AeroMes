import {
  Box,
  Chip,
  IconButton,
  ImageList,
  ImageListItem,
  ImageListItemBar,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Skeleton,
  Tooltip,
  Typography,
} from '@mui/material';
import { SolarIcon } from './SolarIcon';
import { useGetApiV1Files, deleteApiV1FilesId, getGetApiV1FilesQueryKey } from '../api/files/files';
import type { FileObjectDto } from '../api/model';
import { useQueryClient } from '@tanstack/react-query';

function formatBytes(bytes: number | string) {
  const n = typeof bytes === 'string' ? parseInt(bytes, 10) : bytes;
  if (n < 1024) return `${n} B`;
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`;
  return `${(n / (1024 * 1024)).toFixed(1)} MB`;
}

function isImage(contentType: string) {
  return contentType.startsWith('image/');
}

interface Props {
  ownerType: string;
  ownerId: string;
  canDelete?: boolean;
  layout?: 'list' | 'gallery';
}

export default function AttachmentList({ ownerType, ownerId, canDelete, layout = 'list' }: Props) {
  const qc = useQueryClient();
  const { data, isLoading } = useGetApiV1Files(
    { ownerType, ownerId },
    { query: { enabled: !!ownerId } }
  );

  const handleDelete = async (file: FileObjectDto) => {
    await deleteApiV1FilesId(file.id);
    qc.invalidateQueries({ queryKey: getGetApiV1FilesQueryKey({ ownerType, ownerId }) });
  };

  if (isLoading) return <Skeleton variant="rectangular" height={80} />;
  if (!data?.length) {
    return (
      <Typography variant="body2" color="text.secondary" sx={{ py: 1 }}>
        No attachments
      </Typography>
    );
  }

  if (layout === 'gallery') {
    const images = data.filter(f => isImage(f.contentType));
    const docs = data.filter(f => !isImage(f.contentType));
    return (
      <Box>
        {images.length > 0 && (
          <ImageList cols={4} rowHeight={120} gap={8}>
            {images.map(file => (
              <ImageListItem key={file.id}>
                <img
                  src={file.thumbnailUrl ?? file.downloadUrl}
                  alt={file.fileName}
                  loading="lazy"
                  style={{ objectFit: 'cover', width: '100%', height: '100%' }}
                />
                <ImageListItemBar
                  title={file.fileName}
                  actionIcon={
                    canDelete ? (
                      <Tooltip title="Delete">
                        <IconButton size="small" onClick={() => handleDelete(file)} sx={{ color: 'white' }}>
                          <SolarIcon name="delete" size={16} />
                        </IconButton>
                      </Tooltip>
                    ) : undefined
                  }
                />
              </ImageListItem>
            ))}
          </ImageList>
        )}
        {docs.length > 0 && (
          <DocList files={docs} canDelete={canDelete} onDelete={handleDelete} />
        )}
      </Box>
    );
  }

  return <DocList files={data} canDelete={canDelete} onDelete={handleDelete} />;
}

function DocList({
  files, canDelete, onDelete,
}: {
  files: FileObjectDto[];
  canDelete?: boolean;
  onDelete: (file: FileObjectDto) => void;
}) {
  return (
    <List dense disablePadding>
      {files.map(file => (
        <ListItem
          key={file.id}
          disableGutters
          secondaryAction={
            <Box sx={{ display: 'flex', gap: 0.5 }}>
              <Tooltip title="Download">
                <IconButton
                  size="small"
                  component="a"
                  href={file.downloadUrl}
                  target="_blank"
                  rel="noreferrer"
                >
                  <SolarIcon name="download" size={16} />
                </IconButton>
              </Tooltip>
              {canDelete && (
                <Tooltip title="Delete">
                  <IconButton size="small" color="error" onClick={() => onDelete(file)}>
                    <SolarIcon name="delete" size={16} />
                  </IconButton>
                </Tooltip>
              )}
            </Box>
          }
        >
          <ListItemIcon sx={{ minWidth: 32 }}>
            <SolarIcon name={isImage(file.contentType) ? 'view' : 'order'} size={18} />
          </ListItemIcon>
          <ListItemText
            primary={file.fileName}
            secondary={
              <Box component="span" sx={{ display: 'flex', gap: 0.5, alignItems: 'center', mt: 0.25 }}>
                <Chip label={formatBytes(file.sizeBytes)} size="small" />
                <Typography variant="caption" color="text.disabled">
                  {new Date(file.uploadedAt).toLocaleDateString()}
                </Typography>
              </Box>
            }
          />
        </ListItem>
      ))}
    </List>
  );
}
