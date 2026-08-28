#!/bin/bash

TYPES_TARGET_FILE="rust/slatedb-ffi/src/types.rs"
TYPES_TEMP_FILE="rust/slatedb-ffi/src/types.rs.tmp"
FILTER_POLICY_TARGET_FILE="rust/slatedb-ffi/src/filter_policy.rs"
FILTER_POLICY_TEMP_FILE="rust/slatedb-ffi/src/filter_policy.rs.tmp"
OBJECT_TARGET_FILE="rust/slatedb-ffi/src/object_store.rs"
OBJECT_TEMP_FILE="rust/slatedb-ffi/src/object_store.rs.tmp"
ERROR_TARGET_FILE="rust/slatedb-ffi/src/error.rs"
ERROR_TEMP_FILE="rust/slatedb-ffi/src/error.rs.tmp"

# Detect OS (Linux vs macOS)
OS=$(uname)


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


grep -v "CoreCompactionStatus::Compacted" "$TYPES_TARGET_FILE" > "$TYPES_TEMP_FILE" && mv "$TYPES_TEMP_FILE" "$TYPES_TARGET_FILE"

# ============================================================================
# slatedb 0.13.1 renamed/reshaped FilterContext::Bytes(Bytes) (arbitrary
# length) into FilterContext::Inline([u8; 64]) (fixed 64 bytes). Rework the
# FFI-facing conversion to be fallible (TryFrom) instead of infallible (From),
# and propagate that through every ReadOptions/ScanOptions call site that
# used to rely on an infallible `.into()`.
# ============================================================================

python3 - <<'PYEOF'
import re

def replace_once(path, old, new):
    with open(path) as f:
        content = f.read()
    if old not in content:
        raise SystemExit(f"Error: expected text not found in {path}:\n{old}")
    content = content.replace(old, new, 1)
    with open(path, "w") as f:
        f.write(content)

def replace_all_pattern(path, pattern, repl):
    with open(path) as f:
        content = f.read()
    new_content, count = re.subn(pattern, repl, content, flags=re.DOTALL)
    if count == 0:
        raise SystemExit(f"Error: pattern not found in {path}: {pattern}")
    with open(path, "w") as f:
        f.write(new_content)

filter_policy_rs = "rust/slatedb-ffi/src/filter_policy.rs"
config_rs = "rust/slatedb-ffi/src/config.rs"

replace_once(
    filter_policy_rs,
    '''#[derive(Clone, Debug, uniffi::Enum)]
pub enum FilterContext {
    /// Variable-length payload. Maps to [`slatedb::FilterContext::Bytes`].
    Bytes { payload: Vec<u8> },
}

impl From<FilterContext> for slatedb::FilterContext {
    fn from(value: FilterContext) -> Self {
        match value {
            FilterContext::Bytes { payload } => slatedb::FilterContext::Bytes(payload.into()),
        }
    }
}''',
    '''#[derive(Clone, Debug, uniffi::Enum)]
pub enum FilterContext {
    /// Fixed 64-byte inline payload. Maps to [`slatedb::FilterContext::Inline`];
    /// the payload must be exactly 64 bytes.
    Bytes { payload: Vec<u8> },
}

impl TryFrom<FilterContext> for slatedb::FilterContext {
    type Error = crate::error::Error;

    fn try_from(value: FilterContext) -> Result<Self, Self::Error> {
        match value {
            FilterContext::Bytes { payload } => {
                let len = payload.len();
                let inline: [u8; 64] = payload.try_into().map_err(|_| {
                    crate::error::Error::Invalid {
                        message: format!(
                            "FilterContext payload must be exactly 64 bytes, got {len}"
                        ),
                    }
                })?;
                Ok(slatedb::FilterContext::Inline(inline))
            }
        }
    }
}''',
)

replace_once(
    filter_policy_rs,
    '''    #[test]
    fn filter_context_bytes_round_trips_arbitrary_payload() {
        for len in [0usize, 1, 32, 64, 1024] {
            let payload: Vec<u8> = (0..len).map(|i| i as u8).collect();
            let ctx = FilterContext::Bytes {
                payload: payload.clone(),
            };
            let core: slatedb::FilterContext = ctx.into();
            match core {
                slatedb::FilterContext::Bytes(bytes) => {
                    assert_eq!(bytes.as_ref(), payload.as_slice())
                }
                _ => panic!("expected Bytes variant"),
            }
        }
    }''',
    '''    #[test]
    fn filter_context_bytes_round_trips_exact_64_byte_payload() {
        let payload: Vec<u8> = (0..64).map(|i| i as u8).collect();
        let ctx = FilterContext::Bytes {
            payload: payload.clone(),
        };
        let core: slatedb::FilterContext =
            ctx.try_into().expect("64-byte payload should convert");
        match core {
            slatedb::FilterContext::Inline(bytes) => assert_eq!(bytes.to_vec(), payload),
            _ => panic!("expected Inline variant"),
        }
    }

    #[test]
    fn filter_context_bytes_rejects_non_64_byte_payload() {
        for len in [0usize, 1, 32, 63, 65, 1024] {
            let payload: Vec<u8> = (0..len).map(|i| i as u8).collect();
            let ctx = FilterContext::Bytes { payload };
            let result: Result<slatedb::FilterContext, _> = ctx.try_into();
            assert!(result.is_err(), "expected error for payload length {len}");
        }
    }''',
)

replace_once(
    config_rs,
    '''impl From<ReadOptions> for slatedb::config::ReadOptions {
    fn from(value: ReadOptions) -> Self {
        slatedb::config::ReadOptions {
            durability_filter: value.durability_filter.into(),
            dirty: value.dirty,
            cache_blocks: value.cache_blocks,
            filter_context: value.filter_context.map(Into::into),
        }
    }
}''',
    '''impl TryFrom<ReadOptions> for slatedb::config::ReadOptions {
    type Error = Error;

    fn try_from(value: ReadOptions) -> Result<Self, Self::Error> {
        Ok(slatedb::config::ReadOptions {
            durability_filter: value.durability_filter.into(),
            dirty: value.dirty,
            cache_blocks: value.cache_blocks,
            filter_context: value.filter_context.map(TryInto::try_into).transpose()?,
        })
    }
}''',
)

replace_once(
    config_rs,
    '''            order: value.order.unwrap_or_default().into(),
            filter_context: value.filter_context.map(Into::into),
        })''',
    '''            order: value.order.unwrap_or_default().into(),
            filter_context: value.filter_context.map(TryInto::try_into).transpose()?,
        })''',
)

# Every ReadOptions-consuming method looks like:
#     options: ReadOptions,
#     ) -> Result<...> {
#         ...
#         let options = options.into();
# Switch the `.into()` to a fallible `.try_into()?` without touching the
# other (still-infallible) PutOptions/WriteOptions/MergeOptions conversions
# that share the same `let options = options.into();` spelling.
for src_file in [
    "rust/slatedb-ffi/src/db.rs",
    "rust/slatedb-ffi/src/db_reader.rs",
    "rust/slatedb-ffi/src/db_snapshot.rs",
    "rust/slatedb-ffi/src/db_transaction.rs",
]:
    replace_all_pattern(
        src_file,
        r"options: ReadOptions,(.*?)let options = options\.into\(\);",
        r"options: ReadOptions,\1let options = options.try_into()?;",
    )

print("Reworked FilterContext::Bytes -> FilterContext::Inline and its ReadOptions/ScanOptions call sites.")
PYEOF