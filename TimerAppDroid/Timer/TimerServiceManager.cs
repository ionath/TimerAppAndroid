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

        private TimerServiceManager()
        {
            // Get database path
            dbPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                "database.db3");
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
                    if (existingDBItem == null)
                    {
                        existingDBItem = new TimerDBItem();
                        db.Insert(existingDBItem);
                    }
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


        static public TimerService NewTimerService(TimerDBItem timerDBItem)
        {
            TimerService timerService = new TimerService(timerDBItem.Id, timerDBItem.duration, timerDBItem.timeLeft,
                timerDBItem.alarmName, timerDBItem.timeStart, timerDBItem.running);
            timerService.notificationAdaptor = AndroidNotificationManager.GetAdaptor();

            instance.timerServices.Add(timerService);

            if (timerDBItem.running)
            {
                timerService.Start();
            }

            return timerService;
        }

        static public void DeleteTimerService(TimerService timerService)
        {
            instance.timerServices.Remove(timerService);
            timerService.Delete();
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

        }
    }
}