using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MePhIt;

namespace MePhIt.Commands.ServerOrganization
{
    [Group("setup")]
    [Aliases("организуй")]
    [Description("Организация каналов на учебном сервере.")]
    [RequirePermissions(Permissions.Administrator)]
    class CmdGrpSetup : BaseCommandModule
    {
        [Command("channels")]
        [Aliases("каналы")]
        [Description("Создаёт роли студент, староста, преподаватель и устанавливает права")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Channels(CommandContext commandContext)
        {
            LanguageID language = MePhItBot.Bot.Settings.LanguageDefault;
            try
            {
                language = MePhItBot.Bot.Settings.Localization.Language[commandContext.Guild];
            }
            catch (Exception e)
            {
                await commandContext.Message.RespondAsync($"{MePhItUtilities.EmojiError} {e.Message}");
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

            var server = commandContext.Guild;
            var everyone = server.EveryoneRole;
            // ---- No Category channels ----
            // #rules
            // #info
            var channelRules = await server.CreateTextChannelAsync(channelNameRules, null);
            var channelInfo = await server.CreateTextChannelAsync(channelNameInfo, null);
            var allow = Permissions.AccessChannels | Permissions.ReadMessageHistory | Permissions.AddReactions;
            var deny = Permissions.All;
            channelRules.AddOverwriteAsync(everyone, allow, deny);
            channelInfo.AddOverwriteAsync(everyone, allow, deny);

            // ---- Class channels ----
            // #chat
            // #submit-your-work-here
            // #Common
            var categoryClass = await server.CreateChannelCategoryAsync(categoryNameClass);

            allow = Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks | Permissions.CreateInstantInvite
                | Permissions.AttachFiles | Permissions.ReadMessageHistory | Permissions.MentionEveryone
                | Permissions.AddReactions | Permissions.UseVoice | Permissions.Speak;
            deny = Permissions.All;
            await categoryClass.AddOverwriteAsync(everyone, allow, deny);

            server.CreateTextChannelAsync(channelNameChat, categoryClass);
            server.CreateVoiceChannelAsync(channelNameCommon, categoryClass);
            var channelSubmit = await server.CreateChannelAsync(channelNameSubmit, ChannelType.Text, categoryClass);
            allow = Permissions.AccessChannels | Permissions.SendMessages
                | Permissions.EmbedLinks | Permissions.AttachFiles | Permissions.AddReactions;
            deny = Permissions.All;
            channelSubmit.AddOverwriteAsync(everyone, allow, deny);

            // ---- Bot Controls channels ----
            var categoryControl = await server.CreateChannelCategoryAsync(categoryNameControl);

            allow = Permissions.None;
            deny = Permissions.All;
            await categoryControl.AddOverwriteAsync(everyone, allow, deny);

            await server.CreateTextChannelAsync(channelNameCommands, categoryControl);

            await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

        [Command("roles")]
        [Aliases("роли")]
        [Description("Создаёт каналы #правила, #информация, категории Занятие, Управление ботом")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Roles(CommandContext commandContext)
        {
            LanguageID language = MePhItBot.Bot.Settings.LanguageDefault;
            try
            {
                language = MePhItBot.Bot.Settings.Localization.Language[commandContext.Guild];
            }
            catch (Exception e)
            {
                await commandContext.Message.RespondAsync($"{MePhItUtilities.EmojiError} {e.Message}");
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
                return;
            }

            var serverRoleNameTeacher = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgRoleTeacher);
            var serverRoleNameGroupLeader = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgRoleGroupLeader);
            var serverRoleNameStudent = MePhItLocalization.Localization.Message(language, MessageID.CmdSrvrOrgRoleStudent);

            var server = commandContext.Guild;
            var everyone = server.EveryoneRole;
            // Make roles
            var studentPermissions = Permissions.CreateInstantInvite | Permissions.AccessChannels |
                Permissions.SendMessages | Permissions.EmbedLinks | Permissions.AttachFiles | Permissions.AddReactions
                | Permissions.UseVoice | Permissions.Speak;
            var studentRoleColor = DiscordColor.LightGray;
            var groupLeaderRoleColor = new DiscordColor(0xF1C40F);
            var teacherRoleColor = new DiscordColor(0x2ECC71);
            server.CreateRoleAsync(serverRoleNameTeacher, everyone.Permissions, teacherRoleColor, true, true);
            server.CreateRoleAsync(serverRoleNameGroupLeader, studentPermissions, groupLeaderRoleColor, false, true); ;
            server.CreateRoleAsync(serverRoleNameStudent, studentPermissions, studentRoleColor, true, true);

            // Set @everyone permissions
            commandContext.Guild.EveryoneRole.ModifyAsync(null, Permissions.MentionEveryone, null, null, true, null);

            await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

        [Command("all")]
        [Aliases("всё")]
        [Description("Создаёт каналы #правила, #информация, категории Занятие, Управление ботом. Создаёт роли студент, староста, преподаватель " +
            "и устанавливает права")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task All(CommandContext commandContext)
        {
            LanguageID language = MePhItBot.Bot.Settings.LanguageDefault;
            try
            {
                language = MePhItBot.Bot.Settings.Localization.Language[commandContext.Guild];
            }
            catch (Exception e)
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
            var deny = Permissions.All;
            channelRules.AddOverwriteAsync(everyone, allow, deny);
            channelInfo.AddOverwriteAsync(everyone, allow, deny);

            // ---- Class channels ----
            // #chat
            // #submit-your-work-here
            // #Common
            var categoryClass = await server.CreateChannelCategoryAsync(categoryNameClass);

            allow = Permissions.AccessChannels | Permissions.SendMessages | Permissions.EmbedLinks | Permissions.CreateInstantInvite
                | Permissions.AttachFiles | Permissions.ReadMessageHistory | Permissions.MentionEveryone
                | Permissions.AddReactions | Permissions.UseVoice | Permissions.Speak;
            deny = Permissions.All;
            await categoryClass.AddOverwriteAsync(everyone, allow, deny);

            server.CreateTextChannelAsync(channelNameChat, categoryClass);
            server.CreateVoiceChannelAsync(channelNameCommon, categoryClass);
            var channelSubmit = await server.CreateChannelAsync(channelNameSubmit, ChannelType.Text, categoryClass);
            allow = Permissions.AccessChannels | Permissions.SendMessages
                | Permissions.EmbedLinks | Permissions.AttachFiles | Permissions.AddReactions;
            deny = Permissions.All;
            channelSubmit.AddOverwriteAsync(everyone, allow, deny);

            // ---- Bot Controls channels ----
            var categoryControl = await server.CreateChannelCategoryAsync(categoryNameControl);

            allow = Permissions.None;
            deny = Permissions.All;
            await categoryControl.AddOverwriteAsync(everyone, allow, deny);

            await server.CreateTextChannelAsync(channelNameCommands, categoryControl);

            // Make roles
            var studentPermissions = Permissions.CreateInstantInvite | Permissions.AccessChannels |
                Permissions.SendMessages | Permissions.EmbedLinks | Permissions.AttachFiles | Permissions.AddReactions
                | Permissions.UseVoice | Permissions.Speak;
            var studentRoleColor = DiscordColor.LightGray;
            var groupLeaderRoleColor = new DiscordColor(0xF1C40F);
            var teacherRoleColor = new DiscordColor(0x2ECC71);
            server.CreateRoleAsync(serverRoleNameTeacher, everyone.Permissions, teacherRoleColor, true, true);
            server.CreateRoleAsync(serverRoleNameGroupLeader, studentPermissions, groupLeaderRoleColor, false, true); ;
            server.CreateRoleAsync(serverRoleNameStudent, studentPermissions, studentRoleColor, true, true);

            // Set @everyone permissions
            commandContext.Guild.EveryoneRole.ModifyAsync(null, Permissions.MentionEveryone, null, null, true, null);

            await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }
    }
}
