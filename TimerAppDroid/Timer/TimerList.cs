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
using SQLite;
using System.IO;
using System.Security;
using Android.Content.Res;

namespace TimerAppDroid
{
    class TimerList
    {
        List<AndroidTimerMVAdapter> timerAdaptors = new List<AndroidTimerMVAdapter>();
        LinearLayout listLayout;
        Activity activity;

        string dbPath;

        public TimerList(Activity _activity, LinearLayout _listLayout)
        {
            activity = _activity;
            listLayout = _listLayout;

            // Get database path
            dbPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "database.db3");
        }

        public void saveToBundle(Bundle bundle)
        {
            int timerCount = timerAdaptors.Count;
            int[] idArray = new int[timerCount];
            long[] timeStartArray = new long[timerCount];
            long[] durationArray = new long[timerCount];
            double[] timeLeftArray = new double[timerCount];
            int[] flagsArray = new int[timerCount];
            int index = 0;
            foreach (AndroidTimerMVAdapter adaptor in timerAdaptors)
            {
                TimerState state = adaptor.timerService.GetState();
                idArray[index] = state.id;
                timeStartArray[index] = state.timeStart.ToBinary();
                durationArray[index] = state.duration;
                timeLeftArray[index] = state.timeLeft;
                flagsArray[index] = state.flags.ToInt();
                index++;
            }
            bundle.PutLongArray("timers.TimeStart", timeStartArray);
            bundle.PutLongArray("timers.duration", durationArray);
            bundle.PutDoubleArray("timers.timeLeft", timeLeftArray);
            bundle.PutIntArray("timers.flags", flagsArray);
        }
        
        public void LoadTimersFromDatabase()
        {
            object locker = new object();
            lock (locker)
            {
                var db = new SQLiteConnection(dbPath);
                db.CreateTable<TimerDBItem>();
                var table = db.Table<TimerDBItem>();
                if (table.Count() > 0)
                {
                    foreach (var t in table)
                    {
                        AddTimerToListView(t, false);
                    }
                }
            }
        }

        public void SaveTimersToDatabase()
        {
            object locker = new object();
            lock (locker)
            {
                var db = new SQLiteConnection(dbPath);
                db.CreateTable<TimerDBItem>();
                var table = db.Table<TimerDBItem>();

                foreach (AndroidTimerMVAdapter timerAdaptor in timerAdaptors)
                {
                    TimerState timerState = timerAdaptor.timerService.GetState();

                    var existingDBItem = db.Get<TimerDBItem>(timerState.id);
                    if (existingDBItem != null)
                    {
                        existingDBItem.alarmName = timerState.alarmName;
                        existingDBItem.duration = timerState.duration;
                        existingDBItem.timeLeft = timerState.timeLeft;
                        existingDBItem.timeStart = timerState.timeStart;
                        existingDBItem.running = timerState.flags.GetBit(TimerState.RUNNING_BIT);

                        db.Update(existingDBItem);
                    }

                }
            }
        }

        public void SaveTimerToDatabase(TimerDBItem timerDBItem)
        {
            object locker = new object();
            lock (locker)
            {
                var db = new SQLiteConnection(dbPath);
                db.CreateTable<TimerDBItem>();
                var existingDBItem = db.Find<TimerDBItem>(timerDBItem.Id);
                if (existingDBItem != null)
                {
                    db.Update(timerDBItem);
                }
                else
                {
                    db.Insert(timerDBItem);
                }
            }
        }

        public AndroidTimerMVAdapter AddTimer(Activity _activity, TimerService _timerService, TimerDBItem _timerDBItem, TextView _timeView, TextView _alarmNameView,
            RelativeLayout _timerLayout, LinearLayout _controlsLayout)
        {
            AndroidTimerMVAdapter timerAdaptor = new AndroidTimerMVAdapter(_activity, _timerService, _timerDBItem, _timeView, _alarmNameView, _timerLayout, _controlsLayout);
            timerAdaptors.Add(timerAdaptor);
            return timerAdaptor;
        }

