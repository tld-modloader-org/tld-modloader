using System;

namespace Loader
{
    public static class GameEventManager
    {
        public static event Action MainMenuLoaded;
        public static event Action WorldLoaded;
        public static event Action GamePauseOpen;
        public static event Action GamePauseClose;

        public static void MainMenuLoadedTrigger() => MainMenuLoaded?.Invoke();
        public static void WorldLoadedTrigger() => WorldLoaded?.Invoke();
        public static void GamePauseOpenTrigger() => GamePauseOpen?.Invoke();
        public static void GamePauseCloseTrigger() => GamePauseClose?.Invoke();
    }
}
