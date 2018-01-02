var target = Argument("target", "Default");
var deploy_dir = Argument("dir", "");

Task("Default").Does(() => 
{
    CleanDirectories("build");
    MSBuild("tld-modloader.sln", configurator => configurator.WithProperty("OutDir", "../../build"));
});

Task("Deploy").Does(() => 
{
    if (deploy_dir == "") throw new Exception("No deploy directory specified!");

    CopyFile("build/TestScripts.dll", deploy_dir + "/Mods/TestScripts.dll");
    CopyFile("build/Installer.exe", deploy_dir + "/Managed/Installer.exe");
    CopyFile("build/Loader.dll", deploy_dir + "/Managed/Loader.dll");
    CopyFile("build/0Harmony.dll", deploy_dir + "/Managed/0Harmony.dll");
    CopyFile("build/dnlib.dll", deploy_dir + "/Managed/dnlib.dll");
});

RunTarget(target);