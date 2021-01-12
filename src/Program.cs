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
        /// <summary>Program entry point</summary>
        public static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var program = services.GetService<Program>();
            await program.MainAsync(args);
        }

        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ILoggingService _logger;
        private readonly ISettings _settings;

        public Program(IServiceProvider services, DiscordSocketClient client, CommandService commands, ILoggingService logger, ISettings settings)
        {
            _services = services;
            _client = client;
            _commands = commands;
            _logger = logger;
            _settings = settings;
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
                .AddSingleton<ILoggingService, LoggingService>()
                .AddSingleton<ISettings, Settings>()
                .AddSingleton<Program>();

            return map.BuildServiceProvider(true);
        }

        private async Task MainAsync(string[]  strings)
        {
            // setup the logger before doing anything else
            _client.Log += _logger.Log;
            _commands.Log += _logger.Log;
            
            await _logger.Log(new LogMessage(LogSeverity.Info, nameof(Program), $"Version: {_settings.Version}"));
            
            // Centralize the logic for commands into a separate method.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            
            // Login and connect.
            await _client.LoginAsync(TokenType.Bot, _settings.RollCallBotToken);
            await _client.StartAsync();

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite);
        }
    }
}