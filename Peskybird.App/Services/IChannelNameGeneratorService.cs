namespace Peskybird.App.Services
{
    using Discord.WebSocket;

    public interface IChannelNameGeneratorService
    {
        string GenerateName(SocketGuildChannel[] categoryVoiceChannels);
    }
}