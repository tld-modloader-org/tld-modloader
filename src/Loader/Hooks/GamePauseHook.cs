using Harmony;

namespace Loader.Hooks
{
    [HarmonyPatch(typeof(Panel_PauseMenu), "Enable", new [] { typeof(bool) })]
    internal static class GamePauseHook
    {
        private static void Postfix(bool enable)
        {
            if (enable) GameEventManager.OnGamePause();
            else GameEventManager.OnGameUnpause();
        }
    }
}