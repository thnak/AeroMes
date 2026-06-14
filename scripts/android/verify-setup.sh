#!/usr/bin/env bash
# Quick health check — run after setup to confirm everything is in place.
set -euo pipefail

export ANDROID_HOME="${ANDROID_HOME:-$HOME/android-sdk}"
export JAVA_HOME="${JAVA_HOME:-/usr/lib/jvm/java-17-openjdk-amd64}"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$ANDROID_HOME/emulator:$PATH"

PASS=0; FAIL=0

check() {
  local DESC="$1"; shift
  if "$@" &>/dev/null; then
    echo "  [OK] $DESC"
    PASS=$((PASS+1))
  else
    echo "  [FAIL] $DESC"
    FAIL=$((FAIL+1))
  fi
}

echo "=== Android Dev Environment Health Check ==="
echo ""

echo "--- Java ---"
check "java installed"          java -version
check "JAVA_HOME set"           test -d "$JAVA_HOME"
java -version 2>&1 | head -1 | sed 's/^/  /'

echo ""
echo "--- KVM ---"
check "/dev/kvm accessible"     test -r /dev/kvm -a -w /dev/kvm
check "kvm group membership"    groups | grep -q kvm
ls -la /dev/kvm 2>/dev/null | sed 's/^/  /'

echo ""
echo "--- Android SDK ---"
check "ANDROID_HOME exists"     test -d "$ANDROID_HOME"
check "sdkmanager present"      which sdkmanager
check "adb present"             which adb
check "emulator present"        which emulator
check "Android 31 platform"     test -d "$ANDROID_HOME/platforms/android-31"
check "build-tools 34.0.0"      test -d "$ANDROID_HOME/build-tools/34.0.0"
check "system image (API 31)"   test -d "$ANDROID_HOME/system-images/android-31"

echo ""
echo "--- AVD ---"
if avdmanager list avd 2>/dev/null | grep -q "android12_ci"; then
  echo "  [OK] android12_ci AVD exists"
  PASS=$((PASS+1))
  avdmanager list avd 2>/dev/null | grep -A4 "android12_ci" | sed 's/^/  /'
else
  echo "  [FAIL] android12_ci AVD not found — run 03-create-avd.sh"
  FAIL=$((FAIL+1))
fi

echo ""
echo "=== Result: $PASS passed, $FAIL failed ==="
[ "$FAIL" -eq 0 ] && echo "Environment is ready." || echo "Fix the failures above, then re-run."
exit "$FAIL"
