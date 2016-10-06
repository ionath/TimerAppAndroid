﻿using System;
using System.Threading.Tasks;

namespace TimerAppShared
{
    public class TimerService : Object
    {
        TimerState state;
        //DateTime timeStart;
        //long duration;
        //double timeLeft = 0;
        //BitField flags;

        //const int ELAPSED_BIT = 1;
        //const int RUNNING_BIT = 2;

        public TimerMVAdapter timerAdaptor;
        public NotificationAdaptor notificationAdaptor;
        Task task;
        public int Updatecount { get; private set; }

        public TimerService()
        {
            //state.timeStart = DateTime.Now;
            //duration = 0;
            state = new TimerState(0, DateTime.Now, 0, 0, new BitField(0), "");
        }

        public TimerService(int id, long _duration, double _timeLeft, string alarmName, DateTime startTime, bool alreadyRunning)
        {
            //timeStart = DateTime.Now;
            //duration = _duration;
            BitField flags = new BitField();
            if (_timeLeft < _duration)
            {
                flags.SetBits(TimerState.STARTED_BIT);
            }
            if (alreadyRunning == false)
            {
                startTime = DateTime.Now;
            }
            state = new TimerState(id, startTime, _duration, _timeLeft, flags, alarmName);
        }

        public TimerService(int id, int hours, int minutes, int seconds, string alarmName)
        {
            //timeStart = DateTime.Now;
            long duration = 3600 * hours + 60 * minutes + seconds;
            //timeLeft = duration;
            state = new TimerState(id, DateTime.Now, duration, duration, new BitField(0), alarmName);
        }

        public void SetState(long duration, string alarmName)
        {
            state.duration = duration;
            state.timeLeft = (double)duration;
            state.alarmName = alarmName;

            timerAdaptor.UpdateDisplay();
        }

        public TimerState GetState()
        {
            //return new TimerState(timeStart, duration, timeLeft, flags);
            return state;
        }

        public static long CalcDuration(int hour, int minute, int second)
        {
            return (long)hour * 3600 + minute * 60 + second;
        }

        public double CalcSeconds()
        {
            double secondsRemaining = 0;
            if (state.flags.GetBit(TimerState.RUNNING_BIT) || state.flags.GetBit(TimerState.ELAPSED_BIT))
            {
                DateTime timeNow = DateTime.Now;
                TimeSpan timeDelta = timeNow.Subtract(state.timeStart);
                secondsRemaining = state.timeLeft - timeDelta.TotalSeconds;
                if (secondsRemaining < 0)
                {
                    secondsRemaining = -secondsRemaining;
                    if (state.flags.GetBit(TimerState.ELAPSED_BIT) == false)
                    {
                        state.flags.SetBits(TimerState.ELAPSED_BIT);
                        // Trigger notification
                        PostNotification();
                    }
                }
            }
            else
            {
                secondsRemaining = state.timeLeft;
            }
            return secondsRemaining;
        }

        public void PostNotification()
        {
            notificationAdaptor.PostNotification(state);
        }

        public void Start()
        {
            state.flags.SetBits(TimerState.RUNNING_BIT | TimerState.STARTED_BIT);
            //if (IsElapsed() == false)
            {
                state.timeStart = DateTime.Now;
            }

            task = Task.Factory.StartNew(() => {
                while (state.flags.GetBit(TimerState.RUNNING_BIT))
                {
                    timerAdaptor.UpdateDisplay();
                    Updatecount++;
                    // Get milliseconds to next second completed
                    double secondsLeft = CalcSeconds();
                    if (state.flags.GetBit(TimerState.ELAPSED_BIT) == false)
                    {
                        secondsLeft -= (long)secondsLeft;
                    }
                    else
                    {
                        secondsLeft = (long)secondsLeft + 1 - secondsLeft;
                    }
                    int millisecsToWait = (int) Math.Ceiling(1000*secondsLeft);

                    System.Threading.Thread.Sleep(millisecsToWait);
                }
            }
            );
            
            //TimerCallback timerDelegate = new TimerCallback(CheckStatus);
            //System.Threading.Timer threadTimer = new System.Threading.Timer(timerDelegate, this, 1000, 1000);
        }

