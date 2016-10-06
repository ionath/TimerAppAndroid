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

namespace TimerAppDroid
{
    public class AndroidNotificationManager
    {
        static AndroidNotificationManager instance = new AndroidNotificationManager();

        AndroidNotificationAdaptor adaptor;

        public static void Initialize(MainActivity context)
        {
            if (instance.adaptor == null)
            {
                instance.adaptor = new AndroidNotificationAdaptor(context);
            }
        }

        public static AndroidNotificationAdaptor GetAdaptor()
        {
            return instance.adaptor;
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
    }
}