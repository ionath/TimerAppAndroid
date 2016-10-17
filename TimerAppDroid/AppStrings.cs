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
    class AppStrings
    {
        // Singleton instance
        static AppStrings instance = new AppStrings();

        // String resources
        public static string PauseString { get; private set; }
        public static string ResetString { get; private set; }
        public static string DeleteString { get; private set; }
        public static string StartString { get; private set; }
        public static string EditString { get; private set; }

        private AppStrings()
        {

        }
        
        public static void updateStrings(Context context)
        {
            PauseString = context.Resources.GetString(Resource.String.Pause);
            ResetString = context.Resources.GetString(Resource.String.Reset);
            DeleteString = context.Resources.GetString(Resource.String.Delete);
            StartString = context.Resources.GetString(Resource.String.Start);
            EditString = context.Resources.GetString(Resource.String.Edit);
        }
    }
}