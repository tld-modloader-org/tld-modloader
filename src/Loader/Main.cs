using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UTJ;
using UObject = UnityEngine.Object;

namespace Loader
{
    public static class Main
    {
        public const bool DebugMode = true;
        
        public static string ModFolder;

        private static readonly object initializeSyncRoot = new object();
        private static volatile bool initialized = false;

        private static GameObject scriptHost;
        private static List<Assembly> assemblies;

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

                        scriptHost = new GameObject();
                        UObject.DontDestroyOnLoad(scriptHost);

                        assemblies = new List<Assembly>();

                        // TODO: Load this somewhere else
                        // debugConsole = UObject.Instantiate(Resources.Load("uConsole"));
                        
                        LoadMods();
                        PatchHarmony();
                        RemainingEdits();
                        RegisterCommands();
                    }
        }

        private static void LoadMods()
        {
            foreach (string script in Directory.GetFiles(ModFolder, "*.dll", SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFrom(script);
                assemblies.Add(assembly);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(MonoBehaviour)) &&
                        Attribute.IsDefined(type, typeof(ScriptAttribute)))
                    {
                        scriptHost.AddComponent(type);
                    }
                }
            }
        }

        private static void PatchHarmony()
        {
            var currentInstance = HarmonyInstance.Create("ModLoader");
            currentInstance.PatchAll(Assembly.GetExecutingAssembly());

            assemblies.ForEach(assembly => HarmonyInstance.Create(assembly.FullName).PatchAll(assembly));
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
        }

        private static void RegisterCommands()
        {
            RegisterCommands(Assembly.GetExecutingAssembly());
            assemblies.ForEach(RegisterCommands);
        }

        private static void RegisterCommands(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract && type.IsSealed) // static
                {
                    foreach (var method in Enumerate(
                            type.GetMethods(BindingFlags.Public | BindingFlags.Static),
                            type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))) // TODO: This probably isn't the best way to do this
                    {
                        object[] attributes = method.GetCustomAttributes(typeof(CommandAttribute), false);
                        if (attributes.Length > 0)
                        {
                            CommandAttribute commandAttribute = (CommandAttribute) attributes[0];
                            uConsole.RegisterCommand(commandAttribute.Name ?? method.Name,
                                () => method.Invoke(null, null));
                        }
                    }
                }
            }
        }

        private static IEnumerable<T> Enumerate<T>(params T[][] arrays)
        {
            for (int x = 0; x < arrays.Length; x++)
            {
                for (int y = 0; y < arrays[x].Length; y++)
                {
                    yield return arrays[x][y];
                }
            }
        }
    }
}
