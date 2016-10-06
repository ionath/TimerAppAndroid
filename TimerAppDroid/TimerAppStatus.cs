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

namespace TimerAppDroid
{
    public class TimerAppStatus
    {
        static TimerAppStatus instance = new TimerAppStatus();

        int appState = 0;

        public const int STATE_UNINITIALIZED = 0;
        public const int STATE_ACTIVE = 1;
        public const int STATE_PAUSED = 2;

        public static void SetAppState(int state)
        {
            instance.appState = state;
        }
        public static int GetAppState()
        {
            return instance.appState;
        }
    }
}