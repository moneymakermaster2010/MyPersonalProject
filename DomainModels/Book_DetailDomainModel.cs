using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainModels.Base;

namespace DomainModels
{
    public class Book_DetailDomainModel : DomainModelBase
    {
        public int Book_DetailID { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string BookDescription { get; set; }
        public string BookSourceURL { get; set; }
        

        
        public virtual ICollection<Book_Page_ContentDomainModel> Book_Page_Content { get; set; }
    }
}
