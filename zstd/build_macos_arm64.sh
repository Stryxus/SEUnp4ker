#!/bin/bash
set -e
cd "$(dirname "$0")"  # Ensure we're in projectroot/zstd

SRC_DIR="../Modules/zstd/build/cmake"
BUILD_DIR="out/macos-arm64/build"
OUT_DIR="out/macos-arm64"

mkdir -p "$BUILD_DIR" "$OUT_DIR"

cmake -S "$SRC_DIR" -B "$BUILD_DIR" -G Ninja \
  -DCMAKE_OSX_ARCHITECTURES=arm64 \
  -DCMAKE_BUILD_TYPE=Release \
  -DCMAKE_LIBRARY_OUTPUT_DIRECTORY="$PWD/$OUT_DIR" \
  -DCMAKE_ARCHIVE_OUTPUT_DIRECTORY="$PWD/$OUT_DIR" \
  -DCMAKE_RUNTIME_OUTPUT_DIRECTORY="$PWD/$OUT_DIR"

cmake --build "$BUILD_DIR"

echo "macOS arm64 build complete. Output in $OUT_DIR"
