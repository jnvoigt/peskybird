
using Discord.WebSocket;
using System;
using System.Text.RegularExpressions;

namespace Peskybird.App.Services
{


    public class ChannelNameGeneratorService: IChannelNameGeneratorService
    {
        
        private readonly Regex _voiceNameRegex = new("Voice (\\d+)");

        public string GenerateName(SocketGuildChannel[] categoryVoiceChannels)
        {
            var max = 0;
            var min = int.MaxValue;
            foreach (var channel in categoryVoiceChannels)
            {
                var match = _voiceNameRegex.Match(channel.Name);
                if (match.Success)
                {
                    var matchGroup = match.Groups[1];
                    var number = Convert.ToInt32(matchGroup.Value);
                    max = Math.Max(number, max);
                    min = Math.Min(number, min);
                }
            }

            if (min > 1)
            {
                return $"Voice {min - 1}";

            }
            
            return $"Voice {max + 1}";
        }
    }
}