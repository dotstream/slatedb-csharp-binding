// ============================================================================
// Alternative Open Functions with Builder
// ============================================================================
use crate::object_store::ObjectStoreBuilder;
use crate::slatedb_db_builder_t;

// Create a new Db using a custom ObjectStoreBuilder
#[no_mangle]
pub unsafe extern "C" fn slatedb_open_with_object_builder(
    path: *const std::os::raw::c_char,
    object_store_builder: *const ObjectStoreBuilder,
    out_db: *mut *mut slatedb_db_t
) -> slatedb_result_t {
    
    if let Err(err) = require_out_ptr(out_db, "out_db") {
        return err;
    }
        
    let path = match cstr_to_string(path, "path") {
        Ok(path) => path,
        Err(err) => return err,
    };

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

     match runtime.block_on(Db::open(path, object_store)) {
        Ok(db) => {
            let handle = Box::new(slatedb_db_t { runtime, db });
            *out_db = Box::into_raw(handle);
            success_result()
        }
        Err(err) => error_from_slate_error(&err),
    }
}

/// Create a new DbBuilder with custom ObjectStoreBuilder
#[no_mangle]
pub unsafe extern "C" fn slatedb_builder_new_with_object_builder(
    path: *const std::os::raw::c_char,
    object_store_builder: *const ObjectStoreBuilder,
    out_builder: *mut *mut slatedb_db_builder_t,
) -> slatedb_result_t {
    
    if let Err(err) = require_out_ptr(out_builder, "out_builder") {
        return err;
    }

    let path = match cstr_to_string(path, "path") {
        Ok(path) => path,
        Err(err) => return err,
    };

    let objectBuilder = unsafe {object_store_builder.as_ref().unwrap()};

    let object_store = match objectBuilder.build() {
        Ok(store) => store,
        Err(err) => return error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Failed to build object store: {:?}", err.message)),
    };

    let builder = Db::builder(path, object_store);

    let handle = Box::new(slatedb_db_builder_t {
        builder: Some(builder),
    });
    *out_builder = Box::into_raw(handle);
    success_result()
}
