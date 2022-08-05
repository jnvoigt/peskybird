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
        private readonly PeskybirdContext _context;

        public QuoteCommand(PeskybirdContext context)
        {
            _context = context;
        }

        public async Task Execute(IMessage message)
        {
            if (message.Channel is SocketTextChannel textChannel)
            {
                var r = new Random();
                var quotes = _context.Quotes.AsQueryable().Where(q => q.Server == textChannel.Guild.Id).ToArray();

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
    }
}