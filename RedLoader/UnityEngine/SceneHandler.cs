using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;
#if SM_Il2Cpp
using UnityEngine.Events;
#endif

namespace RedLoader.Support
{
    internal static class SceneHandler
    {
        internal class SceneInitEvent
        {
            internal int buildIndex;
            internal string name;
            internal bool wasLoadedThisTick;
        }

        private static Queue<SceneInitEvent> scenesLoaded = new Queue<SceneInitEvent>();

        internal static void Init()
        {
            try
            {
                SceneManager.sceneLoaded = (
                                               (ReferenceEquals(SceneManager.sceneLoaded, null))
                                                   ? new Action<Scene, LoadSceneMode>(OnSceneLoad)
                                                   : Il2CppSystem.Delegate.Combine(SceneManager.sceneLoaded, (UnityAction<Scene, LoadSceneMode>)new Action<Scene, LoadSceneMode>(OnSceneLoad)).Cast<UnityAction<Scene, LoadSceneMode>>()
                                           );
            }
            catch (Exception ex) { RLog.Error($"SceneManager.sceneLoaded override failed: {ex}"); }

            try
            {
                SceneManager.sceneUnloaded = (
                                                 (ReferenceEquals(SceneManager.sceneUnloaded, null))
                                                     ? new Action<Scene>(OnSceneUnload)
                                                     : Il2CppSystem.Delegate.Combine(SceneManager.sceneUnloaded, (UnityAction<Scene>)new Action<Scene>(OnSceneUnload)).Cast<UnityAction<Scene>>()
                                             );
            }
            catch (Exception ex) { RLog.Error($"SceneManager.sceneUnloaded override failed: {ex}"); }
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (ReferenceEquals(scene, null))
                return;

            GlobalEvents.OnSceneWasLoaded.Invoke(scene.buildIndex, scene.name);
            scenesLoaded.Enqueue(new SceneInitEvent { buildIndex = scene.buildIndex, name = scene.name });
        }

        private static void OnSceneUnload(Scene scene)
        {
            if (ReferenceEquals(scene, null))
                return;

            GlobalEvents.OnSceneWasUnloaded.Invoke(scene.buildIndex, scene.name);
        }

        internal static void OnUpdate()
        {
            if (scenesLoaded.Count > 0)
            {
                Queue<SceneInitEvent> requeue = new Queue<SceneInitEvent>();
                SceneInitEvent evt = null;
                while ((scenesLoaded.Count > 0) && ((evt = scenesLoaded.Dequeue()) != null))
                {
                    if (evt.wasLoadedThisTick)
                        GlobalEvents.OnSceneWasInitialized.Invoke(evt.buildIndex, evt.name);
                    else
                    {
                        evt.wasLoadedThisTick = true;
                        requeue.Enqueue(evt);
                    }
                }
                while ((requeue.Count > 0) && ((evt = requeue.Dequeue()) != null))
                    scenesLoaded.Enqueue(evt);
            }
        }
    }
}
