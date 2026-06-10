import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';

export type UserRole = 'Admin' | 'Manager' | 'Planner' | 'QualityEngineer' | 'Operator' | 'Viewer';

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  roles: UserRole[];
  avatarUrl?: string;
}

interface AuthState {
  user: AuthUser | null;
  token: string | null;
  isAuthenticated: boolean;
  hasRole: (role: UserRole | UserRole[]) => boolean;
  login: (user: AuthUser, token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

function loadInitialState(): { user: AuthUser | null; token: string | null } {
  try {
    const token = localStorage.getItem('accessToken');
    const raw = localStorage.getItem('authUser');
    const user: AuthUser | null = raw ? JSON.parse(raw) : null;
    return { token, user };
  } catch {
    return { token: null, user: null };
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState(loadInitialState);

  const login = useCallback((user: AuthUser, token: string) => {
    localStorage.setItem('accessToken', token);
    localStorage.setItem('authUser', JSON.stringify(user));
    setState({ user, token });
  }, []);

  const logout = useCallback(() => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      fetch('/api/v1/auth/logout', {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
        credentials: 'include',
      }).catch(() => {});
    }
    localStorage.removeItem('accessToken');
    localStorage.removeItem('authUser');
    setState({ user: null, token: null });
  }, []);

  const hasRole = useCallback(
    (role: UserRole | UserRole[]) => {
      if (!state.user) return false;
      const required = Array.isArray(role) ? role : [role];
      return required.some((r) => state.user!.roles.includes(r));
    },
    [state.user],
  );

  return (
    <AuthContext.Provider
      value={{
        user: state.user,
        token: state.token,
        isAuthenticated: !!state.token && !!state.user,
        hasRole,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}
