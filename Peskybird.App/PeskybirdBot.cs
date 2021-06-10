using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Peskybird.App
{
    public class PeskybirdBot
    {
        private readonly Regex _quoteAddRegex = new("(?i:addquote) (.*)");

        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly string _token;
        private readonly string _activator;
        private readonly string _helpText;
        private readonly Commander _commander;

        public PeskybirdBot(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            // await using var context = new PeskybirdContext();
            _client = new DiscordSocketClient();
            _token = configuration["PESKY_TOKEN"];

            if (_token == null)
            {
                throw new InvalidOperationException("Bot cannot be started without a token");
            }

            _activator = configuration["PESKY_ACTIVATOR"] ?? "!";
            logger.LogInformation($"activator: {_activator}");

            _helpText = HelpText(_activator);
            
            _commander = new Commander();
            _commander.Add("help", async message =>
            {
                var textChannel = message.Channel as SocketTextChannel;
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync(_helpText);
                }
            });          
            
            _commander.Add("sayhello", async message =>
            {
                var textChannel = message.Channel as SocketTextChannel;
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync($"Hello {message.Author.Username}, Pesky wants a Cookie");
                }
            });
            
            _commander.Add("addquote", AddQuote);
            _commander.Add("quote", Quote);
            
            _client.MessageReceived += OnMessageReceived;
            _client.UserVoiceStateUpdated += OnVoiceServerStateUpdate;
        }

        private async Task Quote(IMessage message)
        {
            var textChannel = message.Channel as SocketTextChannel;
            if (textChannel != null)
            {
                await using var context = new PeskybirdContext();
                var r = new Random();
                var quotes = context.Quotes.AsQueryable().Where(q => q.Server == textChannel.Guild.Id).ToArray();

                if (quotes.Length > 0)
                {
                    var quote = quotes[r.Next(quotes.Length)];
                    await textChannel.SendMessageAsync(quote.Quote);
                }
                else
                {
                    await textChannel.SendMessageAsync("There is nothing to quote");
                }
            }

        }

        private async Task AddQuote(IMessage message)
        {
            var textChannel = message.Channel as SocketTextChannel;
            if (textChannel != null)
            {
                
                var activatorLength = _activator.Length;
                var command = message.Content.Substring(activatorLength);
               
                var quoteAddMatch = _quoteAddRegex.Match(command);
                if (quoteAddMatch.Success)
                {
                    await using var context = new PeskybirdContext();
                    var quote = quoteAddMatch.Groups[1].Value;
                    await context.Quotes.AddAsync(new BotQuote()
                    {
                        Quote = quote,
                        Server = textChannel.Guild.Id,
                        User = message.Author.Id,
                        Time = DateTimeOffset.Now
                    });
                    await context.SaveChangesAsync();
                    await textChannel.SendMessageAsync($"added quote: \"{quote}\"");
                }
            }
        }

        private async Task OnVoiceServerStateUpdate(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var oldChannelId = oldState.VoiceChannel?.Id;
            var newChannelId = newState.VoiceChannel?.Id;

            if (oldChannelId == newChannelId)
            {
                return;
            }

            if (oldChannelId.HasValue)
            {
                await OnChannelLeft(oldState.VoiceChannel);
            }
            
            if (newChannelId.HasValue)
            {
                await OnChannelJoin(newState.VoiceChannel);
            }
        }

        private async Task OnChannelJoin(SocketVoiceChannel voiceChannel)
        {
            Console.WriteLine($"joined {voiceChannel.Name} on {voiceChannel.Guild.Name}");
            Console.WriteLine($"In Category => {voiceChannel.Category?.Name}");
            
        }

        private async Task OnChannelLeft(SocketVoiceChannel voiceChannel)
        {
            Console.WriteLine($"left {voiceChannel.Name} on {voiceChannel.Guild.Name}");
            Console.WriteLine($"In Category => {voiceChannel.Category?.Name}");
        }

        private string HelpText(string activator)
        {
            return @$"```
all commands are preceeded with {activator} as command activator
sayhello
    - checks if pesky is still alive
quote
    - lists a random quote of the current server
addquote <content>
    - adds <content> to the quote list of this server !!under construction!!
```";
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            var textChannel = message.Channel as SocketTextChannel;
            if (textChannel == null)
            {
                return;
            }

            var messageContent = message.Content;
            if (messageContent == null || !messageContent.StartsWith(_activator))
            {
                return;
            }

            var command = GetCommand(messageContent);

            await _commander.Execute(command, message);
        }

        private string GetCommand(string messageContent)
        {
            var activatorLength = _activator.Length;
            var withoutActivator = messageContent.Substring(activatorLength);
            var splitter = withoutActivator.IndexOf(' ');
            if (splitter == -1)
            {
                return withoutActivator;
            }
            var command = withoutActivator.Substring(0, splitter);
            return command;
        }

        public async Task RunBot()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }
    }
    
    
}