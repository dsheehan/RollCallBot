namespace RollCallBot
{
    using System.Diagnostics;
    using System.Reflection;
    
    public static class Util
    {
        /// <summary>Backing field for cached <see cref="Version"/></summary>
        private static string _version;
        
        /// <returns>Version of the application</returns>
        public static string Version()
        {
            return _version ??= FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "Unknown";
        }
    }
}