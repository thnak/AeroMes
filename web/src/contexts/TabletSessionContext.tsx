import { createContext, useContext, useState, type ReactNode } from 'react';

export interface TabletSession {
  operatorId: string;
  shiftCode: string;
  machineCode: string;
  machineName: string;
  workCenterId: number;
  workOrderId: number | null;
  woCode: string;
  targetQty: number;
  jobId: number | null;
  downtimeLogId: number | null;
  downtimeReason: string;
  downtimeStartTime: string | null;
}

const STORAGE_KEY = 'aeromes:tablet-session';

const DEFAULT_SESSION: TabletSession = {
  operatorId: '',
  shiftCode: 'DEFAULT',
  machineCode: '',
  machineName: '',
  workCenterId: 0,
  workOrderId: null,
  woCode: '',
  targetQty: 0,
  jobId: null,
  downtimeLogId: null,
  downtimeReason: '',
  downtimeStartTime: null,
};

function loadSession(): TabletSession {
  try {
    const s = localStorage.getItem(STORAGE_KEY);
    return s ? { ...DEFAULT_SESSION, ...JSON.parse(s) } : DEFAULT_SESSION;
  } catch {
    return DEFAULT_SESSION;
  }
}

interface TabletSessionContextValue {
  session: TabletSession;
  update: (patch: Partial<TabletSession>) => void;
  reset: () => void;
}

const TabletSessionContext = createContext<TabletSessionContextValue | null>(null);

export function TabletSessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<TabletSession>(loadSession);

  function update(patch: Partial<TabletSession>) {
    setSession((prev) => {
      const next = { ...prev, ...patch };
      localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
      return next;
    });
  }

  function reset() {
    localStorage.removeItem(STORAGE_KEY);
    setSession(DEFAULT_SESSION);
  }

  return (
    <TabletSessionContext.Provider value={{ session, update, reset }}>
      {children}
    </TabletSessionContext.Provider>
  );
}

export function useTabletSession() {
  const ctx = useContext(TabletSessionContext);
  if (!ctx) throw new Error('useTabletSession must be used inside TabletSessionProvider');
  return ctx;
}
