using System;
using System.Collections.Generic;
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
        // ------------ ROLE MANAGEMENT ------------

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
        public static async Task<IEnumerable<(DiscordUser User, bool IsOnline)>> GetStudents(DiscordGuild server)
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
                        students.Add((user, user.Presence.Status != UserStatus.Offline));
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
        public static async Task<IEnumerable<DiscordUser>> GetStudentsOnline(DiscordGuild server)
        {
            var students = await GetStudents(server);
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


        // ------------ EMOJI ------------

        /// <summary>
        /// Convert integer in range [0; 10] to emoji
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NumberToEmoji(int number)
        {
            var emoji = "";
            switch(number)
            {
                case 0:
                    emoji = ":one:";
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
                    emoji = ":keycap_ten: ";
                    break;
                default:
                    break;
            }
            return emoji;
        }

        // Emojis for different types of messages
        public static string EmojiInfo = ":information_source:";
        public static string EmojiWarning = ":warning:";
        public static string EmojiCritical = ":exclamation:";
        public static string EmojiError = ":bangbang:";

    }
}
