using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.IO;
using static MePhIt.MePhItUtilities;

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
        public IDictionary<DiscordGuild, TestState> TestStates = new ConcurrentDictionary<DiscordGuild, TestState>();

        /// <summary>
        /// Results of the test completion
        /// </summary>
        public IDictionary<DiscordGuild, TestResults> TestResults = new ConcurrentDictionary<DiscordGuild, TestResults>();

        // Shortcuts
        private MePhItBot Bot = MePhItBot.Bot;
        private MePhItSettings Settings = MePhItBot.Bot.Settings;
        private MePhItLocalization Localization = MePhItBot.Bot.Settings.Localization;

        // Local Settings
        /// <summary>
        /// Test channel group
        /// </summary>
        private IDictionary<DiscordGuild, IDictionary<DiscordMember, DiscordChannel>> tempTestChannelGrp = new ConcurrentDictionary<DiscordGuild, IDictionary<DiscordMember, DiscordChannel>>();
        /// <summary>
        /// Test question messages
        /// </summary>
        private IDictionary<DiscordGuild, IDictionary<DiscordMember, IList<DiscordMessage>>> tempTestChannelQuestions = new ConcurrentDictionary<DiscordGuild, IDictionary<DiscordMember, IList<DiscordMessage>>>();

        // Timeout before the test starts
        private int timeoutBeforeTestMinutes = 2;
        private string commandTestFinish = "finish";
        // Question text format
        private string questionTextFormat = ":question: **{0}**. *{1}?*";
        // Answer text format
        private string answerTextFormat = "{0} {1}";
        
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
        public async Task MyTestList(CommandContext commandContext, DiscordChannel channel = null)
        {
            channel = channel == null ? commandContext.Channel : channel;

            try
            {
                var msg = Localization.Message(commandContext.Guild, MessageID.CmdMyTestTestsSearch) + "\n";

                // List files in nested directories
                var availableTests = new List<string>(Directory.GetFiles(Settings.MyTestFolder, "*.mtc", SearchOption.AllDirectories));

                // Show found test files
                if (availableTests.Count == 0)
                {
                    msg = $"{msg} {Localization.Message(commandContext.Guild, MessageID.CmdMyTestTestsNotFound)}";
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
                commandContext.Message.CreateReactionAsync(Bot.ReactSuccess);
            }
            catch(Exception e)
            {
                await channel.SendMessageAsync($":bangbang: {e.Message}");
                commandContext.Message.CreateReactionAsync(Bot.ReactFail);
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
                var msg = string.Format(Localization.Message(commandContext.Guild, MessageID.CmdMyTestFileLoadSuccess), test.Name);
                await commandContext.Channel.SendMessageAsync(msg);
                commandContext.Message.CreateReactionAsync(Bot.ReactSuccess);
            }
            catch(Exception e)
            {
                await commandContext.Channel.SendMessageAsync($":bangbang: {e.Message}");
                commandContext.Message.CreateReactionAsync(Bot.ReactFail);
            }
        }

        [Command("start")]
        [Aliases("старт")]
        [Description("Начать тестирование")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestStart(CommandContext commandContext, DiscordChannel channelInfoMessages = null)
        {
            channelInfoMessages = channelInfoMessages == null ? commandContext.Channel : channelInfoMessages;

            // 1. Get loaded test
            TestState test;
            if(!TestStates.TryGetValue(commandContext.Guild, out test))
            {
                await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
                return;
            }
            // 2. Get a list of students online
            var students = await GetStudentsOnlineAsync(commandContext.Guild);

            // 3. Create test channels personal for each student
            // Permission to read only for this specific student
            // 3.1. Create MyTest channel category
            // 3.2. Create text channels with names corresponding to student's displayed name
            // 3.3 Store created temporary channels
            tempTestChannelGrp[commandContext.Guild] = await CreateTemporaryTestChannelGroup(commandContext, students);
            var tempChannels = tempTestChannelGrp[commandContext.Guild];

            // 3.4. Inform students to join their corresponding channels
            var channelMentions = "";
            foreach (var mention in GetChannelMentions(tempTestChannelGrp[commandContext.Guild].Values))
            {
                channelMentions += mention;
            }
            // Generic test info
            var msg = string.Format(
                                    Localization.Message(commandContext.Guild, MessageID.CmdMyTestStartHelp),
                                    channelMentions,
                                    Settings.Discord.CurrentUser.Mention,
                                    commandTestFinish,
                                    timeoutBeforeTestMinutes
                                    ) + "\n";
            await channelInfoMessages.SendMessageAsync($"{commandContext.Guild.EveryoneRole.Mention}\n{msg}");


            // 4. Throw all of the questions at their channels
            // Remember message IDs to read student's answers from them
            // Test name and time
            msg = $"{EmojiInfo} {test.Name}\n";
            var testTime = test.Time != TestState.TimeInfinite ? new TimeSpan(0, 0, test.Time).TotalMinutes.ToString() : "∞";
            msg += string.Format(Localization.Message(commandContext.Guild, MessageID.CmdMyTestStartTime), testTime) + "\n";
            foreach (var tempChannel in tempChannels)
            {
                tempChannel.Value.SendMessageAsync($"{tempChannel.Key.Mention}\n{msg}");
            }
            
            // Send Questions to the test channels
            var questions = test.Questions;
            foreach (var question in questions)
            {
                // Question
                msg = string.Format($"{questionTextFormat}\n", questions.IndexOf(question) + 1, question.Text);
                // Answers
                foreach (var answer in question.Answers)
                {
                    msg += string.Format(answerTextFormat, NumberToEmoji(question.Answers.IndexOf(answer) + 1), answer.Text) + "\n";
                }
                foreach(var tempChannel in tempChannels)
                {
                    var message = await tempChannel.Value.SendMessageAsync(msg);
                    for (int i = 1; i <= question.Answers.Count; i++)
                    {
                        await message.CreateReactionAsync(DiscordEmoji.FromName(Settings.Discord, NumberToEmoji(i)));
                    }
                }
            }

            // 5. Start test timer

            // 6. Register callback for test end event

            // 7. Register test start to enable the *ready* command from the student

            // 8. At the end of the test collect all the reactions from the students 
            // and process them as answers

            // 9. Present the question-answer statistics to the students

            // 10. Present the question-answer statistics to the teacher

            // 11. Form the marks earned
            throw new NotImplementedException();
        }

        private async Task<IDictionary<DiscordMember, DiscordChannel>> 
            CreateTemporaryTestChannelGroup(CommandContext commandContext, IEnumerable<DiscordUser> students)
        {
            var tempCategoryName = Localization.Message(commandContext.Guild, MessageID.CmdMyTestStartTempCategoryName);
            var tempChannelNames = new List<(DiscordMember, bool)>();
            foreach (var student in students)
            {
                tempChannelNames.Add((student as DiscordMember, false));
            }

            var tempTestChannels = new Dictionary<DiscordMember, DiscordChannel>();

            var tempChannels = await CreateTemproraryChannelsAsync(commandContext.Guild, tempCategoryName, tempChannelNames);
            // Close channels to everyone except the tested person
            foreach (var tmpCh in tempChannels.NestedChannels)
            {
                await tmpCh.Channel.AddOverwriteAsync(commandContext.Guild.EveryoneRole, Permissions.None, Permissions.All);
                await tmpCh.Channel.AddOverwriteAsync(tmpCh.Member, Permissions.AccessChannels | Permissions.ReadMessageHistory, Permissions.All);
                tempTestChannels[tmpCh.Member] = tmpCh.Channel;
            }
            return tempTestChannels;
        }

        [Command("stop")]
        [Aliases("стоп")]
        [Description("Прекратить тестирование")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestStop(CommandContext commandContext)
        {
            // 1. Stop test timer
            // 2. Delete all bot messages from all opened DM channels
            throw new NotImplementedException();
        }

        [Command("results")]
        [Aliases("результаты")]
        [Description("Показать результаты тестирования")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task MyTestResults(CommandContext commandContext, bool brief = false)
        {
            throw new NotImplementedException();
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
