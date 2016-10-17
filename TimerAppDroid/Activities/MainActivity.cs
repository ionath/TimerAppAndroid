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

namespace TimerAppDroid
{
    [SecurityCritical]
    [Activity(Name="com.chaostrend.timerapp", Label = "John's Timer", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class MainActivity : Activity
    {
        // List of timers
        //TimerList timerList;

        ListView timerListView;
        TimerListAdaptor timerListAdaptor;

        int lastTimerId = 0;
        
        AndroidNotificationAdaptor notificationAdaptor;

        public static Android.Graphics.Color defaultColor = Android.Graphics.Color.White;
        public static Android.Graphics.Color elapsedColor = Android.Graphics.Color.Crimson;
        public static Android.Graphics.Color unactiveColor = Android.Graphics.Color.Gray;

        const int REQUEST_CODE_ADD_TIMER = 1;
        const int REQUEST_RINGTONE_PICKER = 2;
        public const int REQUEST_CODE_PENDING_INTENT = 3;
        public const int REQUEST_CODE_EDIT_TIMER = 4;

        const string defaultSettingsKey = "default";

        BitField flags;
        
        const int PAUSE_ALL_BIT = 1;

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            
            //timerList.saveToBundle(outState);
        }
        
        class SampleTabFragment : Fragment
        {
            public override View OnCreateView(LayoutInflater inflater,
                ViewGroup container, Bundle savedInstanceState)
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                var view = inflater.Inflate(
                    Resource.Layout.sampleTab, container, false);

                var sampleTextView =
                    view.FindViewById<TextView>(Resource.Id.sampleTextView);
                sampleTextView.Text = "sample fragment text";


                return view;
            }
        }
        
        [SecurityCritical]
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            TimerAppStatus.SetAppState(TimerAppStatus.STATE_ACTIVE);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Load string resources
            AppStrings.updateStrings(this);

            // Test tab button
            this.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            var tab = this.ActionBar.NewTab();
            tab.SetText("Tab1");
            tab.SetIcon(Resource.Drawable.notification_small);

            tab.TabSelected += delegate (object sender, ActionBar.TabEventArgs e) {
                e.FragmentTransaction.Add(Resource.Id.fragmentContainer,
                    new SampleTabFragment());
            };
            ActionBar.AddTab(tab);

            // Initialise Notification manager
            AndroidNotificationManager.Initialize(this);
            notificationAdaptor = AndroidNotificationManager.GetAdaptor();
            
            // Load timers
            TimerServiceManager.LoadTimersFromDatabase();
            TimerServiceManager.SortTimersByActiveAndTimeLeft();
            
            timerListView = FindViewById<ListView>(Resource.Id.timerListView);
            if (timerListView != null)
            {
                timerListAdaptor = new TimerListAdaptor(this);
                timerListView.Adapter = timerListAdaptor;
                timerListView.ItemClick += OnListItemClick;
            }
            
            // Load settings
            var preferences = GetPreferences(FileCreationMode.Private);
            string ringtoneString = preferences.GetString("ringtone", null);
            if (ringtoneString != null)
            {
                Android.Net.Uri ringtoneUri = Android.Net.Uri.Parse(ringtoneString);
                notificationAdaptor.AlarmTone = ringtoneUri;
            }

            if (bundle != null)
            {

            }
            
            Button addTimerButton = FindViewById<Button>(Resource.Id.AddTimerButton);
            Button pauseAllButton = FindViewById<Button>(Resource.Id.PauseAllButton);
            Button settingsButton = FindViewById<Button>(Resource.Id.SettingsButton);

            addTimerButton.Click += delegate
            {
                // Show timer editor
                var intent = new Intent(this, typeof(TimerEditorActivity));
                StartActivityForResult(intent, REQUEST_CODE_ADD_TIMER);
            };

            pauseAllButton.Click += delegate
            {
                if (flags.GetBit(PAUSE_ALL_BIT))
                {
                    //timerList.StartAllTimers();
                    // TODO: 
                    pauseAllButton.Text = "Pause All";
                    flags.ClearBits(PAUSE_ALL_BIT);
                }
                else
                {
                    //timerList.StopAllTimers();
                    // TODO: 
                    pauseAllButton.Text = "Resume All";
                    flags.SetBits(PAUSE_ALL_BIT);
                }
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

            // Save timers to database
            //timerList.SaveTimersToDatabase();
            // TODO: 
        }

        void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var listView = sender as ListView;
            var t = TimerServiceManager.Instance[e.Position];
            Android.Widget.Toast.MakeText(this, t.GetState().alarmName, Android.Widget.ToastLength.Short).Show();
            
            var listItemView = e.View as TimerListItemView;
            if (listItemView != null)
            {
                // Toggle controls enabled
                listItemView.ControlsLayout.Enabled = !listItemView.ControlsLayout.Enabled;
            }
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
                            TimerServiceManager.SaveTimerToDatabase(timerDBItem);

                            //if (timerListAdaptor != null)
                            //{
                            //    timerListAdaptor.ContentChanged();
                            //}

                            // Save timer to database
                            //timerList.SaveTimerToDatabase(timerDBItem);

                                //timerList.AddTimerToListView(timerDBItem, start);

                                //timerList.SortTimersByActiveAndTimeLeft();
                                // TODO: 
                        }
                        break;
                    }
                case (REQUEST_CODE_EDIT_TIMER):
                    {
                        if (resultCode == Result.Ok)
                        {
                            //timerList.SortTimersByActiveAndTimeLeft();
                            //timerList.SaveTimersToDatabase();
                            // TODO: 
                        }
                        break;
                    }
                case (REQUEST_RINGTONE_PICKER):
                    {
                        if (resultCode == Result.Ok)
                        {
                            Android.Net.Uri ringtoneUri = (Android.Net.Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                            notificationAdaptor.AlarmTone = ringtoneUri;

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

