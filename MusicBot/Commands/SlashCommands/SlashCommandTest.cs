using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBot.Commands.SlashCommands
{
    public class SlashCommandTest : ApplicationCommandModule
    {
        [SlashCommand("test", "This is a test")]       
        public async Task TestSlashCommands(InteractionContext context)
        {
            await context.DeferAsync();
            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "This is a test",
            };
            await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        
        }
    }
}
