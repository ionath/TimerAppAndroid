using System;
using System.Collections.Generic;
using System.Text;

namespace TimerAppShared
{
    public struct TimerState
    {
        public int id { get; set; }
        public DateTime timeStart { get; set; }
        public long duration { get; set; }
        public double timeLeft { get; set; }

        public const int ELAPSED_BIT = 1;
        public const int RUNNING_BIT = 2;
        public const int STARTED_BIT = 4;
        public BitField flags;
        public string alarmName { get; set; }

        public TimerState(int _id, DateTime _timeStart, long _duration, double _timeLeft, BitField _flags, string _alarmName)
        {
            id = _id;
            timeStart = _timeStart;
            duration = _duration;
            timeLeft = _timeLeft;
            flags = _flags;
            alarmName = _alarmName;
        }
    }
}
