namespace Peskybird.App.MessageHandlers;

using Discord;
using System.Collections.Generic;
using System.Linq;

public class EmoteScanner
{
    public IEmote Emote { get; }
    private readonly IEnumerable<KeywordScanner> _scanners;
    public bool Result { get; private set; }

    public EmoteScanner(IEmote emote, IEnumerable<string> keyWords)
    {
        Emote = emote;
        _scanners = keyWords.Select(kw => new KeywordScanner(kw.ToLowerInvariant())).ToArray();
    }

    public IEnumerable<KeywordScanner> BuildScanner()
    {
        return Enumerable.Empty<KeywordScanner>();
    }

    public void Step(char c)
    {
        if (Result)
        {
            return;
        }

        foreach (var scanner in _scanners)
        {
            scanner.Step(c);
            if (scanner.Result)
            {
                Result = true;
                return;
            } 
        }
    }
}