//! various filesystem utils

use std::{env::consts::DLL_EXTENSION, error::Error, path::PathBuf, fs};

use super::errors::ProxyError;

/// search for Bootstrap in the given path
pub fn get_bootstrap_path(base_path: &PathBuf) -> Result<PathBuf, ProxyError> {
    let bootstrap_names = ["Bootstrap", "libBootstrap"]; //by convention, on unix, the library is prefixed with "lib"

    let path = base_path.join("_RedLoader").join("Dependencies");

    for name in bootstrap_names.iter() {
        let bootstrap_path = path.join(name).with_extension(DLL_EXTENSION);

        if bootstrap_path.exists() {
            return Ok(bootstrap_path);
        }
    }

    Err(ProxyError::BootstrapNotFound(base_path.to_owned()))
}

pub fn is_unity(file_path: &PathBuf) -> Result<bool, Box<dyn Error>> {
    let file_name = file_path
        .file_stem()
        .ok_or("Failed to get file stem")?
        .to_str()
        .ok_or("Failed to convert file stem to str")?;

    let base_folder = file_path.parent().ok_or("Failed to get parent folder")?;

    let data_path = base_folder.join(format!("{file_name}_Data"));

    if !data_path.exists() {
        return Ok(false);
    }

    let global_game_managers = data_path.join("globalgamemanagers");
    let data_unity3d = data_path.join("data.unity3d");
    let main_data = data_path.join("mainData");

    if global_game_managers.exists() || data_unity3d.exists() || main_data.exists() {
        Ok(true)
    } else {
        Ok(false)
    }
}

pub fn check_bepinex_installation(file_path: &PathBuf) -> Result<(), Box<dyn Error>> {
    let base_folder = file_path.parent().ok_or("Failed to get parent folder")?;
    let winhttp_file = base_folder.join("winhttp.dll");
    if !winhttp_file.exists() {
        return Ok(());
    }

    msgbox::create("RedLoader", "BepInEx will be disabled, since it's not compatible with RedLoader. The game will exit now. Just restart it afterwards.", msgbox::IconType::Info)?;
    fs::rename(winhttp_file, base_folder.join("winhttp.dll.disabled"))?;
    std::process::exit(0);
    // Ok(())
    //return Ok(winhttp_file.exists());
}
