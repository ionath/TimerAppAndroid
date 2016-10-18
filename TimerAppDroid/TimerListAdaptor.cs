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
    public class TimerListAdaptor : BaseAdapter<TimerService>
    {
        Activity context;

        // Expanded view
        TimerListItemView expandedView;
        TimerService expandedViewTimerService;

        public TimerListAdaptor(Activity context) : base()
        {
            this.context = context;

            TimerServiceManager.ListModified += delegate
            {
                ContentChanged();
            };
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
                view = new TimerListItemView(context, this, null);
            }
            var timerService = TimerServiceManager.Instance[position];
            
            var timerTextView = view.FindViewById<TextView>(TimerListItemView.timerTextId);
            //timerTextView.Text = timerService.ToString();

            var alarmNameTextView = view.FindViewById<TextView>(TimerListItemView.alarmNameId);
            alarmNameTextView.Text = timerService.GetState().alarmName;
            
            view.updateViewForTimer(timerService, delegate {
                
                context.RunOnUiThread(() =>
                {
                    string timerString = view.timerService.ToString();
                    bool isElapsed = view.timerService.IsElapsed();
                    bool isStarted = view.timerService.IsStarted();

                    if (isElapsed)
                    {
                        alarmNameTextView.SetTextColor(MainActivity.elapsedColor);
                        timerTextView.SetTextColor(MainActivity.elapsedColor);
                    }
                    else if (isStarted)
                    {
                        alarmNameTextView.SetTextColor(MainActivity.defaultColor);
                        timerTextView.SetTextColor(MainActivity.defaultColor);
                    }
                    else
                    {
                        alarmNameTextView.SetTextColor(MainActivity.inactiveColor);
                        timerTextView.SetTextColor(MainActivity.inactiveColor);
                    }
                    alarmNameTextView.Text = view.timerService.GetState().alarmName;
                    timerTextView.Text = timerString;
                });
            });
            timerService.ForceDisplayTimeChangedEvent();

            if (timerService == expandedViewTimerService)
            {
                ExpandItem(view);
            }
            else
            {
                CollapseItem(view);
            }

            return view;
        }

        public void ExpandItem(TimerListItemView expandView)
        {
            // Collapse previous expanded view and expand given view
            if (expandedView != null)
            {
                expandedView.collapseControls();
            }
            expandedView = expandView;
            expandedViewTimerService = expandView.timerService;
            expandView.expandControls();
        }

        public void CollapseItem(TimerListItemView collapseView)
        {
            if (expandedView == collapseView)
            {
                expandedView = null;
            }
            collapseView.collapseControls();
        }
    }
}