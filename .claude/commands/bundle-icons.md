---
description: Regenerate the offline Iconify Solar icon bundle (web/src/lib/solarIconsBundle.json) from the icon names declared in web/src/lib/icons.ts. Run this after adding or removing icons.
---

Run `npm run bundle-icons` from the `web/` directory to regenerate the offline icon bundle.

Steps:
1. `cd web && npm run bundle-icons`
2. Report how many icons were written and whether any were missing (not found in @iconify-json/solar).
3. If any icons are missing, search the Solar set for close alternatives:
   ```
   node -e "const s=require('@iconify-json/solar/icons.json'); console.log(Object.keys(s.icons).filter(n=>n.includes('TERM')))"
   ```
   Update the offending entries in `web/src/lib/icons.ts` with valid names, then re-run `npm run bundle-icons` until the warning is gone.
4. Confirm the build still passes: `npm run build` (run from `web/`).
