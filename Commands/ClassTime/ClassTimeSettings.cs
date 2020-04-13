using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MePhIt.Commands.ClassTime
{
    public class ClassTimeSettings
    {
        public LanguageID Language { get; set; }
        public MePhItLocalization Localization { get; private set; } = MePhItLocalization.Localization;
        public TimeZoneInfo ServerTimeZone { get; private set; } = TimeZoneInfo.Utc;
        public int ClassTimeInMinutes { get; set; }
        public int TimeTillBreakInMinutes { get; set; }
        public int BreakDurationInMinutes { get; set; }
        public int NotifyBeforeClassEndInMinutes { get; set; }

        private int classStartedSeconds = 0;

        public void Start()
        {
            CalculateSchedule();
            timer.Start();
            timer.AutoReset = true;
            var timestamp = TimeZoneInfo.ConvertTime(DateTime.Now, ServerTimeZone);
            classStartedSeconds = timestamp.Second;
            var msg = $"{channel.Guild.EveryoneRole.Mention} {string.Format(Localization.Message(Language, MessageID.ClassStarted), timestamp)}";
            channel.SendMessageAsync(msg);
        }

        public void Stop()
        {
            timer.Stop();
            timer.Close();
            localSettingsHolder.Remove(channel.Guild);
        }

        public void Break()
        {
            AdjustSchedule();
        }

        private DiscordChannel channel;
        private IDictionary<DiscordGuild, ClassTimeSettings> localSettingsHolder;

        private Timer timer = new Timer(OneMinute);
        private const double OneMinute = 60e3;
        private int timePassedInMin = 0;

        private IDictionary<int, ClassTimeEventType> schedule;

        public ClassTimeSettings(in IDictionary<DiscordGuild, ClassTimeSettings> localSettingsHolder, in DiscordChannel channel, 
                           in int classTimeInMin, in int timeTillBreakInMin, in int breakDurationInMin, 
                           in int notifyBeforeClassEndInMin)
        {
            this.channel = channel;
            this.localSettingsHolder = localSettingsHolder;

            ClassTimeInMinutes = classTimeInMin;
            TimeTillBreakInMinutes = timeTillBreakInMin;
            BreakDurationInMinutes = breakDurationInMin;
            NotifyBeforeClassEndInMinutes = notifyBeforeClassEndInMin;
            timer.Elapsed += Timer_Elapsed;
            try
            {
                Language = Localization.Language[channel.Guild];
                ServerTimeZone = MePhItBot.Bot.Settings.TimeZone[channel.Guild];
            }
            catch(Exception e)
            {
                channel.SendMessageAsync($"{MePhItUtilities.EmojiCritical}{e.Message}\n{e.StackTrace}");
                throw e;
            }
        }

        public ClassTimeSettings(in IDictionary<DiscordGuild, ClassTimeSettings> localSettingsHolder, in DiscordChannel channel)
        {
            this.channel = channel;
            this.localSettingsHolder = localSettingsHolder;
            timer.Elapsed += Timer_Elapsed;
            Language = Localization.Language[channel.Guild];
        }

        public enum ClassTimeEventType
        {
            ClassStart,
            BreakStart,
            BreakEnd,
            NotifyClassEnding,
            ClassEnd,
            None
        }

        // Calculate the exact values
        private void CalculateSchedule()
        {
            // Class start: t = 0
            // Break 1 Start: t = TimeTillBreakInMinutes
            // Break 1 End: t = TimeTillBreakInMinutes + BreakDurationInMinutes
            // Break 2 Start: t  = 2 * TimeTillBreakInMinutes + BreakDurationInMinutes
            // Break 2 End: t = 2 * TimeTillBreakInMinutes + 2 * BreakDurationInMinutes
            // ...
            // Break N Start: t = N * (TimeTillBreakInMinutes + BreakDurationInMinutes) - BreakDurationInMinutes
            // Break N End: t = N * (TimeTillBreakInMinutes + BreakDurationInMinutes)
            // Notify Before Class End: t = ClassTimeInMinutes - NotifyBeforeClassEndInMinutes
            // Class End: t = t = ClassTimeInMinutes
            // BreakNum = (ClassTimeInMinutes - NotifyBeforeClassEndInMinutes)/ (TimeTillBreakInMinutes + BreakDurationInMinutes)
            schedule = new Dictionary<int, ClassTimeEventType>();
            int breaksNum = (ClassTimeInMinutes - NotifyBeforeClassEndInMinutes) / (TimeTillBreakInMinutes + BreakDurationInMinutes);
            for(int i = 1; i <= breaksNum; i++)
            {
                var breakStart = i * (TimeTillBreakInMinutes + BreakDurationInMinutes) - BreakDurationInMinutes;
                var breakEnd = i * (TimeTillBreakInMinutes + BreakDurationInMinutes);
                schedule[breakStart] = ClassTimeEventType.BreakStart;
                schedule[breakEnd] = ClassTimeEventType.BreakEnd;
            }
            var notifyTime = ClassTimeInMinutes - NotifyBeforeClassEndInMinutes;
            schedule[notifyTime] = ClassTimeEventType.NotifyClassEnding;
            var classEndTime = ClassTimeInMinutes;
            schedule[classEndTime] = ClassTimeEventType.ClassEnd;
        }

        private void AdjustSchedule()
        {
            // Class start: t = dt
            // Break 1 Start: t = dt + TimeTillBreakInMinutes
            // Break 1 End: t = dt + TimeTillBreakInMinutes + BreakDurationInMinutes
            // Break 2 Start: t  = dt + 2 * TimeTillBreakInMinutes + BreakDurationInMinutes
            // Break 2 End: t = dt + 2 * TimeTillBreakInMinutes + 2 * BreakDurationInMinutes
            // ...
            // Break N Start: t = dt + N * (TimeTillBreakInMinutes + BreakDurationInMinutes) - BreakDurationInMinutes
            // Break N End: t = dt + N * (TimeTillBreakInMinutes + BreakDurationInMinutes)
            // Notify Before Class End: t = ClassTimeInMinutes - NotifyBeforeClassEndInMinutes
            // Class End: t = t = ClassTimeInMinutes
            // BreakNum = (ClassTimeInMinutes - NotifyBeforeClassEndInMinutes)/ (TimeTillBreakInMinutes + BreakDurationInMinutes)
            // TODO: 
            var scheduleAdjusted = new Dictionary<int, ClassTimeEventType>();
            
            var dt = timePassedInMin + BreakDurationInMinutes + 1;
            scheduleAdjusted[timePassedInMin + 1] = ClassTimeEventType.BreakStart;
            scheduleAdjusted[dt] = ClassTimeEventType.BreakEnd;
            var breakNum = (ClassTimeInMinutes - NotifyBeforeClassEndInMinutes) / (TimeTillBreakInMinutes + BreakDurationInMinutes + dt);
            for(int i = 1; i < breakNum; i++)
            {
                var breakStart = dt + i * (TimeTillBreakInMinutes + BreakDurationInMinutes) - BreakDurationInMinutes;
                var breakEnd = dt + i * (TimeTillBreakInMinutes + BreakDurationInMinutes);
                scheduleAdjusted[breakStart] = ClassTimeEventType.BreakStart;
                scheduleAdjusted[breakEnd] = ClassTimeEventType.BreakEnd;
            }

            var notifyTime = ClassTimeInMinutes - NotifyBeforeClassEndInMinutes;
            scheduleAdjusted[notifyTime] = ClassTimeEventType.NotifyClassEnding;
            var classEndTime = ClassTimeInMinutes;
            scheduleAdjusted[classEndTime] = ClassTimeEventType.ClassEnd;
            schedule.Clear();
            schedule = scheduleAdjusted;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timePassedInMin++;
            ClassTimeEventType eventType = ClassTimeEventType.None;
            if(schedule.TryGetValue(timePassedInMin, out eventType))
            {
                var timeNow = TimeZoneInfo.ConvertTime(DateTime.Now, ServerTimeZone);
                switch (eventType)
                {
                    case ClassTimeEventType.BreakStart:
                        var breakEndsAt = DateTime.Now + new TimeSpan(0, BreakDurationInMinutes, 0);
                        breakEndsAt = TimeZoneInfo.ConvertTime(breakEndsAt, ServerTimeZone);
                        var msg = $"{channel.Guild.EveryoneRole.Mention} {string.Format(Localization.Message(Language, MessageID.BreakStarted), BreakDurationInMinutes, breakEndsAt, timeNow)}";
                        channel.SendMessageAsync(msg);
                        break;
                    case ClassTimeEventType.BreakEnd:
                        msg = $"{channel.Guild.EveryoneRole.Mention} {string.Format(Localization.Message(Language, MessageID.BreakEnded), timeNow)}";
                        channel.SendMessageAsync(msg);
                        break;
                    case ClassTimeEventType.NotifyClassEnding:
                        msg = $"{channel.Guild.EveryoneRole.Mention} {string.Format(Localization.Message(Language, MessageID.ClassEndingNotify), NotifyBeforeClassEndInMinutes, timeNow)} ";
                        channel.SendMessageAsync(msg);
                        break;
                    case ClassTimeEventType.ClassEnd:
                        msg = $"{channel.Guild.EveryoneRole.Mention} {string.Format(Localization.Message(Language, MessageID.ClassEnded), timeNow)}";
                        channel.SendMessageAsync(msg);
                        Stop();
                        break;
                    case ClassTimeEventType.ClassStart:
                        msg = $"{channel.Guild.EveryoneRole.Mention} {string.Format(Localization.Message(Language, MessageID.ClassStarted), timeNow)} ";
                        channel.SendMessageAsync(msg);
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
