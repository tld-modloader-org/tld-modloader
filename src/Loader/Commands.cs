namespace Loader
{
    public static class Commands
    {
        private static bool flyModeActive = false;
        
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
    }
}