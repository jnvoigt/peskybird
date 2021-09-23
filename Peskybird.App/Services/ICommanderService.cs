using System.Threading.Tasks;
using Discord;

namespace Peskybird.App.Services
{
    using Discord.WebSocket;

    public interface ICommanderService
    {
        Task Execute(string prefix, IMessage message);

        Task OnVoiceServerStateUpdate(SocketUser user, SocketVoiceState oldState,
            SocketVoiceState newState);
    }
}