using DSharpPlus.Entities;
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

}
