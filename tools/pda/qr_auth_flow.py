#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.11"
# dependencies = ["httpx>=0.27"]
# ///
"""
PDA device-auth reference client for AeroMes.

Exercises the device sign-in handshake described in docs/specs/pda-device-login.md so the
Android team has a runnable reference *before* the native app exists. It plays BOTH sides:
the "web" side (issue + poll a QR) and the "PDA" side (redeem → refresh → logout), plus the
admin "provision a device" step (Flow 0).

  ┌── admin ──┐   ┌─────── web (authenticated user) ───────┐   ┌──────── PDA (device) ────────┐
  provision-device   qr-issue ─► qr-poll(Pending) ............  redeem ─► refresh ─► logout
                                          └────────── qr-poll(Redeemed) ◄┘

⚠ These endpoints are the DESIGNED contract (spec §8) and may not be implemented on the server
  yet. Against a live build this runs end-to-end; against a stub you'll get 404s — that's expected
  and still documents the exact requests/headers the Android client must send.

Examples
  # one-shot full flow (admin token provisions a device, user token issues a QR, PDA redeems it):
  uv run tools/pda/qr_auth_flow.py flow \
      --base-url http://localhost:5170 \
      --admin-token "$ADMIN_JWT" --user-token "$USER_JWT" \
      --device-name "PDA-Line3-07"

  # individual steps:
  uv run tools/pda/qr_auth_flow.py provision-device --admin-token "$ADMIN_JWT" --device-name PDA-07
  uv run tools/pda/qr_auth_flow.py qr-issue        --user-token  "$USER_JWT"
  uv run tools/pda/qr_auth_flow.py redeem --device-id <guid> --device-secret <secret> \
      --payload 'aeromes://pair?v=1&id=<pairingId>&c=<base64url-code>'
"""
from __future__ import annotations

import argparse
import base64
import sys
import time
from urllib.parse import parse_qs, urlparse

import httpx

API = "/api/v1"


def _client(base_url: str) -> httpx.Client:
    return httpx.Client(base_url=base_url.rstrip("/"), timeout=15.0)


def _show(resp: httpx.Response) -> dict:
    print(f"  → {resp.request.method} {resp.request.url}  [{resp.status_code}]")
    try:
        body = resp.json()
    except Exception:
        body = resp.text
    print(f"    {body}")
    resp.raise_for_status()
    return body if isinstance(body, dict) else {}


def parse_payload(payload: str) -> tuple[str, str]:
    """aeromes://pair?v=1&id=<pairingId>&c=<base64url(code)> -> (pairingId, rawCode)."""
    q = parse_qs(urlparse(payload).query)
    pairing_id = q["id"][0]
    c = q["c"][0]
    raw = base64.urlsafe_b64decode(c + "=" * (-len(c) % 4)).decode()
    return pairing_id, raw


# ── admin (Flow 0) ────────────────────────────────────────────────────────────
def provision_device(c: httpx.Client, admin_token: str, name: str, fingerprint: str | None) -> dict:
    print("• provision device (admin)")
    return _show(c.post(f"{API}/devices",
                        headers={"Authorization": f"Bearer {admin_token}"},
                        json={"name": name, "fingerprint": fingerprint}))


# ── web side ──────────────────────────────────────────────────────────────────
def qr_issue(c: httpx.Client, user_token: str) -> dict:
    print("• issue sign-in QR (web user)")
    return _show(c.post(f"{API}/auth/device/qr",
                        headers={"Authorization": f"Bearer {user_token}"},
                        json={"deviceLabelHint": None}))


def qr_poll(c: httpx.Client, user_token: str, pairing_id: str, *, attempts: int = 1, delay: float = 1.0) -> dict:
    print(f"• poll QR status x{attempts} (web user)")
    last: dict = {}
    for _ in range(attempts):
        last = _show(c.get(f"{API}/auth/device/qr/{pairing_id}",
                           headers={"Authorization": f"Bearer {user_token}"}))
        if last.get("status") and last["status"].lower() != "pending":
            break
        time.sleep(delay)
    return last


# ── PDA side ──────────────────────────────────────────────────────────────────
def redeem(c: httpx.Client, device_id: str, device_secret: str, pairing_id: str, code: str) -> dict:
    print("• redeem QR (PDA)")
    return _show(c.post(f"{API}/auth/device/qr/redeem",
                        headers={"X-Device-Id": device_id, "X-Device-Secret": device_secret},
                        json={"pairingId": pairing_id, "code": code}))


