using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Path = System.IO.Path;
using UObject = UnityEngine.Object;

namespace Loader
{
    public static class Main
    {
        public const bool DebugMode = true; // TODO: Some sort of config?
        
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

            GameEventManager.MainMenuLoaded += () =>
            {
                if (DebugMode && debugConsole == null)
                {
                    debugConsole = UObject.Instantiate(Resources.Load("uConsole"));
                    UObject.DontDestroyOnLoad(debugConsole);
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
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                    {
                        object[] attributes = method.GetCustomAttributes(typeof(CommandAttribute), false);
                        if (attributes.Length > 0 && method.ReturnType == typeof(void))
                        {
                            Type[] parameters = method.GetParameters().Types();
                            
                            CommandAttribute commandAttribute = (CommandAttribute) attributes[0];

                            if (parameters.Length == 0)
                            {
                                var methodDelegate = (uConsole.DebugCommand)Delegate.CreateDelegate(typeof(uConsole.DebugCommand), method, true);
                                if (methodDelegate == null) throw new MissingMethodException($"Cannot find the method {method.Name} in {type.Name} (Delegate.CreateDelegate returned zero)");
                                uConsole.RegisterCommand(commandAttribute.Name ?? method.Name, methodDelegate);
                            }
                            else
                            {
                                uConsole.RegisterCommand(commandAttribute.Name ?? method.Name, () =>
                                {
                                    object[] parameterValues = new object[parameters.Length];
                                
                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                        if (parameters[i] == typeof(string)) parameterValues[i] = uConsole.GetString();
                                        else if (parameters[i] == typeof(bool)) parameterValues[i] = uConsole.GetBool();
                                        else if (parameters[i] == typeof(int)) parameterValues[i] = uConsole.GetInt();
                                        else if (parameters[i] == typeof(float)) parameterValues[i] = uConsole.GetFloat();
                                        else throw new ArgumentException($"Parameter type unrecognized ({parameters[i].Name})");
                                    }
                                    
                                    method.Invoke(null, parameterValues); // TODO: Don't store the whole MethodInfo
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}
