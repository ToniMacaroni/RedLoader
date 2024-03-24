use std::path::PathBuf;

use thiserror::Error;

#[derive(Debug, Error)]
pub enum ProxyError {
    #[error("Failed to find Bootstrap at \"{0}\" please make sure you have installed RedLoader correctly")]
    BootstrapNotFound(PathBuf)
}