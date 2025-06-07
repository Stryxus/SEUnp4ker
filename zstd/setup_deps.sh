#!/bin/bash
set -e

echo "Installing Homebrew if not present..."
if ! command -v brew &>/dev/null; then
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

echo "Installing required packages..."
brew update
brew install cmake ninja git

# Cross-compilers for Linux
brew tap messense/macos-cross-toolchains
brew install x86_64-unknown-linux-gnu
brew install aarch64-unknown-linux-gnu

echo "All dependencies installed."
