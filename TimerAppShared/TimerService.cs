using System;
using System.Threading.Tasks;

namespace TimerAppShared
{
    public class TimerService : Object
    {
        TimerState state;
        public TimerState State
        {
            get
            {
                return state;
            }
        }

        Task task;
        public int Updatecount { get; private set; }

        // event handling
        Object eventObjLock = new Object();
        event EventHandler displayTimerChangedEvent;
        public event EventHandler DisplayTimeChanged
        {
            add
            {
                lock (eventObjLock)
                {
                    displayTimerChangedEvent += value;
                }
            }
            remove
            {
                lock (eventObjLock)
                {
                    displayTimerChangedEvent -= value;
                }
            }
        }
        public event EventHandler TimerFinished;
        public event EventHandler TimerPaused;

        protected virtual void OnDisplayTimeChanged(EventArgs e)
        {
            lock (eventObjLock)
            {
                displayTimerChangedEvent?.Invoke(this, e);
            }
        }

        protected virtual void OnTimerFinished(EventArgs e)
        {
            TimerFinished?.Invoke(this, e);
        }

        protected virtual void OnTimerPaused(EventArgs e)
        {
            TimerPaused?.Invoke(this, e);
        }

        public void ForceDisplayTimeChangedEvent()
        {
            OnDisplayTimeChanged(EventArgs.Empty);
        }

        public TimerService()
        {
            state = new TimerState(0, DateTime.Now, 0, 0, new BitField(0), "");
        }
        
        public TimerService(TimerDBItem timerDBItem)
        {
            BitField flags = new BitField();
            state = new TimerState(timerDBItem.Id, timerDBItem.timeStart, timerDBItem.duration, timerDBItem.timeLeft, flags, timerDBItem.alarmName);

            if (timerDBItem.started)
            {
                state.Flags.SetBits(TimerState.STARTED_BIT);
            }
            if (timerDBItem.running)
            {
                state.Flags.SetBits(TimerState.STARTED_BIT|TimerState.RUNNING_BIT);
            }
            if (timerDBItem.started && calcSecondsRemaining() < 0)
            {
                state.Flags.SetBits(TimerState.ELAPSED_BIT);
            }

            if (timerDBItem.running)
            {
                runTask();
            }
        }
        
        public void SetState(long duration, string alarmName)
        {
            state.Duration = duration;
            state.TimeLeft = (double)duration;
            state.AlarmName = alarmName;
            
            OnDisplayTimeChanged(EventArgs.Empty);
        }
        
        public static long CalcDuration(int hour, int minute, int second)
        {
            return (long)hour * 3600 + minute * 60 + second;
        }

        double calcSecondsRemaining()
        {
            DateTime timeNow = DateTime.Now;
            TimeSpan timeDelta = timeNow.Subtract(state.TimeStart);
            return state.TimeLeft - timeDelta.TotalSeconds;
        }

        public TimerDBItem MakeDBItem()
        {
            var timerDBItem = new TimerDBItem();
            timerDBItem.alarmName = state.AlarmName;
            timerDBItem.duration = state.Duration;
            timerDBItem.timeLeft = state.TimeLeft;
            timerDBItem.timeStart = state.TimeStart;
            timerDBItem.running = state.Flags.GetBit(TimerState.RUNNING_BIT);
            timerDBItem.started = state.Flags.GetBit(TimerState.STARTED_BIT);

            return timerDBItem;
        }

        public double CalcSeconds()
        {
            double secondsRemaining = 0;
            if (state.Flags.GetBit(TimerState.RUNNING_BIT) || state.Flags.GetBit(TimerState.ELAPSED_BIT))
            {
                secondsRemaining = calcSecondsRemaining();
                if (secondsRemaining < 0)
                {
                    secondsRemaining = -secondsRemaining;
                }
            }
            else
            {
                secondsRemaining = state.TimeLeft;
            }
            return secondsRemaining;
        }

