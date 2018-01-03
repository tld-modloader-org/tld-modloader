using Harmony;

namespace Loader.Hooks
{
    [HarmonyPatch(typeof(Panel_MainMenu), "Awake")]
    internal static class MainMenuLoadedHook
    {
        private static void Prefix()
        {
            GameEventManager.OnMainMenuLoaded();
        }
    }
}