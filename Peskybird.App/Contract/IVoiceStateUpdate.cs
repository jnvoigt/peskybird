using Discord.WebSocket;
using System.Threading.Tasks;

namespace Peskybird.App.Contract
{

    public interface IVoiceStateUpdate
    {
        Task OnVoiceServerStateUpdate(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState);
    }
}