using Harmony;
using UnityEngine.SceneManagement;

namespace Loader.Hooks
{
    [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded", new [] { typeof(Scene), typeof(LoadSceneMode) })]
    internal static class WorldLoadedHook
    {
        private static void Prefix(Scene scene, LoadSceneMode mode)
        {
            if (!scene.name.ToLower().Contains("region")) return;
            
            GameEventManager.OnWorldLoaded(scene);
        }
    }
}