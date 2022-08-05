using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Peskybird.App.Services;

namespace Peskybird.App.Commands
{
    using Contract;
    using Model;

    [Command("addQuote")]
    // ReSharper disable once UnusedType.Global
    public class AddQuoteCommand : ICommand
    {
        private readonly QuoteRepository _quoteRepository;
        private readonly ICommandHelperService _commandHelperService;
        private readonly Regex _quoteAddRegex = new("(?i:addquote) (.*)");

        public AddQuoteCommand(QuoteRepository quoteRepository, ICommandHelperService commandHelperService)
        {
            _quoteRepository = quoteRepository;
            _commandHelperService = commandHelperService;
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
                    await _quoteRepository.AddQuote(new BotQuote() {Quote = quote, Server = textChannel.Guild.Id, User = message.Author.Id, Time = DateTimeOffset.Now});

                    await textChannel.SendMessageAsync($"added quote: \"{quote}\"");
                }
            }
        }
    }
}