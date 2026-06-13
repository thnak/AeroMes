// Centralized Solar icon name constants.
// Always import from here — never hardcode "solar:*" strings in components.
// Weight guide:
//   Bold Duotone  — nav items, featured actions, hero icons
//   Bold          — status indicators, alerts, inline emphasis
//   Outline       — standard actions, buttons, form fields
//   Linear        — metadata, secondary info, dense lists

export const Icons = {
  // Navigation
  dashboard:       'solar:widget-bold-duotone',
  production:      'solar:buildings-2-bold-duotone',
  workOrders:      'solar:clipboard-list-bold-duotone',
  jobs:            'solar:play-circle-bold-duotone',
  machines:        'solar:settings-bold-duotone',
  products:        'solar:box-bold-duotone',
  bom:             'solar:layers-bold-duotone',
  routing:         'solar:route-bold-duotone',
  quality:         'solar:shield-check-bold-duotone',
  reports:         'solar:chart-square-bold-duotone',
  integration:     'solar:link-round-angle-bold-duotone',
  masterData:      'solar:database-bold-duotone',
  admin:           'solar:user-id-bold-duotone',

  // Actions — Outline weight
  add:             'solar:add-circle-outline',
  edit:            'solar:pen-2-outline',
  delete:          'solar:trash-bin-trash-outline',
  view:            'solar:eye-outline',
  export:          'solar:export-outline',
  import:          'solar:import-outline',
  refresh:         'solar:refresh-outline',
  search:          'solar:magnifer-outline',
  filter:          'solar:filter-outline',
  download:        'solar:download-outline',
  upload:          'solar:upload-outline',
  copy:            'solar:copy-outline',
  close:           'solar:close-circle-outline',
  back:            'solar:arrow-left-outline',
  forward:         'solar:arrow-right-outline',
  expand:          'solar:alt-arrow-down-outline',
  collapse:        'solar:alt-arrow-up-outline',
  more:            'solar:menu-dots-bold',
  print:           'solar:printer-outline',

  // Production actions — Bold weight
  start:           'solar:play-bold',
  pause:           'solar:pause-bold',
  resume:          'solar:play-bold',
  complete:        'solar:check-circle-bold',
  cancel:          'solar:close-circle-bold',
  hold:            'solar:hand-shake-bold',
  release:         'solar:arrow-right-up-bold',

  // Status — Bold weight
  running:         'solar:play-circle-bold',
  paused:          'solar:pause-circle-bold',
  completed:       'solar:check-circle-bold',
  warning:         'solar:danger-bold',
  error:           'solar:close-circle-bold',
  info:            'solar:info-circle-bold',
  success:         'solar:check-circle-bold',

  // Machine
  machineOn:       'solar:cpu-bolt-bold',
  machineOff:      'solar:cpu-outline',
  machineDown:     'solar:danger-circle-bold',
  maintenance:     'solar:sledgehammer-bold',

  // Auth / visibility
  eye:             'solar:eye-outline',
  eyeClosed:       'solar:eye-closed-outline',

  // AppBar
  notifications:   'solar:bell-bing-bold-duotone',
  settings:        'solar:settings-bold-duotone',
  profile:         'solar:user-circle-bold-duotone',
  logout:          'solar:logout-3-outline',
  darkMode:        'solar:moon-bold-duotone',
  lightMode:       'solar:sun-bold-duotone',
  menuOpen:        'solar:hamburger-menu-outline',
  menuClose:       'solar:close-square-outline',
  chevronLeft:     'solar:alt-arrow-left-outline',
  chevronRight:    'solar:alt-arrow-right-outline',
  chevronDown:     'solar:alt-arrow-down-outline',

  // Domain entities — Linear weight
  order:           'solar:document-text-linear',
  lot:             'solar:tag-linear',
  serial:          'solar:qr-code-linear',
  operator:        'solar:user-linear',
  shift:           'solar:calendar-linear',
  location:        'solar:map-point-linear',
  quantity:        'solar:box-minimalistic-linear',
  time:            'solar:clock-circle-linear',
  date:            'solar:calendar-date-linear',

  // OEE
  oee:             'solar:chart-2-bold-duotone',
  availability:    'solar:graph-up-bold-duotone',
  performance:     'solar:rocket-bold-duotone',
  qualityOee:      'solar:shield-check-bold-duotone',

  // Empty states & errors
  emptyTable:      'solar:inbox-line-outline',
  emptySearch:     'solar:magnifer-zoom-out-outline',
  forbidden:       'solar:lock-keyhole-bold',
  notFound:        'solar:ghost-outline',
  serverError:     'solar:server-2-broken',

  // Release / system info
  release:         'solar:history-bold-duotone',
  sparkles:        'solar:stars-minimalistic-bold',
  version:         'solar:tag-bold-duotone',
  server:          'solar:server-2-bold-duotone',
  instance:        'solar:cpu-bold',
  commit:          'solar:code-bold',
} as const;

export type IconKey = keyof typeof Icons;
