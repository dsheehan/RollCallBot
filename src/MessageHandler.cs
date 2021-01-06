namespace RollCallBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    
    public class MessageHandler
    {
        private List<Message> Messages = new List<Message>();
        private readonly LoggingService _logger;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        
        public MessageHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, LoggingService logger)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _logger = logger;
            
            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += ReactionAddedAsync;
            _client.ReactionRemoved += ReactionRemovedAsync;
            _client.MessageDeleted += MessageDeletedAsync;
        }
        
        public Message Find(ulong messageId)
        {
            var found = Messages.FirstOrDefault(x => x.userMessage?.Id == messageId);
            return found;
        }
        
        public async Task MessageDeletedAsync(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            await _logger.Log(new LogMessage(LogSeverity.Debug, nameof(MessageHandler), $"Message Deleted: {arg1.Id}"));
        }

        public async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> cacheableMessage,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _ReactionRemovedAsync(cacheableMessage, channel, reaction).ContinueWith(LogException).ConfigureAwait(false);
        }

        private async Task LogException(Task x)
        {
            if (x.Exception != null) 
                await _logger.Log(new LogMessage(LogSeverity.Error, nameof(MessageHandler), x.Exception.Message, x.Exception));
        }

        private async Task _ReactionRemovedAsync(Cacheable<IUserMessage, ulong> cacheableMessage,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _logger.Log(new LogMessage(LogSeverity.Debug, nameof(MessageHandler), $"Reaction Removed: UserId={reaction.UserId} MessageId={cacheableMessage.Id} ChannelId={channel.Id} Emote={reaction.Emote.Name}"));
            
            if (reaction.UserId == _client.CurrentUser.Id)
                return;
            var message = await cacheableMessage.GetOrDownloadAsync();
            var m = Find(cacheableMessage.Id);
            if (m == null)
            {
                await _logger.Log(new LogMessage(LogSeverity.Debug, nameof(MessageHandler), "Reaction Removed: Recreating older message"));
                m = new Message(message);
                Messages.Add(m);
            }

            m.Remove(reaction.User.Value, reaction.Emote.Name);
            await m.UpdateAsync(message);
        }

        public async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheableMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _ReactionAddedAsync(cacheableMessage, channel, reaction).ContinueWith(LogException).ConfigureAwait(false);
        }

        private async Task _ReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheableMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _logger.Log(new LogMessage(LogSeverity.Debug, nameof(MessageHandler), $"Reaction Added: UserId={reaction.UserId} MessageId={cacheableMessage.Id} ChannelId={channel.Id} Emote={reaction.Emote.Name}"));
            
            if (reaction.UserId == _client.CurrentUser.Id)
                return;

            var message = await cacheableMessage.GetOrDownloadAsync();
            if(message == null)
                return;
            
            var m = Find(cacheableMessage.Id);
            if (m == null)
            {
                await _logger.Log(new LogMessage(LogSeverity.Debug, nameof(MessageHandler), "Reaction Added: Recreating older message"));
                m = new Message(message);
                Messages.Add(m);
            }

            m.Add(reaction.User.Value, reaction.Emote.Name);
            await m.UpdateAsync(message);
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            // We don't want the bot to respond to itself or other bots.
            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;
            // Replace the '!' with whatever character
            // you want to prefix your commands with.
            // Uncomment the second half if you also want
            // commands to be invoked by mentioning the bot instead.
            if (msg.HasCharPrefix('!', ref pos) ||
                msg.Content.StartsWith("Roll Call", StringComparison.OrdinalIgnoreCase) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed successfully)
                await _logger.Log(new LogMessage(LogSeverity.Info, nameof(MessageHandler), $"Handle Command: {msg.Content}"));
                var result = await _commands.ExecuteAsync(context, pos, _services);

                // Uncomment the following lines if you want the bot
                // to send a message if it failed.
                // This does not catch errors from commands with 'RunMode.Async',
                // subscribe a handler for '_commands.CommandExecuted' to see those.
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }


        public Message AddNewMessage(string description)
        {
            var message = new Message(description);
            Messages.Add(message);
            return message;
        }
    }
}
