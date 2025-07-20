using DSharpPlus.Entities;
using Lavalink4NET.Players.Queued;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed record class MyQueuePlayerOptions : QueuedLavalinkPlayerOptions
{
    public DiscordChannel TextChannel { get; set; }
}