namespace RollCallBot
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    
    public interface ISettings
    {
        /// <summary>Version of the application.</summary>
        public string Version { get; }
        
        /// <summary>Discord bot token RollCallBotToken.</summary>
        public string RollCallBotToken { get; }
    }

    /// <summary>Default implementation of <see cref="ISettings"/></summary>
    public class Settings : ISettings
    {
        public string Version { get; }
        public string RollCallBotToken { get; }
        
        public Settings()
        {
            // default values
            Version = _Version();
            RollCallBotToken = _RollCallBotToken();
        }
        
        /// <summary>Default implementation of <see cref="Version"/></summary>
        /// <returns>Product Version of the executing assembly</returns>
        private string _Version()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "Unknown";
        }

        /// <summary>Default implementation of <see cref="RollCallBotToken"/></summary>
        /// Uses the environment variable RollCallBotToken.
        /// If it's a file try and read the contents of the file (useful in conjunction with docker secret).
        /// <returns>The RollCallBotToken</returns>
        private string _RollCallBotToken()
        {
            var tokenOrFile = Environment.GetEnvironmentVariable("RollCallBotToken");
            return File.Exists(tokenOrFile) 
                ? File.ReadAllText(tokenOrFile) // it's a file path, contents are the token
                : tokenOrFile; // it's a token, directly
        }
    }
}