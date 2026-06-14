#!/usr/bin/env bash
# Gracefully stops all running Android emulators.
set -euo pipefail

export ANDROID_HOME="${ANDROID_HOME:-$HOME/android-sdk}"
export PATH="$ANDROID_HOME/platform-tools:$PATH"

echo "=== Stopping emulators ==="

DEVICES=$(adb devices 2>/dev/null | grep "emulator" | awk '{print $1}')

if [ -z "$DEVICES" ]; then
  echo "  No emulators running."
  exit 0
fi

for DEVICE in $DEVICES; do
  echo "  Killing $DEVICE"
  adb -s "$DEVICE" emu kill 2>/dev/null || true
done

# Wait for processes to exit
sleep 2

# Force-kill any leftover emulator processes
pkill -f "emulator.*-avd" 2>/dev/null || true

echo "Done."
