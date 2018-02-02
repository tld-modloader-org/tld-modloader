using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Loader
{
    public class Mod
    {
        public string Name { get; }
        public Assembly Assembly { get; private set; }
        public Component[] Scripts { get; private set; }
        public HarmonyInstance Harmony { get; private set; }

        private GameObject scriptHost;
        
        public Mod(string name)
        {
            Name = name;
        }

        public void Load(string path)
        {
            Assembly = Assembly.LoadFrom(path);
            
            scriptHost = new GameObject($"{Name}__script_host");
            UObject.DontDestroyOnLoad(scriptHost);
        }

        public void StartScripts()
        {
            var scripts = new List<Component>();
            
            foreach (var type in Assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(MonoBehaviour)) &&
                    Attribute.IsDefined(type, typeof(ScriptAttribute)))
                {
                    Component comp = scriptHost.AddComponent(type);
                    scripts.Add(comp);
                }
            }

            Scripts = scripts.ToArray();
        }

        public void PatchHarmony()
        {
            Harmony = HarmonyInstance.Create(Name);
            Harmony.PatchAll(Assembly);
        }

        public void RegisterCommands() => RegisterCommands(Assembly);

        public static void RegisterCommands(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract && type.IsSealed) // static
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                    {
                        object[] attributes = method.GetCustomAttributes(typeof(CommandAttribute), false);
                        if (attributes.Length == 1 && method.ReturnType == typeof(void))
                        {
                            Type[] parameters = method.GetParameters().Types();
                            
                            var commandAttribute = (CommandAttribute)attributes[0];

                            if (parameters.Length == 0)
                            {
                                var methodDelegate = (uConsole.DebugCommand)Delegate.CreateDelegate(typeof(uConsole.DebugCommand), method, true);
                                if (methodDelegate == null) throw new MissingMethodException($"Cannot find the method {method.Name} in {type.Name} (Delegate.CreateDelegate returned null)");
                                uConsole.RegisterCommand(commandAttribute.Name ?? method.Name, methodDelegate);
                            }
                            else
                            {
                                uConsole.RegisterCommand(commandAttribute.Name ?? method.Name, () =>
                                {
                                    var parameterValues = new object[parameters.Length];
                                
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
        
        public override string ToString() => Name;
        public override int GetHashCode() => Name.GetHashCode();
        public override bool Equals(object obj) => obj is Mod mod ? Name.Equals(mod.Name) : base.Equals(obj);
    }
}