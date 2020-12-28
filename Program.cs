using System.Linq;


namespace RollCallBot
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    public class Program
    {
        // Program entry point
        public static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var program = services.GetService<Program>();
            await program.MainAsync(args);
        }

        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly MessageHandler _messageHandler;
        private readonly string DiscordToken = Environment.GetEnvironmentVariable("RollCallBotToken");
        
        public Program(IServiceProvider services, DiscordSocketClient client, CommandService commands, MessageHandler messageHandler)
        {
            _services = services;
            _client = client;
            _messageHandler = messageHandler;
            _commands = commands;
            
            // Subscribe the logging handler to both the client and the CommandService.
            _client.Log += Log;
            _commands.Log += Log;
        }
        
        private static IServiceProvider ConfigureServices()
        {
            var discordSocketConfig = new DiscordSocketConfig {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true /*otherwise IUser references won't resolve*/
            };

            var commandServiceConfig = new CommandServiceConfig {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false,
            };
            
            var map = new ServiceCollection()
                .AddLogging(opt =>
                {
                    opt.AddConsole();
                    opt.AddDebug();
                })
                .AddSingleton(discordSocketConfig)
                .AddSingleton(commandServiceConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<Program>();

            return map.BuildServiceProvider(true);
        }
        
        private static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }

        private async Task MainAsync(string[] strings)
        {
            // Centralize the logic for commands into a separate method.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Login and connect.
            await _client.LoginAsync(TokenType.Bot, DiscordToken);
            await _client.StartAsync();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);
        }
    }
}