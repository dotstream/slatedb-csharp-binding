// ============================================================================
// Alternative Open Functions with Builder
// ============================================================================
use crate::object_store::ObjectStoreBuilder;

#[no_mangle]
pub unsafe extern "C" fn slatedb_reader_open_with_object_builder(
    path: *const std::os::raw::c_char,
    object_store_builder: *const ObjectStoreBuilder,
    checkpoint_id: *const std::os::raw::c_char,
    reader_options: *const slatedb_db_reader_options_t,
    out_reader: *mut *mut slatedb_db_reader_t,
) -> slatedb_result_t {
    
    if let Err(err) = require_out_ptr(out_reader, "out_reader") {
        return err;
    }

    *out_reader = std::ptr::null_mut();

    let path = match cstr_to_string(path, "path") {
        Ok(path) => path,
        Err(err) => return err,
    };

    // Parse checkpoint ID if provided
    let checkpoint_id = if checkpoint_id.is_null() {
        None
    } else {
        let checkpoint_id = match cstr_to_string(checkpoint_id, "checkpoint_id") {
            Ok(checkpoint_id) => checkpoint_id,
            Err(err) => return err,
        };
        if checkpoint_id.is_empty() {
            None
        } else {
            match Uuid::parse_str(&checkpoint_id) {
                Ok(checkpoint_id) => Some(checkpoint_id),
                Err(err) => {
                    return error_result(
                        slatedb_error_kind_t::SLATEDB_ERROR_KIND_INVALID,
                        &format!("invalid checkpoint_id UUID: {err}"),
                    );
                }
            }
        }
    };

    let reader_options = db_reader_options_from_ptr(reader_options);

    let runtime = match create_runtime() {
        Ok(runtime) => runtime,
        Err(err) => return err,
    };

    let builder = unsafe {object_store_builder.as_ref().unwrap()};
    let object_store = match builder.build() {
        Ok(store) => store,
        Err(err) => {
            return error_result(
                slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL,
                &format!("Failed to build object store: {:?}", err.message),
            );
        }
    };

    match runtime.block_on(DbReader::open(
        path,
        object_store,
        checkpoint_id,
        reader_options,
    )) {
        Ok(reader) => {
            let handle = Box::new(slatedb_db_reader_t { runtime, reader });
            *out_reader = Box::into_raw(handle);
            success_result()
        }
        Err(err) => error_from_slate_error(&err),
    }
}
