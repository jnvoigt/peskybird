using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Peskybird.App.Commands
{
    [Command("help")]
    public class HelpCommand : ICommand
    {
        private readonly string _helpText;

        public HelpCommand(IConfiguration configuration)
        {
            var activator = configuration["PESKY_ACTIVATOR"] ?? "!";
            _helpText = HelpText(activator);
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


        public async Task Execute(IMessage message)
        {
            if (message.Channel is SocketTextChannel textChannel)
            {
                await textChannel.SendMessageAsync(_helpText);
            }
        }
    }
}