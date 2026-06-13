import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Default bundles — lazy-loaded per namespace
const loadNamespace = async (lng: string, ns: string) => {
  try {
    const mod = await import(`../locales/${lng}/${ns}.json`);
    return mod.default;
  } catch {
    return {};
  }
};

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: 'vi',
    supportedLngs: ['vi', 'en-US'],
    defaultNS: 'common',
    ns: ['common'],
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
      lookupLocalStorage: 'i18n_lng',
    },
    interpolation: {
      escapeValue: false,
    },
    backend: undefined,
    partialBundledLanguages: true,
    resources: {},
  });

// Load initial bundles synchronously after init
export async function loadNamespaces(lng: string, namespaces: string[]) {
  for (const ns of namespaces) {
    if (!i18n.hasResourceBundle(lng, ns)) {
      const bundle = await loadNamespace(lng, ns);
      i18n.addResourceBundle(lng, ns, bundle, true, true);
    }
  }
}

// Called once at startup to seed common bundle for both languages
loadNamespace('vi', 'common').then((b) => i18n.addResourceBundle('vi', 'common', b, true, true));
loadNamespace('en-US', 'common').then((b) => i18n.addResourceBundle('en-US', 'common', b, true, true));

export default i18n;

export const SUPPORTED_LANGUAGES = [
  { code: 'vi',   label: 'Tiếng Việt' },
  { code: 'en-US', label: 'English' },
] as const;

export type SupportedLanguage = (typeof SUPPORTED_LANGUAGES)[number]['code'];