        public void SetId(int id)
        {
            state.Id = id;
        }
        
        void runTask()
        {
            task = Task.Factory.StartNew(() => {
                while (state.Flags.GetBit(TimerState.RUNNING_BIT))
                {
                    // Perform on display time changed events
                    OnDisplayTimeChanged(EventArgs.Empty);
                    Updatecount++;
                    
                    double secondsRemaining = calcSecondsRemaining();
                    // Check if timer is finished
                    if (secondsRemaining < 0)
                    {
                        secondsRemaining = -secondsRemaining;
                        if (state.Flags.GetBit(TimerState.ELAPSED_BIT) == false)
                        {
                            state.Flags.SetBits(TimerState.ELAPSED_BIT);
                            // Perform on timer finished events
                            OnTimerFinished(EventArgs.Empty);
                        }
                    }

                    // Calculate milliseconds to next second tick
                    if (state.Flags.GetBit(TimerState.ELAPSED_BIT) == false)
                    {
                        secondsRemaining -= (long)secondsRemaining;
                    }
                    else
                    {
                        secondsRemaining = (long)secondsRemaining + 1 - secondsRemaining;
                    }
                    int millisecsToWait = (int)Math.Ceiling(1000 * secondsRemaining);

                    System.Threading.Thread.Sleep(millisecsToWait);
                }
            }
            );
        }

        public void Start()
        {
            if (IsRunning())
                return;

            state.Flags.SetBits(TimerState.RUNNING_BIT | TimerState.STARTED_BIT);
            //if (IsElapsed() == false)
            {
                state.TimeStart = DateTime.Now;
            }

            runTask();
        }

        public void StartWithTime(DateTime startTime, double timeLeft)
        {
            state.Flags.SetBits(TimerState.RUNNING_BIT | TimerState.STARTED_BIT);
            state.TimeStart = startTime;

            runTask();
        }

        public double GetTimeLeft()
        {
            return state.TimeLeft;
        }

        public void Stop()
        {
            //if (IsElapsed() == false)
            {
                DateTime timeNow = DateTime.Now;
                TimeSpan timeDelta = timeNow.Subtract(state.TimeStart);

                state.TimeLeft = state.TimeLeft - timeDelta.TotalSeconds;
                //if (state.timeLeft < 0)
                //{
                //    state.timeLeft = 0;
                //}

                state.Flags.ClearBits(TimerState.RUNNING_BIT);

                OnTimerPaused(EventArgs.Empty);
            }
        }

        public void Reset()
        {
            state.Flags.ClearBits(TimerState.RUNNING_BIT | TimerState.ELAPSED_BIT | TimerState.STARTED_BIT);
            state.TimeLeft = state.Duration;
            //timerAdaptor.UpdateDisplay();
            OnDisplayTimeChanged(EventArgs.Empty);
        }

        public void Delete()
        {
            state.Flags.ClearBits(TimerState.RUNNING_BIT);
        }

        static void CheckStatus(Object state)
        {
            TimerService timerService = (TimerService)state;
            //timerService.timerAdaptor.UpdateDisplay();
            timerService.OnDisplayTimeChanged(EventArgs.Empty);
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

        public Tuple<int, int, int> hourMinSec()
        {
            long secondsLeft = (long)Math.Round(CalcSeconds());
            int hours = (int)secondsLeft / 3600;
            secondsLeft -= 3600 * hours;
            int minutes = (int)secondsLeft / 60;
            secondsLeft -= 60 * minutes;
            int seconds = (int)secondsLeft;

            return Tuple.Create<int, int, int>(hours, minutes, seconds);
        }

        public bool IsElapsed()
        {
            return state.Flags.GetBit(TimerState.ELAPSED_BIT);
        }

        public bool IsRunning()
        {
            return state.Flags.GetBit(TimerState.RUNNING_BIT);
        }

        public bool IsStarted()
        {
            return state.Flags.GetBit(TimerState.STARTED_BIT);
        }
    }
}
