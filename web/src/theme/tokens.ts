// Domain colour tokens — manufacturing-specific semantics
// All values are raw hex so they can be used outside MUI theme context.

export const statusColors = {
  // Work-order / job lifecycle
  DRAFT:      { bg: '#F1F5F9', text: '#475569', border: '#CBD5E1' },
  RELEASED:   { bg: '#EFF6FF', text: '#1D4ED8', border: '#BFDBFE' },
  RUNNING:    { bg: '#F0FDF4', text: '#15803D', border: '#86EFAC' },
  PAUSED:     { bg: '#FFFBEB', text: '#B45309', border: '#FCD34D' },
  COMPLETED:  { bg: '#F0FDF4', text: '#166534', border: '#4ADE80' },
  CLOSED:     { bg: '#F8FAFC', text: '#64748B', border: '#E2E8F0' },
  CANCELLED:  { bg: '#FEF2F2', text: '#B91C1C', border: '#FECACA' },
  ON_HOLD:    { bg: '#FFFBEB', text: '#92400E', border: '#FDE68A' },
  // Dark-mode overrides (alpha-based so they work on dark bg)
  DRAFT_DARK:     { bg: 'rgba(71,85,105,0.15)',   text: '#94A3B8', border: 'rgba(71,85,105,0.4)' },
  RELEASED_DARK:  { bg: 'rgba(29,78,216,0.15)',   text: '#93C5FD', border: 'rgba(29,78,216,0.4)' },
  RUNNING_DARK:   { bg: 'rgba(21,128,61,0.15)',   text: '#86EFAC', border: 'rgba(21,128,61,0.4)' },
  PAUSED_DARK:    { bg: 'rgba(180,83,9,0.15)',    text: '#FCD34D', border: 'rgba(180,83,9,0.4)' },
  COMPLETED_DARK: { bg: 'rgba(22,101,52,0.15)',   text: '#4ADE80', border: 'rgba(22,101,52,0.4)' },
  CLOSED_DARK:    { bg: 'rgba(100,116,139,0.15)', text: '#94A3B8', border: 'rgba(100,116,139,0.4)' },
  CANCELLED_DARK: { bg: 'rgba(185,28,28,0.15)',   text: '#FCA5A5', border: 'rgba(185,28,28,0.4)' },
  ON_HOLD_DARK:   { bg: 'rgba(146,64,14,0.15)',   text: '#FDE68A', border: 'rgba(146,64,14,0.4)' },
} as const;

export type WorkOrderStatus = 'DRAFT' | 'RELEASED' | 'RUNNING' | 'PAUSED' | 'COMPLETED' | 'CLOSED' | 'CANCELLED' | 'ON_HOLD';

export const machineColors = {
  RUNNING:  '#15803D',
  IDLE:     '#94A3B8',
  SETUP:    '#1D4ED8',
  DOWN:     '#B91C1C',
  OFFLINE:  '#374151',
} as const;

// OEE zone thresholds
export const oeeZones = {
  WORLD_CLASS: { min: 85, color: '#15803D', label: 'World class' },
  GOOD:        { min: 65, color: '#3A9188', label: 'Good' },
  AVERAGE:     { min: 45, color: '#B45309', label: 'Average' },
  POOR:        { min: 0,  color: '#B91C1C', label: 'Needs improvement' },
} as const;

export function oeeZoneColor(value: number): string {
  if (value >= 85) return oeeZones.WORLD_CLASS.color;
  if (value >= 65) return oeeZones.GOOD.color;
  if (value >= 45) return oeeZones.AVERAGE.color;
  return oeeZones.POOR.color;
}

export const APPBAR_HEIGHT = 48;
export const FORM_ACTION_BAR_HEIGHT = 64;
