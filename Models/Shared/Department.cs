using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ptc_IGH_Sys.Models.Shared
{
    [Table("UniDepartments")]
    [Index(nameof(Code), IsUnique = true)]
    public class Department : Auditable
    {
        public virtual long Id { get; set; }

       

        [StringLength(20)]
        public virtual string Code { get; set; }

        [StringLength(50)]
        public virtual string Description { get; set; }

        [StringLength(5)]
        public virtual string Prefix { get; set; }

        public virtual bool Active { get; set; }

    }
}
