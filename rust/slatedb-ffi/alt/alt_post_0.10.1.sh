#!/bin/bash

# Remove use slatedb::object_store::ObjectStore; in object_store.rs to avoid conflict with the one in alt_object_store.rs
cat rust/slatedb-ffi/src/object_store.rs | grep -v "use slatedb::object_store::ObjectStore;" > rust/slatedb-ffi/src/object_store.rs.tmp && mv rust/slatedb-ffi/src/object_store.rs.tmp rust/slatedb-ffi/src/object_store.rs

# Remove block_transformer in config.rs 
cat rust/slatedb-ffi/src/config.rs | grep -v "block_transformer: defaults.block_transformer," > rust/slatedb-ffi/src/config.rs.tmp && mv rust/slatedb-ffi/src/config.rs.tmp rust/slatedb-ffi/src/config.rs