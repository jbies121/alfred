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
        private readonly ulong _guildId;
        private readonly SocketGuild _guild;

        public InteractionModule(
            DiscordSocketClient client,
            IConfigurationRoot config
        )
        {
            _client = client;
            _config = config;
            _guildId = UInt64.Parse(_config["testGuild"]);
            _guild = _client.GetGuild(_guildId);
        }

        [SlashCommand("session", "Alfred, I want to play with my Bat-friends.")]
        public async Task HandleSessionCommand(string Name, DateTime Start, TimeSpan Duration, string Location, string? Description = null)
        {
            // Calculate End time and create the scheduled event
            DateTimeOffset End = Start + Duration;
            DateTimeOffset StartOffset = Start;
            var guildEvent = await _guild.CreateEventAsync(Name, Start,  GuildScheduledEventType.External, endTime: End, location: Location, description: Description);
            // Get URL of scheduled event
            string eventURL = "https://discord.com/events/" + _guildId + "/" + guildEvent.Id;
            // Build Embed Response
            string Response = "New Session: " + Name;
            string startTimeCode = "<t:" + StartOffset.ToUnixTimeSeconds().ToString() + ":R>";
            string endTimeCode = "<t:" + End.ToUnixTimeSeconds().ToString() + ":R>";
            EmbedBuilder embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = Response,
                ThumbnailUrl = "https://cdn.discordapp.com/attachments/214118660896194561/1040866115024326816/Buckaroo_Banzai_hogwarts_221c63f9-d003-42c8-a279-26169085a861.png"
            };
                // Or with methods
            embed.WithFooter(footer => footer.Text = "Event Scheduled")
                .WithColor(Color.Green)
                .WithDescription(Description)
                .AddField("Starts",startTimeCode)
                .AddField("Ends",endTimeCode)
                .WithUrl(eventURL);
            //Your embed needs to be built before it is able to be sent
            await RespondAsync(embed: embed.Build());
        }
    }
}