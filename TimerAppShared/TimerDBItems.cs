using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TimerAppShared
{
    [Table("Items")]
    public class TimerDBItem
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public DateTime timeStart { get; set; }
        public long duration { get; set; }
        public double timeLeft { get; set; }

        public string alarmName { get; set; }
        public bool running { get; set; }
    }
}
