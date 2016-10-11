using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using TimerAppShared;

namespace TimerAppDroid
{
    class AndroidTimerMVAdapter : TimerMVAdapter
    {
        TextView timeView;
        TextView alarmNameView;
        EventHandler displayTimeChangedHandler;
        public RelativeLayout timerLayout { get; }
        public LinearLayout controlsLayout { get; }
        public TimerService timerService { get; private set; }
        Activity activity;
        public TimerDBItem timerDBItem { get; set; }

        public AndroidTimerMVAdapter(Activity _activity, TimerService _timerService, TimerDBItem _timerDBItem, TextView _timeView, TextView _alarmNameView,
            RelativeLayout _timerLayout, LinearLayout _controlsLayout)
        {
            activity = _activity;
            timerService = _timerService;
            timerDBItem = _timerDBItem;
            timeView = _timeView;
            alarmNameView = _alarmNameView;
            timerLayout = _timerLayout;
            controlsLayout = _controlsLayout;

            //timerService.timerAdaptor = this;
            displayTimeChangedHandler = delegate
            {
                UpdateDisplay();
            };
            timerService.DisplayTimeChanged += displayTimeChangedHandler;
        }
        ~AndroidTimerMVAdapter()
        {
            if (timerService != null && displayTimeChangedHandler != null)
            {
                timerService.DisplayTimeChanged -= displayTimeChangedHandler;
            }
        }

        public void UpdateDisplay()
        {
            string timerString = timerService.ToString();
            //double seconds = timerService.CalcSeconds();
            //string timerString = seconds.ToString("F3") + " " + ((long)Math.Round(seconds)).ToString() + " " + timerService.Updatecount.ToString();
            bool isElapsed = timerService.IsElapsed();
            bool isStarted = timerService.IsStarted();

            activity.RunOnUiThread(() =>
            {
                timeView.Text = timerString;
                if (isElapsed)
                {
                    alarmNameView.SetTextColor(MainActivity.defaultColor);
                    timeView.SetTextColor(MainActivity.elapsedColor);
                }
                else if (isStarted)
                {
                    alarmNameView.SetTextColor(MainActivity.defaultColor);
                    timeView.SetTextColor(MainActivity.defaultColor);
                }
                else
                {
                    alarmNameView.SetTextColor(MainActivity.unactiveColor);
                    timeView.SetTextColor(MainActivity.unactiveColor);
                }

                alarmNameView.Text = timerService.GetState().alarmName;
            });
        }
    }
}