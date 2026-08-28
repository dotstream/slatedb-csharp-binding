// ============================================================================
// UniFFI error additions required by uniffi_object_store.rs
// ============================================================================
//
// Unlike the other uniffi_*.rs files, this is not appended after
// `src/error.rs` — it's spliced into the body of the existing
// `pub(crate) enum SlateDbError { ... }`, right before its closing brace, by
// `rust/slatedb-ffi/alt/alt_post_0.12.0.sh`. Keep this file scoped to bare
// variant declarations (no enum wrapper, no `use`/`impl` blocks) so that
// splice stays valid.
//
// `SlateDbError::ObjectStoreCreationError` (already defined upstream) is
// reused as-is by uniffi_object_store.rs for provider build failures, so no
// variant for that case is added here.

    #[error("object store creation failed: {source}")]
    ObjectStoreCreationError {
        #[from]
        source: Box<dyn StdError>,
    },

    #[error("invalid {provider} object store config key: {key}")]
    InvalidObjectStoreConfigKey { provider: &'static str, key: String },

    #[error("missing required {provider} object store config key: {key}")]
    MissingObjectStoreConfigKey {
        provider: &'static str,
        key: &'static str,
    },
