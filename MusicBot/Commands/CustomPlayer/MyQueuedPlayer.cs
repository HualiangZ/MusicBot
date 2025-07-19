using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public sealed class MyQueuedPlayer : QueuedLavalinkPlayer
{
    private readonly DiscordChannel _textChannel;
    public MyQueuedPlayer(IPlayerProperties<MyQueuedPlayer, MyQueuePlayerOptions> properties) 
        : base(properties)
    {
        _textChannel = properties.Options.Value.TextChannel;
    }

    protected override async ValueTask NotifyTrackStartedAsync(ITrackQueueItem track, CancellationToken cancellationToken = default)
    {
        await base
            .NotifyTrackStartedAsync(track, cancellationToken)
            .ConfigureAwait(false);

        var skipBtn = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Secondary, "skipBtn", "Skip");
        var embedMusic = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Green,
            Title = "Now Playing",
            ImageUrl = track.Track.ArtworkUri.ToString(),
            Description = $"{track.Track.Title} by: {track.Track.Author}\n",
        };

        var response = await _textChannel
       .SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embedMusic).AddComponents(skipBtn))
       .ConfigureAwait(false);

        var interactWithSkipButton = await response.WaitForButtonAsync("skipBtn").ConfigureAwait(false);

        if (interactWithSkipButton.Result is not null)
        {
            await _textChannel.DeleteMessageAsync(response).ConfigureAwait(false);
            await this.SkipAsync();
        }

        
    }

}
