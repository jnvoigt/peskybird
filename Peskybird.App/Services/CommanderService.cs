using System.Threading.Tasks;
using Autofac;
using Discord;
using Discord.WebSocket;

namespace Peskybird.App.Services
{
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using System;

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

        public async Task OnVoiceServerStateUpdate(SocketUser user, SocketVoiceState oldState,
            SocketVoiceState newState)
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
            await using var context = new PeskybirdContext();

            Console.WriteLine($"joined {voiceChannel.Name} on {voiceChannel.Guild.Name}");
            Console.WriteLine($"In Category => {voiceChannel.Category?.Name}");
        }

        private async Task OnChannelLeft(SocketVoiceChannel voiceChannel)
        {
            await using var context = new PeskybirdContext();

            var management = await context.ChannelConfigs.FirstOrDefaultAsync(cc =>
                cc.Server == voiceChannel.Guild.Id && cc.Category == voiceChannel.CategoryId);

            if (management != null)
            {
                // voiceChannel.
            }

            Console.WriteLine($"left {voiceChannel.Name} on {voiceChannel.Guild.Name}");
            Console.WriteLine($"In Category => {voiceChannel.Category?.Name}");
        }
    }
}