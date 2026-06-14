# Engineering Foundation — Native Android PDA App (Newland + Urovo)

**Status:** Draft for review
**Date:** 2026-06-14
**Companion to:** [`pda-device-login.md`](./pda-device-login.md) (backend auth design), epic #250
**Audience:** AeroMes (.NET MES) team standing up its first native Android handheld app.

> **Provenance.** The vendor-specific facts below come from a fact-checked research pass
> (25 claims adversarially verified, 0 refuted) against primary sources — Newland's official
> `ScanDemo` repo + *Android PDA API Handbook V1.07 (2024-09)*, Urovo's developer `ScanManager`
> reference + official `SDK_ReleaseforAndroid` samples, and Google's ML Kit / Hilt / managed-config
> docs. Citations in §12. **Items explicitly flagged "⚠ validate on hardware" were not confirmable
> from a primary doc** and must be checked on a real device before relying on them.

---

## 1. Decision summary (TL;DR)

| Topic | Decision |
|---|---|
| Language / DI | **Kotlin**, AndroidX, **Hilt** (Google-recommended DI). Coroutines + **Flow** for scan/NFC events. |
| Barcode + QR scanning | Use the **built-in scan engine** (hardware trigger, fastest) per vendor, **not** the camera, as the primary path. Camera + ML Kit only as a cross-device fallback. |
| Newland integration | **Broadcast-intent** model (`nlscan.action.*`). No SDK/AAR binding required. |
| Urovo integration | **In-process `android.device.ScanManager`** API (open/start/stop), receive decodes via its `ACTION_DECODE` broadcast. |
| Cross-vendor strategy | A **Hardware Abstraction Layer (HAL)**: `Scanner` + `NfcReader` Kotlin interfaces, per-vendor impls, selected at runtime via `Build.MANUFACTURER`/`Build.BRAND`, exposed as `Flow<ScanEvent>`. |
| NFC badge (card UID) | Read **HF ISO14443-A/B UID via standard Android `NfcAdapter` reader-mode** (`NfcA`/`IsoDep`/`MifareClassic`). ⚠ validate the standard API is exposed on each model; vendor SDK only if not. |
| UHF RFID | Vendor RFID SDK (separate, model-specific) — out of scope for login; only if a future feature needs it. |
| Distribution / config | **EMM/MDM managed configuration** (`RestrictionsManager`) to push the API base URL and inject the per-device `deviceId`/`deviceSecret` (see auth spec Flow 0). Urovo ships an **OEMConfig** app for device settings. |

---

## 2. Device landscape

Primary fleet vendors: **Newland AIDC** (MT90, NFT10, N7, MT37 …) and **Urovo** (DT40, RT40,
i6310, DT50/DT50D …). Both are standard AOSP Android with vendor add-ons; both expose the
scan engine via Android (broadcast or service), so a single APK targets both with a thin per-vendor layer.

**Accessories observed across the fleet** (use what each model has; gate by capability):
- 1D/2D **barcode/QR scan engine** (hardware trigger keys) — *primary for login*.
- **HF NFC reader** (13.56 MHz): DT40 lists ISO15693 / ISO14443-A&B / MIFARE / FeliCa; DT50D lists NFC — *for badge/card login*.
- **UHF RFID** (e.g. DT50D RFID variant) — vendor SDK, model-specific.
- Camera, fingerprint, PSAM/smartcard (`Icc`/`PICC`/`Mag`), thermal printer, PinPad (Urovo SDK module names).

> ⚠ Accessory availability is **per-model**. Build a capability check (NFC present? scan engine
> vendor?) rather than assuming a uniform fleet.

---

## 3. Newland scanning — broadcast-intent model

Newland PDAs drive the scan engine through **system broadcasts**; the app needs **no SDK/AAR
binding** for basic scan-and-receive. (A separate bound SDK, `nlscan_master_sdk.jar`, exists for
advanced control but is not needed for login.) Verified against Newland's `ScanDemo` + Handbook V1.07.

**Receive results** — register a `BroadcastReceiver` for:
- Action: `nlscan.action.SCANNER_RESULT`
- Extras: `SCAN_BARCODE1` (the decoded string), `SCAN_BARCODE_TYPE` (symbology), `SCAN_STATE` (`"ok"` / fail)

