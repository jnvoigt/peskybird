using System.Threading.Tasks;
using Discord;

namespace Peskybird.App.Services
{
    public interface ICommanderService
    {
        Task Execute(string prefix, IMessage message);
    }
}