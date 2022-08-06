namespace Peskybird.App.Contract;

using Discord;
using System.Threading.Tasks;

public interface IMessageHandler
{
    Task Execute(IMessage message);
}