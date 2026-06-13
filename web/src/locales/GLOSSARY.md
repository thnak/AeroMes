# MES Localization Glossary — EN ↔ VI

## Namespace convention

One JSON file per module under `src/locales/{lng}/{module}.json`.
Modules: `common`, `auth`, `master`, `production`, `quality`, `lab`.

Load a module namespace in a page component:
```tsx
const { t } = useTranslation('master');
// t('products.title') → 'Sản phẩm' / 'Products'
```

Seed new namespaces in `src/lib/i18n.ts` via `loadNamespaces(lng, ['my-module'])`.

## Core MES terms

| English | Tiếng Việt | Key |
|---|---|---|
| Work Order | Lệnh sản xuất | production.workOrders.title |
| Bill of Materials (BOM) | Định mức nguyên liệu | master.bom.title |
| Work Center | Xưởng sản xuất | master.workCenters.title |
| Job | Công việc sản xuất | production.jobs.title |
| Downtime | Thời gian dừng máy | production.downtime.title |
| OEE | Hiệu suất thiết bị tổng thể | production.oee.title |
| Non-Conformance Report (NCR) | Phiếu không phù hợp | quality.ncr.title |
| Defect Code | Mã lỗi | quality.defectCodes.title |
| Standard Operating Procedure (SOP) | Quy trình thao tác chuẩn | lab.sop.title |
| Lab Request | Yêu cầu phòng thí nghiệm | lab.requests.title |
| Lot Number | Số lô | - |
| Work-in-Progress (WIP) | Sản phẩm dở dang | - |
| Finished Goods | Thành phẩm | - |
| Raw Material | Nguyên liệu thô | - |
| Certificate of Analysis (CoA) | Phiếu kiểm nghiệm | - |

## Missing key detection

In development, i18next logs warnings for missing keys to the browser console.
Keys missing in any supported language will appear as `[vi] master:products.foo`.
