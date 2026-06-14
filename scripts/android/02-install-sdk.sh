#!/usr/bin/env bash
# Downloads Android SDK command-line tools and installs all required packages.
# No sudo required. Safe to re-run.
set -euo pipefail

ANDROID_HOME="${ANDROID_HOME:-$HOME/android-sdk}"
CMDLINE_TOOLS_DIR="$ANDROID_HOME/cmdline-tools/latest"

# --- Download cmdline-tools if not present ---------------------------------
if [ ! -f "$CMDLINE_TOOLS_DIR/bin/sdkmanager" ]; then
  echo "=== Downloading Android Command-Line Tools ==="
  mkdir -p "$ANDROID_HOME/cmdline-tools"
  TMP=$(mktemp -d)

  # Fetch the latest version number from Google's repository XML
  REPO_URL="https://dl.google.com/android/repository/repository2-3.xml"
  LATEST_ZIP=$(curl -s "$REPO_URL" \
    | grep -oP 'commandlinetools-linux-\d+_latest\.zip' \
    | head -1)

  if [ -z "$LATEST_ZIP" ]; then
    # Fallback to known stable version
    LATEST_ZIP="commandlinetools-linux-13114758_latest.zip"
    echo "  (could not fetch latest version, using fallback: $LATEST_ZIP)"
  else
    echo "  Found: $LATEST_ZIP"
  fi

  wget -q --show-progress \
    "https://dl.google.com/android/repository/$LATEST_ZIP" \
    -O "$TMP/cmdline-tools.zip"

  unzip -q "$TMP/cmdline-tools.zip" -d "$TMP/extract"
  mv "$TMP/extract/cmdline-tools" "$CMDLINE_TOOLS_DIR"
  rm -rf "$TMP"
  echo "  Installed to: $CMDLINE_TOOLS_DIR"
else
  echo "=== cmdline-tools already present, skipping download ==="
fi

# --- Environment variables --------------------------------------------------
JAVA_HOME_PATH="/usr/lib/jvm/java-17-openjdk-amd64"

add_env_to_file() {
  local FILE="$1"
  if grep -q 'ANDROID_HOME' "$FILE" 2>/dev/null; then
    echo "  $FILE already has Android env vars, skipping"
    return
  fi
  cat >> "$FILE" << ENVBLOCK

# Android SDK
export ANDROID_HOME="$ANDROID_HOME"
export JAVA_HOME="$JAVA_HOME_PATH"
export PATH="\$ANDROID_HOME/cmdline-tools/latest/bin:\$ANDROID_HOME/platform-tools:\$ANDROID_HOME/emulator:\$PATH"
ENVBLOCK
  echo "  Updated $FILE"
}

echo "=== Setting up environment variables ==="
add_env_to_file "$HOME/.bashrc"
add_env_to_file "$HOME/.profile"

# Apply for this session
export ANDROID_HOME="$ANDROID_HOME"
export JAVA_HOME="$JAVA_HOME_PATH"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$ANDROID_HOME/emulator:$PATH"

# --- Install SDK packages ---------------------------------------------------
echo "=== Accepting licenses ==="
yes | sdkmanager --licenses > /dev/null 2>&1 || true

echo "=== Installing SDK packages for Android 12 (API 31) ==="
sdkmanager \
  "platform-tools" \
  "emulator" \
  "platforms;android-31" \
  "build-tools;34.0.0" \
  "system-images;android-31;google_apis;x86_64"

echo ""
echo "=== Installed packages ==="
sdkmanager --list_installed

echo ""
echo "Done. Source your shell or run: source ~/.bashrc"
