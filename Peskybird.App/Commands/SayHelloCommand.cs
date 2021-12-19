using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Peskybird.App.Commands
{
    using Contract;

    [Command("sayhello")]
    // ReSharper disable once UnusedType.Global
    public class SayHelloCommand : ICommand
    {
        public async Task Execute(IMessage message)
        {
            if (message.Channel is SocketTextChannel textChannel)
            {
                await textChannel.SendMessageAsync($"Hello {message.Author.Username}, Pesky wants a Cookie");
            }
        }
    }
}