def refresh(c: httpx.Client, device_id: str, device_secret: str, refresh_token: str) -> dict:
    print("• refresh access token (PDA)")
    return _show(c.post(f"{API}/auth/device/refresh",
                        headers={"X-Device-Id": device_id, "X-Device-Secret": device_secret},
                        json={"refreshToken": refresh_token}))


def logout(c: httpx.Client, access_token: str) -> dict:
    print("• logout (PDA)")
    return _show(c.post(f"{API}/auth/device/logout",
                        headers={"Authorization": f"Bearer {access_token}"}))


def run_flow(args) -> None:
    with _client(args.base_url) as c:
        device_id, device_secret = args.device_id, args.device_secret
        if args.admin_token and not device_id:
            prov = provision_device(c, args.admin_token, args.device_name, args.fingerprint)
            device_id, device_secret = prov.get("deviceId", ""), prov.get("deviceSecret", "")
        if not (device_id and device_secret):
            sys.exit("need --device-id/--device-secret (or --admin-token to provision one)")

        issued = qr_issue(c, args.user_token)
        pairing_id, code = parse_payload(issued["qrPayload"])
        qr_poll(c, args.user_token, pairing_id)              # expect Pending

        redeemed = redeem(c, device_id, device_secret, pairing_id, code)
        qr_poll(c, args.user_token, pairing_id)              # expect Redeemed

        if rt := redeemed.get("refreshToken"):
            refresh(c, device_id, device_secret, rt)
        if at := redeemed.get("accessToken"):
            logout(c, at)
        print("✓ flow complete")


def main() -> None:
    p = argparse.ArgumentParser(description="AeroMes PDA device-auth reference client")
    p.add_argument("--base-url", default="http://localhost:5170")
    sub = p.add_subparsers(dest="cmd", required=True)

    f = sub.add_parser("flow", help="run the full provision→issue→redeem→refresh→logout sequence")
    f.add_argument("--admin-token"); f.add_argument("--user-token", required=True)
    f.add_argument("--device-id"); f.add_argument("--device-secret")
    f.add_argument("--device-name", default="PDA-REF-01"); f.add_argument("--fingerprint")
    f.set_defaults(func=run_flow)

    pv = sub.add_parser("provision-device")
    pv.add_argument("--admin-token", required=True); pv.add_argument("--device-name", required=True)
    pv.add_argument("--fingerprint")
    pv.set_defaults(func=lambda a: provision_device(_client(a.base_url), a.admin_token, a.device_name, a.fingerprint))

    qi = sub.add_parser("qr-issue")
    qi.add_argument("--user-token", required=True)
    qi.set_defaults(func=lambda a: qr_issue(_client(a.base_url), a.user_token))

    qp = sub.add_parser("qr-poll")
    qp.add_argument("--user-token", required=True); qp.add_argument("--pairing-id", required=True)
    qp.add_argument("--attempts", type=int, default=5); qp.add_argument("--delay", type=float, default=1.0)
    qp.set_defaults(func=lambda a: qr_poll(_client(a.base_url), a.user_token, a.pairing_id, attempts=a.attempts, delay=a.delay))

    rd = sub.add_parser("redeem")
    rd.add_argument("--device-id", required=True); rd.add_argument("--device-secret", required=True)
    rd.add_argument("--payload", help="aeromes://pair?... QR payload")
    rd.add_argument("--pairing-id"); rd.add_argument("--code")
    def _redeem(a):
        pid, code = (parse_payload(a.payload) if a.payload else (a.pairing_id, a.code))
        if not (pid and code):
            sys.exit("provide --payload OR --pairing-id and --code")
        return redeem(_client(a.base_url), a.device_id, a.device_secret, pid, code)
    rd.set_defaults(func=_redeem)

    args = p.parse_args()
    try:
        args.func(args)
    except httpx.HTTPStatusError as e:
        sys.exit(f"\n✗ request failed: {e.response.status_code} (endpoint may not be implemented yet — see spec §8)")
    except httpx.ConnectError:
        sys.exit(f"\n✗ cannot reach {args.base_url} — is the API running? (dotnet run --project src/AeroMes.Api)")


if __name__ == "__main__":
    main()
