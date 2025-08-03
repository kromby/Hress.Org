namespace Ez.Hress.Hardhead.DataAccess.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("rep_Image")]
public partial class Image
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int TypeId { get; set; }

    public int ImageId { get; set; }

    public DateTime Inserted { get; set; }

    public int InsertedBy { get; set; }

    public DateTime? Updated { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public int? DeletedBy { get; set; }

    public virtual Event Event { get; set; }
}
