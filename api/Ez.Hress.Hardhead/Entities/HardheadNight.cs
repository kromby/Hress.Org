using Ez.Hress.Shared.Entities;
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

    /// <summary>
    /// A future night shows only its month until a start time is set, since until then its day may be
    /// the placeholder its night was created with.
    /// </summary>
    public string DateString
    {
        get
        {
            if (Date < DateTime.UtcNow || StartTime.HasValue)
                return Date.ToString("d. MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));

            return Date.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
        }
    }

    /// <summary>When the night starts, the time part of Date (Iceland time). Null when no time is set (00:00).</summary>
    public TimeSpan? StartTime => Date.TimeOfDay == TimeSpan.Zero ? null : new TimeSpan(Date.Hour, Date.Minute, 0);

    /// <summary>Start time as "20:00", or null.</summary>
    public string? StartTimeString => StartTime?.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

    /// <summary>Where the night is held. Null when not set.</summary>
    public string? Location { get; set; }

    public const int LocationMaxLength = 100;
    public int Number { get; set; }

    public override string Name { get => string.Format("Harðhaus #{0}", Number); }

    public int GuestCount { get; set; }

    public IList<UserBasicEntity>? Guests { get; set; }

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
