using System;
using System.Collections.Generic;
using System.Text;

namespace TimerAppShared
{
    public interface NotificationAdaptor
    {
        void PostNotification(TimerState timerState);
    }
}
