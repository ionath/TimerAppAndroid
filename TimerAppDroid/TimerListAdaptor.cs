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
using Java.Lang;
using TimerAppShared;

namespace TimerAppDroid
{
    class TimerListAdaptor : BaseAdapter<TimerService>
    {
        Activity context;

        public TimerListAdaptor(Activity context) : base()
        {
            this.context = context;
        }

        public void ContentChanged()
        {
            NotifyDataSetChanged();
        }

        public override int Count
        {
            get
            {
                return TimerServiceManager.Count;
            }
        }

        public override TimerService this[int position]
        {
            get
            {
                return TimerServiceManager.Instance[position];
            }
        }
        
        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            TimerListItemView view = (TimerListItemView)convertView;
            if (view == null)
            {
                view = new TimerListItemView(context, null);
            }
            var timerService = TimerServiceManager.Instance[position];

            var timerTextView = view.FindViewById<TextView>(TimerListItemView.timerTextId);
            //timerTextView.Text = timerService.ToString();

            var alarmNameTextView = view.FindViewById<TextView>(TimerListItemView.alarmNameId);
            alarmNameTextView.Text = timerService.GetState().alarmName;
            
            view.setUpdateDisplayEventHandler(timerService, delegate {
                string timerString = timerService.ToString();
                bool isElapsed = timerService.IsElapsed();
                bool isStarted = timerService.IsStarted();

                context.RunOnUiThread(() =>
                {
                    timerTextView.Text = timerString;
                    if (isElapsed)
                    {
                        alarmNameTextView.SetTextColor(MainActivity.defaultColor);
                        timerTextView.SetTextColor(MainActivity.elapsedColor);
                    }
                    else if (isStarted)
                    {
                        alarmNameTextView.SetTextColor(MainActivity.defaultColor);
                        timerTextView.SetTextColor(MainActivity.defaultColor);
                    }
                    else
                    {
                        alarmNameTextView.SetTextColor(MainActivity.unactiveColor);
                        timerTextView.SetTextColor(MainActivity.unactiveColor);
                    }
                });
            });
            timerService.ForceDisplayTimeChangedEvent();

            return view;
        }
    }
}