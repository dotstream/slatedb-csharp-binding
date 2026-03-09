// ============================================================================
// Alternative Iterator SeekToBeginning
// ============================================================================
/// # Safety
///
/// - `iter` must be a valid pointer to a CSdbIterator
/// - `key` must point to valid memory of at least `key_len` bytes
#[no_mangle]
pub unsafe extern "C" fn slatedb_iterator_seek_from_beginning(
    iter: *mut CSdbIterator
) -> CSdbResult {
    if iter.is_null() {
        return create_error_result(CSdbError::NullPointer, "Iterator pointer is null");
    }


    let iter_ffi = unsafe { &mut *iter };
    let owner = iter_ffi.owner();

    // Validate owner pointer is still alive (basic check)
    if !iter_ffi.is_owner_valid() {
        return create_error_result(CSdbError::InvalidHandle, "Invalid database handle");
    }

    match CSdbIterator::block_on_with_owner(owner, iter_ffi.iter.seek(b"")) {
        Ok(_) => create_success_result(),
        Err(e) => {
            let error_code = slate_error_to_code(&e);
            create_error_result(error_code, &format!("Iterator seek failed: {}", e))
        }
    }
}