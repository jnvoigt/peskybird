using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Discord.WebSocket;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Peskybird.Migrations;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Peskybird.App
{
    using Contract;
    using Discord;
    using Model;

    public static class Program
    {
        public static async Task Main()
        {
            var socketConfig = new DiscordSocketConfig() {GatewayIntents = GatewayIntents.AllUnprivileged,};
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
            var autofacBuilder = new ContainerBuilder();
            autofacBuilder.RegisterInstance(socketConfig);
            autofacBuilder.RegisterInstance(configuration).As<IConfiguration>();
            autofacBuilder.RegisterInstance(logger).As<ILogger>();
            autofacBuilder.RegisterType<PeskybirdBot>().OwnedByLifetimeScope();
            autofacBuilder.RegisterType<PeskybirdDbContext>()
                .InstancePerLifetimeScope();
            autofacBuilder.RegisterType<DiscordSocketClient>()
                .OwnedByLifetimeScope()
                .AsSelf()
                .AsImplementedInterfaces();

            var assembly = typeof(Program).Assembly;

            autofacBuilder.RegisterAssemblyTypes(assembly)
                .Where(type =>
                    type.IsAssignableTo(typeof(ICommand)) && type.GetCustomAttribute<CommandAttribute>() != null)
                .Named<ICommand>(type => type.GetCustomAttribute<CommandAttribute>()!.Key.ToLower());

            autofacBuilder.RegisterAssemblyTypes(assembly).Where(type => type.IsAssignableTo(typeof(IMessageHandler)))
                .InstancePerLifetimeScope()
                .AsSelf()
                .AsImplementedInterfaces();

            autofacBuilder.RegisterAssemblyTypes(assembly)
                .Where(type => type.IsAssignableTo(typeof(IVoiceStateUpdate)))
                .As<IVoiceStateUpdate>();
            autofacBuilder.RegisterAssemblyTypes(assembly)
                .Where(type => type.Name.EndsWith("DbContext"))
                .AsSelf()
                .AsImplementedInterfaces();
            autofacBuilder.RegisterAssemblyTypes(assembly)
                .Where(type => type.Name.EndsWith("Repository"))
                .AsSelf()
                .AsImplementedInterfaces();


            autofacBuilder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces();

            await using var container = autofacBuilder.Build();
            var wasSuccessful = Migrator.Migrate();
            if (!wasSuccessful)
            {
                logger.LogError("migration failed");
                return;
            }

            logger.Log(LogLevel.Information, "time to start bot");
            var bot = container.Resolve<PeskybirdBot>();
            await bot.RunBot();
            await Task.Delay(-1);
        }
    }
}