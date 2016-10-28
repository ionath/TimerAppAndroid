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
using Android.Media;

namespace TimerAppDroid
{
    public class AndroidNotificationManager
    {
        static AndroidNotificationManager instance = new AndroidNotificationManager();

        AndroidNotificationAdaptor adaptor;

        Activity context;
        Ringtone defaultTone;

        NotificationManager notificationManager;

        Dictionary<int, AndroidNotificationAdaptor> adaptorMap = new Dictionary<int, AndroidNotificationAdaptor>();

        public static void Initialize(MainActivity context, Ringtone defaultTone)
        {
            instance.context = context;
            instance.defaultTone = defaultTone;
            instance.notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
            if (instance.adaptor == null)
            {
                instance.adaptor = new AndroidNotificationAdaptor(context, defaultTone, instance.notificationManager);
            }
        }

        public static AndroidNotificationAdaptor GetAdaptor()
        {
            return instance.adaptor;
        }

        public static void UpdateNotification(TimerService timerService)
        {
            var id = timerService.State.Id;
            AndroidNotificationAdaptor adaptor = null;
            if (instance.adaptorMap.ContainsKey(id) == false)
            {
                adaptor = new AndroidNotificationAdaptor(instance.context, instance.defaultTone, instance.notificationManager);
                adaptor.CreateNotification(timerService);
                instance.adaptorMap.Add(id, adaptor);
            }
            else
            {
                adaptor = instance.adaptorMap[id];
            }
            adaptor.UpdateBackgroundNotification();
        }

        public static void PostNotification(TimerState timerState)
        {
            if (instance.adaptor != null)
            {
                instance.adaptor.PostNotification(timerState);
            }
        }

        public static void CancelNotification(int notificationId)
        {
            if (instance.adaptor != null)
            {
                instance.adaptor.CancelNotification(notificationId);
            }
        }
        public static void CancelNotification(TimerService timerService)
        {
            var id = timerService.State.Id;

            if (instance.adaptorMap.ContainsKey(id))
            {
                var adaptor = instance.adaptorMap[id];
                adaptor.CancelNotification();
            }
        }
    }
}