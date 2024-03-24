use lazy_static::lazy_static;

lazy_static! {
    pub static ref IS_SERVER: bool = check_for_server();
}

pub fn check_for_server() -> bool {
    std::env::current_exe()
        .ok()
        .and_then(|pb| pb.file_name().map(|s| s.to_os_string()))
        .and_then(|s| s.into_string().ok())
        .map(|s| s == "SonsOfTheForestDS.exe").unwrap()
}