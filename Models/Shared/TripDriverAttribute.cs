using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ptc_IGH_Sys.Models.Shared
{
    [Table("UniTripDriverAttributes")]
    public class TripDriverAttribute : Auditable
    {
        public virtual long Id { get; set; }

        [StringLength(50)]
        public virtual string Value { get; set; }
        
        [Required]
        public virtual Attribute Attribute { get; set; }

    }
}
