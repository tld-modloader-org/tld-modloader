using Harmony;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Path = System.IO.Path;
using UObject = UnityEngine.Object;

namespace Loader
{
    public static class Main
    {
        public static string ModFolder;

        public static List<Mod> Mods = new List<Mod>();

        private static readonly object initializeSyncRoot = new object();
        private static volatile bool initialized = false;

        private static bool worldHasLoaded = false;

        private static UObject debugConsole;

        public static void Initialize()
        {
            if (!initialized)
                lock (initializeSyncRoot)
                    if (!initialized)
                    {
                        initialized = true;
                        ModFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "Mods"));
                        if (!Directory.Exists(ModFolder)) Directory.CreateDirectory(ModFolder);

                        Mods = LoadMods().ToList();
                        Mods.TrimExcess();
                        
                        var currentInstance = HarmonyInstance.Create("ModLoader");
                        currentInstance.PatchAll(Assembly.GetExecutingAssembly());
                        
                        RemainingEdits();
                        
                        Mod.RegisterCommands(Assembly.GetExecutingAssembly());
                    }
        }

        private static IEnumerable<Mod> LoadMods()
        {
            foreach (string path in Directory.GetFiles(ModFolder, "*.dll", SearchOption.AllDirectories))
            {
                var mod = new Mod(Path.GetFileNameWithoutExtension(path));
                
                mod.Load(path);
                mod.PatchHarmony();
                mod.RegisterCommands();
                mod.StartScripts();
                
                yield return mod;
            }
        }

        private static void RemainingEdits()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                string name = scene.name.ToLower();
                if (name != "boot" && name != "empty" && name != "mainmenu" && !name.Contains("cinematic"))
                {
                    if (!worldHasLoaded)
                    {
                        worldHasLoaded = true;
                        GameEventManager.OnWorldLoaded(scene);
                    }
                }
                else
                {
                    worldHasLoaded = false;
                }
            };

            GameEventManager.MainMenuLoaded += () =>
            {
                if (debugConsole == null)
                {
                    debugConsole = UObject.Instantiate(Resources.Load("uConsole"));
                    UObject.DontDestroyOnLoad(debugConsole);
                }
            };
        }
    }
}
