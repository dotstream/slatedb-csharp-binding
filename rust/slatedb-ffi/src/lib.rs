extern crate slatedb;
extern crate bytes;
extern crate tokio;
extern crate once_cell;

use std::collections::HashMap;
use std::sync::Arc;
use std::sync::Mutex;
use std::cell::RefCell;
use std::ffi::CStr;
use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;
use std::slice;

use once_cell::sync::Lazy;
use tokio::runtime::Builder;

use bytes::Bytes;
use slatedb::{Db, DbBuilder};
use slatedb::config::Settings;
use slatedb::object_store::memory::InMemory;

thread_local! {
    static LAST_ERROR: RefCell<Option<CString>> = RefCell::new(None);
}

#[repr(C)]
#[derive(Debug, PartialEq)]
pub enum SlateDbStatus {
    Ok = 0,
    NotFound = 1,
    Error = 2,
}

static DB_TABLE: Lazy<Mutex<HashMap<u64, (tokio::runtime::Runtime, Db)>>> = Lazy::new(|| Mutex::new(HashMap::new()));
static NEXT_ID: Mutex<u64> = Mutex::new(1);

#[repr(C)]
#[derive(Clone)]
pub struct SlateDbOptions {
    pub block_size: u32,
    pub cache_capacity_bytes: u64,
    pub enable_compression: bool,
}

impl Default for SlateDbOptions {
    fn default() -> Self {
        Self {
            block_size: 4096,
            cache_capacity_bytes: 64 * 1024 * 1024,
            enable_compression: true,
        }
    }
}

fn build_settings_from_options(opts: &SlateDbOptions) -> Settings {
    let settings = Settings::default();
    // todo other mapping

    settings
}

fn new_handle(rt: tokio::runtime::Runtime, db: Db) -> u64 {
    let mut table: std::sync::MutexGuard<'_, HashMap<u64, (tokio::runtime::Runtime, Db)>> = DB_TABLE.lock().unwrap();
    let mut next = NEXT_ID.lock().unwrap();

    let id = *next;
    *next += 1;

    table.insert(id, (rt, db));
    id
}

fn with_db<F, R>(handle: u64, f: F) -> Option<R>
where
    F: FnOnce(&tokio::runtime::Runtime, &Db) -> R,
{
    let table = DB_TABLE.lock().unwrap();
    table.get(&handle).map(|(rt, db)| f(rt, db))
}

fn with_db_mut<F, R>(handle: u64, f: F) -> Option<R>
where
    F: FnOnce(&tokio::runtime::Runtime, &mut Db) -> R,
{
    let mut table = DB_TABLE.lock().unwrap();
    table.get_mut(&handle).map(|(rt, db)| f(rt, db))
}

fn remove_db(handle: u64) {
    let mut table = DB_TABLE.lock().unwrap();
    table.remove(&handle);
}

fn set_last_error(msg: String) -> SlateDbStatus {
    LAST_ERROR.with(|slot| {
        *slot.borrow_mut() = Some(CString::new(msg).unwrap_or_else(|_| CString::new("error").unwrap()));
    });
    SlateDbStatus::Error
}

#[no_mangle]
pub extern "C" fn slatedb_last_error_message() -> *const c_char {
    LAST_ERROR.with(|slot| {
        if let Some(ref cstr) = *slot.borrow() {
            cstr.as_ptr()
        } else {
            ptr::null()
        }
    })
}

