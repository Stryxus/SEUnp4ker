#!/bin/bash
set -e
cd "$(dirname "$0")"

SRC_DIR="../Modules/zstd/build/cmake"
BUILD_DIR="out/linux-x64/build"
OUT_DIR="out/linux-x64"

mkdir -p "$BUILD_DIR" "$OUT_DIR"

cmake -S "$SRC_DIR" -B "$BUILD_DIR" -G Ninja \
  -DCMAKE_SYSTEM_NAME=Linux \
  -DCMAKE_SYSTEM_PROCESSOR=x86_64 \
  -DBUILD_SHARED_LIBS=ON \
  -DCMAKE_BUILD_TYPE=Release \
  -DCMAKE_LIBRARY_OUTPUT_DIRECTORY="$PWD/$OUT_DIR" \
  -DCMAKE_ARCHIVE_OUTPUT_DIRECTORY="$PWD/$OUT_DIR" \
  -DCMAKE_RUNTIME_OUTPUT_DIRECTORY="$PWD/$OUT_DIR"

cmake --build "$BUILD_DIR"

echo "Linux x64 build complete. Output in $OUT_DIR"
