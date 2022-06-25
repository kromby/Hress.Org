namespace Ez.Hress.Shared.Entities
{
    public abstract class EntityBase<T>
    {
        public T? ID { get; set; }
        public virtual string? Name { get; set; }
        public DateTime Inserted { get; set; }
        public int InsertedBy { get; set; }
        public DateTime? Updated { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
