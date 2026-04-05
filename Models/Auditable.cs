using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ptc_IGH_Sys.Models
{
    [Serializable]
    [DataContract]
    public abstract class Auditable
    {
        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        [MaxLength(40)]
        public string CreatedBy { get; set; }

        [MaxLength(40)]
        public string UpdatedBy { get; set; }
    }
}
