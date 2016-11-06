using System;
using System.Collections;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading;
using TimerAppShared;
using Android.Media;
using System.IO;
using SQLite;
using System.Collections.Generic;
using System.Security;
using Android.Content.PM;

namespace TimerAppDroid
{
    [SecurityCritical]
    [Activity(Name="com.chaostrend.timerapp",
        Label = "John's Timer",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        ListView timerListView;
        TimerListAdaptor timerListAdaptor;

        int lastTimerId = 0;
        
        AndroidNotificationAdaptor notificationAdaptor;

        public static Android.Graphics.Color defaultColor = Android.Graphics.Color.White;
        public static Android.Graphics.Color elapsedColor = Android.Graphics.Color.Crimson;
        public static Android.Graphics.Color inactiveColor = Android.Graphics.Color.Gray;

        const int REQUEST_CODE_ADD_TIMER = 1;
        const int REQUEST_RINGTONE_PICKER = 2;
        public const int REQUEST_CODE_PENDING_INTENT = 3;
        public const int REQUEST_CODE_EDIT_TIMER = 4;

        const string defaultSettingsKey = "default";

        //BitField flags;
        
        const int PAUSE_ALL_BIT = 1;

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Load string resources
            AppStrings.updateStrings(this);

            // Initialise Notification manager
            var defaultRingtone = RingtoneManager.GetRingtone(this, RingtoneManager.GetDefaultUri(RingtoneType.Alarm));
            AndroidNotificationManager.Initialize(this, defaultRingtone);
            notificationAdaptor = AndroidNotificationManager.GetAdaptor();

            // Load timers
            TimerServiceManager.LoadTimersFromDatabase();
            TimerServiceManager.SortTimersByActiveAndTimeLeft();

            // Load settings
            var preferences = GetPreferences(FileCreationMode.Private);
            string ringtoneString = preferences.GetString("ringtone", null);
            if (ringtoneString != null)
            {
                Android.Net.Uri ringtoneUri = Android.Net.Uri.Parse(ringtoneString);
                var alarmTone = RingtoneManager.GetRingtone(this, ringtoneUri);
                notificationAdaptor.AlarmTone = alarmTone;
            }
                

            TimerAppStatus.SetAppState(TimerAppStatus.STATE_ACTIVE);

            // Set our view from the "Main" layout resource
            SetContentView(Resource.Layout.Main);

            timerListView = FindViewById<ListView>(Resource.Id.timerListView);
            if (timerListView != null)
            {
                timerListAdaptor = new TimerListAdaptor(this);
                timerListView.Adapter = timerListAdaptor;
            }
            
            Button addTimerButton = FindViewById<Button>(Resource.Id.AddTimerButton);
            Button settingsButton = FindViewById<Button>(Resource.Id.SettingsButton);

            addTimerButton.Click += delegate
            {
                // Show timer editor
                var intent = new Intent(this, typeof(TimerEditorActivity));
                StartActivityForResult(intent, REQUEST_CODE_ADD_TIMER);
            };
            
            settingsButton.Click += delegate
            {
                Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
                StartActivityForResult(intent, REQUEST_RINGTONE_PICKER);
            };
        }

        public void LaunchActivity(Intent intent)
        {
            StartActivity(intent);
        }

        protected override void OnResume()
        {
            base.OnResume(); // Always call the superclass first.

            TimerAppStatus.SetAppState(TimerAppStatus.STATE_ACTIVE);
        }

        protected override void OnPause()
        {
            base.OnPause(); // Always call the superclass first

            TimerAppStatus.SetAppState(TimerAppStatus.STATE_PAUSED);
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case (REQUEST_CODE_ADD_TIMER):
                    {
                        if (resultCode == Result.Ok)
                        {
                            lastTimerId++;

                            // Extract the data returned from the child Activity.
                            int hour = data.GetIntExtra("hour", 0);
                            int minute = data.GetIntExtra("minute", 0);
                            int second = data.GetIntExtra("second", 0);
                            string alarmName = data.GetStringExtra("alarmName");
                            bool start = data.GetBooleanExtra("start", true);

                            var timerDBItem = new TimerDBItem();
                            timerDBItem.duration = 3600 * hour + 60 * minute + second;
                            timerDBItem.timeLeft = timerDBItem.duration;
                            timerDBItem.timeStart = DateTime.Now;
                            timerDBItem.alarmName = alarmName;
                            timerDBItem.running = start;

                            var timerService = TimerServiceManager.NewTimerService(timerDBItem);
                            TimerServiceManager.SaveTimerToDatabase(timerService);
                            TimerServiceManager.SortTimersByActiveAndTimeLeft();
                        }
                        break;
                    }
                case (REQUEST_CODE_EDIT_TIMER):
                    {
                        if (resultCode == Result.Ok)
                        {
                            TimerServiceManager.SaveTimersToDatabase();
                            TimerServiceManager.SortTimersByActiveAndTimeLeft();
                        }
                        break;
                    }
                case (REQUEST_RINGTONE_PICKER):
                    {
                        if (resultCode == Result.Ok)
                        {
                            Android.Net.Uri ringtoneUri = (Android.Net.Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                            var alarmTone = RingtoneManager.GetRingtone(this, ringtoneUri);
                            notificationAdaptor.AlarmTone = alarmTone;

                            // Save ringtone in settings
                            var preferences = GetPreferences(FileCreationMode.Private);
                            ISharedPreferencesEditor editor = preferences.Edit();
                            editor.PutString("ringtone", ringtoneUri.ToString());
                            editor.Commit();
                        }
                        break;
                    }
            }
        }
        
    }
}

