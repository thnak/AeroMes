# AeroMes Design System Specification

> Living document — review each section before starting implementation.
> Stack: React 19 · TypeScript 6 · MUI v9 · Vite 8

---

## 0. Scope Decisions

### Client Platforms
| Platform | Tech | Repo |
|----------|------|------|
| Web App (management + tablet operator) | React 19 + MUI | this repo (`web/`) |
| PDA / Barcode Scanner | Native Android (Kotlin) | **separate repo** |

> The `/pda/` web routes are **removed from scope**. All barcode/warehouse scanning flows are handled by the native Android app. The web frontend serves only browser-based clients (desktop + tablet browsers).

### Phase Strategy
| Phase | What | When |
|-------|------|------|
| **Phase 0 (now)** | Platform layer: theme tokens, layout shells, base components — no API wiring | Start immediately |
| **Phase 1** | API wiring once backend endpoints are stable | After BE stabilizes |

---

## 0.1 Required Dependencies (install before building)

The current `web/package.json` needs these additions:

| Package | Purpose |
|---|---|
| `react-router-dom` | Client-side routing |
| `@mui/x-data-grid` | Sortable/paginated data tables |
| `@mui/x-date-pickers` + `dayjs` | Date/time pickers (shift times, date filters) |
| `apexcharts` + `react-apexcharts` | OEE gauges, Pareto, trend lines |
| `react-hook-form` + `zod` | Form state + schema validation |
| `axios` | API client with JWT interceptor + cookie support |
| `idb` | IndexedDB wrapper — offline buffer for tablet output submissions |
| `@iconify/react` | Iconify icon runtime (Solar icon set) |
| `framer-motion` | Page transitions, micro-interactions, sidebar collapse |

---

## 1. Design Tokens

### 1.1 Color System

The existing seafoam brand palette is established. These semantic and domain layers need to be added to the MUI theme.

#### Semantic colors (extend MUI `palette`)
```
success  #1F7A4D  (green — completion, OK qty)
warning  #B45309  (amber — paused, idle)
error    #B91C1C  (red — downtime, NG qty, cancelled)
info     #1D4ED8  (blue — released/open states)
```

#### Domain: Work Order / Job status colors
| Status | Color token | Hex |
|--------|------------|-----|
| PREPARED | `palette.grey[500]` | #6B7280 |
| RELEASED | `info.main` | #1D4ED8 |
| RUNNING | `success.main` | #1F7A4D |
| PAUSED | `warning.main` | #B45309 |
| COMPLETED | `primary.main` (seafoam) | #044A42 |
| CANCELLED | `error.main` | #B91C1C |

#### Domain: Machine status colors
| Status | Hex | Usage |
|--------|-----|-------|
| RUNNING | #16A34A | Green dot |
| DOWN | #DC2626 | Red dot + pulse animation |
| IDLE | #D97706 | Amber dot |
| OFFLINE | #9CA3AF | Grey dot |

#### Domain: OEE zone colors
| Zone | Range | Color |
|------|-------|-------|
| World-class | ≥ 85% | #16A34A |
| Acceptable | 65–84% | #D97706 |
| Poor | < 65% | #DC2626 |

#### Domain: Inventory location type colors
| Type | Color |
|------|-------|
| RAW_MATERIAL | #7C3AED (purple) |
| WIP | #0369A1 (blue) |
| FINISHED_GOODS | #065F46 (teal) |
| SCRAP | #9CA3AF (grey) |

---

### 1.2 Typography Scale

Extend the existing `Inter` font family:

| Token | Use case | Size / Weight |
|-------|----------|---------------|
| `kpi` | OEE %, production counts on dashboard | 48px / 700 |
| `h1` | Page titles | 24px / 700 |
| `h2` | Section headings | 20px / 600 |
| `h3` | Card headings | 18px / 600 |
| `body1` | Default content | 14px / 400 |
| `body2` | Secondary info | 13px / 400 |
| `caption` | Table meta, timestamps | 12px / 400 |
| `code` | Machine codes, WO codes | 13px / 500 · monospace |
| `operator` | Tablet large readout | 32px / 700 · high contrast |

---

### 1.3 Spacing & Sizing

Base: `8px` MUI default.

Tablet-specific overrides:
- Minimum tap target: `48px` height
- Button height: `56px` (tablet mode)
- Form field height: `56px` (tablet mode)
- Icon size for primary actions: `32px`

---

### 1.4 Elevation Levels

| Level | Usage |
|-------|-------|
| 0 | Flat page background (already: `#F4FAF9`) |
| 1 | Cards, panels (already: `boxShadow: '0 1px 4px 0 rgba(4,74,66,0.08)'`) |
| 2 | Sticky headers, sidebars |
| 3 | Modals, drawers |

---

## 2. Component Library

### 2.1 Status & Indicator Components

| Component | Props | Notes |
|-----------|-------|-------|
| `StatusChip` | `status`, `entity: 'workorder' \| 'job' \| 'machine' \| 'order'` | Color-coded MUI Chip |
| `MachineDot` | `status: 'RUNNING' \| 'DOWN' \| 'IDLE' \| 'OFFLINE'` | Animated dot indicator |
| `OeeZoneBadge` | `value: number` | Shows % with zone color |
| `SyncStatusBar` | `pending: number` | Tablet: shows offline queue count |
| `ShiftBadge` | `shift: string` | Pill showing "Shift A / Ca 1" |

### 2.2 KPI & Dashboard Components

| Component | Props | Notes |
|-----------|-------|-------|
| `KpiCard` | `title, value, unit, delta?, trend?` | OEE dashboard metric tiles |
| `OeeGauge` | `availability, performance, quality, oee` | Circular progress cluster |
| `MachineStatusGrid` | `machines[]` | Grid of MachineDot + code + WO |
| `ProductionCounter` | `ok, ng, target` | Real-time running counts |
| `ShiftSummaryCard` | `shift, startTime, woCode, op` | Active session overview |

