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
    public class CommandsModeration : BaseCommandModule
    {
        [Command("rm")]
        [Aliases("уд")]
        [Description("rm <num> <@user> <#channel>\n" +
            "Удалить <num> сообщений пользователя <@user> на канале <#channel>")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Rm(CommandContext commandContext, int num = 2, DiscordMember member = null, DiscordChannel channel = null)
        {
            if (num <= 0)
            {
                commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactFail);
                return;
            }

            int maxNumber = num < 100 ? num : 100;
            var rmChannel = channel == null ? commandContext.Channel : channel;
            IReadOnlyList<DiscordMessage> messages = null;
            var messagesToDelete = new List<DiscordMessage>();

            var msgIdBefore = commandContext.Message.Id;
            do
            {
                messages = await rmChannel.GetMessagesBeforeAsync(msgIdBefore);
                for (int i = 0; i < messages.Count; i++)
                {
                    var msg = messages[i];

                    if(member != null)
                    {
                        if (msg.Author == member && messagesToDelete.Count < num)
                        {
                            messagesToDelete.Add(msg);
                        }
                    }
                    else
                    {
                        if (messagesToDelete.Count < num)
                        {
                            messagesToDelete.Add(msg);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (messages.Count > 0)
                {
                    msgIdBefore = messages[messages.Count - 1].Id;
                }
                else
                {
                    break;
                }
            }
            while (messages.Count > 0 && messagesToDelete.Count < num);

            rmChannel.DeleteMessagesAsync(messagesToDelete).GetAwaiter().GetResult();
            await commandContext.Message.CreateReactionAsync(MePhItBot.Bot.ReactSuccess);
        }

        [Command("clear")]
        [Aliases("убрать")]
        [Description("Удаляет все сообщения на текущем канале")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Clean(CommandContext commandContext)
        {
            var channel = commandContext.Channel;
            IReadOnlyList<DiscordMessage> messages;
            do
            {
                messages = channel.GetMessagesAsync().GetAwaiter().GetResult();
                await channel.DeleteMessagesAsync(messages);
            }
            while (messages != null);
        }


    }
}