        public void AddTimerToListView(TimerDBItem timerDBItem, bool startTimerNow)
        {
            TimerAppShared.TimerService timerService = TimerServiceManager.NewTimerService(timerDBItem);
            timerService.notificationAdaptor = AndroidNotificationManager.GetAdaptor();

            string timerStr = timerService.ToString();

            // Get string resources
            string pauseString = activity.Resources.GetString(Resource.String.Pause);
            string resetString = activity.Resources.GetString(Resource.String.Reset);
            string deleteString = activity.Resources.GetString(Resource.String.Delete);
            string startString = activity.Resources.GetString(Resource.String.Start);
            string editString = activity.Resources.GetString(Resource.String.Edit);


            // Create UI elements
            int alarmNameTextId = GenerateIds.GenerateId();
            int timerTextId = GenerateIds.GenerateId();
            
            // Timer Layout
            RelativeLayout timerLayout = new RelativeLayout(activity);
            timerLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            // Time Text View
            TextView timerTextView = new TextView(activity);
            timerTextView.Id = timerTextId;
            timerTextView.Text = timerStr;
            TextViewHelper.SetTextAppearance(activity, timerTextView, Resource.Style.TimerText);
            //timerTextView.Gravity = GravityFlags.CenterHorizontal;
            RelativeLayout.LayoutParams timerParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            timerParams.AddRule(LayoutRules.CenterHorizontal);
            timerParams.AddRule(LayoutRules.AlignParentRight);
            timerTextView.LayoutParameters = timerParams;
            timerLayout.AddView(timerTextView);

            // Alarm Name Text View
            TextView alarmNameTextView = new TextView(activity);
            alarmNameTextView.Id = alarmNameTextId;
            alarmNameTextView.Text = timerDBItem.alarmName;
            TextViewHelper.SetTextAppearance(activity, alarmNameTextView, Resource.Style.TimerAlarmNameText);
            //alarmNameTextView.Gravity = GravityFlags.Left;
            RelativeLayout.LayoutParams alarmNameParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            alarmNameParams.AddRule(LayoutRules.AlignParentLeft);
            alarmNameParams.AddRule(LayoutRules.CenterVertical);
            alarmNameParams.AddRule(LayoutRules.AlignLeft, timerTextId);
            alarmNameTextView.LayoutParameters = alarmNameParams;
            timerLayout.AddView(alarmNameTextView);

            listLayout.AddView(timerLayout);

            // Test controls
            LinearLayout controlsLayout = new LinearLayout(activity);
            controlsLayout.Orientation = Android.Widget.Orientation.Horizontal;
            //controlsLayout.SetHorizontalGravity(GravityFlags.CenterHorizontal);
            controlsLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            //controlsLayout.SetGravity(GravityFlags.CenterHorizontal);
            listLayout.AddView(controlsLayout);

            Button pauseButton = new Button(activity);
            ViewGroup.LayoutParams buttonParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            pauseButton.LayoutParameters = buttonParams;
            if (startTimerNow || timerDBItem.running)
            {
                pauseButton.Text = pauseString;
            }
            else
            {
                pauseButton.Text = startString;
            }
            controlsLayout.AddView(pauseButton);

            Button resetButton = new Button(activity);
            resetButton.LayoutParameters = buttonParams;
            resetButton.Text = resetString;
            controlsLayout.AddView(resetButton);

            Button deleteButton = new Button(activity);
            deleteButton.LayoutParameters = buttonParams;
            deleteButton.Text = deleteString;
            controlsLayout.AddView(deleteButton);

            Button editButton = new Button(activity);
            editButton.LayoutParameters = buttonParams;
            editButton.Text = editString;
            controlsLayout.AddView(editButton);

            // Start controls layout collapsed
            controlsLayout.Enabled = false;
            controlsLayout.Visibility = ViewStates.Gone;

            // Toggle Controls visibility
            timerLayout.Click += delegate
            {
                ExpandControlsLayout(controlsLayout);
            };


            // Create adaptor
            AndroidTimerMVAdapter timerAdaptor = AddTimer(activity, timerService, timerDBItem, timerTextView, alarmNameTextView, timerLayout, controlsLayout);

            if (timerDBItem.running)
            {
                timerService.StartWithTime(timerDBItem.timeStart, timerDBItem.timeLeft);
            }
            else if (startTimerNow)
            {
                timerService.Start();
            }


            pauseButton.Click += delegate
            {
                if (timerAdaptor.timerService.IsRunning())
                {
                    timerAdaptor.timerService.Stop();
                    pauseButton.Text = startString;
                }
                else
                {
                    timerAdaptor.timerService.Start();
                    pauseButton.Text = pauseString;

                    SortTimersByActiveAndTimeLeft();
                }

                SaveTimersToDatabase();
            };


            resetButton.Click += delegate
            {
                timerAdaptor.timerService.Reset();
                if (timerAdaptor.timerService.IsRunning())
                {
                    pauseButton.Text = pauseString;
                }
                else
                {
                    pauseButton.Text = startString;
                }
                CollapseControlsLayout(controlsLayout);
                SortTimersByActiveAndTimeLeft();

                SaveTimersToDatabase();
            };

            deleteButton.Click += delegate
            {
                listLayout.RemoveView(timerLayout);
                listLayout.RemoveView(controlsLayout);
                
                DeleteTimer(timerAdaptor);
            };

            editButton.Click += delegate
            {
                // Show timer editor
                var intent = new Intent(activity, typeof(TimerEditorActivity));
                intent.PutExtra("id", timerDBItem.Id);
                activity.StartActivityForResult(intent, MainActivity.REQUEST_CODE_EDIT_TIMER);
            };

            timerAdaptor.UpdateDisplay();
        }

