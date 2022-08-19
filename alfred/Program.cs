using Discord.Interactions;
using Discord.WebSocket;
//using alfred.Log;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.Yaml;
using Discord.Commands;
using Discord;

namespace alfred
{
    public class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("config.yml")
                .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(
                    (_, services) =>
                        services
                            .AddSingleton(config)
                            .AddSingleton(
                                x =>
                                    new DiscordSocketClient(
                                        new DiscordSocketConfig
                                        {
                                            GatewayIntents = GatewayIntents.AllUnprivileged,
                                            AlwaysDownloadUsers = true
                                        }
                                    )
                            )
                            .AddSingleton(
                                x =>
                                    new InteractionService(
                                        x.GetRequiredService<DiscordSocketClient>()
                                    )
                            )
                            .AddSingleton<InteractionHandler>()
                            .AddSingleton(x => new CommandService())
                )
                .Build();
            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope servicescope = host.Services.CreateScope();
            IServiceProvider provider = servicescope.ServiceProvider;

            var _client = provider.GetRequiredService<DiscordSocketClient>();
            var sCommands = provider.GetRequiredService<InteractionService>();
            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();
            var config = provider.GetRequiredService<IConfigurationRoot>();

            _client.Log += async (LogMessage msg) =>
            {
                Console.WriteLine(msg.Message);
            };
            sCommands.Log += async (LogMessage msg) =>
            {
                Console.WriteLine(msg.Message);
            };

            _client.Ready += async () =>
            {
                Console.WriteLine("Bot ready!");
                await sCommands.RegisterCommandsToGuildAsync(UInt64.Parse(config["testGuild"]));
            };

            await _client.LoginAsync(Discord.TokenType.Bot, config["tokens:discord"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
