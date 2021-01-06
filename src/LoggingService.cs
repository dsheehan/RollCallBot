namespace RollCallBot
{
    using System;
    using System.Threading.Tasks;
    using Discord;

    public class LoggingService
    {
        public Task Log(LogMessage message)
        {
            string shortSeverity;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    shortSeverity = "Error";
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    shortSeverity = " Warn";
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    shortSeverity = " Info";
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    shortSeverity = "Debug";
                    break;
                default: 
                    shortSeverity = $"{message.Severity,5}";
                    break;
            }

            Console.WriteLine($"{DateTime.Now:s} [{shortSeverity}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
    }
}