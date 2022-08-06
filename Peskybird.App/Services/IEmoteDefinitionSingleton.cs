namespace Peskybird.App.Services;

using MessageHandlers;
using System.Collections.Generic;

public interface IEmoteDefinitionSingleton
{
    IEnumerable<EmoteDefinition> GetPredefinedEmotes();
}