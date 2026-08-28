#!/bin/bash

CONFIG_TARGET_FILE="rust/slatedb-ffi/src/config.rs"
CONFIG_TEMP_FILE="rust/slatedb-ffi/src/config.rs.tmp"
OBJECT_TARGET_FILE="rust/slatedb-ffi/src/object_store.rs"
OBJECT_TEMP_FILE="rust/slatedb-ffi/src/object_store.rs.tmp"
ERROR_TARGET_FILE="rust/slatedb-ffi/src/error.rs"
ERROR_TEMP_FILE="rust/slatedb-ffi/src/error.rs.tmp"
LIBRARY_FILE="rust/slatedb-ffi/src/lib.rs"
LIBRARY_TEMP_FILE="rust/slatedb-ffi/src/lib.rs.tmp"

# Safety check
if [ ! -f "$CONFIG_TARGET_FILE" ]; then
    echo "Error: $CONFIG_TARGET_FILE not found."
    exit 1
fi


awk '
  # Detect the start of the specific function
  /pub struct ScanOptions/ { in_func = 1 }
  
  # If we are inside the function, check for the lines to remove
  in_func {
    if ($0 ~ /#\[uniffi\(default = None\)\]/) {
      next # Skip these lines
    }
  }

  # Detect the end of the function to stop the specific filtering
  in_func && /^\}/ { in_func = 0 }

  # Print all other lines
  { print $0 }
' "$CONFIG_TARGET_FILE" > "$CONFIG_TEMP_FILE" && mv "$CONFIG_TEMP_FILE" "$CONFIG_TARGET_FILE"

awk '
  # Detect the start of the specific function
  /impl TryFrom<ScanOptions> for slatedb::config::ScanOptions/ { in_func = 1 }
  
  # If we are inside the function, check for the lines to remove
  in_func {
    if ($0 ~ /order: value\.order\.unwrap_or_default\(\)\.into\(\),/) {
      print "            order: value.order.into(),"
      next
    }
  }

  # Detect the end of the function to stop the specific filtering
  in_func && /^\}/ { in_func = 0 }

  # Print all other lines
  { print $0 }
' "$CONFIG_TARGET_FILE" > "$CONFIG_TEMP_FILE" && mv "$CONFIG_TEMP_FILE" "$CONFIG_TARGET_FILE"

echo "Uniffi Default for order is changed in $LIBRARY_FILE."

PATTERN="pub order: Option<IterationOrder>,"
REPLACE_PATTERN="pub order: IterationOrder,"

# Detect OS (Linux vs macOS)
OS=$(uname)

if [ "$OS" = "Darwin" ]; then
    # macOS (BSD sed) requires an argument for -i (even if empty)
    sed -i '' "s/$PATTERN/$REPLACE_PATTERN/g" "$CONFIG_TARGET_FILE"
    sed -i '' "s/order: None/order: IterationOrder::default()/g" "$CONFIG_TARGET_FILE"
else
    # Linux (GNU sed)
    sed -i "s/$PATTERN/$REPLACE_PATTERN/g" "$CONFIG_TARGET_FILE"
    sed -i "s/order: None/order: IterationOrder::default()/g" "$CONFIG_TARGET_FILE"
fi

echo "Removal completed in $CONFIG_TARGET_FILE"

# ============================================================================
# Merge alt/uniffi_*.rs custom UniFFI extensions into src/*.rs
# ============================================================================

SRC_DIR="rust/slatedb-ffi/src"
ALT_DIR="rust/slatedb-ffi/alt"

for uniffi_file in "$ALT_DIR"/uniffi_*.rs; do
    [ -e "$uniffi_file" ] || continue

    base=$(basename "$uniffi_file")
    name=${base#uniffi_}   # remove "uniffi_"

    if [ "$name" = "error.rs" ]; then
        continue # handled below
    fi

    src_file="$SRC_DIR/$name"

    if [[ -f "$src_file" ]]; then
        echo "Merging $uniffi_file + $src_file → $SRC_DIR/$name"
        cat "$src_file" "$uniffi_file" > "$src_file.tmp" && mv "$src_file.tmp" "$src_file"
    else
        echo "⚠️ No file matching for $uniffi_file"
    fi
done

# ============================================================================
# Merge alt2/uniffi_error.rs into the SlateDbError enum in src/error.rs
# ============================================================================
#
# error.rs isn't a simple append target: the new variants must land inside
# the existing `pub(crate) enum SlateDbError { ... }` body, before its
# closing brace, not appended after it.

ERROR_TARGET_FILE="$SRC_DIR/error.rs"
ERROR_FRAGMENT_FILE="$ALT_DIR/uniffi_error.rs"

if [[ -f "$ERROR_FRAGMENT_FILE" ]]; then
    if [[ ! -f "$ERROR_TARGET_FILE" ]]; then
        echo "Error: $ERROR_TARGET_FILE not found." >&2
        exit 1
    fi

    enum_start=$(grep -n "^pub(crate) enum SlateDbError {$" "$ERROR_TARGET_FILE" | head -1 | cut -d: -f1)

    if [[ -z "$enum_start" ]]; then
        echo "Error: 'pub(crate) enum SlateDbError' not found in $ERROR_TARGET_FILE" >&2
        exit 1
    fi

    close_offset=$(tail -n "+$enum_start" "$ERROR_TARGET_FILE" | grep -n "^}$" | head -1 | cut -d: -f1)
    enum_close=$((enum_start + close_offset - 1))

    {
        head -n $((enum_close - 1)) "$ERROR_TARGET_FILE"
        cat "$ERROR_FRAGMENT_FILE"
        tail -n "+$enum_close" "$ERROR_TARGET_FILE"
    } > "$ERROR_TARGET_FILE.tmp" && mv "$ERROR_TARGET_FILE.tmp" "$ERROR_TARGET_FILE"

    echo "Merged $ERROR_FRAGMENT_FILE into the SlateDbError enum in $ERROR_TARGET_FILE."
else
    echo "⚠️ No file matching for $ERROR_FRAGMENT_FILE"
fi

# Safety check
if [ ! -f "$OBJECT_TARGET_FILE" ]; then
    echo "Error: $OBJECT_TARGET_FILE not found."
    exit 1
fi

echo "use crate::error::SlateDbError;" | cat - "$OBJECT_TARGET_FILE" > "$OBJECT_TEMP_FILE" && mv "$OBJECT_TEMP_FILE" "$OBJECT_TARGET_FILE"

# Safety check
if [ ! -f "$ERROR_TARGET_FILE" ]; then
    echo "Error: $ERROR_TARGET_FILE not found."
    exit 1
fi

echo "use std::error::Error as StdError;" | cat - "$ERROR_TARGET_FILE" > "$ERROR_TEMP_FILE" && mv "$ERROR_TEMP_FILE" "$ERROR_TARGET_FILE"