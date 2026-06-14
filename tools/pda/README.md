# PDA tooling

Reference utilities for the PDA device-auth feature. See the design specs:
[`docs/specs/pda-device-login.md`](../../docs/specs/pda-device-login.md) (backend) and
[`docs/specs/pda-android-foundation.md`](../../docs/specs/pda-android-foundation.md) (Android).

## `qr_auth_flow.py`

A runnable reference client for the device sign-in handshake — it plays the admin (provision),
web (issue/poll QR), and PDA (redeem/refresh/logout) sides so the Android team has an exact
request/header reference before the native app exists.

Runs via [uv](https://docs.astral.sh/uv/) with inline (PEP 723) deps — no venv setup:

```bash
uv run tools/pda/qr_auth_flow.py --help

# full flow against a running API:
uv run tools/pda/qr_auth_flow.py flow \
  --base-url http://localhost:5170 \
  --admin-token "$ADMIN_JWT" --user-token "$USER_JWT" --device-name PDA-Line3-07
```

> ⚠ The endpoints are the **designed** contract (spec §8) and may not be implemented on the
> server yet. Against a stub you'll get 404s — that still documents the exact calls the Android
> client must make. Start the API with `dotnet run --project src/AeroMes.Api`.
