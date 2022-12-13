using Discord;
using Discord.Interactions;

namespace alfred.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("session", "Alfred, I want to play with my Bat-friends.")]
        public async Task HandleSessionCommand(string Name, DateTime Start, String Timezone, String Duration, string Location, string? Description = null)
        {
            try 
            {
                TimeZoneInfo UserTimezone = TimeZoneInfo.FindSystemTimeZoneById(Timezone);
                DateTimeOffset ConvertedStart = TimeZoneInfo.ConvertTime(Start, UserTimezone, TimeZoneInfo.Utc);
                if (ConvertedStart < DateTime.Now)
                {
                    throw new ArgumentNullException(nameof(ConvertedStart));
                }
                TimeSpan DurationSpan = TimeSpan.Parse(Duration);
                // Calculate End time and create the scheduled event
                DateTimeOffset End = ConvertedStart + DurationSpan;
                var guildEvent = await Context.Guild.CreateEventAsync(Name, ConvertedStart,  GuildScheduledEventType.External, endTime: End, location: Location, description: Description);
                // Get URL of scheduled event
                string eventURL = "https://discord.com/events/" + Context.Guild.Id + "/" + guildEvent.Id;
                // Get server profile or default user profile
                string AuthorName = Context.Guild.GetUser(Context.User.Id).Nickname != null ? Context.Guild.GetUser(Context.User.Id).Nickname : Context.User.Username;
                string AuthorIcon = Context.Guild.GetUser(Context.User.Id).GetGuildAvatarUrl() != null ? Context.Guild.GetUser(Context.User.Id).GetGuildAvatarUrl() : Context.User.GetAvatarUrl();
                // Build Embed Response
                string Response = "New Session: " + Name;
                string startTimeCode = "<t:" + ConvertedStart.ToUnixTimeSeconds().ToString() + ":R>";
                string endTimeCode = "<t:" + End.ToUnixTimeSeconds().ToString() + ":R>";
                EmbedBuilder embed = new EmbedBuilder
                {
                    // Embed property can be set within object initializer
                    Title = Response,
                    ThumbnailUrl = guildEvent.Guild.IconUrl
                };
                    // Or with methods
                embed.WithFooter(footer => footer.Text = Location)
                    .WithAuthor(AuthorName, AuthorIcon)
                    .WithColor(Color.Blue)
                    .WithDescription(Description)
                    .AddField("Starts",startTimeCode)
                    .AddField("Ends",endTimeCode)
                    .WithUrl(eventURL);
                //Send embed with mention
                await RespondAsync(Context.Guild.EveryoneRole.Mention,embed: embed.Build());
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
                await RespondAsync("DateTime needs to be in the future, try again.", ephemeral: true);
            }
            catch (FormatException)
            {
                await RespondAsync("TimeSpan not in a valid format.", ephemeral: true);
            }
        }
    }
}