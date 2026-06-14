#!/usr/bin/env bash
# CI entry point for Android tests.
# Usage:
#   ANDROID_PROJECT_DIR=/path/to/app ./ci-android.sh unit
#   ANDROID_PROJECT_DIR=/path/to/app ./ci-android.sh instrumented
#   ANDROID_PROJECT_DIR=/path/to/app ./ci-android.sh all
#
# ANDROID_PROJECT_DIR must point to the Android project root (where gradlew lives).
set -euo pipefail

MODE="${1:-unit}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
AVD_NAME="${AVD_NAME:-android12_ci}"

export ANDROID_HOME="${ANDROID_HOME:-$HOME/android-sdk}"
export JAVA_HOME="${JAVA_HOME:-/usr/lib/jvm/java-17-openjdk-amd64}"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$ANDROID_HOME/emulator:$PATH"

# ANDROID_PROJECT_DIR must be set explicitly — no $PWD fallback
if [ -z "${ANDROID_PROJECT_DIR:-}" ]; then
  echo "ERROR: ANDROID_PROJECT_DIR is not set."
  echo ""
  echo "Point it at your Android project root (the directory containing gradlew):"
  echo "  ANDROID_PROJECT_DIR=/path/to/your/android-app $0 $MODE"
  exit 1
fi

GRADLE="$ANDROID_PROJECT_DIR/gradlew"
if [ ! -f "$GRADLE" ]; then
  echo "ERROR: gradlew not found at $GRADLE"
  echo "ANDROID_PROJECT_DIR='$ANDROID_PROJECT_DIR' does not look like an Android project root."
  exit 1
fi

run_unit_tests() {
  echo "=== Unit Tests ==="
  "$GRADLE" -p "$ANDROID_PROJECT_DIR" test --no-daemon 2>&1
}

run_instrumented_tests() {
  echo "=== Instrumented Tests ==="
  # Start emulator; register cleanup trap
  trap '"$SCRIPT_DIR/stop-emulator.sh"' EXIT
  bash "$SCRIPT_DIR/start-emulator.sh" "$AVD_NAME"

  "$GRADLE" -p "$ANDROID_PROJECT_DIR" connectedAndroidTest \
    --no-daemon \
    -Pandroid.testInstrumentationRunnerArguments.notAnnotation=androidx.test.filters.LargeTest \
    2>&1
}

case "$MODE" in
  unit)
    run_unit_tests
    ;;
  instrumented|all)
    run_unit_tests
    run_instrumented_tests
    ;;
  *)
    echo "Unknown mode: $MODE. Use: unit | instrumented | all"
    exit 1
    ;;
esac

echo ""
echo "=== CI done: $MODE ==="
