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

namespace TimerAppDroid
{
    public class TimerListItemView : RelativeLayout
    {
        Context context;

        public const int timerTextId = 1;
        public const int alarmNameId = 2;

        EventHandler updateDisplayEventHandler;
        int listPosition = -1;

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

            this.AddView(timerLayout);
        }

        public void clearUpdateDisplayEventHandler()
        {
            if (updateDisplayEventHandler != null)
            {
                TimerServiceManager.Instance[listPosition].DisplayTimeChanged -= updateDisplayEventHandler;
                updateDisplayEventHandler = null;
                listPosition = -1;
            }
        }

        public void setUpdateDisplayEventHandler(int position, EventHandler eventHandler)
        {
            clearUpdateDisplayEventHandler();

            listPosition = position;
            updateDisplayEventHandler = eventHandler;
            TimerServiceManager.Instance[position].DisplayTimeChanged += eventHandler;
        }
    }
}