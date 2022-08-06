using Xunit;

namespace Peskybird.App.Test;

using FluentAssertions;
using MessageHandlers;

public class KeywordScannerTest
{
    [Fact]
    public void SingleCharShouldMatch()
    {
        var target = new KeywordScanner("w");

        foreach (var c in "w")
        {
            target.Step(c);
        }
        var result = target.Result;

        result.Should().BeTrue();
    }
    
    
    [Fact]
    public void ShouldFindExactly()
    {
        var target = new KeywordScanner("word");

        foreach (var c in "word")
        {
            target.Step(c);
        }
        var result = target.Result;

        result.Should().BeTrue();
    }
    
    
    [Fact]
    public void WordInTextShouldFound()
    {
        var target = new KeywordScanner("word");

        foreach (var c in "hello word, where are you")
        {
            target.Step(c);
        }
        var result = target.Result;

        result.Should().BeTrue();
    }
    
    [Fact]
    public void ShouldIgnoreCase()
    {
        var target = new KeywordScanner("word");

        foreach (var c in "WoRd!")
        {
            target.Step(c);
        }
        var result = target.Result;

        result.Should().BeTrue();
    }
    
}