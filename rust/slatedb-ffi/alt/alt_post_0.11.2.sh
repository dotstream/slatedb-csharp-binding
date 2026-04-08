#!/bin/bash

DB_TARGET_FILE="rust/slatedb-ffi/src/db.rs"
DB_TEMP_FILE="rust/slatedb-ffi/src/db.rs.tmp"
ITERATOR_TARGET_FILE="rust/slatedb-ffi/src/iterator.rs"
ITERATOR_TEMP_FILE="rust/slatedb-ffi/src/iterator.rs.tmp"
FFI_TARGET_FILE="rust/slatedb-ffi/src/ffi.rs"
FFI_TEMP_FILE="rust/slatedb-ffi/src/ffi.rs.tmp"
LIB_FILE="rust/slatedb-ffi/src/lib.rs"

# Safety check
if [ ! -f "$DB_TARGET_FILE" ]; then
    echo "Error: $DB_TARGET_FILE not found."
    exit 1
fi

# Use awk for a more "surgical" removal
awk '
  /#\[no_mangle\]/ { 
    # Store the #[no_mangle] line in a buffer
    holding = $0; next 
  }
  /pub unsafe extern "C" fn slatedb_db_get_key_value_with_options/ {
    # If we find the function, start a skip loop
    found = 1;
    next;
  }
  found {
    # While in skip loop, if we hit a closing brace at start of line, stop skipping
    if ($0 ~ /^\}/) { found = 0; holding = "" }
    next;
  }
  {
    # If we had a #[no_mangle] that WASNT followed by our function, print it now
    if (holding != "") { print holding; holding = "" }
    print $0;
  }
' "$DB_TARGET_FILE" > "$DB_TEMP_FILE" && mv "$DB_TEMP_FILE" "$DB_TARGET_FILE"

echo "Function slatedb_db_get_key_value_with_options removed from $DB_TARGET_FILE."

if [[ ! -f "$ITERATOR_TARGET_FILE" ]]; then
    echo "Error: $ITERATOR_TARGET_FILE not found."
    exit 1
fi

awk '
  # Detect the start of the specific function
  /pub unsafe extern "C" fn slatedb_iterator_next/ { in_func = 1 }
  
  # If we are inside the function, check for the lines to remove
  in_func {
    if ($0 ~ /seq: kv\.seq,/ || 
        $0 ~ /create_ts: kv\.create_ts,/ || 
        $0 ~ /expire_ts_present: kv\.expire_ts\.is_some\(\),/ || 
        $0 ~ /expire_ts: kv\.expire_ts\.unwrap_or\(0\),/) {
      next # Skip these lines
    }
  }

  # Detect the end of the function to stop the specific filtering
  in_func && /^\}/ { in_func = 0 }

  # Print all other lines
  { print $0 }
' "$ITERATOR_TARGET_FILE" > "$ITERATOR_TEMP_FILE" && mv "$ITERATOR_TEMP_FILE" "$ITERATOR_TARGET_FILE"

echo "Metadata lines removed from slatedb_iterator_next in $ITERATOR_TARGET_FILE."

if [[ ! -f "$FFI_TARGET_FILE" ]]; then
    echo "Error: $FFI_TARGET_FILE not found."
    exit 1
fi

awk '
  # Detect the start of the specific function
  /pub struct slatedb_key_value_t/ { in_func = 1 }
  
  # If we are inside the function, check for the lines to remove
  in_func {
    if ($0 ~ /\/\/\/ Sequence number assigned to this entry\./ || 
        $0 ~ /pub seq: u64,/ || 
        $0 ~ /\/\/\/ Creation timestamp in milliseconds since epoch\, or 0 if not set\./ || 
        $0 ~ /pub create_ts: i64,/ || 
        $0 ~ /\/\/\/ Whether \`expire_ts\` is populated\./ || 
        $0 ~ /pub expire_ts_present: bool,/ || 
        $0 ~ /\/\/\/ Expiration timestamp in milliseconds since epoch \(valid when \`expire_ts_present\` is true\)\./ || 
        $0 ~ /pub expire_ts: i64,/) {
      next # Skip these lines
    }
  }

  # Detect the end of the function to stop the specific filtering
  in_func && /^\}/ { in_func = 0 }

  # Print all other lines
  { print $0 }
' "$FFI_TARGET_FILE" > "$FFI_TEMP_FILE" && mv "$FFI_TEMP_FILE" "$FFI_TARGET_FILE"

echo "Metadata lines removed from slatedb_key_value_t in $FFI_TARGET_FILE."

PATTERN="slatedb_db_get_key_value_with_options,"

# Detect OS (Linux vs macOS)
OS=$(uname)

if [ "$OS" = "Darwin" ]; then
    # macOS (BSD sed) requires an argument for -i (even if empty)
    sed -i '' "/$PATTERN/d" "$LIB_FILE"
else
    # Linux (GNU sed)
    sed -i "/$PATTERN/d" "$LIB_FILE"
fi

echo "Removal completed in $LIB_FILE"