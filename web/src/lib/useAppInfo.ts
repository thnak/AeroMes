import { useQuery } from '@tanstack/react-query';
import { api } from './apiClient';

interface ApiEnvelope<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export interface AppInfo {
  version: string;
  buildDate: string;
  environment: string;
  commitSha: string | null;
  instanceId: string;
}

export interface ChangelogChange {
  type: 'feature' | 'fix' | 'improvement' | 'breaking';
  title: string;
  description: string;
}

export interface ChangelogEntry {
  version: string;
  date: string;
  changes: ChangelogChange[];
}

const LAST_SEEN_KEY = 'aeromes:last-seen-release';

export function getLastSeenRelease(): string | null {
  return localStorage.getItem(LAST_SEEN_KEY);
}

export function markReleasesSeen(version: string) {
  localStorage.setItem(LAST_SEEN_KEY, version);
}

export function useAppInfo() {
  return useQuery<AppInfo>({
    queryKey: ['app-info'],
    queryFn: () =>
      api.get<ApiEnvelope<AppInfo>>('/api/v1/app-info').then((r) => r.data!),
    staleTime: 5 * 60 * 1000,
  });
}

export function useChangelog() {
  return useQuery<ChangelogEntry[]>({
    queryKey: ['changelog'],
    queryFn: () => fetch('/changelog.json').then<ChangelogEntry[]>((r) => r.json()),
    staleTime: 60 * 60 * 1000,
  });
}
