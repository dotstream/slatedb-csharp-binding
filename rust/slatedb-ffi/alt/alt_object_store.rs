// ============================================================================
// Alternative Open Functions with Builder
// ============================================================================
use object_store::aws::{AmazonS3ConfigKey, S3ConditionalPut};
use object_store::azure::{AzureConfigKey, MicrosoftAzureBuilder};
use object_store::gcp::{GoogleCloudStorageBuilder, GoogleConfigKey};
use object_store::local::LocalFileSystem;
use object_store::memory::InMemory;
use object_store::ObjectStore;
use std::sync::Arc;
use std::str::FromStr;

#[repr(C)]
pub enum ObjectStoreType {
    Azure,
    Local,
    InMemory,
    S3,
    GCS,
}

pub struct ObjectStoreBuilder {
    config_map: std::collections::HashMap<String, String>,
    object_store_type: ObjectStoreType,
}

pub struct ObjectStoreConfig {
    pub config_map: std::collections::HashMap<String, String>
}

#[repr(C)]
pub struct ObjectStoreConfigItem {
    key: *mut c_char,
    value: *mut c_char,
}

#[repr(C)]
pub struct ObjectStoreConfigArray {
    data: *mut ObjectStoreConfigItem,
    len: usize,
}

impl Clone for ObjectStoreConfig {
    fn clone(&self) -> Self {
        ObjectStoreConfig {
            config_map: self.config_map.clone(),
        }
    }
}

impl ObjectStoreBuilder {

    pub fn new() -> Self {
        ObjectStoreBuilder {
            config_map: std::collections::HashMap::new(),
            object_store_type: ObjectStoreType::InMemory,
        }
    }

    pub fn build(&self) -> Result<Arc<dyn ObjectStore>, slatedb_result_t> {
        
        match self.object_store_type {
            ObjectStoreType::Azure => self.build_azure(),
            ObjectStoreType::InMemory => self.build_in_memory(),
            ObjectStoreType::Local => self.build_local(),
            ObjectStoreType::S3 => self.build_s3(),
            ObjectStoreType::GCS => self.build_gcs(),
        }
    }