### 2.3 Charts (all use `react-apexcharts`)

| Component | ApexCharts type | Use |
|-----------|-----------------|-----|
| `OeeGaugeChart` | `radialBar` | OEE % circular gauge with zone coloring |
| `OeeTrendChart` | `line` | OEE % over time (per shift/day) with area fill |
| `ParetoChart` | `bar` + `line` (mixed) | Defect analysis — bars + cumulative % line |
| `DowntimeBarChart` | `bar` (horizontal) | Downtime minutes by reason code |
| `ProductionAreaChart` | `area` | Output OK vs NG over time |
| `MachineOeeHeatmap` | `heatmap` | OEE by machine × day/shift |

### 2.4 Data Display

| Component | Notes |
|-----------|-------|
| `AppDataGrid` | Thin wrapper around MUI X DataGrid with consistent toolbar (search + filter + export) |
| `RoutingTimeline` | Horizontal step sequence — RoutingSteps with status overlay |
| `BomTree` | Collapsible tree table for BOM parent→children |
| `LotTraceCard` | Lot number → ProductionLog chain |
| `AuditLogTable` | Immutable security event list |

### 2.5 Form Components

| Component | Notes |
|-----------|-------|
| `ProductionOutputForm` | QtyOK + QtyNG inputs + `DefectPicker` (shown when NG > 0); generates idempotency key |
| `DefectPicker` | Multi-row: DefectCode select + Qty input; validates sum = QtyNG |
| `DowntimeForm` | ReasonCode select + optional notes; start/end time pickers |
| `ScanInput` | Barcode-optimized text input with scan icon, auto-submit on scan |
| `ShiftSelector` | Dropdown: Shift A / B / C |
| `DateRangeFilter` | Two date pickers for report filtering |
| `WorkCenterMachineSelect` | Cascading: WorkCenter → Machine |

### 2.6 Layout Components

| Component | Props | Notes |
|-----------|-------|-------|
| `AppShell` | `mode: 'web' \| 'tablet' \| 'pda'` | Swaps nav layout per device |
| `WebSidebar` | `items[]` | Collapsible sidebar navigation |
| `TabletTopBar` | `shift, operator, woCOde` | Minimal header with sync indicator |
| `PageHeader` | `title, breadcrumbs?, actions?` | Consistent page top |
| `SectionCard` | `title, collapsible?` | Content grouping card |
| `ConfirmDialog` | `title, message, onConfirm` | Reused for destructive actions |
| `FormActionBar` | `onCancel, onSubmit, isSubmitting, isDirty` | Sticky bottom bar on create/edit pages; shows unsaved-changes dot when `isDirty` |

---

## 3. Page & Route Inventory

### 3.1 Auth Module (`/auth/`)

| Route | View | Device |
|-------|------|--------|
| `/auth/login` | Email + password form; passkey button | All |
| `/auth/login/mfa` | TOTP code / Email OTP entry | All |
| `/auth/setup-mfa` | TOTP QR scan + verify; Email OTP; recovery codes display | Web |
| `/auth/change-password` | Forced password reset gate | All |

### 3.2 Dashboard (`/`)

| Route | View | Device |
|-------|------|--------|
| `/` | OEE KPI cards, machine status grid, active WO count, shift production counter, trend chart | Web |

### 3.3 Master Data (`/master/`)

All entities follow: **list page → separate create page → separate edit page**.
No drawers or dialogs for data entry; every form is a full route.

| Route | View | Notes |
|-------|------|-------|
| `/master/work-centers` | WorkCenters list | Code, Name, Machine count, Active |
| `/master/work-centers/new` | Create WorkCenter | Full-page form |
| `/master/work-centers/:id` | WorkCenter detail + Machines sub-list | — |
| `/master/work-centers/:id/edit` | Edit WorkCenter | Full-page form, pre-filled |
| `/master/machines` | Machines list | Code, Name, WorkCenter, Status, Brand |
| `/master/machines/new` | Create Machine | Full-page form |
| `/master/machines/:id/edit` | Edit Machine | Full-page form, pre-filled |
| `/master/products` | Products list | Code, Name, Unit, FinishedGood flag |
| `/master/products/new` | Create Product | Full-page form |
| `/master/products/:id/edit` | Edit Product | Full-page form, pre-filled |
| `/master/products/:id/bom` | BOM tree editor | Parent → Children + Qty + Scrap% |
| `/master/operations` | Operations list | Code, Name, Active |
| `/master/operations/new` | Create Operation | Full-page form |
| `/master/operations/:id/edit` | Edit Operation | Full-page form, pre-filled |
| `/master/routings` | Routings list | Code, Name, Product, Default flag |
| `/master/routings/new` | Create Routing | Full-page form |
| `/master/routings/:id/edit` | Edit Routing | Full-page form, pre-filled |
| `/master/routings/:id/steps` | RoutingSteps editor | Step# → Operation → WorkCenter → CycleTime + QC flag |
| `/master/storage-locations` | StorageLocations list | Code, Name, Type color, WorkCenter link |
| `/master/storage-locations/new` | Create StorageLocation | Full-page form |
| `/master/storage-locations/:id/edit` | Edit StorageLocation | Full-page form, pre-filled |
| `/master/defect-codes` | DefectCodes list | Code, Name, Category, Active |
| `/master/defect-codes/new` | Create DefectCode | Full-page form |
| `/master/defect-codes/:id/edit` | Edit DefectCode | Full-page form, pre-filled |

### 3.4 Integration / Planning (`/integration/`)

