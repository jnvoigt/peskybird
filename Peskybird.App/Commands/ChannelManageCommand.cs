namespace Peskybird.App.Commands
{
    using Contract;
    using Discord;
    using Discord.WebSocket;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Services;
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [Command("channel")]
    // ReSharper disable once UnusedType.Global
    public class ChannelManageCommand : ICommand, IVoiceStateUpdate
    {
        private readonly PeskybirdContext _context;
        private readonly ICommandHelperService _commandHelperService;
        private readonly IChannelNameGeneratorService _channelNameGeneratorService;
        private readonly ILogger _logger;
        private readonly Regex _channelModifyRegex = new("(?i:channel) (add|remove) (.*)");

        public ChannelManageCommand(PeskybirdContext context, ICommandHelperService commandHelperService, IChannelNameGeneratorService channelNameGeneratorService, ILogger logger)
        {
            _context = context;
            _commandHelperService = commandHelperService;
            _channelNameGeneratorService = channelNameGeneratorService;
            _logger = logger;
        }

        public async Task Execute(IMessage message)
        {
            var textChannel = message.Channel as SocketTextChannel;
            var user = message.Author as SocketUser;
            if (textChannel == null)
            {
                return;
            }

            if (message.AuthorIsAdmin())
            {
                var command = _commandHelperService.GetCommand(message);

                var match = _channelModifyRegex.Match(command);
                if (match.Success)
                {
                    var categoryName = match.Groups[2].Value;
                    var category = textChannel.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name == categoryName);

                    if (category == null)
                    {
                        await textChannel.SendMessageAsync($"Group \"{categoryName}\" does not exist");
                    }

                    var existingConfig = _context.ChannelConfigs.FirstOrDefault(cc => cc.Category == category.Id);

                    var operation = match.Groups[1].Value;
                    if (operation == "add")
                    {
                        if (existingConfig != null)
                        {
                            await textChannel.SendMessageAsync($"Group \"{categoryName}\" already managed");
                            return;
                        }

                        await _context.ChannelConfigs.AddAsync(new ChannelConfig()
                        {
                            Server = textChannel.Guild.Id,
                            Category = category.Id,
                        });
                        await _context.SaveChangesAsync();
                        await textChannel.SendMessageAsync($"Setup group \"{categoryName}\" for management");
                        return;
                    }

                    if (operation == "remove")
                    {
                        if (existingConfig != null)
                        {
                            _context.ChannelConfigs.Remove(existingConfig);
                            await _context.SaveChangesAsync();
                            await textChannel.SendMessageAsync($"Group \"{categoryName}\" removed from management");
                        }

                        return;
                    }

                    return;
                }

                await textChannel.SendMessageAsync("specisify what to do?");
            }
            else
            {
                await textChannel.SendMessageAsync("No u dont");
            }
        }

        public async Task OnVoiceServerStateUpdate(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState) {
            
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
            var management = await GetChannelManagement(voiceChannel);

            if (management != null)
            {
                // check if there still are empty channels in category
                var categoryVoiceChannels = voiceChannel.Guild.Channels
                    .Where(channel => channel is SocketVoiceChannel vc && vc.CategoryId == voiceChannel.CategoryId).ToArray();
                var emptyVoiceChannelCount = categoryVoiceChannels
                    .Select(channel => channel as SocketVoiceChannel).Count(socketVoiceChannel => socketVoiceChannel?.Users?.Count == 0);

                if (emptyVoiceChannelCount == 0)
                {
                    string channelName = _channelNameGeneratorService.GenerateName(categoryVoiceChannels);
                    
                    _logger.Log(LogLevel.Information,$"create channel {voiceChannel.Name} in {voiceChannel.Guild.Name}/{voiceChannel.Category?.Name}");

                    await voiceChannel.Guild.CreateVoiceChannelAsync(channelName, properties =>
                    {
                        properties.CategoryId = management.Category;
                    });
                }

                _logger.Log(LogLevel.Information, $"joined {voiceChannel.Name} on {voiceChannel.Guild.Name}");
                _logger.Log(LogLevel.Information,$"In Category => {voiceChannel.Category?.Name}");
            }
        }

        private async Task OnChannelLeft(SocketVoiceChannel voiceChannel)
        {

            var management = await GetChannelManagement(voiceChannel);

            if (management != null)
            {
                // check if there still are empty channels in category
                var categoryVoicChannels = voiceChannel.Guild.Channels
                    .Where(channel => channel is SocketVoiceChannel vc && vc.CategoryId == voiceChannel.CategoryId);
                var emptyVoiceChannelCount = categoryVoicChannels
                    .Select(channel => channel as SocketVoiceChannel).Count(socketVoiceChannel => socketVoiceChannel?.Users?.Count == 0);

                if (emptyVoiceChannelCount > 1)
                {
                    _logger.Log(LogLevel.Information,$"delete channel {voiceChannel.Name} in {voiceChannel.Guild.Name}/{voiceChannel.Category?.Name}");
                    await voiceChannel.DeleteAsync();
                }
                
                
                
                // voiceChannel.
                _logger.Log(LogLevel.Information,$"left {voiceChannel.Name} on {voiceChannel.Guild.Name}");
                _logger.Log(LogLevel.Information,$"In Category => {voiceChannel.Category?.Name}");
            }
        }

        private async Task<ChannelConfig?> GetChannelManagement(SocketVoiceChannel voiceChannel)
        {
            return await _context.ChannelConfigs.FirstOrDefaultAsync(cc =>
                cc.Server == voiceChannel.Guild.Id && cc.Category == voiceChannel.CategoryId);
        }
    }
}