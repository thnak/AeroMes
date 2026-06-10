import { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  IconButton,
  Typography,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  Checkbox,
  Button,
  TextField,
  Stack,
  Alert,
  CircularProgress,
  Tooltip,
  Divider,
} from '@mui/material';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import PageHeader, { PageRoot } from '../../components/PageHeader';
import SolarIcon from '../../components/SolarIcon';
import ConfirmDialog from '../../components/ConfirmDialog';
import {
  useGetApiV1Roles,
  useGetApiV1RolesIdPermissions,
  getGetApiV1RolesQueryKey,
  getGetApiV1RolesIdPermissionsQueryKey,
  postApiV1Roles,
  deleteApiV1RolesId,
  putApiV1RolesIdPermissions,
} from '../../api/roles/roles';
import { useGetApiV1AuthPermissions } from '../../api/permissions/permissions';
import type { RoleDto } from '../../api/model/roleDto';
import type { PermissionDto } from '../../api/model/permissionDto';
import { getErrorMessage } from '../../lib/apiClient';

// ─── Permission Matrix ────────────────────────────────────────────────────────

interface PermissionMatrixProps {
  allPermissions: PermissionDto[];
  pendingPermCodes: Set<string>;
  onToggle: (code: string) => void;
}

function PermissionMatrix({ allPermissions, pendingPermCodes, onToggle }: PermissionMatrixProps) {
  const resources = [...new Set(allPermissions.map((p) => p.resource).filter(Boolean))] as string[];
  const actions = [...new Set(allPermissions.map((p) => p.action).filter(Boolean))] as string[];

  if (resources.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
        No permissions defined.
      </Typography>
    );
  }

  return (
    <Box sx={{ overflowX: 'auto' }}>
      <Table size="small" stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: 600, minWidth: 140 }}>Resource</TableCell>
            {actions.map((action) => (
              <TableCell key={action} align="center" sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>
                {action}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {resources.map((resource) => (
            <TableRow key={resource} hover>
              <TableCell sx={{ fontWeight: 500 }}>{resource}</TableCell>
              {actions.map((action) => {
                const perm = allPermissions.find(
                  (p) => p.resource === resource && p.action === action,
                );
                if (!perm) {
                  return <TableCell key={action} align="center" />;
                }
                const code = perm.permissionCode ?? '';
                const checked = pendingPermCodes.has(code);
                return (
                  <TableCell key={action} align="center" sx={{ py: 0.5 }}>
                    <Tooltip
                      title={perm.description ?? code}
                      placement="top"
                      disableHoverListener={!perm.description && !code}
                    >
                      <Checkbox
                        size="small"
                        checked={checked}
                        onChange={() => onToggle(code)}
                        disabled={!code}
                      />
                    </Tooltip>
                  </TableCell>
                );
              })}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
}

// ─── Right Panel ──────────────────────────────────────────────────────────────

interface RightPanelProps {
  selectedRole: RoleDto | null;
}

function RightPanel({ selectedRole }: RightPanelProps) {
  const queryClient = useQueryClient();
  const [pendingPermCodes, setPendingPermCodes] = useState<Set<string>>(new Set());
  const [saveError, setSaveError] = useState<string | null>(null);
  const [saveSuccess, setSaveSuccess] = useState(false);

  const roleId = selectedRole?.id ?? '';

  const {
    data: rolePermissions,
    isLoading: loadingRolePerms,
    error: rolePermsError,
  } = useGetApiV1RolesIdPermissions(roleId, {
    query: { enabled: !!roleId },
  });

  const { data: allPermissions, isLoading: loadingAllPerms } = useGetApiV1AuthPermissions();

  useEffect(() => {
    if (rolePermissions) {
      setPendingPermCodes(
        new Set(rolePermissions.map((p) => p.permissionCode ?? '').filter(Boolean)),
      );
    } else {
      setPendingPermCodes(new Set());
    }
    setSaveError(null);
    setSaveSuccess(false);
  }, [rolePermissions, roleId]);

  const saveMutation = useMutation({
    mutationFn: () =>
      putApiV1RolesIdPermissions(roleId, { permissionCodes: [...pendingPermCodes] }),
    onSuccess: () => {
      setSaveError(null);
      setSaveSuccess(true);
      void queryClient.invalidateQueries({
        queryKey: getGetApiV1RolesIdPermissionsQueryKey(roleId),
      });
      setTimeout(() => setSaveSuccess(false), 3000);
    },
    onError: (err) => {
      setSaveError(getErrorMessage(err));
      setSaveSuccess(false);
    },
  });

  const handleToggle = (code: string) => {
    setPendingPermCodes((prev) => {
      const next = new Set(prev);
      if (next.has(code)) {
        next.delete(code);
      } else {
        next.add(code);
      }
      return next;
    });
    setSaveSuccess(false);
  };

  if (!selectedRole) {
    return (
      <Box
        sx={{
          flex: 1,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          flexDirection: 'column',
          gap: 2,
          color: 'text.disabled',
        }}
      >
        <SolarIcon name="admin" size={48} />
        <Typography variant="body1" color="text.secondary">
          Select a role to manage permissions
        </Typography>
      </Box>
    );
  }

  const isLoading = loadingRolePerms || loadingAllPerms;

  return (
    <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
      <Stack direction="row" sx={{ alignItems: 'center', mb: 2 }} spacing={1}>
        <SolarIcon name="quality" size={20} color="primary.main" />
        <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
          Permissions — {selectedRole.name}
        </Typography>
      </Stack>

      {saveError && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setSaveError(null)}>
          {saveError}
        </Alert>
      )}
      {saveSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Permissions saved successfully.
        </Alert>
      )}
      {rolePermsError && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {getErrorMessage(rolePermsError, 'Failed to load role permissions.')}
        </Alert>
      )}

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', pt: 6 }}>
          <CircularProgress size={32} />
        </Box>
      ) : (
        <>
          <PermissionMatrix
            allPermissions={allPermissions ?? []}
            pendingPermCodes={pendingPermCodes}
            onToggle={handleToggle}
          />

          <Box sx={{ mt: 3 }}>
            <Button
              variant="contained"
              size="small"
              disabled={saveMutation.isPending}
              startIcon={
                saveMutation.isPending ? (
                  <CircularProgress size={14} color="inherit" />
                ) : (
                  <SolarIcon name="success" size={16} />
                )
              }
              onClick={() => saveMutation.mutate()}
            >
              Save permissions
            </Button>
          </Box>
        </>
      )}
    </Box>
  );
}

