using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Loader
{
    public static class Main
    {
        public static string ModFolder;

        private static readonly object initializeSyncRoot = new object();
        private static volatile bool initialized = false;

        private static GameObject scriptHost;
        private static List<Assembly> assemblies;

        private static readonly string nonAutisticDataPath = Application.dataPath.Replace(@"/", @"\");

        public static void Initialize()
        {
            if (!initialized)
                lock (initializeSyncRoot)
                    if (!initialized)
                    {
                        initialized = true;
                        ModFolder = Path.Combine(nonAutisticDataPath, "Mods");
                        if (!Directory.Exists(ModFolder)) Directory.CreateDirectory(ModFolder);

                        scriptHost = new GameObject();
                        UObject.DontDestroyOnLoad(scriptHost);

                        assemblies = new List<Assembly>();

                        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
                        {
                            if (scene.name != "boot" && scene.name != "mainmenu" && scene.name != "empty")
                            {
                                GameEventManager.WorldLoadedTrigger();
                            }
                        };

                        LoadMods();
                        PatchHarmony();
                    }
        }

        public static void LoadMods()
        {
            foreach (string script in Directory.GetFiles(ModFolder, "*.dll", SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFrom(script);
                assemblies.Add(assembly);

                foreach (var type in assembly.GetTypes())
                {
                    Debug.Log($"Looking at type {type.FullName}");
                    if (type.IsSubclassOf(typeof(MonoBehaviour)) &&
                        Attribute.IsDefined(type, typeof(ScriptAttribute)))
                    {
                        Debug.Log("Registering type");
                        scriptHost.AddComponent(type);
                    }
                }
            }
        }

        public static void PatchHarmony()
        {
            var currentInstance = HarmonyInstance.Create("ModLoader");
            currentInstance.PatchAll(Assembly.GetExecutingAssembly());

            foreach (var assembly in assemblies)
            {
                var instance = HarmonyInstance.Create(assembly.FullName);
                instance.PatchAll(assembly);
            }
        }
    }
}
