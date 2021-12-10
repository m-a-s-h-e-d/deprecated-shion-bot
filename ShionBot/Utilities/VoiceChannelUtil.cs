using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Schema;
using Serilog;


namespace ShionBot.Utilities
{
    public class VoiceChannelUtil
    {
        // Yes I will eventually add this to the database instead of using a hashset
        private static HashSet<ulong> _channelSet = new();

        public static async Task HandleCreatePrivateRoom(SocketUser incomingUser, SocketGuild targetGuild)
        {
            var user = (SocketGuildUser) incomingUser;
            var newPrivateChannel = await CreateNewPrivateChannel(user, targetGuild);
            await AddPrivateChannelPerms(user, newPrivateChannel);
            _channelSet.Add(newPrivateChannel.Id);
            await Task.Delay(500);
            await user.ModifyAsync(x => { x.ChannelId = newPrivateChannel.Id; });
        }

        public static async Task CheckPrivateRoom(SocketGuildChannel channel)
        {
            var id = channel.Id;
            if (_channelSet.Contains(id))
            {
                if (channel.Users.Count == 0)
                {
                    await Task.Delay(500);
                    await channel.DeleteAsync();
                    _channelSet.Remove(id);
                }
            }
        }

        private static async Task<RestVoiceChannel> CreateNewPrivateChannel(IUser user, SocketGuild guild)
        {
            var newChannelName = $"{user.Username}'s room";
            var categoryId = guild.CategoryChannels.FirstOrDefault(category => category.Name.Equals("Private Rooms"))?.Id;
            if (categoryId == null)
            {
                throw new ArgumentNullException();
            }
            var newChannel = await guild.CreateVoiceChannelAsync(newChannelName, x => x.CategoryId = categoryId);
            return newChannel;
        }

        private static async Task AddPrivateChannelPerms(IUser user, RestGuildChannel channel)
        {
            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(connect: PermValue.Allow, moveMembers: PermValue.Allow));
        }
    }
}
