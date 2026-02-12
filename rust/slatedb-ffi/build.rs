use std::path::PathBuf;
use std::fs;

fn main() {
    let out_dir: PathBuf = PathBuf::from(std::env::var("OUT_DIR").unwrap());
    let generated = out_dir.join("NativeMethods.g.cs");

    csbindgen::Builder::default()
        .input_extern_file("src/lib.rs")
        .input_extern_file("src/batch.rs")
        .input_extern_file("src/config.rs")
        .input_extern_file("src/db_reader.rs")
        .input_extern_file("src/db.rs")
        .input_extern_file("src/error.rs")
        .input_extern_file("src/iterator.rs")
        .input_extern_file("src/memory.rs")
        .input_extern_file("src/object_store.rs")
        .input_extern_file("src/types.rs")
        .csharp_namespace("SlateDb.Interop")
        .csharp_class_name("NativeMethods")
        .csharp_dll_name("slatedb_csharp_ffi")
        .generate_csharp_file(&generated)
        .unwrap();

    let cs_dest = PathBuf::from("../../src/SlateDb/Interop/NativeMethods.g.cs");
    fs::create_dir_all(cs_dest.parent().unwrap()).unwrap(); 
    fs::copy(&generated, &cs_dest).unwrap();

    println!("cargo:rerun-if-changed=src/lib.rs");
}