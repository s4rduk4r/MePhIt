using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MePhIt.Commands
{
    [Group("set")]
    [Aliases("настрой", "настр")]
    [Description("Настройка параметров бота")]
    [RequirePermissions(Permissions.Administrator)]
    class CommandsSetup : BaseCommandModule
    {
        private MePhItBot Bot = MePhItBot.Bot;
        private MePhItSettings Settings = MePhItBot.Bot.Settings;
        private MePhItLocalization Localization = MePhItBot.Bot.Settings.Localization;

        [Command("lang")]
        [Aliases("язык")]
        [Description("Настроить язык")]
        public async Task Language(CommandContext commandContext, [Description("Буквенный код языка. Например: ru, en")] string language = null)
        {
            if(language == null)
            {
                language = MePhItBot.Bot.Settings.Localization.GetLanguageById(MePhItBot.Bot.Settings.LanguageDefault);
            }
            var languages = Localization.GetLanguages();
            if(languages.Contains(language))
            {
                Localization.Language[commandContext.Guild] = Localization.GetLanguageId(language);
                
                try
                {
                    SaveToSettingsFile(commandContext.Guild);
                }
                catch (Exception e)
                {
                    commandContext.Channel.SendMessageAsync($"{MePhItUtilities.EmojiError} {e.Message}\n{e.StackTrace}");
                }

                commandContext.Message.CreateReactionAsync(Bot.ReactSuccess);
                return;
            }
            commandContext.Message.CreateReactionAsync(Bot.ReactFail);
        }

        [Command("timezone")]
        [
            Aliases("tz", 
                    "время",
                    "вз")
        ]
        [Description("Настройка временной зоны")]
        public async Task TimeZone(CommandContext commandContext, [Description("Часовой пояс")] params string[] timeZoneId)
        {
            if(timeZoneId.Length == 0)
            {
                var msg = $"{MePhItUtilities.EmojiInfo} **Available Timezones:**\n";
                foreach(var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    msg += string.Format("{0}\n", tz.DisplayName);
                }
                MePhItUtilities.SendBigMessage(commandContext.Channel, msg);
                commandContext.Message.CreateReactionAsync(Bot.ReactSuccess);
                return;
            }

            var tzId = "";
            for(int i = 0; i < timeZoneId.Length; i++)
            {
                tzId += timeZoneId[i] + (i < timeZoneId.Length - 1 ? " " : "");
            }

            try
            {
                foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (tz.DisplayName == tzId)
                    {
                        Settings.TimeZone[commandContext.Guild] = tz;
                    }
                }

                try
                {
                    SaveToSettingsFile(commandContext.Guild);
                }
                catch(Exception e)
                {
                    commandContext.Channel.SendMessageAsync($"{MePhItUtilities.EmojiError} {e.Message}\n{e.StackTrace}");
                }

                commandContext.Message.CreateReactionAsync(Bot.ReactSuccess);
            }
            catch(Exception e)
            {
                commandContext.Message.RespondAsync($"{MePhItUtilities.EmojiCritical} {e.Message}\n{e.StackTrace}");
                commandContext.Message.CreateReactionAsync(Bot.ReactFail);
            }
        }

        private void SaveToSettingsFile(DiscordGuild server)
        {
            LanguageID lang;
            TimeZoneInfo timezone;
            try
            {
                lang = Localization.Language[server];
            }
            catch
            {
                lang = Settings.LanguageDefault;
            }
            try
            {
                timezone = Settings.TimeZone[server];
            }
            catch
            {
                timezone = TimeZoneInfo.Utc;
            }
            var settings = new Setup.SettingsSerializable();
            settings.Initialize(server.Id, lang, timezone);
            Setup.SettingsSerializable.Serialize(settings);
        }

        /// <summary>
        /// Enable Sync MyTest directory with remote cloud
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="state">on - enable sync; off - disable sync</param>
        /// <returns></returns>
        [Command("sync")]
        [Aliases("синх")]
        [Description("Синхронизировать папку с тестами и файлы в папке на Яндекс.Диске")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestSyncFilesFromYandexDisk(CommandContext commandContext, string state)
        {
            throw new NotImplementedException();
        }
    }
}
