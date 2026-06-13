import i18n from './i18n';

function getLocale(): string {
  return i18n.language === 'vi' ? 'vi-VN' : 'en-US';
}

export function fmtDate(value: string | Date | null | undefined, opts?: Intl.DateTimeFormatOptions): string {
  if (!value) return '—';
  return new Intl.DateTimeFormat(getLocale(), opts ?? { dateStyle: 'medium' }).format(new Date(value));
}

export function fmtDateTime(value: string | Date | null | undefined): string {
  if (!value) return '—';
  return new Intl.DateTimeFormat(getLocale(), { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
}

export function fmtNumber(value: number | null | undefined, opts?: Intl.NumberFormatOptions): string {
  if (value == null) return '—';
  return new Intl.NumberFormat(getLocale(), opts).format(value);
}

export function fmtCurrency(value: number | null | undefined, currency = 'VND'): string {
  if (value == null) return '—';
  return new Intl.NumberFormat(getLocale(), { style: 'currency', currency }).format(value);
}

export function fmtPercent(value: number | null | undefined, fractionDigits = 1): string {
  if (value == null) return '—';
  return new Intl.NumberFormat(getLocale(), {
    style: 'percent',
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits,
  }).format(value / 100);
}
