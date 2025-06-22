namespace Ez.Hress.Hardhead.DataAccess.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

[Table("rep_Text")]
public partial class Text()
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int TypeId { get; set; }

    //public int? ParentId { get; set; }

    [Required]
    public required string TextValue { get; set; }

    public DateTime Inserted { get; set; }

    public int InsertedBy { get; set; }

    public DateTime? Updated { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Event? Event { get; set; }

    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    //public virtual ICollection<Text> Texts { get; set; }

    //public virtual Text Parent { get; set; }
}
