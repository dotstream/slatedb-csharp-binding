use std::fs;
use std::path::{Path, PathBuf};

fn list_files_with_pattern(directory_path: &Path, pattern: &str) -> Result<Vec<String>, std::io::Error> {
    let mut matching_files = Vec::new();

    for entry in fs::read_dir(directory_path)? {
        let entry = entry?;
        let path = entry.path();

        if let Some(filename) = path.file_name().and_then(|s| s.to_str()) {
            // Filter for files ending with the specified pattern
            if filename.ends_with(pattern) {
                matching_files.push(filename.to_string());
            }
        }
    }
    Ok(matching_files)
}

fn main() {
    let out_dir: PathBuf = PathBuf::from(std::env::var("OUT_DIR").unwrap());
    let generated = out_dir.join("NativeMethods.g.cs");

    let mut builder = csbindgen::Builder::default();
    let path_rs = Path::new("./src");

    match list_files_with_pattern(path_rs, ".rs") {
        Ok(files) => {
            let mut sfiles = files.clone();
            sfiles.sort();
            for file in sfiles {
                let full_path = path_rs.join(&file);
                let content = fs::read_to_string(&full_path).unwrap();

                println!("Adding file to csbindgen: {}", file);
                println!("Content preview:\n{}", &content[..content.len()]);

                if content.contains("extern \"C\"") {
                    builder = builder.input_extern_file(&full_path);
                }

                println!("File processed: {}", &full_path.display());
            }
        }
        Err(e) => eprintln!("Error listing files: {}", e),
    }

    match builder
        .csharp_namespace("SlateDb.Interop")
        .csharp_class_name("NativeMethods")
        .csharp_dll_name("slatedb_csharp_ffi")
        .generate_csharp_file(&generated){
            Ok(_) => {}
            Err(e) => {
                eprintln!("Error while generating C# bindings:");
                eprintln!("  {:?}", e);
                panic!("csbindgen failed");
            }
        }

    let cs_dest = PathBuf::from("../../src/SlateDb/Interop/NativeMethods.g.cs");
    fs::create_dir_all(cs_dest.parent().unwrap()).unwrap();
    fs::copy(&generated, &cs_dest).unwrap();
}
