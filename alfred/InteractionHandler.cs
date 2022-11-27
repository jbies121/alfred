using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace alfred
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private readonly IConfigurationRoot _config;
        private ulong _guildId;

        public InteractionHandler(
            DiscordSocketClient client,
            InteractionService commands,
            IServiceProvider services,
            IConfigurationRoot config
        )
        {
            _client = client;
            _commands = commands;
            _services = services;
            _config = config;
            _guildId = UInt64.Parse(_config["testGuild"]);
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine(_guildId);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.InteractionCreated += HandleInteraction;
            _client.SlashCommandExecuted += HandleSlashCommand;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                // https://discord.com/developers/docs/interactions/receiving-and-responding#security-and-authorization
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleSlashCommand(SocketSlashCommand arg)
        {
            try
            {
                switch(arg.CommandName)
                {
                    case "session":
                        var guild = _client.GetGuild(_guildId);
                        await guild.CreateEventAsync("test event", DateTimeOffset.UtcNow.AddDays(1),  GuildScheduledEventType.External, endTime: DateTimeOffset.UtcNow.AddDays(2), location: "Space");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
