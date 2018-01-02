using Harmony;
using System;

namespace Loader
{
    [HarmonyPatch(typeof(GameManager), "GetVersionString")]
    internal static class VersionDisplay
    {
        private static void Postfix(ref string __result)
        {
            __result += Environment.NewLine;
            __result += "ModLoader active";
        }
    }
}
