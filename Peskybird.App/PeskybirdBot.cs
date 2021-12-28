using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Peskybird.App.Services;

namespace Peskybird.App
{
    public class PeskybirdBot
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly string _token;
        private readonly string _activator;
        private readonly ICommanderService _commanderService;

        public PeskybirdBot(IConfiguration configuration, ILogger logger, DiscordSocketClient client,
            ICommanderService commanderService)
        {
            // await using var context = new PeskybirdContext();
            _logger = logger;
            _client = client;
            _token = configuration["PESKY_TOKEN"];

            if (_token == null)
            {
                throw new InvalidOperationException("Bot cannot be started without a token");
            }

            _activator = configuration["PESKY_ACTIVATOR"] ?? "!";
            logger.LogInformation($"activator: {_activator}");


            _commanderService = commanderService;


            _client.MessageReceived += OnMessageReceived;
            _client.UserVoiceStateUpdated += OnVoiceServerStateUpdate;
        }

        private async Task OnVoiceServerStateUpdate(SocketUser user, SocketVoiceState oldState,
            SocketVoiceState newState)
        {
            try
            {
                await _commanderService.OnVoiceServerStateUpdate(user, oldState, newState);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.Message);
                throw;
            }
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

            try
            {
                await _commanderService.Execute(command, message);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.Message);
                throw;
            }
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