| Route | View | Notes |
|-------|------|-------|
| `/integration/sales-orders` | SO list | Read-only ERP data |
| `/integration/sales-orders/:id` | SO detail + linked POs | — |
| `/integration/production-orders` | PO list with status chips | Filter by status |
| `/integration/production-orders/:id` | PO detail + WO breakdown list | WOs generated per RoutingStep |

### 3.5 Production Execution (`/production/`)

| Route | View | Device |
|-------|------|--------|
| `/production/work-orders` | WO list/board — filter by status, WC | Web |
| `/production/work-orders/:id` | WO detail: progress bar, Job list, ProductionLogs | Web |
| `/production/jobs` | Jobs list: operator, machine, shift, status | Web |
| `/production/jobs/:id` | Job detail: output log timeline | Web |
| `/production/downtime` | Downtime log list + stats | Web |
| `/production/downtime/:id` | Downtime detail | Web |
| `/production/inventory` | Stock by location + product | Web |
| `/production/inventory/trace` | Lot traceability search | Web + PDA |

### 3.6 Tablet Operator Interface (`/tablet/`)

Optimized for 10-inch touch screen. Single-column. Large tap targets.

| Route | View | Flow step |
|-------|------|-----------|
| `/tablet/login` | Pin or RFID scan login | 1 |
| `/tablet/station` | Active Job overview: WO code, target vs actual, machine, shift | 2 |
| `/tablet/station/start` | Select WorkOrder → select Machine → confirm start Job | 2a |
| `/tablet/station/output` | QtyOK + QtyNG number pads + DefectPicker; Submit creates ProductionLog | 3 |
| `/tablet/station/downtime/start` | Reason code list (large tiles) + notes | 4a |
| `/tablet/station/downtime/active` | Active downtime display: elapsed timer, End button | 4b |
| `/tablet/station/finish` | Confirm end Job | 5 |

### 3.7 PDA / Barcode Scanner — OUT OF SCOPE (separate Android repo)

All barcode scanning, warehouse receiving/issuing, and handheld QA flows are handled by a native **Kotlin Android** application in a separate repository. The web frontend has no `/pda/` routes. The Android app will consume the same REST API.

### 3.8 Reports (`/reports/`)

| Route | View | Charts |
|-------|------|--------|
| `/reports/production` | Date range + WC filter → table + output bar chart | BarChart |
| `/reports/oee` | OEE trend by machine/shift; drill down | LineChart + OeeGauge |
| `/reports/downtime` | Downtime by reason (Pareto) + by machine + timeline | ParetoChart + BarChart |
| `/reports/quality` | Defect Pareto by code/category + trend | ParetoChart + LineChart |

### 3.9 Admin (`/admin/`)

| Route | View |
|-------|------|
| `/admin/users` | User list: name, email, roles, MFA status, last login |
| `/admin/users/new` | Create User — full-page form |
| `/admin/users/:id` | User detail: roles, MFA status, session list |
| `/admin/users/:id/edit` | Edit User — full-page form, pre-filled |
| `/admin/roles` | Roles × Permissions matrix |
| `/admin/roles/new` | Create Role — full-page form |
| `/admin/roles/:id/edit` | Edit Role — full-page form |
| `/admin/audit-log` | Security audit log: immutable event list with filter |

---

## 4. Layout & Display Contexts

### Layout principle: Sidebar nav + slim AppBar for user context
Navigation lives in the **left sidebar**. A slim **AppBar (48px)** sits at the top — not for navigation, but for user information and quick actions. This keeps the main content area as large as possible while always showing who is logged in.

```
┌──────┬────────────────────────────────────────────┐
│      │ [Page title / breadcrumb]  [🔔][⚙][Avatar]│  ← 48px AppBar
│ Logo ├────────────────────────────────────────────┤
│      │                                            │
│ Nav  │          Main Content Area                 │
│items │       (vh - 48px, scrollable)              │
│      │                                            │
│      │                                            │
└──────┴────────────────────────────────────────────┘
```

**AppBar contents (right-aligned):**
- Notification bell (Solar `bell-bold-duotone`) — future alerts/notifications slot
- Settings shortcut (Solar `settings-bold-duotone`) → `/admin/users`
- Color scheme toggle (Solar `sun-bold` / `moon-bold`)
- User avatar (Solar `user-circle-bold-duotone` fallback) + display name + role badge
- Click avatar → dropdown: Profile · Change Password · Logout

**AppBar contents (left-aligned):**
- Page breadcrumb or current page title (populated by router context)

Sidebar: logo + version (top) → nav items (middle, scrollable) → collapse toggle (bottom).

---

### 4.1 Manager/Web — 15" Laptop (primary audience)
- Viewport: ~1366×768 to 1440×900
- Sidebar: 240px expanded, collapses to 64px icon-only on click/hover
- Dense data tables, multi-column forms (2–3 col), full chart panels
- Page transitions: Framer Motion fade/slide-up (150ms)
- Micro-interactions: card hover lift, KPI count-up on mount
- Auth pages: split-panel layout — illustration left, form right
- Page section backgrounds: subtle radial gradient from brand color (not flat white)

### 4.2 Dashboard — 50"+ Display Wall
- Viewport: 1920×1080 minimum; readable at 10+ feet
- **No sidebar** — own full-bleed shell (`DashboardLayout`)
- `?display=tv` query param removes all interactive chrome
- KPI numbers: 64–96px, high contrast
- Machine status grid: large cells, pulsing animation on DOWN machines
- Auto-refresh every 30s with visible countdown ring
- Ambient background: slow animated gradient mesh or factory SVG silhouette (seafoam dark tones)

