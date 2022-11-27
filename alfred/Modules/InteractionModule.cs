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
                await RespondWithModalAsync<SessionModal>("eventName");
        }

        [ModalInteraction("eventName")]
        public async Task HandleSessionNameInput(SessionModal modal)
        {
            string input = modal.SessionName;
            await RespondAsync(input);
        }
    }

    public class SessionModal : IModal
        {
            public string Title => "Session Modal";
            [InputLabel("Session Name:")]
            [ModalTextInput("eventName", TextInputStyle.Short, placeholder: "Session Name as shown in-game", maxLength: 50)]
            public string SessionName { get; set; }
        }
}