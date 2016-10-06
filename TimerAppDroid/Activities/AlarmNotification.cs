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

namespace TimerAppDroid
{
    [Activity(Label = "AlarmNotification")]
    public class AlarmNotification : Activity
    {
        int notificationId = 0;
        string alarmName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            notificationId = Intent.GetIntExtra("notificationId", 0);
            alarmName = Intent.GetStringExtra("alarmName");


            // Set our view from the "AlarmNotification" layout resource
            SetContentView(Resource.Layout.AlarmNotification);

            TextView alarmNameText = FindViewById<TextView>(Resource.Id.alarmNameTextView);
            alarmNameText.Text = alarmName;

            Button dismissButton = FindViewById<Button>(Resource.Id.dismissButton);
            dismissButton.Click += delegate
            {
                AndroidNotificationManager.CancelNotification(notificationId);
                Finish();
            };
        }
    }
}