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
    [Activity(Label = "TimerEditor")]
    public class TimerEditorActivity : Activity
    {
        enum eSlot { NONE, HOUR, MINUTE, SECOND};

        eSlot selected = eSlot.NONE;
        TextView selectedText;
        int currentValueOfInput = 0;
        int currentInputCount = 0;
        int hour = 0;
        int minute = 0;
        int second = 0;
        bool timeEditable = true;

        Android.Graphics.Color unselectedColor = Android.Graphics.Color.White;
        Android.Graphics.Color readOnlyColor = Android.Graphics.Color.Gray;
        Android.Graphics.Color selectedColor = Android.Graphics.Color.Aqua;
        Android.Graphics.Color elapsedColor = Android.Graphics.Color.Red;

        View topLevelLayout;
        TextView hourText;
        TextView minuteText;
        TextView secondText;
        TextView separator1;
        TextView separator2;

        string alarmName;

        TimerService timerService = null;
        EventHandler displayTimeChangedHandler;

        public override void Finish()
        {
            base.Finish();
            
            // Unsubscribe from event handling
            if (timerService != null && displayTimeChangedHandler != null)
            {
                timerService.DisplayTimeChanged -= displayTimeChangedHandler;
            }
        }

        void updateSelected(eSlot newSelected)
        {
            // If new selection is the same as previous then do nothing
            if (newSelected == selected)
            {
                return;
            }
            // If time editing is disabled then do nothing
            if (timeEditable == false)
            {
                return;
            }

            // Unselect previous selection
            if (selected != eSlot.NONE && selectedColor != null)
            {
                selectedText.SetTextColor(unselectedColor);
            }

            // Set new selection
            switch (newSelected)
            {
                case eSlot.HOUR:
                    selectedText = hourText;
                    break;
                case eSlot.MINUTE:
                    selectedText = minuteText;
                    break;
                case eSlot.SECOND:
                    selectedText = secondText;
                    break;
                case eSlot.NONE:
                    selectedText = null;
                    break;
            }
            // Set color of new selection
            if (selectedText != null)
            {
                selectedText.SetTextColor(selectedColor);
            }
            
            selected = newSelected;
            currentValueOfInput = 0;
            currentInputCount = 0;
        }

        void numPadInput(int numPadKey)
        {
            string numPadStr = numPadKey.ToString();
            if (selected != eSlot.NONE)
            {
                currentInputCount++;
                currentValueOfInput = 10 * currentValueOfInput + numPadKey;
                string timerStr;
                if (selected == eSlot.HOUR)
                {
                    timerStr = currentValueOfInput.ToString();
                    hour = currentValueOfInput;
                }
                else
                {
                    const int MAX_MINUTES = 59;
                    if (currentValueOfInput > MAX_MINUTES)
                        currentValueOfInput = MAX_MINUTES;
                    timerStr = currentValueOfInput.ToString().PadLeft(2, '0');
                    if (selected == eSlot.MINUTE)
                    {
                        minute = currentValueOfInput;
                    }
                    else
                    {
                        second = currentValueOfInput;
                    }
                }

                selectedText.Text = timerStr;
                if (currentInputCount > 1)
                {
                    updateSelected(eSlot.NONE);
                }
            }
        }

        void setFocusNone()
        {
            if (topLevelLayout != null)
            {
                topLevelLayout.RequestFocus();
            }
        }

        void setTimerTextColor(Android.Graphics.Color color)
        {
            hourText.SetTextColor(color);
            minuteText.SetTextColor(color);
            secondText.SetTextColor(color);
            separator1.SetTextColor(color);
            separator2.SetTextColor(color);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.TimerEditor);

            //TimerService timerService = null;
            int id = Intent.GetIntExtra("id", 0);
            // Get TimerDBItem
            if (id != 0)
            {
                timerService = TimerServiceManager.GetTimerServiceWithId(id);
            }

            // Get views by id
            topLevelLayout = FindViewById<View>(Resource.Id.topLevelLayout);
            hourText = FindViewById<TextView>(Resource.Id.hourText);
            minuteText = FindViewById<TextView>(Resource.Id.minuteText);
            secondText = FindViewById<TextView>(Resource.Id.secondText);
            separator1 = FindViewById<TextView>(Resource.Id.seperator1);
            separator2 = FindViewById<TextView>(Resource.Id.seperator2);
            Button saveButton = FindViewById<Button>(Resource.Id.saveButton);
            Button startButton = FindViewById<Button>(Resource.Id.startButton);
            EditText editAlarmName = FindViewById<EditText>(Resource.Id.editAlarmName);

            setFocusNone();

            if (timerService != null)
            {
                var duration = timerService.hourMinSec();
                hour = duration.Item1;
                minute = duration.Item2;
                second = duration.Item3;
                hourText.Text = hour.ToString();
                minuteText.Text = minute.ToString().PadLeft(2, '0');
                secondText.Text = second.ToString().PadLeft(2, '0');

                alarmName = timerService.State.AlarmName;
                editAlarmName.Text = alarmName;

                if (timerService.IsStarted())
                {
                    timeEditable = false;
                    setTimerTextColor(readOnlyColor);

                    startButton.Enabled = false;

                    displayTimeChangedHandler = delegate
                    {
                        var timeLeft = timerService.hourMinSec();

                        this.RunOnUiThread(() =>
                        {
                            if (timerService.IsElapsed())
                            {
                                setTimerTextColor(elapsedColor);
                            }
                            else if (timerService.IsRunning())
                            {
                                setTimerTextColor(readOnlyColor);
                            }

                            hourText.Text = timeLeft.Item1.ToString();
                            minuteText.Text = timeLeft.Item2.ToString().PadLeft(2, '0');
                            secondText.Text = timeLeft.Item3.ToString().PadLeft(2, '0');
                        });
                    };
                    timerService.DisplayTimeChanged += displayTimeChangedHandler;
                }
            }

            int[] numPadIds = new int[] {
                Resource.Id.numPad0,
                Resource.Id.numPad1,
                Resource.Id.numPad2,
                Resource.Id.numPad3,
                Resource.Id.numPad4,
                Resource.Id.numPad5,
                Resource.Id.numPad6,
                Resource.Id.numPad7,
                Resource.Id.numPad8,
                Resource.Id.numPad9,
            };
            Button[] numPad = new Button[numPadIds.Length];
            for (int index=0; index<numPadIds.Length; ++index)
            {
                int localIndex = index;
                numPad[index] = FindViewById<Button>(numPadIds[index]);
                numPad[index].Click += delegate
                {
                    numPadInput(localIndex);
                };
            }

            startButton.Click += delegate
            {
                Intent resultIntent = new Intent();
                resultIntent.PutExtra("hour", hour);
                resultIntent.PutExtra("minute", minute);
                resultIntent.PutExtra("second", second);
                resultIntent.PutExtra("alarmName", alarmName);
                resultIntent.PutExtra("start", true);
                if (timerService != null)
                {
                    resultIntent.PutExtra("id", id);

                    timerService.SetState(TimerService.CalcDuration(hour, minute, second), alarmName);
                    timerService.Start();
                }
                SetResult(Result.Ok, resultIntent);
                Finish();
            };

            saveButton.Click += delegate
            {
                Intent resultIntent = new Intent();
                resultIntent.PutExtra("hour", hour);
                resultIntent.PutExtra("minute", minute);
                resultIntent.PutExtra("second", second);
                resultIntent.PutExtra("alarmName", alarmName);
                resultIntent.PutExtra("start", false);
                if (timerService != null)
                {
                    resultIntent.PutExtra("id", id);

                    timerService.SetState(TimerService.CalcDuration(hour, minute, second), alarmName);
                }
                SetResult(Result.Ok, resultIntent);
                Finish();
            };

            hourText.Click += delegate
            {
                updateSelected(eSlot.HOUR);
            };
            minuteText.Click += delegate
            {
                updateSelected(eSlot.MINUTE);
            };
            secondText.Click += delegate
            {
                updateSelected(eSlot.SECOND);
            };

            alarmName = editAlarmName.Text;
            editAlarmName.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) => {

                alarmName = e.Text.ToString();

            };
        }
    }
}