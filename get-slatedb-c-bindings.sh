#!/bin/bash

version="0.12.1"

rm -rf rust/slatedb-ffi/src/*.rs
mkdir -p slatedb-bindings
rm -rf slatedb-bindings/*
curl -sL https://github.com/slatedb/slatedb/archive/refs/tags/v${version}.tar.gz | tar xz -C slatedb-bindings --strip-components=4 slatedb-${version}/bindings/uniffi/src

cp -R slatedb-bindings/* rust/slatedb-ffi/src
rm -rf slatedb-bindings

ALT_DIR="rust/slatedb-ffi/alt"
SRC_DIR="rust/slatedb-ffi/src"


for alt_file in "$ALT_DIR"/alt_*.rs; do
    base=$(basename "$alt_file")
    name=${base#alt_}   # remove "alt_"
    src_file="$SRC_DIR/$name"

    if [[ -f "$src_file" ]]; then
        echo "Merging $alt_file + $src_file → $SRC_DIR/$name"
        cat "$src_file" "$alt_file" > "$src_file.tmp" && mv "$src_file.tmp" "$src_file"
    else
        echo "⚠️ No file matching for $alt_file"
    fi
done

# Call action post mortem
src_file="$ALT_DIR/alt_post_${version}.sh"

if [[ -f "$src_file" ]]; then
    echo "Executing post-mortem action for version ${version}"
    bash "$src_file"
else
    echo "⚠️ No post-mortem action found for version ${version}"
fi