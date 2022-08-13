using Discord.Commands;
using Discord.Interactions;
using Discord;

namespace alfred.Modules
{
    public class PrefixModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task HandlePingCommand()
        {
            await Context.Message.ReplyAsync("PING");
        }
    }
}
