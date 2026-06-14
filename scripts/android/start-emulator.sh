#!/usr/bin/env bash
# Starts the Android emulator in headless mode and waits for full boot.
# Usage: ./start-emulator.sh [avd-name] [boot-timeout-seconds]
set -euo pipefail

AVD_NAME="${1:-android12_ci}"
TIMEOUT="${2:-240}"

export ANDROID_HOME="${ANDROID_HOME:-$HOME/android-sdk}"
export PATH="$ANDROID_HOME/emulator:$ANDROID_HOME/platform-tools:$PATH"
export LIBGL_ALWAYS_SOFTWARE=1  # software GL fallback if swiftshader not detected

echo "=== Starting emulator: $AVD_NAME ==="

# Kill any existing emulator process
if adb devices 2>/dev/null | grep -q "emulator"; then
  echo "  Existing emulator detected, killing..."
  adb emu kill 2>/dev/null || true
  sleep 2
fi

emulator \
  -avd "$AVD_NAME" \
  -no-window \
  -no-audio \
  -no-snapshot \
  -no-boot-anim \
  -gpu swiftshader_indirect \
  -memory 2048 \
  -cores 2 \
  2>&1 | sed 's/^/  [emu] /' &

EMULATOR_PID=$!
echo "  Emulator PID: $EMULATOR_PID"

echo "=== Waiting for device to appear (adb wait-for-device) ==="
adb wait-for-device

echo "=== Waiting for full boot (up to ${TIMEOUT}s) ==="
ELAPSED=0
until adb shell getprop sys.boot_completed 2>/dev/null | grep -q "^1$"; do
  if ! kill -0 "$EMULATOR_PID" 2>/dev/null; then
    echo "ERROR: Emulator process died unexpectedly."
    exit 1
  fi
  if [ "$ELAPSED" -ge "$TIMEOUT" ]; then
    echo "ERROR: Emulator did not boot within ${TIMEOUT}s."
    kill "$EMULATOR_PID" 2>/dev/null || true
    exit 1
  fi
  sleep 3
  ELAPSED=$((ELAPSED + 3))
  printf "  %ds elapsed...\r" "$ELAPSED"
done

echo ""
echo "=== Post-boot setup ==="
# Unlock screen (keyevent MENU)
adb shell input keyevent 82
# Dismiss any startup dialogs
adb shell settings put global window_animation_scale 0.0
adb shell settings put global transition_animation_scale 0.0
adb shell settings put global animator_duration_scale 0.0

echo ""
echo "Emulator ready after ${ELAPSED}s."
adb devices
