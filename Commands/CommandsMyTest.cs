using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
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
    [RequirePermissions(Permissions.Administrator)]
    public class CommandsMyTest : BaseCommandModule
    {
        /// <summary>
        /// Ongoing MyTest tests
        /// </summary>
        public IDictionary<DiscordGuild, TestState> TestStates = new Dictionary<DiscordGuild, TestState>();
        /// <summary>
        /// MyTest filepaths holder
        /// </summary>
        public IDictionary<DiscordGuild, IList<string>> TestFiles = new Dictionary<DiscordGuild, IList<string>>();

        private MePhItSettings Settings = MePhItBot.Bot.Settings;
        private MePhItLocalization Localization = MePhItBot.Bot.Settings.Localization;
        
        /// <summary>
        /// List available test files
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="channel">Channel where to show findings</param>
        /// <returns></returns>
        [Command("list")]
        [Aliases("список")]
        [Description("Показать доступные файлы тестов")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestTests(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel = channel == null ? commandContext.Channel : channel;

            var msg = Localization.Message(Settings.LanguageDefault, MessageID.CmdMyTestTestsSearch) + "\n";

            try
            {
                // List files in nested directories
                var availableTests = new List<string>(Directory.GetFiles(Settings.MyTestFolder, "*.mtc", SearchOption.AllDirectories));

                // Show found test files
                if (availableTests.Count == 0)
                {
                    msg = $"{msg} {Localization.Message(Settings.LanguageDefault, MessageID.CmdMyTestTestsNotFound)}";
                }
                else
                {
                    // Convert filepaths to Unix
                    foreach (var test in availableTests)
                    {
                        var relPath = Path.GetRelativePath(Settings.MyTestFolder, test).Split(Path.DirectorySeparatorChar);
                        string relPathFixed = "";
                        foreach(var rPath in relPath)
                        {
                            var sep = rPath != relPath[relPath.Length - 1] ? Path.AltDirectorySeparatorChar.ToString() : "";
                            relPathFixed += $"{rPath}{sep}";
                        }
                        msg = $"{msg}{relPathFixed}\n";
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
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestFile(CommandContext commandContext, params string[] filepath)
        {
            var filePath = "";
            foreach (var str in filepath)
            {
                filePath += str + (str != filepath[filepath.Length - 1] ? " " : "");
            }

            try
            {
                filePath = Path.Combine(Settings.MyTestFolder, filePath);
                var test = new TestState();
                test.LoadTest(filePath);
                TestStates[commandContext.Guild] = test;
                var msg = string.Format(Localization.Message(Settings.LanguageDefault, MessageID.CmdMyTestFileLoadSuccess), test.Name);
                await commandContext.Channel.SendMessageAsync(msg);                
            }
            catch(Exception e)
            {
                await commandContext.Channel.SendMessageAsync($":bangbang: {e.Message}");
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
            }
        }

        [Command("start")]
        [Aliases("старт")]
        [Description("Начать тестирование")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestStart(CommandContext commandContext, DiscordChannel channel)
        {
            // 1. Get loaded test
            TestState test;
            if(!TestStates.TryGetValue(commandContext.Guild, out test))
            {
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
                return;
            }
            // 2. Get a list of students online

            // 3. Open DM channel to each student

            // 4. Throw all of the questions at them.
            // Remember message IDs to read student's answers from them

            // 5. Start test timer

            // 6. Register callback for test end event

            // 7. Register test start to enable the *ready* command from the student

            // 8. At the end of the test collect all the reactions from the students 
            // and process them as answers

            // 9. Present the question-answer statistics to the students

            // 10. Present the question-answer statistics to the teacher

            // 11. Form the marks earned
        }

        /// <summary>
        /// Sync MyTest directory with remote cloud
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        [Command("sync")]
        [Aliases("синх")]
        [Description("Синхронизировать папку с тестами и файлы в папке на Яндекс.Диске")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestSyncFilesFromYandexDisk(CommandContext commandContext)
        {
            throw new NotImplementedException();
        }
    }
}
