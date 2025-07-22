using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Protocol.Payloads.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public sealed class MyQueuedPlayer : QueuedLavalinkPlayer
{
    private readonly DiscordChannel _textChannel;
    private DiscordMessage _message;
    private DiscordMessage response;
    public MyQueuedPlayer(IPlayerProperties<MyQueuedPlayer, MyQueuePlayerOptions> properties) 
        : base(properties)
    {
        _textChannel = properties.Options.Value.TextChannel;
    }

    protected override async ValueTask NotifyTrackEndedAsync(ITrackQueueItem queueItem, TrackEndReason endReason, CancellationToken cancellationToken = default)
    {
        await base
            .NotifyTrackEndedAsync(queueItem, endReason, cancellationToken)
            .ConfigureAwait(false);

        await _message.DeleteAsync().ConfigureAwait(false);
    }

    protected override async ValueTask NotifyTrackStartedAsync(ITrackQueueItem track, CancellationToken cancellationToken = default)
    {
        await base
            .NotifyTrackStartedAsync(track, cancellationToken)
            .ConfigureAwait(false);

        var shuffleBtn = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "shuffleBtn", "Shuffle");
        var skipBtn = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "skipBtn", "Skip");
        var embedMusic = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Green,
            Title = "Now Playing",
            ImageUrl = track.Track.ArtworkUri.ToString(),
            Description = $"{track.Track.Title} by: {track.Track.Author}\n",
        };

        response = await _textChannel
       .SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embedMusic).AddComponents(skipBtn, shuffleBtn))
       .ConfigureAwait(false);

        _message = response;

        ApplicationHost.client.ComponentInteractionCreated += Client_ComponentInteractionCreated;

    }
    public void ShuffleSong()
    {
        Shuffle = !Shuffle;
    }

    private async Task Client_ComponentInteractionCreated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs args)
    {
        switch (args.Interaction.Data.CustomId)
        {
            case "skipBtn":
                await _textChannel.DeleteMessageAsync(response).ConfigureAwait(false);
                await this.SkipAsync().ConfigureAwait(false);
                await args.Interaction.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Song skipped"))
                    .ConfigureAwait(false);
                await args.Interaction.DeleteOriginalResponseAsync().ConfigureAwait(false);
                break;
            case "shuffleBtn":
                Shuffle = !Shuffle;
                await args.Interaction.CreateResponseAsync(
                   InteractionResponseType.ChannelMessageWithSource,
                   new DiscordInteractionResponseBuilder().WithContent("Shuffled"))
                   .ConfigureAwait(false);
                //await args.Interaction.DeleteOriginalResponseAsync().ConfigureAwait(false);
                break;
        }
    }
}
