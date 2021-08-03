using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Peskybird.App.Services;

namespace Peskybird.App.Commands
{
    [Command("channel")]
    public class ChannelManageCommand : ICommand
    {
        private readonly PeskybirdContext _context;
        private readonly ICommandHelperService _commandHelperService;
        private readonly Regex _channelModifyRegex = new("(?i:channel) (add|remove) (.*)");

        public ChannelManageCommand(PeskybirdContext context, ICommandHelperService commandHelperService)
        {
            _context = context;
            _commandHelperService = commandHelperService;
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
    }
}