        public void DeleteTimer(AndroidTimerMVAdapter timerAdaptor)
        {
            TimerServiceManager.DeleteTimerService(timerAdaptor.timerService);

            timerAdaptors.Remove(timerAdaptor);

            DeleteTimerFromDatabase(timerAdaptor.timerDBItem);
        }

        void DeleteTimerFromDatabase(TimerDBItem timerDBItem)
        {
            object locker = new object();
            lock (locker)
            {
                var db = new SQLiteConnection(dbPath);
                db.Delete<TimerDBItem>(timerDBItem.Id);
            }
        }

        public AndroidTimerMVAdapter GetTimerAdaptorWithId(int id)
        {
            foreach (AndroidTimerMVAdapter timerAdaptor in timerAdaptors)
            {
                if (timerAdaptor.timerService.GetState().id == id)
                {
                    return timerAdaptor;
                }
            }
            return null;
        }

        public void SortTimersByActiveAndTimeLeft()
        {
            timerAdaptors.Sort((t1, t2) =>
            {
                // primary sort by active
                int isStarted1 = t1.timerService.IsStarted() ? 0 : 1;
                int isStartet2 = t2.timerService.IsStarted() ? 0 : 1;
                if (isStarted1 < isStartet2)
                    return -1;
                else if (isStarted1 > isStartet2)
                    return 1;

                // secondary sort by time left
                double timeLeft1 = t1.timerService.GetTimeLeft();
                double timeleft2 = t2.timerService.GetTimeLeft();
                if (timeLeft1 < timeleft2)
                    return -1;
                else if (timeLeft1 > timeleft2)
                    return 1;
                else return 0;
            });
            
            UpdateLayout(listLayout);
        }
        
        public void UpdateLayout(LinearLayout layout)
        {
            // clear the view of items
            layout.RemoveAllViews();

            // Add views for each timer
            foreach (AndroidTimerMVAdapter adaptor in timerAdaptors)
            {
                layout.AddView(adaptor.timerLayout);
                layout.AddView(adaptor.controlsLayout);
            }
        }

        public void ExpandControlsLayout(LinearLayout controlsLayout)
        {
            foreach (AndroidTimerMVAdapter adaptor in timerAdaptors)
            {
                if (adaptor.controlsLayout == controlsLayout && adaptor.controlsLayout.Enabled == false)
                {
                    adaptor.controlsLayout.Enabled = true;
                    adaptor.controlsLayout.Visibility = ViewStates.Visible;
                }
                else
                {
                    adaptor.controlsLayout.Enabled = false;
                    adaptor.controlsLayout.Visibility = ViewStates.Gone;
                }
            }
        }

        public void CollapseControlsLayout(LinearLayout controlsLayout)
        {
            controlsLayout.Enabled = false;
            controlsLayout.Visibility = ViewStates.Gone;
        }

        public void StartAllTimers()
        {
            foreach (AndroidTimerMVAdapter timerAdaptor in timerAdaptors)
            {
                timerAdaptor.timerService.Start();
            }
        }

        public void StopAllTimers()
        {
            foreach (AndroidTimerMVAdapter timerAdaptor in timerAdaptors)
            {
                timerAdaptor.timerService.Stop();
            }
        }
    }
}