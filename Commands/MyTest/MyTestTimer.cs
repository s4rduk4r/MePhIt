using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MePhIt.Commands.MyTest
{
    public class MyTestTimer : Timer
    {
        /// <summary>
        /// Test started flag. Changes behaviour from pre-test to testing
        /// </summary>
        public bool TestStarted { get; private set; } = false;
        public CommandsMyTest CommandsMyTest { get; private set; } = null;
        
        public readonly int TimeTick = 1000; //ms
        /// <summary>
        /// Passed time counter
        /// </summary>
        public int TimePassed { get; set; } = 0;

        /// <summary>
        /// Sets test time for a given server. Restarts the timer with the new Interval set
        /// </summary>
        /// <param name="server"></param>
        public void StartTest()
        {
            var server = CommandsMyTest.GetServer(this);
            Stop();
            Interval = CommandsMyTest.Settings[server].TestState.Time * 1e3;
            TestStarted = true;
            Start();
        }


        public MyTestTimer(CommandsMyTest cmdMyTest) : base()
        {
            CommandsMyTest = cmdMyTest;
            Interval = TimeTick;
        }

        public MyTestTimer(CommandsMyTest cmdMyTest, double interval) : base(interval)
        {
            CommandsMyTest = cmdMyTest;
            Interval = interval;
        }
    }
}
