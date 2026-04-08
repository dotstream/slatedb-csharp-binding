#!/bin/bash

set -e 

./get-slatedb-c-bindings.sh

all="${1:-false}"

RUNTIMES_DIR="src/SlateDb/runtimes"

# Cleaning runtimes directory
mkdir -p "$RUNTIMES_DIR"
rm -rf "$RUNTIMES_DIR"

RIDS=(
    osx-arm64  osx-x64
    linux-arm64 linux-x64
    win-arm64 win-x64
)

RUST_TARGETS=(
    aarch64-apple-darwin x86_64-apple-darwin
    aarch64-unknown-linux-gnu x86_64-unknown-linux-gnu
    aarch64-pc-windows-msvc x86_64-pc-windows-msvc
)

LIB_NAMES=(
    libslatedb_csharp_ffi.dylib libslatedb_csharp_ffi.dylib
    libslatedb_csharp_ffi.so libslatedb_csharp_ffi.so
    slatedb_csharp_ffi.dll slatedb_csharp_ffi.dll
)

# Force nightly toolchain via PATH (Homebrew cargo/rustc ignores RUSTUP_TOOLCHAIN)
NIGHTLY_BIN="$(dirname "$(rustup which cargo --toolchain nightly)")"
export PATH="$NIGHTLY_BIN:$PATH"
echo "Using: $(cargo --version), $(rustc --version)"
echo "Running on uname -s=$(uname -s), uname -m=$(uname -m)"

# -----------------------------
#  PLATFORM DETECTION (ROBUST)
# -----------------------------
OS="$(uname -s)"

is_windows=false
is_macos=false
is_linux=false

case "$OS" in
    MINGW*|MSYS*|CYGWIN*) is_windows=true ;;
    Darwin)               is_macos=true ;;
    Linux)                is_linux=true ;;
esac

# Detect native RID
detect_native_rid() {
    local arch
    case "$(uname -m)" in
        arm64|aarch64) arch="arm64" ;;
        *)             arch="x64" ;;
    esac

    if $is_macos; then
        echo "osx-$arch"
    elif $is_linux; then
        echo "linux-$arch"
    elif $is_windows; then
        echo "win-$arch"
    else
        echo "linux-$arch"
    fi
}

NATIVE_RID="$(detect_native_rid)"

# Check zigbuild for cross builds
HAS_ZIGBUILD=false
if command -v cargo-zigbuild &>/dev/null || [ -f "$HOME/.cargo/bin/cargo-zigbuild" ]; then
    HAS_ZIGBUILD=true
fi

if [ "$all" = true ] && [ "$HAS_ZIGBUILD" = false ]; then
    echo "Error: all requires cargo-zigbuild. Install: brew install zig && cargo install cargo-zigbuild" >&2
    exit 1
fi

SUCCEEDED=""
FAILED=""

for i in "${!RIDS[@]}"; do
    RID="${RIDS[$i]}"
    TARGET="${RUST_TARGETS[$i]}"
    LIB_NAME="${LIB_NAMES[$i]}"
    OUT_DIR="$RUNTIMES_DIR/$RID/native"

    # Skip non-native platforms unless all
    if [ "$all" = false ] && [ "$RID" != "$NATIVE_RID" ]; then
        continue
    fi

    # macOS targets require macOS host
    if [[ "$RID" == osx-* ]] && ! $is_macos; then
        echo "  Skipping $RID (requires macOS host)"
        continue
    fi

    # Linux + Windows targets require Linux or Windows host
    if [[ "$RID" == linux-* ]] && ! $is_linux; then
        echo "  Skipping $RID (requires Linux host)"
        continue
    fi

    if [[ "$RID" == win-* ]] && ! $is_windows; then
        echo "  Skipping $RID (requires Windows host)"
        continue
    fi

    echo ""
    echo "=== Building $RID ($TARGET) ==="

    rustup target add --toolchain nightly "$TARGET" 2>/dev/null || true
    mkdir -p "$OUT_DIR"

    # Use zigbuild for cross-compilation, plain cargo for native
    # Cross-compilation only on Linux
    if $is_linux && [ "$RID" != "$NATIVE_RID" ]; then
        BUILD_CMD="cargo zigbuild"
    else
        BUILD_CMD="cargo build"
    fi

    if $BUILD_CMD --release -p slatedb-csharp-ffi --verbose --target "$TARGET" \
        --manifest-path "Cargo.toml" 2>&1; then

        SRC="target/$TARGET/release/$LIB_NAME"
        if [ -f "$SRC" ]; then
            cp "$SRC" "$OUT_DIR/$LIB_NAME"

            # Strip debug symbols to reduce size
            case "$LIB_NAME" in
                *.dylib) strip -x "$OUT_DIR/$LIB_NAME" 2>/dev/null || true ;;
                *.so)    strip --strip-debug "$OUT_DIR/$LIB_NAME" 2>/dev/null || true ;;
                *.dll)   strip --strip-debug "$OUT_DIR/$LIB_NAME" 2>/dev/null || true ;;
            esac

            SIZE=$(du -h "$OUT_DIR/$LIB_NAME" | cut -f1)
            echo "  -> $OUT_DIR/$LIB_NAME ($SIZE)"
            SUCCEEDED="$SUCCEEDED $RID"
        else
            echo "  Error: built successfully but $SRC not found" >&2
            FAILED="$FAILED $RID"
        fi
    else
        echo "  Build failed for $RID" >&2
        FAILED="$FAILED $RID"
    fi
done

echo ""
echo "=== Summary ==="
echo "Succeeded:${SUCCEEDED:- none}"
echo "Failed:   ${FAILED:- none}"
echo ""
echo "Native libraries are in: $RUNTIMES_DIR/"

cd ./src/SlateDb

dotnet build