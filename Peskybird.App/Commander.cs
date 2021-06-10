using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Peskybird.App
{
    public class Commander
    {
        private Dictionary<string, Func<IMessage, Task>> Commands { get; set; }
        public Commander()
        {
            Commands = new();
        }

        public void Add(string prefix, Func<IMessage, Task> handle)
        {
            Commands.Add(prefix, handle);
        }

        public async Task Execute(string prefix, IMessage message)
        {
            if (Commands.ContainsKey(prefix.ToLower()))
            {
                var command = Commands[prefix];
                await command(message);
            }
            else
            {
                var textChannel = message.Channel as SocketTextChannel;
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync($"pesky does not know what to do with \"{prefix}\"");
                }
            }
        }
    }
}