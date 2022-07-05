using System.Globalization;

namespace Ez.Hress.Shared.Entities
{
    public abstract class EntityBase<T>
    {
        public T? ID { get; set; }
        public virtual string? Name { get; set; }
        public DateTime Inserted { get; set; }
        public string InsertedString
        {
            get
            {
                if (Inserted < DateTime.Now)
                    return Inserted.ToString("d. MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
                else
                    return Inserted.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
            }
        }
        public int InsertedBy { get; set; }
        public DateTime? Updated { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
