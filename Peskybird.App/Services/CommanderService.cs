using System.Threading.Tasks;
using Autofac;
using Discord;
using Discord.WebSocket;

namespace Peskybird.App.Services
{
    // ReSharper disable once UnusedType.Global
    public class CommanderService : ICommanderService
    {
        private readonly ILifetimeScope _container;

        public CommanderService(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task Execute(string prefix, IMessage message)
        {
            await using var scope = _container.BeginLifetimeScope();
            var command = scope.ResolveOptionalNamed<ICommand>(prefix.ToLower());

            if (command != null)
            {
                await command.Execute(message);
            }
            else
            {
                var textChannel = message.Channel as SocketTextChannel;
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync($"pesky does not know what to do with \"{prefix}\"");
                }
            }
        }
    }
}