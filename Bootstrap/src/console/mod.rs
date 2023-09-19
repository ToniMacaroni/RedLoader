pub mod os;
use lazy_static::lazy_static;

#[cfg(all(unix))]
use os::unix as imp;

#[cfg(all(windows))]
use os::windows as imp;

use crate::{errors::DynErr, hide_console};

lazy_static! {
    static ref IS_SERVER: bool = check_for_server();
}

pub fn check_for_server() -> bool {
    std::env::current_exe()
        .ok()
        .and_then(|pb| pb.file_name().map(|s| s.to_os_string()))
        .and_then(|s| s.into_string().ok())
        .map(|s| s == "SonsOfTheForestDS.exe").unwrap()
}

pub fn init() -> Result<(), DynErr> {
    if *IS_SERVER {
        return Ok(());
    }

    if hide_console!() {
        return Ok(());
    }

    unsafe { imp::init() }
}

pub fn null_handles() -> Result<(), DynErr> {
    if *IS_SERVER {
        return Ok(());
    }

    if hide_console!() {
        return Ok(());
    }

    imp::null_handles()
}

pub fn set_handles() -> Result<(), DynErr> {
    if *IS_SERVER {
        return Ok(());
    }
    
    if hide_console!() {
        return Ok(());
    }

    imp::set_handles()
}

pub fn set_title(title: &str) {
    if *IS_SERVER {
        return;
    }

    if hide_console!() {
        return;
    }

    imp::set_title(title)
}