**Control the engine** — `sendBroadcast(...)`:
- `nlscan.action.SCANNER_TRIG` — soft (programmatic) trigger; `SCAN_TIMEOUT` extra = 1–9 (seconds)
- `nlscan.action.STOP_SCAN` — stop scanning
- `ACTION_BAR_SCANCFG` — configure the engine; trigger mode via `EXTRA_TRIG_MODE`

```kotlin
// Newland — receive decodes
class NewlandScanReceiver(private val onScan: (String, String) -> Unit) : BroadcastReceiver() {
    override fun onReceive(ctx: Context, i: Intent) {
        if (i.getStringExtra("SCAN_STATE") != "ok") return
        val data = i.getStringExtra("SCAN_BARCODE1") ?: return
        val sym  = i.getStringExtra("SCAN_BARCODE_TYPE").orEmpty()
        onScan(data, sym)
    }
}
context.registerReceiver(receiver, IntentFilter("nlscan.action.SCANNER_RESULT"))

// Soft trigger from the login screen
context.sendBroadcast(Intent("nlscan.action.SCANNER_TRIG").putExtra("SCAN_TIMEOUT", 5))
```

> ⚠ Exact extra-key spellings (`SCAN_BARCODE1` vs `EXTRA_SCAN_DATA`) and `ACTION_BAR_SCANCFG`
> config keys vary by firmware/handbook revision. Confirm against the V1.07 handbook on your
> target firmware; the demo repo is the most reliable reference. Newland's `NlsScanManager`
> method signatures and the full symbology table were **not** covered by primary sources.

---

## 4. Urovo scanning — in-process `ScanManager` API

Urovo exposes a native **in-process** API class **`android.device.ScanManager`** (no-arg
constructor; **not** `bindService()`), plus a broadcast for delivering decodes. Verified against
Urovo's developer reference + `SDK_ReleaseforAndroid/Samples/ScanManager` demo.

**Lifecycle / control** (mirror the sample — open in `onResume`, close in `onPause`):
- `openScanner()` / `closeScanner()`
- `startDecode()` / `stopDecode()` — soft trigger / stop
- `setTriggerMode(...)` — `HOST` (software), `CONTINUOUS`, `PULSE`
- `switchOutputMode(mode)` — **0 = Intent (broadcast)**, 1 = TextBox (keystroke/wedge)
- Custom broadcast action via `PropertyID.WEDGE_INTENT_ACTION_NAME`

**Receive results** — `BroadcastReceiver` for `ScanManager.ACTION_DECODE`, extras:
- `ScanManager.DECODE_DATA_TAG` (raw bytes) / `BARCODE_STRING_TAG` (string)
- `BARCODE_LENGTH_TAG`, `BARCODE_TYPE_TAG`

```kotlin
// Urovo — open + soft trigger + receive
private val scan = android.device.ScanManager()

fun onResume() {
    scan.openScanner()
    scan.switchOutputMode(0)             // Intent mode
    scan.setTriggerMode(/* HOST */)
}
fun onPause() { scan.closeScanner() }

class UrovoScanReceiver(val onScan: (String, Int) -> Unit) : BroadcastReceiver() {
    override fun onReceive(ctx: Context, i: Intent) {
        val s = i.getStringExtra(ScanManager.BARCODE_STRING_TAG) ?: return
        val type = i.getIntExtra(ScanManager.BARCODE_TYPE_TAG, -1)
        onScan(s, type)
    }
}
context.registerReceiver(receiver, IntentFilter(ScanManager.ACTION_DECODE))
context.sendBroadcast(Intent()) // or: scan.startDecode()
```

> ⚠ The official `SDK_ReleaseforAndroid` samples date to ~May 2020 and enumerate I6310 / DT50 /
> DT40 / DT30. Newer firmware and **RT40** may differ; verify constant names against the SDK
> version on your devices. Add the SDK jar/aar or declare `android.device.ScanManager` per Urovo's
> integration notes.

### Newland vs Urovo — which model on a login screen
- **Newland:** broadcast model — lightest, fully decoupled, no library. Register receiver while the
  login screen is foreground, soft-trigger on demand or on hardware key.
