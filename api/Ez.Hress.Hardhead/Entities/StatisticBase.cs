﻿using System.Globalization;

namespace Ez.Hress.Hardhead.Entities;

public class StatisticBase
{
    public int AttendedCount { get; set; }
    public DateTime FirstAttended { get; set; }
    public string FirstAttendedString => FirstAttended.ToString("d. MMM yyyy", CultureInfo.GetCultureInfo("is-IS"));

    public DateTime LastAttended { get; set; }
    public string LastAttendedString => LastAttended.ToString("d. MMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
}
