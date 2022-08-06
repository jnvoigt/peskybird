namespace Peskybird.App.Services;

using MessageHandlers;
using System.Collections.Generic;

public interface IEmoteDefinitionService
{
    IEnumerable<EmoteDefinition> GetPredefinedEmotes();
}