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
    public class CommandsServerOrganization : BaseCommandModule
    {
        [Command("ping")]
        [Aliases("пинг")]
        [Description("Проверка связи с ботом")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Ping(CommandContext commandContext)
        {
            await commandContext.RespondAsync("pong!");
            await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

        [Command("say")]
        [Aliases("скажи")]
        [Description("say #channel Hello world!\nПоместить текст без форматирования в канал")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Say(CommandContext commandContext,
                              [Description("Канал вывода сообщения")]
                              DiscordChannel channel = null,
                              [Description("Размещаемый на канале текст")]
                              params string[] text)
        {
            string txt = "";
            foreach(var str in text)
            {
                txt += str + " ";
            }

            channel = channel == null ? commandContext.Channel : channel;
            await channel.SendMessageAsync(txt);
            commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

        [Command("info")]
        [Aliases("инфо")]
        [Description("info #channel объявление\nИнформационное сообщение")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task Info(CommandContext commandContext,
                               [Description("Канал вывода сообщения")]
                               DiscordChannel channel = null,
                               [Description("Размещаемый на канале текст")]
                               params string[] text)
        {
            string txt = ":information_source: ";
            foreach(var str in text)
            {
                txt += str + " ";
            }
            channel = channel == null ? commandContext.Channel : channel;
            await channel.SendMessageAsync(txt);
            commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

        [Command("warn")]
        [Aliases("пред")]
        [Description("warn #channel объявление\nВажное сообщение")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task Warn(CommandContext commandContext,
                               [Description("Канал вывода сообщения")]
                               DiscordChannel channel = null,
                               [Description("Размещаемый на канале текст")]
                               params string[] text)
        {
            string txt = ":warning: ";
            foreach (var str in text)
            {
                txt += str + " ";
            }
            channel = channel == null ? commandContext.Channel : channel;
            await channel.SendMessageAsync(txt);
            commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

        [Command("crit")]
        [Aliases("крит")]
        [Description("crit #channel объявление\nКритически важное сообщение")]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task Critical(CommandContext commandContext,
                                   [Description("Канал вывода сообщения")]
                                   DiscordChannel channel = null,
                                   [Description("Размещаемый на канале текст")]
                                   params string[] text)
        {
            string txt = ":exclamation: ";
            foreach (var str in text)
            {
                txt += str + " ";
            }
            channel = channel == null ? commandContext.Channel : channel;
            await channel.SendMessageAsync(txt);
            commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }
    }
}
