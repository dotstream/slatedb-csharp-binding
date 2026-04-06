// ============================================================================
// Alternative Open Functions with Wal Reader
// ============================================================================
use crate::object_store::ObjectStoreBuilder;
use crate::slatedb_error_kind_t;
use crate::ffi::error_result;

// Create a new Wal Reader using a custom ObjectStoreBuilder
#[no_mangle]
pub unsafe extern "C" fn slatedb_wal_reader_with_object_builder_new(
    path: *const c_char,
    object_store_builder: *const ObjectStoreBuilder,
    out_reader: *mut *mut slatedb_wal_reader_t,
) -> slatedb_result_t {
    if let Err(err) = require_out_ptr(out_reader, "out_reader") {
        return err;
    }
    if let Err(err) = require_handle(object_store_builder, "object_store_builder") {
        return err;
    }
    *out_reader = std::ptr::null_mut();

    let path = match cstr_to_string(path, "path") {
        Ok(p) => p,
        Err(err) => return err,
    };

    let runtime = match create_runtime() {
        Ok(rt) => rt,
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

    let reader = WalReader::new(path, object_store);
    *out_reader = Box::into_raw(Box::new(slatedb_wal_reader_t { runtime, reader }));
    success_result()
}