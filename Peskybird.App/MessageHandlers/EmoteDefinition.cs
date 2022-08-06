namespace Peskybird.App.MessageHandlers;

using System;
using System.Collections.Generic;
using System.Linq;

public class EmoteDefinition
{
    public string Emote { get; set; } = String.Empty;
    public IEnumerable<string> Keywords { get; set; } = Enumerable.Empty<string>();
}