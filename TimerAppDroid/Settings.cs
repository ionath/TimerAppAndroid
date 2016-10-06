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
using SQLite;

namespace TimerAppDroid
{
    [Table("Settings")]
    class SettingsDBItem
    {
        [PrimaryKey, Column("_key")]
        public string key { get; set; }
        public Android.Net.Uri ringtone { get; set; }
    }
}