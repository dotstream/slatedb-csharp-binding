#!/bin/bash

set -e 

./get-slatedb-c-bindings.sh "$@"

cargo build

kind="${2:-debug}"

SRC_DIR="./target/$kind"
DEST_DIR="./src/SlateDb/runtimes"
LIB_NAME="slatedb_csharp_ffi"

if [ -z "$SRC_DIR" ] || [ -z "$DEST_DIR" ] || [ -z "$LIB_NAME" ]; then
    echo "Usage: $0 <source_dir> <dest_dir> <lib_name>"
    exit 1
fi

# Detect OS
OS="$(uname -s)"
ARCH=$(uname -m)
RID=""

case "$OS" in
    Linux*)
        RID="linux-x64"
        EXT="so"
        LIB_NAME="libslatedb_csharp_ffi.so"
        ;;
    Darwin*)
        if [[ "$ARCH" == "arm64" ]]; then
            echo "Architecture is osx-arm64 (Apple Silicon)"
            RID="osx-arm64"
        elif [[ "$ARCH" == "x86_64" ]]; then
            echo "Architecture is osx-x64 (Intel)"
            RID="osx-x64"
        else
            echo "Unknown architecture: $ARCH"
            exit 1
        fi
        LIB_NAME="libslatedb_csharp_ffi.dylib"
        ;;
    CYGWIN*|MINGW*|MSYS*)
        RID="win-x64"
        EXT="dll"
        LIB_NAME="slatedb_csharp_ffi.dll"
        ;;
    *)
        echo "Unsupported OS: $OS"
        exit 1
        ;;
esac

ls -l "$SRC_DIR"
SRC_FILE="$SRC_DIR/${LIB_NAME}"
DEST_FILE="$DEST_DIR/$RID/native/${LIB_NAME}"

if [ ! -f "$SRC_FILE" ]; then
    echo "Library not found: $SRC_FILE"
    exit 1
fi

mkdir -p "$DEST_DIR/$RID/native/"
cp "$SRC_FILE" "$DEST_FILE"

echo "Copied $SRC_FILE → $DEST_FILE"

cd ./src/SlateDb

dotnet build