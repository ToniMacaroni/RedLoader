using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using RedLoader;
using RedLoader.Preloader.Core;
using RedLoader.Unity.IL2CPP;
using RedLoader.Unity.IL2CPP.Utils;
using MonoMod.Utils;
using RedLoader;

// ReSharper disable once CheckNamespace
namespace Doorstop;

internal static class Entrypoint
{
    /// <summary>
    ///     The main entrypoint of Redloader, called from Doorstop.
    /// </summary>
    public static void Start()
    {
        // We set it to the current directory first as a fallback, but try to use the same location as the .exe file.
        var silentExceptionLog = Environment.GetEnvironmentVariable("PRELOADER_LOG") ??
                                 $"preloader_{DateTime.Now:yyyyMMdd_HHmmss_fff}.log";
        Mutex mutex = null;

        try
        {
            EnvVars.LoadVars();

            silentExceptionLog =
                Path.Combine(Path.GetDirectoryName(EnvVars.DOORSTOP_PROCESS_PATH), "_Redloader", silentExceptionLog);

            var mutexId = Utility.HashStrings(Process.GetCurrentProcess().ProcessName, EnvVars.DOORSTOP_PROCESS_PATH,
                                              typeof(Entrypoint).FullName);

            mutex = new Mutex(false, $"Global\\{mutexId}");
            mutex.WaitOne();

            UnityPreloaderRunner.PreloaderMain();
        }
        catch (Exception ex)
        {
            File.WriteAllText(silentExceptionLog, ex.ToString());

            try
            {
                if (PlatformHelper.Is(Platform.Windows))
                {
                    MessageBox.Show("Failed to start Redloader", "Redloader");
                }
                else if (NotifySend.IsSupported)
                {
                    NotifySend.Send("Failed to start Redloader", "Check logs for details");
                }
                else if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDLOADER_FAIL_FAST")))
                {
                    // Don't exit the game if we have no way of signaling to the user that a crash happened
                    return;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            Environment.Exit(1);
        }
        finally
        {
            mutex?.ReleaseMutex();
        }
    }
}
