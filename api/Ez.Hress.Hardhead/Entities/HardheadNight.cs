﻿using Ez.Hress.Shared.Entities;
using System.Globalization;

namespace Ez.Hress.Hardhead.Entities;

public class HardheadNight : EntityBase<int>
{
    public HardheadNight(int id, int number, UserBasicEntity host)
    {
        ID = id;
        Number = number;
        Host = host;
        NextHostID = new int?();
    }

    public DateTime Date { get; set; }

    public string DateString
    {
        get
        {
            if (Date < DateTime.UtcNow)
                return Date.ToString("d. MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));

            return Date.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
        }
    }
    public int Number { get; set; }

    public override string Name { get => string.Format("Harðhaus #{0}", Number); }

    public int GuestCount { get; set; }

    public UserBasicEntity Host
    {
        get; set;
    }

    public Movie? Movie    {        get;set;    }

    public int YearID { private get; set; }
    public HrefEntity Year
    {
        get => new()
        {
            Href = string.Format("/api/hardhead/{0}", YearID),
            ID = YearID
        };
    }

    public int? NextHostID { get; set; }

    public void Validate()
    {
        if (ID <= 0)
            throw new ArgumentException("Must be greater than zero.", nameof(ID));

        if (Number <= 0)
            throw new ArgumentException("Must be greater than zero.", nameof(Number));

        if (Host == null)
            throw new ArgumentException("Can not be null or empty.", nameof(Host));

        if (Host.ID <= 0)
            throw new ArgumentException("Must be greater than zero.", nameof(Host.ID));

        if (Date < new DateTime(2000, 1, 1))
            throw new ArgumentException("Must be in this century.", nameof(Date));
    }
}
