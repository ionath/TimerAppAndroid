using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using TimerAppShared;

namespace TimerAppDroid
{
    public class TimerListItemView : LinearLayout
    {
        LinearLayout controlsLayout;
        public LinearLayout ControlsLayout
        {
            get
            {
                return controlsLayout;
            }
        }


        Context context;

        public const int timerTextId = 1;
        public const int alarmNameId = 2;

        EventHandler updateDisplayEventHandler;
        TimerService timerService;

        public TimerListItemView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            this.context = context;
            Initialize();
        }

        public TimerListItemView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            this.context = context;
            Initialize();
        }

        private void Initialize()
        {
            this.Orientation = Orientation.Vertical;

            // Timer Layout
            RelativeLayout timerLayout = new RelativeLayout(context);
            timerLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            // Time Text View
            TextView timerTextView = new TextView(context);
            timerTextView.Id = timerTextId;
            //timerTextView.Text = timerStr;
            timerTextView.Text = "Timer Text";
            TextViewHelper.SetTextAppearance(context, timerTextView, Resource.Style.TimerText);
            RelativeLayout.LayoutParams timerParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            timerParams.AddRule(LayoutRules.CenterHorizontal);
            timerParams.AddRule(LayoutRules.AlignParentRight);
            timerTextView.LayoutParameters = timerParams;
            timerLayout.AddView(timerTextView);

            // Alarm Name Text View
            TextView alarmNameTextView = new TextView(context);
            alarmNameTextView.Id = alarmNameId;
            //alarmNameTextView.Text = timerDBItem.alarmName;
            alarmNameTextView.Text = "Alarm Name";
            TextViewHelper.SetTextAppearance(context, alarmNameTextView, Resource.Style.TimerAlarmNameText);
            RelativeLayout.LayoutParams alarmNameParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            alarmNameParams.AddRule(LayoutRules.AlignParentLeft);
            alarmNameParams.AddRule(LayoutRules.CenterVertical);
            alarmNameParams.AddRule(LayoutRules.AlignLeft, timerTextId);
            alarmNameTextView.LayoutParameters = alarmNameParams;
            timerLayout.AddView(alarmNameTextView);

            timerLayout.Click += delegate
            {
                if (ControlsLayout != null)
                {
                    if (ControlsLayout.Enabled)
                    {
                        collapseControls();
                    }
                    else
                    {
                        expandControls();
                    }
                }
            };

            this.AddView(timerLayout);

            // Get string resources
            string pauseString = context.Resources.GetString(Resource.String.Pause);
            string resetString = context.Resources.GetString(Resource.String.Reset);
            string deleteString = context.Resources.GetString(Resource.String.Delete);
            string startString = context.Resources.GetString(Resource.String.Start);
            string editString = context.Resources.GetString(Resource.String.Edit);

            // Test controls
            controlsLayout = new LinearLayout(context);
            controlsLayout.Orientation = Android.Widget.Orientation.Horizontal;
            controlsLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            this.AddView(controlsLayout);

            Button pauseButton = new Button(context);
            ViewGroup.LayoutParams buttonParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            pauseButton.LayoutParameters = buttonParams;
            //if (startTimerNow || timerDBItem.running)
            if (true)
            {
                pauseButton.Text = pauseString;
            }
            //else
            //{
            //    pauseButton.Text = startString;
            //}
            controlsLayout.AddView(pauseButton);

            collapseControls();
        }

        public void expandControls()
        {
            ControlsLayout.Enabled = true;
            ControlsLayout.Visibility = ViewStates.Visible;
        }

        public void collapseControls()
        {
            ControlsLayout.Enabled = false;
            ControlsLayout.Visibility = ViewStates.Gone;
        }

        public void clearUpdateDisplayEventHandler()
        {
            if (updateDisplayEventHandler != null)
            {
                timerService.DisplayTimeChanged -= updateDisplayEventHandler;
                updateDisplayEventHandler = null;
                timerService = null;
            }
        }

        public void setUpdateDisplayEventHandler(TimerService timerService, EventHandler eventHandler)
        {
            clearUpdateDisplayEventHandler();

            this.timerService = timerService;
            updateDisplayEventHandler = eventHandler;
            this.timerService.DisplayTimeChanged += eventHandler;
        }
    }
}