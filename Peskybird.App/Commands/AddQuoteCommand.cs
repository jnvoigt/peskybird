using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Peskybird.App.Services;

namespace Peskybird.App.Commands
{
    [Command("addQuote")]
    public class AddQuoteCommand: ICommand
    {
        private readonly ICommandHelperService _commandHelperService;
        private readonly Regex _quoteAddRegex = new("(?i:addquote) (.*)");
        private readonly PeskybirdContext _context;

        public AddQuoteCommand(PeskybirdContext context, ICommandHelperService commandHelperService)
        {
            _commandHelperService = commandHelperService;
            _context = context;
        }

        public async Task Execute(IMessage message)
        {
            if (message.Channel is SocketTextChannel textChannel)
            {
                
                var command = _commandHelperService.GetCommand(message);

                var quoteAddMatch = _quoteAddRegex.Match(command);
                if (quoteAddMatch.Success)
                {
                    var quote = quoteAddMatch.Groups[1].Value;
                    await _context.Quotes.AddAsync(new BotQuote()
                    {
                        Quote = quote,
                        Server = textChannel.Guild.Id,
                        User = message.Author.Id,
                        Time = DateTimeOffset.Now
                    });
                    await _context.SaveChangesAsync();
                    await textChannel.SendMessageAsync($"added quote: \"{quote}\"");
                }
            }

        }
    }
}