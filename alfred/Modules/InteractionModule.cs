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
        public async Task HandleSessionCommand(string Name, DateTime Start, TimeSpan Duration, string Location, string? Description = null)
        {
                try
                {
                    // Calculate End time and create the scheduled event
                    DateTime End = Start + Duration;
                    var guildEvent = await _guild.CreateEventAsync(Name, Start,  GuildScheduledEventType.External, endTime: End, location: Location, description: Description);
                    // Get URL of scheduled event
                    string eventURL = "https://discord.com/events/" + _guildId + "/" + guildEvent.Id;
                    // Build Embed Response
                    string Response = "New Session: " + Name;
                    EmbedBuilder embed = new EmbedBuilder
                    {
                        // Embed property can be set within object initializer
                        Title = Response,
                    };
                        // Or with methods
                    embed.WithAuthor(Context.Client.CurrentUser)
                        .WithFooter(footer => footer.Text = Context.Client.CurrentUser.ToString())
                        .WithColor(Color.Green)
                        .WithDescription(Description)
                        .WithUrl(eventURL)
                        .WithCurrentTimestamp();

                    //Your embed needs to be built before it is able to be sent
                    await RespondAsync(embed: embed.Build());
                }
                catch (Exception ex)
                {
                    await RespondAsync(ex.ToString());
                }
        }
    }
}