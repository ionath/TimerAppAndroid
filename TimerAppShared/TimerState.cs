using System;
using System.Collections.Generic;
using System.Text;

namespace TimerAppShared
{
    public struct TimerState
    {
        public int Id { get; set; }
        public DateTime TimeStart { get; set; }
        public long Duration { get; set; }
        public double TimeLeft { get; set; }

        public const int ELAPSED_BIT = 1;
        public const int RUNNING_BIT = 2;
        public const int STARTED_BIT = 4;
        public BitField Flags;
        public string AlarmName { get; set; }

        public TimerState(int _id, DateTime _timeStart, long _duration, double _timeLeft, BitField _flags, string _alarmName)
        {
            Id = _id;
            TimeStart = _timeStart;
            Duration = _duration;
            TimeLeft = _timeLeft;
            Flags = _flags;
            AlarmName = _alarmName;
        }
    }
}
