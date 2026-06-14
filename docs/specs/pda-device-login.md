# Feature Spec — PDA Device Login (QR Pairing + Card/Tag Sign-in)

**Status:** Draft for review
**Author:** (design assist)
**Date:** 2026-06-14
**Affects:** `AeroMes.Domain/Auth`, `AeroMes.Application/Auth`, `AeroMes.Infrastructure`, `AeroMes.Api` (Auth), `web/` (QR generation UI + credential management), Android PDA app.

---

## 1. Summary

Operators on the shop floor use a **fixed fleet of shared, company-owned Android PDA
handhelds**. A handful of devices per line are **shared by many employees**; a worker picks up
whatever device is free, signs in, does their task, and **manually signs out** (or is auto-signed-out
on idle) so the next worker can use it. Typing an email + password (and clearing MFA) on a rugged
keypad is slow and error-prone. This feature adds two faster, hardware-friendly sign-in methods,
with a deliberate bootstrap dependency between them:

1. **QR sign-in** — an already-authenticated web session generates a **single-use, short-lived
   QR code**. The worker scans it on any fleet device and is signed in as that user. (Used for
   the first sign-in and any time the worker has no enrolled card.)
2. **Card / Tag sign-in** — after a user has signed in *at least once via QR*, they
   **enrol an RFID/NFC card or barcode user-tag**. On subsequent shifts they just tap the card /
   scan the tag on any fleet device and are signed in instantly.

**Devices are always trusted** — the fleet is fixed and pre-provisioned by an admin (§4.0), so
"trust" is established once per physical device, *not* per QR scan. The QR flow is instead the
**user-level root of trust**: it is the only way to reach the authenticated screen where a worker
enrols their card/tag. Cards/tags are low-entropy bearer secrets, so they are only honoured on
**provisioned fleet devices** (which carry an installed device secret) and, optionally, gated by a PIN.

Because devices are **shared**, every session is short-lived and ends explicitly: **manual logout**
is a primary UI action and an **idle auto-logout** backstops a worker who walks away without signing out.

---

## 2. Goals / Non-goals

### Goals
- Let a web-authenticated user transfer their session onto any shared fleet device by scanning one QR (single-use, TTL ≈ 90s).
- Admin-provision a **fixed fleet** of always-trusted devices once; restrict card/tag login to that company hardware.
- Self-service card/tag enrolment from the PDA *after* QR login, plus management (list/revoke) from web and PDA.
- Daily tap-to-sign-in via card or tag on any fleet device — fast worker handoff.
- **Clean session handoff on shared hardware**: prominent manual logout + configurable idle auto-logout.
- Reuse the existing JWT/refresh-token machinery, audit logging, permission policies, and localization.
- Native-app-friendly token delivery (refresh token in the **response body**, not an HttpOnly cookie).

### Non-goals
- Replacing web (cookie) auth — unchanged.
- Replacing MFA for web/admin — unchanged.
- **Per-user device binding / BYOD** — devices are shared company assets, not personal. No "this is my phone" trust.
- Offline PDA auth (device must reach the API). A short offline grace window is an optional later phase.
- Biometric login — out of scope (could be a future credential `Type`).

---

## 3. Actors & terminology

| Term | Meaning |
|---|---|
| **Web user** | A person authenticated in the React web app, generating a pairing QR. |
| **PDA** | Android handheld with camera (QR), and NFC reader and/or barcode/laser scanner (card/tag). |
| **Pairing code** | One-time secret encoded in the QR. High entropy, hashed at rest, single-use, short TTL. |
| **Trusted device** | A `PdaDevice` row created/confirmed during a successful QR redemption; holds a long-lived hashed **device secret**. |
| **Sign-in credential** | A user's enrolled card UID or user-tag value, hashed at rest (`UserSignInCredential`). |
| **Card** | NFC/RFID tag read as a UID string. **User-tag** | Any scannable token (barcode/QR badge) read as a string. Both modelled as `UserSignInCredential.Type`. |

---

## 4. UX flows

### Flow 0 — Device provisioning (admin, once per physical device)