### 4.3 Operator Interface (OI) — 10" Tablet
- Viewport: 768–1024px, touch-first
- `TabletLayout`: no sidebar, minimal floating header (shift badge + sync dot)
- All tappable targets ≥ 48px; primary action buttons ≥ 56px
- One workflow step per screen
- Large high-contrast status readouts (operator typography)
- Muted background illustration (non-distracting)
- Step-flow navigation (no free sidebar access)

### 4.4 PDA / Handheld — OUT OF SCOPE
Native Android (Kotlin) app — separate repo.

---

## 5. Visual & Motion Design

### 5.1 Animation (Framer Motion + CSS)

| Animation | Trigger | Spec |
|-----------|---------|------|
| Page enter | Route change | `opacity 0→1 + y +16px→0`, 150ms ease-out |
| Card hover lift | Mouse enter | `scale(1.015)`, shadow deepen, 120ms |
| KPI count-up | On mount / data load | Animated number, 800ms ease-out |
| Machine DOWN pulse | Status = DOWN | Concentric ring pulse, red, 1.5s infinite |
| Sidebar collapse | Toggle click | Width transition 240→64px, 200ms |
| FormDrawer open | Open state | Slide from right, 250ms ease |
| Skeleton → content | Data arrives | Fade in, 180ms |
| Dashboard refresh | 30s tick | Subtle flash on updated cells |

### 5.2 Icon System — Iconify Solar

All icons use **Solar** via `@iconify/react`. Solar has 5 weight variants for styling hierarchy:

| Weight | Iconify suffix | When to use |
|--------|---------------|-------------|
| Bold Duotone | `-bold-duotone` | Primary/decorative — nav icons, featured actions, KPI card icons |
| Bold | `-bold` | High emphasis — status, alerts, active states |
| Outline | `-outline` | Standard — table actions, button icons |
| Linear | `-linear` | Low emphasis — secondary info, metadata |
| Broken | `-broken` | Experimental / loading states |

```tsx
import { Icon } from '@iconify/react';
<Icon icon="solar:bell-bold-duotone" width={24} />
<Icon icon="solar:user-circle-linear" width={20} />
```

**Common icons reference:**

| Use | Solar icon |
|-----|-----------|
| User/profile | `solar:user-circle-bold-duotone` |
| Notifications | `solar:bell-bold-duotone` |
| Settings | `solar:settings-bold-duotone` |
| Dashboard | `solar:widget-bold-duotone` |
| Work Orders | `solar:clipboard-list-bold-duotone` |
| Jobs | `solar:hammer-bold-duotone` |
| Downtime | `solar:stop-circle-bold-duotone` |
| Quality/defects | `solar:bug-bold-duotone` |
| Products | `solar:box-bold-duotone` |
| Machines | `solar:cpu-bolt-bold-duotone` |
| Reports | `solar:chart-2-bold-duotone` |
| OEE/Analytics | `solar:graph-up-bold-duotone` |
| Admin | `solar:shield-user-bold-duotone` |
| Logout | `solar:logout-3-bold` |
| Sun (light mode) | `solar:sun-bold` |
| Moon (dark mode) | `solar:moon-bold` |
| Add | `solar:add-circle-bold` |
| Edit | `solar:pen-bold` |
| Delete | `solar:trash-bin-minimalistic-bold` |
| Search | `solar:magnifer-bold` |
| Filter | `solar:filter-bold` |
| Collapse sidebar | `solar:sidebar-minimalistic-bold` |
| Check/success | `solar:check-circle-bold` |
| Warning | `solar:danger-triangle-bold` |
| Info | `solar:info-circle-bold` |

### 5.3 Background & Illustration System

**Format**: PNG or WEBP files. No SVG illustrations, no Lottie.  
**Location**: `web/src/assets/illustrations/`  
**AI prompts**: `web/src/assets/illustrations/prompts/*.txt` — one `.txt` per image with dimensions, placement context, and full generation prompt.

**Illustration inventory:**

| File | Dimensions | Used in |
|------|-----------|---------|
| `auth-hero.png` | 1200×900 | Login page left panel |
| `dashboard-ambient-bg.png` | 1920×1080 (+ 4K) | Dashboard TV background |
| `empty-no-data.png` | 400×300 | Table/list with no records |
| `empty-no-results.png` | 400×300 | Search returns nothing |
| `empty-no-orders.png` | 400×300 | Tablet station: no WOs available |
| `error-403.png` | 400×300 | Forbidden error page |
| `error-404.png` | 400×300 | Not found error page |
| `error-500.png` | 400×300 | Server error page |
| `tablet-idle-welcome.png` | 800×400 | Tablet idle/welcome screen |

Until real images are generated, the `<Illustration>` component renders a colored placeholder div at the correct aspect ratio.

**`<Illustration>` component** (`src/components/Illustration.tsx`):
- Renders `<img src={illustrations[name]} />` with `object-fit: cover`
- Falls back to a seafoam-tinted placeholder div if image not found/loaded
- Accepts `width`, `height`, `className` props

**Page section backgrounds**: shallow radial gradient (`primary.lighter` → transparent) behind page header sections — never flat white.

---

## 6. UX Patterns

### 6.1 Offline-first (Tablet Production Output)
1. User submits output → generate UUID idempotency key
2. Optimistic UI: immediately show "Submitted" state
3. If API fails → store in IndexedDB queue
4. Background sync on reconnect → replay queue in order
5. `SyncStatusBar` shows `N pending` when queue > 0

### 6.2 Idempotency Key Flow
- Key generated per `ProductionOutputForm` mount: `crypto.randomUUID()`
- Sent as `X-Idempotency-Key` header
- Duplicate server response (409/200) handled same as success
- Key is reset only on explicit new form session

