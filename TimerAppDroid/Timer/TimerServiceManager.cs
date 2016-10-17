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
using System.Collections;
using TimerAppShared;
using System.IO;
using SQLite;

namespace TimerAppDroid
{
    class TimerServiceManager
    {
        static TimerServiceManager instance = new TimerServiceManager();

        List<TimerService> timerServices = new List<TimerService>();
        
        string dbPath;

        // Event handling
        public static event EventHandler ListModified;

        private TimerServiceManager()
        {
            // Get database path
            dbPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "database.db3");
        }

        protected virtual void OnListModified(EventArgs e)
        {
            ListModified?.Invoke(this, e);
        }

        static public int Count
        {
            get
            {
                return instance.timerServices.Count;
            }
        }

        static public TimerServiceManager Instance
        {
            get
            {
                return instance;
            }
        }

        public TimerService this[int index]
        {
            get
            {
                return timerServices[index];
            }
        }

        static public void LoadTimersFromDatabase()
        {
            object locker = new object();
            lock (locker)
            {
                var db = new SQLiteConnection(instance.dbPath);
                db.CreateTable<TimerDBItem>();
                var table = db.Table<TimerDBItem>();
                if (table.Count() > 0)
                {
                    foreach (var t in table)
                    {
                        //AddTimerToListView(t, false);
                        NewTimerService(t);
                    }
                }
            }

            instance.OnListModified(EventArgs.Empty);
        }

        static public void SaveTimersToDatabase()
        {
            object locker = new object();
            lock (locker)
            {
                var db = new SQLiteConnection(instance.dbPath);
                db.CreateTable<TimerDBItem>();
                var table = db.Table<TimerDBItem>();

                foreach (var timerService in instance.timerServices)
                {
                    TimerState timerState = timerService.GetState();

                    var existingDBItem = db.Find<TimerDBItem>(timerState.id);
                    //if (existingDBItem == null)
                    //{
                    //    existingDBItem = new TimerDBItem();
                    //    db.Insert(existingDBItem);
                    //}
                    if (existingDBItem != null)
                    {
                        existingDBItem.alarmName = timerState.alarmName;
                        existingDBItem.duration = timerState.duration;
                        existingDBItem.timeLeft = timerState.timeLeft;
                        existingDBItem.timeStart = timerState.timeStart;
                        existingDBItem.running = timerState.flags.GetBit(TimerState.RUNNING_BIT);
                        existingDBItem.started = timerState.flags.GetBit(TimerState.STARTED_BIT);

                        db.Update(existingDBItem);
                    }

                }
            }
        }

        static public void SaveTimerToDatabase(TimerDBItem timerDBItem)
        {
            object locker = new object();
            lock (locker)
            {
                var db = new SQLiteConnection(instance.dbPath);
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

        static void DeleteTimerFromDatabase(TimerService timerService)
        {
            if (timerService != null)
            {
                object locker = new object();
                lock (locker)
                {
                    var db = new SQLiteConnection(instance.dbPath);
                    db.Delete<TimerDBItem>(timerService.GetState().id);
                }
            }
        }


        static public TimerService NewTimerService(TimerDBItem timerDBItem)
        {
            TimerService timerService = new TimerService(timerDBItem);
            timerService.notificationAdaptor = AndroidNotificationManager.GetAdaptor();

            instance.timerServices.Add(timerService);
            
            instance.OnListModified(EventArgs.Empty);

            return timerService;
        }

        static public void DeleteTimerService(TimerService timerService)
        {
            instance.timerServices.Remove(timerService);
            timerService.Delete();
            DeleteTimerFromDatabase(timerService);

            instance.OnListModified(EventArgs.Empty);
        }

        static public TimerService GetTimerServiceWithId(int id)
        {
            foreach (TimerService timerService in instance.timerServices)
            {
                if (timerService.GetState().id == id)
                {
                    return timerService;
                }
            }
            return null;
        }

        static public void SortTimersByActiveAndTimeLeft()
        {
            instance.timerServices.Sort((t1, t2) =>
            {
                // primary sort by active
                int isStarted1 = t1.IsStarted() ? 0 : 1;
                int isStartet2 = t2.IsStarted() ? 0 : 1;
                if (isStarted1 < isStartet2)
                    return -1;
                else if (isStarted1 > isStartet2)
                    return 1;

                // secondary sort by time left
                double timeLeft1 = t1.GetTimeLeft();
                double timeleft2 = t2.GetTimeLeft();
                if (timeLeft1 < timeleft2)
                    return -1;
                else if (timeLeft1 > timeleft2)
                    return 1;
                else return 0;
            });
            
            instance.OnListModified(EventArgs.Empty);
        }
    }
}