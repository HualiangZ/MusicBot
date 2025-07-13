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
        public async Task TestSlashCommands(InteractionContext context, [Option("TestOptions", "Enter anything this is a test")] string opt)
        {
            await context.DeferAsync();
            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "This is a test",
                Description = opt,
            };
            await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        
        }
    }
}