### 6.3 Confirmation Pattern
- All destructive/irreversible actions (Cancel WO, End Downtime, Delete record) use `ConfirmDialog`
- Tablet: full-screen confirm step (not a small modal)

### 6.4 Loading & Error States
- All data fetches: skeleton loaders (not spinners) for content areas
- API errors: MUI `Alert` inline + toast notification
- RFC 7807 `ProblemDetails` response → extract `title` + `detail` for display

### 6.5 Real-time Dashboard
- OEE dashboard polls every 30s (or SSE if added later)
- Machine status grid uses local state updated per poll
- "Last updated: Xs ago" indicator in dashboard header

---

## 6. Navigation Structure

```
Web Sidebar
├── Dashboard                    (/)
├── Production
│   ├── Work Orders              (/production/work-orders)
│   ├── Jobs                     (/production/jobs)
│   ├── Downtime                 (/production/downtime)
│   └── Inventory                (/production/inventory)
├── Planning
│   ├── Production Orders        (/integration/production-orders)
│   └── Sales Orders             (/integration/sales-orders)
├── Quality
│   └── Defect Analysis          (/reports/quality)
├── Reports
│   ├── Production               (/reports/production)
│   ├── OEE Analysis             (/reports/oee)
│   └── Downtime Analysis        (/reports/downtime)
├── Master Data
│   ├── Work Centers             (/master/work-centers)
│   ├── Machines                 (/master/machines)
│   ├── Products                 (/master/products)
│   ├── Operations & Routings    (/master/routings)
│   ├── Storage Locations        (/master/storage-locations)
│   └── Defect Codes             (/master/defect-codes)
└── Admin                        (/admin/users)  [admin role only]

Tablet Nav (bottom tabs or header buttons)
├── My Station     (/tablet/station)
└── Downtime       (/tablet/station/downtime/*)

PDA Nav (step flow, no persistent nav)
└── Scan → Action → Confirm
```

---

## 7. API Client Design

All backend calls go through a typed API client module.

| Module | Base URL | Auth |
|--------|----------|------|
| `masterApi` | `/api/v1/master/` | Cookie / JWT |
| `productionApi` | `/api/v1/` | Cookie / JWT |
| `integrationApi` | `/api/v1/integration/` | Cookie / JWT |
| `qualityApi` | `/api/v1/quality/` | Cookie / JWT |
| `reportsApi` | `/api/v1/reports/` | Cookie / JWT |
| `authApi` | `/api/v1/auth/` | Cookie |

Response type: all success responses should be typed; errors parsed from `ProblemDetails` RFC 7807.

---

## 8. Phase 0 — Platform Layer Build Checklist

> Build these now, no API wiring needed. All components use static props / mock data.

### Step 1: Install packages
```bash
npm install react-router-dom @mui/x-data-grid @mui/x-date-pickers dayjs \
  apexcharts react-apexcharts react-hook-form zod @hookform/resolvers \
  axios idb
```

### Step 2: Extend theme (`web/src/theme/`)
- [ ] `tokens.ts` — status colors, OEE zone colors, machine status colors, location type colors
- [ ] `theme.ts` — add semantic `success/warning/error/info` to palette; add `kpi` + `code` + `operator` typography variants
- [ ] `theme.ts` — add `MuiDataGrid`, `MuiDatePicker` component overrides

### Step 3: Router setup (`web/src/router/`)
- [ ] `index.tsx` — `createBrowserRouter` with all route paths defined (no pages yet, just placeholders)
- [ ] Route guards: `AuthGuard` (redirect to login if no token) + `RoleGuard`

### Step 4: API client (`web/src/api/`)
- [ ] `client.ts` — Axios instance with base URL, JWT Bearer interceptor, 401 → refresh → retry
- [ ] `auth.ts`, `master.ts`, `production.ts`, `quality.ts`, `integration.ts`, `reports.ts` — typed API modules (functions only, no implementation yet — just signatures + return types)

### Step 5: Layout shells (`web/src/layouts/`)
- [ ] `WebLayout.tsx` — collapsible left sidebar + top app bar + `<Outlet>`
- [ ] `TabletLayout.tsx` — minimal top bar + `<Outlet>` (no sidebar)
- [ ] `AuthLayout.tsx` — centered card shell for login pages

### Step 6: Sidebar navigation (`web/src/components/nav/`)
- [ ] `SidebarNav.tsx` — renders nav items from config; highlights active route
- [ ] `navConfig.ts` — all nav items with icon, label, path, requiredRole

### Step 7: Base components (`web/src/components/`)
- [ ] `StatusChip.tsx` — WO/Job/PO/Machine status → color-coded Chip
- [ ] `MachineDot.tsx` — animated status dot
- [ ] `OeeZoneBadge.tsx` — % value with zone background color
- [ ] `KpiCard.tsx` — metric tile (title + big number + unit + optional delta)
- [ ] `PageHeader.tsx` — title + breadcrumbs + action slot
- [ ] `SectionCard.tsx` — MUI Card wrapper with title prop
- [ ] `ConfirmDialog.tsx` — reusable destructive-action confirm modal
- [ ] `FormActionBar.tsx` — sticky bottom bar for create/edit pages (Cancel + Save, dirty indicator)
- [ ] `AppDataGrid.tsx` — MUI X DataGrid wrapper with search toolbar + consistent empty state
- [ ] `SyncStatusBar.tsx` — tablet: shows N pending offline submissions

### Step 8: Chart stubs (`web/src/components/charts/`)
- [ ] `OeeGaugeChart.tsx` — radialBar with zone color
- [ ] `OeeTrendChart.tsx` — line/area chart
- [ ] `ParetoChart.tsx` — mixed bar + cumulative line
- [ ] `DowntimeBarChart.tsx` — horizontal bar
- [ ] `ProductionAreaChart.tsx` — area chart OK vs NG

