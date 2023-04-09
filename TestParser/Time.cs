using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestParser
{
    internal class Time
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public Time(int hours, int minutes)
        {
            Hours = hours;
            Minutes = minutes;
        }
    }
}
