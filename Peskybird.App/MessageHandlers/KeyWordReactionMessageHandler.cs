namespace Peskybird.App.MessageHandlers;

using Contract;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class KeyWordReactionMessageHandler : IMessageHandler
{
    private readonly ILogger _logger;
    private readonly Lazy<IEnumerable<EmoteDefinition>>_emoteDefinitions = new(LoadEmoteDefinitions);

    public KeyWordReactionMessageHandler(ILogger logger)
    {
        _logger = logger;
    }

    private static IEnumerable<EmoteDefinition> LoadEmoteDefinitions()
    {
        var assembly = typeof(KeyWordReactionMessageHandler).Assembly;

        using var stream = assembly.GetManifestResourceStream(typeof(KeyWordReactionMessageHandler).Namespace + ".emotes.json");
        using var streamReader = new StreamReader(stream!);
        using var jsonReader = new JsonTextReader(streamReader);

        var serializer = new JsonSerializer();
        return serializer.Deserialize<IEnumerable<EmoteDefinition>>(jsonReader) ?? Enumerable.Empty<EmoteDefinition>();
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
            var scanners = BuildEmoteScanner(_emoteDefinitions.Value).ToArray();
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