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
    using Autofac;
    using Autofac.Core;
    using Autofac.Core.Lifetime;
    using Contract;
    using System.Collections.Generic;

    public class PeskybirdBot
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly string _token;
        private readonly string _activator;
        private readonly ICommanderService _commanderService;
        private readonly IEnumerable<IMessageHandler> _messageHandlers;

        public PeskybirdBot(IConfiguration configuration, ILogger logger, DiscordSocketClient client,
            ICommanderService commanderService, IEnumerable<IMessageHandler> messageHandlers)
        {
            // await using var context = new PeskybirdContext();
            _logger = logger;
            _client = client;
            _token = configuration["PESKY_TOKEN"];
            // _container = container;

            if (_token == null)
            {
                throw new InvalidOperationException("Bot cannot be started without a token");
            }

            _activator = configuration["PESKY_ACTIVATOR"] ?? "!";
            logger.LogInformation($"activator: {_activator}");


            _commanderService = commanderService;
            _messageHandlers = messageHandlers;

            _client.Log += message =>
            {

                if (message.Severity == LogSeverity.Error)
                {
                    _logger.LogError(message.Exception.Message + message.Exception.StackTrace);
                } 
                
                if (message.Severity == LogSeverity.Info)
                {
                    _logger.LogInformation(message.Message);
                } 
                
                if (message.Severity == LogSeverity.Warning)
                {
                    _logger.LogWarning(message.Message);
                } 
                
                if (message.Severity == LogSeverity.Critical)
                {
                    _logger.LogCritical(message.Message);
                } 
                return Task.CompletedTask;
            };
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
            if (messageContent == null)
            {
                return;
            }
            
            if (messageContent.StartsWith(_activator))
            {
                var command = GetCommandKey(messageContent);
                try
                {
                    await _commanderService.Execute(command, message);
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e.ToString());
                }
            }
            
            try
            {
                foreach (var handler in _messageHandlers)
                {
                    await handler.Execute(message);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e.Message);
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
            _logger.Log(LogLevel.Information, "Start Bot");
            await _client.LoginAsync(TokenType.Bot, _token);
            _logger.Log(LogLevel.Information, "Started Bot");
            await _client.StartAsync();
        }
    }
}