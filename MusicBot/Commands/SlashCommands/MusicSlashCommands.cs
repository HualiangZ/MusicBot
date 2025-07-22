using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Protocol;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace MusicBot.Commands.SlashCommands
{
    internal class MusicSlashCommands : ApplicationCommandModule
    {
        private readonly IAudioService _audioService;

        private List<LavalinkTrack> trackQueue = new List<LavalinkTrack>();
        public MusicSlashCommands(IAudioService audioService) 
        {
            this._audioService = audioService;
        }
        public async Task SongAddedResponse(InteractionContext context)
        {
            var addedResponse = new DiscordFollowupMessageBuilder().WithContent("Added");
            var response = await context
                .FollowUpAsync(addedResponse)
                .ConfigureAwait(false);

            await response.DeleteAsync().ConfigureAwait(false);
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
      
            bool isPlaylist = false;
            var errResponse = new DiscordFollowupMessageBuilder()
                        .WithContent("No results.")
                        .AsEphemeral();

            if (song.Contains("playlist"))
            {
                isPlaylist = true;
            }

            if (isPlaylist) 
            { 
                var playlist = await _audioService.Tracks.LoadTracksAsync(song, searchMode: TrackSearchMode.Spotify).ConfigureAwait(false);
                foreach(var track in playlist.Tracks)
                {
                    if(track is not null)
                    {
                        await player.PlayAsync(track).ConfigureAwait(false);
                    }
                    else
                    {
                        await context.FollowUpAsync(errResponse).ConfigureAwait(false);
                    }
                }
                await SongAddedResponse(context).ConfigureAwait(false);
            }
            else
            {
                LavalinkTrack track = null;
                if (song.Contains("spotify"))
                {
                    track = await _audioService.Tracks
                    .LoadTrackAsync(song, TrackSearchMode.Spotify)
                    .ConfigureAwait(false);
                }
                else
                {
                    track = await _audioService.Tracks
                    .LoadTrackAsync(song, TrackSearchMode.YouTubeMusic)
                    .ConfigureAwait(false);
                }

                if (track is null)
                {
                    await context
                        .FollowUpAsync(errResponse)
                        .ConfigureAwait(false);

                    return;
                }

                var position = await player.PlayAsync(track).ConfigureAwait(false);

                if (position is 0)
                {
                    await SongAddedResponse(context).ConfigureAwait(false);
                }
                else
                {
                    await context
                        .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Added to queue {track.Title}, {track.Author}"))
                        .ConfigureAwait(false);
                }

            }

        }


        [SlashCommand("Skip", "Skip current song")]
        public async Task Skip(InteractionContext context)
        {
            var player = await GetPlayerAsync(context, connectToVoiceChannel: false).ConfigureAwait(false);
            if(player is null)
            {
                return;
            }

            if(player.CurrentTrack  is null)
            {
                await context.CreateResponseAsync("Nothing playing!").ConfigureAwait(false);
                return;
            }
            await player.SkipAsync().ConfigureAwait(false);

            var track = player.CurrentTrack;

            if(track is null)
            {
                await context.CreateResponseAsync("Skipped. Stopped playing because the queue is now empty.").ConfigureAwait(false);
            }
        }

        private async ValueTask<MyQueuedPlayer?> GetPlayerAsync(InteractionContext context, bool connectToVoiceChannel = true)
        {
            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            static ValueTask<MyQueuedPlayer> CreatePlayerAsync(IPlayerProperties<MyQueuedPlayer, MyQueuePlayerOptions> properties, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ArgumentNullException.ThrowIfNull(properties);

                return ValueTask.FromResult(new MyQueuedPlayer(properties));
            }

            var options = new MyQueuePlayerOptions() {HistoryCapacity = 10000, TextChannel = context.Channel} ;

            var resultPlayer = await _audioService.Players
                .RetrieveAsync<MyQueuedPlayer, MyQueuePlayerOptions>(context.Guild.Id,
                context.Member?.VoiceState.Channel.Id, 
                CreatePlayerAsync, 
                Options.Create(options), 
                retrieveOptions )
                .ConfigureAwait(false);

            if (!resultPlayer.IsSuccess)
            {
                var errMessage = resultPlayer.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not in a voice channel",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is not connected",
                    _ => "Unknown error.",
                };

                var errResponse = new DiscordFollowupMessageBuilder()
                    .WithContent(errMessage)
                    .AsEphemeral();

                await context.FollowUpAsync(errResponse).ConfigureAwait(false);

            }

            return resultPlayer.Player;
        } 

    }
}
