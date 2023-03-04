using Discord;
using Discord.Interactions;

namespace alfred.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("session", "Alfred, I want to play with my Bat-friends.")]
        public async Task HandleSessionCommand(
            [Summary(description: "Name of your session")] string Name,
            [Summary(description: "ex. 'now', '6:00 PM', or '2023-01-31 13:00'")] string Start,
            [Summary(description: "ex. 'Eastern Standard Time', 'EST'")] string Timezone,
            [Summary(description: "ex. '1:00' is 1 hour.")] string Duration,
            [Summary(description: "Where is the session located?")] string Location,
            [Summary(description: "Any other details to mention?")] string? Description = null
        )
        {
            try
            {
                // Parse and convert datetime, handle 'now' as an input
                TimeZoneInfo UserTimezone = TimeZoneInfo.FindSystemTimeZoneById(Timezone);
                DateTimeOffset ConvertedStart;
                // Take "now" as an input
                if (string.Equals(Start, "now", StringComparison.OrdinalIgnoreCase))
                {
                    ConvertedStart = DateTimeOffset.UtcNow.AddMinutes(1);
                }
                else
                {
                    DateTime ParsedStart = DateTime.Parse(Start);
                    ConvertedStart = TimeZoneInfo.ConvertTime(
                        ParsedStart,
                        UserTimezone,
                        TimeZoneInfo.Utc
                    );
                }

                TimeSpan DurationSpan = TimeSpan.Parse(Duration);
                // If user did not supply a date and the converted time is >2 days ahead of the source timezone, jump back a day.
                if (!(Start.Contains("-") || Start.Contains("/")))
                {
                    var UserTimezoneNow = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        UserTimezone
                    );
                    if (UserTimezoneNow.Day - ConvertedStart.Day == -2)
                    {
                        ConvertedStart = ConvertedStart - TimeSpan.FromDays(1);
                    }
                }
                // Handle Start time from the past
                DateTimeOffset GuildStartTime =
                    ConvertedStart < DateTime.Now
                        ? DateTimeOffset.Now.AddMinutes(1)
                        : ConvertedStart;
                // Calculate End time and create the scheduled event
                DateTimeOffset End = ConvertedStart + DurationSpan;
                var guildEvent = await Context.Guild.CreateEventAsync(
                    Name,
                    GuildStartTime,
                    GuildScheduledEventType.External,
                    endTime: End,
                    location: Location,
                    description: Description
                );
                // Get URL of scheduled event
                string eventURL =
                    "https://discord.com/events/" + Context.Guild.Id + "/" + guildEvent.Id;
                // Get server profile or default user profile
                string AuthorName =
                    Context.Guild.GetUser(Context.User.Id).Nickname != null
                        ? Context.Guild.GetUser(Context.User.Id).Nickname
                        : Context.User.Username;
                string AuthorIcon =
                    Context.Guild.GetUser(Context.User.Id).GetGuildAvatarUrl() != null
                        ? Context.Guild.GetUser(Context.User.Id).GetGuildAvatarUrl()
                        : Context.User.GetAvatarUrl();
                // Build Embed Response
                string Response = "New Session: " + Name;
                string startTimeCode =
                    "<t:" + ConvertedStart.ToUnixTimeSeconds().ToString() + ":F>";
                string endTimeCode = "<t:" + End.ToUnixTimeSeconds().ToString() + ":R>";
                EmbedBuilder embed = new EmbedBuilder
                {
                    // Embed property can be set within object initializer
                    Title = Response,
                    ThumbnailUrl = guildEvent.Guild.IconUrl
                };
                // Or with methods
                embed
                    .WithFooter(footer => footer.Text = Location)
                    .WithAuthor(AuthorName, AuthorIcon)
                    .WithColor(Color.Blue)
                    .WithDescription(Description)
                    .AddField("Starts", startTimeCode)
                    .AddField("Ends", endTimeCode)
                    .WithUrl(eventURL);
                //Send embed with mention
                await RespondAsync(Context.Guild.EveryoneRole.Mention, embed: embed.Build());
            }
            catch (TimeZoneNotFoundException)
            {
                await RespondAsync("Unable to retrieve the time zone.", ephemeral: true);
            }
            catch (InvalidTimeZoneException)
            {
                await RespondAsync("Unable to retrieve the time zone.", ephemeral: true);
            }
            catch (ArgumentNullException)
            {
                await RespondAsync(
                    "DateTime needs to be in the future, try again.",
                    ephemeral: true
                );
            }
            catch (FormatException)
            {
                await RespondAsync("Duration not in a valid format.", ephemeral: true);
            }
        }

        [SlashCommand("penalties", "Alfred, who has the most Bat-Penalties?")]
        public async Task HandlePenaltiesCommand(
            [Summary(description: "Which Series? F1 or F3")] string Series
        )
        {
            string Description = "Current season penalty points for " + Series + " series:";
            string penalty_path = Series == "F1" ? "penalty-f1.txt" : "penalty-f3.txt";
            Dictionary<string, string> result = File.ReadAllLines("penalty-f1.txt")
                .Select(x => x.Split('='))
                .ToDictionary(x => x[0], x => x[1]);

            EmbedBuilder embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "Penalty Point Standings",
                ThumbnailUrl = Context.Guild.IconUrl
            };
            // Or with methods
            embed
                .WithFooter(
                    footer =>
                        footer.Text = "Check for punishment discord roles if you have one to serve."
                )
                .WithColor(Color.Red)
                .WithDescription(Description);

            foreach (KeyValuePair<string, string> driver in result)
            {
                string box =
                    @"
"
                    + "<:greenflag:1007774467872796832>  "
                    + driver.Key
                    + " - "
                    + driver.Value
                    + @"
";
                embed.AddField("Penalties", box);
            }
            //Send embed with mention
            await RespondAsync(Context.Guild.EveryoneRole.Mention, embed: embed.Build());
        }
    }
}