The fleet is fixed, so each handheld is registered **once** by an admin (or by IT during MDM
rollout). This is what makes a device "trusted" — there is no per-user device pairing.

```
 Admin (web)                     API                                   PDA (unprovisioned)
 ───────────                     ───                                   ───────────────────
 Devices → "Add device"
 POST /devices                ► create PdaDevice{Active}
   { name:"PDA-Line3-07" }      generate deviceSecret (raw, once)
        ◄──────────────────────  { deviceId, deviceSecret, enrolToken/QR }
 show one-time enrol QR ─────────────────────────────────────────────► admin scans on the PDA
                                                                         store deviceId + deviceSecret
                                                                         in Keystore (one time)
```

- `deviceSecret` is shown **once**; if lost, admin rotates it (`POST /devices/{id}/rotate-secret`)
  and re-enrols the device. Provisioning can also be pushed silently via MDM instead of the QR.
- After this, the device is a permanent, always-trusted fleet member until an admin **revokes** it
  (lost/decommissioned). Revoke kills all of that device's active sessions immediately.

### Flow A — QR sign-in (user session transfer onto a shared device)

```
 Web (authenticated)                 API                                  PDA (provisioned, logged out)
 ───────────────────                 ───                                  ─────────────────────────────
 click "Sign in to a PDA"
 POST /auth/device/qr  ───────────►  create DeviceLoginCode{Pending}
                                     code = 32 random bytes (raw)
                                     store SHA-256(code), TTL 90s
        ◄─────────────────────────  { pairingId, qrPayload, expiresAt }
 render QR + countdown
 poll GET /auth/device/qr/{id} ───►  status: Pending
                                                                          scan QR (camera)
                                          ◄──────────────────────────────  POST /auth/device/qr/redeem
                                                                            headers: X-Device-Id, X-Device-Secret
                                                                            { pairingId, code }
                                     validate device (provisioned & active)
                                     validate code hash + Pending + !expired
                                     atomic flip → Redeemed
                                     issue access + refresh (as issuing user)
                                          ──────────────────────────────►  { accessToken, refreshToken, user, forcePasswordChange }
 poll sees status: Redeemed ◄──────  status: Redeemed (deviceName)        store user tokens in memory/Keystore
 show "Signed in on <device>"                                            → enter app (or prompt to enrol card — Flow B)
```

Notes:
- The PDA signs in **as the web user who generated the QR** (session transfer onto shared hardware).
- The device is **already provisioned** (Flow 0) and proves itself via `X-Device-Id`/`X-Device-Secret`.
  QR redeem does **not** create devices or issue device secrets — an unprovisioned device is rejected.
- TTL is short because the QR is on-screen. Single-use enforced by the `Pending → Redeemed`
  atomic transition (see §6).
- The user's tokens are session-scoped to this handoff; **logout clears them** so the next worker
  starts clean (see §5).

### Flow B — Card / Tag enrolment (after QR login)

```
 PDA (authenticated via Flow A)            API
 ──────────────────────────────            ───
 Settings → "Add card / tag"
 user taps card  → read UID  ────────────► POST /auth/me/credentials   (Bearer access token)
 (optional) set a 4–6 digit PIN            { type: Card|Tag, value, label, pin? }
                                           hash value (+ pin), link to user
        ◄──────────────────────────────── { credentialId, type, label, last4 }
 "Card registered ✓"
```

- Requires a valid Bearer access token → enforces "QR completed first".
- A user may enrol multiple credentials (e.g. a primary card + a backup tag).
- Management: `GET /auth/me/credentials`, `DELETE /auth/me/credentials/{id}`. Web mirrors these.

### Flow C — Card / Tag fast sign-in (daily use, trusted device)

```
 PDA (logged out, but trusted)             API
 ─────────────────────────────             ───
 login screen → "Tap card"
 read UID (+ PIN if device policy) ───────► POST /auth/device/credential-login
                                            headers: X-Device-Id, X-Device-Secret
                                            body: { value, pin? }
                                            validate device (secret hash, active)
                                            hash value → find active credential → user
                                            verify pin (if set), user.IsActive, not ForcePasswordChange
                                            issue access + refresh (as that user)
        ◄──────────────────────────────── { accessToken, refreshToken, user }
 enter app
```

