using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MePhIt
{
    /// <summary>
    /// Helper methods to ease routine tasks
    /// </summary>
    internal static class MePhItUtilities
    {
        public static int DISCORD_MESSAGE_SIZE_LIMIT = 2000;

        // ------------ MESSAGE MANAGEMENT ------------
        public static async Task SendBigMessage(DiscordChannel channel, string msg)
        {
            if (msg.Length > DISCORD_MESSAGE_SIZE_LIMIT)
            {
                var msgToSend = "";
                foreach (var m in msg.Split("\n"))
                {
                    msgToSend += $"{m}\n";
                    if (msgToSend.Length > 0.9 * DISCORD_MESSAGE_SIZE_LIMIT)
                    {
                        channel.SendMessageAsync(msgToSend);
                        msgToSend = "";
                    }
                }
            }
            else
            {
                channel.SendMessageAsync(msg);
            }
        }

        // ------------ CHANNEL MANAGEMENT ------------
        /// <summary>
        /// Create temporary channels for specific server members
        /// </summary>
        /// <param name="server">Server where to create new channels</param>
        /// <param name="categoryName">Category channel into which new channels should be put</param>
        /// <param name="channelNames">Tuple with desired channel names, their types and who this channel is created for</param>
        /// <returns></returns>
        public static async Task<(DiscordChannel CategoryChannel, IEnumerable<(DiscordMember Member, DiscordChannel Channel)> NestedChannels)>
            CreateTemproraryChannelsAsync(DiscordGuild server, string categoryName, IEnumerable<(DiscordMember Member, bool IsAudio)> channelNames)
        {
            var tempChannels = new BlockingCollection<(DiscordMember Member, DiscordChannel Channel)>();
            DiscordChannel category = null;
            try
            {
                category = await server.CreateChannelCategoryAsync(categoryName);
            }
            catch (Exception e)
            {
            }
            finally
            {
                DiscordChannel channel = null;
                foreach (var ch in channelNames)
                {
                    if (ch.IsAudio)
                    {
                        channel = await server.CreateVoiceChannelAsync(ch.Member.DisplayName, category);
                    }
                    else
                    {
                        channel = await server.CreateTextChannelAsync(ch.Member.DisplayName, category);
                    }
                    if (channel != null)
                    {
                        tempChannels.Add((ch.Member, channel));
                    }
                }
            }

            return (category, tempChannels);
        }

        /// <summary>
        /// Delete temporary channels
        /// </summary>
        /// <param name="channels">Channels to delete</param>
        public static async Task DeleteTemproraryChannelsAsync(IEnumerable<DiscordChannel> channels)
        {
            foreach (var ch in channels)
            {
                ch.DeleteAsync();
            }
        }

        /// <summary>
        /// Get channel mentions
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetChannelMentions(IEnumerable<DiscordChannel> channels)
        {
            var channelNames = new List<string>();
            foreach(var ch in channels)
            {
                channelNames.Add(ch.Mention);
            }
            return channelNames;
        }

        // ------------ ROLE MANAGEMENT ------------
        /// <summary>
        /// Create temporary role on the server for a specific user
        /// </summary>
        /// <param name="server">Discord server</param>
        /// <param name="user">Server member</param>
        /// <returns></returns>
        public static async Task<DiscordRole> CreateTemporaryRoleAsync(DiscordGuild server, DiscordUser user)
        {
            var tempRoleName = user.Username;
            var tempRolePermissions = Permissions.AccessChannels | Permissions.SendMessages | Permissions.ReadMessageHistory;
            return await server.CreateRoleAsync(tempRoleName, tempRolePermissions, null, false, false);
        }

        /// <summary>
        /// Create temporary roles on the server for the multiple users
        /// </summary>
        /// <param name="server">Discord server</param>
        /// <param name="users">User list</param>
        /// <returns></returns>
        public static async Task<IEnumerable<(DiscordUser User, DiscordRole Role)>> CreateTemporaryRolesAsync(DiscordGuild server, IEnumerable<DiscordUser> users)
        {
            var tempRoles = new List<(DiscordUser User, DiscordRole Role)>();
            foreach(var user in users)
            {
                var tempRoleName = user.Username;
                var tempRolePermissions = Permissions.AccessChannels | Permissions.SendMessages | Permissions.ReadMessageHistory;
                var tempRole = await server.CreateRoleAsync(tempRoleName, tempRolePermissions, null, false, false);
                tempRoles.Add((user, tempRole));
            }

            return tempRoles;
        }

        /// <summary>
        /// Delete temporary roles
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static async Task DeleteTemporaryRolesAsync(IEnumerable<DiscordRole> roles)
        {
            foreach(var role in roles)
            {
                role.DeleteAsync();
            }
        }

        /// <summary>
        /// Gets the *student* role on specific server
        /// </summary>
        /// <returns></returns>
        public static DiscordRole GetRoleStudent(in DiscordGuild server)
        {
            var localization = MePhItBot.Bot.Settings.Localization;
            var roleStudentName = localization.Message(server, MessageID.CmdSrvrOrgRoleStudent);
            foreach(var role in server.Roles)
            {
                if(role.Value.Name == roleStudentName)
                {
                    return role.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the *teacher* role on specific server
        /// </summary>
        /// <returns></returns>
        public static DiscordRole GetRoleTeacher(in DiscordGuild server)
        {
            var localization = MePhItBot.Bot.Settings.Localization;
            var roleTeacherName = localization.Message(server, MessageID.CmdSrvrOrgRoleTeacher);
            foreach (var role in server.Roles)
            {
                if (role.Value.Name == roleTeacherName)
                {
                    return role.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the *group-leader* role on specific server
        /// </summary>
        /// <returns></returns>
        public static DiscordRole GetRoleGroupLeader(in DiscordGuild server)
        {
            var localization = MePhItBot.Bot.Settings.Localization;
            var roleGroupLeaderName = localization.Message(server, MessageID.CmdSrvrOrgRoleGroupLeader);
            foreach (var role in server.Roles)
            {
                if (role.Value.Name == roleGroupLeaderName)
                {
                    return role.Value;
                }
            }
            return null;
        }


        // ------------ STUDENTS ------------

        /// <summary>
        /// Gets student's list on the server
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<(DiscordUser User, bool IsOnline)>> GetStudentsAsync(DiscordGuild server)
        {
            var students = new List<(DiscordUser, bool)>();
            var roleStudent = GetRoleStudent(server);
            var users = await server.GetAllMembersAsync();
            foreach (var user in users)
            {
                if (user.IsBot) continue;
                foreach(var role in user.Roles)
                {
                    if(role == roleStudent)
                    {
                        students.Add((user, user.Presence != null && user.Presence.Status != UserStatus.Offline));
                        break;
                    }
                }
            }
            return students;
        }

        /// <summary>
        /// Gets online students only
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<DiscordUser>> GetStudentsOnlineAsync(DiscordGuild server)
        {
            var students = await GetStudentsAsync(server);
            var studentsOnline = new List<DiscordUser>();
            foreach(var student in students)
            {
                if(student.IsOnline)
                {
                    studentsOnline.Add(student.User);
                }
            }
            return studentsOnline;
        }

        /// <summary>
        /// Check if student is online
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        public static bool IsOnline(DiscordMember student)
        {
            return (student.Presence != null && student.Presence.Status != UserStatus.Offline);
        }

        // ------------ EMOJI ------------

        /// <summary>
        /// Convert integer in range [0; 10] to emoji
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NumberToEmoji(in int number)
        {
            var emoji = "";
            switch(number)
            {
                case 0:
                    emoji = ":zero:";
                    break;
                case 1:
                    emoji = ":one:";
                    break;
                case 2:
                    emoji = ":two:";
                    break;
                case 3:
                    emoji = ":three:";
                    break;
                case 4:
                    emoji = ":four:";
                    break;
                case 5:
                    emoji = ":five:";
                    break;
                case 6:
                    emoji = ":six:";
                    break;
                case 7:
                    emoji = ":seven:";
                    break;
                case 8:
                    emoji = ":eight:";
                    break;
                case 9:
                    emoji = ":nine:";
                    break;
                case 10:
                    emoji = ":keycap_ten:";
                    break;
                default:
                    break;
            }
            return emoji;
        }

        /// <summary>
        /// Convert numeric emoji to number
        /// </summary>
        /// <param name="emoji"></param>
        /// <returns></returns>
        public static int EmojiToNumber(in DiscordClient client, in string emoji)
        {
            for (int i = 0; i <= 10; i++)
            {
                var controlEmoji = DiscordEmoji.FromName(client, NumberToEmoji(i));
                if (controlEmoji == emoji)
                {
                    return i;
                }
            }

            throw new NotSupportedException("Unknown emoji");
        }

        /// <summary>
        /// Convert numeric emoji to number
        /// </summary>
        /// <param name="emoji"></param>
        /// <returns></returns>
        public static int EmojiToNumber(in DiscordClient client, in DiscordEmoji emoji)
        {
            for(int i = 0; i <= 10; i++)
            {
                var controlEmoji = DiscordEmoji.FromName(client, NumberToEmoji(i));
                if(controlEmoji == emoji)
                {
                    return i;
                }
            }

            throw new NotSupportedException("Unknown emoji");
        }

        // Emojis for different types of messages
        public static string EmojiInfo = ":information_source:";
        public static string EmojiWarning = ":warning:";
        public static string EmojiCritical = ":exclamation:";
        public static string EmojiError = ":bangbang:";

    }
}
