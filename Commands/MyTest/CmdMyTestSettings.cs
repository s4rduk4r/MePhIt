using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using MyTestLib;

namespace MePhIt.Commands.MyTest
{
    /// <summary>
    /// Local settings for MyTest command group
    /// </summary>
    public class CmdMyTestSettings
    {
        /// <summary>
        /// Discord channel where to send MyTest state messages
        /// </summary>
        public DiscordChannel Channel { get; set; }

        /// <summary>
        /// Test state for each student
        /// </summary>
        public IDictionary<DiscordMember, TestState> TestState { get; set; } = new ConcurrentDictionary<DiscordMember, TestState>();

        /// <summary>
        /// Test channel group
        /// </summary>
        public IDictionary<DiscordMember, DiscordChannel> TempTestChannelGrp { get; set; } = new ConcurrentDictionary<DiscordMember, DiscordChannel>();
        /// <summary>
        /// Test question messages
        /// </summary>
        public IDictionary<DiscordMember, IList<(TestQuestion Question, ulong MessageId)>> TempTestChannelQuestions { get; set; } 
                                        = new ConcurrentDictionary<DiscordMember, IList<(TestQuestion Question, ulong MessageId)>>();
        /// <summary>
        /// Timers
        /// </summary>
        public MyTestTimer Timer { get; set; } = null;
    }
}
