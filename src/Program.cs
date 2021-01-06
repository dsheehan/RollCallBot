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
        private readonly MessageHandler _messageHandler;
        private readonly string DiscordToken = Environment.GetEnvironmentVariable("RollCallBotToken");
        
        public Program(IServiceProvider services, DiscordSocketClient client, CommandService commands, MessageHandler messageHandler)
        {
            _services = services;
            _client = client;
            _messageHandler = messageHandler;
            _commands = commands;
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