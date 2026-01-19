use std::path::PathBuf;
use std::fs;

fn main() {
    let out_dir: PathBuf = PathBuf::from(std::env::var("OUT_DIR").unwrap());
    let generated = out_dir.join("NativeMethods.g.cs");

    csbindgen::Builder::default()
        .input_extern_file("src/lib.rs")
        .csharp_namespace("SlateDB.Interop")
        .csharp_class_name("NativeMethods")
        .csharp_dll_name("slatedb_csharp_ffi")
        .generate_csharp_file(&generated)
        .unwrap();

    let cs_dest = PathBuf::from("../../src/SlateDB/Interop/NativeMethods.g.cs");
    fs::create_dir_all(cs_dest.parent().unwrap()).unwrap(); 
    fs::copy(&generated, &cs_dest).unwrap();

    println!("cargo:rerun-if-changed=src/lib.rs");
}
