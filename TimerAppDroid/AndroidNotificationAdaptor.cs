//using System;
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
using System;
using Android.Content.Res;
using System.Threading.Tasks;

namespace TimerAppDroid
{
    public class AndroidNotificationAdaptor : NotificationAdaptor
    {
        MainActivity context;
        Ringtone currentlyPlayingTone;
        public int defaultAlarmTimeout { get; set; }
        public Android.Net.Uri AlarmTone { get; set; }

        int lastNotificationId = 0;

        public AndroidNotificationAdaptor(MainActivity _context)
        {
            context = _context;
            AlarmTone = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            defaultAlarmTimeout = 30;
        }
        
        public void PostNotification(TimerState timerState)
        {
            if (TimerAppStatus.GetAppState() == TimerAppStatus.STATE_ACTIVE)
            {
                PostForegroundNotification(timerState);
            }
            else
            {
                PostBackgroundNotification(timerState);
            }

            lastNotificationId = timerState.id;
        }

        public void CancelNotification(int notificationId)
        {
            if (lastNotificationId == notificationId)
            {
                StopAlarmTone();
            }

            NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Cancel(notificationId);
        }

        Intent CreateIntent(TimerState timerState)
        {
            Intent intent = new Intent(context, typeof(AlarmNotification));
            intent.PutExtra("notificationId", timerState.id);
            intent.PutExtra("alarmName", timerState.alarmName);
            return intent;
        }

        void PostBackgroundNotification(TimerState timerState)
        {
            Intent intent = CreateIntent(timerState);

            const int pendingIntentId = MainActivity.REQUEST_CODE_PENDING_INTENT;
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);


            //Android.Net.Uri defaultTone = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            //Android.Net.Uri tone = Android.Net.Uri.Parse("android.resource://" + context.PackageName + "/Raw/" + Resource.Raw.elegant_ringtone);
            Notification.Builder builder = new Notification.Builder(context)
                .SetContentTitle(timerState.alarmName)
                .SetContentText("Timer has finished")
                .SetContentIntent(pendingIntent)
                .SetDefaults(NotificationDefaults.Vibrate | NotificationDefaults.Lights)
                .SetSmallIcon(Resource.Drawable.notification_small);
            
            Notification notification = builder.Build();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                notification.Category = Notification.CategoryAlarm;
            }

            //
            NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(timerState.id, notification);

            PlayAlarmTone(timerState);
        }

        void PostForegroundNotification(TimerState timerState)
        {
            Intent intent = CreateIntent(timerState);
            context.LaunchActivity(intent);

            PlayAlarmTone(timerState);
        }

        void PlayAlarmTone(TimerState timerState)
        {
            StopAlarmTone();
            currentlyPlayingTone = RingtoneManager.GetRingtone(context, AlarmTone);
            currentlyPlayingTone.Play();

            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(defaultAlarmTimeout * 1000);
                if (timerState.id == lastNotificationId)
                {
                    if (currentlyPlayingTone != null && currentlyPlayingTone.IsPlaying)
                    {
                        currentlyPlayingTone.Stop();
                    }
                }
            }
            );

        }

        void StopAlarmTone()
        {
            if (currentlyPlayingTone != null)
            {
                if (currentlyPlayingTone.IsPlaying)
                {
                    currentlyPlayingTone.Stop();
                }
            }
        }
    }
}