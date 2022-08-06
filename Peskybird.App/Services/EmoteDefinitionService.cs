namespace Peskybird.App.Services;

using MessageHandlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class EmoteDefinitionService : IEmoteDefinitionService
{
    private readonly Lazy<IEnumerable<EmoteDefinition>>_emoteDefinitions = new(LoadEmoteDefinitions);

    private static IEnumerable<EmoteDefinition> LoadEmoteDefinitions()
    {
        var assembly = typeof(KeyWordReactionMessageHandler).Assembly;

        using var stream = assembly.GetManifestResourceStream(typeof(EmoteDefinitionService).Namespace + ".emotes.json");
        using var streamReader = new StreamReader(stream!);
        using var jsonReader = new JsonTextReader(streamReader);

        var serializer = new JsonSerializer();
        return serializer.Deserialize<IEnumerable<EmoteDefinition>>(jsonReader) ?? Enumerable.Empty<EmoteDefinition>();
    }

    public IEnumerable<EmoteDefinition> GetPredefinedEmotes()
    {
        return this._emoteDefinitions.Value;
    }
}