- **Urovo:** in-process API — open the scanner in `onResume`, close in `onPause`; deliver via Intent
  mode so the same `BroadcastReceiver`-style path feeds the HAL. (Use **Intent**, not TextBox/wedge,
  so input never lands in a focused field by accident.)
- The HAL (§7) normalizes both into one `Flow<ScanEvent>`, so UI code is vendor-blind.

---

## 5. NFC badge / card UID for login

For badge sign-in we only need the **card UID** (a stable per-card identifier) — which becomes the
`value` hashed into a `UserSignInCredential` (auth spec Flow B/C). UID reading is a standard Android
capability and should **not** require a vendor SDK for HF ISO14443.

**Recommended: standard `NfcAdapter` reader mode** (foreground, login screen only):

```kotlin
nfcAdapter.enableReaderMode(activity, { tag ->
    val uid = tag.id.joinToString("") { "%02X".format(it) }   // stable UID
    // -> POST /auth/device/credential-login { value = uid }
}, NfcAdapter.FLAG_READER_NFC_A or NfcAdapter.FLAG_READER_NFC_B
   or NfcAdapter.FLAG_READER_SKIP_NDEF_CHECK, null)
```

- Manifest: `<uses-permission android:name="android.permission.NFC"/>`,
  `<uses-feature android:name="android.hardware.nfc" android:required="false"/>` (false → fleet can include non-NFC models).
- `Tag.id` gives the UID for `NfcA`/`NfcB`/`IsoDep`/`MifareClassic` without reading card contents.
- Reader mode (not `enableForegroundDispatch`) is preferred for a dedicated tap-to-login screen — no
  activity relaunch, tighter control.

> ⚠ **Validate on hardware (open question):** the research confirmed DT40/DT50D *hardware* supports
> ISO14443-A/B/MIFARE/FeliCa, but **did not confirm** whether each Newland/Urovo model surfaces HF
> NFC through the standard `NfcAdapter` or only via a proprietary reader SDK. Test `NfcAdapter` on
> MT90/N7/NFT10 and DT40/DT50 early. If a model hides HF behind a vendor SDK, add a per-vendor
> `NfcReader` impl behind the same HAL interface — UI code is unaffected.
>
> ⚠ **UHF RFID** (where present) is a *separate, model-specific vendor SDK*, not `NfcAdapter`, and is
> **out of scope** for login. Only wire it up if a future material/asset-tracking feature needs it.

---

## 6. Camera fallback (cross-device) + other accessories

**Camera barcode/QR fallback** — for any device without a usable scan engine, or for the web-QR
pairing screen if the imager misreads on-screen QR:
- **CameraX + ML Kit Barcode Scanning** — on-device, free, supports **13 formats** (9 1D + 4 2D incl.
  QR). Use as a fallback only; the hardware imager is faster and ergonomically better on a PDA.

**Other accessories** (only if a feature needs them — not for login):
- **Fingerprint:** prefer AndroidX `BiometricPrompt` if the model routes the sensor through the
  standard framework; otherwise vendor SDK. ⚠ per-model.
- **PSAM / smartcard (`Icc`/`PICC`/`Mag`), thermal printer, PinPad:** Urovo `SDK_ReleaseforAndroid`
  has sample modules; Newland equivalents are model-specific. ⚠ signatures not covered by research —
  pull the vendor SDK when scoped.

---

## 7. Recommended architecture — vendor-agnostic HAL

Keep all vendor specifics behind two small Kotlin interfaces; the rest of the app (login VM, UI)
is vendor-blind and testable with fakes.

