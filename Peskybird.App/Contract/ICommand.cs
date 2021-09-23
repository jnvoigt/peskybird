using Discord;
using System;
using System.Threading.Tasks;

namespace Peskybird.App.Contract
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