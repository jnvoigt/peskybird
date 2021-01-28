using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Peskybird.App
{
    public class PeskybirdContext: DbContext
    {
        public DbSet<BotQuote> Quotes { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=pesky.sqlite", x => x.MigrationsAssembly("Peskybird.Migrations"));
        }
    }

    
    [Table("Quotes")]
    public class BotQuote
    {
        public long Id { get; set; }
        public string Quote { get; set; }
        public ulong Server { get; set; }
        public ulong User { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}