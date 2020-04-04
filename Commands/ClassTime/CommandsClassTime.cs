using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MePhIt.Commands.ClassTime;

namespace MePhIt.Commands
{
    [Group("class")]
    [Aliases("занятие")]
    public class CommandsClassTime : BaseCommandModule
    {
        private IDictionary<DiscordGuild, ClassTimeSettings> LocalSettings = new ConcurrentDictionary<DiscordGuild, ClassTimeSettings>();
        private MePhItLocalization Localization = MePhItBot.Bot.Settings.Localization;

        [Command("ru")]
        [Aliases("ru_RU", "russian", "ру", "русский")]
        [Description("Установить **русский** язык сообщений")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task LanguageSelect_ru_RU(CommandContext commandContext)
        {
            ClassTimeSettings localSettings;
            if(LocalSettings.TryGetValue(commandContext.Guild, out localSettings))
            {
                localSettings.Language = LanguageID.ru_RU;
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
            }
            else
            {
                localSettings = new ClassTimeSettings(LocalSettings, commandContext.Channel);
                localSettings.Language = LanguageID.ru_RU;
                try
                {
                    LocalSettings[commandContext.Guild] = localSettings;
                    commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
                }
                catch(Exception e)
                {
                    var errMsg = await commandContext.Channel.SendMessageAsync(e.Message);
                    await errMsg.CreateReactionAsync(MePhItBot.Bot.ReactFail);
                }
            }
        }

        [Command("en")]
        [Aliases("en_US", "english", "англ", "английский")]
        [Description("Установить **английский** язык сообщений\nSet messages to **English US**")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task LanguageSelect_en_US(CommandContext commandContext)
        {
            ClassTimeSettings localSettings;
            if (LocalSettings.TryGetValue(commandContext.Guild, out localSettings))
            {
                localSettings.Language = LanguageID.en_US;
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
            }
            else
            {
                localSettings = new ClassTimeSettings(LocalSettings, commandContext.Channel);
                localSettings.Language = LanguageID.en_US;
                try
                {
                    LocalSettings[commandContext.Guild] = localSettings;
                    commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
                }
                catch (Exception e)
                {
                    var errMsg = await commandContext.Channel.SendMessageAsync(e.Message);
                    await errMsg.CreateReactionAsync(MePhItBot.Bot.ReactFail);
                }
            }
        }

        [Command("start")]
        [Aliases("старт")]
        [Description("Начать занятие")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Start(CommandContext commandContext,
                                DiscordChannel channel = null,   // Channel to output info messages
                                int durationTimeInMinutes = 150, // class longivity
                                int timeTillBreakInMinutes = 60,   // time before the break begins
                                int breakDurationInMinutes = 15, // break time length
                                int notifyBeforeEventInMinutes = 5) // ahead notify about the break and about the class end
        {
            ClassTimeSettings localSettings = null;
            channel = channel == null ? commandContext.Channel : channel;
            var server = commandContext.Guild;
            if (!LocalSettings.TryGetValue(server, out localSettings))
            {
                localSettings = new ClassTimeSettings(LocalSettings, channel,
                                         durationTimeInMinutes, timeTillBreakInMinutes, breakDurationInMinutes,
                                         notifyBeforeEventInMinutes);
                localSettings.Start();
                LocalSettings.Add(server, localSettings);
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
            }
            else
            {
                localSettings.ClassTimeInMinutes = durationTimeInMinutes;
                localSettings.TimeTillBreakInMinutes = timeTillBreakInMinutes;
                localSettings.BreakDurationInMinutes = breakDurationInMinutes;
                localSettings.NotifyBeforeClassEndInMinutes = notifyBeforeEventInMinutes;
                localSettings.Start();
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
                return;
            }
        }

        [Command("stop")]
        [Aliases("стоп")]
        [Description("Закончить занятие")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Stop(CommandContext commandContext)
        {
            var server = commandContext.Guild;
            ClassTimeSettings localSettings = null;
            if (LocalSettings.TryGetValue(server, out localSettings))
            {
                localSettings.Stop();
                LocalSettings.Remove(server);
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
            }
            else
            {
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
            }
        }

        [Command("break")]
        [Aliases("перерыв")]
        [Description("Сделать перерыв на занятии")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Break(CommandContext commandContext)
        {
            var server = commandContext.Guild;
            ClassTimeSettings localSettings = null;
            if (LocalSettings.TryGetValue(server, out localSettings))
            {
                localSettings.Break();
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
            }
            else
            {
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
            }
        }

        [Command("list")]
        [Aliases("список")]
        [Description("Сформировать список студентов, присутствующих на занятии. Студенты, находящиеся в оффлайне считаются пропустившими занятие")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task List(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel = channel == null ? commandContext.Channel : channel;

            MePhItLocalization localization = null;
            ClassTimeSettings localSettings = null;
            try
            {
                localSettings = LocalSettings[commandContext.Guild];
                localization = localSettings.Localization;
            }
            catch(Exception e)
            {
                LocalSettings[commandContext.Guild] = new ClassTimeSettings(LocalSettings, channel);
                commandContext.Message.CreateReactionAsync(DiscordEmoji.FromName(commandContext.Client, MePhItUtilities.EmojiWarning));
            }

            var students = await MePhItUtilities.GetStudentsAsync(commandContext.Guild);
            var memberList = "";
            var strAbsense = Localization.Message(commandContext.Guild, MessageID.ListAbsent);
            foreach(var student in students)
            {
                memberList += student.IsOnline ? student.User.Mention : string.Format(strAbsense, student.User.Mention);
                memberList += "\n";
            }

            commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);

            await channel.SendMessageAsync(Localization.Message(commandContext.Guild, MessageID.ListHeader));
            channel.SendMessageAsync(memberList);
        }

    }
}
