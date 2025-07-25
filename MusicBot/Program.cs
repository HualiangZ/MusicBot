﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicBot.Commands;
using MusicBot.Commands.SlashCommands;
using MusicBot.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

var jsonReader = new JsonReader();
await jsonReader.ReadJson();

var builder = new HostApplicationBuilder(args);
builder.Services.AddHostedService<ApplicationHost>();
builder.Services.AddSingleton<DiscordClient>();
builder.Services.AddSingleton(new DiscordConfiguration { Token = jsonReader.token });
builder.Services.AddLavalink();

builder.Services.AddLogging(x => x
    .AddConsole()
    .AddFilter("Microsoft.Extensions.Http.DefaultHttpClientFactory", LogLevel.Warning)
    .SetMinimumLevel(LogLevel.Trace));

builder.Services.AddLogging(s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));
builder.Build().Run();

public class ApplicationHost : BackgroundService 
{ 
    private readonly IServiceProvider provider;
    public static DiscordClient client;

    public ApplicationHost(IServiceProvider serviceProvider, DiscordClient discordClient)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(discordClient);

        this.provider = serviceProvider;
        client = discordClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jsonReader = new JsonReader();
        await jsonReader.ReadJson();
        client
            .UseSlashCommands(new SlashCommandsConfiguration { Services = provider })
            .RegisterCommands<MusicSlashCommands>(jsonReader.guildId);
        client.UseInteractivity(new InteractivityConfiguration()
        {
            Timeout = TimeSpan.FromMinutes(5),
        });
        
        await client
            .ConnectAsync()
            .ConfigureAwait(false);

        var readyTaskCompletionSource = new TaskCompletionSource();

        Task SetResult(DiscordClient client, ReadyEventArgs eventArgs)
        {
            readyTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        }

        client.Ready += SetResult;
        await readyTaskCompletionSource.Task.ConfigureAwait(false);
        client.Ready -= SetResult;

        await Task
            .Delay(Timeout.InfiniteTimeSpan, stoppingToken)
            .ConfigureAwait(false);

    }

}