- No QR needed for daily use, but the device must be a **provisioned fleet device** (Flow 0)
  presenting its `deviceId` + `deviceSecret`. A cloned card is therefore useless off company hardware.
- Optional PIN turns this into two-factor (have-the-card + know-the-PIN).

### Flow D — Logout / worker handoff (shared device)

Because the device is shared, ending a session cleanly is essential.

```
 PDA (signed in)                          API
 ───────────────                          ───
 tap "Sign out"  ───────────────────────► POST /auth/device/logout   (Bearer)
                                           revoke this session's refresh-token family
        ◄──────────────────────────────── 204
 drop in-memory access token,             → back to "Tap card / Scan QR" screen
 wipe user tokens from storage
```

- **Manual logout** is a prominent, always-reachable action (header button), not buried in settings.
- **Idle auto-logout:** the app starts an inactivity timer (config `Auth:Pda:IdleLogoutMinutes`,
  e.g. 5). On expiry it calls `/auth/device/logout` and returns to the login screen, so an abandoned
  PDA never leaks a session. The device identity (`deviceId`/`deviceSecret`) is **retained** — only
  the *user* session is cleared.
- **Fast user switch:** tapping a *different* card while signed in = logout current + login new
  (with a brief "Switch to <name>?" confirm). Convenient on a busy line.
- The device stays provisioned/trusted across logouts; only an admin **device revoke** un-trusts it.

---

## 5. Token & session lifecycle for PDA

The web app uses an HttpOnly refresh **cookie**; a native PDA app cannot use that cleanly.
PDA endpoints therefore return the refresh token **in the response body** and the app stores it
in secure storage. The underlying `RefreshToken` entity, hashing, family rotation, and
`ITokenService.CreateToken` are reused unchanged.

