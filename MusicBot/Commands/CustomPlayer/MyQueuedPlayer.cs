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
    public MyQueuedPlayer(IPlayerProperties<QueuedLavalinkPlayer, QueuedLavalinkPlayerOptions> properties) 
        : base(properties)
    {
    }

}
