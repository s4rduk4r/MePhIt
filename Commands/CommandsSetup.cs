using System;
using System.Collections.Generic;
using System.Text;
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
                commandContext.Message.CreateReactionAsync(Bot.ReactSuccess);
                return;
            }
            commandContext.Message.CreateReactionAsync(Bot.ReactFail);
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
