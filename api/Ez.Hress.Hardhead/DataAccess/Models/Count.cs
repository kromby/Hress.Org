using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess.Models;

[Table("rep_Count")]
public partial class Count
{
    public Count()
    {

    }

    public int Id { get; set; }
    
    public int EventId { get; set; }
    
    public int TypeId { get; set; }
    
    [Required]
    [Column("Count")]
    public decimal CountValue { get; set; }
    
    public DateTime Inserted { get; set; }
    public int InsertedBy { get; set; }
    public DateTime? Updated { get; set; }
    public int? UpdatedBy { get; set; }
    public virtual Event? Event { get; set; }
}
