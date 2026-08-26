// ============================================================================
// UniFFI port of alt_iterator.rs
// ============================================================================
//
// Draft only — not wired into `lib.rs`. Verified (via a scratch build against
// the real slatedb 0.12.0 uniffi crate) to compile cleanly when appended
// as-is to the bottom of `src/iterator.rs` — exactly how
// `get-slatedb-c-bindings.sh` merges `alt/alt_*.rs` into `src/*.rs` for each
// matching filename (`cat src/iterator.rs alt/alt_iterator.rs > ...`). To
// adopt: rename this file to `alt_iterator.rs` and drop it in `rust/slatedb-ffi/alt/`.
//
// `DbIterator::seek` (already in src/iterator.rs) rejects empty keys via
// `validate_key`, since an empty key is invalid as a *data* key. But
// SlateDB's underlying iterator treats an empty key as "rewind to the start
// of the range" — that's what the old `slatedb_iterator_seek_from_beginning`
// relied on. A second `impl` block is used here (same pattern as
// src/db.rs, which already splits sync vs. async-runtime exports across two
// `impl Db` blocks) so `validate_key` doesn't need to be bypassed inline, and
// the private `inner` field remains directly accessible since this lands in
// the same module.

#[uniffi::export(async_runtime = "tokio")]
impl DbIterator {
    /// Seeks the iterator back to the start of its range.
    pub async fn seek_to_beginning(&self) -> Result<(), Error> {
        let mut guard = self.inner.lock().await;
        guard.seek(Vec::new()).await.map_err(Into::into)
    }
}
