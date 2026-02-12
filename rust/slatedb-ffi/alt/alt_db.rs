// ============================================================================
// Alternative Open Functions with Builder
// ============================================================================
use crate::object_store::ObjectStoreBuilder;

// Create a new Db using a custom ObjectStoreBuilder
#[no_mangle]
pub extern "C" fn slatedb_open_with_object_builder(
    path: *const c_char,
    object_store_builder: *const ObjectStoreBuilder
) -> CSdbHandleResult {
    
    let path_str = match safe_str_from_ptr(path) {
        Ok(s) => s,
        Err(err) => return create_handle_error_result(err, "Invalid path"),
    };

    let rt = match Builder::new_multi_thread().enable_all().build() {
        Ok(rt) => rt,
        Err(err) => return create_handle_error_result(CSdbError::InternalError, &err.to_string()),
    };

    let builder = unsafe {object_store_builder.as_ref().unwrap()};
    let object_store = match builder.build() {
        Ok(store) => store,
        Err(err) => {
            return CSdbHandleResult {
                handle: CSdbHandle::null(),
                result: CSdbResult {
                    error: err.error,
                    message: err.message,
                },
            }
        }
    };

    match rt.block_on(async {
        Db::builder(path_str, object_store).build().await
    }) {
        Ok(db) => {
            let ffi = Box::new(SlateDbFFI { rt, db });
            create_handle_success_result(CSdbHandle(Box::into_raw(ffi)))
        }
        Err(err) => create_handle_error_result(CSdbError::InternalError, &err.to_string()),
    }
}

/// Create a new DbBuilder with custom ObjectStoreBuilder
#[no_mangle]
pub extern "C" fn slatedb_builder_new_with_object_builder(
    path: *const c_char,
    object_store_builder: *const ObjectStoreBuilder,
) -> *mut slatedb::DbBuilder<String> {
    
    let path_str = match safe_str_from_ptr(path) {
        Ok(s) => s.to_string(),
        Err(_) => return std::ptr::null_mut(),
    };

    let builder = unsafe {object_store_builder.as_ref().unwrap()};
    let object_store = match builder.build() {
        Ok(store) => store,
        Err(_) => return std::ptr::null_mut(),
    };

    let builder = Db::builder(path_str, object_store);
    Box::into_raw(Box::new(builder))
}
