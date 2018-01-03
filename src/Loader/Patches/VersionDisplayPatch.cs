using System;
using Harmony;

namespace Loader.Patches
{
    [HarmonyPatch(typeof(GameManager), "GetVersionString")]
    internal static class VersionDisplayPatch
    {
        private static void Postfix(ref string __result)
        {
            __result += Environment.NewLine;
            __result += "ModLoader active";
        }
    }
}
