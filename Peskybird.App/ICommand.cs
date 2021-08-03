using System;
using System.Threading.Tasks;
using Discord;

namespace Peskybird.App
{
    public interface ICommand
    {
        Task Execute(IMessage message);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public string Key { get; }

        public CommandAttribute(string key)
        {
            this.Key = key;
        }
    }
}