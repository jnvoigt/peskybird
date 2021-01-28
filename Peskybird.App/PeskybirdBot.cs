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
        private Regex _quoteAddRegex = new("(?i:addquote) (.*)");

        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly string _token;
        private readonly string _activator;
        private readonly string _helpText;

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

            _client.MessageReceived += OnMessageReceived;
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
            if (!messageContent.StartsWith(_activator))
            {
                return;
            }

            var activatorLength = _activator.Length;
            var command = messageContent.Substring(activatorLength);
            var lowerCommand = command.ToLower();

            if (lowerCommand == "help")
            {
                await textChannel.SendMessageAsync(_helpText);
                return;
            }
            
            if (lowerCommand == "sayhello")
            {
                await textChannel.SendMessageAsync($"Hello {message.Author.Username}, Pesky wants a Cookie");
                return;
            }
            
            if (lowerCommand == "quote")
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

                return;
            }

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
            else
            {
                await textChannel.SendMessageAsync($"pesky does not know what to do with \"{command}\"");
            }
        }

        public async Task RunBot()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }
    }
}