#!/bin/bash
set -e

echo "Detecting platform and installing dependencies..."

if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    echo "Detected macOS."
    if ! command -v brew &>/dev/null; then
        echo "Homebrew is not installed. Please install it from https://brew.sh/"
        # Try to open the Homebrew homepage in the default browser
        if command -v open &>/dev/null; then
            open "https://brew.sh/"
        else
            echo "Please visit https://brew.sh/ to install Homebrew."
        fi
        exit 1
    fi

    echo "Installing required packages with Homebrew..."
    brew update
    brew install cmake ninja git

    # Cross-compilers for Linux (macOS only)
    brew tap messense/macos-cross-toolchains
    brew install x86_64-unknown-linux-gnu
    brew install aarch64-unknown-linux-gnu

elif [[ -f "/etc/os-release" ]]; then
    . /etc/os-release
    if [[ "$ID" == "ubuntu" || "$ID_LIKE" == *"debian"* ]]; then
        # Ubuntu/Debian
        echo "Detected Ubuntu/Debian."
        sudo apt-get update
        sudo apt-get install -y cmake ninja-build git build-essential
    elif [[ "$ID" == "fedora" || "$ID_LIKE" == *"rhel"* || "$ID_LIKE" == *"centos"* ]]; then
        # Fedora/RHEL/CentOS
        echo "Detected Fedora/RHEL/CentOS."
        sudo dnf install -y cmake ninja-build git make gcc gcc-c++
    else
        echo "Unsupported Linux distribution: $ID"
        exit 1
    fi
else
    echo "Unsupported platform: $OSTYPE"
    exit 1
fi

echo "All dependencies installed."
