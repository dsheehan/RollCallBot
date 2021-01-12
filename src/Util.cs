namespace RollCallBot
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    
    public static class Util
    {
        /// <summary>Backing field for cached <see cref="Version"/></summary>
        private static string _version;
        
        /// <summary>Version of the application.</summary>
        /// Version is only read once and cached as it remains constant for the life of the process.
        /// <returns>Version of the application</returns>
        public static string Version()
        {
            return _version ??= FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "Unknown";
        }

        /// <summary>Get the RollCallBotToken.</summary>
        /// Uses the environment variable RollCallBotToken.
        /// If it's a file try and read the contents of the file (useful in conjunction with docker secret).
        /// <returns>The RollCallBotToken</returns>
        public static string RollCallBotToken()
        {
            var tokenOrFile = Environment.GetEnvironmentVariable("RollCallBotToken");
            return File.Exists(tokenOrFile) 
                ? File.ReadAllText(tokenOrFile) // it's a file path, contents are the token
                : tokenOrFile; // it's a token, directly
        }
    }
}