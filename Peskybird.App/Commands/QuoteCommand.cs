using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Peskybird.App.Commands
{
    using Contract;
    using Model;

    [Command("quote")]
    // ReSharper disable once UnusedType.Global
    public class QuoteCommand : ICommand
    {
        private readonly QuoteRepository _quoteRepository;

        public QuoteCommand(QuoteRepository quoteRepository)
        {
            _quoteRepository = quoteRepository;
        }

        public async Task Execute(IMessage message)
        {
            if (message.Channel is SocketTextChannel textChannel)
            {
                var quote = await _quoteRepository.GetRandomQuote(textChannel.Guild.Id);
              
                if (quote != null)
                {
                    await textChannel.SendMessageAsync(quote.Quote);
                }
                else
                {
                    await textChannel.SendMessageAsync("There is nothing to quote");
                }
            }
        }
    }
}