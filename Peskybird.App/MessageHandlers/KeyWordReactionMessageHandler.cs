namespace Peskybird.App.MessageHandlers;

using Contract;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class KeyWordReactionMessageHandler : IMessageHandler
{
    private readonly IEmoteDefinitionSingleton _emoteDefinitionSingleton;
    private readonly ILogger _logger;

    public KeyWordReactionMessageHandler(IEmoteDefinitionSingleton emoteDefinitionSingleton, ILogger logger)
    {
        _emoteDefinitionSingleton = emoteDefinitionSingleton;
        _logger = logger;
    }

    private IEnumerable<EmoteScanner> BuildEmoteScanner(IEnumerable<EmoteDefinition> definitions)
    {
        return definitions.Select(def =>
        {
            if (Emoji.TryParse(def.Emote, out var emote))
            {
                return (emote, keywords: def.Keywords);
            }

            (IEmote? emote, IEnumerable<string> keywords) t = (emote: null, keywords: Enumerable.Empty<string>());
            return t;
        }).Where(t => t.emote != null).Select(t => new EmoteScanner(t.emote!, t.keywords));
    }

    public async Task Execute(IMessage message)
    {
        if (message.Channel is SocketTextChannel textChannel && message is SocketMessage restUserMessage)
        {
            var scanners = BuildEmoteScanner(_emoteDefinitionSingleton.GetPredefinedEmotes()).ToArray();
            foreach (var c in message.Content)
            {
                foreach (var emoteScanner in scanners)
                {
                    emoteScanner.Step(c);
                }
            }

            var emotes = scanners.Where(s => s.Result).Select(s => s.Emote);
            foreach (var emote in emotes)
            {
                await restUserMessage.AddReactionAsync(emote);
            }
        }
    }
}