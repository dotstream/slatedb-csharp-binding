use std::{ffi::CString, ptr::null};

use slatedb_csharp_ffi::{CSdbError, db::slatedb_open_with_object_builder, object_store::{slatedb_object_store_builder_config_new, slatedb_object_store_builder_new}, slatedb_close, slatedb_get_with_options, slatedb_write_batch_close, slatedb_write_batch_delete, slatedb_write_batch_new, slatedb_write_batch_put, slatedb_write_batch_write};


fn main() {
    
    let path = CString::new("data/slate.db").unwrap();

    let conf = slatedb_object_store_builder_config_new();
    let object_store = slatedb_object_store_builder_new(slatedb_csharp_ffi::object_store::ObjectStoreType::InMemory, conf);

    let status = slatedb_open_with_object_builder(path.as_ptr(), object_store);

    if status.result.error != CSdbError::Success {
        println!("Failed to open database: {:?}", status.result.message);
        return ;
    }

    let handle = &status.handle;

    let mut batchHandle= std::ptr::null_mut();
    let batch_out = unsafe { slatedb_write_batch_new(&mut batchHandle) };


    let key = b"hello";
    let value = b"world";

    let put_status = unsafe { slatedb_write_batch_put(batchHandle, key.as_ptr(), key.len(), value.as_ptr(), value.len()) };
    println!("slatedb_write_batch_put → {:?}", put_status.error);

    let delete_status = unsafe { slatedb_write_batch_delete(batchHandle, b"test".as_ptr(), b"test".len()) };
    println!("slatedb_write_batch_delete → {:?}", delete_status.error);

    let write_status = unsafe { slatedb_write_batch_write(*handle, batchHandle, null()) };
    
    println!("slatedb_write_batch_write → {:?}", write_status.error);

    let close_batch = unsafe { slatedb_write_batch_close(batchHandle) };
    println!("slatedb_write_batch_close → {:?}", close_batch.error);

    let mut value_out = slatedb_csharp_ffi::CSdbValue { data: null::<u8>() as *mut u8, len: 0 };
    let get_status = unsafe { slatedb_get_with_options(*handle, key.as_ptr(), key.len(), null(), &mut value_out) };

    println!("slatedb_get_with_options → {:?}", get_status.error);
    if get_status.error == CSdbError::Success {
        let value_slice = unsafe { std::slice::from_raw_parts(value_out.data, value_out.len) };
        println!("Got value: {}", String::from_utf8_lossy(value_slice));
    }


     let close_status = slatedb_close(*handle);
    println!("slatedb_close → {:?}", close_status.error);
}
