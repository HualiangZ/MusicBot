using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace MusicBot.Commands
{
    internal class TestCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task TestCommand(CommandContext context)
        {
            await context.Channel.SendMessageAsync("this is a test");
        }

        [Command("add")]
        public async Task Add(CommandContext context, int n1, int n2)
        {
            await context.Channel.SendMessageAsync($"{n1+n2}");
        }

        [Command("embed1")]
        public async Task EmbedMessage(CommandContext context)
        {
            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("First discord embed")
                    .WithDescription($"This command was ran by {context.User.Username}"));
            await context.Channel.SendMessageAsync(message);
        }

        [Command("embed2")]
        public async Task EmbedMessage2(CommandContext context)
        {
            var message = new DiscordEmbedBuilder()
            {
                Title = "Second embed",
                Description = $"This command was ran by {context.User.Username}",
                Color = DiscordColor.Blue,
            };
            await context.Channel.SendMessageAsync(embed: message);
        }
    }
}
