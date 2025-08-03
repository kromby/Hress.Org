namespace Ez.Hress.Hardhead.DataAccess.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("rep_Event")]
public partial class Event
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Event()
    {
        Images = new HashSet<Image>();
        Texts = new HashSet<Text>();
        Counts = new HashSet<Count>();
    }

    public int Id { get; set; }

    public int Number { get; set; }

    public int TypeId { get; set; }

    public DateTime Date { get; set; }

    public int? ParentId { get; set; }

    public DateTime Inserted { get; set; }

    public int InsertedBy { get; set; }

    public DateTime? Updated { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<Image> Images { get; set; }
    public virtual ICollection<Text> Texts { get; set; }
    public virtual ICollection<Count> Counts { get; set; }
}
