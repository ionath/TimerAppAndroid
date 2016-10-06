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
    class GenerateIds
    {
        // Singleton instance
        static GenerateIds instance = new GenerateIds();

        int generatedId = 1;

        private GenerateIds()
        {

        }
        
        public static int GenerateId()
        {
            instance.generatedId++;
            return instance.generatedId;
        }
    }
}