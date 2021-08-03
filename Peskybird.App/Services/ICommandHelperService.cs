using Discord;

namespace Peskybird.App.Services
{
    public interface ICommandHelperService
    {
        string GetCommand(IMessage message);
    }
}