using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBot.Commands.SlashCommands
{
    [SlashCommandGroup("calculator", "This is a calculator")]
    public class SlashCommandGroups : ApplicationCommandModule
    {
        /* options are not allowed to have whitespaces in parameter name */
        [SlashCommand("add", "Add 2 numbers together")]
        public async Task Add(InteractionContext context, [Option("Number1", "First number")] double num1, [Option("Number2", "Second number")] double num2)
        {
            await context.DeferAsync();

            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = "Add two numbers",
                Description = $"{num1} + {num2} = {num1 + num2}"
            };

            await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("subtract", "Subtract 2 numbers together")]
        public async Task Subtract(InteractionContext context, [Option("Number1", "First number")] double num1, [Option("Number2", "Seciond number")] double num2)
        {
            await context.DeferAsync();

            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = "Subtract two numbers",
                Description = $"{num1} - {num2} = {num1 - num2}"
            };

            await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
