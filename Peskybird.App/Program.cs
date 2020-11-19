using System.IO;
using System.Threading.Tasks;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Peskybird.App;
using Serilog;

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


var bot = new PeskybirdBot(configuration, logger);
await bot.RunBot();
await Task.Delay(-1);
