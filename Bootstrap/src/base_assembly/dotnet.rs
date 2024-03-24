#![allow(unused_imports)]

use lazy_static::lazy_static;
use netcorehost::{hostfxr, nethost, pdcstr};
use std::{
    ffi::c_void, fs::{self, File}, io::{copy, Read}, path::Path, ptr::{addr_of, addr_of_mut, null_mut}, sync::RwLock
};

use crate::{
    debug,
    errors::{dotneterr::DotnetErr, DynErr},
    icalls, melonenv::{self, context::IS_SERVER},
    utils::{self, strings::wide_str},
};

/// These are functions that NativeHost.dll will fill in, once we call LoadStage1.
/// Interacting with the .net runtime is a pain, so it's a lot easier to just have it give us pointers like this directly.
#[repr(C)]
#[derive(Debug)]
pub struct HostImports {
    pub load_assembly_get_ptr: fn(isize, isize, isize, *mut *mut c_void),

    pub initialize: fn(),
    pub pre_start: fn(),
    pub start: fn(),
}

/// These are functions that we will pass to NativeHost.dll.
/// CoreCLR does not have internal calls like mono does, so we have to pass these ourselves.
/// They are stored in Managed, and are accessed by MelonLoader for hooking.
#[repr(C)]
#[derive(Debug)]
pub struct HostExports {
    pub hook_attach: unsafe fn(*mut *mut c_void, *mut c_void),
    pub hook_detach: unsafe fn(*mut *mut c_void, *mut c_void),
}

// Initializing the host imports as a static variable. Later on this is replaced with a filled in version of the struct.
lazy_static! {
    pub static ref IMPORTS: RwLock<HostImports> = RwLock::new(HostImports {
        load_assembly_get_ptr: |_, _, _, _| {},
        initialize: || {},
        pre_start: || {},
        start: || {},
    });
}

pub fn init() -> Result<(), DynErr> {
    let runtime_dir = melonenv::paths::runtime_dir()?;

    let mut hostfxr = nethost::load_hostfxr();
    if hostfxr.is_err() {
        println!("Failed to load hostfxr, attempting to setup .NET runtime...");
        setup_dotnet()?;
        hostfxr = nethost::load_hostfxr();
    }

    //let hostfxr = nethost::load_hostfxr().map_err(|_| DotnetErr::FailedHostFXRLoad)?;

    let config_path = runtime_dir.join("RedLoader.runtimeconfig.json");
    if !config_path.exists() {
        return Err(DotnetErr::RuntimeConfig.into());
    }

    let context = hostfxr.map_err(|_| DotnetErr::FailedHostFXRLoad)?.initialize_for_runtime_config(utils::strings::pdcstr(config_path)?)?;

    let loader = context.get_delegate_loader_for_assembly(utils::strings::pdcstr(
        runtime_dir.join("NativeHost.dll"),
    )?)?;

    let init = loader.get_function_with_unmanaged_callers_only::<fn(*mut HostImports)>(
        pdcstr!("NativeHost.NativeEntryPoint, NativeHost"),
        pdcstr!("LoadStage1"),
    )?;

    let mut imports = HostImports {
        load_assembly_get_ptr: |_, _, _, _| {},
        initialize: || {},
        pre_start: || {},
        start: || {},
    };

    let mut exports = HostExports {
        hook_attach: icalls::bootstrap_interop::attach,
        hook_detach: icalls::bootstrap_interop::detach,
    };

    debug!("[Dotnet] Invoking LoadStage1")?;
    //NativeHost will fill in the HostImports struct with pointers to functions
    init(addr_of_mut!(imports));

    debug!("[Dotnet] Reloading NativeHost into correct load context and getting LoadStage2 pointer")?;

    //a function pointer to be filled
    let mut init_stage_two = null_mut::<c_void>();

    //have to make all strings utf16 for C# to understand, of course they can only be passed as IntPtrs
    (imports.load_assembly_get_ptr)(
        wide_str(runtime_dir.join("NativeHost.dll"))?.as_ptr() as isize,
        wide_str("NativeHost.NativeEntryPoint, NativeHost")?.as_ptr()
            as isize,
        wide_str("LoadStage2")?.as_ptr() as isize,
        addr_of_mut!(init_stage_two),
    );

    debug!("[Dotnet] Invoking LoadStage2")?;

    //turn the function pointer into a function we can invoke
    let init_stage_two: fn(*mut HostImports, *mut HostExports) =
        unsafe { std::mem::transmute(init_stage_two) };
    init_stage_two(addr_of_mut!(imports), addr_of_mut!(exports));

    if addr_of!(imports.initialize).is_null() {
        Err("Failed to get HostImports::Initialize!")?
    }

    (imports.initialize)();

    *IMPORTS.try_write()? = imports;

    Ok(())
}

pub fn pre_start() -> Result<(), DynErr> {
    let imports = IMPORTS.try_read()?;

    (imports.pre_start)();

    Ok(())
}

pub fn start() -> Result<(), DynErr> {
    let imports = IMPORTS.try_read()?;

    (imports.start)();

    Ok(())
}

fn setup_dotnet() -> Result<(), DynErr> {
    let netrt_path = Path::new("dotnetrt");

    if Path::exists(netrt_path) {
        std::env::set_var("DOTNET_ROOT", netrt_path.canonicalize()?);
        return Ok(());
    }

    let response = ureq::get("https://dotnetcli.azureedge.net/dotnet/Runtime/6.0.0/dotnet-runtime-6.0.0-win-x64.zip").call()?;

    let archive_path = Path::new("dotnet.zip");

    let _ = copy(&mut response.into_reader(), &mut File::create(archive_path)?)?;

    fs::create_dir_all(&netrt_path)?;

    let file = File::open(&archive_path)?;
    let mut archive = zip::ZipArchive::new(file)?;

    for i in 0..archive.len() {
        let mut file = archive.by_index(i)?;
        let file_path = netrt_path.join(file.name());

        if file.is_dir() {
            fs::create_dir_all(&file_path)?;
        } else {
            if let Some(parent) = file_path.parent() {
                if !parent.exists() {
                    fs::create_dir_all(&parent)?;
                }
            }
            let mut extracted_file = fs::File::create(&file_path)?;
            let _ = std::io::copy(&mut file, &mut extracted_file)?;
        }
    }

    fs::remove_file(&archive_path)?;

    std::env::set_var("DOTNET_ROOT", netrt_path.canonicalize()?);
    
    Ok(())
}