using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainModels.Base;

namespace DomainModels
{
    public class Book_Page_ContentDomainModel : DomainModelBase
    {
        public int Book_Page_ContentID { get; set; }
        public int BookID { get; set; }
        public long PageNumber { get; set; }
        public byte[] Content { get; set; }

        public virtual Book_DetailDomainModel Book_Detail { get; set; }
    }
}