### Step 9: Page stubs (`web/src/pages/`)
Each page: just the `PageHeader` + layout shell + `TODO: wire API` placeholder.
Create/edit pages also render `<FormActionBar>` stub.
- [ ] `Dashboard/index.tsx`
- [ ] `auth/Login.tsx`, `auth/MfaChallenge.tsx`, `auth/SetupMfa.tsx`
- [ ] `master/WorkCentersPage.tsx`, `WorkCenterCreatePage.tsx`, `WorkCenterEditPage.tsx`
- [ ] `master/MachinesPage.tsx`, `MachineCreatePage.tsx`, `MachineEditPage.tsx`
- [ ] `master/ProductsPage.tsx`, `ProductCreatePage.tsx`, `ProductEditPage.tsx`, `BomEditorPage.tsx`
- [ ] `master/OperationsPage.tsx`, `OperationCreatePage.tsx`, `OperationEditPage.tsx`
- [ ] `master/RoutingsPage.tsx`, `RoutingCreatePage.tsx`, `RoutingEditPage.tsx`, `RoutingStepsPage.tsx`
- [ ] `master/StorageLocationsPage.tsx`, `StorageLocationCreatePage.tsx`, `StorageLocationEditPage.tsx`
- [ ] `master/DefectCodesPage.tsx`, `DefectCodeCreatePage.tsx`, `DefectCodeEditPage.tsx`
- [ ] `integration/SalesOrders.tsx`, `ProductionOrders.tsx`
- [ ] `production/WorkOrders.tsx`, `WorkOrderDetail.tsx`, `Jobs.tsx`, `Downtime.tsx`, `Inventory.tsx`
- [ ] `tablet/Station.tsx`, `OutputForm.tsx`, `DowntimeFlow.tsx`
- [ ] `reports/Production.tsx`, `Oee.tsx`, `Downtime.tsx`, `Quality.tsx`
- [ ] `admin/UsersPage.tsx`, `UserCreatePage.tsx`, `UserEditPage.tsx`
- [ ] `admin/RolesPage.tsx`, `RoleCreatePage.tsx`, `RoleEditPage.tsx`, `AuditLogPage.tsx`

---

## 8. API Coverage Audit (what's built vs. what the UI needs)

Cross-referenced against the 17 backend controllers (93 endpoints).

### APIs already implemented ✓
| Area | Endpoints |
|------|-----------|
| Auth + MFA + Passkeys | login, refresh, logout, sessions, me, TOTP, OTP, recovery codes, passkey register/login |
| Users & Roles | CRUD users, roles, permissions matrix |
| Audit Log | query, user-specific, CSV export |
| Work Centers | GET, POST, PUT, DELETE |
| Machines | GET, POST, PUT, DELETE |
| Products | GET, POST, PUT, DELETE |
| BOM Items | GET by parent, POST, PUT, DELETE |
| Operations | GET, POST, PUT, DELETE |
| Routings + Steps | GET, GET/:id with steps, POST, PUT, DELETE; add/remove steps |
| Storage Locations | GET, POST, PUT, DELETE |
| Work Orders | GET (filter by status), POST /:id/start |
| Jobs | POST (start), POST /:id/finish |
| Production | POST /submit-output, GET /oee |
| Downtime | POST /start, POST /:id/end |

### APIs NOT YET IMPLEMENTED — frontend pages will need stubs or deferral
| Missing Endpoint | Affects Page |
|-----------------|--------------|
| `GET /quality/defect-codes` + CRUD | `/master/defect-codes` |
| `GET /integration/sales-orders` + detail | `/integration/sales-orders` |
| `GET /integration/production-orders` + detail | `/integration/production-orders` |
| `GET /production/work-orders/:id` (detail) | WO detail page |
| `GET /production/jobs` (list) | Jobs list |
| `GET /production/jobs/:id` (detail) | Job detail |
| `GET /production/downtime` (list) | Downtime log list |
| `GET /production/inventory` | Inventory stock page |
| `GET /reports/production` | Production report |
| `GET /reports/downtime` | Downtime report |
| `GET /reports/quality` | Quality/defect report |

> **Note**: `App.tsx` already has a working prototype dashboard — 4 KPI stat cards + work order list with status chips. This is the starting foundation for the real dashboard page.

---

## 9. Remaining Open Questions

| # | Question | Default if not answered |
|---|----------|------------------------|
| 1 | **Server state**: TanStack Query v5 for cache/loading/refetch, or custom hooks? | Use TanStack Query — it handles loading + error + stale states out of the box |
| 2 | **Real-time dashboard**: polling every 30s, or SignalR hub? | Start with polling; upgrade to SignalR when BE adds hub |
| 3 | **Tablet auth**: full username/password login, or PIN shortcut? | Full login for now; PIN shortcut deferred |
| 4 | **i18n**: bilingual UI (VI + EN) via `react-i18next`, or English-only first? | English-only labels first; i18n hooks deferred |
| 5 | **Report export**: Excel/PDF from backend, or client-side? | Backend CSV export (already exists for audit log); Excel TBD |
| 6 | **Offline buffer**: IndexedDB queue for tablet output, or error toast + manual retry? | Error toast + retry for now; offline queue in later milestone |
| 7 | **Dark mode toggle**: show in UI from day one? | Yes — theme already supports it, low effort |

---

## 10. Page Standard Spec

This section defines the **anatomy, spacing, and composing rules** for every page in the Web UI. All pages must follow this spec.

### 10.1 Standard page anatomy

