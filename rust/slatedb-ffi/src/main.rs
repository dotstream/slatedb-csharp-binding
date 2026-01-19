use std::ffi::CString;
use std::os::raw::{c_int};
use std::slice;

use slatedb_csharp_ffi::{
    slatedb_open,
    slatedb_put,
    slatedb_get,
    slatedb_free_value,
    slatedb_close,
    SlateDbOptions,
    SlateDbStatus,
};

fn main() {
    let path = CString::new("data/slate.db").unwrap();
    let opts = SlateDbOptions::default();
    let mut handle: u64 = 0;

    let status = unsafe {
        slatedb_open(path.as_ptr(), &opts, &mut handle)
    };

    println!("slatedb_open → {:?}", status);

    if status != SlateDbStatus::Ok {
        println!("Fail to open database");
        return;
    }

    // --- PUT ---
    let key = b"hello";
    let value = b"world";

    let put_status = unsafe {
        slatedb_put(
            handle,
            key.as_ptr(),
            key.len() as c_int,
            value.as_ptr(),
            value.len() as c_int,
        )
    };

    println!("slatedb_put → {:?}", put_status);

    if put_status != SlateDbStatus::Ok {
        println!("Fail to put value");
        return;
    }

    // --- GET ---
    let mut out_ptr: *const u8 = std::ptr::null();
    let mut out_len: c_int = 0;

    let get_status = unsafe {
        slatedb_get(
            handle,
            key.as_ptr(),
            key.len() as c_int,
            &mut out_ptr,
            &mut out_len,
        )
    };

    println!("slatedb_get → {:?}", get_status);

    if get_status == SlateDbStatus::Ok {
        unsafe {
            let slice = slice::from_raw_parts(out_ptr, out_len as usize);
            let result = String::from_utf8_lossy(slice);
            println!("Value read = {}", result);

            // Libération mémoire retournée par la FFI
            slatedb_free_value(out_ptr as *mut u8, out_len);
        }
    } else {
        println!("Fail to get value");
    }

    // --- CLOSE ---
    let close_status = slatedb_close(handle);
    println!("slatedb_close → {:?}", close_status);
}
