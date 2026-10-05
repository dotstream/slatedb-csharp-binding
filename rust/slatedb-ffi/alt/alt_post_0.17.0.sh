#!/bin/bash

# Since 0.17.0, upstream ships its own `ObjectStoreBuilder` (src/object_store_builder.rs)
# along with the `InvalidObjectStoreConfigKey` / `MissingObjectStoreConfigKey` error
# variants, so uniffi_object_store.rs and uniffi_error.rs are no longer merged.