namespace Peskybird.App.Model;

using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class QuoteRepository
{
    private readonly PeskybirdDbContext _dbContext;

    public QuoteRepository(PeskybirdDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddQuote(BotQuote botQuote)
    {
        await _dbContext.Quotes.AddAsync(botQuote);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<BotQuote?> GetRandomQuote(ulong guildId)
    {
        var r = new Random();
        BotQuote?[] quotes = await _dbContext.Quotes.AsQueryable().Where(q => q.Server == guildId).ToArrayAsync();

        if (quotes.Length > 0)
        {
            var quote = quotes[r.Next(quotes.Length)];
            return quote;
        }

        return null;
    }
}