using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using System.IO;

using MyTestLib;

namespace MePhIt.Commands
{

    [Group("mytest")]
    [
        Aliases("mt",
                "тест")
    ]
    [Description("Управление поведением MyTest")]
    public class CommandsMyTest : BaseCommandModule
    {
        public IDictionary<DiscordGuild, TestState> TestStates = new Dictionary<DiscordGuild, TestState>();
        private MePhItSettings Settings = MePhItBot.Bot.Settings;
        private MePhItLocalization Localization = MePhItBot.Bot.Settings.Localization;
        
        /// <summary>
        /// List available test files
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="channel">Channel where to show findings</param>
        /// <returns></returns>
        [Command("tests")]
        [Aliases("тесты")]
        [Description("Показать доступные файлы тестов")]
        public async Task MyTestTests(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel = channel == null ? commandContext.Channel : channel;

            var msg = Localization.Message(Settings.LanguageDefault, MessageID.CmdMyTestTestsSearch) + "\n";

            try
            {
                var availableTests = Directory.GetFiles(Settings.MyTestFolder, ".mtc");
                if (availableTests == null || availableTests.Length == 0)
                {
                    msg = $"{msg} {Localization.Message(Settings.LanguageDefault, MessageID.CmdMyTestTestsNotFound)}";
                }
                else
                {
                    foreach (var test in availableTests)
                    {
                        msg = $"{msg}{Path.GetFileName(test)}\n";
                    }
                }

                await channel.SendMessageAsync(msg);
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
            }
            catch(Exception e)
            {
                await channel.SendMessageAsync($":bangbang: {e.Message}");
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
            }
        }

        /// <summary>
        /// Select specific test file
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        [Command("file")]
        [Aliases("файл")]
        [Description("Выбрать файл теста из установленных в системе")]
        public async Task MyTestFile(CommandContext commandContext)
        {
            // TODO:
            throw new NotImplementedException();
        }


        [Command("sync")]
        [Aliases("синх")]
        [Description("Синхронизировать папку с тестами и файлы в папке на Яндекс.Диске")]
        public async Task MyTestSyncFilesFromYandexDisk(CommandContext commandContext)
        {
            throw new NotImplementedException();
        }
    }
}
