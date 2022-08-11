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
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(
                    (_, services) =>
                        services.AddSingleton(
                            x =>
                                new DiscordSocketClient(
                                    new DiscordSocketConfig
                                    {
                                        GatewayIntents = GatewayIntents.AllUnprivileged,
                                        AlwaysDownloadUsers = true
                                    }
                                )
                        )
                )
                .Build();
            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope servicescope = host.Services.CreateScope();
            IServiceProvider provider = servicescope.ServiceProvider;

            var _client = provider.GetRequiredService<DiscordSocketClient>();

            _client.Log += async (LogMessage msg) =>
            {
                Console.WriteLine(msg.Message);
            };

            _client.Ready += async () =>
            {
                Console.WriteLine("Bot ready!");
            };

            await _client.LoginAsync(
                TokenType.Bot,
                Environment.GetEnvironmentVariable("DiscordToken")
            );
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
