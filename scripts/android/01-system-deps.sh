#!/usr/bin/env bash
# Run once as a sudoer. Installs Java 17, emulator libs, and KVM access.
set -euo pipefail

echo "=== [1/3] Installing packages ==="
sudo apt update -qq
sudo apt install -y \
  openjdk-17-jdk \
  curl unzip wget \
  libpulse0 libgl1 libx11-6 libxext6 libglib2.0-0 \
  libdrm2 libgbm1 libxt6 \
  cpu-checker

echo "=== [2/3] Adding $USER to kvm group ==="
sudo usermod -aG kvm "$USER"
sudo chmod 666 /dev/kvm

# Persist kvm permission across reboots
echo 'KERNEL=="kvm", GROUP="kvm", MODE="0666", OPTIONS+="static_node=kvm"' \
  | sudo tee /etc/udev/rules.d/99-kvm4all.rules > /dev/null
sudo udevadm control --reload-rules && sudo udevadm trigger --name-match=kvm 2>/dev/null || true

echo "=== [3/3] Verifying ==="
java -version
kvm-ok 2>/dev/null || true
ls -la /dev/kvm

echo ""
echo "Done. KVM group takes effect on next login (or run: newgrp kvm)"
