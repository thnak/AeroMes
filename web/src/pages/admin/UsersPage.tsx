import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQueryClient, useMutation } from '@tanstack/react-query';
import { Link as RouterLink } from 'react-router-dom';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import {
  Box,
  Button,
  Chip,
  Drawer,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Checkbox,
  ListItemText,
  Stack,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  IconButton,
  Tooltip,
  ToggleButtonGroup,
  ToggleButton,
  Link,
} from '@mui/material';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import {
  useGetApiV1Users,
  postApiV1Users,
  getGetApiV1UsersQueryKey,
} from '../../api/users/users';
import { useGetApiV1Roles } from '../../api/roles/roles';
import type { RoleDto } from '../../api/model/roleDto';
import type { UserSummaryDto } from '../../api/model/userSummaryDto';
import type { CreateUserRequest } from '../../api/model/createUserRequest';
import PageHeader, { PageRoot } from '../../components/PageHeader';
import SolarIcon from '../../components/SolarIcon';
import { getErrorMessage } from '../../lib/apiClient';

dayjs.extend(relativeTime);

// ---------------------------------------------------------------------------
// Zod schema
// ---------------------------------------------------------------------------

const createUserSchema = z.object({
  email: z.string().email('Invalid email address'),
  fullName: z.string().min(1, 'Full name is required'),
  department: z.string().optional(),
  employeeCode: z.string().optional(),
  roles: z.array(z.string()).optional(),
});

type CreateUserFormValues = z.infer<typeof createUserSchema>;

// ---------------------------------------------------------------------------
// TempPasswordDialog
// ---------------------------------------------------------------------------

interface TempPasswordDialogProps {
  open: boolean;
  tempPassword: string;
  onClose: () => void;
}

function TempPasswordDialog({ open, tempPassword, onClose }: TempPasswordDialogProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(tempPassword);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // clipboard not available
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>User Created</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <Typography variant="body2" color="text.secondary">
            The user has been created. Share this temporary password with them — it will not be
            shown again.
          </Typography>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              p: 2,
              borderRadius: 1,
              bgcolor: 'action.hover',
              fontFamily: 'monospace',
              fontSize: '1.1rem',
              fontWeight: 700,
              letterSpacing: 1,
            }}
          >
            <Box sx={{ flex: 1, wordBreak: 'break-all' }}>{tempPassword}</Box>
            <Tooltip title={copied ? 'Copied!' : 'Copy to clipboard'}>
              <IconButton size="small" onClick={handleCopy}>
                <SolarIcon name={copied ? 'complete' : 'copy'} size={20} />
              </IconButton>
            </Tooltip>
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} variant="contained">
          Done
        </Button>
      </DialogActions>
    </Dialog>
  );
}

// ---------------------------------------------------------------------------
// CreateUserDrawer
// ---------------------------------------------------------------------------

interface CreateUserDrawerProps {
  open: boolean;
  onClose: () => void;
  onCreated: (tempPassword: string) => void;
  roleOptions: RoleDto[];
}

