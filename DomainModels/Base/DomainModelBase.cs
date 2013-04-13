using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomainModels.Base
{
    public class DomainModelBase
    {
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<bool> Active { get; set; }

        public virtual UserDomainModel CreatedByUser { get; set; }
        public virtual UserDomainModel UpdatedByUser { get; set; }
    }
}
