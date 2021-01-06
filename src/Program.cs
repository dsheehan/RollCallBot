namespace RollCallBot
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
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
        private readonly LoggingService _logger;
        private readonly string DiscordToken = Environment.GetEnvironmentVariable("RollCallBotToken");
        
        public Program(IServiceProvider services, DiscordSocketClient client, CommandService commands, LoggingService logger)
        {
            _services = services;
            _client = client;
            _commands = commands;
            _logger = logger;
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
                .AddSingleton(discordSocketConfig)
                .AddSingleton(commandServiceConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<MessageHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton<Program>();

            return map.BuildServiceProvider(true);
        }

        private async Task MainAsync(string[]  strings)
        {
            // setup the logger before doing anything else
            _client.Log += _logger.Log;
            _commands.Log += _logger.Log;
            
            await _logger.Log(new LogMessage(LogSeverity.Info, nameof(Program), $"Version: {Util.Version()}"));
            
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