#[no_mangle]
pub extern "C" fn slatedb_options_new() -> *mut SlateDbOptions {
    Box::into_raw(Box::new(SlateDbOptions::default()))
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_options_free(opts: *mut SlateDbOptions) {
    if !opts.is_null() {
        drop(Box::from_raw(opts));
    }
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_options_set_block_size(opts: *mut SlateDbOptions, block_size: u32) {
    if let Some(o) = opts.as_mut() {
        o.block_size = block_size;
    }
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_options_set_cache_capacity(
    opts: *mut SlateDbOptions,
    cache_capacity_bytes: u64,
) {
    if let Some(o) = opts.as_mut() {
        o.cache_capacity_bytes = cache_capacity_bytes;
    }
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_options_set_enable_compression(
    opts: *mut SlateDbOptions,
    enable: bool,
) {
    if let Some(o) = opts.as_mut() {
        o.enable_compression = enable;
    }
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_open(
    path: *const c_char,
    opts: *const SlateDbOptions,
    out_handle: *mut u64,
) -> SlateDbStatus {

    if path.is_null() {
        return set_last_error("null pointer in slatedb_open".into());
    }

    let cstr = match CStr::from_ptr(path).to_str() {
        Ok(s) => s,
        Err(e) => return set_last_error(format!("invalid path utf8: {e}")),
    };

    println!("slatedb_open called with path: {}", cstr);

    if out_handle.is_null() {
        return set_last_error("output null pointer in slatedb_open".into());
    }

    let options = if opts.is_null() {
        SlateDbOptions::default()
    } else {
        (*opts).clone()
    };

    let settings = build_settings_from_options(&options);

    let object_store = Arc::new(InMemory::new());

    let rt: tokio::runtime::Runtime = match Builder::new_multi_thread().enable_all().build() {
        Ok(rt) => rt,
        Err(e) => return set_last_error(format!("builder failed to open: {e:?}")),
    };

    let builder = DbBuilder::new(cstr, object_store.clone())
        .with_settings(settings);


    let handle: u64 = match rt.block_on(async {
        builder.build().await
    }) {
        Ok(db) => new_handle(rt, db),
        Err(err) => return set_last_error(format!("db open failed: {err:?}")),
    };

    *out_handle = handle;

    SlateDbStatus::Ok
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_put(
    handle: u64,
    key: *const u8,
    key_len: c_int,
    value: *const u8,
    value_len: c_int,
) -> SlateDbStatus {

    if key.is_null() || value.is_null() || key_len < 0 || value_len < 0 {
        return set_last_error("invalid pointers/lengths in slatedb_put".into());
    }

    let key_slice = slice::from_raw_parts(key, key_len as usize);
    let value_slice = slice::from_raw_parts(value, value_len as usize);


    let res = with_db_mut(handle, |rt, db| rt.block_on(db.put(key_slice, value_slice)));
    match res {
        Some(Ok(())) => SlateDbStatus::Ok,
        Some(Err(e)) => set_last_error(format!("put failed: {e:?}")),
        None => set_last_error(format!("invalid handle {handle}")),
    }
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_get(
    handle: u64,
    key: *const u8,
    key_len: c_int,
    out_value: *mut *const u8,
    out_len: *mut c_int,
) -> SlateDbStatus {
    
    if key.is_null() || out_value.is_null() || out_len.is_null() || key_len < 0 {
        return set_last_error("invalid args in slatedb_get".into());
    }

    let key_slice = slice::from_raw_parts(key, key_len as usize);

    let result = with_db(handle, |rt, db| rt.block_on(db.get(key_slice)));
    let value: Bytes = match result {
        Some(Ok(Some(v))) => v,
        Some(Ok(None)) => return SlateDbStatus::NotFound,
        Some(Err(e)) => return set_last_error(format!("get failed: {e:?}")),
        None => return set_last_error(format!("invalid handle {handle}")),
    };

    let mut vec = value.to_vec();
    let len = vec.len() as c_int; 
    let ptr = vec.as_mut_ptr(); 
    std::mem::forget(vec);

    *out_value = ptr;
    *out_len = len;

    SlateDbStatus::Ok
}

#[no_mangle]
pub unsafe extern "C" fn slatedb_free_value(ptr: *mut u8, len: c_int) {
    if ptr.is_null() || len < 0 {
        return;
    }
    let _ = Vec::from_raw_parts(ptr, len as usize, len as usize);
}

#[no_mangle]
pub extern "C" fn slatedb_close(handle: u64) -> SlateDbStatus {
    let res = with_db_mut(handle, |rt, db| rt.block_on(db.close()));
    match res {
        Some(Ok(_)) => {
            remove_db(handle);
            SlateDbStatus::Ok
        },
        Some(Err(e)) => return set_last_error(format!("close failed: {e:?}")),
        None => return set_last_error(format!("invalid handle {handle}")),
    }
}