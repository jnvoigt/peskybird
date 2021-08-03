using Discord;
using Microsoft.Extensions.Configuration;

namespace Peskybird.App.Services
{
    public class CommandHelperService : ICommandHelperService
    {
        private readonly string _activator;

        public CommandHelperService(IConfiguration configuration)
        {
            _activator = configuration["PESKY_ACTIVATOR"] ?? "!";
        }
        
        public string GetCommand(IMessage message)
        {
            var activatorLength = _activator.Length;
            return message.Content.Substring(activatorLength);
        }

    }
}