```kotlin
// --- HAL contracts ---
sealed interface ScanEvent { data class Decoded(val data: String, val symbology: String) : ScanEvent }

interface Scanner {
    val scans: Flow<ScanEvent>        // hot flow fed by the active vendor receiver/API
    fun softTrigger(timeoutSec: Int = 5)
    fun stop()
}

interface NfcReader {
    val tags: Flow<String>            // emits card UID hex
    fun start(activity: Activity); fun stop(activity: Activity)
}

// --- runtime vendor selection ---
enum class Vendor { NEWLAND, UROVO, GENERIC }
fun detectVendor(): Vendor = when {
    Build.MANUFACTURER.equals("Newland", true) || Build.BRAND.contains("NLS", true) -> Vendor.NEWLAND
    Build.MANUFACTURER.contains("Urovo", true) || Build.BRAND.contains("Urovo", true) -> Vendor.UROVO
    else -> Vendor.GENERIC   // camera + standard NfcAdapter
}

// --- Hilt wiring ---
@Module @InstallIn(SingletonComponent::class)
object HardwareModule {
    @Provides @Singleton
    fun scanner(@ApplicationContext c: Context): Scanner = when (detectVendor()) {
        Vendor.NEWLAND -> NewlandScanner(c)     // registers nlscan.action.SCANNER_RESULT
        Vendor.UROVO   -> UrovoScanner(c)       // android.device.ScanManager + ACTION_DECODE
        Vendor.GENERIC -> CameraMlKitScanner(c) // CameraX + ML Kit
    }
    @Provides @Singleton
    fun nfc(@ApplicationContext c: Context): NfcReader = StandardNfcReader(c) // NfcAdapter; swap per-vendor if needed
}
```

Design notes:
- **`Flow` over callbacks** so the login `ViewModel` just `collect`s scan/UID events; vendor receivers
  are registered/unregistered with the screen lifecycle (`repeatOnLifecycle(STARTED)`).
- **Foreground-only** scanning on the login screen (register receiver / `openScanner` in
  `onResume`/`STARTED`, tear down in `onPause`/`STOPPED`) — no long-running background scan service
  needed for auth. (A bound foreground service is only worth it if scanning must survive backgrounding.)
- **`GENERIC` impl** (camera + `NfcAdapter`) doubles as the path for emulator/dev phones and any
  unrecognized device — keeps the app runnable off the target fleet.
- Hilt is Google's recommended DI (built on Dagger); it cleanly provides the right impl per device.

### Mapping to the auth feature (companion spec)
- **QR pairing (Flow A):** scan engine or camera decodes the `aeromes://pair?...` payload → `POST /auth/device/qr/redeem` with `X-Device-Id`/`X-Device-Secret` headers.
- **Card login (Flow C):** `NfcReader` UID → `POST /auth/device/credential-login { value = uid }`.
- **Tag login (Flow C):** scan engine decodes a 1D/2D badge → same credential-login endpoint.
- **Enrolment (Flow B):** same Scanner/NfcReader, but `POST /auth/me/credentials` with the Bearer token.

---

## 8. Platform targets

- **Language:** Kotlin; **AndroidX**; **Hilt**, Coroutines/Flow, CameraX, ML Kit, AndroidX Biometric.
- **minSdk = 31 (Android 12)** — confirmed by the team as the fleet floor. ⚠ Verify the lowest device
  actually runs ≥ 12 (`Build.VERSION.SDK_INT >= 31`) before locking it in; any model on 11 or below
  would be excluded. `targetSdk` = latest stable the EMM/Play requirements demand (currently 34/35).
- **API-31 implications to lean on:** `NfcAdapter.enableReaderMode` and `BiometricPrompt` are well
  past their min levels; CameraX/ML Kit/Hilt all fine. Note the **Android 12 `exported` requirement** —
  every `BroadcastReceiver`/`Activity` with an intent-filter (incl. the Newland/Urovo scan receivers,
  if statically registered) **must declare `android:exported`**. Prefer **context-registered**
  receivers (`RECEIVER_NOT_EXPORTED` flag) for the scan path so they aren't system-exported.
- Treat scan-engine and NFC API surfaces as **runtime capabilities**, not compile-time guarantees.

---

## 9. Distribution & provisioning (EMM / MDM)

The shared-fleet auth model (auth spec Flow 0) needs each device to carry a `deviceId` +
`deviceSecret` and to know the API base URL — **inject these via EMM managed configuration**, not
hand entry.

- App declares managed-config keys via an **`android.content.APP_RESTRICTIONS`** `<meta-data>` →
  `res/xml/app_restrictions.xml`; reads them at runtime with
  **`RestrictionsManager.getApplicationRestrictions()`** (a `Bundle`). Stable since API 21; standard
  across EMMs (Android Enterprise managed configs).