```
WebLayout (fills viewport 100vh)
├── Sidebar (240px / 64px collapsed, position: fixed left, z-index: drawer)
│   ├── Logo block (56px, borderBottom)
│   ├── NavSection × N
│   │   ├── Section heading (overline, 0.625rem)
│   │   └── NavItemRow (40px min-height, 8px border-radius)
│   └── (bottom area for future user pill)
└── Main area (flex-col, margin-left = sidebarWidth, transition: 220ms)
    ├── AppBar (48px, position: fixed, bgcolor: background.paper, backdrop-blur: 8px)
    │   ├── Breadcrumbs (left, auto-derived from route pathname)
    │   └── Quick actions (right): Bell(badge) · Settings · Theme toggle · Avatar
    ├── AppBar spacer (48px, flexShrink: 0)
    └── Scroll area (flex: 1, overflow-y: auto, bgcolor: background.default)
        └── <Outlet /> → route component
            └── <PageRoot p={3}>
                ├── <PageHeader>           ← always first
                ├── <TableToolbar>         ← list pages only
                └── content (grid/table/form)
```

### 10.2 PageHeader anatomy

```tsx
<PageHeader
  title="Work Orders"
  subtitle="Manage and monitor all production work orders"
  breadcrumbs={[{ label: 'Production' }, { label: 'Work Orders' }]}
  actions={
    <>
      <Button variant="outlined" startIcon={<SolarIcon name="export" />}>Export</Button>
      <Button variant="contained" startIcon={<SolarIcon name="add" />}>New WO</Button>
    </>
  }
/>
```

| Element | Spec |
|---------|------|
| Container `mb` | `3` (24px) |
| Breadcrumbs | `caption` size, last crumb `fontWeight: 600`, separator `mx: 0.5` |
| Title | `h5` / `component="h1"`, `fontWeight: 600` |
| Subtitle | `body2`, `color: text.secondary`, `mt: 0.5` |
| Actions | `Stack direction="row" spacing={1}`, right-aligned, `pt: 0.25` |
| Action button order | Secondary (outlined) left → Primary (contained) right |

### 10.3 List/table page pattern

```
<PageRoot>
  <PageHeader title="…" subtitle="…" actions={<Button>New</Button>} />
  <TableToolbar search={q} onSearchChange={setQ} filters={[…]} totalCount={n}
                actions={<ExportButton /> <RefreshButton />} />
  <Card sx={{ flex: 1 }}>  {/* or DataGrid directly */}
    <DataGrid … slots={{ noRowsOverlay: TableEmptyState }} />
  </Card>
</PageRoot>
```

### 10.4 Detail page pattern (view-only)

Detail pages show a record's full state with context cards. They do **not** embed edit forms.

```
<PageRoot>
  <PageHeader
    title="WO-2026-0089"
    subtitle="Frame Assembly A · Released 08 Jun 2026"
    breadcrumbs={[
      { label: 'Production' },
      { label: 'Work Orders', href: '/production/work-orders' },
      { label: 'WO-2026-0089' },
    ]}
    actions={
      <>
        <Button variant="outlined" startIcon={<SolarIcon name="pen" />}
          onClick={() => navigate('edit')}>Edit</Button>
        <Button color="error">Cancel WO</Button>
      </>
    }
  />
  <Grid container spacing={2.5}>
    <Grid size={{ xs: 12, md: 8 }}>  {/* main info card */}
      <Card>…</Card>
    </Grid>
    <Grid size={{ xs: 12, md: 4 }}>  {/* status / metadata card */}
      <Card>…</Card>
    </Grid>
  </Grid>
</PageRoot>
```

The "Edit" button navigates to `/:id/edit` — it never opens a drawer.

### 10.5 Create/edit page rules

Create and edit actions are always **full-page routes** (`/new`, `/:id/edit`). Drawers and dialogs are never used for data entry.

| Rule | Detail |
|------|--------|
| Route pattern | `<list-path>/new` (create), `<list-path>/:id/edit` (edit) |
| Page title | "New `<Entity>`" for create; "`<Entity Name>`" for edit |
| Breadcrumbs | `…Context > List page > New` or `…Context > List page > Record name > Edit` |
| Form width | `<Container maxWidth="md">` (960px) for most forms; `"lg"` (1280px) for complex multi-section forms (e.g. BOM, RoutingSteps) |
| Section grouping | Use `<SectionCard>` per logical group — one card per concern (basic info, configuration, relationships) |
| Field layout | `<Grid container spacing={2.5}>` inside each `SectionCard` |
| Field sizing | `size="small"` (default via theme) |
| Form library | `react-hook-form` + `zod` + `@hookform/resolvers` |
| Per-field errors | Always use `helperText={errors.field?.message}` — no toast-only validation |
| Action bar | `<FormActionBar>` stuck to the viewport bottom — Cancel (outlined) + Save (contained) |
| Cancel behaviour | Navigate back to list or detail page; browser Back also works |
| Unsaved changes | `<FormActionBar isDirty={isDirty}>` shows a yellow dot; browser `beforeunload` warning |
| Edit pre-fill | Load record via `useQuery`, pass to `reset(data)` in `useEffect` |
| Submit — create | On success navigate to the new record's detail page |
| Submit — edit | On success navigate back to the detail page |
| Destructive action | Always requires `<ConfirmDialog>` before executing |

### 10.6 Loading states

| Context | Component |
|---------|-----------|
| KPI row initial load | `<KpiRowSkeleton count={4} />` |
| List page initial load | `<TablePageSkeleton rows={8} />` |
| Detail page initial load | `<DetailPageSkeleton />` |
| In-table/grid refetch | DataGrid `loading` prop (built-in overlay) |
| Button submitting | `disabled + CircularProgress size={14}` inside button |

### 10.7 Empty states