function CreateUserDrawer({ open, onClose, onCreated, roleOptions }: CreateUserDrawerProps) {
  const queryClient = useQueryClient();
  const { mutate, isPending, error, reset } = useMutation({
    mutationFn: (req: CreateUserRequest) => postApiV1Users(req),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: getGetApiV1UsersQueryKey() });
      onCreated(data.tempPassword ?? '');
      onClose();
      formReset();
    },
  });

  const {
    control,
    handleSubmit,
    reset: formReset,
    formState: { errors },
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: {
      email: '',
      fullName: '',
      department: '',
      employeeCode: '',
      roles: [],
    },
  });

  const onSubmit = (values: CreateUserFormValues) => {
    const payload: CreateUserRequest = {
      email: values.email,
      fullName: values.fullName,
      department: values.department || null,
      employeeCode: values.employeeCode || null,
      roles: values.roles && values.roles.length > 0 ? values.roles : null,
    };
    mutate(payload);
  };

  const handleClose = () => {
    onClose();
    formReset();
    reset(); // clear mutation error state
  };

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={handleClose}
      slotProps={{ paper: { sx: { width: 400, p: 3 } } }}
    >
      <Stack spacing={3} component="form" onSubmit={handleSubmit(onSubmit)} sx={{ height: '100%' }}>
        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            New User
          </Typography>
          <IconButton size="small" onClick={handleClose}>
            <SolarIcon name="close" size={20} />
          </IconButton>
        </Stack>

        {error && (
          <Alert severity="error" onClose={reset}>
            {getErrorMessage(error)}
          </Alert>
        )}

        <Controller
          name="fullName"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Full Name"
              fullWidth
              required
              error={!!errors.fullName}
              helperText={errors.fullName?.message}
            />
          )}
        />

        <Controller
          name="email"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Email"
              type="email"
              fullWidth
              required
              error={!!errors.email}
              helperText={errors.email?.message}
            />
          )}
        />

        <Controller
          name="department"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Department"
              fullWidth
              error={!!errors.department}
              helperText={errors.department?.message}
            />
          )}
        />

        <Controller
          name="employeeCode"
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label="Employee Code"
              fullWidth
              error={!!errors.employeeCode}
              helperText={errors.employeeCode?.message}
            />
          )}
        />

        <Controller
          name="roles"
          control={control}
          render={({ field }) => (
            <FormControl fullWidth>
              <InputLabel>Roles</InputLabel>
              <Select
                {...field}
                multiple
                label="Roles"
                value={field.value ?? []}
                renderValue={(selected) =>
                  (selected as string[])
                    .map((id) => roleOptions.find((r) => r.id === id)?.name ?? id)
                    .join(', ')
                }
              >
                {roleOptions.map((role) => (
                  <MenuItem key={role.id} value={role.id}>
                    <Checkbox checked={(field.value ?? []).includes(role.id)} />
                    <ListItemText primary={role.name ?? role.id} />
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
        />

        <Box sx={{ flex: 1 }} />

        <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
          <Button variant="outlined" onClick={handleClose} disabled={isPending}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" loading={isPending}>
            Create User
          </Button>
        </Stack>
      </Stack>
    </Drawer>
  );
}

// ---------------------------------------------------------------------------
// UsersPage
// ---------------------------------------------------------------------------

type ActiveFilter = 'all' | 'active' | 'inactive';

export default function UsersPage() {
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 20 });
  const [searchText, setSearchText] = useState('');
  const [roleFilter, setRoleFilter] = useState<string>('');
  const [activeFilter, setActiveFilter] = useState<ActiveFilter>('all');
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [tempPassword, setTempPassword] = useState<string | null>(null);

  const isActiveParam =
    activeFilter === 'active' ? true : activeFilter === 'inactive' ? false : undefined;

  const { data, isLoading } = useGetApiV1Users({
    role: roleFilter || undefined,
    isActive: isActiveParam,
    page: paginationModel.page + 1,
    pageSize: paginationModel.pageSize,
  });

  const { data: rolesData } = useGetApiV1Roles();
  const roleOptions = rolesData ?? [];

  // Client-side search filter on the current page
  const filteredRows: UserSummaryDto[] = (data?.items ?? []).filter((row) => {
    if (!searchText) return true;
    const lower = searchText.toLowerCase();
    return (
      (row.fullName ?? '').toLowerCase().includes(lower) ||
      (row.email ?? '').toLowerCase().includes(lower)
    );
  });

  const columns: GridColDef<UserSummaryDto>[] = [
    {
      field: 'fullName',
      headerName: 'Full Name',
      flex: 1,
    },
    {
      field: 'email',
      headerName: 'Email',
      flex: 1,
    },
    {
      field: 'department',
      headerName: 'Department',
      flex: 0.7,
    },
    {
      field: 'isActive',
      headerName: 'Status',
      flex: 0.5,
      renderCell: ({ value }) => (
        <Chip
          size="small"
          color={value ? 'success' : 'default'}
          label={value ? 'Active' : 'Inactive'}
        />
      ),
    },
    {
      field: 'lastLoginAt',
      headerName: 'Last Login',
      flex: 0.7,
      renderCell: ({ value }) =>
        value ? (
          <Tooltip title={dayjs(value as string).format('YYYY-MM-DD HH:mm')}>
            <span>{dayjs(value as string).fromNow()}</span>
          </Tooltip>
        ) : (
          <Typography variant="body2" color="text.disabled">
            Never
          </Typography>
        ),
    },
    {
      field: 'actions',
      headerName: '',
      width: 80,
      sortable: false,
      filterable: false,
      renderCell: ({ row }) => (
        <Link component={RouterLink} to={`/admin/users/${row.id}`} underline="hover">
          View
        </Link>
      ),
    },
  ];

  return (
    <PageRoot>
      <PageHeader
        title="Users"
        subtitle="Manage system user accounts and access"
        actions={
          <Button
            variant="contained"
            startIcon={<SolarIcon name="add" size={18} />}
            onClick={() => setDrawerOpen(true)}
          >
            New User
          </Button>
        }
      />

      {/* Toolbar */}
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={2}
        sx={{ mb: 2, alignItems: { sm: 'center' } }}
      >
        <TextField
          size="small"
          placeholder="Search name or email…"
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          slotProps={{
            input: {
              startAdornment: (
                <Box sx={{ mr: 0.5, display: 'flex', color: 'text.secondary' }}>
                  <SolarIcon name="search" size={18} />
                </Box>
              ),
            },
          }}
          sx={{ minWidth: 240 }}
        />

        <FormControl size="small" sx={{ minWidth: 160 }}>
          <InputLabel>Role</InputLabel>
          <Select
            value={roleFilter}
            label="Role"
            onChange={(e) => {
              setRoleFilter(e.target.value as string);
              setPaginationModel((prev) => ({ ...prev, page: 0 }));
            }}
          >
            <MenuItem value="">All Roles</MenuItem>
            {roleOptions.map((role) => (
              <MenuItem key={role.id} value={role.name ?? role.id}>
                {role.name ?? role.id}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <ToggleButtonGroup
          size="small"
          exclusive
          value={activeFilter}
          onChange={(_, val) => {
            if (val !== null) {
              setActiveFilter(val as ActiveFilter);
              setPaginationModel((prev) => ({ ...prev, page: 0 }));
            }
          }}
        >
          <ToggleButton value="all">All</ToggleButton>
          <ToggleButton value="active">Active</ToggleButton>
          <ToggleButton value="inactive">Inactive</ToggleButton>
        </ToggleButtonGroup>
      </Stack>

      {/* DataGrid */}
      <Box sx={{ flex: 1, minHeight: 0 }}>
        <DataGrid
          rows={filteredRows}
          columns={columns}
          rowCount={Number(data?.total ?? 0)}
          paginationMode="server"
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          loading={isLoading}
          getRowId={(row) => row.id}
          disableRowSelectionOnClick
          pageSizeOptions={[20, 50]}
          sx={{ border: 0 }}
        />
      </Box>

      {/* Create User Drawer */}
      <CreateUserDrawer
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        onCreated={(pw) => setTempPassword(pw)}
        roleOptions={roleOptions}
      />

      {/* Temp Password Dialog */}
      {tempPassword !== null && (
        <TempPasswordDialog
          open
          tempPassword={tempPassword}
          onClose={() => setTempPassword(null)}
        />
      )}
    </PageRoot>
  );
}
