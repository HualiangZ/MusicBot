﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MusicBot.Commands;
using MusicBot.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBot
{
    internal class Program
    {
        public static DiscordClient client { get; set; }
        private static CommandsNextExtension commands {  get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new JsonReader();
            await jsonReader.ReadJson();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            client = new DiscordClient(discordConfig);

            client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(1)
            });
 
            client.Ready += Client_Ready;
            client.MessageCreated += MessageCreated;
            client.VoiceStateUpdated += VoiceStateUpdated;

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] {jsonReader.prefix},
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            commands = client.UseCommandsNext(commandsConfig);
            commands.RegisterCommands<TestCommands>();

            var slashCommandConfig = client.GetSlashCommands();

            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task VoiceStateUpdated(DiscordClient sender, DSharpPlus.EventArgs.VoiceStateUpdateEventArgs args)
        {
            if(args.Before == null && args.Channel.Name == "General")
            {
                await args.Channel.SendMessageAsync($"{args.User.Username} has joined voice");
            }
        }

        private static async Task MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs args)
        {
            if(args.Author.IsBot == false)
            {
                await args.Channel.SendMessageAsync($"message was created by {args.Author.Username}");
            }
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
