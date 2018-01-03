using dnlib.DotNet;
using System;
using System.IO;
using System.Linq;

namespace Installer
{
    internal static class Program
    {
        private static void Main()
        {
            var managedFolder = new DirectoryInfo(Environment.CurrentDirectory);

            string tempAssemblyCsharpPath = Path.Combine(managedFolder.FullName, "Assembly-CSharp-temp.dll");
            string backupAssemblyCsharpPath = Path.Combine(managedFolder.FullName, "Assembly-CSharp-backup.dll");

            string assemblyCsharpPath = Path.Combine(managedFolder.FullName, "Assembly-CSharp.dll");
            string modLoaderPath = Path.Combine(managedFolder.FullName, "Loader.dll");

            try
            {
                if (managedFolder.Name != "Managed") throw new DirectoryNotFoundException("Installer has not been run from the managed folder");

                if (!File.Exists(assemblyCsharpPath) || !File.Exists(modLoaderPath))
                    throw new FileNotFoundException("Cannot find the right files in the current folder");

                using (var assemblyCsharp = ModuleDefMD.Load(assemblyCsharpPath))
                using (var modLoader = ModuleDefMD.Load(modLoaderPath))
                {
                    TypeDef main = modLoader.GetTypes().First(x => x.Name == "Main");

                    IMethod mainInitialize = assemblyCsharp.Import(main.FindMethod("Initialize"));
                    Utils.InsertCall(assemblyCsharp.GlobalType.FindOrCreateStaticConstructor(), 0, mainInitialize);

                    assemblyCsharp.Write(tempAssemblyCsharpPath);
                }

                File.Move(assemblyCsharpPath, backupAssemblyCsharpPath);
                File.Move(tempAssemblyCsharpPath, assemblyCsharpPath);

                Console.WriteLine("Installation was successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception has occured:");
                Console.WriteLine(ex);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempAssemblyCsharpPath)) File.Delete(tempAssemblyCsharpPath);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Unable to delete temporary file \"{tempAssemblyCsharpPath}\"");
                }
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
