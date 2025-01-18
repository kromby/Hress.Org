using System.ComponentModel;
using System.Globalization;

namespace Ez.Hress.Shared.Entities;

public abstract class EntityBase<T>
{
    public T? ID { get; set; }
    public virtual string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime Inserted { get; set; }
    [DefaultValue("")]
    public string InsertedString
    {
        get
        {
            if (Inserted.Year == 0001)
                return string.Empty;
            if (Inserted < DateTime.UtcNow)
                return Inserted.ToString("d. MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));

            return Inserted.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
        }
    }

    [DefaultValue(0)]
    public int InsertedBy { get; set; }
    public DateTime? Updated { get; set; }
    public int? UpdatedBy { get; set; }

    public DateTime? Deleted { get; set; }
}