    fn build_azure(&self) -> Result<Arc<dyn ObjectStore>, slatedb_result_t> {
        let mut builder = MicrosoftAzureBuilder::new();
        
        self.config_map.iter().try_for_each(|(key, value)| {
           let mut b = builder.clone();
           let azure_key = AzureConfigKey::from_str(key)
            .map_err(|_| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Invalid Azure config key: {}", key)))?;
            b = b.with_config(azure_key, value);
            builder = b;
            Ok::<(), slatedb_result_t>(())
        })?;

        let store = builder.build()
        .map_err(|e| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Failed to build Azure object store: {}", e)))?;

        Ok(Arc::new(store) as Arc<dyn ObjectStore>)
    }
    
    fn build_in_memory(&self) -> Result<Arc<dyn ObjectStore>, slatedb_result_t> {
        let object_store = Arc::new(InMemory::new());
        Ok(object_store)
    }
    
    fn build_s3(&self) -> Result<Arc<dyn ObjectStore>, slatedb_result_t> {

        let mut builder = object_store::aws::AmazonS3Builder::new();

        self.config_map.iter().try_for_each(|(key, value)| {
           let mut b = builder.clone();
           let aws_key = AmazonS3ConfigKey::from_str(key)
            .map_err(|_| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Invalid S3 config key: {}", key)))?;
            b = b.with_config(aws_key, value);
            builder = b;
            Ok::<(), slatedb_result_t>(())
        })?;

        builder = builder.with_conditional_put(S3ConditionalPut::ETagMatch);

        let store = builder.build()
        .map_err(|e| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Failed to build S3 object store: {}", e)))?;

        Ok(Arc::new(store) as Arc<dyn ObjectStore>)
    }
    
    fn build_gcs(&self) -> Result<Arc<dyn ObjectStore>, slatedb_result_t> {
        let mut builder = GoogleCloudStorageBuilder::new();

        self.config_map.iter().try_for_each(|(key, value)| {
           let mut b = builder.clone();
           let google_key = GoogleConfigKey::from_str(key)
            .map_err(|_| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Invalid Google config key: {}", key)))?;
            b = b.with_config(google_key, value);
            builder = b;
            Ok::<(), slatedb_result_t>(())
        })?;

        let store = builder.build()
        .map_err(|e| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Failed to build GCS object store: {}", e)))?;

        Ok(Arc::new(store) as Arc<dyn ObjectStore>)
    }
    
    fn build_local(&self) -> Result<Arc<dyn ObjectStore>, slatedb_result_t> {
        let local_path = self.config_map.get("local_path")
            .ok_or_else(|| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, "Missing local_path configuration for Local object store"))?;
        
        let lfs = LocalFileSystem::new_with_prefix(local_path)
            .map_err(|e| error_result(slatedb_error_kind_t::SLATEDB_ERROR_KIND_INTERNAL, &format!("Failed to build Local object store: {}", e)))?;

        Ok(Arc::new(lfs) as Arc<dyn ObjectStore>)
    }

}

#[no_mangle]
pub extern "C" fn slatedb_object_store_builder_new(
    _type: ObjectStoreType,
    config_map: *const ObjectStoreConfig
) -> *mut ObjectStoreBuilder {

    let mut object_store_builder = ObjectStoreBuilder::new();
    object_store_builder.config_map = unsafe { (*config_map).config_map.clone() };
    object_store_builder.object_store_type = _type;

    Box::into_raw(Box::new(object_store_builder))
}

#[no_mangle]
pub extern "C" fn slatedb_object_store_builder_config_new()
    -> *mut ObjectStoreConfig {
    Box::into_raw(Box::new(ObjectStoreConfig { config_map: std::collections::HashMap::new() }))
}

#[no_mangle]
pub extern "C" fn slatedb_object_store_builder_config_set(
    config: *mut ObjectStoreConfig,
    key: *const std::os::raw::c_char,
    value: *const std::os::raw::c_char
) {
    let c_str_key = unsafe { std::ffi::CStr::from_ptr(key) };
    let c_str_value = unsafe { std::ffi::CStr::from_ptr(value) };

    if let (Ok(key_str), Ok(value_str)) = (c_str_key.to_str(), c_str_value.to_str()) {
        unsafe {
            (*config).config_map.insert(key_str.to_string(), value_str.to_string());
        }
    }
}

#[no_mangle]
pub extern "C" fn slatedb_object_store_builder_config_delete(
    config: *mut ObjectStoreConfig,
    key: *const std::os::raw::c_char) {
    let c_str_key = unsafe { std::ffi::CStr::from_ptr(key) };       
    if let Ok(key_str) = c_str_key.to_str() {
        unsafe {
            (*config).config_map.remove(key_str);
        }
    }
}

#[no_mangle]
pub extern "C" fn slatedb_object_store_builder_config_get(
    config: *mut ObjectStoreConfig) -> ObjectStoreConfigArray {

    let config = unsafe { &*config };
    let mut vec = config
            .config_map
            .iter()
            .map(|(key, value)| {
                let c_key = std::ffi::CString::new(key.as_str()).unwrap();
                let c_value = std::ffi::CString::new(value.as_str()).unwrap();
                ObjectStoreConfigItem {
                    key: c_key.into_raw(),
                    value: c_value.into_raw(),
                }
        }).collect::<Vec<_>>();

    let len = vec.len();
    let data = vec.as_mut_ptr();
    std::mem::forget(vec);
    ObjectStoreConfigArray { data, len }
}

#[no_mangle]
pub extern "C" fn slatedb_object_store_builder_config_get_free(arr: ObjectStoreConfigArray) {
    unsafe {
        let slice = std::slice::from_raw_parts(arr.data, arr.len);

        for item in slice {
            if !item.key.is_null() {
                drop(std::ffi::CString::from_raw(item.key));
            }
            if !item.value.is_null() {
                drop(std::ffi::CString::from_raw(item.value));
            }
        }

        drop(Vec::from_raw_parts(arr.data, arr.len, arr.len));
    }
}

#[no_mangle]
pub extern "C" fn slatedb_object_store_builder_config_free(config: *mut ObjectStoreConfig) {
    unsafe {
        drop(Box::from_raw(config));
    }
}   