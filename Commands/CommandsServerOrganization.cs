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
        public async Task Say(CommandContext commandContext, DiscordChannel channel = null, params string[] text)
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
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task Info(CommandContext commandContext, DiscordChannel channel = null, params string[] text)
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
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task Warn(CommandContext commandContext, DiscordChannel channel = null, params string[] text)
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
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task Critical(CommandContext commandContext, DiscordChannel channel = null, params string[] text)
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

        [Command("setup")]
        [Aliases("организуй")]
        [Description("Организация каналов на учебном сервере. Создаёт каналы #правила, #информация, категории Занятие, Управление ботом.\n" +
            "lang:  ru - создать каналы и категории для русскоязычного сервера\n" +
            "       en - создать каналы и категории для англоязычного сервера")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Setup(CommandContext commandContext)
        {
            LanguageID language = MePhItBot.Bot.Settings.LanguageDefault;
            try
            {
                language = MePhItBot.Bot.Settings.Localization.Language[commandContext.Guild];
            }
            catch(Exception e)
            {
                await commandContext.Message.RespondAsync($":bangbang: {e.Message}");
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
                return;
            }
            
            var channelNameRules = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgChannelNameRules);
            var channelNameInfo = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgChannelNameInfo);
            var categoryNameClass = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgCategoryNameClass);
            var channelNameChat = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgChannelNameChat);
            var channelNameCommon = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgChannelNameCommon);
            var channelNameSubmit = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgChannelNameSubmit);
            var categoryNameControl = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgCategoryNameControl);
            var channelNameCommands = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgChannelNameCommands);

            var serverRoleNameTeacher = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgRoleTeacher);
            var serverRoleNameGroupLeader = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgRoleGroupLeader);
            var serverRoleNameStudent = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgRoleStudent);

            var server = commandContext.Guild;
            var everyone = server.EveryoneRole;
            // ---- No Category channels ----
            // #rules
            // #info
            var channelRules = await server.CreateTextChannelAsync(channelNameRules, null);
            var channelInfo = await server.CreateTextChannelAsync(channelNameInfo, null);
            var allow = Permissions.AccessChannels | Permissions.ReadMessageHistory | Permissions.AddReactions;
            var deny = Permissions.Administrator | Permissions.AttachFiles
                | Permissions.BanMembers | Permissions.ChangeNickname | Permissions.CreateInstantInvite | Permissions.DeafenMembers
                | Permissions.EmbedLinks | Permissions.KickMembers | Permissions.ManageChannels | Permissions.ManageEmojis
                | Permissions.ManageGuild | Permissions.ManageMessages | Permissions.ManageNicknames | Permissions.ManageRoles
                | Permissions.ManageWebhooks | Permissions.MentionEveryone | Permissions.MoveMembers | Permissions.MuteMembers
                | Permissions.PrioritySpeaker | Permissions.SendMessages | Permissions.SendTtsMessages
                | Permissions.Speak | Permissions.UseExternalEmojis | Permissions.UseVoice | Permissions.UseVoiceDetection
                | Permissions.ViewAuditLog;
            channelRules.AddOverwriteAsync(everyone, allow, deny);
            channelInfo.AddOverwriteAsync(everyone, allow, deny);

            // ---- Class channels ----
            // #chat
            // #submit-your-work-here
            // #Common
            var categoryClass = await server.CreateChannelCategoryAsync(categoryNameClass);
            
            allow = Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks
                | Permissions.AttachFiles | Permissions.ReadMessageHistory | Permissions.MentionEveryone
                | Permissions.AddReactions | Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection;
            deny = Permissions.CreateInstantInvite | Permissions.ManageChannels | Permissions.ManageRoles
                | Permissions.ManageWebhooks | Permissions.SendTtsMessages | Permissions.ManageMessages
                | Permissions.UseExternalEmojis | Permissions.MuteMembers | Permissions.DeafenMembers
                | Permissions.MoveMembers | Permissions.PrioritySpeaker | Permissions.Stream;
            await categoryClass.AddOverwriteAsync(everyone, allow, deny);
            
            server.CreateTextChannelAsync(channelNameChat, categoryClass);
            server.CreateVoiceChannelAsync(channelNameCommon, categoryClass);
            var channelSubmit = await server.CreateChannelAsync(channelNameSubmit, ChannelType.Text, categoryClass);
            allow = Permissions.CreateInstantInvite | Permissions.AccessChannels | Permissions.SendMessages
                | Permissions.EmbedLinks | Permissions.AttachFiles | Permissions.AddReactions;
            deny = Permissions.ManageChannels | Permissions.ManageRoles | Permissions.ManageWebhooks
                | Permissions.SendTtsMessages | Permissions.ManageMessages | Permissions.ReadMessageHistory
                | Permissions.MentionEveryone | Permissions.UseExternalEmojis;
            channelSubmit.AddOverwriteAsync(everyone, allow, deny);

            // ---- Bot Controls channels ----
            var categoryControl = await server.CreateChannelCategoryAsync(categoryNameControl);

            allow = Permissions.None;
            deny = Permissions.AccessChannels | Permissions.AddReactions | Permissions.Administrator | Permissions.AttachFiles
                | Permissions.BanMembers | Permissions.ChangeNickname | Permissions.CreateInstantInvite | Permissions.DeafenMembers
                | Permissions.EmbedLinks | Permissions.KickMembers | Permissions.ManageChannels | Permissions.ManageEmojis
                | Permissions.ManageGuild | Permissions.ManageMessages | Permissions.ManageNicknames | Permissions.ManageRoles
                | Permissions.ManageWebhooks | Permissions.MentionEveryone | Permissions.MoveMembers | Permissions.MuteMembers
                | Permissions.PrioritySpeaker | Permissions.ReadMessageHistory | Permissions.SendMessages | Permissions.SendTtsMessages
                | Permissions.Speak | Permissions.UseExternalEmojis | Permissions.UseVoice | Permissions.UseVoiceDetection
                | Permissions.ViewAuditLog;
            await categoryControl.AddOverwriteAsync(everyone, allow, deny);
            
            await server.CreateTextChannelAsync(channelNameCommands, categoryControl);

            // Make roles
            var studentPermissions = Permissions.CreateInstantInvite | Permissions.AccessChannels | 
                Permissions.SendMessages | Permissions.EmbedLinks | Permissions.AttachFiles | Permissions.AddReactions
                | Permissions.UseVoice | Permissions.Speak | Permissions.UseVoiceDetection;
            var studentRoleColor = DiscordColor.LightGray;
            var groupLeaderRoleColor = new DiscordColor(0xF1C40F);
            var teacherRoleColor = new DiscordColor(0x2ECC71);
            server.CreateRoleAsync(serverRoleNameTeacher, everyone.Permissions, teacherRoleColor, true, true);
            server.CreateRoleAsync(serverRoleNameGroupLeader, studentPermissions, groupLeaderRoleColor, false, true); ;
            server.CreateRoleAsync(serverRoleNameStudent, studentPermissions, studentRoleColor, true, true);

            await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

    }
}
