/**
 * Generates src/lib/solarIconsBundle.json containing only the Solar icons
 * actually referenced in src/lib/icons.ts. Run with: npm run bundle-icons
 *
 * Why a separate script: @iconify-json/solar is ~2 MB; importing it at
 * runtime would ship all 1000+ icons. This script extracts only the ~70
 * used ones so the bundle stays small and works fully offline.
 */

import { readFileSync, writeFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const require = createRequire(import.meta.url);

// Parse icon keys from icons.ts without a TS compiler — just regex the string literals.
const iconsTs = readFileSync(path.join(__dirname, '../src/lib/icons.ts'), 'utf8');
const usedIconIds = new Set(
  [...iconsTs.matchAll(/'solar:([^']+)'/g)].map(m => m[1])
);

console.log(`Found ${usedIconIds.size} unique Solar icon names.`);

// Load full Solar collection from devDep (never ends up in runtime bundle).
const solar = require('@iconify-json/solar/icons.json');

const missing = [];
const filteredIcons = {};

for (const name of usedIconIds) {
  if (solar.icons[name]) {
    filteredIcons[name] = solar.icons[name];
  } else {
    missing.push(name);
  }
}

if (missing.length) {
  console.warn('WARNING — icons not found in @iconify-json/solar:', missing);
}

const bundle = {
  prefix: solar.prefix,
  icons: filteredIcons,
  ...(solar.aliases ? { aliases: {} } : {}),
};

// Only carry over aliases that point to icons we're bundling.
if (solar.aliases) {
  for (const [alias, def] of Object.entries(solar.aliases)) {
    if (usedIconIds.has(alias) && filteredIcons[def.parent]) {
      bundle.aliases[alias] = def;
    }
  }
}

const outPath = path.join(__dirname, '../src/lib/solarIconsBundle.json');
writeFileSync(outPath, JSON.stringify(bundle, null, 2), 'utf8');

console.log(
  `Wrote ${Object.keys(filteredIcons).length} icons to src/lib/solarIconsBundle.json`
);
