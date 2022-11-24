using Discord.Interactions;
using Discord;

namespace alfred.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
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
        public async Task HandleSessionCommand()
        {
            await RespondAsync(
                "Oh! Very sorry, Master Wayne, but the Discord Event feature isn't ready quite yet. \U0001F97A"
            );
        }
    }
}
