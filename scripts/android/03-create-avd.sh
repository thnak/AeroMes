#!/usr/bin/env bash
# Creates an Android 12 (API 31) AVD for CI use. Safe to re-run.
set -euo pipefail

AVD_NAME="${AVD_NAME:-android12_ci}"
PACKAGE="system-images;android-31;google_apis;x86_64"
DEVICE="pixel_4"

export ANDROID_HOME="${ANDROID_HOME:-$HOME/android-sdk}"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/emulator:$ANDROID_HOME/platform-tools:$PATH"

echo "=== Creating AVD: $AVD_NAME ==="

# Delete existing AVD with same name so this script is idempotent
if avdmanager list avd | grep -q "Name: $AVD_NAME"; then
  echo "  Deleting existing AVD: $AVD_NAME"
  avdmanager delete avd --name "$AVD_NAME"
fi

echo "no" | avdmanager create avd \
  --name "$AVD_NAME" \
  --package "$PACKAGE" \
  --device "$DEVICE" \
  --force

# Tune config for headless CI: disable unnecessary features
AVD_CONFIG_DIR="$HOME/.android/avd/${AVD_NAME}.avd"
cat >> "$AVD_CONFIG_DIR/config.ini" << 'CFG'

# CI overrides — headless, no GPU window
hw.gpu.enabled=yes
hw.gpu.mode=swiftshader_indirect
hw.audioInput=no
hw.audioOutput=no
hw.camera.back=none
hw.camera.front=none
hw.sdCard=no
CFG

echo ""
echo "=== AVD list ==="
avdmanager list avd

echo ""
echo "Done. AVD '$AVD_NAME' is ready."