        public void StartWithTime(DateTime startTime, double timeLeft)
        {
            state.flags.SetBits(TimerState.RUNNING_BIT | TimerState.STARTED_BIT);
            state.timeStart = startTime;

            task = Task.Factory.StartNew(() => {
                while (state.flags.GetBit(TimerState.RUNNING_BIT))
                {
                    timerAdaptor.UpdateDisplay();
                    Updatecount++;
                    // Get milliseconds to next second completed
                    double secondsLeft = CalcSeconds();
                    if (state.flags.GetBit(TimerState.ELAPSED_BIT) == false)
                    {
                        secondsLeft -= (long)secondsLeft;
                    }
                    else
                    {
                        secondsLeft = (long)secondsLeft + 1 - secondsLeft;
                    }
                    int millisecsToWait = (int)Math.Ceiling(1000 * secondsLeft);

                    System.Threading.Thread.Sleep(millisecsToWait);
                }
            }
            );

            //TimerCallback timerDelegate = new TimerCallback(CheckStatus);
            //System.Threading.Timer threadTimer = new System.Threading.Timer(timerDelegate, this, 1000, 1000);
        }

        public double GetTimeLeft()
        {
            return state.timeLeft;
        }

        public void Stop()
        {
            //if (IsElapsed() == false)
            {
                DateTime timeNow = DateTime.Now;
                TimeSpan timeDelta = timeNow.Subtract(state.timeStart);

                state.timeLeft = state.timeLeft - timeDelta.TotalSeconds;
                //if (state.timeLeft < 0)
                //{
                //    state.timeLeft = 0;
                //}

                state.flags.ClearBits(TimerState.RUNNING_BIT);
            }
        }

        public void Reset()
        {
            state.flags.ClearBits(TimerState.RUNNING_BIT | TimerState.ELAPSED_BIT | TimerState.STARTED_BIT);
            state.timeLeft = state.duration;
            timerAdaptor.UpdateDisplay();
        }

        public void Delete()
        {
            state.flags.ClearBits(TimerState.RUNNING_BIT);
        }

        static void CheckStatus(Object state)
        {
            TimerService timerService = (TimerService)state;
            timerService.timerAdaptor.UpdateDisplay();
        }

        public override string ToString()
        {
            //DateTime timeNow = DateTime.Now;
            //long secondsLeft = 0;
            //if (GetBit(RUNNING_BIT) || GetBit(ELAPSED_BIT))
            //{
            //    TimeSpan timeDelta = timeNow.Subtract(timeStart);
            //    secondsLeft = (long)(timeLeft - timeDelta.TotalSeconds);
            //}
            //else
            //{
            //    secondsLeft = (long)timeLeft;
            //}

            //if (secondsLeft < 0)
            //{
            //    secondsLeft *= -1;
            //    SetBit(ELAPSED_BIT, true);
            //}

            long secondsLeft = (long)Math.Round( CalcSeconds() );
            int hours = (int)secondsLeft / 3600;
            secondsLeft -= 3600 * hours;
            int minutes = (int)secondsLeft / 60;
            secondsLeft -= 60 * minutes;
            int seconds = (int)secondsLeft;

            string timeStr = hours.ToString() + ":" + minutes.ToString().PadLeft(2, '0') + ":" + seconds.ToString().PadLeft(2, '0');
            return timeStr;
        }

        public bool IsElapsed()
        {
            return state.flags.GetBit(TimerState.ELAPSED_BIT);
        }

        public bool IsRunning()
        {
            return state.flags.GetBit(TimerState.RUNNING_BIT);
        }

        public bool IsStarted()
        {
            return state.flags.GetBit(TimerState.STARTED_BIT);
        }
    }
}