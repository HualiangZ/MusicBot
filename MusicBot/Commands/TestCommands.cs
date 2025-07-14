using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web;

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

        [Command("interact")]
        public async Task Interact(CommandContext context)
        {
            var interact = Program.client.GetInteractivity();

            var message = await interact.WaitForMessageAsync(m => m.Content == "hello");
            if(message.Result.Content == "hello")
            {
                await context.Channel.SendMessageAsync($"{context.User.Username} hello");
            }
        }
        [Command("select")]
        public async Task Select(CommandContext context)
        {
            var interact = Program.client.GetInteractivity();

            DiscordEmoji[] emojiOptions = { 
                DiscordEmoji.FromName(Program.client, ":one:"), 
                DiscordEmoji.FromName(Program.client, ":two:"),
                DiscordEmoji.FromName(Program.client, ":three:"),
                DiscordEmoji.FromName(Program.client, ":four:")
            };

            string optionDiscription = 
                $"{emojiOptions[0]} \n " +
                $"{emojiOptions[1]} \n " +
                $"{emojiOptions[2]} \n " +
                $"{emojiOptions[3]} \n ";

            var message = new DiscordEmbedBuilder
            {
                Title = "select one",
                Description = optionDiscription,
            };

            var sentMessage = await context.Channel.SendMessageAsync(embed: message);
            
            foreach(var emoji in emojiOptions) {
                await sentMessage.CreateReactionAsync(emoji);
            }

            var waitForReaction = await interact.WaitForReactionAsync(m => m.Message.Id == sentMessage.Id);
            if (waitForReaction.Result.Message.Id == sentMessage.Id) {
                if (waitForReaction.Result.Emoji == emojiOptions[0])
                    await context.Channel.SendMessageAsync($"{context.User.Username} has selected {waitForReaction.Result.Emoji.Name}");

                else if(waitForReaction.Result.Emoji == emojiOptions[1])
                    await context.Channel.SendMessageAsync($"{context.User.Username} has selected {waitForReaction.Result.Emoji.Name}");

                else if (waitForReaction.Result.Emoji == emojiOptions[2])
                    await context.Channel.SendMessageAsync($"{context.User.Username} has selected {waitForReaction.Result.Emoji.Name}");

                else if (waitForReaction.Result.Emoji == emojiOptions[3])
                    await context.Channel.SendMessageAsync($"{context.User.Username} has selected {waitForReaction.Result.Emoji.Name}");

            }
        } 
    }
}
