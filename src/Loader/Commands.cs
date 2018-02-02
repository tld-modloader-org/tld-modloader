using System.Linq;
using UnityEngine;

namespace Loader
{
    public static class Commands
    {
        private static bool flyModeActive = false;

        [Command("mods")]
        public static void DisplayMods()
        {
            foreach (var mod in Main.Mods)
            {
                Debug.Log($"{mod.Name}: {mod.Scripts.Length} scripts ({mod.Scripts.Select(x => x.GetType().Name).Aggregate((a, b) => $"{a}, {b}")})");
            }
        }
        
        [Command("fly")]
        public static void Fly()
        {
            if (!flyModeActive)
            {
                FlyMode.Enter();
                flyModeActive = true;
            }
            else
            {
                FlyMode.TeleportPlayerAndExit();
                flyModeActive = false;
            }
        }

        [Command("warning")]
        public static void DisplayWarning(string warning)
        {
            InterfaceManager.m_Panel_HUD.DisplayWarningMessage(warning);
        }
    }
}