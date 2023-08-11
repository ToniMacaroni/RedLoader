using System;
using System.Collections.Generic;
#pragma warning disable 0618 // Disabling the obsolete references warning to prevent the IDE going crazy when subscribing deprecated methods to some events in RegisterCallbacks

namespace MelonLoader
{
    public abstract class MelonMod : MelonTypeBase<MelonMod>
    {
        static MelonMod()
        {
            TypeName = "Mod";
        }

        protected private override bool RegisterInternal()
        {
            try
            {
                OnPreSupportModule();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to register {MelonTypeName} '{MelonAssembly.Location}': Melon failed to initialize in the deprecated OnPreSupportModule callback!");
                MelonLogger.Error(ex.ToString());
                return false;
            }

            if (!base.RegisterInternal())
                return false;

            if (MelonEvents.MelonHarmonyInit.Disposed)
                HarmonyInit();
            else
                MelonEvents.MelonHarmonyInit.Subscribe(HarmonyInit, Priority, true);

            return true;
        }
        private void HarmonyInit()
        {
            if (!MelonAssembly.HarmonyDontPatchAll)
                HarmonyInstance.PatchAll(MelonAssembly.Assembly);
        }

        protected private override void RegisterCallbacks()
        {
            base.RegisterCallbacks();

            MelonEvents.OnSceneWasLoaded.Subscribe(OnSceneWasLoaded, Priority);
            MelonEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority);
            MelonEvents.OnSceneWasUnloaded.Subscribe(OnSceneWasUnloaded, Priority);
        }

        #region Callbacks

        /// <summary>
        /// Runs when a new Scene is loaded.
        /// </summary>
        public virtual void OnSceneWasLoaded(int buildIndex, string sceneName) { }

        /// <summary>
        /// Runs once a Scene is initialized.
        /// </summary>
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName) { }

        /// <summary>
        /// Runs once a Scene unloads.
        /// </summary>
        public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName) { }

        #endregion
    }
}