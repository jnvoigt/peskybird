namespace Peskybird.App.Test;

using FluentAssertions;
using MessageHandlers;
using Xunit;

public class EmoteScannerTest
{
    [Fact]
    public void ShouldFindSingleMatchingKeyword()
    {
        var target = new EmoteScanner(null! , new[]{"word"});

        foreach (var c in "word")
        {
            target.Step(c);
        }
        var result = target.Result;

        result.Should().BeTrue();
    }
}