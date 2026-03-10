#!/bin/bash

set -e 

./get-slatedb-c-bindings.sh "$@"
all="${2:-false}"

RUNTIMES_DIR="src/SlateDb/runtimes"

# Cleaning runtimes directory
mkdir -p "$RUNTIMES_DIR"
rm -rf "$RUNTIMES_DIR"

RIDS=(                   osx-arm64                       osx-x64                        linux-arm64                 linux-x64                   win-arm64                    win-x64               )
RUST_TARGETS=(           aarch64-apple-darwin            x86_64-apple-darwin            aarch64-unknown-linux-gnu   x86_64-unknown-linux-gnu    aarch64-pc-windows-msvc      x86_64-pc-windows-msvc  )
LIB_NAMES=(              libslatedb_csharp_ffi.dylib     libslatedb_csharp_ffi.dylib    libslatedb_csharp_ffi.so    libslatedb_csharp_ffi.so    slatedb_csharp_ffi.dll       slatedb_csharp_ffi.dll  )

# Force nightly toolchain via PATH (Homebrew cargo/rustc ignores RUSTUP_TOOLCHAIN)
NIGHTLY_BIN="$(dirname "$(rustup which cargo --toolchain nightly)")"
export PATH="$NIGHTLY_BIN:$PATH"
echo "Using: $(cargo --version), $(rustc --version)"
echo "Running on uname -s=$(uname -s), uname -m=$(uname -m)"

# Detect native platform RID
detect_native_rid() {
    local arch
    case "$(uname -m)" in
        arm64|aarch64) arch="arm64" ;;
        *)             arch="x64" ;;
    esac
    case "$(uname -s)" in
        Darwin)              echo "osx-$arch" ;;
        Linux)               echo "linux-$arch" ;;
        MINGW*|MSYS*|CYGWIN*) echo "win-$arch" ;;
        *)                   echo "linux-$arch" ;;
    esac
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

    # macOS targets require a macOS host (no SDK available on Linux)
    case "$RID" in
        osx-*)
            if [ "$(uname -s)" != "Darwin" ]; then
                echo "  Skipping $RID (requires macOS host)"
                continue
            fi ;;
        linux-*)
            if [ "$(uname -s)" != "Linux" ]; then
                echo "  Skipping $RID (requires Linux host)"
                continue
            fi ;;
        win-*)
            case "$(uname -s)" in
                MINGW*|MSYS*|CYGWIN*)
                    # OK, on est sur Windows
                    ;;
                *)
                    echo "  Skipping $RID (requires Windows host)"
                    continue
                    ;;
            esac
    esac

    echo ""
    echo "=== Building $RID ($TARGET) ==="

    rustup target add --toolchain nightly "$TARGET" 2>/dev/null || true
    mkdir -p "$OUT_DIR"

    # Use zigbuild for cross-compilation, plain cargo for native
    if [ "$RID" = "$NATIVE_RID" ]; then
        BUILD_CMD="cargo build"
    else
        BUILD_CMD="cargo zigbuild"
    fi

    if $BUILD_CMD --release -p slatedb-csharp-ffi --target "$TARGET" \
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