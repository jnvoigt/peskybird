using System.Linq;
using Discord;
using Discord.WebSocket;

namespace Peskybird.App
{
    public static class MessageExtension
    {
        public static bool AuthorIsAdmin(this IMessage message)
        {
            var textChannel = message.Channel as SocketTextChannel;

            if (textChannel == null)
            {
                return false;
            }

            var guild = textChannel?.Guild;

            if (guild == null)
            {
                return false;
            }

            var authorId = message.Author.Id;
            var guildUser = message.Author as SocketGuildUser;

            if (guild.OwnerId == authorId)
            {
                return true;
            }

            return guildUser != null && guildUser.Roles.Any(r => r.Permissions.Administrator);
        }
    }
}