namespace Peskybird.App.Model;

using System;

public class BotQuote
{
    public long Id { get; set; }
    public string Quote { get; set; }
    public ulong Server { get; set; }
    public ulong User { get; set; }
    public DateTimeOffset Time { get; set; }
}