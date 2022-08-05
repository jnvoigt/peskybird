namespace Peskybird.App.Model;

using System.ComponentModel.DataAnnotations.Schema;

public class ChannelConfig
{
    public long Id { get; set; }
    public ulong Server { get; set; }
    public ulong Category { get; set; }
    [Column("name_generator")] public int NameGenerator { get; set; }
}