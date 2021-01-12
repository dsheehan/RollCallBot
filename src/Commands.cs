namespace RollCallBot
{
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILoggingService _logger;
        private ISettings _settings;

        public Commands(MessageHandler messageHandler, ILoggingService logger, ISettings settings)
        {
            _messageHandler = messageHandler;
            _logger = logger;
            _settings = settings;
        }
        
        [Command("Roll Call", RunMode = RunMode.Async)]
        [Alias("RollCall")]
        [Summary("Start a roll call")]
        public async Task ReactAsync([Remainder] string description)
        {
            await _logger.Log(new LogMessage(LogSeverity.Info, nameof(Commands), $"New Roll Call: {description}"));
            var emoji = new Emoji("\uD83D\uDC4C");   // equivalent to "👌"
            
            await Context.Message.AddReactionAsync(emoji);
            // var message = new Message(description);
            var message = _messageHandler.AddNewMessage(description);
            await message.Send(Context.Channel);
        }
        
        [Command("Roll Call", RunMode = RunMode.Async)]
        [Alias("RollCall")]
        [Summary("Start a roll call")]
        public async Task ReactAsync2()
        {
            await ReactAsync(null);
        }
        
        [Command("Version", RunMode = RunMode.Async)]
        [Summary("Get bot version")]
        public async Task Version()
        {
            await ReplyAsync(_settings.Version);
        }
    }
}