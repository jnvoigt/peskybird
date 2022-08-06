namespace Peskybird.App.MessageHandlers;

public class KeywordScanner
{
    private readonly string _keyword;
    private int _pointer = -1;
    public bool Result { get; private set; }
    public KeywordScanner(string keyword)
    {
        _keyword = keyword;
    }

    public void Step(char chartToCheck)
    {
        if (Result || _keyword.Length == 0)
        {
            return;
        }
        var c = _keyword[_pointer + 1];

        if (c == normalizeChar(chartToCheck))
        {
            _pointer++;
        }
        else
        {
            _pointer = -1;
            return;
        }
        
        if (_pointer + 1 >= _keyword.Length)
        {
            Result = true;
        }
    }

    private char normalizeChar(char c)
    {
        if (char.IsLetter(c))
        {
            return char.ToLowerInvariant(c);
        }
        
        return c;
    }
}