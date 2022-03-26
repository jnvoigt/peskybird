using Discord.WebSocket;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Peskybird.App.Services
{
    public class ChannelNameGeneratorService : IChannelNameGeneratorService
    {
        private readonly Regex _voiceNameRegex = new("Voice (\\d+)");

        public string GenerateName(SocketGuildChannel[] categoryVoiceChannels)
        {
            var numbers = categoryVoiceChannels
                .Select(channel => _voiceNameRegex.Match(channel.Name))
                .Where(match => match.Success)
                .Select(match => Convert.ToInt32(match.Groups[1].Value))
                .OrderBy(n => n);
            
            var track = 1;
            foreach (var number in numbers)
            {
                if (number != track)
                {
                    break;
                }

                track++;
            }

            return $"Voice {track}";
        }
    }
}