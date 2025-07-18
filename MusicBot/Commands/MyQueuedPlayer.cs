using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBot.Commands
{
    public class MyQueuedPlayer : QueuedLavalinkPlayer
    {
        public DiscordChannel channel {  get; set; }
        public MyQueuedPlayer(IPlayerProperties<QueuedLavalinkPlayer, QueuedLavalinkPlayerOptions> properties, DiscordChannel channel) : base(properties)
        {
            this.channel = channel;
        }

    }
}
