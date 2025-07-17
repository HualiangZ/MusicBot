using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
namespace MusicBot.Commands.SlashCommands
{
    internal class MusicSlashCommands : ApplicationCommandModule
    {
        private readonly IAudioService _audioService;
        public static DiscordMessage discordMessage { get;  set; }
        public MusicSlashCommands(IAudioService audioService) 
        {
            this._audioService = audioService;
        }

        [SlashCommand("play", "plays music")]
        public async Task Play(InteractionContext context, [Option("song", "Song to play")] string song)
        {
            var interact = ApplicationHost.client.GetInteractivity();
            await context.DeferAsync().ConfigureAwait(false);

            var player = await GetPlayerAsync(context, connectToVoiceChannel: true).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            var track = await _audioService.Tracks
                .LoadTrackAsync(song, TrackSearchMode.YouTube)
                .ConfigureAwait(false);

            if (track is null)
            {
                var errResponse = new DiscordFollowupMessageBuilder()
                .WithContent("No results.")
                .AsEphemeral();

                await context
                    .FollowUpAsync(errResponse)
                    .ConfigureAwait(false);

                return;
            }

            var position = await player.PlayAsync(track).ConfigureAwait(false);

            var skipBtn = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "skipBtn", "Skip");

            var embedMusic = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "Now Playing",
                ImageUrl = track.ArtworkUri.ToString(),
                Description = $"{track.Title} \n",
            };


            if (position is 0)
            {
                var response = await context
                    .EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMusic).AddComponents(skipBtn))
                    .ConfigureAwait(false);

                var interactWithSkipButton = await response.WaitForButtonAsync("skipBtn").ConfigureAwait(false);
                if(interactWithSkipButton.Result is not null)
                {
                    await this.Skip(context).ConfigureAwait(false);
                }
                
            }
            else
            {
                await context
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Added to queue {track.Title}"))
                    .ConfigureAwait(false);
            }

        }

        [SlashCommand("skip", "skip track")]
        public async Task Skip(InteractionContext context)
        {
            var player = await GetPlayerAsync(context, connectToVoiceChannel: false);
            if (player is null) { return; }

            if (player.CurrentTrack is null)
            {
                await context.CreateResponseAsync("Nothing is Playing").ConfigureAwait(false);
                return;
            }

            await player.SkipAsync().ConfigureAwait(false);

            var track = player.CurrentTrack;

            var skipBtn = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Danger, "skipBtn", "Skip");

            if (track is not null)
            {
                var embedMusic = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Green,
                    Title = "Now Playing",
                    ImageUrl = track.ArtworkUri.ToString(),
                    Description = $"{track.Title} \n",
                };

                var response = await context
               .FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embedMusic).AddComponents(skipBtn))
               .ConfigureAwait(false);

                var interactWithSkipButton = await response.WaitForButtonAsync("skipBtn").ConfigureAwait(false);
                if (interactWithSkipButton.Result is not null)
                {
                    await this.Skip(context).ConfigureAwait(false);
                }
            }
            else
            {
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Playlist is empty")).ConfigureAwait(false);
            }
        }


        private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(InteractionContext contex, bool connectToVoiceChannel = true)
        {
            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            var playerOptions = new QueuedLavalinkPlayerOptions { HistoryCapacity = 10000 };

            var result = await _audioService.Players
                .RetrieveAsync(
                    contex.Guild.Id, 
                    contex.Member?.VoiceState.Channel.Id, 
                    playerFactory: PlayerFactory.Queued, 
                    Options.Create(playerOptions), 
                    retrieveOptions)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var errMessage = result.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not in a voice channel",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is not connected",
                    _ => "Unknown error.",
                };

                var errResponse = new DiscordFollowupMessageBuilder()
                    .WithContent(errMessage)
                    .AsEphemeral();

                await contex.FollowUpAsync(errResponse).ConfigureAwait(false);

            }
            return result.Player;
        } 

    }
}
