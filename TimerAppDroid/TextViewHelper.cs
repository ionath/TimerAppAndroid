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
    class TextViewHelper
    {
        public static void SetTextAppearance(Activity activity, TextView textView, int style)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // 'TextView.SetTextAppearance(Context, int)' is obsolete: 'deprecated'
                textView.SetTextAppearance(activity, style);
#pragma warning restore CS0618 // 'TextView.SetTextAppearance(Context, int)' is obsolete: 'deprecated'
            }
            else
            {
                textView.SetTextAppearance(style);
            }
        }

    }
}