- Suggested managed keys: `apiBaseUrl`, `deviceId`, `deviceSecret`, `idleLogoutMinutes`, `environment`.
  The EMM (Intune, Hexnode, SOTI, etc.) sets these per device/group; the app reads them on launch and
  stores `deviceSecret` in **Android Keystore / EncryptedSharedPreferences**.
- **Urovo OEMConfig** (`com.urovo.oemconfigs`) exposes device-level settings (ScanService, NFC, IME,
  scanner) via the EMM — use it to standardize scan-engine settings across the fleet.
- ⚠ A **Newland OEMConfig / managed-config app** equivalent was **not verified** — confirm with
  Newland; otherwise rely on app-level managed config + manual device-settings baseline.

---

## 10. Recommended build order

1. **Skeleton + HAL contracts** (`Scanner`, `NfcReader`, Hilt module, `GENERIC` camera/NfcAdapter impl) — runs on a dev phone.
2. **Vendor scanners** — `NewlandScanner` (broadcast) and `UrovoScanner` (`ScanManager`), validated on one device each.
3. **QR pairing login** end-to-end against the backend (`/auth/device/qr/redeem`), device secret from managed config.
4. **NFC card login** — `StandardNfcReader`, validate `NfcAdapter` UID on each model (⚠ §5).
5. **Barcode tag login** + **manual/idle logout** (auth spec Flow D).
6. **Camera fallback** + capability gating + EMM managed-config rollout.

---

## 11. Open questions to validate on hardware (before/early in build)

1. **HF NFC API surface:** does `NfcAdapter` reader-mode return the UID on MT90 / N7 / NFT10 and DT40 / DT50, or is a vendor reader SDK required? *(§5)*
2. **Fleet Android versions:** `minSdk = 31 (Android 12)` set by the team — confirm **no** in-service model runs ≤ Android 11. *(§8)*
3. **Newland managed-config / OEMConfig:** is there an equivalent to `com.urovo.oemconfigs` for pushing scan/NFC settings + app config? *(§9)*
4. **Exact constant names** for Newland extras (`SCAN_BARCODE1` etc.) and Urovo SDK tags on the *firmware you actually ship* — confirm against on-device handbook/SDK version. *(§3, §4)*
5. **`NlsScanManager` / advanced bound-SDK** signatures, symbology tables, and UHF/fingerprint/PSAM/printer SDK APIs — pull from the vendor SDK when those features are scoped (not needed for login).

---

## 12. Sources (primary, verified)

**Newland**
- `NewlandAutoID/ScanDemo` (official demo) — broadcast actions/extras: https://github.com/NewlandAutoID/ScanDemo
- *Newland Android PDA API Handbook V1.07* (2024-09): https://www.newland-id.com/sites/default/files/documents/2024-09/Newland%20Android%20PDA%20API%20Handbook%20V1.07.pdf
- Community sample (MT90): https://github.com/prakashinfotech/NewLand-Scanner-DeviceMT90

**Urovo**
- ScanManager developer reference: https://www.urovo.com/developer/android/device/ScanManager.html
- Official SDK samples (`ScanManagerDemo.java`, accessory modules): https://github.com/urovosamples/SDK_ReleaseforAndroid
- DT40 spec (NFC standards): https://us.urovo.com/products/mobile/dt40.html · DT50D: https://en.urovo.com/products/rfid-data/dt50d.html
- Urovo OEMConfig: https://hexnode.com/marketplace/products/urovo-oemconfig/

**Android platform**
- ML Kit Barcode Scanning (13 formats): https://developers.google.com/ml-kit/vision/barcode-scanning/android
- Hilt DI: https://developer.android.com/training/dependency-injection/hilt-android
- Managed configurations / `RestrictionsManager`: https://developer.android.com/work/managed-configurations

> **Confidence:** vendor scan-API facts and Android-platform facts are **high** (primary sources,
> adversarially verified). NFC-via-`NfcAdapter`, the "broadcast is lighter" ranking, and minSdk
> guidance are **inferences/medium** — flagged ⚠ and listed in §11 for hardware validation.