| Trigger | Component |
|---------|-----------|
| No rows, no filter active | `<TableEmptyState />` |
| No rows, filter/search active | `<TableEmptyState filtered onClear={clearFn} />` |
| Full-page context (e.g. no production orders) | `<EmptyState icon="…" title="…" description="…" action={<Button>}/>` |

### 10.8 Page spacing constants

| Token | Value | Use |
|-------|-------|-----|
| `PageRoot` padding | `p: 3` = 24px | All sides of the page content area |
| `PageHeader` bottom margin | `mb: 3` = 24px | Gap between header and content |
| Card padding | `p: '20px'` | Standard card content padding |
| Grid gap | `spacing={2.5}` = 20px | Between cards in grid layouts |
| Section gap | `mb: 3` = 24px | Between `<PageSection>` blocks |
| Inline spacing | `spacing={1.5}` = 12px | Between chips, buttons in a row |
| Compact row | `spacing={1}` = 8px | Dense lists, toolbar items |

### 10.9 Typography hierarchy on a page

```
Page title:     h5 (20px, 600)
Section title:  subtitle1 (16px, 600)
Card heading:   subtitle1 (16px, 600)
Table header:   caption + uppercase + tracking (12px, 600, 0.06em)
Body text:      body2 (14px, 400)
Metadata/label: caption (12px, 400–600)
Code / ID:      ui-monospace, 0.8125rem (13px)
```

---

### 10.10 Create/edit page anatomy

#### Visual structure

```
WebLayout
└── Scroll area
    └── <PageRoot pb={10}>       ← extra bottom padding clears FormActionBar
        ├── <PageHeader>          title, breadcrumbs (back to list), no action buttons
        └── <Container maxWidth="md">  (or "lg" for wide forms)
            └── <Stack spacing={3}>
                ├── <SectionCard title="Basic Information">
                │   └── <Grid container spacing={2.5}>
                │       ├── <Grid size={{ xs: 12, sm: 6 }}> <TextField …/> </Grid>
                │       └── …
                ├── <SectionCard title="Configuration">
                │   └── …
                └── <SectionCard title="…">     (as many as needed)

<FormActionBar>   ← position: fixed, bottom: 0, left: sidebarWidth, right: 0
  Cancel (outlined, onClick: navigate(-1))
  Save   (contained, type: submit, loading: isSubmitting)
```

#### Container width guide

| Form complexity | `maxWidth` | Examples |
|-----------------|-----------|---------|
| Simple (≤ 8 fields) | `"sm"` (600px) | DefectCode, Operation |
| Standard (8–20 fields) | `"md"` (960px) | WorkCenter, Machine, Product |
| Complex / multi-section | `"lg"` (1280px) | Routing+Steps, BOM, User with permissions |

#### `FormActionBar` spec

```tsx
<FormActionBar isDirty={isDirty} isSubmitting={isSubmitting}
               onCancel={() => navigate(-1)} onSubmit={handleSubmit(onSubmit)} />
```

- Position: `fixed`, bottom 0, left = sidebar width (240px expanded / 64px collapsed), right 0
- Height: 64px, `bgcolor: background.paper`, `borderTop: 1px solid divider`, `backdropFilter: blur(8px)`
- Content: right-aligned row — Cancel button + Save button
- `isDirty = true` → yellow dot on Cancel button label + `beforeunload` event registered
- `isSubmitting = true` → Save button shows `CircularProgress size={16}` and is disabled
- Sidebar collapse: `FormActionBar` subscribes to sidebar state and adjusts `left` with the same 220ms transition

#### Unsaved-changes guard

```tsx
useBeforeUnload(isDirty, 'You have unsaved changes. Leave anyway?');
// Also block react-router navigation:
useBlocker(isDirty);
```

Show `<ConfirmDialog>` when the blocker activates (not a native browser confirm).

#### Edit page data loading

```tsx
const { data, isLoading } = useGetEntityById(id);
const form = useForm({ resolver: zodResolver(schema) });

useEffect(() => {
  if (data) form.reset(mapToFormValues(data));
}, [data]);

if (isLoading) return <DetailPageSkeleton />;
```

Never pre-populate form default values from the URL or cache without going through `form.reset` — avoids stale dirty-state bugs.

#### Page examples

**Create — simple entity (`/master/defect-codes/new`)**

```
PageHeader: "New Defect Code"
Breadcrumbs: Master Data > Defect Codes > New
Container: maxWidth="sm"
  SectionCard "Defect Code Details"
    Code (TextField, required, monospace font)
    Name (TextField, required, fullWidth)
    Category (Autocomplete/Select)
    Active (Switch, default true)
FormActionBar
```

**Edit — standard entity (`/master/machines/:id/edit`)**

```
PageHeader: "<Machine Name>"
Breadcrumbs: Master Data > Machines > MCH-CNC-04 > Edit
Container: maxWidth="md"
  SectionCard "Identity"
    Code (read-only chip + helper "Code cannot change after creation")
    Name  Brand  Model
  SectionCard "Assignment"
    WorkCenter (Select)
    IsActive (Switch)
  SectionCard "Capacity"
    DesignedCycleTime  CycleTimeUnit
    CapacityPerShift
FormActionBar
```

**Create — complex entity (`/master/routings/new`)**

```
PageHeader: "New Routing"
Breadcrumbs: Master Data > Routings > New
Container: maxWidth="lg"
  SectionCard "Routing Header"      (Code, Name, Product, IsDefault)
  SectionCard "Steps"               (inline step editor — add/remove rows)
    Step# | Operation | WorkCenter | CycleTime | IsQCStep | [delete row]
    [+ Add Step] button at bottom of section
FormActionBar
```

The step editor inside `SectionCard "Steps"` is an inline row editor (not a nested drawer), since we're already on a full page with plenty of vertical space.
