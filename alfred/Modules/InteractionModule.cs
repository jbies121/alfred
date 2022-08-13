using Discord.Interactions;
using Discord;

namespace alfred.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "Receive a ping message.")]
        public async Task HandlePingCommand()
        {
            await RespondAsync("PING");
        }
    }
}
