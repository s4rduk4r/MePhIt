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
        
        [Command("tests")]
        [Aliases("тесты")]
        [Description("Показать доступные файлы тестов")]
        public async Task MyTestTests(CommandContext commandContext)
        {
            IList<string> availableTests = new List<string>();
        }

        [Command("file")]
        [Aliases("файл")]
        [Description("Выбрать файл теста из установленных в системе")]
        public async Task MyTestFile(CommandContext commandContext)
        {
            // TODO:
        }
    }
}
