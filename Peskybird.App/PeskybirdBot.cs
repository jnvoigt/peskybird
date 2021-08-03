using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Peskybird.App
{
    public class PeskybirdBot
    {
        private readonly DiscordSocketClient _client;
        private readonly string _token;
        private readonly string _activator;
        private readonly Commander _commander;

        public PeskybirdBot(IConfiguration configuration, ILogger logger, DiscordSocketClient client,
            Commander commander)
        {
            // await using var context = new PeskybirdContext();
            _client = client;
            _token = configuration["PESKY_TOKEN"];

            if (_token == null)
            {
                throw new InvalidOperationException("Bot cannot be started without a token");
            }

            _activator = configuration["PESKY_ACTIVATOR"] ?? "!";
            logger.LogInformation($"activator: {_activator}");


            _commander = commander;


            _client.MessageReceived += OnMessageReceived;
            _client.UserVoiceStateUpdated += OnVoiceServerStateUpdate;
        }

        private async Task OnVoiceServerStateUpdate(SocketUser user, SocketVoiceState oldState,
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
            if (messageContent == null || !messageContent.StartsWith(_activator))
            {
                return;
            }

            var command = GetCommandKey(messageContent);

            await _commander.Execute(command, message);
        }

        private string GetCommandKey(string messageContent)
        {
            var activatorLength = _activator.Length;
            var withoutActivator = messageContent.Substring(activatorLength);
            var splitter = withoutActivator.IndexOf(' ');
            if (splitter == -1)
            {
                return withoutActivator;
            }

            var command = withoutActivator.Substring(0, splitter);
            return command;
        }

        public async Task RunBot()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }
    }
}