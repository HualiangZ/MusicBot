using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
namespace MusicBot.Commands.SlashCommands
{
    internal class MusicSlashCommands : ApplicationCommandModule
    {
        private readonly IAudioService _audioService;
        private DiscordMessage discordResponse { get;  set; }
        private InteractionContext _context { get; set; }
        public MusicSlashCommands(IAudioService audioService) 
        {
            this._audioService = audioService;
/*            _audioService.TrackStarted += audioService_TrackStarted;
            _audioService.TrackEnded += audioService_TrackEnded;*/
        }

/*        private async Task audioService_TrackEnded(object sender, Lavalink4NET.Events.Players.TrackEndedEventArgs eventArgs)
        {
            switch (eventArgs.Reason) 
            {
                case TrackEndReason.Finished:
                    await _context.Channel.DeleteMessageAsync(discordResponse).ConfigureAwait(false);
                    break;
            }
        }

        private async Task audioService_TrackStarted(object sender, Lavalink4NET.Events.Players.TrackStartedEventArgs eventArgs)
        {
            var skipBtn = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "skipBtn", "Skip");
            var embedMusic = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "Now Playing",
                ImageUrl = eventArgs.Player.CurrentTrack.ArtworkUri.ToString(),
                Description = $"{eventArgs.Player.CurrentTrack.Title} by: {eventArgs.Player.CurrentTrack.Author}\n",
            };

             discordResponse = await _context
           .FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embedMusic).AddComponents(skipBtn))
           .ConfigureAwait(false);

            var interactWithSkipButton = await discordResponse.WaitForButtonAsync("skipBtn").ConfigureAwait(false);

            if (interactWithSkipButton.Result is not null)
            {
                await _context.Channel.DeleteMessageAsync(discordResponse).ConfigureAwait(false);
                await this.Skip(_context).ConfigureAwait(false);
            }
        }*/

        [SlashCommand("play", "plays music")]
        public async Task Play(InteractionContext context, [Option("song", "Song to play")] string song)
        {
            var interact = ApplicationHost.client.GetInteractivity();
            await context.DeferAsync().ConfigureAwait(false);

            _context = context;

            var player = await GetPlayerAsync(context, connectToVoiceChannel: true).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            var track = await _audioService.Tracks
                .LoadTrackAsync(song, TrackSearchMode.YouTubeMusic)
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
                await ResponseMessage(context, track).ConfigureAwait(false);

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

            if (track is not null)
            {
                await ResponseMessage(context, track).ConfigureAwait(false);
            }
            else
            {
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Playlist is empty")).ConfigureAwait(false);
            }
        }

        private async Task ResponseMessage(InteractionContext context, LavalinkTrack track)
        {
            var skipBtn = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "skipBtn", "Skip");
            var embedMusic = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Green,
                Title = "Now Playing",
                ImageUrl = track.ArtworkUri.ToString(),
                Description = $"{track.Title} by: {track.Author}\n",
            };

            var response = await context
           .FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embedMusic).AddComponents(skipBtn))
           .ConfigureAwait(false);

            var interactWithSkipButton = await response.WaitForButtonAsync("skipBtn").ConfigureAwait(false);

            if (interactWithSkipButton.Result is not null)
            {
                await context.Channel.DeleteMessageAsync(response).ConfigureAwait(false);
                await this.Skip(context).ConfigureAwait(false);
            }
        }
        private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(InteractionContext context, bool connectToVoiceChannel = true)
        {
            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            var playerOptions = new QueuedLavalinkPlayerOptions { HistoryCapacity = 10000 };

            var result = await _audioService.Players
                .RetrieveAsync(
                    context.Guild.Id, 
                    context.Member?.VoiceState.Channel.Id, 
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

                await context.FollowUpAsync(errResponse).ConfigureAwait(false);

            }

            return result.Player;
        } 

    }
}
