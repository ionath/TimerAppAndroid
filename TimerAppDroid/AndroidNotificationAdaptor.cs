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
        Activity context;
        public Ringtone AlarmTone { get; set; }
        NotificationManager notificationManager;

        Ringtone currentlyPlayingTone;
        public int defaultAlarmTimeout { get; set; }

        int lastNotificationId = 0;

        public int NotificationId { get; set; }
        TimerService timerService;
        Notification.Builder builder;

        public AndroidNotificationAdaptor(Activity _context, Ringtone alarmTone, NotificationManager _notificationManager)
        {
            context = _context;
            //AlarmTone = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            AlarmTone = alarmTone;
            notificationManager = _notificationManager;
            defaultAlarmTimeout = 30;
        }
        
        public void PostNotification(TimerState timerState)
        {
            PostForegroundNotification(timerState);

            lastNotificationId = timerState.Id;
        }

        public void CancelNotification(int notificationId)
        {
            if (lastNotificationId == notificationId)
            {
                StopAlarmTone();
            }

            //NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Cancel(notificationId);
        }

        public void CancelNotification()
        {
            var id = timerService.State.Id;

            CancelNotification(id);
        }

        Intent CreateIntent(TimerState timerState)
        {
            Intent intent = new Intent(context, typeof(AlarmNotification));
            intent.PutExtra("notificationId", timerState.Id);
            intent.PutExtra("alarmName", timerState.AlarmName);
            return intent;
        }

        public void CreateNotification(TimerService timerService)
        {
            NotificationId = timerService.State.Id;
            this.timerService = timerService;

            //Intent intent = CreateIntent(timerService.State);
            Intent intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);

            const int pendingIntentId = MainActivity.REQUEST_CODE_PENDING_INTENT;
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            builder = new Notification.Builder(context)
                .SetContentTitle(timerService.State.AlarmName)
                .SetContentText(timerService.ToString())
                .SetContentIntent(pendingIntent)
                .SetSmallIcon(Resource.Drawable.notification_small);

        }

        public void UpdateBackgroundNotification()
        {
            builder.SetContentText(timerService.ToString());

            Notification notification = builder.Build();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                notification.Category = Notification.CategoryAlarm;
            }

            //
            //NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(NotificationId, notification);
        }

        void PostBackgroundNotification(TimerState timerState)
        {
            Intent intent = CreateIntent(timerState);

            const int pendingIntentId = MainActivity.REQUEST_CODE_PENDING_INTENT;
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);


            //Android.Net.Uri defaultTone = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            //Android.Net.Uri tone = Android.Net.Uri.Parse("android.resource://" + context.PackageName + "/Raw/" + Resource.Raw.elegant_ringtone);
            Notification.Builder builder = new Notification.Builder(context)
                .SetContentTitle(timerState.AlarmName)
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
            //NotificationManager notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(timerState.Id, notification);

            PlayAlarmTone(timerState);
        }

        void PostForegroundNotification(TimerState timerState)
        {
            Intent intent = CreateIntent(timerState);
            context.StartActivity(intent);

            PlayAlarmTone(timerState);
        }

        void PlayAlarmTone(TimerState timerState)
        {
            StopAlarmTone();
            currentlyPlayingTone = AlarmTone;
            currentlyPlayingTone.Play();

            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(defaultAlarmTimeout * 1000);
                if (timerState.Id == lastNotificationId)
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