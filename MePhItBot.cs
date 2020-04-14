/*
 * DSharpPlus API docs - https://dsharpplus.emzi0767.com/api/index.html
 * Title: MePhITBot
 * Description: Discord bot for teaching needs. 
 * Has very basic moderation features and ability to perform tests through MyTest engine.
 * Features:
 * - organize new server into something useful
 * - manage posts
 * - time management for classes. Start, stop, make a break, etc.
 * - automated testing via MyTest engine
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using MePhIt.Commands;
using System.Linq;

namespace MePhIt
{
    class MePhItBot
    {
        public static MePhItBot Bot;
        private DiscordClient Discord { get { return Settings.Discord; } }
        // Bot reacts to commands
        public DiscordEmoji ReactSuccess { get { return Settings.EmojiReactSuccess; } }
        public DiscordEmoji ReactFail { get { return Settings.EmojiReactFail; } }
        public IList<DiscordEmoji> Numbers { get; private set; }

        private CommandsNextExtension commands;

        public MePhItSettings Settings { get; private set; }

        public void Start()
        {
            Settings = new MePhItSettings();

            IList<string> commandPrefixes;
            commandPrefixes = Settings.LoadSettings();

            CommandsRegister(commandPrefixes);

            Discord.ConnectAsync();
        }

        private void CommandsRegister(IList<string> commandPrefixes)
        {
            commands = Discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = commandPrefixes
            }) ;

            // Add commands here
            commands.RegisterCommands<CommandsServerOrganization>();
            commands.RegisterCommands<Commands.ServerOrganization.CmdGrpSetup>();
            commands.RegisterCommands<CommandsModeration>();
            commands.RegisterCommands<CommandsMyTest>();
            commands.RegisterCommands<CommandsClassTime>();
            commands.RegisterCommands<CommandsSetup>();
        }

        static async Task Main(string[] args)
        {
            Bot = new MePhItBot();
            while(true)
            {
                try
                {
                    if (Bot.Crash || Bot.FirstStart)
                    {
                        Bot.Start();
                        Bot.Crash = false;
                        Bot.FirstStart = false;

                        await Task.Delay(-1);
                    }
                }
                catch (Exception e)
                {
                    var timestamp = DateTime.Now;
                    var filename = $"crash-{timestamp.Year}.{timestamp.Month}.{timestamp.Day}-{timestamp.Hour}.{timestamp.Minute}.{timestamp.Second}.log";
                    var sw = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None));
                    sw.WriteLine($"CRASH:\n{e.Message}\n{e.StackTrace}");
                    sw.Close();
                    Bot.Crash = true;
                }
            }
        }

        public bool Crash = false;
        public bool FirstStart = true;
    }
}
