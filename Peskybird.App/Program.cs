using System.IO;
using System.Threading.Tasks;
using DbUp;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Peskybird.Migrations;
using Serilog;

namespace Peskybird.App
{
    public static class Program
    {
        public static async Task Main()
        {
            var serilogLogger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            var logger = new LoggerFactory().AddSerilog(serilogLogger).CreateLogger("Logging");

            var dotenvPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(dotenvPath))
            {
                logger.LogInformation(".env added");
                DotEnv.Config(false, dotenvPath);
            }

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var wasSuccessful = Migrator.Migrate();
            if (!wasSuccessful)
            {
                logger.LogError("migration failed");
                return;
            }

            var bot = new PeskybirdBot(configuration, logger);
            await bot.RunBot();
            await Task.Delay(-1);
        }
    }

    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}