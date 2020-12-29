using System;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace RollCallBot
{
    // Create a module with no prefix
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly MessageHandler _messageHandler;
        private readonly ILogger<Commands> _logger;

        public Commands(MessageHandler messageHandler, ILogger<Commands> logger)
        {
            _messageHandler = messageHandler;
            _logger = logger;
        }
        
        [Command("Roll Call", RunMode = RunMode.Async)]
        [Alias("RollCall")]
        [Summary("Start a roll call")]
        public async Task ReactAsync([Remainder] string description)
        {
            _logger.LogInformation($"New Roll Call: {description}");
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
            _logger.LogInformation($"New Roll Call");
            var emoji = new Emoji("\uD83D\uDC4C");   // equivalent to "👌"
            
            await Context.Message.AddReactionAsync(emoji);
            // var message = new Message(description);
            var message = _messageHandler.AddNewMessage(null);
            await message.Send(Context.Channel);
        }
    }
}