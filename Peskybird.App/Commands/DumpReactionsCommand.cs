namespace Peskybird.App.Commands;

using Contract;
using Discord;
using Discord.WebSocket;
using Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Command("dumpreactions")]
public class DumpReactionsCommand: ICommand
{
    private readonly IEmoteDefinitionService _emoteDefinitionService;

    public DumpReactionsCommand(IEmoteDefinitionService emoteDefinitionService)
    {
        _emoteDefinitionService = emoteDefinitionService;
    }
    public async Task Execute(IMessage message)
    {
        var textChannel = message.Channel as SocketTextChannel;
        if (textChannel == null)
        {
            return;
        }
        var pagesize = 15;

        if (message.AuthorIsAdmin())
        {
            var groupedEmotes = _emoteDefinitionService.GetPredefinedEmotes()
                .Select((d, i) => (d, i))
                .GroupBy((def) => def.i / pagesize + 1, tuple => tuple.d).ToArray();
            var count = groupedEmotes.Length;

            foreach (var emoteGroups in groupedEmotes)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append($"Emote reactions dump page {emoteGroups.Key} of {count}");

                foreach (var emote in emoteGroups)
                {
                    stringBuilder.Append("\n");
                    stringBuilder.Append($"{emote.Emote}");
                    stringBuilder.Append("``");
                    foreach (var kw in emote.Keywords)
                    {
                        stringBuilder.Append($"'{kw}'");
                    }
                    stringBuilder.Append("``");
                }
                
                await textChannel.SendMessageAsync(stringBuilder.ToString());

            }
            

        }
    }
}