// ─── RolesPage ────────────────────────────────────────────────────────────────

export default function RolesPage() {
  const queryClient = useQueryClient();

  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null);
  const [showNewRoleInput, setShowNewRoleInput] = useState(false);
  const [newRoleName, setNewRoleName] = useState('');
  const [createError, setCreateError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<RoleDto | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { data: roles, isLoading: loadingRoles } = useGetApiV1Roles();

  const selectedRole = roles?.find((r) => r.id === selectedRoleId) ?? null;

  const createMutation = useMutation({
    mutationFn: (name: string) => postApiV1Roles({ name }),
    onSuccess: () => {
      setShowNewRoleInput(false);
      setNewRoleName('');
      setCreateError(null);
      void queryClient.invalidateQueries({ queryKey: getGetApiV1RolesQueryKey() });
    },
    onError: (err) => {
      setCreateError(getErrorMessage(err));
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteApiV1RolesId(id),
    onSuccess: (_data, deletedId) => {
      setDeleteTarget(null);
      setDeleteError(null);
      if (selectedRoleId === deletedId) setSelectedRoleId(null);
      void queryClient.invalidateQueries({ queryKey: getGetApiV1RolesQueryKey() });
    },
    onError: (err) => {
      setDeleteError(getErrorMessage(err));
    },
  });

  const handleCreateConfirm = () => {
    const name = newRoleName.trim();
    if (!name) return;
    createMutation.mutate(name);
  };

  const handleCreateCancel = () => {
    setShowNewRoleInput(false);
    setNewRoleName('');
    setCreateError(null);
  };

  return (
    <PageRoot>
      <PageHeader
        title="Roles & Permissions"
        subtitle="Manage system roles and their permission assignments"
        breadcrumbs={[{ label: 'Admin' }, { label: 'Roles' }]}
      />

      <Box sx={{ display: 'flex', gap: 2, flex: 1, minHeight: 0 }}>
        {/* Left Panel — 280px */}
        <Paper
          variant="outlined"
          sx={{
            width: 280,
            flexShrink: 0,
            display: 'flex',
            flexDirection: 'column',
            overflow: 'hidden',
          }}
        >
          {/* Header */}
          <Stack
            direction="row"
            sx={{ alignItems: 'center', justifyContent: 'space-between', px: 2, py: 1.5 }}
          >
            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
              Roles
            </Typography>
            <Tooltip title="New role">
              <span>
                <IconButton
                  size="small"
                  onClick={() => {
                    setShowNewRoleInput(true);
                    setCreateError(null);
                  }}
                  disabled={showNewRoleInput}
                >
                  <SolarIcon name="add" size={18} />
                </IconButton>
              </span>
            </Tooltip>
          </Stack>

          <Divider />

          {/* Inline create form */}
          {showNewRoleInput && (
            <Box sx={{ px: 2, py: 1.5, borderBottom: '1px solid', borderColor: 'divider' }}>
              <TextField
                autoFocus
                size="small"
                fullWidth
                placeholder="Role name"
                value={newRoleName}
                onChange={(e) => setNewRoleName(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') handleCreateConfirm();
                  if (e.key === 'Escape') handleCreateCancel();
                }}
                error={!!createError}
                helperText={createError ?? undefined}
                disabled={createMutation.isPending}
                sx={{ mb: 1 }}
              />
              <Stack direction="row" spacing={1}>
                <Button
                  size="small"
                  variant="contained"
                  disabled={!newRoleName.trim() || createMutation.isPending}
                  onClick={handleCreateConfirm}
                  startIcon={
                    createMutation.isPending ? (
                      <CircularProgress size={12} color="inherit" />
                    ) : undefined
                  }
                >
                  Add
                </Button>
                <Button
                  size="small"
                  variant="outlined"
                  onClick={handleCreateCancel}
                  disabled={createMutation.isPending}
                >
                  Cancel
                </Button>
              </Stack>
            </Box>
          )}

          {/* Role list */}
          <Box sx={{ flex: 1, overflowY: 'auto' }}>
            {loadingRoles ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', pt: 4 }}>
                <CircularProgress size={24} />
              </Box>
            ) : !roles || roles.length === 0 ? (
              <Box sx={{ px: 2, py: 3, textAlign: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  No roles yet.
                </Typography>
              </Box>
            ) : (
              <List dense disablePadding>
                {roles.map((role) => (
                  <ListItem
                    key={role.id}
                    disablePadding
                    secondaryAction={
                      <Tooltip title="Delete role">
                        <IconButton
                          edge="end"
                          size="small"
                          color="error"
                          onClick={(e) => {
                            e.stopPropagation();
                            setDeleteTarget(role);
                            setDeleteError(null);
                          }}
                        >
                          <SolarIcon name="delete" size={16} />
                        </IconButton>
                      </Tooltip>
                    }
                  >
                    <ListItemButton
                      selected={selectedRoleId === role.id}
                      onClick={() => setSelectedRoleId(role.id ?? null)}
                      sx={{ pr: 6 }}
                    >
                      <ListItemText
                        primary={role.name}
                        slotProps={{ primary: { variant: 'body2' } }}
                      />
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
            )}
          </Box>
        </Paper>

        {/* Right Panel */}
        <Paper
          variant="outlined"
          sx={{
            flex: 1,
            display: 'flex',
            flexDirection: 'column',
            p: 3,
            minWidth: 0,
            overflowY: 'auto',
          }}
        >
          <RightPanel selectedRole={selectedRole} />
        </Paper>
      </Box>

      {/* Delete confirm dialog */}
      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => {
          if (!deleteMutation.isPending) {
            setDeleteTarget(null);
            setDeleteError(null);
          }
        }}
        onConfirm={() => {
          if (deleteTarget?.id) deleteMutation.mutate(deleteTarget.id);
        }}
        title={`Delete role "${deleteTarget?.name}"?`}
        description={
          deleteError ? (
            <Alert severity="error" sx={{ mt: 1 }}>
              {deleteError}
            </Alert>
          ) : (
            'This will permanently remove the role and all its permission assignments. Users currently assigned this role will lose these permissions.'
          )
        }
        confirmLabel="Delete"
        confirmColor="error"
        loading={deleteMutation.isPending}
      />
    </PageRoot>
  );
}