| Concern | Web | PDA |
|---|---|---|
| Access token | JWT, 15 min (`Jwt:ExpiryMinutes`) | same |
| Refresh token | HttpOnly cookie, 7 days | **response body**, configurable `Jwt:PdaRefreshTokenDays` (e.g. 30) |
| Refresh call | `POST /auth/refresh` (reads cookie) | `POST /auth/device/refresh` (reads body/Authorization) |
| MFA | enforced | **bypassed** by QR (already MFA'd on web) and by card+PIN (see §7) |
| `wc_scope` claim | from `DefaultWorkCenterId` | same — shop-floor scoping preserved |
| Logout | clears cookie | `POST /auth/device/logout` revokes refresh family (manual + idle) |
| Idle timeout | (web inactivity) | `Auth:Pda:IdleLogoutMinutes` — auto-logout on shared device |

Because devices are **shared**, PDA refresh lifetime is the *session* lifetime, not a multi-day
convenience: a logout (manual or idle) revokes the family immediately, so the next worker on the
same handset cannot resume the previous worker's session. Keep `Jwt:PdaRefreshTokenDays` modest
(e.g. 1–7 days) — it only matters across an uninterrupted shift, since handoff forces re-auth anyway.

`DeviceInfo` on the `RefreshToken` is set to the `PdaDevice` name/fingerprint so existing
session-management UI (`Application/Auth/Sessions`) lists and can revoke PDA sessions.

---

## 6. Domain model (new)

`Domain/Auth/` — zero external deps, private setters, static factory + behaviour methods,
following `RefreshToken`/`WorkCenter` conventions.

### `PdaDevice`
```csharp
public class PdaDevice
{
    public Guid DeviceId { get; private set; }
    public string Name { get; private set; }            // human label, e.g. "PDA-Line3-07"
    public string Fingerprint { get; private set; }     // stable hardware id from the app
    public string DeviceSecretHash { get; private set; }// SHA-256 of the once-issued device secret
    public string ProvisionedByUserId { get; private set; } // admin who registered it (Flow 0)
    public DateTime CreatedAt { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt is null;             // always-trusted while active

    public static PdaDevice Provision(string name, string fingerprint, string secretHash, string provisionedBy);
    public void Touch();           // update LastSeenAt
    public void RotateSecret(string newSecretHash);
    public void Revoke();          // lost/decommissioned → un-trusts the device, kills its sessions
}
```

> Devices are **admin-provisioned and always trusted** while active. `Fingerprint` is optional here
> (MDM-pushed installs may not have a camera-scanned fingerprint); the authoritative identity is
> `DeviceId` + the installed `deviceSecret`.

### `DeviceLoginCode` (the QR pairing record)
```csharp
public class DeviceLoginCode
{
    public Guid PairingId { get; private set; }
    public string CodeHash { get; private set; }        // SHA-256 of the raw QR secret
    public string IssuedByUserId { get; private set; }
    public DeviceLoginCodeStatus Status { get; private set; } // Pending | Redeemed | Expired | Cancelled
    public Guid? RedeemedByDeviceId { get; private set; }
    public string? DeviceName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RedeemedAt { get; private set; }

    public static DeviceLoginCode Issue(string userId, string codeHash, DateTime expiresAt);
    public bool TryRedeem(Guid deviceId, string deviceName); // returns false if not Pending / expired
    public void Cancel();
}
```

> **Single-use enforcement:** redemption is a guarded UPDATE
> `... SET Status=Redeemed WHERE PairingId=@id AND Status=Pending AND ExpiresAt>@now`
> and the handler treats **rows-affected = 0** as "already used / expired" → `401`. This makes
> concurrent double-scans safe without a distributed lock.

### `UserSignInCredential`
```csharp
public class UserSignInCredential
{
    public long CredentialId { get; private set; }
    public string UserId { get; private set; }
    public SignInCredentialType Type { get; private set; } // Card | Tag
    public string CredentialHash { get; private set; }     // SHA-256(normalised value)
    public string? PinHash { get; private set; }           // optional, Identity password hasher
    public string Label { get; private set; }              // "Blue badge"
    public string Last4 { get; private set; }              // display only, non-sensitive
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt is null;

    public static UserSignInCredential Enrol(string userId, SignInCredentialType type, string hash, string label, string last4, string? pinHash);
    public void MarkUsed();
    public void Revoke();
}
```

**Uniqueness:** unique index on `CredentialHash` (a physical card maps to at most one active user).
Re-enrolling a card already bound to another user is rejected (`409`).

---

## 7. Security model

| Threat | Mitigation |
|---|---|
| QR screen-captured / shoulder-surfed | 90s TTL + single-use + high entropy (256-bit). Optional "confirm device on web after scan" step (§13). |
| Replayed QR redeem | Atomic `Pending→Redeemed`; second redeem gets `401`. |
| Cloned/stolen card | Card login only on **trusted devices** (valid `deviceId`+`deviceSecret`); optional per-credential or per-device **PIN**; rate-limit + lockout. |
| Card login bypasses MFA | By policy: card login yields a **non-MFA** session (`mfa_verified` absent). MFA-required users (admins) are **rejected** from card login and must use web/QR. Recommend card login be limited to shop-floor roles. |
| Lost PDA | Admin **revokes the `PdaDevice`** → its sessions + future card logins die immediately. |
| `ForcePasswordChange` user taps card | Rejected → must complete password change on web first. |
| Brute-forcing card UIDs | UIDs are sparse but low-entropy; require device secret (high entropy) as the real gate; rate-limit credential-login per device/IP. |
| Secret at rest | Pairing code, device secret, card value, and PIN are all **hashed** (SHA-256 for high-entropy secrets; Identity `IPasswordHasher` for the low-entropy PIN). Raw values returned at most once. |

Every step emits a `SecurityAuditEvent` via `IAuditLogger` (new `AuditEventTypes`:
`AuthDeviceQrIssued`, `AuthDeviceQrRedeemed`, `AuthDeviceQrFailed`, `AuthCredentialEnrolled`,
`AuthCredentialLogin`, `AuthCredentialLoginFailed`, `AuthDeviceRevoked`).

---

## 8. API contracts

All under `api/v1/auth`. New `DeviceAuthController` (keeps `AuthController` focused), same
controller-centric style as `AuthController`/`MfaController` (direct `UserManager`/`ITokenService`,
not CQRS) for token-issuing endpoints; CQRS handlers for device/credential **management** (§11).

| Method | Route | Auth | Purpose |
|---|---|---|---|
| POST | `/devices` | `permission:Device:Manage` | **Provision** a fleet device → `{deviceId, deviceSecret}` (once). |
| POST | `/devices/{deviceId}/rotate-secret` | `permission:Device:Manage` | Re-issue a lost device secret. |
| GET | `/devices` | `permission:Device:View` | Admin: list fleet devices. |
| DELETE | `/devices/{deviceId}` | `permission:Device:Manage` | Admin: revoke/decommission a device. |
| POST | `/auth/device/qr` | Bearer/Cookie (any authenticated user) | Issue a sign-in QR for the caller. |
| GET | `/auth/device/qr/{pairingId}` | same caller | Poll status (Pending/Redeemed/Expired). |
| DELETE | `/auth/device/qr/{pairingId}` | same caller | Cancel an outstanding code. |
| POST | `/auth/device/qr/redeem` | Device (`X-Device-Id`/`X-Device-Secret`) | Provisioned PDA redeems code → user tokens. |
| POST | `/auth/device/refresh` | Device + refresh in body | PDA token refresh (body, not cookie). |
| POST | `/auth/device/logout` | Bearer | End the current user session (manual + idle). |
| POST | `/auth/device/credential-login` | Device (`X-Device-Id`/`X-Device-Secret`) | Card/tag sign-in. |
| GET | `/auth/me/credentials` | Bearer | List caller's credentials. |
| POST | `/auth/me/credentials` | Bearer | Enrol a card/tag (Flow B). |
| DELETE | `/auth/me/credentials/{id}` | Bearer | Revoke one. |

### Request/response records (named records — register each in `AeroMesJsonContext`)

```csharp
// Issue
public record IssueDeviceQrRequest(string? DeviceLabelHint);
public record IssueDeviceQrResult(Guid PairingId, string QrPayload, DateTimeOffset ExpiresAt);

// Poll
public record DeviceQrStatusResult(string Status, string? DeviceName, DateTimeOffset? RedeemedAt);

// Provision (admin, Flow 0)
public record ProvisionDeviceRequest(string Name, string? Fingerprint);
public record ProvisionDeviceResult(Guid DeviceId, string DeviceSecret); // DeviceSecret shown once

// Redeem (PDA — device proven via X-Device-Id/X-Device-Secret headers)
public record RedeemDeviceQrRequest(Guid PairingId, string Code);
public record DeviceLoginResponse(
    string AccessToken, string TokenType, int ExpiresInSeconds,
    string RefreshToken,                 // body, not cookie
    string Email, string FullName, IReadOnlyList<string> Roles, bool ForcePasswordChange);

// PDA refresh
public record DeviceRefreshRequest(string RefreshToken);
public record DeviceRefreshResult(string AccessToken, string TokenType, int ExpiresInSeconds, string RefreshToken);

// Credential enrol / list
public record EnrolCredentialRequest(SignInCredentialType Type, string Value, string Label, string? Pin);
public record CredentialResult(long CredentialId, SignInCredentialType Type, string Label, string Last4, DateTimeOffset CreatedAt, DateTimeOffset? LastUsedAt);

// Credential login (PDA)
public record CredentialLoginRequest(string Value, string? Pin);  // device proven via headers
```

**`QrPayload`** is a compact URI the PDA parses, e.g.
`aeromes://pair?v=1&id={pairingId}&c={base64url(code)}` (API base URL comes from PDA config,
not the QR, so codes don't leak environment). Web renders it with a QR library (`qrcode.react`).

---

## 9. Data / migrations

New tables (EF Core, `Infrastructure/Data` configs in `Configurations/`):
- `PdaDevices` — unique index on `Fingerprint`; index on `EnrolledByUserId`.
- `DeviceLoginCodes` — index on `(Status, ExpiresAt)` for the redeem guard + cleanup; index on `IssuedByUserId`.
- `UserSignInCredentials` — **unique** index on `CredentialHash`; index on `UserId`.

Migration: `dotnet ef migrations add PdaDeviceLogin --project src/AeroMes.Infrastructure --startup-project src/AeroMes.Api`.

A background cleanup (hosted service or on-write sweep) deletes `Expired/Redeemed` login codes
older than N hours to keep the table small.

---

## 10. Config (appsettings)

```jsonc
"Auth": {
  "Pda": {
    "QrTtlSeconds": 90,
    "RefreshTokenDays": 7,                    // short — sessions end at worker handoff
    "IdleLogoutMinutes": 5,                   // shared-device auto-logout
    "RequireWebConfirmAfterScan": false,     // §13 hardening toggle
    "CredentialLogin": {
      "RequireTrustedDevice": true,
      "RequirePin": false,                    // org-wide default; per-credential PIN still allowed
      "MaxAttemptsPerDevice": 5,
      "LockoutMinutes": 15,
      "DisallowMfaRequiredUsers": true
    }
  }
}
```

---

## 11. Backend implementation plan (file-by-file)

Follows `/code-style`: one folder per use-case, `ValidationResult<T>` for commands, validators
co-located and invoked at the top of `HandleAsync`, repository interfaces in Application,
`record` DTOs, JSON source-gen registration.

```
Domain/Auth/
  PdaDevice.cs
  DeviceLoginCode.cs            (+ enum DeviceLoginCodeStatus)
  UserSignInCredential.cs       (+ enum SignInCredentialType)

Application/Interfaces/
  IPdaDeviceRepository.cs
  IDeviceLoginCodeRepository.cs
  IUserSignInCredentialRepository.cs
  ITokenService.cs              (add CreateRefreshToken? — see note)

Application/Auth/Devices/Commands/
  ProvisionDevice/              (record + handler + validator + Messages.resx)
  RotateDeviceSecret/
  RevokeDevice/
  IssueDeviceQr/
  RedeemDeviceQr/
  EnrolCredential/
  RevokeCredential/
  CredentialLogin/
  LogoutDevice/
Application/Auth/Devices/Queries/
  GetDeviceQrStatus/
  ListMyCredentials/
  ListDevices/

Infrastructure/
  Repositories/PdaDeviceRepository.cs
  Repositories/DeviceLoginCodeRepository.cs
  Repositories/UserSignInCredentialRepository.cs
  Data/Configurations/PdaDeviceConfiguration.cs
  Data/Configurations/DeviceLoginCodeConfiguration.cs
  Data/Configurations/UserSignInCredentialConfiguration.cs

Api/
  Controllers/DeviceAuthController.cs
  Serialization/AeroMesJsonContext.cs   (register all new records)
  Auth/DeviceAuthHandler.cs              (validates X-Device-Id/X-Device-Secret for credential-login)
```

**Token-issuance note:** the raw-refresh-token creation + hashing currently lives privately in
`AuthController`/`MfaController` (`CreateRefreshToken`/`HashToken`). To avoid a third copy,
**extract it into `ITokenService` / a `RefreshTokenFactory`** and have all three controllers use it.
This is a small refactor worth doing as part of this feature (flag for review).

**Permissions:** add `Device:View`, `Device:Manage` to the permission seed; map to admin roles.

---

## 12. Web UI changes (`web/`)

- New page/dialog **"Sign in a device"** (e.g. under profile menu or `/settings/devices`):
  - Calls `POST /auth/device/qr`, renders QR (`qrcode.react`) with a live TTL ring, polls status,
    shows "Paired with <device>" on success, "Regenerate" on expiry.
- New **"Trusted devices"** admin page: list (`GET /devices`), revoke (`DELETE /devices/{id}`).
- **My credentials** section in profile: list/revoke the user's cards/tags (enrol stays PDA-only,
  since it needs the hardware reader).
- Run `npm run fetch:spec && npm run generate:api` after the API contracts land; new Orval hooks
  appear under the `device-auth` tag.
- New i18n keys in `web/src/locales/*/auth.json` (vi default + en-US).

## 13. Android PDA app responsibilities (informative)

- **Config:** API base URL provisioned out-of-band (MDM / build flavour), not in the QR.
- **QR scan:** CameraX + ML Kit barcode scanner → parse `aeromes://pair?...` → `redeem`.
- **Secure storage:** `accessToken` (memory/short), `refreshToken` + `deviceId` + `deviceSecret`
  in Android Keystore / `EncryptedSharedPreferences`. Never log secrets.
- **Card/tag read:** NFC `IsoDep`/`NfcA` UID, or integrated barcode scanner intent → `value`.
- **Token refresh:** call `/auth/device/refresh` on 401; on refresh failure, fall back to card/tag
  or QR login screen.
- **Device fingerprint:** stable per-install id (e.g. `ANDROID_ID` or a generated UUID persisted in Keystore).

---

## 14. Localization & audit

- All user-facing messages localized (vi default, en-US) per resource-file convention; per-use-case
  `*Messages.resx` co-located with each handler.
- Audit every issue/redeem/enrol/login/revoke with IP + user-agent + device id, mirroring existing
  `AuthController` audit calls.

---

## 15. Edge cases

- **Double scan / race:** guarded UPDATE → only one wins, others `401`.
- **Expired QR:** `redeem` → `401 "Mã đã hết hạn"`; web poll surfaces "Expired", offers regenerate.
- **Same device re-paired:** fingerprint already known → reuse `PdaDevice`, rotate & return new device secret.
- **Card already bound to another user:** enrol → `409`.
- **Card login for MFA-required / disabled / force-password-change user:** `403`/`401` with a clear message → use web.
- **Revoked device tries credential-login:** `401`; its refresh tokens are also revoked on device revoke.
- **User offboarded (`IsActive=false`):** all login paths reject; cascade-revoke their credentials.
- **Clock skew on TTL:** server-side UTC only; PDA never decides expiry.

## 16. Testing (xUnit + Testcontainers)

- `DeviceQrFlowTests`: issue → poll Pending → redeem → poll Redeemed; tokens valid; second redeem `401`; expired `401`; cancel works.
- `CredentialEnrolTests`: enrol requires Bearer; duplicate card `409`; list/revoke; `last4` correct, raw value never returned.
- `CredentialLoginTests`: happy path on trusted device; wrong/absent device secret `401`; PIN required/incorrect; MFA-required user blocked; revoked device blocked; lockout after N attempts.
- `DeviceRefreshTests`: body refresh rotates family; revoking device kills refresh.

---

## 17. Open decisions (need a call before build)

1. ~~**QR semantics — session transfer vs. admin provisioning.**~~ **DECIDED:** both, at different layers —
   admin provisions the **fixed shared fleet** once (Flow 0, device trust), and QR sign-in does the
   **per-user session transfer** onto a shared device (Flow A). Devices are always trusted; users come and go.
2. **PIN with card/tag — required, optional, or off?**
   *Recommended:* **optional, org-configurable** (`RequirePin`), with per-credential PIN allowed even when
   org default is off. True 2-factor where it matters, fast tap where it doesn't.
3. **Web-confirm-after-scan hardening.**
   *Recommended:* **off by default** (`RequireWebConfirmAfterScan=false`) for shop-floor speed; switch on
   for high-security sites. Adds a "PDA scanned — approve?" step on web before tokens are issued.
4. **MFA users via card.** *Recommended:* **block** (`DisallowMfaRequiredUsers=true`); admins use web/QR only.
5. **PDA refresh lifetime.** *Recommended:* **short (1–7 days)** — devices are shared, so a session ends at
   handoff (logout/idle) anyway; a long refresh only helps within one uninterrupted shift.
6. **Idle auto-logout window.** *Recommended:* **5 minutes** (`Auth:Pda:IdleLogoutMinutes`), tunable per site —
   short enough that an abandoned shared PDA doesn't leak a session, long enough not to fight a busy operator.

---

## 18. Suggested phasing

- **Phase 1 — Device fleet + QR sign-in** (Flows 0 & A) + admin provisioning/list/revoke + PDA body-refresh
  + manual & idle **logout** (Flow D). Delivers faster login + clean shared-device handoff; everything builds on it.
- **Phase 2 — Card/Tag enrolment + sign-in** (Flows B & C) + PIN policy + web credential management.
- **Phase 3 (optional)** — web-confirm hardening, short offline grace window, biometric credential type.
