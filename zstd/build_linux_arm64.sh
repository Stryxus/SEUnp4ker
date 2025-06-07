#!/bin/bash
set -e
cd "$(dirname "$0")"

SRC_DIR="../Modules/zstd/build/cmake"
BUILD_DIR="out/linux-arm64/build"
OUT_DIR="out/linux-arm64"

mkdir -p "$BUILD_DIR" "$OUT_DIR"

cmake -S "$SRC_DIR" -B "$BUILD_DIR" -G Ninja \
  -DCMAKE_SYSTEM_NAME=Linux \
  -DCMAKE_SYSTEM_PROCESSOR=aarch64 \
  -DCMAKE_C_COMPILER=aarch64-unknown-linux-gnu-gcc \
  -DCMAKE_CXX_COMPILER=aarch64-unknown-linux-gnu-g++ \
  -DCMAKE_BUILD_TYPE=Release \
  -DCMAKE_LIBRARY_OUTPUT_DIRECTORY="$PWD/$OUT_DIR" \
  -DCMAKE_ARCHIVE_OUTPUT_DIRECTORY="$PWD/$OUT_DIR" \
  -DCMAKE_RUNTIME_OUTPUT_DIRECTORY="$PWD/$OUT_DIR"

cmake --build "$BUILD_DIR"

echo "Linux arm64 build complete. Output in $OUT_DIR"
