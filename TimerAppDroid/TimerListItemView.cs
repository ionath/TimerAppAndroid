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
        Button pauseButton;

        Context context;

        public const int timerTextId = 1;
        public const int alarmNameId = 2;

        EventHandler updateDisplayEventHandler;
        TimerService timerService;

        public TimerListItemView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            this.context = context;
            initialize();
        }

        public TimerListItemView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            this.context = context;
            initialize();
        }

        private void initialize()
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
            
            // Test controls
            controlsLayout = new LinearLayout(context);
            controlsLayout.Orientation = Android.Widget.Orientation.Horizontal;
            controlsLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

            this.AddView(controlsLayout);

            // Pause Button
            pauseButton = new Button(context);
            ViewGroup.LayoutParams buttonParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            pauseButton.LayoutParameters = buttonParams;
            pauseButton.Text = AppStrings.PauseString;
            controlsLayout.AddView(pauseButton);

            collapseControls();
        }

        public void updatePauseButtonState()
        {
            if (timerService != null)
            {
                if (timerService.IsRunning())
                {
                    pauseButton.Text = AppStrings.PauseString;
                }
                else
                {
                    pauseButton.Text = AppStrings.StartString;
                }
            }
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

        public void updateViewForTimer(TimerService timerService, EventHandler eventHandler)
        {
            clearUpdateDisplayEventHandler();

            this.timerService = timerService;
            updateDisplayEventHandler = eventHandler;
            this.timerService.DisplayTimeChanged += eventHandler;

            updatePauseButtonState();
        }
    }
}