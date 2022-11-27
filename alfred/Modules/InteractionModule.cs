using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace alfred.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;
        private ulong _guildId;

        public InteractionModule(
            DiscordSocketClient client,
            IConfigurationRoot config
        )
        {
            _client = client;
            _config = config;
            _guildId = UInt64.Parse(_config["testGuild"]);
        }
        
        [SlashCommand("slap", "Remind Alfred who the real Bat- I mean Man of the house is..")]
        public async Task HandleSlapCommand()
        {
            await RespondAsync("Oh! Very sorry, Master Wayne. \U0001F97A");
        }

        [SlashCommand("calendar", "Alfred, fetch the Bat- I mean the calendar..")]
        public async Task HandleCalendarCommand()
        {
            await RespondAsync(
                "Oh! Very sorry, Master Wayne, but the google interaction isn't ready quite yet. \U0001F97A"
            );
        }

        [SlashCommand("session", "Alfred, I want to play with my Bat-friends.")]
        public async Task HandleSessionCommand(string name, DateTime start, DateTime end, uint offset, string location, string? description = null)
        {
                string Response = "Event Received: " + name + " - " + start;
                DateTimeOffset startOffset = DateTimeOffset.UtcNow.AddDays(1);
                DateTimeOffset endOffset = DateTimeOffset.UtcNow.AddDays(2);

                var guild = _client.GetGuild(_guildId);
                await guild.CreateEventAsync(name, startOffset,  GuildScheduledEventType.External, endTime: endOffset, location: location, description: description);
                await RespondAsync(Response);
        }
    }
}