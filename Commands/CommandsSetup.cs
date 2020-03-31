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
    [Aliases("настройка", "настр")]
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
        public async Task Language(CommandContext commandContext, string language = "ru_RU")
        {
            var languages = Localization.GetLanguages();
            if(languages.Contains(language))
            {
                Localization.Language[commandContext.Guild] = Localization.GetLanguageId(language);
                commandContext.Message.CreateReactionAsync(Bot.ReactSuccess);
                return;
            }
            commandContext.Message.CreateReactionAsync(Bot.ReactFail);
        }

    }
}
