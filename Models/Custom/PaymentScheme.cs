using Microsoft.EntityFrameworkCore;
using ptc_IGH_Sys.Models;
using ptc_IGH_Sys.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ptc_IGH_Sys.Models.Custom
{
    [Table("UniPaymentSchemes")]
    [Index(nameof(Code), IsUnique = true)]
    public class PaymentScheme : Auditable
    {
        public virtual long Id { get; set; }

        //[RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Alphanumeric only!")]
      
        [StringLength(20)]
        public virtual string Code { get; set; }

        [StringLength(50)]
        public virtual string Description { get; set; }

        public virtual bool Active { get; set; }

    }
}
