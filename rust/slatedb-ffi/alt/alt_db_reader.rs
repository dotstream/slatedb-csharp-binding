// ============================================================================
// Alternative Open Functions with Builder
// ============================================================================
use crate::object_store::ObjectStoreBuilder;

#[no_mangle]
pub extern "C" fn slatedb_reader_open_with_object_builder(
    path: *const c_char,
    object_store_builder: *const ObjectStoreBuilder,
    checkpoint_id: *const c_char, // Nullable - use null for latest
    reader_options: *const CSdbReaderOptions,
) -> CSdbReaderHandleResult {
    
    let path_str = match safe_str_from_ptr(path) {
        Ok(s) => s,
        Err(err) => return create_reader_handle_error_result(err, "Invalid path"),
    };

    // Parse checkpoint ID if provided
    let checkpoint_uuid = if checkpoint_id.is_null() {
        None
    } else {
        match safe_str_from_ptr(checkpoint_id) {
            Ok(id_str) => match Uuid::parse_str(id_str) {
                Ok(uuid) => Some(uuid),
                Err(err) => {
                    return create_reader_handle_error_result(
                        CSdbError::InvalidArgument,
                        &format!("Invalid checkpoint_id format '{id_str}': {err}"),
                    )
                }
            },
            Err(err) => return create_reader_handle_error_result(err, "Invalid checkpoint_id"),
        }
    };

    // Parse reader options
    let opts = convert_reader_options(reader_options);

    // Create a dedicated runtime for this DbReader instance
    let rt = match Builder::new_multi_thread().enable_all().build() {
        Ok(rt) => rt,
        Err(err) => {
            return create_reader_handle_error_result(CSdbError::InternalError, &err.to_string())
        }
    };

    let builder = unsafe {object_store_builder.as_ref().unwrap()};
    let object_store = match builder.build() {
        Ok(store) => store,
        Err(err) => {
            return CSdbReaderHandleResult {
                handle: CSdbReaderHandle::null(),
                result: CSdbResult {
                    error: err.error,
                    message: err.message,
                },
            }
        }
    };

    match rt.block_on(async { DbReader::open(path_str, object_store, checkpoint_uuid, opts).await })
    {
        Ok(reader) => {
            let ffi = Box::new(SlateDbReaderFFI { rt, reader });
            create_reader_handle_success_result(CSdbReaderHandle(Box::into_raw(ffi)))
        }
        Err(err) => create_reader_handle_error_result(CSdbError::InternalError, &err.to_string()),
    }
}
