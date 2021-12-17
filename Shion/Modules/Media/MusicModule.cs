using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Shion.Modules.Utility;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace Shion.Modules.Media
{
    public class MusicModule : ShionModuleBase
    {
        private readonly ILogger<GeneralModule> logger;
        private readonly LavaNode lavaNode;

        public MusicModule(ILogger<GeneralModule> logger, LavaNode lavaNode)
        {
            this.logger = logger;
            this.lavaNode = lavaNode;
        }

        [Command("Join")]
        public async Task JoinAsync()
        {
            if (this.lavaNode.HasPlayer(this.Context.Guild))
            {
                await this.ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = this.Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await this.ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await this.lavaNode.JoinAsync(voiceState.VoiceChannel, this.Context.Channel as ITextChannel);
                await this.ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await this.ReplyAsync(exception.Message);
            }
        }

        [Command("Play")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await this.ReplyAsync("Please provide search terms.");
                return;
            }

            if (!this.lavaNode.HasPlayer(this.Context.Guild))
            {
                await this.ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var queries = searchQuery.Split(' ');
            foreach (var query in queries)
            {
                var searchResponse = await this.lavaNode.SearchYouTubeAsync(query);
                if (searchResponse.Status == SearchStatus.LoadFailed ||
                    searchResponse.Status == SearchStatus.NoMatches)
                {
                    await this.ReplyAsync($"I wasn't able to find anything for `{query}`.");
                    return;
                }

                var player = this.lavaNode.GetPlayer(this.Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            player.Queue.Enqueue(track);
                        }

                        await this.ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        var track = searchResponse.Tracks.FirstOrDefault();
                        player.Queue.Enqueue(track);
                        await this.ReplyAsync($"Enqueued: {track.Title}");
                    }
                }
                else
                {
                    var track = searchResponse.Tracks.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (var i = 0; i < searchResponse.Tracks.Count; i++)
                        {
                            if (i == 0)
                            {
                                await player.PlayAsync(track);
                                await this.ReplyAsync($"Now Playing: {track.Title}");
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks.ElementAt(i));
                            }
                        }

                        await this.ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        await this.ReplyAsync($"Now Playing: {track.Title}");
                    }
                }
            }
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason != TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
                return;
            }

            if (queueable is not LavaTrack track)
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }
    }
}
