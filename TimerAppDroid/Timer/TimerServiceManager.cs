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

namespace TimerAppDroid
{
    class TimerServiceManager
    {
        static TimerServiceManager instance = new TimerServiceManager();

        ArrayList timerServices = new ArrayList();

        private TimerServiceManager()
        {

        }

        static public TimerService NewTimerService(TimerDBItem timerDBItem)
        {
            TimerService timerService = new TimerService(timerDBItem.Id, timerDBItem.duration, timerDBItem.timeLeft,
                timerDBItem.alarmName, timerDBItem.timeStart, timerDBItem.running);

            instance.timerServices.Add(timerService);

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
    }
}