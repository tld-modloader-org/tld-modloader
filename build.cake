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

    CopyFile("build/Installer.exe", deploy_dir + "/Installer.exe");
    CopyFile("build/Loader.dll", deploy_dir + "/Loader.dll");
    CopyFile("build/0Harmony.dll", deploy_dir + "/0Harmony.dll");
    CopyFile("build/dnlib.dll", deploy_dir + "/dnlib.dll");
});

RunTarget(target);