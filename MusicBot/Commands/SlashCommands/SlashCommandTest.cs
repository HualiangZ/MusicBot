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

        [SlashCommand("button", "button test")]
        public async Task ButtonTest(InteractionContext context)
        {
            var button1 = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "button1", "Button 1");
            var button2 = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "button2", "Button 2");

            var embed2 = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "This is a test",
                Description = "Select a Button",
            };

            await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed2).AddComponents(button1, button2));
        }

    }
}
