#!/bin/bash
set -e

./build_macos_arm64.sh
./build_linux_x64.sh
./build_linux_arm64.sh

echo "All builds complete."
