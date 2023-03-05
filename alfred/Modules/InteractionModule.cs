using Discord;
using Discord.Interactions;
using Newtonsoft.Json.Linq;

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
            // Set path based on series selected
            JObject result = JObject.Parse(File.ReadAllText("penalties.json"));
            // Filter for F3 drivers with unserved penalties
            var f3DriversWithUnservedPenalties = result["season5"]["drivers"].Where(
                driver =>
                    driver["F3"] != null
                    && (
                        (int)driver["F3"]["penalty"]["ExQ"]["unserved"] > 0
                        || (int)driver["F3"]["penalty"]["Drive-Thru"]["unserved"] > 0
                        || (int)driver["F3"]["penalty"]["PitStart"]["unserved"] > 0
                        || (int)driver["F3"]["penalty"]["RaceBan"]["unserved"] > 0
                    )
            );
            // Filter for F1 drivers with unserved penalties
            var f1DriversWithUnservedPenalties = result["season5"]["drivers"].Where(
                driver =>
                    driver["F1"] != null
                    && (
                        (int)driver["F1"]["penalty"]["ExQ"]["unserved"] > 0
                        || (int)driver["F1"]["penalty"]["Drive-Thru"]["unserved"] > 0
                        || (int)driver["F1"]["penalty"]["PitStart"]["unserved"] > 0
                        || (int)driver["F1"]["penalty"]["RaceBan"]["unserved"] > 0
                    )
            );
            // Get Drivers with penalty points
            // Filter F1 drivers with more than 0 penalty points
            var f1DriversWithPenalties = result["season5"]["drivers"].Where(
                d => d["F1"] != null && d["F1"]["points"].Value<int>() > 0
            );

            // Sort F1 drivers by their penalty points in descending order
            var sortedF1Drivers = f1DriversWithPenalties.OrderByDescending(
                d => d["F1"]["points"].Value<int>()
            );

            // Filter F3 drivers with more than 0 penalty points
            var f3DriversWithPenalties = result["season5"]["drivers"].Where(
                d => d["F3"] != null && d["F3"]["points"].Value<int>() > 0
            );

            // Sort F3 drivers by their penalty points in descending order
            var sortedF3Drivers = f3DriversWithPenalties.OrderByDescending(
                d => d["F3"]["points"].Value<int>()
            );
            // Create penalty 'unserved_box' listing driver penalties. This involves a flag, the driver and their penalty score.
            string unserved_box = "";
            string total_box = "";
            if (Series == "F1")
            {
                foreach (var driver in sortedF1Drivers)
                {
                    var penalty = driver["F1"]["penalty"];
                    driver["F1"]["flag"] = "<:greenflag:1007774467872796832>";
                    if ((int)penalty["ExQ"]["unserved"] > 0)
                    {
                        driver["F1"]["flag"] = "<:yellowflag:1081738960306450532>";
                        driver["F1"]["ToServe"] = "ExQ";
                    }
                    if ((int)penalty["Drive-Thru"]["unserved"] > 0)
                    {
                        driver["F1"]["flag"] = "<:blackflag:1081739006552854542>";
                        driver["F1"]["ToServe"] = "Drive-Thru";
                    }
                    if ((int)penalty["PitStart"]["unserved"] > 0)
                    {
                        driver["F1"]["flag"] = "<:redflag:1081738987988852789>";
                        driver["F1"]["ToServe"] = "PitStart";
                    }
                    if ((int)penalty["RaceBan"]["unserved"] > 0)
                    {
                        driver["F1"]["flag"] = "<:blackandwhiteflag:1081739024286355538>";
                        driver["F1"]["ToServe"] = "RaceBan";
                    }

                    total_box +=
                        driver["F1"]["flag"]
                        + " "
                        + driver["name"]
                        + " - "
                        + driver[Series]["points"]
                        + @"
                    ";
                }
                if (f1DriversWithUnservedPenalties.Any())
                {
                    foreach (var driver in f1DriversWithUnservedPenalties)
                    {
                        unserved_box +=
                            driver["F1"]["flag"]
                            + " "
                            + driver["name"]
                            + " - "
                            + driver[Series]["penalty"][driver[Series]["ToServe"].ToString()][
                                "unserved"
                            ]
                            + "x "
                            + driver[Series]["ToServe"]
                            + @"
                            ";
                    }
                }
                else
                {
                    unserved_box = "No unserved penalties for this series.";
                }
            }
            else
            {
                foreach (var driver in sortedF3Drivers)
                {
                    var penalty = driver["F3"]["penalty"];
                    driver["F3"]["flag"] = "<:greenflag:1007774467872796832>";
                    if ((int)penalty["ExQ"]["unserved"] > 0)
                    {
                        driver["F3"]["flag"] = "<:yellowflag:1081738960306450532>";
                        driver["F3"]["ToServe"] = "ExQ";
                    }
                    if ((int)penalty["Drive-Thru"]["unserved"] > 0)
                    {
                        driver["F3"]["flag"] = "<:blackflag:1081739006552854542>";
                        driver["F3"]["ToServe"] = "Drive-Thru";
                    }
                    if ((int)penalty["PitStart"]["unserved"] > 0)
                    {
                        driver["F3"]["flag"] = "<:redflag:1081738987988852789>";
                        driver["F3"]["ToServe"] = "PitStart";
                    }
                    if ((int)penalty["RaceBan"]["unserved"] > 0)
                    {
                        driver["F3"]["flag"] = "<:blackandwhiteflag:1081739024286355538>";
                        driver["F3"]["ToServe"] = "RaceBan";
                    }

                    total_box +=
                        driver["F3"]["flag"]
                        + " "
                        + driver["name"]
                        + " - "
                        + driver[Series]["points"]
                        + @"
                    ";
                }
                if (f3DriversWithUnservedPenalties.Any())
                {
                    foreach (var driver in f3DriversWithUnservedPenalties)
                    {
                        unserved_box +=
                            driver["F3"]["flag"]
                            + " "
                            + driver["name"]
                            + " - "
                            + driver[Series]["penalty"][driver[Series]["ToServe"].ToString()][
                                "unserved"
                            ]
                            + "x "
                            + driver[Series]["ToServe"]
                            + @"
                            ";
                    }
                }
                else
                {
                    unserved_box = "No unserved penalties for this series.";
                }
            }

            // Create Embed
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
                        footer.Text =
                            "Unserved penalties must be served at the next event participated in."
                )
                .WithColor(Color.Red)
                .WithDescription(Description);
            embed
                .AddField("Total Penalty Points", total_box)
                .AddField("Unserved Penalties", unserved_box);
            //Send embed with mention
            await RespondAsync(Context.Guild.EveryoneRole.Mention, embed: embed.Build());
        }
    }
}
