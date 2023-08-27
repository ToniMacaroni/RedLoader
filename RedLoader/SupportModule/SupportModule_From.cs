namespace RedLoader
{
    internal class SupportModule_From : ISupportModule_From
    {
        public void OnApplicationLateStart()
            => GlobalEvents.OnApplicationLateStart.Invoke();

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
            => GlobalEvents.OnSceneWasLoaded.Invoke(buildIndex, sceneName);

        public void OnSceneWasInitialized(int buildIndex, string sceneName)
            => GlobalEvents.OnSceneWasInitialized.Invoke(buildIndex, sceneName);

        public void OnSceneWasUnloaded(int buildIndex, string sceneName)
            => GlobalEvents.OnSceneWasUnloaded.Invoke(buildIndex, sceneName);

        public void Update()
            => GlobalEvents.OnUpdate.Invoke();

        public void FixedUpdate()
            => GlobalEvents.OnFixedUpdate.Invoke();

        public void LateUpdate()
            => GlobalEvents.OnLateUpdate.Invoke();

        public void OnGUI()
            => GlobalEvents.OnGUI.Invoke();

        public void Quit()
            => GlobalEvents.OnApplicationQuit.Invoke();

        public void DefiniteQuit()
        {
            GlobalEvents.OnApplicationDefiniteQuit.Invoke();
            Core.Quit();
        }

        public void SetInteropSupportInterface(InteropSupport.Interface interop)
        {
            if (InteropSupport.SMInterface == null)
                InteropSupport.SMInterface = interop;
        }
    }
}