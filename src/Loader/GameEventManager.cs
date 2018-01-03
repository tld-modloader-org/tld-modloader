using System;
using UnityEngine.SceneManagement;

namespace Loader
{
    public static class GameEventManager
    {
        public static event Action MainMenuLoaded;
        public static event Action<Scene> WorldLoaded;
        public static event Action GamePause;
        public static event Action GameUnpause;

        public static void OnMainMenuLoaded() => MainMenuLoaded?.Invoke();
        public static void OnWorldLoaded(Scene scene) => WorldLoaded?.Invoke(scene);
        public static void OnGamePause() => GamePause?.Invoke();
        public static void OnGameUnpause() => GameUnpause?.Invoke();
    }
}
