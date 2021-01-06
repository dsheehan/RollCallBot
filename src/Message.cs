namespace RollCallBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Discord;
    using Discord.WebSocket;
    
    public class Message
    {
        public  IUserMessage userMessage { get; private set; }
        private IEmbed embed;
        private struct VotingOption
        {
            public string emote;
            public string label;
            public List<IUser> users;

            public VotingOption(string emote, string label)
            {
                this.emote = emote;
                this.label = label;
                this.users = new List<IUser>();
            }

            public override string ToString()
            {
                return $"{emote} = {label} ({users.Count})";
            }
        }
        private List<VotingOption> VotingOptions = new();
        private string description;
        private SocketGuild guild;

        public Message(string description)
        {
            this.description = description ?? "Daily 9PM Roll Call";
            VotingOptions.Add(new VotingOption("✅","In"));
            VotingOptions.Add(new VotingOption("❌","Out"));
            VotingOptions.Add(new VotingOption("❓","Maybe"));
        }

        public Message(IUserMessage userMessage)
        {
            this.userMessage = userMessage;
            embed = userMessage.Embeds.First();
            description = embed.Description;
            var pattern = new Regex(@"(.*) = (.*) \((\d+)\)");
            foreach (var embedField in embed.Fields)
            {
                var match = pattern.Match(embedField.Name);
                if (match.Success)
                {
                    var votingOption = new VotingOption(match.Groups[1].Value, match.Groups[2].Value);
                    VotingOptions.Add(votingOption);
                }
            }

            var task = Task.Run(() => react(userMessage));
            task.Wait();
        }

        private async Task react(IUserMessage userMessage)
        {
            foreach (var reaction in userMessage.Reactions)
            {
                var users = userMessage.GetReactionUsersAsync(reaction.Key, 10);
                await foreach (var user in users)
                {
                    foreach (var user1 in user)
                    {
                        Add(user1, reaction.Key.Name);
                    }
                }
            }
        }

        public void Add(IUser user, string emote)
        {
            if(user.IsBot)
                return; // fast exit
            
            var users = FindCorrectUserList(emote);
            //if(users == null || users.Exists(x => x.Id == user.Id))
            //    return; // user already in list
            users.Add(user);
        }

        private List<IUser> FindCorrectUserList(string emote)
        {
            if (emote.StartsWith("au"))
            {
                return emote.EndsWith("dead") ? VotingOptions.FirstOrDefault(x => x.label == "Out").users : VotingOptions.FirstOrDefault(x => x.label == "In").users;
            }
            else
            {
                return VotingOptions.FirstOrDefault(x => x.emote == emote).users;
            }
        }

        public void Remove(IUser user, string emote)
        {
            var users = FindCorrectUserList(emote);
            users?.RemoveAll(x => x.Id == user.Id);
        }

        private string GetNickname(IUser user)
        {
            var u = guild.GetUser(user.Id);
            return u.Nickname ?? u.Username;
        }
        private EmbedFieldBuilder b(VotingOption votingOption)
        {
            var inUserList = string.Join('\n', votingOption.users.Select((d, i) => $"{i+1}. {GetNickname(d)}"));
            if (string.IsNullOrEmpty(inUserList))
                inUserList = "1. ";
            var inBuilder = new EmbedFieldBuilder().WithName($"{votingOption.emote} = {votingOption.label} ({votingOption.users.Count})").WithValue(inUserList).WithIsInline(true);
            return inBuilder;
        }
        public Embed RebuildEmbed()
        {
            var embedFieldBuilders = VotingOptions.Select(b);
            return new EmbedBuilder()
                .WithTitle($"Roll Call {userMessage?.CreatedAt.ToLocalTime()??DateTimeOffset.Now:D}")
                .WithTimestamp(DateTimeOffset.Now)
                .WithDescription(description)
                .WithFields(embedFieldBuilders)
                //.WithAuthor(new EmbedAuthorBuilder().WithName($"RollCallBot v{Util.Version()}").WithUrl("https://github.com/dsheehan/RollCallBot").WithIconUrl("https://cdn.discordapp.com/embed/avatars/0.png"))
                .Build();
        }

        public async Task Send(ISocketMessageChannel socketMessageChannel)
        {
            guild = (socketMessageChannel as SocketGuildChannel).Guild;
            embed = RebuildEmbed();
            var msg = await socketMessageChannel.SendMessageAsync(null, false, embed as Embed);
            var emojis = VotingOptions.Select(x => new Emoji(x.emote) as IEmote);
            await msg.AddReactionsAsync(emojis.ToArray());
            userMessage = msg;
        }

        public async Task UpdateAsync(IUserMessage message)
        {
            guild = (message.Channel as SocketGuildChannel).Guild;
            embed = RebuildEmbed();
            await message.ModifyAsync(x => x.Embed = embed as Embed);
        }

        
    }
}