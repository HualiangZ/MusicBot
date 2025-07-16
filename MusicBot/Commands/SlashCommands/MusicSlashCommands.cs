using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
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

        public MusicSlashCommands(IAudioService audioService) 
        {
            this._audioService = audioService;
        }

        [SlashCommand("play", "plays music")]
        public async Task Play(InteractionContext context, [Option("song", "Song to play")] string song)
        {
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

            if (position is 0)
            {
                await context
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Playing: {track.Uri}"))
                    .ConfigureAwait(false);
            }
            else
            {
                await context
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Added to queue {track.Uri}"))
                    .ConfigureAwait(false);
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
