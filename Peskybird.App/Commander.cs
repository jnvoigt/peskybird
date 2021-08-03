using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Peskybird.App
{
    public class Commander
    {
        private readonly ILifetimeScope _container;

        public Commander(ILifetimeScope container)
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