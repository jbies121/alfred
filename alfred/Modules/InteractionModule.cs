using Discord.Interactions;
using Discord;

namespace alfred.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("slap", "Receive a ping message.")]
        public async Task HandlePingCommand()
        {
            await RespondAsync("Oh! Very sorry, Master Wayne. \U0001F97A");
        }
    }
}
