using System;
using System.Collections.Generic;
#pragma warning disable 0618 // Disabling the obsolete references warning to prevent the IDE going crazy when subscribing deprecated methods to some events in RegisterCallbacks

namespace RedLoader
{
    public abstract class MelonPlugin : ModTypeBase<MelonPlugin>
    {
        static MelonPlugin()
        {
            TypeName = "Plugin";
        }

        protected private override void RegisterCallbacks()
        {
            base.RegisterCallbacks();

            GlobalEvents.OnPreInitialization.Subscribe(OnPreInitialization, Priority);
            GlobalEvents.OnApplicationEarlyStart.Subscribe(OnApplicationEarlyStart, Priority);
            GlobalEvents.OnPreModsLoaded.Subscribe(OnPreModsLoaded, Priority);
            GlobalEvents.OnApplicationStart.Subscribe(OnApplicationStarted, Priority);
            GlobalEvents.OnPreSupportModule.Subscribe(OnPreSupportModule, Priority);
        }

        protected private override bool RegisterInternal()
        {
            if (!base.RegisterInternal())
                return false;

            if (GlobalEvents.MelonHarmonyEarlyInit.Disposed)
                HarmonyInit();
            else
                GlobalEvents.MelonHarmonyEarlyInit.Subscribe(HarmonyInit, Priority, true);

            return true;
        }
        private void HarmonyInit()
        {
            if (!MelonAssembly.HarmonyDontPatchAll)
                HarmonyInstance.PatchAll(MelonAssembly.Assembly);
        }

        #region Callbacks

        /// <summary>
        /// Runs before Game Initialization.
        /// </summary>
        public virtual void OnPreInitialization() { }

        /// <summary>
        /// Runs after Game Initialization, before OnApplicationStart and before Assembly Generation on Il2Cpp games
        /// </summary>
        public virtual void OnApplicationEarlyStart() { }

        /// <summary>
        /// Runs before MelonMods from the Mods folder are loaded.
        /// </summary>
        public virtual void OnPreModsLoaded() { }

        /// <summary>
        /// Runs after all RedLoader components are fully initialized (including all MelonMods).
        /// </summary>
        public virtual void OnApplicationStarted() { }

        #endregion
    }
}