// ============================================================================
// Alternative Iterator SeekToBeginning
// ============================================================================
/// # Safety
///
/// - `iter` must be a valid pointer to a CSdbIterator
/// - `key` must point to valid memory of at least `key_len` bytes
#[no_mangle]
pub unsafe extern "C" fn slatedb_iterator_seek_from_beginning(
    iterator: *mut slatedb_iterator_t
) -> slatedb_result_t {

     if let Err(err) = require_handle(iterator, "iterator") {
        return err;
    }

    let _iter_ffi = unsafe { &mut *iterator };
    let _handle = &mut *iterator;

    let handle = &mut *iterator;
    match handle.runtime.block_on(handle.iter.seek(b"")) {
        Ok(()) => success_result(),
        Err(err) => error_from_slate_error(&err),
    }
}