# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
npm run dev                  # Vite dev server → http://localhost:5173 (proxies /api → :5170)
npm run build                # tsc -b && vite build → ../src/AeroMes.Api/wwwroot
npm run lint                 # ESLint v10 flat config

# API code generation (requires backend running or aeromes-openapi.json snapshot)
npm run fetch:spec           # Pull OpenAPI JSON from running backend
npm run generate:api         # Regenerate orval hooks + types from ../aeromes-openapi.json

# Asset tooling
npm run bundle-icons         # Regenerate Solar icon offline bundle (scripts/bundleIcons.mjs)
npm run stub:illustrations   # Create placeholder WebP illustrations (uv + Pillow)
npm run generate:illustrations  # Generate illustrations via DALL-E 3 (needs OPENAI_API_KEY)
```

## Architecture

### Entry point → providers

`main.tsx` wraps the app in: `ThemeProvider` (MUI, seafoam palette, light/dark) → `AuthProvider` → `QueryClientProvider` → `BrowserRouter`.

### Routing (`src/App.tsx`)

React Router v7 with lazy-loaded pages + Suspense. All web routes are wrapped in `AuthGuard` (unauthenticated → `/auth/login`). Three layout trees:

| Prefix | Layout | Notes |
|---|---|---|
| `/auth/*` | `AuthLayout` | Split-screen hero + form card |
| `/tablet/*` | `TabletLayout` | Touch-optimized M5 operator interface |
| everything else | `WebLayout` | AppBar + collapsible sidebar + outlet |

### API layer

**Do not write raw axios calls.** All endpoints are orval-generated in `src/api/<tag>/`. The mutator `src/lib/orvalMutator.ts` routes every generated hook through `src/lib/apiClient.ts` which handles:
- JWT Bearer injection from localStorage
- Transparent 401 → refresh → retry (deduped `refreshPromise`)
- `403 PasswordChangeRequired` → redirect to `/auth/change-password`
- Final 401 → clear token, redirect to `/auth/login`

Error messages come as RFC 7807 ProblemDetails; use `getErrorMessage()` from `apiClient.ts`.

To add a new endpoint: update `../aeromes-openapi.json` (or `npm run fetch:spec`), then `npm run generate:api`. Never hand-write hooks that orval would generate.

### React Query config (`src/lib/queryClient.ts`)

- `staleTime: 30 000` — MES data changes frequently; nothing is treated as permanently fresh
- `refetchOnWindowFocus: false` — production-floor UX
- No retry on 401/403/404; max 2 retries for server errors; no mutation retries

### Auth (`src/contexts/AuthContext.tsx`)

`useAuth()` returns `{ user, token, login, logout, hasRole }`. Roles: `Admin | Manager | Planner | QualityEngineer | Operator | Viewer`. Token lives in localStorage; backend issues refresh via httpOnly cookie on `/api/v1/auth/refresh`.

### Theme & design tokens

Seafoam palette: primary `#044A42`, secondary `#3A9188`. Design tokens live in `src/theme/tokens.ts`:
- `statusColors` — work order states (DRAFT → CANCELLED, light + dark variants)
- `machineColors` — RUNNING / IDLE / SETUP / DOWN / OFFLINE
- `OEE_ZONES` — WORLD_CLASS ≥85, GOOD ≥65, AVERAGE ≥45, POOR
- Sidebar: `SIDEBAR_WIDTH=240`, `SIDEBAR_COLLAPSED_WIDTH=64`, `APPBAR_HEIGHT=48`

### Icons

All icons are Solar (Iconify) loaded from the offline bundle `src/lib/solarIconsBundle.json` — **no CDN calls**. Use the `Icons.*` constants from `src/lib/icons.ts` rather than raw strings. Weight convention: Bold Duotone = nav/primary, Bold = status, Outline = actions, Linear = metadata. Render with `<SolarIcon name={Icons.foo} size={N} />`.

### Illustrations (`src/assets/illustrations/index.ts`)

Central registry exports a typed `illustrations` object. Import from there; never import WebP files directly. Prompts for AI regeneration are in `illustrations/prompts/*.txt`.

### Page structure pattern

A typical data page:
1. Orval hook → `useGetXxx()` with loading/error handling via `LoadingSkeleton`/`EmptyState`
2. `PageHeader` (title + breadcrumb + action buttons)
3. `TableToolbar` (search, filter, export, refresh)
4. MUI-X `DataGrid` with custom column renderers (`StatusChip`, links, formatters)
5. `FormDrawer` for create/edit (Zod schema → zodResolver → react-hook-form `Controller`)
6. `ConfirmDialog` for destructive actions

### Vite build output

`vite.config.ts` sets `build.outDir = '../src/AeroMes.Api/wwwroot'` — the SPA is served by the .NET backend as static files. The `/api` dev-proxy points to `:5170` (the backend port).
