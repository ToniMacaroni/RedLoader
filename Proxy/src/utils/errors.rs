use std::path::PathBuf;

use thiserror::Error;

#[derive(Debug, Error)]
pub enum ProxyError {
    #[error("Failed to find Bootstrap at \"{0}\" please make sure you have installed MelonLoader correctly")]
    BootstrapNotFound(PathBuf)
}