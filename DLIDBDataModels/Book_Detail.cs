//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DLIDBDataModels
{
    public partial class Book_Detail
    {
        public Book_Detail()
        {
            this.Book_Page_Content = new HashSet<Book_Page_Content>();
        }
    
        public int Book_DetailID { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string BookDescription { get; set; }
        public string BookSourceURL { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> LastModifiedDate { get; set; }
        public Nullable<bool> Active { get; set; }
    
        public virtual User CreatedByUser { get; set; }
        public virtual User UpdatedByUser { get; set; }
        public virtual ICollection<Book_Page_Content> Book_Page_Content { get; set; }
    }
    
}