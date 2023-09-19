﻿using System;
using System.Collections.Generic;
#pragma warning disable 0618 // Disabling the obsolete references warning to prevent the IDE going crazy when subscribing deprecated methods to some events in RegisterCallbacks

namespace RedLoader
{
    [Obsolete("This class is deprecated, please use SonsMod instead.")]
    public abstract class MelonMod : ModTypeBase<MelonMod>
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
                RLog.Error($"Failed to register {MelonTypeName} '{ModAssembly.Location}': Melon failed to initialize in the deprecated OnPreSupportModule callback!");
                RLog.Error(ex.ToString());
                return false;
            }

            if (!base.RegisterInternal())
                return false;

            if (GlobalEvents.MelonHarmonyInit.Disposed)
                HarmonyInit();
            else
                GlobalEvents.MelonHarmonyInit.Subscribe(HarmonyInit, Priority, true);

            return true;
        }
        private void HarmonyInit()
        {
            if (!ModAssembly.HarmonyDontPatchAll)
                HarmonyInstance.PatchAll(ModAssembly.Assembly);
        }

        protected private override void RegisterCallbacks()
        {
            base.RegisterCallbacks();

            GlobalEvents.OnSceneWasLoaded.Subscribe(OnSceneWasLoaded, Priority);
            GlobalEvents.OnSceneWasInitialized.Subscribe(OnSceneWasInitialized, Priority);
            GlobalEvents.OnSceneWasUnloaded.Subscribe(OnSceneWasUnloaded, Priority);
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